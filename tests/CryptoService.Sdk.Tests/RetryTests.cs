// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Net;
using System.Net.Http.Headers;

namespace CryptoService.Sdk.Tests;

/// <summary>The resilience contract: retries, Retry-After, timeouts, the deadline and cancellation.</summary>
public sealed class RetryTests
{
    private const string Hash = """{"data":{"algorithm":"sha256","digest":"00","length":1}}""";
    private const string Key = """{"data":{"algorithm":"ed25519","keyId":"k_AAAAAAAAAAAAAAAAAAAAAA","kid":"k","metadata":{"kid":"k"},"publicKey":"00"}}""";

    [Fact]
    public async Task NoRetryByDefault()
    {
        StubHandler handler = Statuses(HttpStatusCode.ServiceUnavailable, HttpStatusCode.OK);
        using CryptoServiceClient client = StubHandler.Client(handler);

        CryptoApiException error = await Assert.ThrowsAsync<CryptoApiException>(() => HashAsync(client));

        Assert.Equal(503, error.StatusCode);
        Assert.Single(handler.Requests);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    public async Task AnIdempotentCallIsRetriedAfterATransientStatus(HttpStatusCode status)
    {
        StubHandler handler = Statuses(status, status, HttpStatusCode.OK);
        var time = new InstantTimeProvider();
        using CryptoServiceClient client = Client(handler, time, maxRetries: 2);

        HashResult result = await HashAsync(client);

        Assert.Equal("00", result.Digest);
        Assert.Equal(3, handler.Requests.Count);
        Assert.Equal(2, time.Delays.Count);
        Assert.InRange(time.Delays[0], TimeSpan.Zero, TimeSpan.FromMilliseconds(200));
        Assert.InRange(time.Delays[1], TimeSpan.Zero, TimeSpan.FromMilliseconds(400));
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests, 2)]
    [InlineData(HttpStatusCode.ServiceUnavailable, 2)]
    [InlineData(HttpStatusCode.GatewayTimeout, 1)]
    public async Task AStatefulCallIsRetriedOnlyWhenTheServerDidNotAct(HttpStatusCode status, int attempts)
    {
        StubHandler handler = StubHandler.Sequence(
            () => StubHandler.Response(status, ""),
            () => StubHandler.Response(HttpStatusCode.OK, Key));
        using CryptoServiceClient client = Client(handler, new InstantTimeProvider(), maxRetries: 3);

        if (attempts == 2)
        {
            await client.GenerateKeyPairAsync();
        }
        else
        {
            await Assert.ThrowsAsync<CryptoApiException>(() => client.GenerateKeyPairAsync());
        }

        Assert.Equal(attempts, handler.Requests.Count);
    }

