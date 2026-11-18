<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->
<!-- SPDX-FileCopyrightText: 2026 Sebastien Rousseau -->

# Roadmap

The SDK follows the Crypto Service suite: what the server learns, the
SDK carries in the same release. This page lists what is planned for
the client itself. It is a plan, not a promise; the
[CHANGELOG](https://github.com/sebastienrousseau/crypto-service-sdk-dotnet/blob/main/CHANGELOG.md)
records what shipped.

## 0.0.41 (in preparation)

## 0.0.40 (released 2026-11-16)

- First NuGet release of `CryptoService.Sdk`, by trusted publishing.
- Every v2 operation (59), the probes and the compliance documents.
- The suite's resilience contract and error model.
- The conformance kit in CI through the public methods, 100 % line and
  branch coverage, fuzzing, benchmarks, examples, the manual and the
  release workflow.

## Next

- Publish benchmark results in `BENCHMARKS.md`, generated from a full
  run.
- Run the ApiCompat check against the published 0.0.40 package from the
  next release on.
- Methods for operations crypto-service adds, in the release that adds
  them; until then `RequestAsync` reaches any route.

## Not planned

- Cryptography in the client
  ([ADR 0001](adr/0001-thin-client-over-the-server.md)).
- The v1 routes and `/metrics`.
- A blocking API ([ADR 0004](adr/0004-async-model.md)).
