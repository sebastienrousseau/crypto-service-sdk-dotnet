// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace CryptoService.Sdk.Tests;

/// <summary>Construction, headers, raw requests, redirects and disposal.</summary>
public sealed partial class ClientTests
{
    private const string Health = """{"status":"ok","statusCode":200,"uptime":1}""";
    private const string Hash = """{"data":{"algorithm":"sha256","digest":"00","length":1}}""";

    [Fact]
    public void TheOptionsAreChecked()
    {
        Assert.Throws<ArgumentNullException>(() => new CryptoServiceClient(null!));
        Assert.Throws<ArgumentNullException>(() => new CryptoServiceClient((HttpMessageHandler)null!, new CryptoServiceClientOptions()));
        Assert.Throws<ArgumentNullException>(() => new CryptoServiceClient(new StubHandler((_, _) => null!), null!));
        Assert.Throws<ArgumentNullException>(() => new CryptoServiceClient((HttpClient)null!, new CryptoServiceClientOptions()));
        using var http = new HttpClient();
        Assert.Throws<ArgumentNullException>(() => new CryptoServiceClient(http, null!));
        Assert.Throws<ArgumentException>(() => new CryptoServiceClient(new CryptoServiceClientOptions()));
        Assert.Throws<ArgumentException>(() => new CryptoServiceClient(Options(o => o.BaseUrl = new Uri("/relative", UriKind.Relative))));
        Assert.Throws<ArgumentException>(() => new CryptoServiceClient(Options(o => o.Timeout = TimeSpan.Zero)));
        Assert.Throws<ArgumentException>(() => new CryptoServiceClient(Options(o => o.Deadline = TimeSpan.FromSeconds(-1))));
        Assert.Throws<ArgumentException>(() => new CryptoServiceClient(Options(o => o.Retry = null!)));
        Assert.Throws<ArgumentNullException>(() => new CryptoServiceClientOptions().TimeProvider = null!);
    }

