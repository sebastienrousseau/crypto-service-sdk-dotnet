// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using System.Text.Json.Serialization;

namespace CryptoService.Sdk;

/// <summary>The body of <c>POST /v2/sealedbox/seal</c> (X25519, anonymous sender).</summary>
public sealed class SealedboxSealRequest
{
    /// <summary>The recipient's X25519 public key, hex.</summary>
    [JsonPropertyName("recipientPublicKey")]
    public required string RecipientPublicKey { get; init; }

    /// <summary>The plaintext, UTF-8 text.</summary>
    [JsonPropertyName("plaintext")]
    public required string Plaintext { get; init; }
}

/// <summary>The body of <c>POST /v2/sealedbox/open</c> and <c>/v2/sealedbox/open-pq</c>.</summary>
public sealed class SealedboxOpenRequest
{
    /// <summary>The server-held recipient key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The sealed box, base64.</summary>
    [JsonPropertyName("sealed")]
    public required string Sealed { get; init; }
}

/// <summary>The body of <c>POST /v2/sealedbox/seal-pq</c> (X25519 and ML-KEM-768).</summary>
public sealed class SealedboxSealPqRequest
{
    /// <summary>The recipient's X25519 public key, hex.</summary>
    [JsonPropertyName("x25519PublicKey")]
    public required string X25519PublicKey { get; init; }

    /// <summary>The recipient's ML-KEM-768 public key, hex.</summary>
    [JsonPropertyName("mlKemPublicKey")]
    public required string MlKemPublicKey { get; init; }

    /// <summary>The plaintext, UTF-8 text.</summary>
    [JsonPropertyName("plaintext")]
    public required string Plaintext { get; init; }
}

/// <summary>The body of <c>POST /v2/stream/pq-encrypt</c>.</summary>
public sealed class StreamPqEncryptRequest
{
    /// <summary>The recipient's X25519 public key, 64 hex characters.</summary>
    [JsonPropertyName("x25519PublicKey")]
    public required string X25519PublicKey { get; init; }

    /// <summary>The recipient's ML-KEM-768 public key, hex.</summary>
    [JsonPropertyName("mlKemPublicKey")]
    public required string MlKemPublicKey { get; init; }

    /// <summary>The plaintext, UTF-8 text.</summary>
    [JsonPropertyName("plaintext")]
    public required string Plaintext { get; init; }

    /// <summary>The chunk size in bytes (64 to 1048576), or <see langword="null"/>.</summary>
    [JsonPropertyName("chunkSize")]
    public int? ChunkSize { get; init; }
}

/// <summary>The body of <c>POST /v2/stream/pq-decrypt</c>.</summary>
public sealed class StreamPqDecryptRequest
{
    /// <summary>The server-held hybrid key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The stream ciphertext, base64.</summary>
    [JsonPropertyName("ciphertext")]
    public required string Ciphertext { get; init; }

    /// <summary>The chunk size used to encrypt, or <see langword="null"/>.</summary>
    [JsonPropertyName("chunkSize")]
    public int? ChunkSize { get; init; }
}

/// <summary>The body of <c>POST /v2/stream/multi-pq-encrypt</c>.</summary>
public sealed class StreamMultiPqEncryptRequest
{
    /// <summary>The plaintext, UTF-8 text.</summary>
    [JsonPropertyName("plaintext")]
    public required string Plaintext { get; init; }

    /// <summary>The recipients.</summary>
    [JsonPropertyName("recipients")]
    public required IReadOnlyList<StreamRecipient> Recipients { get; init; }

    /// <summary>The chunk size in bytes (1024 to 16777216), or <see langword="null"/>.</summary>
    [JsonPropertyName("chunkSize")]
    public int? ChunkSize { get; init; }
}

/// <summary>One recipient of a multi-recipient stream.</summary>
public sealed class StreamRecipient
{
    /// <summary>The caller's ID for the recipient (1 to 255 characters).</summary>
    [JsonPropertyName("recipientId")]
    public required string RecipientId { get; init; }

    /// <summary>The recipient's X25519 public key, 64 hex characters.</summary>
    [JsonPropertyName("x25519PublicKey")]
    public required string X25519PublicKey { get; init; }

    /// <summary>The recipient's ML-KEM-768 public key, hex.</summary>
    [JsonPropertyName("mlKemPublicKey")]
    public required string MlKemPublicKey { get; init; }
}

