<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->
<!-- SPDX-FileCopyrightText: 2026 Sebastien Rousseau -->

# Architecture

`CryptoService.Sdk` is a thin client: every call is a request to
crypto-server, which does the cryptography
([ADR 0001](adr/0001-thin-client-over-the-server.md)). This page is for
contributors: where things live and how a call flows.

## Layout

| Path | What it holds |
| :--- | :--- |
| `src/CryptoService.Sdk/CryptoServiceClient.cs` | Construction, disposal, `RequestAsync` and the generic send helpers |
| `src/CryptoService.Sdk/CryptoServiceClient.*.cs` | One public method per operation, by area (Meta, Hashing, Symmetric, Keys, PostQuantum, Asymmetric, Kms, Opaque) |
| `src/CryptoService.Sdk/Models/` | Request and result classes, one per schema |
| `src/CryptoService.Sdk/Internal/Transport.cs` | Request building, timeouts, the deadline, the retry loop, redirect refusal |
| `src/CryptoService.Sdk/Internal/RetryDecision.cs`, `Idempotency.cs` | When to retry and after how long; the retry-safety table |
| `src/CryptoService.Sdk/Internal/Decoding.cs` | 2xx decoding and problem parsing (both fuzzed) |
| `src/CryptoService.Sdk/Internal/CryptoJsonContext.cs` | The source-generated JSON contract |
| `src/CryptoService.Sdk/Crypto*Exception.cs`, `ProblemDetails.cs` | The error model ([ADR 0003](adr/0003-error-model.md)) |
| `conformance/`, `ci/server.ref` | crypto-service's conformance kit, vendored byte for byte, and the commit it came from |
| `tests/`, `fuzz/`, `benchmarks/`, `examples/` | Unit and conformance tests, SharpFuzz targets, BenchmarkDotNet, runnable examples |

## How a call flows

```text
SignAsync(SignRequest)
  -> PostAsync: argument check, the request to UTF-8 JSON (source-generated, nulls omitted)
  -> Transport.SendAsync: a CancellationTokenSource for the deadline; is the route idempotent?
     -> AttemptAsync: a linked source for the per-attempt timeout
        -> BuildRequest: Accept, User-Agent, traceparent, x-api-key or Bearer, HTTP/2 policy
        -> HttpClient.SendAsync (headers first), redirect check
        -> 2xx: Decoding.ReadSuccessAsync streams the body into DataEnvelope<SignResult>
        -> else: Decoding.ReadErrorAsync reads up to 64 KiB into a ProblemDetails
     -> failure: RetryDecision.DelayAfter (status, idempotency, Retry-After, backoff)
        -> wait, or raise CryptoApiException / CryptoTransportException
  -> the envelope's data
```

## Invariants

- **No cryptography in the client**; byte accessors only decode the
  encoding the server chose.
- **No secret in a message or `ToString()`**; options redact
  credentials, models print their type name only.
- **A stateful call is never repeated after the server may have acted**
  (504, a lost connection, a timeout).
- **Immutable after construction**, so one client serves concurrent
  calls from any thread.
- **The caller's `HttpClient` is theirs**: the client never changes its
  settings and disposes only an `HttpClient` it created.

## Where the contract comes from

crypto-service's OpenAPI document is the contract; the models follow
its schemas by hand, and the conformance kit pins statuses and bodies.
The kit runner re-encodes each typed answer with the SDK's own contract
before matching it, so a member the models drop or misname fails a
vector. When crypto-service changes the wire, a new kit and
`ci/server.ref` come in together.
