<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->

# Architecture decision records

Decisions that will be questioned later, with the reasons they were
made. A new ADR is merged with the change it explains; a superseded one
stays, marked as such, with a link to what replaced it.

| ADR | Decision | Status |
| :--- | :--- | :--- |
| [0001](0001-thin-client-over-the-server.md) | A thin client over crypto-server, proven by its conformance kit; no cryptography in the client | Accepted |
| [0002](0002-http-stack.md) | `HttpClient` over a pooled `SocketsHttpHandler`, source-generated `System.Text.Json`, no third-party dependency | Accepted |
| [0003](0003-error-model.md) | Two exceptions by cause, RFC 9457 problems, no secret in a message | Accepted |
| [0004](0004-async-model.md) | Async only, `CancellationToken` everywhere, one client shared across threads | Accepted |