    [Fact]
    public async Task AnIdempotentCallIsRetriedAfterAConnectionFailure()
    {
        int calls = 0;
        var handler = new StubHandler((_, _) => ++calls == 1
            ? Task.FromException<HttpResponseMessage>(new HttpRequestException("reset"))
            : Task.FromResult(StubHandler.Response(HttpStatusCode.OK, Hash)));
        using CryptoServiceClient client = Client(handler, new InstantTimeProvider(), maxRetries: 1);

        await HashAsync(client);

        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task AStatefulCallIsNotRetriedAfterAConnectionFailure()
    {
        StubHandler handler = StubHandler.Refuse();
        using CryptoServiceClient client = Client(handler, new InstantTimeProvider(), maxRetries: 3);

        CryptoTransportException error = await Assert.ThrowsAsync<CryptoTransportException>(() => client.MlKemGenerateKeyPairAsync());

        Assert.False(error.TimedOut);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task AnIOExceptionIsAConnectionFailure()
    {
        var handler = new StubHandler((_, _) => Task.FromException<HttpResponseMessage>(new IOException("broken pipe")));
        using CryptoServiceClient client = Client(handler, new InstantTimeProvider(), maxRetries: 1);

        CryptoTransportException error = await Assert.ThrowsAsync<CryptoTransportException>(() => HashAsync(client));

        Assert.IsType<IOException>(error.InnerException);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task AnUnexpectedHandlerFailurePropagatesUnwrapped()
    {
        var handler = new StubHandler((_, _) => Task.FromException<HttpResponseMessage>(new InvalidOperationException("bug")));
        using CryptoServiceClient client = Client(handler, new InstantTimeProvider(), maxRetries: 3);

        await Assert.ThrowsAsync<InvalidOperationException>(() => HashAsync(client));
        await Assert.ThrowsAsync<InvalidOperationException>(() => client.RequestAsync("GET", "/health"));
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task AClientErrorIsNeverRetried()
    {
        StubHandler handler = Statuses(HttpStatusCode.BadRequest, HttpStatusCode.OK);
        using CryptoServiceClient client = Client(handler, new InstantTimeProvider(), maxRetries: 3);

        await Assert.ThrowsAsync<CryptoApiException>(() => HashAsync(client));

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task TheLastErrorIsRaisedWhenRetriesRunOut()
    {
        StubHandler handler = Statuses(HttpStatusCode.ServiceUnavailable);
        using CryptoServiceClient client = Client(handler, new InstantTimeProvider(), maxRetries: 2);

        CryptoApiException error = await Assert.ThrowsAsync<CryptoApiException>(() => HashAsync(client));

        Assert.Equal(503, error.StatusCode);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task ARetryAfterInSecondsReplacesTheBackoff()
    {
        StubHandler handler = StubHandler.Sequence(
            () => StubHandler.RetryAfter(HttpStatusCode.TooManyRequests, new RetryConditionHeaderValue(TimeSpan.FromSeconds(1))),
            () => StubHandler.Response(HttpStatusCode.OK, Hash));
        var time = new InstantTimeProvider();
        using CryptoServiceClient client = Client(handler, time, maxRetries: 1);

        await HashAsync(client);

        Assert.Equal([TimeSpan.FromSeconds(1)], time.Delays);
    }

    [Fact]
    public async Task ARetryAfterDateIsMeasuredFromNow()
    {
        var time = new InstantTimeProvider();
        StubHandler handler = StubHandler.Sequence(
            () => StubHandler.RetryAfter(HttpStatusCode.ServiceUnavailable, new RetryConditionHeaderValue(time.GetUtcNow().AddSeconds(1))),
            () => StubHandler.RetryAfter(HttpStatusCode.ServiceUnavailable, new RetryConditionHeaderValue(time.GetUtcNow().AddSeconds(-5))),
            () => StubHandler.Response(HttpStatusCode.OK, Hash));
        using CryptoServiceClient client = Client(handler, time, maxRetries: 2);

        await HashAsync(client);

        // A date already past is a zero wait, which needs no timer.
        Assert.Equal([TimeSpan.FromSeconds(1)], time.Delays);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task ARetryAfterLongerThanMaxDelayEndsTheCall()
    {
        StubHandler handler = StubHandler.Sequence(
            () => StubHandler.RetryAfter(HttpStatusCode.TooManyRequests, new RetryConditionHeaderValue(TimeSpan.FromSeconds(30))),
            () => StubHandler.Response(HttpStatusCode.OK, Hash));
        var time = new InstantTimeProvider();
        using CryptoServiceClient client = Client(handler, time, maxRetries: 3);

        CryptoApiException error = await Assert.ThrowsAsync<CryptoApiException>(() => HashAsync(client));

        Assert.Equal(429, error.StatusCode);
        Assert.Empty(time.Delays);
    }

    [Fact]
    public async Task AnAttemptThatTimesOutIsRetriedWhenIdempotent()
    {
        StubHandler handler = StubHandler.Hang();
        using CryptoServiceClient client = StubHandler.Client(handler, o =>
        {
            o.Timeout = TimeSpan.FromMilliseconds(50);
            o.Retry = new RetryOptions { MaxRetries = 1, InitialDelay = TimeSpan.Zero };
        });

        CryptoTransportException error = await Assert.ThrowsAsync<CryptoTransportException>(() => HashAsync(client));

        Assert.True(error.TimedOut);
        Assert.Equal("The attempt timed out.", error.Message);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task TheDeadlineEndsTheCallAndIsNotRetried()
    {
        StubHandler handler = StubHandler.Hang();
        using CryptoServiceClient client = StubHandler.Client(handler, o =>
        {
            o.Timeout = Timeout.InfiniteTimeSpan;
            o.Deadline = TimeSpan.FromMilliseconds(100);
            o.Retry = new RetryOptions { MaxRetries = 5 };
        });

        CryptoTransportException error = await Assert.ThrowsAsync<CryptoTransportException>(() => HashAsync(client));

        Assert.True(error.TimedOut);
        Assert.Equal("The call's deadline expired.", error.Message);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task ARetryWhoseWaitPassesTheDeadlineIsNotMade()
    {
        StubHandler handler = StubHandler.Sequence(
            () => StubHandler.RetryAfter(HttpStatusCode.ServiceUnavailable, new RetryConditionHeaderValue(TimeSpan.FromSeconds(2))),
            () => StubHandler.Response(HttpStatusCode.OK, Hash));
        var time = new InstantTimeProvider();
        using CryptoServiceClient client = StubHandler.Client(handler, o =>
        {
            o.TimeProvider = time;
            o.Deadline = TimeSpan.FromSeconds(1);
            o.Retry = new RetryOptions { MaxRetries = 3, MaxDelay = TimeSpan.FromSeconds(5) };
        });

        await Assert.ThrowsAsync<CryptoApiException>(() => HashAsync(client));

        Assert.Single(handler.Requests);
        Assert.Empty(time.Delays);
    }

    [Fact]
    public async Task TheDeadlineCanExpireDuringARetryWait()
    {
        StubHandler handler = Statuses(HttpStatusCode.ServiceUnavailable);
        using CryptoServiceClient client = StubHandler.Client(handler, o =>
        {
            o.TimeProvider = new FrozenTimeProvider();
            o.Deadline = TimeSpan.FromMilliseconds(100);
            o.Retry = new RetryOptions { MaxRetries = 1, InitialDelay = TimeSpan.FromMilliseconds(50) };
        });

        CryptoTransportException error = await Assert.ThrowsAsync<CryptoTransportException>(() => HashAsync(client));

        Assert.True(error.TimedOut);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task CancellingDuringARetryWaitThrowsOperationCanceled()
    {
        StubHandler handler = Statuses(HttpStatusCode.ServiceUnavailable);
        using CryptoServiceClient client = StubHandler.Client(handler, o =>
        {
            o.TimeProvider = new FrozenTimeProvider();
            o.Retry = new RetryOptions { MaxRetries = 1 };
        });
        using var cancel = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.HashAsync(new HashRequest { Algorithm = "sha256", Data = "a" }, cancel.Token));

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task AWaitThatEndsAsTheTokenIsCancelledSendsNothingMore()
    {
        StubHandler handler = Statuses(HttpStatusCode.ServiceUnavailable);
        using var cancel = new CancellationTokenSource();
        var time = new InstantTimeProvider { OnDelay = cancel.Cancel };
        using CryptoServiceClient client = Client(handler, time, maxRetries: 3);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.HashAsync(new HashRequest { Algorithm = "sha256", Data = "a" }, cancel.Token));

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task CancellingAnAttemptThrowsOperationCanceled()
    {
        using CryptoServiceClient client = StubHandler.Client(StubHandler.Hang(), o => o.Retry = new RetryOptions { MaxRetries = 3 });
        using var cancel = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.HashAsync(new HashRequest { Algorithm = "sha256", Data = "a" }, cancel.Token));
    }

    [Fact]
    public async Task AnUndecodable2xxAnswerIsNotRetried()
    {
        StubHandler handler = StubHandler.Answer(HttpStatusCode.OK, "<html>");
        using CryptoServiceClient client = Client(handler, new InstantTimeProvider(), maxRetries: 3);

        CryptoTransportException error = await Assert.ThrowsAsync<CryptoTransportException>(() => HashAsync(client));

        Assert.False(error.TimedOut);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task ManyConcurrentCallsShareOneClient()
    {
        StubHandler handler = StubHandler.Answer(HttpStatusCode.OK, Hash);
        using CryptoServiceClient client = StubHandler.Client(handler);

        HashResult[] results = await Task.WhenAll(Enumerable.Range(0, 64).Select(_ => HashAsync(client)));

        Assert.All(results, r => Assert.Equal("00", r.Digest));
    }

    private static Task<HashResult> HashAsync(CryptoServiceClient client) =>
        client.HashAsync(new HashRequest { Algorithm = "sha256", Data = "a" });

    private static StubHandler Statuses(params HttpStatusCode[] statuses) =>
        StubHandler.Sequence([.. statuses.Select(s => (Func<HttpResponseMessage>)(() => StubHandler.Response(s, s == HttpStatusCode.OK ? Hash : "")))]);

    private static CryptoServiceClient Client(StubHandler handler, TimeProvider time, int maxRetries) =>
        StubHandler.Client(handler, o =>
        {
            o.TimeProvider = time;
            o.Retry = new RetryOptions { MaxRetries = maxRetries };
        });

    /// <summary>A clock that never moves and whose timers never fire.</summary>
    private sealed class FrozenTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 11, 14, 12, 0, 0, TimeSpan.Zero);

        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period) => new Never();

        private sealed class Never : ITimer
        {
            public bool Change(TimeSpan dueTime, TimeSpan period) => true;

            public void Dispose()
            {
            }

            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }
    }
}
