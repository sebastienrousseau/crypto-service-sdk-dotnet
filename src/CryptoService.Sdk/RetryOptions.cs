// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using System.Globalization;

namespace CryptoService.Sdk;

/// <summary>
/// How the client retries a failed attempt. The same contract holds in
/// every Crypto Service SDK.
/// </summary>
/// <remarks>
/// <para>
/// An idempotent call (every <c>GET</c> and every <c>POST</c> that cannot
/// create or delete server state) is retried after HTTP 429, 503 or 504,
/// after a connection failure and after an attempt that timed out. A
/// stateful call (<c>/v2/keys/generate</c>, <c>/v2/keys/delete</c>, every
/// <c>*/keygen</c>, <c>/v2/kms/create-key</c> and
/// <c>/v2/opaque/register/*</c>) is retried only after 429 or 503, when the
/// server certainly did not act; never after a 504 or a lost connection,
/// when it may have.
/// </para>
/// <para>
/// Between attempts the client waits an exponential backoff with full
/// jitter: for retry <c>n</c>, a random time between zero and
/// <c>min(MaxDelay, InitialDelay × 2^(n-1))</c>. A <c>Retry-After</c>
/// header (seconds or an HTTP date) replaces the backoff; when it asks for
/// longer than <see cref="MaxDelay"/>, the client stops and reports that
/// answer instead of waiting.
/// </para>
/// </remarks>
public sealed class RetryOptions
{
    private readonly int _maxRetries;
    private readonly TimeSpan _initialDelay = TimeSpan.FromMilliseconds(200);
    private readonly TimeSpan _maxDelay = TimeSpan.FromSeconds(2);

    /// <summary>Retries after the first attempt. Defaults to 0: no retry.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is negative.</exception>
    public int MaxRetries
    {
        get => _maxRetries;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            _maxRetries = value;
        }
    }

    /// <summary>The backoff ceiling of the first retry. Defaults to 200 ms.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is negative.</exception>
    public TimeSpan InitialDelay
    {
        get => _initialDelay;
        init
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, TimeSpan.Zero);
            _initialDelay = value;
        }
    }

    /// <summary>
    /// The longest wait between two attempts, for the backoff and for a
    /// <c>Retry-After</c>. Defaults to 2 seconds.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is negative.</exception>
    public TimeSpan MaxDelay
    {
        get => _maxDelay;
        init
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, TimeSpan.Zero);
            _maxDelay = value;
        }
    }

    /// <inheritdoc />
    public override string ToString() =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"RetryOptions {{ MaxRetries = {MaxRetries}, InitialDelay = {InitialDelay}, MaxDelay = {MaxDelay} }}");
}
