<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->
<!-- SPDX-FileCopyrightText: 2026 Sebastien Rousseau -->

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
The version follows the Crypto Service suite and moves by `0.0.1`.

## [0.0.41] - Unreleased

## [0.0.40] - 2026-11-16

### Added

- `CryptoServiceClient`: one async method per v2 operation of the
  crypto-server OpenAPI document (59 of 59), plus `GetServiceInfoAsync`,
  `HealthAsync`, `LiveAsync` and `ReadyAsync`, and `RequestAsync` for
  any other route. The v1 routes and `/metrics` are excluded.
- Typed requests and results with the wire's encodings kept, and
  `Get*Bytes()` accessors for binary fields; `KmsEncryptRequest.FromText`
  and `FromBytes` state the encoding.
- The suite's resilience contract: per-attempt `Timeout` (30 s),
  optional `Deadline`, opt-in retries (`RetryOptions`: 0 retries,
  200 ms, 2 s) with full-jitter backoff and `Retry-After`, the
  retry-safety table of the TypeScript SDK, W3C `traceparent`, a pooled
  `SocketsHttpHandler` with HTTP/2 negotiation and no redirects.
- The error model: `CryptoApiException` with the RFC 9457
  `ProblemDetails`, `CryptoTransportException` with `TimedOut`, both
  `CryptoServiceException`.
- The conformance kit of crypto-service at
  `3e8ecfd757e3577011953d8b47677f4c003426fb`, run in CI through the
  public methods; unit tests at 100 % line and branch coverage, FsCheck
  properties, two SharpFuzz targets, BenchmarkDotNet scenarios, six
  examples, the manual and the release workflow (NuGet trusted
  publishing).
