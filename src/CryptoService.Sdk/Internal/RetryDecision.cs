// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using System.Net.Http.Headers;

namespace CryptoService.Sdk.Internal;

/// <summary>An attempt that did not succeed, and what the retry decision needs of it.</summary>
/// <param name="Error">What to raise when no retry follows.</param>
/// <param name="Status">The HTTP status, or <see langword="null"/> when no answer arrived.</param>
/// <param name="RetryAfter">The answer's <c>Retry-After</c>, or <see langword="null"/>.</param>
/// <param name="Retryable">For an attempt without an answer: whether another
/// attempt may follow (a call whose deadline expired may not).</param>
internal sealed record Failure(
    CryptoServiceException Error,
    int? Status,
    RetryConditionHeaderValue? RetryAfter,
    bool Retryable);

/// <summary>The retry contract: when to try again, and after how long.</summary>
internal static class RetryDecision
{
    /// <summary>
    /// The wait before the attempt after <paramref name="attempt"/>, or
    /// <see langword="null"/> to give up and raise its error.
    /// </summary>
    internal static TimeSpan? DelayAfter(RetryOptions retry, int attempt, bool idempotent, Failure failure, DateTimeOffset now)
    {
        if (attempt > retry.MaxRetries)
        {
            return null;
        }

        if (failure.Status is not int status)
        {
            return failure.Retryable && idempotent ? Backoff(retry, attempt) : null;
        }

        // A 504 may have been processed upstream; a 429 or 503 was not.
        if (status is not (429 or 503 or 504) || (status == 504 && !idempotent))
        {
            return null;
        }

        if (RetryAfter(failure.RetryAfter, now) is not TimeSpan asked)
        {
            return Backoff(retry, attempt);
        }

        return asked <= retry.MaxDelay ? asked : null;
    }

    /// <summary>
    /// Exponential backoff with full jitter: a random time between zero and
    /// <c>min(MaxDelay, InitialDelay × 2^(attempt-1))</c>.
    /// </summary>
    internal static TimeSpan Backoff(RetryOptions retry, int attempt)
    {
        double ceiling = Math.Min(retry.MaxDelay.Ticks, retry.InitialDelay.Ticks * Math.Pow(2, attempt - 1));
        return TimeSpan.FromTicks((long)(Random.Shared.NextDouble() * ceiling));
    }

    /// <summary>The wait a <c>Retry-After</c> asks for (seconds or an HTTP date), or <see langword="null"/>.</summary>
    internal static TimeSpan? RetryAfter(RetryConditionHeaderValue? header, DateTimeOffset now)
    {
        if (header is null)
        {
            return null;
        }

        if (header.Delta is TimeSpan delta)
        {
            return delta;
        }

        // A Retry-After is a delta or a date: with no delta, it has a date.
        DateTimeOffset date = header.Date.GetValueOrDefault();
        return date > now ? date - now : TimeSpan.Zero;
    }
}
