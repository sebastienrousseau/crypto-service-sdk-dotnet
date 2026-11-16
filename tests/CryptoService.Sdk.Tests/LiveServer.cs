// SPDX-License-Identifier: Apache-2.0 OR MIT

namespace CryptoService.Sdk.Tests;

/// <summary>
/// The live server the conformance tests run against:
/// <c>CRYPTO_SERVICE_URL</c> and <c>CRYPTO_SERVICE_API_KEY</c>, and
/// optionally <c>CRYPTO_SERVICE_JWT_SECRET</c> (with
/// <c>CRYPTO_SERVICE_JWT_ISSUER</c> and <c>CRYPTO_SERVICE_JWT_AUDIENCE</c>
/// when the server pins them) to run the vectors that need scoped tokens.
/// </summary>
internal static class LiveServer
{
    /// <summary>The server's base URL, or <see langword="null"/>.</summary>
    internal static string? Url => Env("CRYPTO_SERVICE_URL");

    /// <summary>The server's API key, or <see langword="null"/>.</summary>
    internal static string? ApiKey => Env("CRYPTO_SERVICE_API_KEY");

    /// <summary>The server's JWT_SECRET, or <see langword="null"/>.</summary>
    internal static string? JwtSecret => Env("CRYPTO_SERVICE_JWT_SECRET");

    /// <summary>The JWT issuer the server pins, or <see langword="null"/>.</summary>
    internal static string? JwtIssuer => Env("CRYPTO_SERVICE_JWT_ISSUER");

    /// <summary>The JWT audience the server pins, or <see langword="null"/>.</summary>
    internal static string? JwtAudience => Env("CRYPTO_SERVICE_JWT_AUDIENCE");

    /// <summary>
    /// Whether a missing server fails the run instead of skipping it:
    /// <c>make conformance</c> sets <c>CRYPTO_SERVICE_CONFORMANCE=required</c>.
    /// </summary>
    internal static bool Required => Env("CRYPTO_SERVICE_CONFORMANCE") == "required";

    /// <summary>Why a server test is skipped, or <see langword="null"/> to run it.</summary>
    internal static string? SkipReason =>
        Url is null && !Required
            ? "CRYPTO_SERVICE_URL is unset: start crypto-server and set CRYPTO_SERVICE_URL and CRYPTO_SERVICE_API_KEY to run the conformance kit (make conformance)."
            : null;

    private static string? Env(string name) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value : null;
}

/// <summary>A theory that runs only against a live server, or always under <c>make conformance</c>.</summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class ServerTheoryAttribute : TheoryAttribute
{
    /// <summary>Skips the test when no server is configured and none is required.</summary>
    public ServerTheoryAttribute()
    {
        if (LiveServer.SkipReason is { } reason)
        {
            Skip = reason;
        }
    }
}