/// <summary>A multi-recipient stream ciphertext.</summary>
public sealed class StreamMultiPqEncryptResult
{
    /// <summary>The algorithm used.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The ciphertext, standard base64 with padding.</summary>
    [JsonPropertyName("ciphertext")]
    public required string Ciphertext { get; init; }

    /// <summary>The number of recipients.</summary>
    [JsonPropertyName("recipientCount")]
    public required int RecipientCount { get; init; }

    /// <summary>The ciphertext's bytes.</summary>
    /// <returns>The decoded <see cref="Ciphertext"/>.</returns>
    /// <exception cref="FormatException">The server sent a value that is not valid base64.</exception>
    public byte[] GetCiphertextBytes() => Convert.FromBase64String(Ciphertext);
}

/// <summary>The body of <c>POST /v2/stream/multi-pq-decrypt</c>.</summary>
public sealed class StreamMultiPqDecryptRequest
{
    /// <summary>The server-held hybrid key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The stream ciphertext, base64.</summary>
    [JsonPropertyName("ciphertext")]
    public required string Ciphertext { get; init; }

    /// <summary>The recipient's ID, or <see langword="null"/> to find it by key.</summary>
    [JsonPropertyName("recipientId")]
    public string? RecipientId { get; init; }

    /// <summary>The chunk size used to encrypt, or <see langword="null"/>.</summary>
    [JsonPropertyName("chunkSize")]
    public int? ChunkSize { get; init; }
}

/// <summary>A plaintext recovered from a multi-recipient stream.</summary>
public sealed class StreamMultiPqDecryptResult
{
    /// <summary>The plaintext as UTF-8 text.</summary>
    [JsonPropertyName("plaintext")]
    public required string Plaintext { get; init; }

    /// <summary>The recipient that opened it.</summary>
    [JsonPropertyName("recipientId")]
    public required string RecipientId { get; init; }
}

/// <summary>The body of <c>POST /v2/multi-recipient/encrypt</c>.</summary>
public sealed class MultiRecipientEncryptRequest
{
    /// <summary>The plaintext, UTF-8 text.</summary>
    [JsonPropertyName("plaintext")]
    public required string Plaintext { get; init; }

    /// <summary>The recipients.</summary>
    [JsonPropertyName("recipients")]
    public required IReadOnlyList<MultiRecipient> Recipients { get; init; }
}

/// <summary>One recipient of a multi-recipient envelope.</summary>
public sealed class MultiRecipient
{
    /// <summary><c>classical</c> (X25519) or <c>pq</c> (X25519 and ML-KEM-768).</summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>The recipient's X25519 public key, hex.</summary>
    [JsonPropertyName("publicKey")]
    public required string PublicKey { get; init; }

    /// <summary>The recipient's ML-KEM-768 public key, hex, for <c>pq</c>.</summary>
    [JsonPropertyName("mlKemPublicKey")]
    public string? MlKemPublicKey { get; init; }
}

/// <summary>A multi-recipient envelope.</summary>
public sealed class MultiRecipientEncryptResult
{
    /// <summary>Always <c>multi-recipient-secretbox</c>.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The ciphertext, standard base64 with padding.</summary>
    [JsonPropertyName("ciphertext")]
    public required string Ciphertext { get; init; }

    /// <summary>The wrapped content key, once per recipient.</summary>
    [JsonPropertyName("recipients")]
    public required IReadOnlyList<WrappedRecipientKey> Recipients { get; init; }
}

/// <summary>The content key wrapped for one recipient.</summary>
public sealed class WrappedRecipientKey
{
    /// <summary>The ephemeral X25519 public key, lowercase hex.</summary>
    [JsonPropertyName("ephemeralPublicKey")]
    public required string EphemeralPublicKey { get; init; }

    /// <summary>The ML-KEM ciphertext, lowercase hex, for a post-quantum recipient.</summary>
    [JsonPropertyName("mlKemCiphertext")]
    public string? MlKemCiphertext { get; init; }

    /// <summary><c>x25519</c> or <c>x25519-ml-kem-768</c>.</summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>The wrapped key, standard base64 with padding.</summary>
    [JsonPropertyName("wrappedKey")]
    public required string WrappedKey { get; init; }
}

