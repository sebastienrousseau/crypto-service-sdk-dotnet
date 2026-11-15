// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using CryptoService.Sdk.Internal;
using Xunit.Abstractions;

namespace CryptoService.Sdk.Tests;

/// <summary>
/// Runs the conformance kit against the live server, file by file and in
/// order, through the SDK's public methods (a raw request where the kit
/// sends what no typed method can). A typed answer is re-encoded with the
/// SDK's own contract and matched against the template, so a member the
/// SDK fails to decode fails the vector.
/// </summary>
public sealed class ConformanceTests(ITestOutputHelper output)
{
    public static TheoryData<string> Files()
    {
        var files = new TheoryData<string>();
        foreach (KitFile file in Kit.Load())
        {
            files.Add(file.Name);
        }

        return files;
    }

    [ServerTheory]
    [MemberData(nameof(Files))]
    public async Task EveryVectorPasses(string file)
    {
        Assert.False(LiveServer.Url is null, "CRYPTO_SERVICE_URL is unset: make conformance needs a running crypto-server.");
        Assert.False(LiveServer.ApiKey is null, "CRYPTO_SERVICE_API_KEY is unset: make conformance needs the server's API key.");
        Assert.False(
            LiveServer.Required && LiveServer.JwtSecret is null,
            "CRYPTO_SERVICE_JWT_SECRET is unset: make conformance runs every file as its own principal with tokens signed by the server's JWT_SECRET.");

        KitFile kit = Kit.Load(file);
        var runner = new VectorRunner(kit.Group);
        var failures = new List<string>();
        int passed = 0;
        int notRun = 0;
        foreach (KitVector vector in kit.Vectors)
        {
            Outcome outcome = await runner.RunAsync(vector);
            output.WriteLine($"{outcome.Verdict,-7} {outcome.Route,-5} {file} {vector.Id}{(outcome.Problems.Count > 0 ? ": " + string.Join("; ", outcome.Problems) : "")}");
            passed += outcome.Verdict == "PASS" ? 1 : 0;
            notRun += outcome.Verdict == "NOT RUN" ? 1 : 0;
            if (outcome.Verdict == "FAIL")
            {
                failures.Add(vector.Id + ": " + string.Join("; ", outcome.Problems));
            }
        }

        output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"conformance {file}: {passed} passed, {failures.Count} failed, {notRun} not run, of {kit.Vectors.Count}"));
        Assert.Empty(failures);
    }
}

/// <summary>What became of one vector.</summary>
/// <param name="Verdict"><c>PASS</c>, <c>FAIL</c> or <c>NOT RUN</c>.</param>
/// <param name="Problems">Why it failed, or why it did not run.</param>
/// <param name="Route"><c>typed</c> through a public method, or <c>raw</c>.</param>
internal sealed record Outcome(string Verdict, IReadOnlyList<string> Problems, string Route = "");

/// <summary>Runs one kit file's vectors in order, keeping its captures.</summary>
internal sealed class VectorRunner(string group)
{
    private readonly Dictionary<string, string> _captured = new(StringComparer.Ordinal);
    private readonly string _subject = "conformance-" + group;

    internal async Task<Outcome> RunAsync(KitVector vector)
    {
        CryptoServiceClientOptions? options = OptionsFor(vector.Auth);
        if (options is null)
        {
            return new Outcome("NOT RUN", ["needs CRYPTO_SERVICE_JWT_SECRET for a scoped token"]);
        }

        for (int attempt = 1; ; attempt++)
        {
            Answer answer = await SendAsync(vector, options);
            if (answer.Status == 429 && Expected(vector) != 429 && attempt < 4)
            {
                // The tenant quota (60 a minute per principal) refused the
                // request before the server acted: wait it out and resend.
                await Task.Delay(TimeSpan.FromSeconds(answer.RetryAfterSeconds + 1));
                continue;
            }

            List<string> problems = Check(vector, answer);
            if (problems.Count == 0)
            {
                Capture(vector, answer);
            }

            return new Outcome(problems.Count == 0 ? "PASS" : "FAIL", problems, answer.Path);
        }
    }

    private static int Expected(KitVector vector) => vector.Expect.GetProperty("status").GetInt32();

    private CryptoServiceClientOptions? OptionsFor(JsonElement auth)
    {
        var options = new CryptoServiceClientOptions { BaseUrl = new Uri(LiveServer.Url!) };
        if (auth.ValueKind == JsonValueKind.String && auth.GetString() == "none")
        {
            return options;
        }

        string[] scopes = auth.ValueKind == JsonValueKind.Object
            ? [.. auth.GetProperty("scopes").EnumerateArray().Select(s => s.GetString()!)]
            : ["crypto:admin"];
        if (LiveServer.JwtSecret is { } secret)
        {
            options.Token = Jwt.Mint(secret, _subject, scopes, LiveServer.JwtIssuer, LiveServer.JwtAudience);
            return options;
        }

        if (auth.ValueKind == JsonValueKind.Object)
        {
            return null;
        }

        options.ApiKey = LiveServer.ApiKey;
        return options;
    }

