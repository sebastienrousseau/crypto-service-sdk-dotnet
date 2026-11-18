<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->
<!-- SPDX-FileCopyrightText: 2026 Sebastien Rousseau -->

# ADR 0004: The async model

- Status: Accepted
- Date: 2026-11-15

## Context

Every operation is network I/O. The suite's SDKs offer an async API
first and a blocking one where the language makes it idiomatic.
pain001-sdk-dotnet, the template this repository follows, is async
only. In .NET a blocking wrapper over async I/O (`.Result`,
`.GetAwaiter().GetResult()`) risks thread-pool starvation and
deadlocks under a synchronization context, and `HttpClient`'s own
synchronous `Send` does not support every handler.

## Decision

- Every operation is `Task<T> XxxAsync(..., CancellationToken
  cancellationToken = default)`. There is no blocking variant; a caller
  that must block does so at its own boundary.
- Library awaits use `ConfigureAwait(false)` (CA2007 is enforced).
- `CryptoServiceClient` is immutable after construction and safe to
  share across threads and concurrent calls; it implements
  `IDisposable` and `IAsyncDisposable`, and should live as long as the
  application, not one call.
- Cancellation is the token: cancelling it raises
  `OperationCanceledException` and sends no further attempt. Timeouts
  and the deadline are separate, and raise `CryptoTransportException`
  with `TimedOut` set.

## Consequences

- Concurrency is `Task.WhenAll` over one client (the `Concurrent`
  example); the pooled handler bounds connections, not the SDK.
- A console or test that needs a synchronous call uses an `async Main`
  or its framework's async support.
