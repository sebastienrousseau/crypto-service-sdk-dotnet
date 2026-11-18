<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->
<!-- SPDX-FileCopyrightText: 2026 Sebastien Rousseau -->

# Security Policy

## Supported versions

This SDK follows the Crypto Service suite cadence. While pre-`1.0`, the
latest released `0.0.x` receives security fixes; older `0.0.x` versions
do not. `0.0.40` is the first release.

| Version | Status | Receives security fixes? |
| :--- | :--- | :--- |
| `0.0.40` | Current (first release, 2026-11-16) | Yes |

## Reporting a vulnerability

**Do not open a public issue for security reports.**

Report privately through GitHub Private Vulnerability Reporting:
<https://github.com/sebastienrousseau/crypto-service-sdk-dotnet/security/advisories/new>

Include the SDK version, the crypto-service commit or version, and the
smallest reproduction with synthetic keys. Never send a real key,
credential, token or plaintext. A vulnerability in the server itself
belongs to [crypto-service](https://github.com/sebastienrousseau/crypto-service/security).

## Security posture

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
