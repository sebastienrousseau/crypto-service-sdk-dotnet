<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->
<!-- SPDX-FileCopyrightText: 2026 Sebastien Rousseau -->

# Assurance case

This page argues that `CryptoService.Sdk` meets its security
requirements: what it protects, from whom, where the trust boundaries
are, how the secure design principles are applied and how the common
weaknesses of an HTTP client library are countered. Every claim names
the code or the test that makes it true; paths are relative to the
repository root and tests are in `tests/CryptoService.Sdk.Tests/`
unless stated. The claim is reviewed with each security review (the
latest is [security-review-2026-11.md](security-review-2026-11.md)).

## The claim

Used as documented, the SDK sends each call only to the base URL its
caller configured, with the caller's credentials and nothing else; it
never discloses a credential, key or plaintext through its own output;
it turns any answer, however malformed, into a typed result or a typed
exception within a bounded time; and it never repeats a call that may
have changed state on the server. It performs no cryptography, so it
cannot weaken one ([ADR 0001](adr/0001-thin-client-over-the-server.md)).

## Threat model

### Assets

| Asset | Where it is in the SDK |
| :--- | :--- |
| The API key and the bearer token | `CryptoServiceClientOptions.ApiKey` and `Token`, copied into `Internal/Transport.cs` at construction |
| Plaintexts, passwords, exported and unwrapped keys, shared secrets | Request and result models in `src/CryptoService.Sdk/Models/`, only for the duration of a call |
| The integrity of state on the server (keys, KMS keys, OPAQUE registrations) | Calls that create or delete it, listed in `Internal/Idempotency.cs` |
| The integrity of the released package | The NuGet package, its symbols, SBOM and checksums, built by `.github/workflows/release.yml` |

### Actors

| Actor | Trusted for | Not trusted for |
| :--- | :--- | :--- |
| The calling application | Its options, its requests, the `HttpMessageHandler` or `HttpClient` it injects | Nothing the SDK can check beyond argument validity |
| crypto-server at the base URL | The results of the operations it performs | The shape, size or content of what it sends back |
| The network between them | Nothing | Anything: an attacker may read, drop, delay or rewrite traffic not protected by TLS |
| A redirect target, a proxy, a load balancer | Nothing | Receiving credentials |
| Package registries (nuget.org) and GitHub | Hosting what the release workflow published | Altering it unnoticed: checksums, provenance and signatures make that detectable |
| A contributor, a dependency, a CI action | Nothing by default | Merging, publishing or running with write permissions unreviewed |

### Entry points

- The public constructors and `CryptoServiceClientOptions`
  (`CryptoServiceClient.cs`, `CryptoServiceClientOptions.cs`,
  `RetryOptions.cs`).
- The typed operation methods (`CryptoServiceClient.*.cs`) and the
  untyped `RequestAsync`.
- Every byte of every answer: status, headers (`Retry-After`,
  `Content-Type`, `Location` when a handler followed a redirect) and
  body (`Internal/Decoding.cs`).
- The build and release pipeline: pull requests, dependencies, the
  pinned toolchain and the workflows in `.github/workflows/`.

## Trust boundaries

```text
 caller code ──(1)── CryptoService.Sdk ──(2)── HttpClient/handler ──(3)── network ──(4)── crypto-server
                                                                                          │
 maintainer ──(5)── GitHub (CI, signed tags, release env) ──(6)── nuget.org ──(7)── consumers
```

1. **Caller to SDK.** Options are validated and copied once, at
   construction (`Transport` constructor): an absolute base URL with no
   user info, query or fragment; positive timeouts; credentials and
   `traceparent` without a line break or NUL. Request objects are
   checked for null before serialization (`ANullRequestIsRefused`).
2. **SDK to its HTTP stack.** The SDK builds every request itself
   (`Transport.BuildRequest`) and owns the handler it creates
   (`CryptoServiceClient.CreateHandler`: no redirects, no cookies, no
   automatic decompression). An injected handler or `HttpClient` is the
   caller's; the SDK still refuses an answer whose request URI changed
   (`Transport.SendCheckedAsync`, `ARedirectAnInjectedClientFollowedIsRefused`).
3. **Network.** Confidentiality and integrity come from TLS with the
   platform's certificate validation; the SDK has no option to weaken
   it. Plain `http` is accepted for loopback and test servers, and
   `SECURITY.md` tells users to use `https` elsewhere.
4. **crypto-server's answers.** Every body is untrusted input: a 2xx
   body is decoded by the source-generated contract
   (`Internal/CryptoJsonContext.cs`, `RespectNullableAnnotations`,
   `required` members) and any failure becomes `CryptoTransportException`;
   an error body is read up to 64 KiB (`Decoding.MaxErrorBodyBytes`) and
   anything that is not a problem becomes `about:blank`. Both decoders
   are fuzzed (`fuzz/CryptoService.Sdk.Fuzz`) and property-tested
   (`PropertyTests`).
