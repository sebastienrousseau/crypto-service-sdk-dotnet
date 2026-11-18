<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->
<!-- SPDX-FileCopyrightText: 2026 Sebastien Rousseau -->

# Policies

## Minimum toolchain

| Item | Floor | Enforced by |
| :--- | :--- | :--- |
| Target framework | `net10.0` | `<TargetFramework>` in every project |
| SDK to build | .NET SDK 10.0.404 | `global.json` (roll forward within the feature band), `mise.toml` |
| Platforms tested | Linux, Windows and macOS, on the GitHub-hosted `*-latest` runners | `ci.yml` build matrix |

The floor is a .NET release Microsoft supports. It moves:

- only when the current floor reaches end of support, or a newer
  release is needed for a feature the client uses;
- in a release whose CHANGELOG says so under **Changed**;
- never as a side effect of another change.

## Versioning

The version is `0.0.x` and moves by `0.0.1`, in lockstep with the
Crypto Service suite. Before `1.0`, any release may change the public
API; the CHANGELOG says how, and the API compatibility check
(`scripts/api-compat.sh`) flags every break against the last published
package so none is accidental.

## Output stability

The SDK produces no cryptographic output of its own. What it
guarantees is the mapping: the same server answer decodes to the same
objects and raises the same exception types
([ADR 0003](adr/0003-error-model.md)). A change to that mapping is
breaking even when no signature moves.

## API surface

Every v2 operation of the OpenAPI document has a method. The
deprecated v1 routes (sunset 2027-01-01) and `/metrics` do not, and
will not; `RequestAsync` reaches any route.

## Deprecation

A public member is marked `[Obsolete]` with the replacement named, for
at least one release, before it is removed.
