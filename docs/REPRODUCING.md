<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->
<!-- SPDX-FileCopyrightText: 2026 Sebastien Rousseau -->

# Reproducing a release

Anyone can rebuild a released `CryptoService.Sdk` from its signed tag
and check that it is the package the release published, without
trusting the CI runner that built it.

## What is reproducible

| Release | Result |
| :--- | :--- |
| 0.0.41 and later | The release workflow pins the zip entry times and the SBOM's serial number and timestamp, so every asset of the GitHub release (`.nupkg`, `.snupkg`, `.cdx.json`) is expected to rebuild byte for byte and match `SHA256SUMS` as published; the script checks it. |
| 0.0.40 | Every file inside the `.nupkg` and the `.snupkg` (the DLL, the PDB, the XML documentation, the nuspec, the README and the package metadata) rebuilds byte for byte. The archives themselves differ only in their zip entry times, and the SBOM only in its serial number and timestamp: 0.0.40 was released before the release workflow pinned those. After `scripts/normalise-release.py` pins them on both sides, the three assets are identical. |

The package on nuget.org carries nuget.org's repository signature
(`.signature.p7s`), added after upload, so its archive differs from the
GitHub asset; every file in it that came from the build is the same.

## What makes it deterministic

- `<Deterministic>true</Deterministic>` and, with `CI=true`,
  `ContinuousIntegrationBuild`, which maps source paths to `/_/` so the
  checkout directory does not reach the binaries
  ([`Directory.Build.props`](https://github.com/sebastienrousseau/crypto-service-sdk-dotnet/blob/main/Directory.Build.props)).
- The .NET SDK pinned in `global.json` and `mise.toml`; packages pinned
  exactly in `Directory.Packages.props`, locked with content hashes in
  `packages.lock.json` and restored in locked mode; the CycloneDX tool
  pinned in `.config/dotnet-tools.json`.
- SourceLink records the commit and the repository URL in the PDB, so a
  rebuild must come from a clone of the GitHub repository at the tag
  (a clone from another URL changes the PDB, and through its checksum
  the DLL).
- `scripts/normalise-release.py` sets every zip entry time to the
  tagged commit's time and derives the SBOM's serial number from its
  content (a UUIDv5 of its SHA-256), with that same timestamp. The
  release workflow runs it before writing `SHA256SUMS`.

## Procedure

You need git, curl, Python 3 and the exact .NET SDK the tag's
`global.json` names (10.0.404 for 0.0.40 and 0.0.41; `mise install`
gives it). `global.json` lets a newer SDK of the same feature band
build the project, but only the pinned one is expected to give the
same bytes, and the script refuses another.

```sh
git clone https://github.com/sebastienrousseau/crypto-service-sdk-dotnet.git
cd crypto-service-sdk-dotnet
mise install
mise exec -- scripts/reproduce-release.sh 0.0.40
```

[`scripts/reproduce-release.sh`](https://github.com/sebastienrousseau/crypto-service-sdk-dotnet/blob/main/scripts/reproduce-release.sh)
does this, in a temporary directory:

1. Clones the tag `v<version>` from GitHub and checks that
   `dotnet --version` is the SDK its `global.json` pins.
2. With `CI=true`, runs `dotnet restore --locked-mode`,
   `dotnet build -c Release` and `dotnet pack -c Release --no-build` on
   `src/CryptoService.Sdk/CryptoService.Sdk.csproj`, then the pinned
   CycloneDX tool with the release workflow's arguments.
3. Downloads the release's `SHA256SUMS` and every asset it lists, and
   checks them against it.
4. Runs `scripts/normalise-release.py` with the tagged commit's time on
   both sides.
5. Prints `identical` or `DIFFERENT` for each asset, and exits non-zero
   on any difference.

To check the tag's signature as well, run
`git -c gpg.format=ssh -c gpg.ssh.allowedSignersFile=KEYS.asc verify-tag v0.0.40`
in the clone, and `gh attestation verify <nupkg> --repo
sebastienrousseau/crypto-service-sdk-dotnet` for the build provenance.

## Expected hashes for 0.0.40

As published in the release's `SHA256SUMS`:

| Asset | SHA-256 |
| :--- | :--- |
| `CryptoService.Sdk.0.0.40.nupkg` | `5967cd8a884ae48f050eabf5b477e46af552624c024e716e2308b9ebee79ce2a` |
| `CryptoService.Sdk.0.0.40.snupkg` | `9f3645c319bd84b931fbb70d950f67bd3fba8ea4bd285679c5e3aacad6a97a42` |
| `CryptoService.Sdk.0.0.40.cdx.json` | `4bdbec637dbc0dc9a8fe0ea341225f8178fd27b8102e45572cff026f2b9e21eb` |

The files the build produced, the same in the release and in a
rebuild:

| File | SHA-256 |
| :--- | :--- |
| `lib/net10.0/CryptoService.Sdk.dll` | `1031355089ea390ec4b71dca5ec2e52b6a63635c25755865e03e19881eda02a3` |
| `lib/net10.0/CryptoService.Sdk.pdb` (in the `.snupkg`) | `1868abb9ae1b2c786639a662de2bb22ec92fa78aaea2bf3df7cb7b1c620c3b1c` |
| `lib/net10.0/CryptoService.Sdk.xml` | `335a175f0262b14537e839a33de80a4706c4f338acb151ea406bbda4f272f156` |
| `CryptoService.Sdk.nuspec` | `881310c091768067fb194c6468d1cb0e043f6c177b21d16c1137b66b591ec3d0` |

After normalisation to the tagged commit's time (1794908956, that is
2026-11-17T09:49:16Z), both the release and a rebuild give:

| Asset | SHA-256 |
| :--- | :--- |
| `CryptoService.Sdk.0.0.40.nupkg` | `a3aa2f897ab179780fc3e21cfc2fb35846de378feccdefbce23313e94df0da13` |
| `CryptoService.Sdk.0.0.40.snupkg` | `0789e338faa313cd85d68608443e5d8518568cf3ef9cbb05bdfd6b419333771b` |
| `CryptoService.Sdk.0.0.40.cdx.json` | `451ad47a50dff37d9a6be3023e3dd27323f9d591dbd6499a2770670e77314b3d` |

## Verification record

On 2026-11-18, on Linux x86-64 with .NET SDK 10.0.404, outside CI: two
clean clones of `v0.0.40` built in different directories gave
identical DLL, PDB and XML documentation files, and the same files as
the release; `scripts/reproduce-release.sh 0.0.40` then printed
`identical` for all three assets. A first attempt from a clone whose
`origin` was a local path gave a different PDB and DLL, which is why
the script clones from GitHub.
