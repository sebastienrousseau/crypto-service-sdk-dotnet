<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->
<!-- SPDX-FileCopyrightText: 2026 Sebastien Rousseau -->

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
The version follows the Crypto Service suite and moves by `0.0.1`.

## [0.0.41] - Unreleased

### Security

- The client refuses an API key, a token or a `traceparent` holding a
  line break or a NUL, and `RequestAsync` a media type holding one:
  `HttpClient` sent such a value as is, injecting a header into every
  request (security review SR-1).
- The client refuses a base URL with user info, a query or a fragment.
  User info appeared in `ToString()`, and a query or a fragment sent
  every call to the server's root path; the options now redact user
  info when described (SR-2).
- A publishing run of the release workflow must start from its tag, and
  must build with exactly the .NET SDK `global.json` pins (SR-3, SR-4).

### Added

- `docs/ASSURANCE.md`, the assurance case: threat model, trust
  boundaries, secure design principles and common weaknesses, each
  with the code and tests behind it.
- `docs/security-review-2026-11.md`, the first recorded security
  review, with its findings and their disposition.
- The vulnerability response process in `SECURITY.md`: acknowledgement,
  triage, fix and release, advisory and CVE, coordinated disclosure and
  credit.
- Reproducible release assets: `scripts/normalise-release.py` pins the
  zip entry times and the SBOM's serial number and timestamp to the
  tagged commit, and `scripts/reproduce-release.sh` rebuilds a release
  and compares it byte for byte (`docs/REPRODUCING.md`).
- `SPDX-FileCopyrightText` beside the licence identifier in every
  source file.

### Changed

- The roadmap looks a year ahead and lists the non-goals.
- The README and the getting-started page no longer say the package is
  not yet on NuGet; the ApiCompat check now compares with the
  published 0.0.40.

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