5. **Maintainer to GitHub.** Commits carry a DCO sign-off (`dco.yml`)
   and are signed; release tags are SSH-signed and verified against
   `KEYS.asc` before a release is built (`release.yml`).
6. **GitHub to nuget.org.** Publication is by trusted publishing (OIDC)
   from the `release` environment, which only `v*` tags can use and
   which needs the maintainer's approval; no long-lived NuGet key
   exists ([Packaging](packaging.md)).
7. **Registry to consumers.** Each release carries `SHA256SUMS`, a
   CycloneDX SBOM and a build provenance attestation, and rebuilds
   byte for byte from its tag ([REPRODUCING.md](REPRODUCING.md)).

## Secure design principles

The principles of Saltzer and Schroeder, and how the code applies
them:

| Principle | How it is applied |
| :--- | :--- |
| **Economy of mechanism** | The SDK is a thin client: one transport (`Internal/Transport.cs`, under 300 lines), one decoder (`Internal/Decoding.cs`), one source-generated JSON contract and no cryptography. No third-party runtime dependency (`src/CryptoService.Sdk/CryptoService.Sdk.csproj` references none). Complexity ceilings (cyclomatic 10, cognitive 15, Halstead 30) are enforced by the build and `scripts/halstead.py`. |
| **Fail-safe defaults** | Retries are off by default (`NoRetryByDefault`); redirects are refused (`TheDefaultHandlerRefusesRedirectsAndPoolsConnections`); every attempt has a 30-second timeout; TLS validation cannot be turned off; an answer the contract cannot decode is an error, never a partial result (`A2xxAnswerThatBreaksTheContractIsATransportError`); options that would build a wrong URL or header are refused at construction (`ABaseUrlWithUserInfoAQueryOrAFragmentIsRefused`, `AHeaderValueWithAControlCharacterIsRefused`). |
| **Complete mediation** | Every request, typed or raw, goes through `Transport.BuildRequest` and `Transport.SendCheckedAsync`, so the credential, redirect and timeout rules hold for all 59 operations and `RequestAsync` alike (`SendsTheRouteAndDecodesTheAnswer` runs every operation through them). Credentials are added only to calls that need them: the `/health`, `/live` and `/ready` probes are sent without (`CryptoServiceClient.Meta.cs`). |
| **Open design** | The whole client, its threat model and this argument are public; its security rests on TLS and the server, not on secrecy of the code. |
| **Separation of privilege** | Publishing needs a signed tag that verifies against `KEYS.asc`, a `v*` tag ref, and an approval in the `release` environment; the build job that runs the tests holds no publishing permission (`release.yml`). |
| **Least privilege** | Every workflow defaults to `contents: read`; write scopes (`id-token`, `attestations`, `contents: write`, `security-events`, `pages`) are granted per job, only where needed. Checkouts use `persist-credentials: false`. The SDK asks for no permission of its host: it writes no file, reads no environment variable and runs no code from the server. |
| **Least common mechanism** | A client's state is fixed at construction and shared only through the immutable `Transport`; each call has its own cancellation sources and request (`ManyConcurrentCallsShareOneClient`). |
| **Psychological acceptability** | The secure path is the default path: `new CryptoServiceClient(options)` gets the hardened handler; credentials print as `set (redacted)` (`NoDescriptionShowsACredential`), so logging the options or the client is safe. |

## Common weaknesses

The weaknesses of the CWE Top 25 and the OWASP Top 10 that apply to an
HTTP client library, and what counters each:

