// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using CryptoService.Sdk.Internal;

namespace CryptoService.Sdk;

// Symmetric encryption, password encryption and AES key wrapping.
public sealed partial class CryptoServiceClient
{
    /// <summary>Encrypts with XChaCha20-Poly1305 under a 256-bit hex key (<c>POST /v2/encrypt</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<CiphertextResult> EncryptAsync(EncryptRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/encrypt", request, CryptoJsonContext.Default.EncryptRequest, CryptoJsonContext.Default.CiphertextResultEnvelope, cancellationToken);

    /// <summary>Decrypts a ciphertext from <see cref="EncryptAsync"/> (<c>POST /v2/decrypt</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<PlaintextResult> DecryptAsync(DecryptRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/decrypt", request, CryptoJsonContext.Default.DecryptRequest, CryptoJsonContext.Default.PlaintextResultEnvelope, cancellationToken);

    /// <summary>Seals with XChaCha20-Poly1305 (<c>POST /v2/secretbox/seal</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<SealResult> SecretboxSealAsync(SecretboxSealRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/secretbox/seal", request, CryptoJsonContext.Default.SecretboxSealRequest, CryptoJsonContext.Default.SealResultEnvelope, cancellationToken);

    /// <summary>Opens a secretbox (<c>POST /v2/secretbox/open</c>); the server answers the plaintext as a bare string.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<string> SecretboxOpenAsync(SecretboxOpenRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/secretbox/open", request, CryptoJsonContext.Default.SecretboxOpenRequest, CryptoJsonContext.Default.StringEnvelope, cancellationToken);

    /// <summary>Encrypts with a password, Argon2id and XChaCha20-Poly1305 (<c>POST /v2/password/encrypt</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<PasswordEncryptResult> PasswordEncryptAsync(PasswordEncryptRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/password/encrypt", request, CryptoJsonContext.Default.PasswordEncryptRequest, CryptoJsonContext.Default.PasswordEncryptResultEnvelope, cancellationToken);

    /// <summary>Decrypts a <see cref="PasswordEncryptAsync"/> envelope (<c>POST /v2/password/decrypt</c>); the server answers the plaintext as a bare string.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<string> PasswordDecryptAsync(PasswordDecryptRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/password/decrypt", request, CryptoJsonContext.Default.PasswordDecryptRequest, CryptoJsonContext.Default.StringEnvelope, cancellationToken);

    /// <summary>Wraps a key with AES-KW or AES-KWP (<c>POST /v2/keys/wrap</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<KeyWrapResult> KeyWrapAsync(KeyWrapRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/keys/wrap", request, CryptoJsonContext.Default.KeyWrapRequest, CryptoJsonContext.Default.KeyWrapResultEnvelope, cancellationToken);

    /// <summary>Unwraps a key (<c>POST /v2/keys/unwrap</c>); the server answers the key as a bare hex string.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<string> KeyUnwrapAsync(KeyUnwrapRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/keys/unwrap", request, CryptoJsonContext.Default.KeyUnwrapRequest, CryptoJsonContext.Default.StringEnvelope, cancellationToken);
}
