<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->
<!-- SPDX-FileCopyrightText: 2026 Sebastien Rousseau -->

# Crypto Service SDK for .NET

A typed, async .NET client for the Crypto Service REST API
(crypto-server v2): hashing, authenticated encryption, signatures,
post-quantum KEM and signatures, HPKE and KMS calls from C#, F# or any
.NET language. The server holds the keys and does the cryptography;
this package turns its JSON into typed calls, and crypto-service's
conformance kit proves it against a live server in CI.

```csharp
using CryptoService.Sdk;

await using var client = new CryptoServiceClient(new CryptoServiceClientOptions
{
    BaseUrl = new Uri("http://127.0.0.1:3000"),
    ApiKey = Environment.GetEnvironmentVariable("CRYPTO_SERVICE_API_KEY"),
});
HashResult digest = await client.HashAsync(new HashRequest { Algorithm = "sha256", Data = "hello" });
Console.WriteLine(digest.Digest);
```

## In this manual

- [Getting started](getting-started.md): install, a server, the first call.
- [Examples](examples.md): runnable projects, each checked in CI.
- [API reference](api/index.md): every public type and member, from the
  XML documentation comments.
- [Architecture](ARCHITECTURE.md) and the
  [decision records](adr/index.md).
- [Comparison](COMPARISON.md) with the suite's other clients.
- [Benchmarks](BENCHMARKS.md): the scenarios and how to run them.
- [Packaging](packaging.md), [Policies](POLICIES.md), [Roadmap](ROADMAP.md).
- For contributors: [Development](development.md),
  [Contributing](contributing.md), [Governance](governance.md),
  [Security](security.md), [Support](support.md),
  [Changelog](changelog.md).
