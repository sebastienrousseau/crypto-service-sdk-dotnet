// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoService.Sdk;

/// <summary>The body of <c>POST /v2/keys/generate</c>.</summary>
public sealed class GenerateKeyRequest
{
    /// <summary>
    /// <c>ed25519</c> (the server's default), <c>x25519</c>, <c>ed448</c>,
    /// <c>x448</c>, <c>p256</c>, <c>p384</c>, <c>ml-kem-512</c>,
    /// <c>ml-kem-768</c>, <c>ml-kem-1024</c>, <c>ml-dsa-44</c>,
    /// <c>ml-dsa-65</c> or <c>ml-dsa-87</c>; <see langword="null"/> for the default.
    /// </summary>
    [JsonPropertyName("algorithm")]
    public string? Algorithm { get; init; }

    /// <summary>JWK-style metadata, or <see langword="null"/>.</summary>
    [JsonPropertyName("metadata")]
    public KeyMetadata? Metadata { get; init; }
}

/// <summary>JWK-style key metadata.</summary>
public sealed class KeyMetadata
{
    /// <summary>An expiry the caller records, or <see langword="null"/>.</summary>
    [JsonPropertyName("exp")]
    public string? Exp { get; init; }

    /// <summary>The key's <c>kid</c>; the server assigns one when absent.</summary>
    [JsonPropertyName("kid")]
    public string? Kid { get; init; }

    /// <summary><c>sig</c> or <c>enc</c>, or <see langword="null"/>.</summary>
    [JsonPropertyName("use")]
    public string? Use { get; init; }
}

/// <summary>A key pair the server generated and holds.</summary>
public sealed class GenerateKeyResult
{
    /// <summary>The key's algorithm.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The key's ID (<c>k_</c> and 22 characters), for the routes that use it.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The key's <c>kid</c>.</summary>
    [JsonPropertyName("kid")]
    public required string Kid { get; init; }

    /// <summary>The key's metadata.</summary>
    [JsonPropertyName("metadata")]
    public required KeyMetadata Metadata { get; init; }

    /// <summary>The public key, lowercase hex.</summary>
    [JsonPropertyName("publicKey")]
    public required string PublicKey { get; init; }

    /// <summary>The public key's bytes.</summary>
    /// <returns>The decoded <see cref="PublicKey"/>.</returns>
    /// <exception cref="FormatException">The server sent a value that is not valid hex.</exception>
    public byte[] GetPublicKeyBytes() => Convert.FromHexString(PublicKey);
}

/// <summary>A key pair the server generated and holds, without metadata.</summary>
public sealed class KeyPairResult
{
    /// <summary>The key's algorithm.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The key's ID (<c>k_</c> and 22 characters).</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The public key, lowercase hex.</summary>
    [JsonPropertyName("publicKey")]
    public required string PublicKey { get; init; }

    /// <summary>The public key's bytes.</summary>
    /// <returns>The decoded <see cref="PublicKey"/>.</returns>
    /// <exception cref="FormatException">The server sent a value that is not valid hex.</exception>
    public byte[] GetPublicKeyBytes() => Convert.FromHexString(PublicKey);
}

/// <summary>A body naming one server-held key.</summary>
public sealed class KeyIdRequest
{
    /// <summary>The key's ID, as a key-generation call returned it.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }
}

/// <summary>An exported key: the private parts of a server-held key pair.</summary>
/// <remarks>Every field is key material: never log it.</remarks>
public sealed class ExportKeyResult
{
    /// <summary>The key's algorithm.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The public key, hex, for a single-algorithm key.</summary>
    [JsonPropertyName("publicKey")]
    public string? PublicKey { get; init; }

    /// <summary>The private key, hex, for a classical key.</summary>
    [JsonPropertyName("privateKey")]
    public string? PrivateKey { get; init; }

    /// <summary>The secret key, hex, for a post-quantum key.</summary>
    [JsonPropertyName("secretKey")]
    public string? SecretKey { get; init; }

    /// <summary>The X25519 public key, hex, for a hybrid key.</summary>
    [JsonPropertyName("x25519PublicKey")]
    public string? X25519PublicKey { get; init; }

    /// <summary>The X25519 private key, hex, for a hybrid key.</summary>
    [JsonPropertyName("x25519PrivateKey")]
    public string? X25519PrivateKey { get; init; }

    /// <summary>The ML-KEM public key, hex, for a hybrid key.</summary>
    [JsonPropertyName("mlKemPublicKey")]
    public string? MlKemPublicKey { get; init; }

    /// <summary>The ML-KEM secret key, hex, for a hybrid key.</summary>
    [JsonPropertyName("mlKemSecretKey")]
    public string? MlKemSecretKey { get; init; }

