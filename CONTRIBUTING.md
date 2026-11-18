<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->
<!-- SPDX-FileCopyrightText: 2026 Sebastien Rousseau -->

# Contributing

This repository is the .NET client for the Crypto Service REST API. It
is thin on purpose: crypto-server does the cryptography, and this SDK
turns its JSON into typed .NET calls. Read [AGENTS.md](AGENTS.md) for
the rules that must not drift, and [DEVELOPMENT.md](DEVELOPMENT.md) for
the toolchain and every gate.

## Before you open a pull request

You need the .NET 10 SDK (`mise install` gives the pinned one).

```sh
make check          # format, build, 100 % coverage, audit, versions, Halstead
```

`make check` is what CI runs on every pull request, on Linux, Windows
and macOS.

### Against a live server

The conformance kit, the examples and the README code blocks need a
running crypto-server; [DEVELOPMENT.md](DEVELOPMENT.md#a-server) shows
how to build and start the pinned one. Then:

```sh
make conformance examples readme
```

`make regression` does all of it, server included.

## What the tests are for

- `OperationTests`: every public method against an in-memory handler:
  route, credentials, body, decoding of every binary field.
- `ClientTests`: construction, headers, `traceparent`, raw requests,
  redirects, disposal.
- `RetryTests`: the resilience contract, with clocks that never sleep.
- `ErrorTests`, `PropertyTests`: the error model, and FsCheck properties
  over arbitrary bytes.
- `KitTests`: the vendored kit matches `conformance/SHA256SUMS`.
- `ConformanceTests`: every vector against a live server.

## The conformance kit

`conformance/` is vendored from
[crypto-service](https://github.com/sebastienrousseau/crypto-service),
byte for byte, at the commit in `ci/server.ref`. Never edit it here:
change it upstream, then copy the new kit in and update
`ci/server.ref` in the same commit.

## Commits

Conventional commits (`feat:`, `fix:`, `docs:`, ...), an imperative
subject of 50 characters at most, and a body wrapped at 72 that says
what and why. Every commit is signed off (`git commit -s`), certifying
the [Developer Certificate of Origin](DCO.txt), and signed.

## Licence

Apache-2.0 OR MIT, at your option. By contributing you agree your work
is released under the same dual grant.
