<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->
<!-- SPDX-FileCopyrightText: 2026 Sebastien Rousseau -->

# Roadmap

The SDK follows the Crypto Service suite: what the server learns, the
SDK carries in the same release. This page lists what is planned for
the client itself over the next year, to November 2027, and what is
not planned at all. It is a plan, not a promise, and it is revised with
every release; the
[CHANGELOG](https://github.com/sebastienrousseau/crypto-service-sdk-dotnet/blob/main/CHANGELOG.md)
records what shipped.

## Shipped

- **0.0.40** (2026-11-16): the first NuGet release of
  `CryptoService.Sdk`, by trusted publishing, with every v2 operation
  (59), the probes and the compliance documents, the suite's
  resilience contract and error model, the conformance kit in CI,
  100 % line and branch coverage, fuzzing, benchmarks, examples, the
  manual and the release workflow.

## 0.0.41 (in preparation)

- The assurance case ([ASSURANCE.md](ASSURANCE.md)) and the first
  recorded security review
  ([security-review-2026-11.md](security-review-2026-11.md)), with its
  fixes: the client refuses header values with a line break and base
  URLs with user info, a query or a fragment.
- The vulnerability response process in `SECURITY.md`.
- A documented rebuild of the released package
  ([REPRODUCING.md](REPRODUCING.md)) and an SBOM whose serial number
  and timestamp derive from the release, so it rebuilds byte for byte.
- The ApiCompat check runs against the published 0.0.40 package.

## The next twelve months

In rough order; each item lands only if it serves users of the suite
and keeps the client thin.

1. **Lockstep with crypto-server**: a method for each operation the
   server adds, in the release that adds it; until then `RequestAsync`
   reaches any route.
2. **Published benchmark results**: generate `BENCHMARKS.md` from a
   full `make bench` run with its environment recorded, comparable
   across the suite's SDKs.
3. **The v1 sunset**: the server retires its deprecated v1 routes on
   2027-01-01 ([Policies](POLICIES.md#api-surface)). The SDK never
   wrapped them; its docs and examples are checked for any reference
   left.
4. **.NET releases**: `net10.0` (LTS, supported to November 2028)
   stays the floor for the year; the next .NET release joins the CI
   test matrix once it is generally available, without moving the
   floor ([Policies](POLICIES.md#minimum-toolchain)).
5. **OpenSSF Best Practices silver, then gold**: the criteria left need
   a second maintainer (bus factor, two-person review, access
   continuity), which is a people question, not a code one.
6. **A security review each year**, and after any change to the
   transport, the credentials or the decoders, recorded beside the
   first one.

## Not planned

These are deliberate non-goals, not gaps:

- **Cryptography in the client**, including an offline or fallback
  mode ([ADR 0001](adr/0001-thin-client-over-the-server.md)).
- **The v1 routes and `/metrics`** ([Policies](POLICIES.md#api-surface)).
- **A blocking API** ([ADR 0004](adr/0004-async-model.md)).
- **A third-party runtime dependency** (a retry library, another JSON
  serialiser, a dependency-injection package): `HttpClient` and
  source-generated `System.Text.Json` only.
- **An option to follow redirects or to turn TLS validation off.**
- **Target frameworks below the floor** (`netstandard2.0`, .NET
  Framework).
- **Logging or telemetry** of any kind from the SDK.
