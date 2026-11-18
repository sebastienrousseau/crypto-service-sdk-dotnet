// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace CryptoService.Sdk.Tests;

/// <summary>What the client sent, captured before the request is disposed.</summary>
internal sealed record SentRequest(
    HttpMethod Method,
    Uri? Uri,
    Version Version,
    HttpVersionPolicy VersionPolicy,
    HttpRequestHeaders Headers,
    string? ContentType,
    string? Body)
{
    /// <summary>The single value of a request header, or <see langword="null"/>.</summary>
    internal string? Header(string name) =>
        Headers.TryGetValues(name, out IEnumerable<string>? values) ? string.Join(",", values) : null;
}

/// <summary>An in-memory HTTP handler: no network, canned answers in order.</summary>
internal sealed class StubHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _respond;

    internal StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
    {
        _respond = respond;
    }

    /// <summary>Every request the client sent, in order.</summary>
    internal List<SentRequest> Requests { get; } = [];

    /// <summary>The last request the client sent.</summary>
    internal SentRequest Last => Requests[^1];

    /// <summary>A handler that answers every request with this status and body.</summary>
    internal static StubHandler Answer(HttpStatusCode status, string body, string mediaType = "application/json") =>
        new((_, _) => Task.FromResult(Response(status, body, mediaType)));

    /// <summary>A handler that answers with each of these in turn, repeating the last.</summary>
    internal static StubHandler Sequence(params Func<HttpResponseMessage>[] answers)
    {
        int next = 0;
        return new((_, _) =>
        {
            Func<HttpResponseMessage> answer = answers[Math.Min(next, answers.Length - 1)];
            next++;
            return Task.FromResult(answer());
        });
    }

    /// <summary>A handler that fails every request to connect.</summary>
    internal static StubHandler Refuse() =>
        new((_, _) => Task.FromException<HttpResponseMessage>(new HttpRequestException("connection refused")));

    /// <summary>A handler that never answers, until the request is cancelled.</summary>
    internal static StubHandler Hang() =>
        new(async (_, token) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            throw new InvalidOperationException("unreachable");
        });

    /// <summary>A response with this status and body; HttpClient owns and disposes it.</summary>
    internal static HttpResponseMessage Response(HttpStatusCode status, string body, string mediaType = "application/json") =>
        new(status) { Content = new StringContent(body, Encoding.UTF8, mediaType) };

    /// <summary>A response with this status and no content; HttpClient owns and disposes it.</summary>
    internal static HttpResponseMessage Status(HttpStatusCode status) => new(status);

    /// <summary>A redirect to <paramref name="location"/> with no content.</summary>
    internal static HttpResponseMessage Redirect(HttpStatusCode status, Uri location)
    {
        HttpResponseMessage response = Status(status);
        response.Headers.Location = location;
        return response;
    }

    /// <summary>A response with this status, an empty body and a <c>Retry-After</c>.</summary>
    internal static HttpResponseMessage RetryAfter(HttpStatusCode status, RetryConditionHeaderValue? retryAfter)
    {
        HttpResponseMessage response = Response(status, "");
        response.Headers.RetryAfter = retryAfter;
        return response;
    }

    /// <summary>A client sending through <paramref name="handler"/>.</summary>
    internal static CryptoServiceClient Client(
        StubHandler handler,
        Action<CryptoServiceClientOptions>? configure = null)
    {
        var options = new CryptoServiceClientOptions
        {
            BaseUrl = new Uri("http://crypto.test:3105/"),
            ApiKey = "synthetic-api-key",
        };
        configure?.Invoke(options);
        return new CryptoServiceClient(handler, options, disposeHandler: true);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        Requests.Add(new SentRequest(
            request.Method,
            request.RequestUri,
            request.Version,
            request.VersionPolicy,
            request.Headers,
            request.Content?.Headers.ContentType?.ToString(),
            body));
        HttpResponseMessage response = await _respond(request, cancellationToken);
        response.RequestMessage ??= request;
        return response;
    }
}
