// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

namespace CryptoService.Sdk;

/// <summary>
/// Where crypto-server is, how to authenticate to it, and how long and how
/// often to try. The client copies these values when it is constructed;
/// changing the options afterwards does not affect it.
/// </summary>
public sealed class CryptoServiceClientOptions
{
    private TimeProvider _timeProvider = TimeProvider.System;

    /// <summary>
    /// The server's absolute base URL, for example
    /// <c>https://crypto.internal.example</c>. A path prefix is kept, so a
    /// server mounted under <c>/crypto</c> works too. Required.
    /// </summary>
    public Uri? BaseUrl { get; set; }

    /// <summary>
    /// The static API key, sent as the <c>x-api-key</c> header. Leave it
    /// <see langword="null"/> to authenticate with <see cref="Token"/>.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// A bearer token (an HS256 JWT the server verifies), sent as
    /// <c>Authorization: Bearer &lt;token&gt;</c>. The server tries it
    /// before the API key.
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// How long one attempt may take, from sending the request to the last
    /// byte of the answer. Defaults to 30 seconds;
    /// <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> disables it.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// The budget for a whole call, every attempt and every retry wait
    /// included, or <see langword="null"/> (the default) for none. A retry
    /// whose wait would end after the deadline is not made.
    /// </summary>
    public TimeSpan? Deadline { get; set; }

    /// <summary>
    /// The retry policy. The default makes no retry
    /// (<see cref="RetryOptions.MaxRetries"/> is 0).
    /// </summary>
    public RetryOptions Retry { get; set; } = new();

    /// <summary>
    /// A W3C <c>traceparent</c> value sent with every request, or
    /// <see langword="null"/>. Ignored when
    /// <see cref="GenerateTraceparent"/> is set.
    /// </summary>
    public string? Traceparent { get; set; }

    /// <summary>
    /// Whether to generate a fresh W3C <c>traceparent</c> (version 00,
    /// random trace and parent IDs, sampled) for every request.
    /// </summary>
    public bool GenerateTraceparent { get; set; }

    /// <summary>
    /// The clock that measures retry waits, <c>Retry-After</c> dates and the
    /// <see cref="Deadline"/>. Defaults to <see cref="TimeProvider.System"/>;
    /// a test can pass a fake one.
    /// </summary>
    /// <exception cref="ArgumentNullException">The value is
    /// <see langword="null"/>.</exception>
    public TimeProvider TimeProvider
    {
        get => _timeProvider;
        set => _timeProvider = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Describes the options with the credentials redacted: the API key and
    /// token appear as <c>set</c> or <c>unset</c>, never their value, and
    /// user info in the base URL (which the client refuses) is redacted.
    /// </summary>
    /// <returns>A description safe to log.</returns>
    public override string ToString() =>
        $"CryptoServiceClientOptions {{ BaseUrl = {Describe(BaseUrl)}, ApiKey = {Redact(ApiKey)}, Token = {Redact(Token)}, Timeout = {Timeout}, Deadline = {Deadline?.ToString() ?? "none"}, Retry = {Retry} }}";

    private static string Redact(string? secret) => secret is null ? "unset" : "set (redacted)";

    private static string? Describe(Uri? url)
    {
        if (url is null || !url.IsAbsoluteUri || url.UserInfo.Length == 0)
        {
            return url?.ToString();
        }

        return url.AbsoluteUri.Replace(url.UserInfo + "@", "(user info redacted)@", StringComparison.Ordinal);
    }
}
