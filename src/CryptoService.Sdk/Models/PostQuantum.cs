// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoService.Sdk;

/// <summary>The body of <c>POST /v2/pq/encapsulate</c> (ML-KEM-768).</summary>
public sealed class MlKemEncapsulateRequest
{
    /// <summary>The recipient's ML-KEM-768 public key, hex.</summary>
    [JsonPropertyName("publicKey")]
    public required string PublicKey { get; init; }
}

/// <summary>An ML-KEM encapsulation.</summary>
/// <remarks><see cref="SharedSecret"/> is key material: never log it.</remarks>
public sealed class MlKemEncapsulateResult
{
    /// <summary>Always <c>ml-kem-768</c>.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The KEM ciphertext, lowercase hex.</summary>
    [JsonPropertyName("ciphertext")]
    public required string Ciphertext { get; init; }

    /// <summary>The shared secret, lowercase hex.</summary>
    [JsonPropertyName("sharedSecret")]
    public required string SharedSecret { get; init; }

    /// <summary>The shared secret's bytes.</summary>
    /// <returns>The decoded <see cref="SharedSecret"/>.</returns>
    public byte[] GetSharedSecretBytes() => Convert.FromHexString(SharedSecret);
}

/// <summary>The body of <c>POST /v2/pq/decapsulate</c>.</summary>
public sealed class MlKemDecapsulateRequest
{
    /// <summary>The server-held ML-KEM-768 key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The KEM ciphertext, hex.</summary>
    [JsonPropertyName("ciphertext")]
    public required string Ciphertext { get; init; }
}

/// <summary>A recovered shared secret.</summary>
/// <remarks><see cref="SharedSecret"/> is key material: never log it.</remarks>
public sealed class SharedSecretResult
{
    /// <summary>The KEM used.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The hybrid combiner, for a hybrid KEM; otherwise <see langword="null"/>.</summary>
    [JsonPropertyName("combiner")]
    public string? Combiner { get; init; }

    /// <summary>The shared secret, lowercase hex.</summary>
    [JsonPropertyName("sharedSecret")]
    public required string SharedSecret { get; init; }

    /// <summary>The shared secret's bytes.</summary>
    /// <returns>The decoded <see cref="SharedSecret"/>.</returns>
    public byte[] GetSharedSecretBytes() => Convert.FromHexString(SharedSecret);
}

/// <summary>A hybrid X25519 and ML-KEM-768 key pair the server holds.</summary>
public sealed class HybridKeyPairResult
{
    /// <summary>Always <c>x25519-ml-kem-768</c>.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The ML-KEM-768 public key, lowercase hex.</summary>
    [JsonPropertyName("mlKemPublicKey")]
    public required string MlKemPublicKey { get; init; }

    /// <summary>The X25519 public key, lowercase hex.</summary>
    [JsonPropertyName("x25519PublicKey")]
    public required string X25519PublicKey { get; init; }
}

/// <summary>The body of <c>POST /v2/pq/hybrid/encapsulate</c>.</summary>
public sealed class HybridEncapsulateRequest
{
    /// <summary>The recipient's ML-KEM-768 public key, hex.</summary>
    [JsonPropertyName("mlKemPublicKey")]
    public required string MlKemPublicKey { get; init; }

    /// <summary>The recipient's X25519 public key, 64 hex characters.</summary>
    [JsonPropertyName("x25519PublicKey")]
    public required string X25519PublicKey { get; init; }
}

/// <summary>A hybrid encapsulation.</summary>
/// <remarks><see cref="SharedSecret"/> is key material: never log it.</remarks>
public sealed class HybridEncapsulateResult
{
    /// <summary>Always <c>x25519-ml-kem-768</c>.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The combiner, <c>crypto-service/hybrid-kem/v2</c>.</summary>
    [JsonPropertyName("combiner")]
    public required string Combiner { get; init; }

    /// <summary>The ML-KEM ciphertext, lowercase hex.</summary>
    [JsonPropertyName("mlKemCiphertext")]
    public required string MlKemCiphertext { get; init; }

    /// <summary>The combined shared secret, lowercase hex.</summary>
    [JsonPropertyName("sharedSecret")]
    public required string SharedSecret { get; init; }

    /// <summary>The ephemeral X25519 public key, lowercase hex.</summary>
    [JsonPropertyName("x25519EphemeralPublic")]
    public required string X25519EphemeralPublic { get; init; }

    /// <summary>The shared secret's bytes.</summary>
    /// <returns>The decoded <see cref="SharedSecret"/>.</returns>
    public byte[] GetSharedSecretBytes() => Convert.FromHexString(SharedSecret);
}

/// <summary>The body of <c>POST /v2/pq/hybrid/decapsulate</c>.</summary>
public sealed class HybridDecapsulateRequest
{
    /// <summary>The server-held hybrid key's ID.</summary>
    [JsonPropertyName("keyId")]
    public required string KeyId { get; init; }

    /// <summary>The ML-KEM ciphertext, hex.</summary>
    [JsonPropertyName("mlKemCiphertext")]
    public required string MlKemCiphertext { get; init; }

    /// <summary>The sender's ephemeral X25519 public key, 64 hex characters.</summary>
    [JsonPropertyName("x25519EphemeralPublic")]
    public required string X25519EphemeralPublic { get; init; }
}