    /// <summary>Any other hex field a key type carries, by name.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? OtherFields { get; set; }
}

/// <summary>A deleted key.</summary>
public sealed class DeleteKeyResult
{
    /// <summary>Always <see langword="true"/>.</summary>
    [JsonPropertyName("deleted")]
    public required bool Deleted { get; init; }

    /// <summary>The deleted key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }
}

/// <summary>The body of the signing routes that take a server-held key.</summary>
public sealed class SignRequest
{
    /// <summary>The signing key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The message, UTF-8 text.</summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }
}

/// <summary>A signature.</summary>
public sealed class SignResult
{
    /// <summary>The algorithm used.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The signature, lowercase hex.</summary>
    [JsonPropertyName("signature")]
    public required string Signature { get; init; }

    /// <summary>The signature's bytes.</summary>
    /// <returns>The decoded <see cref="Signature"/>.</returns>
    /// <exception cref="FormatException">The server sent a value that is not valid hex.</exception>
    public byte[] GetSignatureBytes() => Convert.FromHexString(Signature);
}

/// <summary>The body of <c>POST /v2/verify</c> (Ed25519).</summary>
public sealed class VerifyRequest
{
    /// <summary>The public key, 64 hex characters.</summary>
    [JsonPropertyName("publicKey")]
    public required string PublicKey { get; init; }

    /// <summary>The message, UTF-8 text.</summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }

    /// <summary>The signature, 128 hex characters.</summary>
    [JsonPropertyName("signature")]
    public required string Signature { get; init; }
}

/// <summary>A verification verdict.</summary>
public sealed class VerifyResult
{
    /// <summary>The algorithm used.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>Whether the signature or MAC is valid.</summary>
    [JsonPropertyName("valid")]
    public required bool Valid { get; init; }
}

/// <summary>The body of <c>POST /v2/stream/sign</c>: a batch of Ed25519 signatures.</summary>
public sealed class StreamSignRequest
{
    /// <summary>The messages to sign.</summary>
    [JsonPropertyName("items")]
    public required IReadOnlyList<StreamSignItem> Items { get; init; }
}

/// <summary>One message to sign in a batch.</summary>
public sealed class StreamSignItem
{
    /// <summary>The caller's ID for the item, echoed in the answer.</summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>The signing key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The message, UTF-8 text.</summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }
}

/// <summary>The signatures of a batch.</summary>
public sealed class StreamSignResult
{
    /// <summary>The number of signatures.</summary>
    [JsonPropertyName("count")]
    public required int Count { get; init; }

    /// <summary>One signature per item, by ID.</summary>
    [JsonPropertyName("signatures")]
    public required IReadOnlyList<StreamSignature> Signatures { get; init; }
}

/// <summary>One signature of a batch.</summary>
public sealed class StreamSignature
{
    /// <summary>Always <c>ed25519</c>.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The item's ID.</summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>The signature, lowercase hex.</summary>
    [JsonPropertyName("signature")]
    public required string Signature { get; init; }
}

/// <summary>The body of <c>POST /v2/stream/verify</c>: a batch of Ed25519 verifications.</summary>
public sealed class StreamVerifyRequest
{
    /// <summary>The signatures to verify.</summary>
    [JsonPropertyName("items")]
    public required IReadOnlyList<StreamVerifyItem> Items { get; init; }
}

/// <summary>One signature to verify in a batch.</summary>
public sealed class StreamVerifyItem
{
    /// <summary>The caller's ID for the item, echoed in the answer.</summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>The message, UTF-8 text.</summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }

    /// <summary>The public key, 64 hex characters.</summary>
    [JsonPropertyName("publicKey")]
    public required string PublicKey { get; init; }

    /// <summary>The signature, hex.</summary>
    [JsonPropertyName("signature")]
    public required string Signature { get; init; }
}

/// <summary>The verdicts of a batch.</summary>
public sealed class StreamVerifyResult
{
    /// <summary>Whether every signature is valid.</summary>
    [JsonPropertyName("allValid")]
    public required bool AllValid { get; init; }

    /// <summary>The number of items.</summary>
    [JsonPropertyName("count")]
    public required int Count { get; init; }

    /// <summary>One verdict per item, by ID.</summary>
    [JsonPropertyName("results")]
    public required IReadOnlyList<StreamVerdict> Results { get; init; }

    /// <summary>The number of valid signatures.</summary>
    [JsonPropertyName("validCount")]
    public required int ValidCount { get; init; }
}

/// <summary>One verdict of a batch.</summary>
public sealed class StreamVerdict
{
    /// <summary>The item's ID.</summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>Whether its signature is valid.</summary>
    [JsonPropertyName("valid")]
    public required bool Valid { get; init; }
}
