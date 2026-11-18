// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using CryptoService.Sdk.Internal;

namespace CryptoService.Sdk;

// Hashing, MACs, key derivation and password hashing.
public sealed partial class CryptoServiceClient
{
    /// <summary>Computes a digest (<c>POST /v2/hash</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<HashResult> HashAsync(HashRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/hash", request, CryptoJsonContext.Default.HashRequest, CryptoJsonContext.Default.HashResultEnvelope, cancellationToken);

    /// <summary>Computes an HMAC (<c>POST /v2/hmac</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<HmacResult> MacAsync(HmacRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/hmac", request, CryptoJsonContext.Default.HmacRequest, CryptoJsonContext.Default.HmacResultEnvelope, cancellationToken);

    /// <summary>Verifies an HMAC (<c>POST /v2/hmac/verify</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<VerifyResult> MacVerifyAsync(HmacVerifyRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/hmac/verify", request, CryptoJsonContext.Default.HmacVerifyRequest, CryptoJsonContext.Default.VerifyResultEnvelope, cancellationToken);

    /// <summary>Derives a key from a password (<c>POST /v2/kdf</c>); the server enforces OWASP floors.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<KdfResult> KdfAsync(KdfRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/kdf", request, CryptoJsonContext.Default.KdfRequest, CryptoJsonContext.Default.KdfResultEnvelope, cancellationToken);

    /// <summary>Hashes a password with Argon2id (<c>POST /v2/password/hash</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<PasswordHashResult> PasswordHashAsync(PasswordHashRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/password/hash", request, CryptoJsonContext.Default.PasswordHashRequest, CryptoJsonContext.Default.PasswordHashResultEnvelope, cancellationToken);

    /// <summary>Verifies a password against a stored Argon2id hash (<c>POST /v2/password/verify</c>).</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<PasswordVerifyResult> PasswordVerifyAsync(PasswordVerifyRequest request, CancellationToken cancellationToken = default) =>
        PostAsync("/v2/password/verify", request, CryptoJsonContext.Default.PasswordVerifyRequest, CryptoJsonContext.Default.PasswordVerifyResultEnvelope, cancellationToken);
}
