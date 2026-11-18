<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->
<!-- SPDX-FileCopyrightText: 2026 Sebastien Rousseau -->

# Packaging

## NuGet

`CryptoService.Sdk` is published to nuget.org by
`.github/workflows/release.yml` on a signed tag, through trusted
publishing (OIDC): the workflow exchanges a short-lived GitHub token for
a NuGet API key in the `release` environment, with the account name in
the `NUGET_USER` secret. No long-lived key exists.

Each release ships:

| Asset | What it is |
| :--- | :--- |
| `CryptoService.Sdk.<version>.nupkg` | The library, its XML documentation and the README |
| `CryptoService.Sdk.<version>.snupkg` | Symbols |
| `CryptoService.Sdk.<version>.cdx.json` | CycloneDX SBOM |
| `SHA256SUMS` | SHA-256 of every asset |

and a build provenance attestation for the packages
(`gh attestation verify <file> --repo sebastienrousseau/crypto-service-sdk-dotnet`).

## Package validation

Every `dotnet pack` runs .NET package validation. `scripts/api-compat.sh`
adds the newest published version as the ApiCompat baseline, so a
breaking change to the public API fails CI; until the first release is
published there is no baseline and the script says so.

## No other distribution

No distribution packages (Linux, Homebrew, containers) are claimed: the
package is a library, consumed from NuGet.