/// <summary>The body of <c>POST /v2/pq/dsa/keygen</c>.</summary>
public sealed class MlDsaKeygenRequest
{
    /// <summary>The ML-DSA level: 44, 65 or 87.</summary>
    [JsonPropertyName("level")]
    public required int Level { get; init; }
}

/// <summary>The body of <c>POST /v2/pq/dsa/verify</c>.</summary>
public sealed class MlDsaVerifyRequest
{
    /// <summary>The ML-DSA level: 44, 65 or 87.</summary>
    [JsonPropertyName("level")]
    public required int Level { get; init; }

    /// <summary>The message, UTF-8 text.</summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }

    /// <summary>The public key, hex.</summary>
    [JsonPropertyName("publicKey")]
    public required string PublicKey { get; init; }

    /// <summary>The signature, hex.</summary>
    [JsonPropertyName("signature")]
    public required string Signature { get; init; }
}

/// <summary>The body of <c>POST /v2/pq/slh-dsa/keygen</c>.</summary>
public sealed class SlhDsaKeygenRequest
{
    /// <summary>The SLH-DSA variant, for example <c>sha2-128f</c> or <c>shake-256s</c>.</summary>
    [JsonPropertyName("variant")]
    public required string Variant { get; init; }
}

/// <summary>The body of <c>POST /v2/pq/slh-dsa/verify</c>.</summary>
public sealed class SlhDsaVerifyRequest
{
    /// <summary>The SLH-DSA variant, for example <c>sha2-128f</c>.</summary>
    [JsonPropertyName("variant")]
    public required string Variant { get; init; }

    /// <summary>The message, UTF-8 text.</summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }

    /// <summary>The public key, hex.</summary>
    [JsonPropertyName("publicKey")]
    public required string PublicKey { get; init; }

    /// <summary>The signature, hex.</summary>
    [JsonPropertyName("signature")]
    public required string Signature { get; init; }
}

/// <summary>The body of <c>POST /v2/stream/iso20022</c>: an ISO 20022 dual-signature envelope to verify.</summary>
public sealed class Iso20022VerifyRequest
{
    /// <summary>The envelope.</summary>
    [JsonPropertyName("envelope")]
    public required Iso20022Envelope Envelope { get; init; }

    /// <summary>The signed payload, any JSON value.</summary>
    [JsonPropertyName("payload")]
    public required JsonElement Payload { get; init; }

    /// <summary>The signer's public keys from the caller's own registry.</summary>
    [JsonPropertyName("trustedKeys")]
    public required Iso20022TrustedKeys TrustedKeys { get; init; }
}

/// <summary>An ISO 20022 dual-signature envelope.</summary>
public sealed class Iso20022Envelope
{
    /// <summary>The classical (Ed25519) signature.</summary>
    [JsonPropertyName("classical")]
    public required Iso20022Signature Classical { get; init; }

    /// <summary><c>sha256</c>, <c>sha384</c> or <c>sha512</c>.</summary>
    [JsonPropertyName("digestAlgorithm")]
    public required string DigestAlgorithm { get; init; }

    /// <summary>The message ID.</summary>
    [JsonPropertyName("messageId")]
    public required string MessageId { get; init; }

    /// <summary><c>pacs.008</c>, <c>pain.001</c>, <c>camt.053</c> or <c>generic</c>.</summary>
    [JsonPropertyName("messageType")]
    public required string MessageType { get; init; }

    /// <summary>The payload digest.</summary>
    [JsonPropertyName("payloadDigest")]
    public required string PayloadDigest { get; init; }

    /// <summary>The post-quantum (ML-DSA) signature.</summary>
    [JsonPropertyName("postQuantum")]
    public required Iso20022Signature PostQuantum { get; init; }

    /// <summary>The signing time.</summary>
    [JsonPropertyName("timestamp")]
    public required string Timestamp { get; init; }
}

/// <summary>One signature of an ISO 20022 envelope.</summary>
public sealed class Iso20022Signature
{
    /// <summary>The signature algorithm.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The public key the envelope names; the server ignores it.</summary>
    [JsonPropertyName("publicKey")]
    public required string PublicKey { get; init; }

    /// <summary>The signature.</summary>
    [JsonPropertyName("signature")]
    public required string Signature { get; init; }
}

/// <summary>The public keys an ISO 20022 envelope is checked against.</summary>
public sealed class Iso20022TrustedKeys
{
    /// <summary>The classical public key.</summary>
    [JsonPropertyName("classicalPublicKey")]
    public required string ClassicalPublicKey { get; init; }

    /// <summary>The post-quantum public key.</summary>
    [JsonPropertyName("postQuantumPublicKey")]
    public required string PostQuantumPublicKey { get; init; }
}

/// <summary>The verdicts on an ISO 20022 envelope.</summary>
public sealed class Iso20022VerifyResult
{
    /// <summary>Whether the classical signature is valid.</summary>
    [JsonPropertyName("classicalValid")]
    public required bool ClassicalValid { get; init; }

    /// <summary>Whether the payload matches its digest.</summary>
    [JsonPropertyName("digestMatches")]
    public required bool DigestMatches { get; init; }

    /// <summary>The message ID.</summary>
    [JsonPropertyName("messageId")]
    public required string MessageId { get; init; }

    /// <summary>Whether the post-quantum signature is valid.</summary>
    [JsonPropertyName("postQuantumValid")]
    public required bool PostQuantumValid { get; init; }

    /// <summary>Whether everything is valid.</summary>
    [JsonPropertyName("valid")]
    public required bool Valid { get; init; }
}
