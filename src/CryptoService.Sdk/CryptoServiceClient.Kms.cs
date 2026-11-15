// SPDX-License-Identifier: Apache-2.0 OR MIT

using CryptoService.Sdk.Internal;

namespace CryptoService.Sdk;

// The KMS provider routes.
public sealed partial class CryptoServiceClient
{
    /// <summary>Creates a managed key in a KMS provider (<c>POST /v2/kms/create-key</c>). Stateful: retried only after 429 or 503.</summary>
    /// <param name="request">The request, or <see langword="null"/> for the server defaults.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<KmsKey> KmsCreateKeyAsync(KmsCreateKeyRequest? request = null, CancellationToken cancellationToken = default) =>
        PostOptionalAsync("/v2/kms/create-key", request, CryptoJsonContext.Default.KmsCreateKeyRequest, CryptoJsonContext.Default.KmsKeyEnvelope, cancellationToken);

    /// <summary>Wraps a key with a managed KMS key (<c>POST /v2/kms/wrap</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<KmsWrapResult> KmsWrapAsync(KmsWrapRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/kms/wrap", request, CryptoJsonContext.Default.KmsWrapRequest, CryptoJsonContext.Default.KmsWrapResultEnvelope, cancellationToken);

    /// <summary>Unwraps a key with a managed KMS key (<c>POST /v2/kms/unwrap</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<KmsUnwrapResult> KmsUnwrapAsync(KmsUnwrapRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/kms/unwrap", request, CryptoJsonContext.Default.KmsUnwrapRequest, CryptoJsonContext.Default.KmsUnwrapResultEnvelope, cancellationToken);

    /// <summary>Generates a data encryption key under a managed KMS key (<c>POST /v2/kms/generate-data-key</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<KmsDataKey> KmsGenerateDataKeyAsync(KmsGenerateDataKeyRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/kms/generate-data-key", request, CryptoJsonContext.Default.KmsGenerateDataKeyRequest, CryptoJsonContext.Default.KmsDataKeyEnvelope, cancellationToken);

    /// <summary>Encrypts with a managed KMS key (<c>POST /v2/kms/encrypt</c>); build the request with <see cref="KmsEncryptRequest.FromText"/> or <see cref="KmsEncryptRequest.FromBytes"/>.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<KmsEncryptResult> KmsEncryptAsync(KmsEncryptRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/kms/encrypt", request, CryptoJsonContext.Default.KmsEncryptRequest, CryptoJsonContext.Default.KmsEncryptResultEnvelope, cancellationToken);

    /// <summary>Decrypts with a managed KMS key (<c>POST /v2/kms/decrypt</c>); the answer has the plaintext as text and as hex.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<KmsDecryptResult> KmsDecryptAsync(KmsDecryptRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/kms/decrypt", request, CryptoJsonContext.Default.KmsDecryptRequest, CryptoJsonContext.Default.KmsDecryptResultEnvelope, cancellationToken);
}
