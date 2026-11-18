<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->
<!-- SPDX-FileCopyrightText: 2026 Sebastien Rousseau -->

<p align="center">
  <img src="https://cloudcdn.pro/crypto-service/v1/logos/crypto-service.svg" alt="Crypto Service logo" width="128" />
</p>

<h1 align="center">Crypto Service SDK for .NET</h1>

<p align="center">
  A typed, async .NET client for the Crypto Service REST API: hashing, AEAD, signatures, post-quantum KEM and signatures, HPKE and KMS calls from C#, with the cryptography done by the server.
</p>

<p align="center">
  <a href="https://github.com/sebastienrousseau/crypto-service-sdk-dotnet/actions"><img src="https://img.shields.io/github/actions/workflow/status/sebastienrousseau/crypto-service-sdk-dotnet/ci.yml?style=for-the-badge&logo=github" alt="Build" height="28" /></a>
  <a href="https://www.nuget.org/packages/CryptoService.Sdk"><img src="https://img.shields.io/nuget/v/CryptoService.Sdk?style=for-the-badge&color=fc8d62&logo=nuget" alt="Registry" height="28" /></a>
  <a href="https://sebastienrousseau.github.io/crypto-service-sdk-dotnet/api/"><img src="https://img.shields.io/badge/docs-API%20reference-blue?style=for-the-badge&labelColor=555555&logo=dotnet" alt="Docs" height="28" /></a>
  <a href="https://scorecard.dev/viewer/?uri=github.com/sebastienrousseau/crypto-service-sdk-dotnet"><img src="https://img.shields.io/ossf-scorecard/github.com/sebastienrousseau/crypto-service-sdk-dotnet?style=for-the-badge&label=OpenSSF%20Scorecard&logo=openssf" alt="OpenSSF Scorecard" height="28" /></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-Apache--2.0%20OR%20MIT-blue.svg?style=for-the-badge" alt="License: Apache-2.0 OR MIT" height="28" /></a>
  <a href="https://github.com/sebastienrousseau/crypto-service-sdk-dotnet/blob/main/docs/POLICIES.md"><img src="https://img.shields.io/badge/.NET-10-93450a.svg?style=for-the-badge&logo=dotnet" alt=".NET 10 or later" height="28" /></a>
</p>

<p align="center">
  <img src="https://raw.githubusercontent.com/sebastienrousseau/crypto-service-sdk-dotnet/main/.github/demo.gif" alt="The Hash and SignVerify examples using the .NET SDK against crypto-server on loopback: a SHA-256 digest checked against its test vector, then an Ed25519 key generated on the server, a signature by keyId, and its verification" width="100%" />
</p>

---

## Contents

**Getting started**

