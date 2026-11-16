<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->

# Ecosystem comparison

The suite's SDKs are thin clients over one server, share one
resilience contract (options, retry-safety table, error model) and run
crypto-service's conformance kit against a live server in CI.

| Project | Language (minimum) | Conformance kit | Runtime dependencies | Call model |
| :--- | :---: | :--- | :--- | :--- |
| `@sebastienrousseau/crypto-sdk` | TypeScript | The server's own test suite runs the kit; the SDK is the origin of the retry-safety table | None beyond the platform `fetch` | `async`, `fetch` |
| **crypto-service-sdk-dotnet** | **.NET 10** | **Every vector, through the public methods, in `ci.yml`** | **None** | **`async`/`await`, `CancellationToken`** |
| crypto-service-sdk-python, -go, -java, -rust, -cpp | See each repository | The same kit | See each repository | See each repository |
| Raw HTTP | Any | No | Your HTTP client | Yours |

The other SDKs are developed alongside this one; their rows link to
their repositories rather than restate claims this repository cannot
check.

## This SDK's row

- **.NET 10**: `<TargetFramework>net10.0</TargetFramework>` in every
  project; [Policies](POLICIES.md).
- **Conformance kit in CI**: the `conformance` job of
  [`ci.yml`](https://github.com/sebastienrousseau/crypto-service-sdk-dotnet/blob/main/.github/workflows/ci.yml)
  builds crypto-service at `ci/server.ref`, starts it, and runs every
  vector of every kit file, each file as its own principal.
- **No runtime dependency**: the library's csproj has no
  `PackageReference`; [ADR 0002](adr/0002-http-stack.md).
- **async/await**: every method returns a `Task` and takes a
  `CancellationToken`; [ADR 0004](adr/0004-async-model.md).
