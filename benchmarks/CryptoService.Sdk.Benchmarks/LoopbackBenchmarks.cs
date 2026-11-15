// SPDX-License-Identifier: Apache-2.0 OR MIT

using BenchmarkDotNet.Attributes;

namespace CryptoService.Sdk.Benchmarks;

/// <summary>
/// One round trip per call to a running crypto-server at
/// <c>CRYPTO_SERVICE_URL</c> (with <c>CRYPTO_SERVICE_API_KEY</c>): the
/// client, the loopback stack and the server's work together.
/// </summary>
[MemoryDiagnoser]
public class LoopbackBenchmarks : IDisposable
{
    private static readonly HashRequest Request = new() { Algorithm = "sha256", Data = "abc" };
    private CryptoServiceClient? _client;

    /// <summary>The server address, or <see langword="null"/> to skip these benchmarks.</summary>
    internal static string? BaseUrl =>
        Environment.GetEnvironmentVariable("CRYPTO_SERVICE_URL") is { Length: > 0 } url ? url : null;

    [GlobalSetup]
    public void Setup() =>
        _client = new CryptoServiceClient(new CryptoServiceClientOptions
        {
            BaseUrl = new Uri(BaseUrl ?? throw new InvalidOperationException("CRYPTO_SERVICE_URL is unset.")),
            ApiKey = Environment.GetEnvironmentVariable("CRYPTO_SERVICE_API_KEY"),
        });

    [Benchmark(Description = "round_trip_hash")]
    public Task<HashResult> RoundTripHash() =>
        (_client ?? throw new InvalidOperationException("Setup has not run.")).HashAsync(Request);

    [GlobalCleanup]
    public void Dispose()
    {
        _client?.Dispose();
        _client = null;
        GC.SuppressFinalize(this);
    }
}