/// <summary>The body of <c>POST /v2/hpke/keygen</c>.</summary>
public sealed class HpkeKeygenRequest
{
    /// <summary>
    /// <c>x25519</c>, <c>p256</c>, <c>x-wing</c> (the server's default) or
    /// <c>x25519-ml-kem-768</c>; <see langword="null"/> for the default.
    /// </summary>
    [JsonPropertyName("kem")]
    public string? Kem { get; init; }
}

/// <summary>The body of <c>POST /v2/hpke/seal</c> (RFC 9180).</summary>
/// <remarks>
/// The server reads <see cref="Plaintext"/>, <see cref="Info"/> and
/// <see cref="Aad"/> as hex when they are an even-length hex string and as
/// UTF-8 text otherwise; the client sends them as given.
/// </remarks>
public sealed class HpkeSealRequest
{
    /// <summary>The recipient's public key, hex.</summary>
    [JsonPropertyName("recipientPublicKey")]
    public required string RecipientPublicKey { get; init; }

    /// <summary>The plaintext.</summary>
    [JsonPropertyName("plaintext")]
    public required string Plaintext { get; init; }

    /// <summary>The KEM of the recipient's key, or <see langword="null"/>.</summary>
    [JsonPropertyName("kem")]
    public string? Kem { get; init; }

    /// <summary><c>chacha20-poly1305</c> or <c>aes-128-gcm</c>, or <see langword="null"/>.</summary>
    [JsonPropertyName("aead")]
    public string? Aead { get; init; }

    /// <summary>The application info, or <see langword="null"/>.</summary>
    [JsonPropertyName("info")]
    public string? Info { get; init; }

    /// <summary>Additional authenticated data, or <see langword="null"/>.</summary>
    [JsonPropertyName("aad")]
    public string? Aad { get; init; }

    /// <summary>A pre-shared key, or <see langword="null"/>.</summary>
    [JsonPropertyName("psk")]
    public string? Psk { get; init; }

    /// <summary>The pre-shared key's ID, or <see langword="null"/>.</summary>
    [JsonPropertyName("pskId")]
    public string? PskId { get; init; }
}

/// <summary>An HPKE ciphertext.</summary>
public sealed class HpkeSealResult
{
    /// <summary>The ciphertext, lowercase hex.</summary>
    [JsonPropertyName("ciphertext")]
    public required string Ciphertext { get; init; }

    /// <summary>The encapsulated key, lowercase hex.</summary>
    [JsonPropertyName("encapsulatedKey")]
    public required string EncapsulatedKey { get; init; }
}

/// <summary>The body of <c>POST /v2/hpke/open</c>.</summary>
public sealed class HpkeOpenRequest
{
    /// <summary>The server-held recipient key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The ciphertext, hex.</summary>
    [JsonPropertyName("ciphertext")]
    public required string Ciphertext { get; init; }

    /// <summary>The encapsulated key, hex.</summary>
    [JsonPropertyName("encapsulatedKey")]
    public required string EncapsulatedKey { get; init; }

    /// <summary><c>chacha20-poly1305</c> or <c>aes-128-gcm</c>, or <see langword="null"/>.</summary>
    [JsonPropertyName("aead")]
    public string? Aead { get; init; }

    /// <summary>The application info used to seal, or <see langword="null"/>.</summary>
    [JsonPropertyName("info")]
    public string? Info { get; init; }

    /// <summary>The additional authenticated data used to seal, or <see langword="null"/>.</summary>
    [JsonPropertyName("aad")]
    public string? Aad { get; init; }

    /// <summary>The pre-shared key used to seal, or <see langword="null"/>.</summary>
    [JsonPropertyName("psk")]
    public string? Psk { get; init; }

    /// <summary>The pre-shared key's ID, or <see langword="null"/>.</summary>
    [JsonPropertyName("pskId")]
    public string? PskId { get; init; }
}

/// <summary>A plaintext as text and as bytes.</summary>
public sealed class HpkeOpenResult
{
    /// <summary>The plaintext's bytes, lowercase hex.</summary>
    [JsonPropertyName("hex")]
    public required string Hex { get; init; }

    /// <summary>The plaintext as UTF-8 text (invalid sequences become U+FFFD).</summary>
    [JsonPropertyName("plaintext")]
    public required string Plaintext { get; init; }

    /// <summary>The plaintext's exact bytes.</summary>
    /// <returns>The decoded <see cref="Hex"/>.</returns>
    /// <exception cref="FormatException">The server sent a value that is not valid hex.</exception>
    public byte[] GetPlaintextBytes() => Convert.FromHexString(Hex);
}
