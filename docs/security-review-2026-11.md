<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->
<!-- SPDX-FileCopyrightText: 2026 Sebastien Rousseau -->

# Security review, November 2026

| | |
| :--- | :--- |
| Date | 2026-11-18 |
| Reviewer | Sebastien Rousseau, with AI-assisted analysis (Claude) |
| Subject | `crypto-service-sdk-dotnet` at `origin/main` 79483a4 (0.0.40 plus a test-dependency bump), and the fixes on `feat/v0.0.41` |
| Measured against | The [assurance case](ASSURANCE.md) |
| Previous review | None: this is the first |

## Scope

- **Library**: every file in `src/CryptoService.Sdk/`: the transport
  (request building, credentials, timeouts, deadline, retries, redirect
  refusal), the options and their descriptions, the decoders, the error
  model, the source-generated JSON contract and the models.
- **Transport and TLS**: the handler the client creates, and what holds
  when the caller injects a handler or an `HttpClient`.
- **Credential handling**: where the API key and token go, and every
  place a value could be printed.
- **Input parsing**: 2xx bodies, error bodies, `Retry-After`, redirects.
- **CI and release**: every workflow in `.github/workflows/`, their
  triggers and permissions, the scripts they run, the release pipeline
  from tag to NuGet.
- **Dependencies**: `Directory.Packages.props`, the lock files, the
  local tools in `.config/dotnet-tools.json`, Dependabot.

