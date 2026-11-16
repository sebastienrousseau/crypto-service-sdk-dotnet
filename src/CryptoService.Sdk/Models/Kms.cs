// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Text.Json.Serialization;

namespace CryptoService.Sdk;

/// <summary>The body of <c>POST /v2/kms/create-key</c>.</summary>
public sealed class KmsCreateKeyRequest
{
    /// <summary>The key algorithm, or <see langword="null"/> for the provider's default.</summary>
    [JsonPropertyName("algorithm")]
    public string? Algorithm { get; init; }

    /// <summary><c>encrypt</c>, <c>sign</c> or <c>wrap</c>, or <see langword="null"/>.</summary>
    [JsonPropertyName("usage")]
    public string? Usage { get; init; }

    /// <summary>The KMS provider, or <see langword="null"/> for the server's default.</summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; init; }

    /// <summary>String metadata to store with the key, or <see langword="null"/>.</summary>
    [JsonPropertyName("metadata")]
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}

/// <summary>A managed KMS key.</summary>
public sealed class KmsKey
{
    /// <summary>The key algorithm.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>When the key was created.</summary>
    [JsonPropertyName("createdAt")]
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>The current key version, when the provider versions keys.</summary>
    [JsonPropertyName("currentVersion")]
    public int? CurrentVersion { get; init; }

    /// <summary>Whether the key is enabled.</summary>
    [JsonPropertyName("enabled")]
    public required bool Enabled { get; init; }

    /// <summary>The provider's key ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The provider.</summary>
    [JsonPropertyName("provider")]
    public required string Provider { get; init; }

    /// <summary><c>encrypt</c>, <c>sign</c> or <c>wrap</c>.</summary>
    [JsonPropertyName("usage")]
    public required string Usage { get; init; }
}

/// <summary>
/// The body of <c>POST /v2/kms/encrypt</c>. Build it with
/// <see cref="FromText"/> or <see cref="FromBytes"/>, which state the
/// encoding; the server never guesses it.
/// </summary>
public sealed class KmsEncryptRequest
{
    /// <summary>The managed key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The plaintext, in <see cref="Encoding"/>.</summary>
    [JsonPropertyName("plaintext")]
    public required string Plaintext { get; init; }

    /// <summary><c>utf8</c> or <c>hex</c>; <see langword="null"/> means <c>utf8</c>.</summary>
    [JsonPropertyName("encoding")]
    public string? Encoding { get; init; }

    /// <summary>The encryption context, or <see langword="null"/>.</summary>
    [JsonPropertyName("context")]
    public IReadOnlyDictionary<string, string>? Context { get; init; }

    /// <summary>The KMS provider, or <see langword="null"/> for the server's default.</summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; init; }

    /// <summary>A request to encrypt UTF-8 text (<c>encoding: utf8</c>).</summary>
    /// <param name="keyId">The managed key's ID.</param>
    /// <param name="text">The text.</param>
    /// <returns>The request.</returns>
    public static KmsEncryptRequest FromText(string keyId, string text) =>
        new() { KeyId = keyId, Plaintext = text, Encoding = "utf8" };

    /// <summary>A request to encrypt bytes, sent as hex (<c>encoding: hex</c>).</summary>
    /// <param name="keyId">The managed key's ID.</param>
    /// <param name="bytes">The bytes.</param>
    /// <returns>The request.</returns>
    public static KmsEncryptRequest FromBytes(string keyId, ReadOnlySpan<byte> bytes) =>
        new() { KeyId = keyId, Plaintext = Convert.ToHexStringLower(bytes), Encoding = "hex" };
}

/// <summary>A KMS ciphertext.</summary>
public sealed class KmsEncryptResult
{
    /// <summary>The ciphertext, an opaque provider string: pass it back unchanged.</summary>
    [JsonPropertyName("ciphertext")]
    public required string Ciphertext { get; init; }

    /// <summary>The encryption context, when one was given.</summary>
    [JsonPropertyName("context")]
    public IReadOnlyDictionary<string, string>? Context { get; init; }

    /// <summary>The managed key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The key version used, when the provider versions keys.</summary>
    [JsonPropertyName("keyVersion")]
    public int? KeyVersion { get; init; }

    /// <summary>The provider.</summary>
    [JsonPropertyName("provider")]
    public required string Provider { get; init; }
}

/// <summary>The body of <c>POST /v2/kms/decrypt</c>.</summary>
public sealed class KmsDecryptRequest
{
    /// <summary>The managed key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The ciphertext, as <see cref="KmsEncryptResult.Ciphertext"/> returned it.</summary>
    [JsonPropertyName("ciphertext")]
    public required string Ciphertext { get; init; }