    private async Task<Answer> SendAsync(KitVector vector, CryptoServiceClientOptions options)
    {
        JsonObject request = Template.Substitute(vector.Request, _captured)!.AsObject();
        string method = request["method"]!.GetValue<string>();
        string path = request["path"]!.GetValue<string>();
        var recorder = new RecordingHandler();
        using var client = new CryptoServiceClient(recorder, options, disposeHandler: true);
        JsonElement body = request["body"] is { } b ? JsonSerializer.SerializeToElement(b) : JsonSerializer.SerializeToElement(new JsonObject());
        TypedCall? typed = request.ContainsKey("rawBody") ? null : Dispatcher.Prepare(method, path, body);
        if (typed is not null)
        {
            JsonElement? decoded = await TypedAsync(typed, client);
            return recorder.Answer(decoded, "typed");
        }

        await RawAsync(client, request, method, path);
        return recorder.Answer(null, "raw");
    }

    private static async Task<JsonElement?> TypedAsync(TypedCall typed, CryptoServiceClient client)
    {
        try
        {
            return await typed(client, CancellationToken.None);
        }
        catch (CryptoApiException error)
        {
            return JsonSerializer.SerializeToElement(error.Problem, CryptoJsonContext.Default.ProblemDetails);
        }
    }

    private static Task<RawResponse> RawAsync(CryptoServiceClient client, JsonObject request, string method, string path)
    {
        if (request["rawBody"] is { } raw)
        {
            string? mediaType = request["headers"]?["content-type"]?.GetValue<string>();
            return client.RequestAsync(method, path, Encoding.UTF8.GetBytes(raw.GetValue<string>()), mediaType);
        }

        byte[]? json = request["body"] is { } b ? Encoding.UTF8.GetBytes(b.ToJsonString()) : null;
        return client.RequestAsync(method, path, json);
    }

    private List<string> Check(KitVector vector, Answer answer)
    {
        var problems = new List<string>();
        JsonElement expect = vector.Expect;
        if (answer.Status != Expected(vector))
        {
            problems.Add(string.Create(CultureInfo.InvariantCulture, $"status {answer.Status}, expected {Expected(vector)}"));
        }

        if (answer.MediaType != expect.GetProperty("mediaType").GetString())
        {
            problems.Add($"media type {answer.MediaType}, expected {expect.GetProperty("mediaType").GetString()}");
        }

        if (expect.TryGetProperty("headers", out JsonElement headers))
        {
            foreach (JsonProperty header in headers.EnumerateObject())
            {
                string? actual = answer.Headers.GetValueOrDefault(header.Name);
                if (actual != header.Value.GetString())
                {
                    problems.Add($"header {header.Name}: {actual}");
                }
            }
        }

        JsonElement template = expect.TryGetProperty("body", out JsonElement body) ? body : expect.GetProperty("text");
        problems.AddRange(Template.Mismatches(template, answer.Body, _captured).Select(m => "body" + m));
        return problems;
    }

    private void Capture(KitVector vector, Answer answer)
    {
        if (vector.Capture.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (JsonProperty capture in vector.Capture.EnumerateObject())
        {
            _captured[capture.Name] = Template.Pointer(answer.Body, capture.Value.GetString()!);
        }
    }
}

/// <summary>A recorded answer: status, media type, headers and the body to match.</summary>
internal sealed record Answer(int Status, string? MediaType, IReadOnlyDictionary<string, string> Headers, JsonElement Body, int RetryAfterSeconds, string Path);

/// <summary>Records the last answer's status, media type, headers and raw body as it passes.</summary>
internal sealed class RecordingHandler : DelegatingHandler
{
    private HttpStatusCode _status;
    private string? _mediaType;
    private byte[] _body = [];
    private Dictionary<string, string> _headers = new(StringComparer.OrdinalIgnoreCase);
    private int _retryAfter;

    internal RecordingHandler()
        : base(CryptoServiceClient.CreateHandler())
    {
    }

    /// <summary>
    /// The answer: the typed result re-encoded when there is one, else the
    /// raw body (parsed as JSON when the media type says so).
    /// </summary>
    internal Answer Answer(JsonElement? typed, string path)
    {
        JsonElement body = typed ?? Parse();
        return new Answer((int)_status, _mediaType, _headers, body, _retryAfter, path);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
        await response.Content.LoadIntoBufferAsync(cancellationToken);
        _status = response.StatusCode;
        _mediaType = response.Content.Headers.ContentType?.MediaType;
        _body = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        _headers = response.Headers.Concat(response.Content.Headers)
            .ToDictionary(h => h.Key, h => string.Join(", ", h.Value), StringComparer.OrdinalIgnoreCase);
        _retryAfter = (int)(response.Headers.RetryAfter?.Delta?.TotalSeconds ?? 0);
        return response;
    }

    private JsonElement Parse() =>
        _mediaType is not null && _mediaType.EndsWith("json", StringComparison.Ordinal)
            ? JsonDocument.Parse(_body).RootElement.Clone()
            : JsonSerializer.SerializeToElement(Encoding.UTF8.GetString(_body));
}