    [Fact]
    public void TheOptionsHaveTheContractDefaults()
    {
        var options = new CryptoServiceClientOptions();

        Assert.Equal(TimeSpan.FromSeconds(30), options.Timeout);
        Assert.Null(options.Deadline);
        Assert.Equal(0, options.Retry.MaxRetries);
        Assert.Equal(TimeSpan.FromMilliseconds(200), options.Retry.InitialDelay);
        Assert.Equal(TimeSpan.FromSeconds(2), options.Retry.MaxDelay);
        Assert.Same(TimeProvider.System, options.TimeProvider);
        Assert.Throws<ArgumentOutOfRangeException>(() => new RetryOptions { MaxRetries = -1 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new RetryOptions { InitialDelay = TimeSpan.FromTicks(-1) });
        Assert.Throws<ArgumentOutOfRangeException>(() => new RetryOptions { MaxDelay = TimeSpan.FromTicks(-1) });
    }

    [Fact]
    public void NoDescriptionShowsACredential()
    {
        CryptoServiceClientOptions options = Options(o =>
        {
            o.ApiKey = "synthetic-secret-key";
            o.Token = "synthetic-secret-token";
            o.Deadline = TimeSpan.FromSeconds(5);
        });
        using var client = new CryptoServiceClient(options);

        string described = options + " " + client + " " + new CryptoServiceClientOptions();

        Assert.DoesNotContain("synthetic-secret", described, StringComparison.Ordinal);
        Assert.Contains("ApiKey = set (redacted)", described, StringComparison.Ordinal);
        Assert.Contains("Token = unset", described, StringComparison.Ordinal);
        Assert.Contains("MaxRetries = 0", described, StringComparison.Ordinal);
        Assert.Contains("BaseUrl = http://crypto.test:3105", client.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task EveryRequestCarriesTheUserAgentAcceptAndHttp2Policy()
    {
        var handler = StubHandler.Answer(HttpStatusCode.OK, Hash);
        using CryptoServiceClient client = StubHandler.Client(handler);

        await client.HashAsync(new HashRequest { Algorithm = "sha256", Data = "a" });

        SentRequest sent = handler.Last;
        Assert.Equal("crypto-service-sdk-dotnet/0.0.40", sent.Header("User-Agent"));
        Assert.Equal("crypto-service-sdk-dotnet/0.0.40", CryptoServiceClient.UserAgent);
        Assert.Equal(["application/json", "application/problem+json"], sent.Headers.Accept.Select(a => a.MediaType));
        Assert.Equal(HttpVersion.Version20, sent.Version);
        Assert.Equal(HttpVersionPolicy.RequestVersionOrLower, sent.VersionPolicy);
        Assert.Null(sent.Header("traceparent"));
        Assert.Null(sent.Header("Authorization"));
    }

    [Fact]
    public async Task APathPrefixIsKept()
    {
        var handler = StubHandler.Answer(HttpStatusCode.OK, Health);
        using CryptoServiceClient client = StubHandler.Client(handler, o => o.BaseUrl = new Uri("https://gateway.test/crypto/"));

        await client.HealthAsync();

        Assert.Equal("https://gateway.test/crypto/health", handler.Last.Uri!.AbsoluteUri);
    }

    [Fact]
    public async Task AFixedTraceparentIsSentTrimmed()
    {
        var handler = StubHandler.Answer(HttpStatusCode.OK, Health);
        using CryptoServiceClient client = StubHandler.Client(
            handler,
            o => o.Traceparent = "  00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01 ");

        await client.HealthAsync();

        Assert.Equal("00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01", handler.Last.Header("traceparent"));
    }

    [Fact]
    public async Task ABlankTraceparentIsNotSent()
    {
        var handler = StubHandler.Answer(HttpStatusCode.OK, Health);
        using CryptoServiceClient client = StubHandler.Client(handler, o => o.Traceparent = "  ");

        await client.HealthAsync();

        Assert.Null(handler.Last.Header("traceparent"));
    }

    [Fact]
    public async Task AGeneratedTraceparentIsFreshAndWellFormed()
    {
        var handler = StubHandler.Answer(HttpStatusCode.OK, Health);
        using CryptoServiceClient client = StubHandler.Client(handler, o =>
        {
            o.GenerateTraceparent = true;
            o.Traceparent = "00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01";
        });

        await client.HealthAsync();
        await client.HealthAsync();

        string first = handler.Requests[0].Header("traceparent")!;
        string second = handler.Requests[1].Header("traceparent")!;
        Assert.Matches(TraceparentPattern(), first);
        Assert.Matches(TraceparentPattern(), second);
        Assert.NotEqual(first, second);
    }

    [Fact]
    public async Task RequestAsyncReturnsAnyStatusWithItsBody()
    {
        var handler = StubHandler.Answer(HttpStatusCode.UnsupportedMediaType, """{"type":"x"}""", "application/problem+json");
        using CryptoServiceClient client = StubHandler.Client(handler);

        RawResponse raw = await client.RequestAsync("post", "/v2/hash", Encoding.UTF8.GetBytes("<x/>"), "application/xml");

        Assert.Equal(415, raw.StatusCode);
        Assert.Equal("application/problem+json", raw.MediaType);
        Assert.Equal("""{"type":"x"}""", Encoding.UTF8.GetString(raw.Body.Span));
        Assert.Equal(HttpMethod.Post, handler.Last.Method);
        Assert.Equal("application/xml", handler.Last.ContentType);
        Assert.Equal("<x/>", handler.Last.Body);
        Assert.Equal("synthetic-api-key", handler.Last.Header("x-api-key"));
    }

    [Fact]
    public async Task RequestAsyncSendsNoBodyWhenGivenNone()
    {
        var handler = new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent)));
        using CryptoServiceClient client = StubHandler.Client(handler);

        RawResponse raw = await client.RequestAsync("GET", "/metrics");

        Assert.Equal(204, raw.StatusCode);
        Assert.Null(raw.MediaType);
        Assert.True(raw.Body.IsEmpty);
        Assert.Null(handler.Last.Body);
    }

    [Fact]
    public async Task RequestAsyncChecksItsArguments()
    {
        using CryptoServiceClient client = StubHandler.Client(StubHandler.Answer(HttpStatusCode.OK, "{}"));

        await Assert.ThrowsAsync<ArgumentException>(() => client.RequestAsync("", "/"));
        await Assert.ThrowsAsync<ArgumentException>(() => client.RequestAsync("GET", ""));
        await Assert.ThrowsAsync<ArgumentException>(() => client.RequestAsync("GET", "health"));
    }

