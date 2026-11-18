// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using System.Net.Http.Headers;
using System.Text;
using CryptoService.Sdk.Internal;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

namespace CryptoService.Sdk.Tests;

/// <summary>
/// Properties over arbitrary input: the two parsers never fail in an
/// unexpected way, and the retry arithmetic stays in its bounds. The fuzz
/// targets (fuzz/) drive the same two parsers with coverage guidance.
/// </summary>
public sealed class PropertyTests
{
    [Property(MaxTest = 2000)]
    public bool TheProblemParserNeverThrows(byte[] body, int status, string? reason)
    {
        ProblemDetails problem = Decoding.ParseProblem(body, status, reason);
        return problem.Type is not null && problem.Title is not null && problem.Detail is not null;
    }

    [Property(MaxTest = 2000)]
    public bool TheProblemParserNeverThrowsOnJsonLikeText(NonNull<string> text)
    {
        byte[] body = Encoding.UTF8.GetBytes("{\"type\":\"t\",\"title\":\"t\",\"status\":1,\"detail\":" + text.Get + "}");
        return Decoding.ParseProblem(body, 500, null) is not null;
    }

    [Property(MaxTest = 2000)]
    public bool TheSuccessDecoderFailsOnlyWithATransportError(byte[] body)
    {
        try
        {
            return Decoding.DecodeSuccess(body, CryptoJsonContext.Default.HashResultEnvelope).Data is not null;
        }
        catch (CryptoTransportException)
        {
            return true;
        }
    }

    [Property]
    public bool TheBackoffStaysUnderItsCeiling(PositiveInt attempt, byte initialMs, byte maxMs)
    {
        var retry = new RetryOptions
        {
            InitialDelay = TimeSpan.FromMilliseconds(initialMs),
            MaxDelay = TimeSpan.FromMilliseconds(maxMs),
        };
        int n = Math.Min(attempt.Get, 64);
        TimeSpan delay = RetryDecision.Backoff(retry, n);
        double ceiling = Math.Min(maxMs, initialMs * Math.Pow(2, n - 1));
        return delay >= TimeSpan.Zero && delay.TotalMilliseconds <= ceiling;
    }

    [Property]
    public Property ARetryAfterDeltaIsTakenAsIs()
    {
        return Prop.ForAll(
            Gen.Choose(0, 86_400).ToArbitrary(),
            seconds => RetryDecision.RetryAfter(new RetryConditionHeaderValue(TimeSpan.FromSeconds(seconds)), DateTimeOffset.UnixEpoch)
                == TimeSpan.FromSeconds(seconds));
    }
}