| Weakness | Counter in the SDK | Evidence |
| :--- | :--- | :--- |
| CWE-20 Improper input validation | Options checked at construction; request objects checked for null; the method and path of `RequestAsync` checked | `TheOptionsAreChecked`, `ANullRequestIsRefused`, `RequestAsyncChecksItsArguments` |
| CWE-113 / CWE-93 Header injection (CRLF) | API key, token, `traceparent` and the raw media type are refused if they hold CR, LF or NUL, because `HttpClient` sends values added without validation as they are | `Transport.CheckHeaderValue`; `AHeaderValueWithAControlCharacterIsRefused`, `RequestAsyncChecksItsArguments` |
| CWE-200 / CWE-532 Exposure of sensitive information, including in logs | The SDK logs nothing; exception messages name the status, problem type and title only, and the server's `detail` stays in `Problem`; options and the client describe credentials as set or unset and redact user info; models are classes whose `ToString()` is the type name | `CryptoApiException.Describe`, `CryptoServiceClientOptions.ToString`; `NoDescriptionShowsACredential`, `TheOptionsNeverShowTheBaseUrlsUserInfo` |
| CWE-295 Improper certificate validation | The SDK never sets a certificate callback and has no option to | `CryptoServiceClient.CreateHandler`; [ADR 0002](adr/0002-http-stack.md) |
| CWE-319 Cleartext transmission of sensitive information | TLS is the transport for any non-loopback server; `http` remains possible for loopback and tests, documented in `SECURITY.md` | Residual risk, see below |
| CWE-601 Open redirect / OWASP A10 SSRF-style credential forwarding | Redirects refused by the default handler, and detected and refused when an injected handler follows one, so a credential never goes to another origin | `ARedirectAnswerIsAnErrorNotFollowed`, `ARedirectAnInjectedClientFollowedIsRefused` |
| CWE-400 / CWE-770 Uncontrolled resource consumption | Per-attempt timeout and optional call deadline; error bodies capped at 64 KiB; no automatic decompression; retries bounded by `MaxRetries` and `MaxDelay`, a `Retry-After` beyond `MaxDelay` ends the call | `AnOversizedErrorBodyIsNotRead`, `TheDeadlineEndsTheCallAndIsNotRetried`, `ARetryAfterLongerThanMaxDelayEndsTheCall` |
| CWE-502 Deserialization of untrusted data | Source-generated `System.Text.Json` into fixed types: no polymorphic type handling, no reflection, `required` and nullability enforced | `Internal/CryptoJsonContext.cs`; fuzz target `success`; `TheSynchronousDecoderMatchesTheStreamingOne` |
| CWE-754 / CWE-755 Improper handling of exceptional conditions | Whatever the server sends, a call ends in a typed result, `CryptoApiException` or `CryptoTransportException` (or `OperationCanceledException` for the caller's own token); the problem parser never throws. An exception an injected handler raises that is not a transport failure propagates unchanged rather than being disguised | Fuzz targets `problem` and `success`; `ABodyThatIsNotAProblemBecomesAboutBlank`, `AnUnexpectedHandlerFailurePropagatesUnwrapped` |
| CWE-362 Race condition | Immutable client; per-call state only | `ManyConcurrentCallsShareOneClient` |
| Non-idempotent replay (OWASP API: business-logic integrity) | A stateful call is retried only after 429 or 503, when the server did not act; never after 504, a timeout or a lost connection | `Internal/Idempotency.cs`; `AStatefulCallIsRetriedOnlyWhenTheServerDidNotAct`, `AStatefulCallIsNotRetriedAfterAConnectionFailure`, `TheRetrySafetyTableMatchesTheReferenceSdk` |
| CWE-798 Hard-coded credentials | None in the library; tests, examples and CI use synthetic values made up for the run | `AGENTS.md`; CI's `conformance` job |
| CWE-787 / CWE-125 / CWE-416 Memory safety | Managed code only, no `unsafe`, no native interop | `src/CryptoService.Sdk/` |
| OWASP A02 Cryptographic failures | No cryptography in the client; every primitive is the server's | [ADR 0001](adr/0001-thin-client-over-the-server.md) |
| OWASP A06 Vulnerable and outdated components / CWE-1395 | No runtime dependency; build, test and fuzz packages pinned exactly and locked with content hashes; `dotnet list package --vulnerable` fails CI; Dependabot watches NuGet and the actions | `Directory.Packages.props`, `packages.lock.json`, `ci.yml` audit job, `.github/dependabot.yml` |
| OWASP A08 Software and data integrity failures / CWE-494 | Actions pinned by commit SHA; signed tags verified against `KEYS.asc`; trusted publishing; checksums, SBOM and provenance; reproducible rebuild | `.github/workflows/release.yml`, [REPRODUCING.md](REPRODUCING.md) |
| OWASP A09 Security logging and monitoring failures | Not the SDK's to log; it gives callers what they need to log safely: status, problem type, `TimedOut`, and a `traceparent` to correlate with the server's logs | `CryptoApiException`, `CryptoTransportException.TimedOut`, `AGeneratedTraceparentIsFreshAndWellFormed` |

## Assumptions and residual risks

- **The server is the one the caller meant.** The SDK cannot tell a
  genuine crypto-server from an impostor at the configured URL; TLS
  with certificate validation is what establishes that. Over plain
  `http` to a non-loopback host, credentials and data are exposed to
  the network: documented, not prevented.
- **A 2xx body is decoded as it streams, without a size cap.** A
  hostile server could send a very large body; the per-attempt timeout
  bounds how long it can do so, not how much memory it uses. Results
  such as batch signatures have no natural size limit, so a cap would
  break legitimate use. Accepted in the
  [security review](security-review-2026-11.md#findings).
- **Secrets live in managed strings** for the life of the options and
  the client, and in request and result objects for a call; .NET
  offers no reliable way to wipe them. A process memory dump exposes
  them, as it would for any .NET HTTP client.
- **An injected `HttpClient` or handler is the caller's.** If it turns
  off certificate validation or adds headers, the SDK cannot see it;
  redirect refusal is the one rule it still enforces.

## Keeping the case true

The tests named above run on every change (`ci.yml`, 100 % line and
branch coverage), the fuzz targets run on every change and weekly
(`fuzz.yml`), and CodeQL and OpenSSF Scorecard run on the repository.
A change to the transport, the options, the decoders or the release
workflow updates this page in the same pull request.