    /// <summary>The encryption context used to encrypt, or <see langword="null"/>.</summary>
    [JsonPropertyName("context")]
    public IReadOnlyDictionary<string, string>? Context { get; init; }

    /// <summary>The KMS provider, or <see langword="null"/> for the server's default.</summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; init; }
}

/// <summary>A KMS plaintext, as text and as bytes.</summary>
public sealed class KmsDecryptResult
{
    /// <summary>The plaintext's bytes, lowercase hex.</summary>
    [JsonPropertyName("hex")]
    public required string Hex { get; init; }

    /// <summary>The managed key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The plaintext as UTF-8 text (invalid sequences become U+FFFD).</summary>
    [JsonPropertyName("plaintext")]
    public required string Plaintext { get; init; }

    /// <summary>The provider.</summary>
    [JsonPropertyName("provider")]
    public required string Provider { get; init; }

    /// <summary>The plaintext's exact bytes.</summary>
    /// <returns>The decoded <see cref="Hex"/>.</returns>
    public byte[] GetPlaintextBytes() => Convert.FromHexString(Hex);
}

/// <summary>The body of <c>POST /v2/kms/generate-data-key</c>.</summary>
public sealed class KmsGenerateDataKeyRequest
{
    /// <summary>The managed key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The data key spec, or <see langword="null"/> for the provider's default.</summary>
    [JsonPropertyName("keySpec")]
    public string? KeySpec { get; init; }

    /// <summary>The KMS provider, or <see langword="null"/> for the server's default.</summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; init; }
}

/// <summary>A data encryption key, in the clear and wrapped.</summary>
/// <remarks><see cref="Plaintext"/> is key material: never log it.</remarks>
public sealed class KmsDataKey
{
    /// <summary>The wrapped data key, an opaque provider string.</summary>
    [JsonPropertyName("ciphertext")]
    public required string Ciphertext { get; init; }

    /// <summary>The managed key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The data key, lowercase hex.</summary>
    [JsonPropertyName("plaintext")]
    public required string Plaintext { get; init; }

    /// <summary>The provider.</summary>
    [JsonPropertyName("provider")]
    public required string Provider { get; init; }

    /// <summary>The data key's bytes.</summary>
    /// <returns>The decoded <see cref="Plaintext"/>.</returns>
    public byte[] GetPlaintextBytes() => Convert.FromHexString(Plaintext);
}

/// <summary>The body of <c>POST /v2/kms/wrap</c>.</summary>
public sealed class KmsWrapRequest
{
    /// <summary>The managed key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The key bytes to wrap, hex (the encoding unwrap returns).</summary>
    [JsonPropertyName("unwrappedKey")]
    public required string UnwrappedKey { get; init; }

    /// <summary>The wrapping context, or <see langword="null"/>.</summary>
    [JsonPropertyName("context")]
    public IReadOnlyDictionary<string, string>? Context { get; init; }

    /// <summary>The KMS provider, or <see langword="null"/> for the server's default.</summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; init; }
}

/// <summary>A key wrapped by a managed key.</summary>
public sealed class KmsWrapResult
{
    /// <summary>The managed key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The provider.</summary>
    [JsonPropertyName("provider")]
    public required string Provider { get; init; }

    /// <summary>The wrapped key, an opaque provider string.</summary>
    [JsonPropertyName("wrappedKey")]
    public required string WrappedKey { get; init; }
}

/// <summary>The body of <c>POST /v2/kms/unwrap</c>.</summary>
public sealed class KmsUnwrapRequest
{
    /// <summary>The managed key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The wrapped key, as <see cref="KmsWrapResult.WrappedKey"/> returned it.</summary>
    [JsonPropertyName("wrappedKey")]
    public required string WrappedKey { get; init; }

    /// <summary>The wrapping context used, or <see langword="null"/>.</summary>
    [JsonPropertyName("context")]
    public IReadOnlyDictionary<string, string>? Context { get; init; }

    /// <summary>The KMS provider, or <see langword="null"/> for the server's default.</summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; init; }
}

/// <summary>A key unwrapped by a managed key.</summary>
/// <remarks><see cref="UnwrappedKey"/> is key material: never log it.</remarks>
public sealed class KmsUnwrapResult
{
    /// <summary>The managed key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The provider.</summary>
    [JsonPropertyName("provider")]
    public required string Provider { get; init; }

    /// <summary>The key, lowercase hex.</summary>
    [JsonPropertyName("unwrappedKey")]
    public required string UnwrappedKey { get; init; }

    /// <summary>The key's bytes.</summary>
    /// <returns>The decoded <see cref="UnwrappedKey"/>.</returns>
    public byte[] GetUnwrappedKeyBytes() => Convert.FromHexString(UnwrappedKey);
}
