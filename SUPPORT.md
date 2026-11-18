<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->
<!-- SPDX-FileCopyrightText: 2026 Sebastien Rousseau -->

# Getting support

## Read first

- [README.md](README.md): install, quick start, configuration and the
  example index.
- The [user manual](https://sebastienrousseau.github.io/crypto-service-sdk-dotnet/),
  built from [`docs/`](docs/index.md), with the API reference generated
  from the XML documentation comments.
- [`examples/`](examples/): runnable projects, each run against
  crypto-server in CI.
- [DEVELOPMENT.md](DEVELOPMENT.md): building, testing and every CI gate
  on your own machine.

## Questions and how-to

Open a [GitHub issue](https://github.com/sebastienrousseau/crypto-service-sdk-dotnet/issues/new/choose).
Include:

- the SDK version (`CryptoService.Sdk` package or commit) and the .NET
  SDK version (`dotnet --version`);
- the crypto-service version or commit (`GET /` answers it);
- the smallest code that shows the problem, with synthetic keys;
- the exception type, `StatusCode`, `Problem.Type` and message.

Never paste a real key, credential, token or plaintext.

A question about what the server accepts or answers (an algorithm, a
parameter floor, a problem type) belongs in
[crypto-service](https://github.com/sebastienrousseau/crypto-service/issues):
this SDK only carries the request and the answer.

## Bugs and feature requests

Use the [bug report](https://github.com/sebastienrousseau/crypto-service-sdk-dotnet/issues/new?template=bug_report.yml)
or [feature request](https://github.com/sebastienrousseau/crypto-service-sdk-dotnet/issues/new?template=feature_request.yml)
form. Security reports never go in an issue: follow
[SECURITY.md](SECURITY.md).

## Supported versions

While pre-`1.0`, only the latest `0.0.x` receives fixes.

| Version | Status | Supported |
| :--- | :--- | :--- |
| `0.0.40` | Current (first release, 2026-11-16) | Yes |

## Response times

This project has a single maintainer (see [GOVERNANCE.md](GOVERNANCE.md)).
Issues are usually answered within a few business days; there is no
guaranteed response time.
