<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->

# AGENTS.md

`crypto-service-sdk-dotnet` is the typed .NET client for the Crypto
Service REST API (crypto-server v2). The server is the one engine; this
SDK is a thin client that every vector of the shared conformance kit
proves. These are the rules an automated change is most likely to
break; humans follow them too. Read [CONTRIBUTING.md](CONTRIBUTING.md)
first.

## Versioning

- The version is `0.0.x` and moves strictly by `0.0.1`, in lockstep
  with the Crypto Service suite. Never propose a `0.1.0` or `1.0` jump,
  and never bump it as a side effect of a change.
- `<Version>` in `src/CryptoService.Sdk/CryptoService.Sdk.csproj` is
  the single source of truth; `CITATION.cff`, the `CHANGELOG.md`
  heading, the `SECURITY.md` and `SUPPORT.md` tables, the README install
  snippets and the bug report form follow it, and
  `scripts/verify-release-versions` checks them. The `User-Agent` is
  read from the built assembly.
- Work for the next version happens on `feat/v<next-version>`, the only
  pull request into `main`; every other branch merges into it.

## The gate

`make check` runs `dotnet format --verify-no-changes`, the Release build
(warnings as errors, `latest-recommended` analysers, CA1502 and
SonarAnalyzer complexity ceilings), the unit tests under coverlet with
100 % line and branch coverage, the vulnerable-package audit, the
version check and the Halstead check. CI adds the conformance kit,
every example and every README code block against a live server, and a
60-second fuzz run per target. "It compiles" is not done: say which
command ran and what it printed.

## Rules

- **Thin client, no cryptography.** Every operation is an HTTP call to
  crypto-server; never implement or emulate a primitive locally
  ([ADR 0001](docs/adr/0001-thin-client-over-the-server.md)).
- **Never log or expose secrets.** No credential, key, plaintext or
  body in an exception message, a `ToString()` or debug output. The SDK
  logs nothing.
- **Keep the resilience contract.** The retry-safety table in
  `Internal/Idempotency.cs` is the suite's; it changes in crypto-service
  first. Redirects stay refused, TLS validation stays on, with no
  option to turn either off.
- **No third-party runtime dependency**: `HttpClient` and source-
  generated `System.Text.Json` only. Test, fuzz and benchmark packages
  are pinned exactly in `Directory.Packages.props`.
- Every behaviour change lands with its test in the same commit;
  coverage never drops below 100 %.
- Every source file opens with the SPDX header `Apache-2.0 OR MIT`;
  Markdown files carry it on line 1.
- Synthetic keys and credentials only, in tests, examples and docs.

## Leave alone

- `conformance/` is vendored from crypto-service at the commit in
  `ci/server.ref`, byte for byte, and pinned by `conformance/SHA256SUMS`.
  Never edit it here: change it in crypto-service, then copy it in and
  update `ci/server.ref` in the same commit.
- Generated output (`bin/`, `obj/`, `artifacts/`, `docs/api/`, `site/`)
  and the `packages.lock.json` files, which only `dotnet restore`
  writes.
