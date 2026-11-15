// SPDX-License-Identifier: Apache-2.0 OR MIT

using CryptoService.Sdk.Internal;

namespace CryptoService.Sdk;

// ML-KEM, the hybrid KEM, ML-DSA and SLH-DSA.
public sealed partial class CryptoServiceClient
{
    /// <summary>Generates a server-held ML-KEM-768 key pair (<c>POST /v2/pq/keygen</c>). Stateful: retried only after 429 or 503.</summary>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<KeyPairResult> MlKemGenerateKeyPairAsync(CancellationToken cancellationToken = default) =>
        PostAsync("/v2/pq/keygen", EmptyRequest.Instance, CryptoJsonContext.Default.EmptyRequest, CryptoJsonContext.Default.KeyPairResultEnvelope, cancellationToken);

    /// <summary>Encapsulates a shared secret to an ML-KEM-768 public key (<c>POST /v2/pq/encapsulate</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<MlKemEncapsulateResult> MlKemEncapsulateAsync(MlKemEncapsulateRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/pq/encapsulate", request, CryptoJsonContext.Default.MlKemEncapsulateRequest, CryptoJsonContext.Default.MlKemEncapsulateResultEnvelope, cancellationToken);

    /// <summary>Recovers an ML-KEM shared secret with a server-held key (<c>POST /v2/pq/decapsulate</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<SharedSecretResult> MlKemDecapsulateAsync(MlKemDecapsulateRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/pq/decapsulate", request, CryptoJsonContext.Default.MlKemDecapsulateRequest, CryptoJsonContext.Default.SharedSecretResultEnvelope, cancellationToken);

    /// <summary>Generates a server-held hybrid X25519 and ML-KEM-768 key pair (<c>POST /v2/pq/hybrid/keygen</c>). Stateful: retried only after 429 or 503.</summary>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<HybridKeyPairResult> PqGenerateKeyPairAsync(CancellationToken cancellationToken = default) =>
        PostAsync("/v2/pq/hybrid/keygen", EmptyRequest.Instance, CryptoJsonContext.Default.EmptyRequest, CryptoJsonContext.Default.HybridKeyPairResultEnvelope, cancellationToken);

    /// <summary>Encapsulates a shared secret with the hybrid KEM (<c>POST /v2/pq/hybrid/encapsulate</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<HybridEncapsulateResult> PqEncapsulateAsync(HybridEncapsulateRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/pq/hybrid/encapsulate", request, CryptoJsonContext.Default.HybridEncapsulateRequest, CryptoJsonContext.Default.HybridEncapsulateResultEnvelope, cancellationToken);

    /// <summary>Recovers a hybrid KEM shared secret with a server-held key (<c>POST /v2/pq/hybrid/decapsulate</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<SharedSecretResult> PqDecapsulateAsync(HybridDecapsulateRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/pq/hybrid/decapsulate", request, CryptoJsonContext.Default.HybridDecapsulateRequest, CryptoJsonContext.Default.SharedSecretResultEnvelope, cancellationToken);

    /// <summary>Generates a server-held ML-DSA key pair, FIPS 204 (<c>POST /v2/pq/dsa/keygen</c>). Stateful: retried only after 429 or 503.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<KeyPairResult> PqSignKeygenAsync(MlDsaKeygenRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/pq/dsa/keygen", request, CryptoJsonContext.Default.MlDsaKeygenRequest, CryptoJsonContext.Default.KeyPairResultEnvelope, cancellationToken);

    /// <summary>Signs with a server-held ML-DSA key; the level comes from the key (<c>POST /v2/pq/dsa/sign</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<SignResult> PqSignAsync(SignRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/pq/dsa/sign", request, CryptoJsonContext.Default.SignRequest, CryptoJsonContext.Default.SignResultEnvelope, cancellationToken);

    /// <summary>Verifies an ML-DSA signature (<c>POST /v2/pq/dsa/verify</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<VerifyResult> PqVerifyAsync(MlDsaVerifyRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/pq/dsa/verify", request, CryptoJsonContext.Default.MlDsaVerifyRequest, CryptoJsonContext.Default.VerifyResultEnvelope, cancellationToken);

    /// <summary>Generates a server-held SLH-DSA key pair, FIPS 205 (<c>POST /v2/pq/slh-dsa/keygen</c>). Stateful: retried only after 429 or 503.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<KeyPairResult> PqHashSignKeygenAsync(SlhDsaKeygenRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/pq/slh-dsa/keygen", request, CryptoJsonContext.Default.SlhDsaKeygenRequest, CryptoJsonContext.Default.KeyPairResultEnvelope, cancellationToken);

    /// <summary>Signs with a server-held SLH-DSA key; the variant comes from the key (<c>POST /v2/pq/slh-dsa/sign</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<SignResult> PqHashSignAsync(SignRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/pq/slh-dsa/sign", request, CryptoJsonContext.Default.SignRequest, CryptoJsonContext.Default.SignResultEnvelope, cancellationToken);

    /// <summary>Verifies an SLH-DSA signature (<c>POST /v2/pq/slh-dsa/verify</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<VerifyResult> PqHashVerifyAsync(SlhDsaVerifyRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/pq/slh-dsa/verify", request, CryptoJsonContext.Default.SlhDsaVerifyRequest, CryptoJsonContext.Default.VerifyResultEnvelope, cancellationToken);
}
