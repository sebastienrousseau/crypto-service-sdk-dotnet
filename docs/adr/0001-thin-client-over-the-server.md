<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->
<!-- SPDX-FileCopyrightText: 2026 Sebastien Rousseau -->

# ADR 0001: A thin client over the server

- Status: Accepted
- Date: 2026-11-15

## Context

crypto-server holds keys and performs hashing, AEAD, signatures,
post-quantum KEM and signatures, HPKE and KMS operations behind a
versioned REST API. A .NET user needs those operations, with the
server's key custody (private keys never leave it) and its parameter
floors (OWASP KDF floors, algorithm allow-lists).

Reimplementing primitives in .NET would be a second implementation of
every algorithm, drifting from the first and moving keys into the
caller's process. A generated OpenAPI client would type the wire but
prove nothing about it.

## Decision

The SDK is a thin, hand-written client. It builds requests, sends them
with `HttpClient` and decodes the answers. It implements no
cryptography: no hashing, encryption, signing or key generation, and no
local verification of what the server answers. Binary fields keep the
encoding the OpenAPI document names (hex, base64, UTF-8 text);
`Get*Bytes()` accessors only decode that encoding.

crypto-service's conformance kit (`conformance/`), vendored byte for
byte at the commit in `ci/server.ref` and pinned by its `SHA256SUMS`, is
the contract: CI builds the server at that commit and runs every vector
through the public methods.

## Consequences

- One engine: a fix in crypto-server reaches every SDK without a
  release of this package.
- Keys stay on the server: the client handles `keyId`s and public keys.
- Every operation is a network call; the user runs or reaches a server.
- The kit decides when the client is wrong; a wire change is a
  coordinated suite release, never a local divergence.
