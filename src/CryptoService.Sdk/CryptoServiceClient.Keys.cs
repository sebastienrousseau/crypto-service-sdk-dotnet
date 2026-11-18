// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using CryptoService.Sdk.Internal;

namespace CryptoService.Sdk;

// Server-held keys, Ed25519 signatures, batches and ISO 20022 envelopes.
public sealed partial class CryptoServiceClient
{
    /// <summary>Generates a server-held key pair (<c>POST /v2/keys/generate</c>); the private key stays on the server. Stateful: retried only after 429 or 503.</summary>
    /// <param name="request">The request, or <see langword="null"/> for the server defaults.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<GenerateKeyResult> GenerateKeyPairAsync(GenerateKeyRequest? request = null, CancellationToken cancellationToken = default) =>
        PostOptionalAsync("/v2/keys/generate", request, CryptoJsonContext.Default.GenerateKeyRequest, CryptoJsonContext.Default.GenerateKeyResultEnvelope, cancellationToken);

    /// <summary>Exports a server-held key, private parts included (<c>POST /v2/keys/export</c>); needs the <c>crypto:keys:export</c> scope, which <c>crypto:admin</c> does not imply.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<ExportKeyResult> ExportKeyAsync(KeyIdRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/keys/export", request, CryptoJsonContext.Default.KeyIdRequest, CryptoJsonContext.Default.ExportKeyResultEnvelope, cancellationToken);

    /// <summary>Deletes a server-held key (<c>POST /v2/keys/delete</c>). Stateful: retried only after 429 or 503.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<DeleteKeyResult> DeleteKeyAsync(KeyIdRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/keys/delete", request, CryptoJsonContext.Default.KeyIdRequest, CryptoJsonContext.Default.DeleteKeyResultEnvelope, cancellationToken);

    /// <summary>Signs with a server-held Ed25519 key (<c>POST /v2/sign</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<SignResult> SignAsync(SignRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/sign", request, CryptoJsonContext.Default.SignRequest, CryptoJsonContext.Default.SignResultEnvelope, cancellationToken);

    /// <summary>Verifies an Ed25519 signature (<c>POST /v2/verify</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<VerifyResult> VerifyAsync(VerifyRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/verify", request, CryptoJsonContext.Default.VerifyRequest, CryptoJsonContext.Default.VerifyResultEnvelope, cancellationToken);

    /// <summary>Signs a batch of messages with server-held Ed25519 keys (<c>POST /v2/stream/sign</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<StreamSignResult> StreamSignAsync(StreamSignRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/stream/sign", request, CryptoJsonContext.Default.StreamSignRequest, CryptoJsonContext.Default.StreamSignResultEnvelope, cancellationToken);

    /// <summary>Verifies a batch of Ed25519 signatures (<c>POST /v2/stream/verify</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<StreamVerifyResult> StreamVerifyAsync(StreamVerifyRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/stream/verify", request, CryptoJsonContext.Default.StreamVerifyRequest, CryptoJsonContext.Default.StreamVerifyResultEnvelope, cancellationToken);

    /// <summary>Verifies an ISO 20022 dual-signature envelope against the caller's trusted keys (<c>POST /v2/stream/iso20022</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<Iso20022VerifyResult> StreamIso20022Async(Iso20022VerifyRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/stream/iso20022", request, CryptoJsonContext.Default.Iso20022VerifyRequest, CryptoJsonContext.Default.Iso20022VerifyResultEnvelope, cancellationToken);
}
