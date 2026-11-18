<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->
<!-- SPDX-FileCopyrightText: 2026 Sebastien Rousseau -->

# Security Policy

## Supported versions

This SDK follows the Crypto Service suite cadence. While pre-`1.0`, the
latest released `0.0.x` receives security fixes; an older `0.0.x` is
fixed by upgrading, since each step is `0.0.1`. `0.0.40` was the first
release.

| Version | Status | Receives security fixes? |
| :--- | :--- | :--- |
| `0.0.41` | Current (in preparation on `main`) | Yes |
| `0.0.40` | First release (2026-11-16), latest on NuGet | Yes, until 0.0.41 is out |

## Reporting a vulnerability

**Do not open a public issue for security reports.**

Report privately through GitHub Private Vulnerability Reporting:
<https://github.com/sebastienrousseau/crypto-service-sdk-dotnet/security/advisories/new>

Include the SDK version, the crypto-service commit or version, and the
smallest reproduction with synthetic keys. Never send a real key,
credential, token or plaintext. A vulnerability in the server itself
belongs to [crypto-service](https://github.com/sebastienrousseau/crypto-service/security).

## How a report is handled

The project has one maintainer, working in a personal capacity, with no
SLA and no bug bounty. The times below are goals, not guarantees.

1. **Acknowledgement.** The maintainer is notified of every private
   report and aims to acknowledge it in the advisory thread within 5
   working days.
2. **Triage.** The maintainer reproduces the report with synthetic keys,
   decides whether it is a vulnerability of this SDK, of crypto-server
   (then, with the reporter's agreement, it moves to
   [crypto-service](https://github.com/sebastienrousseau/crypto-service/security))
   or not a vulnerability, rates its severity with CVSS, and tells the
   reporter the outcome and the plan, aiming for 10 working days after
   acknowledgement.
3. **Fix and release.** The fix is prepared in the advisory's temporary
   private fork, with a test that fails without it, and ships as the
   next `0.0.x` through the usual signed release
   ([DEVELOPMENT.md](DEVELOPMENT.md#release-model)): NuGet package,
   SBOM, checksums and build provenance. The goal is a fixed release
   within 30 days of triage for a critical or high severity issue and
   within 90 days of the report for any other.
4. **Advisory and CVE.** When the fix is published, the maintainer
   publishes a GitHub security advisory on this repository, naming the
   affected and fixed versions of `CryptoService.Sdk`, and requests a
   CVE for it through GitHub, a CVE Numbering Authority. The advisory
   reaches the GitHub Advisory Database, which `dotnet list package
   --vulnerable` and Dependabot read. The `CHANGELOG.md` entry of the
   release links it.
5. **Coordinated disclosure.** Details stay private until the fixed
   release is out, or 90 days after the report if that comes first,
   unless the reporter and the maintainer agree on another date. An
   issue already public or exploited is fixed and disclosed as fast as
   possible.
6. **Credit.** The reporter is credited by name or handle in the
   advisory and the changelog, unless they ask to stay anonymous.

## Security posture

The assurance case, [docs/ASSURANCE.md](docs/ASSURANCE.md), states the
threat model, the trust boundaries, how the secure design principles
are applied and how common weaknesses are countered, with the code and
tests behind each claim. The latest security review is
[docs/security-review-2026-11.md](docs/security-review-2026-11.md).

### Scope

This package is an HTTP client for crypto-server. It sends JSON to a
server you configure and decodes the JSON that comes back. It performs
no cryptography, writes no files and runs no code from the server.

### How it handles risk

| Surface | How it's handled |
| :--- | :--- |
| **Secrets in logs** | The SDK logs nothing. No credential, key, plaintext or body appears in an exception message or a `ToString()`: options print credentials as redacted, an API error names its status, problem type and title only, and the server's `detail` stays in `CryptoApiException.Problem`. |
| **Credentials** | The API key goes only in `x-api-key`, a token only in `Authorization: Bearer`, to the configured base URL. Redirects are refused, so neither follows a `Location` to another origin. Use `https` for any server that is not on loopback. |
| **TLS** | Certificate validation is the platform's and cannot be turned off through the SDK: there is no option for it. |
| **Hostile or broken server** | 2xx bodies are decoded by a source-generated contract that rejects missing or null required members; an undecodable answer is a `CryptoTransportException`. Error bodies are read up to 64 KiB, and anything that is not a problem becomes `about:blank`. Both parsers are fuzzed. Every attempt has a timeout (30 s by default). |
| **Retries** | Off by default. When on, a stateful call (key generation or deletion, KMS key creation, OPAQUE registration) is resent only after 429 or 503, when the server did not act. |
| **Dependencies** | No third-party runtime dependency. Test, fuzz and benchmark packages are pinned exactly and locked with content hashes; CI fails on any package `dotnet list package --vulnerable` reports. |

### Cryptography status

This package implements no cryptography: crypto-server performs every
operation. Transport security is the platform's `HttpClient` and TLS
stack.
