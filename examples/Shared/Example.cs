// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using CryptoService.Sdk;

namespace CryptoService.Examples;

/// <summary>
/// What every example shares: the server address from
/// <c>CRYPTO_SERVICE_URL</c> (default <c>http://127.0.0.1:3000</c>) and the
/// API key from <c>CRYPTO_SERVICE_API_KEY</c>. Every client retries up to
/// three times: the server meters 60 requests a minute per principal and
/// answers 429 with a short <c>Retry-After</c> past that.
/// </summary>
internal static class Example
{
    /// <summary>The server's base URL.</summary>
    internal static Uri BaseUrl =>
        new(Environment.GetEnvironmentVariable("CRYPTO_SERVICE_URL") is { Length: > 0 } url ? url : "http://127.0.0.1:3000");

    /// <summary>The API key, or <see langword="null"/> when unset.</summary>
    internal static string? ApiKey => Environment.GetEnvironmentVariable("CRYPTO_SERVICE_API_KEY");

    /// <summary>A client for the configured server.</summary>
    internal static CryptoServiceClient CreateClient(Action<CryptoServiceClientOptions>? configure = null)
    {
        var options = new CryptoServiceClientOptions
        {
            BaseUrl = BaseUrl,
            ApiKey = ApiKey,
            Retry = new RetryOptions { MaxRetries = 3 },
        };
        configure?.Invoke(options);
        return new CryptoServiceClient(options);
    }

    /// <summary>Prints a failed expectation and returns the exit code 1.</summary>
    internal static int Fail(string message)
    {
        Console.Error.WriteLine("unexpected: " + message);
        return 1;
    }
}
