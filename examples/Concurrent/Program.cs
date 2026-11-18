// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau
//
// Many calls in flight on one client: the client is safe to share, its
// pooled handler reuses connections, and each call has its own timeout
// within an overall deadline. Retries are on for this example (three at
// most, idempotent hashing only), and one CancellationToken bounds the
// whole batch.
//
//   dotnet run --project examples/Concurrent

using CryptoService.Examples;
using CryptoService.Sdk;

await using CryptoServiceClient client = Example.CreateClient(o =>
{
    o.Timeout = TimeSpan.FromSeconds(10);
    o.Deadline = TimeSpan.FromSeconds(20);
    o.Retry = new RetryOptions { MaxRetries = 3 };
    o.GenerateTraceparent = true;
});
using var batch = new CancellationTokenSource(TimeSpan.FromSeconds(30));

HashResult[] digests = await Task.WhenAll(Enumerable.Range(0, 12).Select(i =>
    client.HashAsync(new HashRequest { Algorithm = "sha3-256", Data = $"record {i}" }, batch.Token))).ConfigureAwait(false);

int distinct = digests.Select(d => d.Digest).Distinct(StringComparer.Ordinal).Count();
Console.WriteLine($"{digests.Length} concurrent calls, {distinct} distinct digests");
return distinct == digests.Length ? 0 : Example.Fail("two records hashed alike");