Out of scope: crypto-server itself (reviewed in crypto-service), the
GitHub and nuget.org platforms, and the repository settings beyond what
the API shows (the `release` environment's rules were read).

## Method

1. A line-by-line read of the library, the workflows and the release
   scripts, against each threat and boundary of the assurance case.
2. Dynamic checks of suspected issues, each reproduced before any fix:
   - a raw loopback socket capturing what `HttpClient` sends for a
     header value holding CR LF, added with `TryAddWithoutValidation`
     as `Transport.BuildRequest` does;
   - how `Uri` composes a base URL with user info, a query or a
     fragment with a route, as `Transport.CheckBaseUrl` and
     `BuildRequest` do;
   - two clean rebuilds of tag `v0.0.40` compared with the released
     assets ([REPRODUCING.md](REPRODUCING.md)).
3. Tools: `make check` (format, analysers with warnings as errors,
   complexity ceilings, unit tests at 100 % line and branch coverage,
   `dotnet list package --vulnerable`, version check, Halstead),
   `scripts/api-compat.sh` against the published 0.0.40, `actionlint`
   and `shellcheck`, and the open code-scanning, Dependabot and
   secret-scanning alerts (CodeQL, OpenSSF Scorecard).
4. Each fix was written as a test first, seen to fail, then made to
   pass, in the same commit as the fix.

## Findings

| ID | Severity | Finding | Disposition |
| :--- | :--- | :--- | :--- |
| SR-1 | Low | Header injection (CWE-113) | Fixed |
| SR-2 | Low | Base URL user info printed; query or fragment swallowed the route (CWE-532, CWE-20) | Fixed |
| SR-3 | Low | A publishing release run was not bound to its tag | Fixed |
| SR-4 | Low | The release could build with an SDK other than the pinned one | Fixed |
| SR-5 | Informational | Release archives and SBOM were not byte-reproducible | Fixed |
| SR-6 | Low | 2xx and raw bodies are read without a size cap (CWE-400) | Accepted |
| SR-7 | Informational | Plain `http` is accepted for any host (CWE-319) | Accepted |
| SR-8 | Informational | `Get*Bytes()` accessors throw an undocumented `FormatException` | Accepted, documentation task |
| SR-9 | Informational | Secrets held in managed strings (CWE-316) | Accepted |
| SR-10 | Informational | Open Scorecard alert `CIIBestPracticesID` | Accepted, resolves with the badge |

### SR-1: header injection through options and the raw media type

`Transport.BuildRequest` adds the API key, the `traceparent` and the
raw request's media type with `TryAddWithoutValidation`. The loopback
capture showed that `HttpClient` (.NET 10.0.404) then writes a CR LF in
the value to the wire as is, so a value such as
`key\r\nx-injected: 1` sends an extra header on every request. The
values come from the caller, so exploitation needs an application that
copies untrusted text into them, for example a `traceparent` taken
from an inbound message. The bearer token, set through
`AuthenticationHeaderValue`, was refused by the same check for
consistency; its own wire behaviour was not captured.

Fix: `Transport.CheckHeaderValue` refuses CR, LF and NUL in the API
key, the token and the `traceparent` at construction, and in the media
type of `RequestAsync`, with a message that never repeats the value.
Tests: `AHeaderValueWithAControlCharacterIsRefused`,
`RequestAsyncChecksItsArguments`.

### SR-2: base URL with user info, a query or a fragment

`CheckBaseUrl` accepted any absolute URI. With user info
(`https://user:secret@host`), `CryptoServiceClient.ToString()` and
`CryptoServiceClientOptions.ToString()` printed the password, against
the rule that no description shows a credential. With a query or a
fragment, `BuildRequest` appended the route after it, so every call
went to the root path of the server with the route in the query.

Fix: the client refuses a base URL with user info, a query or a
fragment; the options' description replaces user info with
`(user info redacted)`. Tests:
`ABaseUrlWithUserInfoAQueryOrAFragmentIsRefused`,
`TheOptionsNeverShowTheBaseUrlsUserInfo`. This is a behaviour change
for a caller who passed such a URL; none could have worked as
intended.

### SR-3: a publishing release run not bound to its tag

`release.yml` can be dispatched by hand with `dry-run` off. It then
verified that the tag `v<version>` was signed, but built whatever ref
was dispatched, and the `attest` job, which has no environment, would
attest that build. The `release` environment admits only `v*` tags, so
neither the GitHub release nor NuGet could be reached from a branch;
the exposure was a provenance attestation for a build of another ref.

Fix: a publishing run fails unless `GITHUB_REF` is `refs/tags/v<version>`.
Verified by running the guard's shell with both kinds of ref, and with
`actionlint`; it runs for real at the next tag.

### SR-4: the release SDK could drift with the runner image

`global.json` pins .NET SDK 10.0.404 with `rollForward: latestFeature`,
so a runner image with a newer SDK of the band would build the release
with it. The 0.0.40 release run used 10.0.404 (its log says so), but
nothing enforced it.

Fix: the release build fails unless `dotnet --version` is exactly the
pinned SDK. A runner image upgrade will then stop a release until
`global.json` and `mise.toml` are moved on purpose. Development keeps
the roll-forward.

### SR-5: release assets not byte-reproducible

Every file inside the 0.0.40 packages rebuilds bit for bit, but the
archives carried the pack time in their zip entries and the SBOM a
random serial number and the current time, so a third party could not
match `SHA256SUMS`. `DeterministicTimestamp=true` and
`SOURCE_DATE_EPOCH` were tried and do not change the entry times with
this SDK.

Fix: `scripts/normalise-release.py` pins both to the tagged commit's
time in the release workflow, and `scripts/reproduce-release.sh`
rebuilds a release and compares it; for 0.0.40 all three assets match
after normalisation ([REPRODUCING.md](REPRODUCING.md)).

### SR-6: 2xx and raw bodies have no size cap

Error bodies are capped at 64 KiB, but `Decoding.ReadSuccessAsync`
streams a 2xx body into the result without a limit, and
`SendRawAsync` reads a raw body whole. A hostile or broken server could
make the client allocate until the per-attempt timeout (30 s by
default) ends the call.

Accepted: the server is the party the caller chose to trust with its
keys, and results such as batch signatures have no natural size limit,
so a fixed cap would break legitimate calls. A caller who needs a cap
can inject a handler that enforces one. Revisit if users ask for a
configurable limit.

### SR-7: plain `http` to any host

The client sends credentials over `http` if the base URL says so.
Accepted: loopback servers, CI and test containers use `http`, and the
`SECURITY.md` posture tells users to use `https` for any server that is
not on loopback. Refusing `http` for non-loopback hosts would be a
breaking change for the maintainer to decide, not a fix.

### SR-8: `Get*Bytes()` accessors throw `FormatException`

The 17 accessors that decode a hex or base64 field of a result
(`Convert.FromHexString`, `Convert.FromBase64String`) throw
`FormatException` if the server sent a malformed value. The call itself has succeeded by then; the exception is
not a security issue, but it is not documented in the XML comments.
Accepted, recorded as a newcomer task.

### SR-9: secrets in managed strings

The API key, the token and request and result values are .NET strings
for their lifetime; they cannot be reliably wiped. Accepted: inherent
to any .NET HTTP client, listed as a residual risk in the assurance
case.

### SR-10: Scorecard `CIIBestPracticesID`

The one open code-scanning alert is Scorecard's check for an OpenSSF
Best Practices badge. The project has a passing entry; the alert
clears when Scorecard next reads it. No code change.

## What was checked and found sound

- Credentials go only in `x-api-key` and `Authorization: Bearer`, only
  on authenticated calls (not the `/health`, `/live`, `/ready` probes),
  only to the configured base URL; redirects are refused by the
  default handler and detected after an injected one followed them.
- No code path sets a certificate callback or offers to; no
  `unsafe` code, no native interop, no environment variable read, no
  file written.
- Exception messages carry the status, the problem type and the title
  only; models have no `ToString()` that prints a value.
- The retry-safety table never repeats a stateful call after 504, a
  timeout or a lost connection; the deadline is honoured during waits.
- The JSON contract is source-generated into fixed types, with no
  polymorphic type handling; both decoders are fuzzed weekly and on
  every change.
- Every workflow defaults to `contents: read`, grants write scopes per
  job, pins every action by commit SHA and checks out without
  persisting credentials. `ci/server.ref` from a pull request is
  checked to be a 40-character SHA before use. No workflow runs on
  `pull_request_target`.
- No runtime dependency; development packages pinned and locked with
  content hashes; no vulnerable package reported; no open Dependabot
  or secret-scanning alert.

## Next review

Within a year, or with any change to the transport, the options, the
decoders or the release workflow, whichever comes first, recorded
beside this one.
