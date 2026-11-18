// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Serialization.Metadata;

namespace CryptoService.Sdk.Internal;

/// <summary>One call: what to send, and whether to send the credentials.</summary>
/// <param name="Method">The HTTP method.</param>
/// <param name="Path">The path from the base URL, starting with <c>/</c>.</param>
/// <param name="Body">The JSON body, or <see langword="null"/> for none.</param>
/// <param name="Authenticate">Whether to send the API key or token.</param>
internal sealed record Call(HttpMethod Method, string Path, byte[]? Body, bool Authenticate);

/// <summary>
/// Sends calls: builds each request, bounds each attempt by the timeout
/// and the call by the deadline, retries as <see cref="RetryDecision"/>
/// says, and decodes the answer. Immutable after construction, so one
/// instance serves any number of concurrent calls.
/// </summary>
internal sealed class Transport
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly string? _apiKey;
    private readonly string? _token;
    private readonly string? _traceparent;
    private readonly bool _generateTraceparent;
    private readonly TimeSpan _timeout;
    private readonly TimeSpan? _deadline;
    private readonly RetryOptions _retry;
    private readonly TimeProvider _time;

    internal Transport(HttpClient http, CryptoServiceClientOptions options)
    {
        _http = http;
        _baseUrl = CheckBaseUrl(options.BaseUrl);
        _apiKey = options.ApiKey;
        _token = options.Token;
        _traceparent = string.IsNullOrWhiteSpace(options.Traceparent) ? null : options.Traceparent.Trim();
        _generateTraceparent = options.GenerateTraceparent;
        _timeout = CheckTimeout(options.Timeout, nameof(options.Timeout));
        _deadline = options.Deadline is { } deadline ? CheckTimeout(deadline, nameof(options.Deadline)) : null;
        _retry = options.Retry ?? throw new ArgumentException("Retry must not be null.", nameof(options));
        _time = options.TimeProvider;
    }

    /// <summary>The <c>User-Agent</c> every request carries.</summary>
    internal static string UserAgent { get; } =
        "crypto-service-sdk-dotnet/" + typeof(Transport).Assembly.GetName().Version!.ToString(3);

    /// <summary>The base URL without a trailing slash.</summary>
    internal string BaseUrl => _baseUrl;

    /// <summary>Sends a call, retrying as the policy allows, and decodes its 2xx answer.</summary>
    internal async Task<T> SendAsync<T>(Call call, JsonTypeInfo<T> info, CancellationToken cancellationToken)
    {
        using CancellationTokenSource budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        DateTimeOffset? end = null;
        if (_deadline is { } deadline)
        {
            budget.CancelAfter(deadline);
            end = _time.GetUtcNow() + deadline;
        }

        bool idempotent = Idempotency.IsIdempotent(call.Method, call.Path);
        for (int attempt = 1; ; attempt++)
        {
            (T? value, Failure? failure) = await AttemptAsync(call, info, budget.Token, cancellationToken).ConfigureAwait(false);
            if (failure is null)
            {
                return value!;
            }

            DateTimeOffset now = _time.GetUtcNow();
            TimeSpan? wait = RetryDecision.DelayAfter(_retry, attempt, idempotent, failure, now);
            if (wait is not { } delay || now + delay >= end)
            {
                throw failure.Error;
            }

            await WaitAsync(delay, budget.Token, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Sends one request, once, and returns whatever the server answered;
    /// the low-level call behind <see cref="CryptoServiceClient.RequestAsync"/>.
    /// </summary>
    internal async Task<RawResponse> SendRawAsync(Call call, string? mediaType, CancellationToken cancellationToken)
    {
        using CancellationTokenSource attempt = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        attempt.CancelAfter(_timeout);
        try
        {
            using HttpRequestMessage request = BuildRequest(call, mediaType);
            using HttpResponseMessage response = await SendCheckedAsync(request, attempt.Token).ConfigureAwait(false);
            byte[] body = await response.Content.ReadAsByteArrayAsync(attempt.Token).ConfigureAwait(false);
            return new RawResponse((int)response.StatusCode, response.Content.Headers.ContentType?.MediaType, body);
        }
        catch (Exception ex) when (Lost(ex, cancellationToken, cancellationToken) is { } failure)
        {
            throw failure.Error;
        }
    }

    private async Task<(T? Value, Failure? Failure)> AttemptAsync<T>(
        Call call,
        JsonTypeInfo<T> info,
        CancellationToken budget,
        CancellationToken caller)
    {
        using CancellationTokenSource attempt = CancellationTokenSource.CreateLinkedTokenSource(budget);
        attempt.CancelAfter(_timeout);
        try
        {
            using HttpRequestMessage request = BuildRequest(call, null);
            using HttpResponseMessage response = await SendCheckedAsync(request, attempt.Token).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return (await Decoding.ReadSuccessAsync(response.Content, info, attempt.Token).ConfigureAwait(false), null);
            }

            CryptoApiException error = await Decoding.ReadErrorAsync(response, attempt.Token).ConfigureAwait(false);
            return (default, new Failure(error, (int)response.StatusCode, response.Headers.RetryAfter, Retryable: false));
        }
        catch (Exception ex) when (Lost(ex, budget, caller) is { } failure)
        {
            return (default, failure);
        }
    }

    // A failure with no answer, or null for an exception that is not one
    // (the caller's cancellation, a decoding error), which propagates.
    private static Failure? Lost(Exception ex, CancellationToken budget, CancellationToken caller)
    {
        switch (ex)
        {
            case OperationCanceledException when caller.IsCancellationRequested:
                return null;
            case OperationCanceledException:
                bool expired = budget.IsCancellationRequested;
                string what = expired ? "The call's deadline expired." : "The attempt timed out.";
                return new Failure(new CryptoTransportException(what, ex, timedOut: true), null, null, Retryable: !expired);
            case HttpRequestException or IOException:
                return new Failure(
                    new CryptoTransportException("The connection failed before an answer arrived.", ex, timedOut: false),
                    null,
                    null,
                    Retryable: true);
            default:
                return null;
        }
    }

    private async Task<HttpResponseMessage> SendCheckedAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Uri sent = request.RequestUri!;
        HttpResponseMessage response = await _http
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        // An injected HttpClient may follow redirects itself, rewriting the
        // request's URI; the answer of another URL is refused, not decoded.
        if (request.RequestUri != sent)
        {
            response.Dispose();
            throw new CryptoTransportException("The server redirected the request; the client refuses redirects.");
        }

        return response;
    }

    private async Task WaitAsync(TimeSpan delay, CancellationToken budget, CancellationToken caller)
    {
        try
        {
            await Task.Delay(delay, _time, budget).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (!caller.IsCancellationRequested)
        {
            throw new CryptoTransportException("The call's deadline expired.", ex, timedOut: true);
        }

        // A wait that ends as the token is cancelled can complete rather than
        // throw; the next attempt must not be sent either way.
        caller.ThrowIfCancellationRequested();
    }

    private HttpRequestMessage BuildRequest(Call call, string? mediaType)
    {
        var request = new HttpRequestMessage(call.Method, new Uri(_baseUrl + call.Path, UriKind.Absolute))
        {
            Version = HttpVersion.Version20,
            VersionPolicy = HttpVersionPolicy.RequestVersionOrLower,
        };
        HttpRequestHeaders headers = request.Headers;
        headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/problem+json"));
        headers.TryAddWithoutValidation("User-Agent", UserAgent);
        string? traceparent = _generateTraceparent ? NewTraceparent() : _traceparent;
        if (traceparent is not null)
        {
            headers.TryAddWithoutValidation("traceparent", traceparent);
        }

        if (call.Authenticate)
        {
            AddCredentials(headers);
        }

        if (call.Body is { } body)
        {
            request.Content = new ByteArrayContent(body);
            request.Content.Headers.TryAddWithoutValidation("Content-Type", mediaType ?? "application/json");
        }

        return request;
    }

    private void AddCredentials(HttpRequestHeaders headers)
    {
        if (_apiKey is not null)
        {
            headers.TryAddWithoutValidation("x-api-key", _apiKey);
        }

        if (_token is not null)
        {
            headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }
    }

    /// <summary>A W3C trace context header: version 00, random trace and parent IDs, sampled.</summary>
    internal static string NewTraceparent() =>
        $"00-{ActivityTraceId.CreateRandom().ToHexString()}-{ActivitySpanId.CreateRandom().ToHexString()}-01";

    private static string CheckBaseUrl(Uri? baseUrl)
    {
        if (baseUrl is null || !baseUrl.IsAbsoluteUri)
        {
            throw new ArgumentException("BaseUrl must be an absolute URI.", nameof(baseUrl));
        }

        return baseUrl.AbsoluteUri.TrimEnd('/');
    }

    private static TimeSpan CheckTimeout(TimeSpan value, string name)
    {
        if (value <= TimeSpan.Zero && value != Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentException(name + " must be positive, or infinite.", name);
        }

        return value;
    }
}
