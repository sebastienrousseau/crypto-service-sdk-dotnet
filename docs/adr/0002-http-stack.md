<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->

# ADR 0002: The HTTP stack

- Status: Accepted
- Date: 2026-11-15

## Context

The SDK makes many small JSON calls, often concurrently, from servers
that keep one client for the process lifetime. It must reuse
connections, follow DNS changes, refuse redirects (a redirect would
carry the API key or token to another origin), keep TLS validation on,
and add no dependency a user's application would have to reconcile.

## Decision

- **`HttpClient` over a `SocketsHttpHandler`** that the client creates
  (`CryptoServiceClient.CreateHandler()`): pooled keep-alive
  connections recycled every five minutes (`PooledConnectionLifetime`),
  `AllowAutoRedirect = false`, no cookies, and
  `EnableMultipleHttp2Connections`. Each request asks for HTTP/2 with
  `RequestVersionOrLower`, so HTTPS negotiates HTTP/2 by ALPN and
  plain HTTP stays on HTTP/1.1. The SDK never sets a certificate
  callback and offers no option to.
- **Injection**: a caller's `HttpMessageHandler` (tests, custom
  transports) or `HttpClient` (`IHttpClientFactory` typed clients). An
  injected `HttpClient` is never reconfigured or disposed; a response
  whose request URI changed (a redirect the caller's handler followed)
  is refused.
- **`System.Text.Json` with source generation** (`CryptoJsonContext`):
  no reflection, trimming- and AOT-compatible, decoding that honours
  `required` and nullability. 2xx bodies are deserialized from the
  response stream; error bodies are read up to 64 KiB.
- **No third-party runtime dependency.**

## Consequences

- A caller using `IHttpClientFactory` should set
  `ConfigurePrimaryHttpMessageHandler(CryptoServiceClient.CreateHandler)`
  to keep redirects refused at the handler, not only detected after.
- The retry policy, timeouts and deadline are the SDK's own, so they
  behave the same whatever `HttpClient` is injected; an injected
  client's `Timeout` still applies on top.
