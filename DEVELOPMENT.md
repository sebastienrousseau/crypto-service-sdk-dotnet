<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->

# Development

The single entry point for working on `crypto-service-sdk-dotnet`: the
toolchain, every CI gate reproduced locally, the test layout and the
release model. [CONTRIBUTING.md](CONTRIBUTING.md) covers how to send a
change; [AGENTS.md](AGENTS.md) lists the rules that must not drift.

## Toolchain

| Tool | Version | Used for |
| :--- | :--- | :--- |
| .NET SDK | 10.0.404 (`global.json`, `mise.toml`) | Build, tests, coverage, fuzzing, examples, benchmarks, pack |
| Node.js and pnpm | 24 and 11.9.0 | Building and running crypto-server; markdownlint |
| Python | 3.12 | The manual (MkDocs), codespell, REUSE, the Halstead check |
| vhs | any recent | Rendering `.github/demo.gif` (`make demo`) |

`mise install` gives the pinned .NET SDK and Node. Every NuGet package
is pinned exactly in `Directory.Packages.props` and locked with content
hashes in each project's `packages.lock.json`; CI restores in locked
mode. After a package change, regenerate the lock files with
`dotnet restore --force-evaluate` (never edit them by hand) and say so
in the commit body. Local tools (`cyclonedx`, `sharpfuzz`) are pinned in
`.config/dotnet-tools.json`.

## A server

The conformance kit, the examples, the README code blocks and the
loopback benchmark need crypto-server at the commit in
[`ci/server.ref`](ci/server.ref). Build it once from a clone of
[crypto-service](https://github.com/sebastienrousseau/crypto-service)
checked out at that commit (`pnpm install --frozen-lockfile &&
pnpm -r run build`), then start it from `packages/crypto-server` with
synthetic credentials made up for the run:

| Variable | Example value | Why |
| :--- | :--- | :--- |
| `PORT`, `HOST` | `3000`, `127.0.0.1` | Loopback only |
| `CRYPTO_API_KEY` | 32 random characters | The examples and README blocks use it |
| `JWT_SECRET` | 32 bytes or more | The kit runs each file as its own principal with minted tokens |
| `RATE_LIMIT_MAX` | `100000` | The default is 10 requests a minute per address |
| `NODE_ENV` | `development` | No issuer or audience pinning needed |

Run `node dist/index.js` with those set, then export
`CRYPTO_SERVICE_URL`, `CRYPTO_SERVICE_API_KEY` (the same key) and
`CRYPTO_SERVICE_JWT_SECRET` (the same secret). The server also meters
60 requests a minute per principal; the kit's files each stay under it,
and the examples retry a 429 within its short `Retry-After`.
`make regression` does all of this itself.

## Every CI gate, locally

| Gate | Local command | CI |
| :--- | :--- | :--- |
| Format | `make format` (`dotnet format --verify-no-changes`) | `ci.yml` Format |
| Build: warnings as errors, `latest-recommended` analysers, complexity | `make build` | `ci.yml` Build and unit tests (Linux, Windows, macOS) |
| Unit tests with 100 % line and branch coverage | `make coverage` | `ci.yml` Build and unit tests |
| Vulnerable packages | `make audit` | `ci.yml` Vulnerable packages |
| Version references agree | `make verify-versions` | `ci.yml` Release versions agree |
| API compatibility (ApiCompat) | `scripts/api-compat.sh` | `ci.yml` Build and unit tests (Linux) |
| Halstead difficulty ≤ 30 | `make halstead` | `ci.yml` Complexity |
| Conformance kit against a server | `make conformance` | `ci.yml` Conformance |
| Every example against a server | `make examples` | `ci.yml` Conformance |
| Every README code block against a server | `make readme` | `ci.yml` Conformance |
| Benchmark smoke run | `make bench-smoke` | `ci.yml` Conformance |
| Fuzzing, 60 s per target | `make fuzz` | `fuzz.yml` |
| The manual, strict, with the API reference | `make docs` | `docs.yml` |
| markdownlint, codespell, REUSE | `make docs-lint` | `docs-lint.yml` |
| Signed-off-by on every commit | `git log --format=%B` | `dco.yml` |
| CodeQL, OpenSSF Scorecard | n/a (GitHub only) | `codeql.yml`, `scorecard.yml` |

`make check` runs format, build, coverage, the audit, the version check
and the Halstead check: everything that needs no server.

### Complexity ceilings

| Metric | Ceiling | Enforced by |
| :--- | :--- | :--- |
| Cyclomatic complexity | 10 | CA1502 (`CodeMetricsConfig.txt`) |
| Cognitive complexity | 15 | SonarAnalyzer S3776 (`SonarLint.xml`) |
| Lines per method | 60 | SonarAnalyzer S138 |
| Lines per file | 500 | SonarAnalyzer S104 |
| Halstead difficulty | 30 | `scripts/halstead.py` |

They hold in the library, tests, fuzz harness, examples and benchmarks.
No method is over a ceiling, so there is no baseline.

### Coverage

`make coverage` runs the unit tests under coverlet and fails below
100 % line or branch coverage of `CryptoService.Sdk`. The report is
`artifacts/coverage/coverage.cobertura.xml`. The one exclusion is code
marked `[GeneratedCode]`: the System.Text.Json contract and the route
regex the compiler's source generators write. There is no other.

### Fuzzing

`fuzz/CryptoService.Sdk.Fuzz` is a SharpFuzz harness with two targets:
`problem` (the problem-body parser must never throw) and `success` (the
2xx decoder over seven contracts may fail only with
`CryptoTransportException`). `scripts/fuzz.sh <target> <seconds>`
instruments `CryptoService.Sdk.dll` and runs the target under the
pinned libfuzzer-dotnet driver (Linux x86-64) from `fuzz/corpus/`. Only
the SDK assembly is instrumented, so coverage feedback stops at
`System.Text.Json`; FsCheck properties (`PropertyTests`) drive the same
parsers on every platform.

## Test layout

| Path | What it covers | Needs a server |
| :--- | :--- | :--- |
| `tests/CryptoService.Sdk.Tests/OperationTests.cs` | Every public method: route, credentials, body, decoding | No |
| `ClientTests.cs` | Options, headers, `traceparent`, raw requests, redirects, disposal | No |
| `RetryTests.cs` | Retries, `Retry-After`, timeouts, the deadline, cancellation | No |
| `ErrorTests.cs`, `PropertyTests.cs` | The error model and the retry-safety table; FsCheck properties | No |
| `KitTests.cs` | The kit against `SHA256SUMS`, `ci/server.ref` | No |
| `ConformanceTests.cs` | Every vector of every kit file, in order | Yes |

The conformance tests skip unless `CRYPTO_SERVICE_URL` is set;
`make conformance` sets `CRYPTO_SERVICE_CONFORMANCE=required`, and then
a missing URL, key or JWT secret fails the run.

## Regression run

`make regression` (`scripts/regression.sh`) clones crypto-service at
`ci/server.ref` (or uses `CRYPTO_SERVICE_CHECKOUT`), builds it, starts
it on a free loopback port with credentials made up for the run, runs
`make check`, the kit, every example, every README block and the
benchmark smoke run, and stops the server on exit, failure included.

## The manual

`make docs` builds the MkDocs Material manual from `docs/` in strict
mode, after `scripts/api-docs.sh` renders the API reference from the
library's XML documentation comments into `docs/api/` (generated, not
committed). `docs.yml` deploys it to GitHub Pages from `main`.

