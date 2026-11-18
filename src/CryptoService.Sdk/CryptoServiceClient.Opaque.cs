// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using CryptoService.Sdk.Internal;

namespace CryptoService.Sdk;

// The OPAQUE registration and login routes.
public sealed partial class CryptoServiceClient
{
    /// <summary>Starts an OPAQUE registration (<c>POST /v2/opaque/register/init</c>). Stateful: retried only after 429 or 503.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<OpaqueRegisterInitResult> OpaqueRegisterInitAsync(OpaqueRegisterInitRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/opaque/register/init", request, CryptoJsonContext.Default.OpaqueRegisterInitRequest, CryptoJsonContext.Default.OpaqueRegisterInitResultEnvelope, cancellationToken);

    /// <summary>Stores an OPAQUE registration record (<c>POST /v2/opaque/register/finish</c>). Stateful: retried only after 429 or 503.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<OpaqueRegisterFinishResult> OpaqueRegisterFinishAsync(OpaqueRegisterFinishRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/opaque/register/finish", request, CryptoJsonContext.Default.OpaqueRegisterFinishRequest, CryptoJsonContext.Default.OpaqueRegisterFinishResultEnvelope, cancellationToken);

    /// <summary>Starts an OPAQUE login (<c>POST /v2/opaque/login/init</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<OpaqueLoginInitResult> OpaqueLoginInitAsync(OpaqueLoginInitRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/opaque/login/init", request, CryptoJsonContext.Default.OpaqueLoginInitRequest, CryptoJsonContext.Default.OpaqueLoginInitResultEnvelope, cancellationToken);

    /// <summary>Finishes an OPAQUE login and returns the session key (<c>POST /v2/opaque/login/finish</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<OpaqueLoginFinishResult> OpaqueLoginFinishAsync(OpaqueLoginFinishRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/opaque/login/finish", request, CryptoJsonContext.Default.OpaqueLoginFinishRequest, CryptoJsonContext.Default.OpaqueLoginFinishResultEnvelope, cancellationToken);
}
