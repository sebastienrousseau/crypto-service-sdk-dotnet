<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->

# ADR 0003: The error model

- Status: Accepted
- Date: 2026-11-15

## Context

crypto-server answers every error as an RFC 9457 problem
(`application/problem+json`) with a stable `urn:crypto-service:problem:*`
type, a `code` from crypto-lib or the key store, and `errors[]` for a
validation failure. Proxies in front of it can answer anything. Every
SDK of the suite exposes the same two failure kinds, and exception
messages end up in logs, so they must never carry a key, a plaintext or
a credential.

## Decision

| Raised | When | Carries |
| :--- | :--- | :--- |
| `CryptoApiException` | A status outside 2xx | `StatusCode`, `Problem` (`Type`, `Title`, `Status`, `Detail`, `Instance`, `Code`, `Errors`, `Limit`, `ResetSeconds`, other members in `Extensions`) |
| `CryptoTransportException` | No usable answer: a connection failure, an attempt timeout or the deadline (`TimedOut` true), a refused redirect, a 2xx body that breaks the documented contract | `TimedOut`, the cause in `InnerException` |
| `OperationCanceledException` | The caller's token was cancelled | |
| `ArgumentException` | Invalid options or a null request | |

Both SDK exceptions derive from `CryptoServiceException`. A body that is
not a problem (HTML from a proxy, an empty body, JSON of another shape)
becomes an `about:blank` problem with the reason phrase as title. The
message names the status, the problem type and its title only; the
`detail` stays in `Problem`.

## Consequences

- `catch (CryptoServiceException)` catches every failure the SDK owns.
- A returned result always satisfies the documented schema: missing or
  null required members are a `CryptoTransportException`, never a
  half-filled object.
- The problem parser and the 2xx decoder are fuzz targets: neither may
  fail any other way.