## Release model

The version is `0.0.x` and moves by `0.0.1` on `feat/v<next-version>`,
the only pull request into `main`. A release:

1. Date the `## [0.0.N] - Unreleased` heading in `CHANGELOG.md`, rewrite
   `.github/release-highlights.md`, and run
   `scripts/verify-release-versions 0.0.N`.
2. Run the release workflow by hand with `dry-run` on (the default) and
   read the notes, SBOM and checksums it uploads.
3. After the release PR is merged, the maintainer pushes a signed,
   annotated tag `v0.0.N` whose message is
   `crypto-service-sdk-dotnet v0.0.N`.
4. `release.yml` verifies the tag against `KEYS.asc`, builds, tests,
   packs, writes the CycloneDX SBOM (`.cdx.json`) and `SHA256SUMS`,
   attests build provenance, creates the GitHub release titled
   `crypto-service-sdk-dotnet 0.0.N`, checks its sections, and
   publishes to NuGet.

### Settings and secrets the maintainer creates

None of these exist yet; the workflows are ready for them.

| What | Where | Why |
| :--- | :--- | :--- |
| `KEYS.asc` | Repository root, signed commit | The allowed SSH signers the tag is verified against |
| Environment `release`, with required reviewer and tag rule `v*` | Repository settings, Environments | Gates the GitHub release and NuGet jobs |
| Secret `NUGET_USER` in `release` | Environment secrets | The nuget.org account name `NuGet/login` exchanges the OIDC token for |
| Trusted publishing policy for `CryptoService.Sdk` | nuget.org, account, Trusted Publishing | Repository `sebastienrousseau/crypto-service-sdk-dotnet`, workflow `release.yml`, environment `release` |
| Pages source "GitHub Actions" | Repository settings, Pages | `docs.yml` deploys the manual |
