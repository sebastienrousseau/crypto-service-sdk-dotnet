// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using CryptoService.Sdk.Internal;

namespace CryptoService.Sdk;

// Sealed boxes, post-quantum streams, multi-recipient envelopes and HPKE.
public sealed partial class CryptoServiceClient
{
    /// <summary>Seals to an X25519 public key, anonymous sender (<c>POST /v2/sealedbox/seal</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<SealResult> SealedboxSealAsync(SealedboxSealRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/sealedbox/seal", request, CryptoJsonContext.Default.SealedboxSealRequest, CryptoJsonContext.Default.SealResultEnvelope, cancellationToken);

    /// <summary>Opens a sealed box with a server-held X25519 key (<c>POST /v2/sealedbox/open</c>); the server answers the plaintext as a bare string.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<string> SealedboxOpenAsync(SealedboxOpenRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/sealedbox/open", request, CryptoJsonContext.Default.SealedboxOpenRequest, CryptoJsonContext.Default.StringEnvelope, cancellationToken);

    /// <summary>Seals to a hybrid X25519 and ML-KEM-768 recipient (<c>POST /v2/sealedbox/seal-pq</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<SealResult> SealedboxSealPqAsync(SealedboxSealPqRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/sealedbox/seal-pq", request, CryptoJsonContext.Default.SealedboxSealPqRequest, CryptoJsonContext.Default.SealResultEnvelope, cancellationToken);

    /// <summary>Opens a hybrid sealed box with a server-held hybrid key (<c>POST /v2/sealedbox/open-pq</c>); the server answers the plaintext as a bare string.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<string> SealedboxOpenPqAsync(SealedboxOpenRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/sealedbox/open-pq", request, CryptoJsonContext.Default.SealedboxOpenRequest, CryptoJsonContext.Default.StringEnvelope, cancellationToken);

    /// <summary>Encrypts with hybrid post-quantum STREAM: X25519, ML-KEM-768 and XChaCha20-Poly1305 (<c>POST /v2/stream/pq-encrypt</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<CiphertextResult> StreamPqEncryptAsync(StreamPqEncryptRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/stream/pq-encrypt", request, CryptoJsonContext.Default.StreamPqEncryptRequest, CryptoJsonContext.Default.CiphertextResultEnvelope, cancellationToken);

    /// <summary>Decrypts hybrid post-quantum STREAM ciphertext with a server-held hybrid key (<c>POST /v2/stream/pq-decrypt</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<PlaintextResult> StreamPqDecryptAsync(StreamPqDecryptRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/stream/pq-decrypt", request, CryptoJsonContext.Default.StreamPqDecryptRequest, CryptoJsonContext.Default.PlaintextResultEnvelope, cancellationToken);

    /// <summary>Encrypts for several recipients with hybrid post-quantum STREAM (<c>POST /v2/stream/multi-pq-encrypt</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<StreamMultiPqEncryptResult> StreamMultiPqEncryptAsync(StreamMultiPqEncryptRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/stream/multi-pq-encrypt", request, CryptoJsonContext.Default.StreamMultiPqEncryptRequest, CryptoJsonContext.Default.StreamMultiPqEncryptResultEnvelope, cancellationToken);

    /// <summary>Decrypts multi-recipient STREAM ciphertext with a server-held hybrid key (<c>POST /v2/stream/multi-pq-decrypt</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<StreamMultiPqDecryptResult> StreamMultiPqDecryptAsync(StreamMultiPqDecryptRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/stream/multi-pq-decrypt", request, CryptoJsonContext.Default.StreamMultiPqDecryptRequest, CryptoJsonContext.Default.StreamMultiPqDecryptResultEnvelope, cancellationToken);

    /// <summary>Encrypts for several recipients with hybrid key wrapping (<c>POST /v2/multi-recipient/encrypt</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<MultiRecipientEncryptResult> MultiRecipientEncryptAsync(MultiRecipientEncryptRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/multi-recipient/encrypt", request, CryptoJsonContext.Default.MultiRecipientEncryptRequest, CryptoJsonContext.Default.MultiRecipientEncryptResultEnvelope, cancellationToken);

    /// <summary>Generates a server-held HPKE key pair, classical or post-quantum hybrid (<c>POST /v2/hpke/keygen</c>). Stateful: retried only after 429 or 503.</summary>
    /// <param name="request">The request, or <see langword="null"/> for the server defaults.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<KeyPairResult> HpkeGenerateKeyPairAsync(HpkeKeygenRequest? request = null, CancellationToken cancellationToken = default) =>
        PostOptionalAsync("/v2/hpke/keygen", request, CryptoJsonContext.Default.HpkeKeygenRequest, CryptoJsonContext.Default.KeyPairResultEnvelope, cancellationToken);

    /// <summary>Seals a message with HPKE, RFC 9180 (<c>POST /v2/hpke/seal</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<HpkeSealResult> HpkeSealAsync(HpkeSealRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/hpke/seal", request, CryptoJsonContext.Default.HpkeSealRequest, CryptoJsonContext.Default.HpkeSealResultEnvelope, cancellationToken);

    /// <summary>Opens an HPKE ciphertext with a server-held key (<c>POST /v2/hpke/open</c>); the answer has the plaintext as text and as hex.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<HpkeOpenResult> HpkeOpenAsync(HpkeOpenRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/hpke/open", request, CryptoJsonContext.Default.HpkeOpenRequest, CryptoJsonContext.Default.HpkeOpenResultEnvelope, cancellationToken);
}