- [Install](#install): NuGet (`CryptoService.Sdk`), or a project reference from source
- [Requirements](#requirements): toolchain floor, platforms, a server
- [Quick Start](#quick-start): a digest and a server-held Ed25519 signature in a few lines

**The Crypto Service ecosystem**

- [The Crypto Service ecosystem](#the-crypto-service-ecosystem): the server, its 18 npm packages, the six SDKs and the conformance kit

**Library reference**

- [Capabilities at a glance](#capabilities-at-a-glance): the current surface by theme
- [Ecosystem comparison](#ecosystem-comparison): short matrix; full table at [`docs/COMPARISON.md`](docs/COMPARISON.md)
- [Benchmarks](#benchmarks): the scenarios; method at [`docs/BENCHMARKS.md`](docs/BENCHMARKS.md)
- [Features](#features): the client, its types and its error model
- [Configuration](#configuration): core options
- [Examples](#examples): runnable example index

**Operational**

- [When not to use Crypto Service](#when-not-to-use-crypto-service): limitations
- [Development](#development): make targets, fuzzing, CI
- [Security](#security): guarantees and their limits
- [Documentation](#documentation): all reference docs
- [Stability guarantees](#stability-guarantees): versioning, output stability, minimum toolchain discipline
- [License](#license)

---

## Install

### As a .NET library

```xml
<PackageReference Include="CryptoService.Sdk" Version="0.0.41" />
```

Or from the command line:

```sh
dotnet add package CryptoService.Sdk --version 0.0.41
```

The package is on
[NuGet](https://www.nuget.org/packages/CryptoService.Sdk) from 0.0.40;
the snippets name the version `main` prepares, so use the newest
published one until it is out, or reference
`src/CryptoService.Sdk/CryptoService.Sdk.csproj` from a clone. The SDK
calls a running crypto-server: build and start it from
[crypto-service](https://github.com/sebastienrousseau/crypto-service)
(`packages/crypto-server`), as [DEVELOPMENT.md](DEVELOPMENT.md#a-server)
shows.

---

## Requirements

- .NET 10 (`net10.0`), the current LTS release
  ([policy](docs/POLICIES.md)).
- No other package: the library depends on the base class library
  only, and its JSON contract is source-generated, so it is trimming-
  and Native AOT-compatible.
- A crypto-server reachable over HTTP or HTTPS, with an API key or a
  bearer token. CI tests against crypto-service at the commit pinned in
  [`ci/server.ref`](ci/server.ref).
- CI builds and tests on Linux, Windows and macOS.

---

## Quick Start

```csharp
using CryptoService.Sdk;

await using var client = new CryptoServiceClient(new CryptoServiceClientOptions
{
    BaseUrl = new Uri(Environment.GetEnvironmentVariable("CRYPTO_SERVICE_URL") ?? "http://127.0.0.1:3000"),
    ApiKey = Environment.GetEnvironmentVariable("CRYPTO_SERVICE_API_KEY"),
});

HashResult digest = await client.HashAsync(new HashRequest { Algorithm = "sha256", Data = "hello" });
Console.WriteLine($"sha256: {digest.Digest}");

// An Ed25519 key pair held by the server: the client only ever sees its keyId and public key.
GenerateKeyResult key = await client.GenerateKeyPairAsync(new GenerateKeyRequest { Algorithm = "ed25519" });
SignResult signature = await client.SignAsync(new SignRequest { KeyId = key.KeyId, Message = "hello" });
VerifyResult verdict = await client.VerifyAsync(new VerifyRequest
{
    PublicKey = key.PublicKey,
    Message = "hello",
    Signature = signature.Signature,
});
Console.WriteLine($"signature valid: {verdict.Valid}");

await client.DeleteKeyAsync(new KeyIdRequest { KeyId = key.KeyId });
```

Each call is one HTTP request to crypto-server, which holds the keys
and does the cryptography. A status outside 2xx raises
`CryptoApiException` with the server's RFC 9457 problem; no usable
answer raises `CryptoTransportException`.

---

## The Crypto Service ecosystem

crypto-server is the one engine. Each SDK is a thin client for its REST
API, and the same conformance kit, vendored into every SDK, proves each
client against a running server.

| Component | Purpose | Use case |
| :--- | :--- | :--- |
| [crypto-service](https://github.com/sebastienrousseau/crypto-service) | The engine: 18 npm packages (`@sebastienrousseau/crypto-*`), among them crypto-server, crypto-lib and the TypeScript SDK; owns the OpenAPI document and the conformance kit | Run crypto-server for this SDK to call |
| crypto-service-sdk-dotnet | This repository: the .NET client | Call the API from C#, F# or any .NET language |
| [crypto-service-sdk-python](https://github.com/sebastienrousseau/crypto-service-sdk-python), [-go](https://github.com/sebastienrousseau/crypto-service-sdk-go), [-java](https://github.com/sebastienrousseau/crypto-service-sdk-java), [-rust](https://github.com/sebastienrousseau/crypto-service-sdk-rust), [-cpp](https://github.com/sebastienrousseau/crypto-service-sdk-cpp) | The same client surface and resilience contract in Python, Go, Java, Rust and C++ | Call the API from those languages |
| [crypto-service.co](https://crypto-service.co) | The project site | Overview and documentation |
| `conformance/` | The shared conformance kit, vendored from crypto-service | Prove this client against the server it describes |

---

## Capabilities at a glance

| Area | Capability | Status |
| :--- | :--- | :--- |
| Hashing and MACs | `HashAsync` (SHA-2, SHA-3, BLAKE2b, BLAKE3), `MacAsync`, `MacVerifyAsync` | Available |
| Key derivation | `KdfAsync` (scrypt, HKDF, PBKDF2), `PasswordHashAsync`, `PasswordVerifyAsync` (Argon2id) | Available |
| Symmetric | `EncryptAsync`/`DecryptAsync`, secretbox, password encryption, AES-KW and AES-KWP | Available |
| Server-held keys | `GenerateKeyPairAsync`, `ExportKeyAsync`, `DeleteKeyAsync`; Ed25519 `SignAsync`, `VerifyAsync`, batch `StreamSignAsync`/`StreamVerifyAsync` | Available |
| Post-quantum | ML-KEM-768, the hybrid X25519 and ML-KEM-768 KEM, ML-DSA, SLH-DSA, ISO 20022 dual-signature envelopes | Available |
| Public-key encryption | Sealed boxes (classical and post-quantum), STREAM encryption (single and multi-recipient), multi-recipient envelopes, HPKE | Available |
| KMS | Create a key, wrap, unwrap, data keys, encrypt and decrypt with explicit encoding | Available |
| OPAQUE | Registration and login message exchange | Available |
| Service | `HealthAsync`, `LiveAsync`, `ReadyAsync`, `AlgorithmsAsync`, `GetServiceInfoAsync`, CBOM and DORA documents | Available |
| Low level | `RequestAsync`: any method and path, the answer returned whatever its status | Available |
| Resilience | Per-attempt timeout, call deadline, opt-in retries with the suite's retry-safety table, W3C `traceparent` | Available |
| Publication | NuGet package `CryptoService.Sdk`, by trusted publishing, with an SBOM, checksums and build provenance | Available from 0.0.40 |

Every v2 operation of the OpenAPI document is covered: 59 of 59.

---

## Ecosystem comparison

| Project | Language (minimum) | Conformance kit in CI | Call model |
| :--- | :---: | :---: | :--- |
| `@sebastienrousseau/crypto-sdk` (in crypto-service) | TypeScript, Node 24 | The server's own suite runs the kit | `async`, `fetch` |
| **crypto-service-sdk-dotnet** | **.NET 10** | **Yes, every vector** | **`async`/`await`, `CancellationToken`** |
| crypto-service-sdk-python, -go, -java, -rust, -cpp | See each repository | Same kit, same contract | See each repository |
| Raw HTTP | Any | No | Yours |

See [`docs/COMPARISON.md`](docs/COMPARISON.md) for the evidence and the complete matrix.

---

## Benchmarks

`benchmarks/CryptoService.Sdk.Benchmarks` (BenchmarkDotNet) measures
the scenarios every Crypto Service SDK names alike: `encode_request`,
`decode_response` and `decode_problem` (the client's own work, no
network) and `round_trip_hash` (one loopback call to crypto-server,
run only when `CRYPTO_SERVICE_URL` is set). CI runs a smoke pass on
every change to keep them running; it measures nothing. No numbers are
published yet.

See [`docs/BENCHMARKS.md`](docs/BENCHMARKS.md) for the method.

---

## Features

- `CryptoServiceClient`: one async method per operation, each taking
  a `CancellationToken`, safe to share across threads. Construct it
  with options alone (it owns a pooled `SocketsHttpHandler`), with
  your own `HttpMessageHandler`, or with an `HttpClient` from
  `IHttpClientFactory`.
- Typed requests and results, one class per schema, with the wire's
  encodings kept exactly: hex keys and signatures, base64 AEAD and
  stream ciphertexts, UTF-8 text. `Get*Bytes()` accessors decode the
  binary fields. Routes that answer a bare string (`SecretboxOpenAsync`,
  `SealedboxOpenAsync`, `PasswordDecryptAsync`, `KeyUnwrapAsync`)
  return a `string`; `HpkeOpenAsync` and `KmsDecryptAsync` return both
  the text and the hex bytes; `KmsEncryptRequest.FromText` and
  `FromBytes` state the encoding instead of letting the server guess.
- Errors by cause ([ADR 0003](docs/adr/0003-error-model.md)):
  `CryptoApiException` (status and the RFC 9457 `ProblemDetails`:
  `Type`, `Title`, `Status`, `Detail`, `Instance`, `Code`, `Errors`)
  and `CryptoTransportException` (`TimedOut`), both
  `CryptoServiceException`. A body that is not a problem becomes an
  `about:blank` problem.
- Resilience ([ADR 0002](docs/adr/0002-http-stack.md)): retries off by
  default; when on, idempotent calls retry after 429, 503, 504 and
  connection failures, stateful ones (key generation and deletion,
  every keygen, KMS key creation, OPAQUE registration) only after 429
  or 503. Full-jitter backoff, `Retry-After` honoured.
- Async only, the .NET idiom for I/O ([ADR 0004](docs/adr/0004-async-model.md)).
- No logging, no cryptography and no third-party runtime dependency.

---

## Configuration

```csharp
using CryptoService.Sdk;

var options = new CryptoServiceClientOptions
{
    BaseUrl = new Uri(Environment.GetEnvironmentVariable("CRYPTO_SERVICE_URL") ?? "http://127.0.0.1:3000"),
    ApiKey = Environment.GetEnvironmentVariable("CRYPTO_SERVICE_API_KEY"),
    Timeout = TimeSpan.FromSeconds(10),
    Deadline = TimeSpan.FromSeconds(30),
    Retry = new RetryOptions { MaxRetries = 3, InitialDelay = TimeSpan.FromMilliseconds(200), MaxDelay = TimeSpan.FromSeconds(2) },
    GenerateTraceparent = true,
};
Console.WriteLine(options); // the API key prints as "set (redacted)"

await using var client = new CryptoServiceClient(options);
ReadyStatus ready = await client.ReadyAsync();
Console.WriteLine($"server {ready.Status}");
```

| Option | Default | Meaning |
| :--- | :--- | :--- |
| `BaseUrl` | none (required) | The server's absolute URL; a path prefix is kept |
| `ApiKey` | none | Sent as `x-api-key` |
| `Token` | none | Sent as `Authorization: Bearer <token>`; the server tries it first |
| `Timeout` | 30 seconds | Per attempt, from sending to the last byte |
| `Deadline` | none | The whole call, retries and waits included |
| `Retry` | `MaxRetries` 0, `InitialDelay` 200 ms, `MaxDelay` 2 s | See [Features](#features); a `Retry-After` longer than `MaxDelay` ends the call |
| `Traceparent` / `GenerateTraceparent` | none / false | A fixed W3C `traceparent`, or a fresh one per request |
| `TimeProvider` | `TimeProvider.System` | The clock for waits, dates and the deadline |

The client copies the options when it is constructed. It never changes
the settings of an `HttpClient` you pass in, and does not dispose it;
give that client's primary handler `CryptoServiceClient.CreateHandler()`
so it refuses redirects too.

---

## Examples

Each example is a console project that checks its own result. CI runs
every one against crypto-server (`scripts/run-examples.sh`), with
`CRYPTO_SERVICE_URL` (default `http://127.0.0.1:3000`) and
`CRYPTO_SERVICE_API_KEY`.

| Example | Shows | Run it |
| :--- | :--- | :--- |
| [`Hash`](examples/Hash/Program.cs) | `HashAsync` with SHA-256 and BLAKE3, checked against FIPS 180-2 | `dotnet run --project examples/Hash` |
| [`AeadRoundTrip`](examples/AeadRoundTrip/Program.cs) | `EncryptAsync`/`DecryptAsync`, and a secretbox with AAD | `dotnet run --project examples/AeadRoundTrip` |
| [`SignVerify`](examples/SignVerify/Program.cs) | A server-held Ed25519 key: sign by keyId, verify, delete | `dotnet run --project examples/SignVerify` |
| [`MlKem`](examples/MlKem/Program.cs) | ML-KEM-768: encapsulate to the public key, decapsulate by keyId | `dotnet run --project examples/MlKem` |
| [`ErrorHandling`](examples/ErrorHandling/Program.cs) | Problem details, validation errors, a 401 and a transport error | `dotnet run --project examples/ErrorHandling` |
| [`Concurrent`](examples/Concurrent/Program.cs) | Many calls on one client with `Task.WhenAll`, retries and a deadline | `dotnet run --project examples/Concurrent` |

---

## When not to use Crypto Service

- **You cannot run crypto-server.** This SDK computes nothing itself;
  every call goes to the server.
- **You need cryptography in process, offline or with no network
  hop.** Use a local library (the .NET base class library, or
  crypto-lib from crypto-service on Node); this is a client.
- **You need the deprecated v1 OpenPGP routes.** They are excluded
  (sunset 2027-01-01); `RequestAsync` reaches them if you must.
- **You scrape `/metrics`.** Point your Prometheus scraper at the
  server; the SDK has no method for it.
- **You need a framework older than .NET 10, or a blocking API.** The
  package targets `net10.0` only, and is async only.

---

## Development

| Command | What it runs |
| :--- | :--- |
| `make check` | Format, build (analysers, complexity), 100 % line and branch coverage, package audit, versions, Halstead |
| `make conformance` | The conformance kit against `CRYPTO_SERVICE_URL`; fails without a server |
| `make examples` | Every example against `CRYPTO_SERVICE_URL` |
| `make readme` | Every code block of this README, compiled and run against `CRYPTO_SERVICE_URL` |
| `make fuzz` | Both SharpFuzz targets under libfuzzer-dotnet, 60 s each |
| `make bench` | BenchmarkDotNet; `round_trip_hash` too when `CRYPTO_SERVICE_URL` is set |
| `make regression` | All of it against the pinned server, which it builds and starts |
| `make docs`, `make docs-lint` | The manual (strict), and markdownlint, codespell and REUSE |

The build treats warnings as errors with the `latest-recommended` .NET
analysers; CA1502 and SonarAnalyzer hold every method to cyclomatic
complexity 10, cognitive complexity 15 and 60 lines, every file to 500
lines, and `scripts/halstead.py` every method to Halstead difficulty 30.
Unit tests run under coverlet and fail below 100 % line and branch
coverage; FsCheck properties drive the parsers with arbitrary bytes,
and two SharpFuzz targets fuzz them with coverage guidance.
[DEVELOPMENT.md](DEVELOPMENT.md) maps every gate to its local command.

---

## Security

The SDK performs no cryptography and logs nothing. It never puts a
credential, a key, a plaintext or a body in an exception message or a
`ToString()`: options print their credentials as redacted, and an
error message names the status, the problem type and its title only.
It refuses redirects, never disables TLS certificate validation (there
is no option to), bounds every attempt with a timeout, and retries a
stateful call only when the server certainly did not act. It has no
third-party runtime dependency. Error bodies are read up to 64 KiB.

Report vulnerabilities according to [`SECURITY.md`](SECURITY.md).

---

## Documentation

- [User manual](https://sebastienrousseau.github.io/crypto-service-sdk-dotnet/),
  built from [`docs/`](docs/index.md).
- [API reference](https://sebastienrousseau.github.io/crypto-service-sdk-dotnet/api/),
  generated from the XML documentation comments, which also ship in the
  package for IDE tooltips.
- [Developer docs](DEVELOPMENT.md): toolchain, every CI gate, test
  layout, release model.
- [Architecture](docs/ARCHITECTURE.md) and the
  [decision records](docs/adr/index.md).
- [`conformance/README.md`](conformance/README.md): the conformance kit
  and its vector format.

---

## Stability guarantees

The version is `0.0.x` and moves by `0.0.1`; before `1.0` any release
may change the API, and the [CHANGELOG](CHANGELOG.md) says how. Each
release runs package validation (ApiCompat) against the previous
published package, so no break is accidental. Output stability: the
same server answer decodes to the same objects and raises the same
exception types; a change to that mapping is breaking even when no
signature moves. Each release vendors the conformance kit of the
crypto-service commit in `ci/server.ref`, and CI fails if any vector
fails against that server. The v1 routes and `/metrics` are outside
the SDK's surface. The `net10.0` floor moves only with a release that
says so ([policy](docs/POLICIES.md)).

---

## License

Dual-licensed under [Apache-2.0](LICENSE-APACHE) or [MIT](LICENSE-MIT),
at your option. See [LICENSE](LICENSE). Every file carries its licence
in machine-readable form ([REUSE](https://reuse.software/), checked in
CI).