    [Fact]
    public async Task RequestAsyncReportsAConnectionFailure()
    {
        using CryptoServiceClient client = StubHandler.Client(StubHandler.Refuse());

        CryptoTransportException error = await Assert.ThrowsAsync<CryptoTransportException>(() => client.RequestAsync("GET", "/health"));

        Assert.False(error.TimedOut);
        Assert.IsType<HttpRequestException>(error.InnerException);
    }

    [Fact]
    public async Task RequestAsyncLetsTheCallersCancellationThrough()
    {
        using CryptoServiceClient client = StubHandler.Client(StubHandler.Hang());
        using var cancel = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.RequestAsync("GET", "/health", cancellationToken: cancel.Token));
    }

    [Fact]
    public async Task ARedirectAnInjectedClientFollowedIsRefused()
    {
        var handler = new StubHandler((request, _) =>
        {
            request.RequestUri = new Uri("http://elsewhere.test/collect");
            return Task.FromResult(StubHandler.Response(HttpStatusCode.OK, Hash));
        });
        using var http = new HttpClient(handler);
        using var client = new CryptoServiceClient(http, Options());

        CryptoTransportException error = await Assert.ThrowsAsync<CryptoTransportException>(
            () => client.HashAsync(new HashRequest { Algorithm = "sha256", Data = "a" }));

        Assert.Contains("redirect", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ARedirectAnswerIsAnErrorNotFollowed()
    {
        var handler = new StubHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.Found);
            response.Headers.Location = new Uri("http://elsewhere.test/");
            return Task.FromResult(response);
        });
        using CryptoServiceClient client = StubHandler.Client(handler);

        CryptoApiException error = await Assert.ThrowsAsync<CryptoApiException>(() => client.HealthAsync());

        Assert.Equal(302, error.StatusCode);
        Assert.Equal("about:blank", error.Problem.Type);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public void TheDefaultHandlerRefusesRedirectsAndPoolsConnections()
    {
        using SocketsHttpHandler handler = CryptoServiceClient.CreateHandler();

        Assert.False(handler.AllowAutoRedirect);
        Assert.False(handler.UseCookies);
        Assert.True(handler.EnableMultipleHttp2Connections);
        Assert.Equal(TimeSpan.FromMinutes(5), handler.PooledConnectionLifetime);
        Assert.Null(handler.SslOptions.RemoteCertificateValidationCallback);
    }

    [Fact]
    public async Task DisposingLeavesAnInjectedHttpClientAlone()
    {
        var handler = StubHandler.Answer(HttpStatusCode.OK, Health);
        using var http = new HttpClient(handler);
        var client = new CryptoServiceClient(http, Options());

        await client.DisposeAsync();

        Assert.Equal(HttpStatusCode.OK, (await http.GetAsync(new Uri("http://crypto.test/health"))).StatusCode);
    }

    [Fact]
    public async Task DisposingAnOwnedClientClosesIt()
    {
        CryptoServiceClient client = StubHandler.Client(StubHandler.Answer(HttpStatusCode.OK, Health));

        client.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => client.HealthAsync());
    }

    [Fact]
    public async Task ADefaultClientReachesALoopbackPort()
    {
        // Nothing listens on port 9 of the loopback in a test environment:
        // the client's own handler reports the refused connection.
        using var client = new CryptoServiceClient(new CryptoServiceClientOptions { BaseUrl = new Uri("http://127.0.0.1:9") });

        CryptoTransportException error = await Assert.ThrowsAsync<CryptoTransportException>(() => client.HealthAsync());

        Assert.False(error.TimedOut);
    }

    internal static CryptoServiceClientOptions Options(Action<CryptoServiceClientOptions>? configure = null)
    {
        var options = new CryptoServiceClientOptions { BaseUrl = new Uri("http://crypto.test:3105") };
        configure?.Invoke(options);
        return options;
    }

    [GeneratedRegex("^00-[0-9a-f]{32}-[0-9a-f]{16}-01$")]
    private static partial Regex TraceparentPattern();
}
