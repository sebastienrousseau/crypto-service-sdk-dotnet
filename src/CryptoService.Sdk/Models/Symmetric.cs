// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using System.Text.Json.Serialization;

namespace CryptoService.Sdk;

/// <summary>The body of <c>POST /v2/encrypt</c> (XChaCha20-Poly1305).</summary>
public sealed class EncryptRequest
{
    /// <summary>The 256-bit key, 64 hex characters.</summary>
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    /// <summary>The plaintext, UTF-8 text (1 byte to 10 MiB).</summary>
    [JsonPropertyName("plaintext")]
    public required string Plaintext { get; init; }
}

/// <summary>The body of <c>POST /v2/decrypt</c>.</summary>
public sealed class DecryptRequest
{
    /// <summary>The 256-bit key, 64 hex characters.</summary>
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    /// <summary>The ciphertext <see cref="CryptoServiceClient.EncryptAsync"/> returned, base64.</summary>
    [JsonPropertyName("ciphertext")]
    public required string Ciphertext { get; init; }
}

/// <summary>A ciphertext and the algorithm that produced it.</summary>
public sealed class CiphertextResult
{
    /// <summary>The algorithm used.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The ciphertext, standard base64 with padding.</summary>
    [JsonPropertyName("ciphertext")]
    public required string Ciphertext { get; init; }

    /// <summary>The ciphertext's bytes.</summary>
    /// <returns>The decoded <see cref="Ciphertext"/>.</returns>
    /// <exception cref="FormatException">The server sent a value that is not valid base64.</exception>
    public byte[] GetCiphertextBytes() => Convert.FromBase64String(Ciphertext);
}

/// <summary>A recovered plaintext.</summary>
public sealed class PlaintextResult
{
    /// <summary>The plaintext as UTF-8 text (invalid sequences become U+FFFD).</summary>
    [JsonPropertyName("plaintext")]
    public required string Plaintext { get; init; }
}

/// <summary>The body of <c>POST /v2/secretbox/seal</c>.</summary>
public sealed class SecretboxSealRequest
{
    /// <summary>The key, hex.</summary>
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    /// <summary>The plaintext, UTF-8 text.</summary>
    [JsonPropertyName("plaintext")]
    public required string Plaintext { get; init; }

    /// <summary>Additional authenticated data, or <see langword="null"/>.</summary>
    [JsonPropertyName("aad")]
    public string? Aad { get; init; }
}

/// <summary>The body of <c>POST /v2/secretbox/open</c>.</summary>
public sealed class SecretboxOpenRequest
{
    /// <summary>The key, hex.</summary>
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    /// <summary>The sealed box, base64.</summary>
    [JsonPropertyName("ciphertext")]
    public required string Ciphertext { get; init; }

    /// <summary>The additional authenticated data used to seal, or <see langword="null"/>.</summary>
    [JsonPropertyName("aad")]
    public string? Aad { get; init; }
}

/// <summary>A sealed box and the algorithm that sealed it.</summary>
public sealed class SealResult
{
    /// <summary>The algorithm used.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The sealed box, standard base64 with padding.</summary>
    [JsonPropertyName("sealed")]
    public required string Sealed { get; init; }

    /// <summary>The sealed box's bytes.</summary>
    /// <returns>The decoded <see cref="Sealed"/>.</returns>
    /// <exception cref="FormatException">The server sent a value that is not valid base64.</exception>
    public byte[] GetSealedBytes() => Convert.FromBase64String(Sealed);
}

/// <summary>The body of <c>POST /v2/password/encrypt</c> (Argon2id and XChaCha20-Poly1305).</summary>
public sealed class PasswordEncryptRequest
{
    /// <summary>The password.</summary>
    [JsonPropertyName("password")]
    public required string Password { get; init; }

    /// <summary>The plaintext, UTF-8 text.</summary>
    [JsonPropertyName("plaintext")]
    public required string Plaintext { get; init; }
}

/// <summary>A password-encrypted envelope.</summary>
public sealed class PasswordEncryptResult
{
    /// <summary>Always <c>argon2id-xchacha20-poly1305</c>.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The envelope, base64; pass it to <see cref="CryptoServiceClient.PasswordDecryptAsync"/>.</summary>
    [JsonPropertyName("encrypted")]
    public required string Encrypted { get; init; }
}

/// <summary>The body of <c>POST /v2/password/decrypt</c>.</summary>
public sealed class PasswordDecryptRequest
{
    /// <summary>The password.</summary>
    [JsonPropertyName("password")]
    public required string Password { get; init; }

    /// <summary>The envelope <see cref="PasswordEncryptResult.Encrypted"/> holds.</summary>
    [JsonPropertyName("ciphertext")]
    public required string Ciphertext { get; init; }
}

/// <summary>The body of <c>POST /v2/keys/wrap</c> (AES-KW, RFC 3394, or AES-KWP, RFC 5649).</summary>
public sealed class KeyWrapRequest
{
    /// <summary>The key-encryption key, hex.</summary>
    [JsonPropertyName("kek")]
    public required string Kek { get; init; }

    /// <summary>The key to wrap, hex.</summary>
    [JsonPropertyName("keyToWrap")]
    public required string KeyToWrap { get; init; }

    /// <summary><c>aes-kw</c> or <c>aes-kwp</c>, or <see langword="null"/> for the server's default.</summary>
    [JsonPropertyName("algorithm")]
    public string? Algorithm { get; init; }
}

/// <summary>A wrapped key.</summary>
public sealed class KeyWrapResult
{
    /// <summary><c>aes-kw</c> or <c>aes-kwp</c>.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The wrapped key, standard base64 with padding.</summary>
    [JsonPropertyName("wrapped")]
    public required string Wrapped { get; init; }

    /// <summary>The wrapped key's bytes.</summary>
    /// <returns>The decoded <see cref="Wrapped"/>.</returns>
    /// <exception cref="FormatException">The server sent a value that is not valid base64.</exception>
    public byte[] GetWrappedBytes() => Convert.FromBase64String(Wrapped);
}

/// <summary>The body of <c>POST /v2/keys/unwrap</c>.</summary>
public sealed class KeyUnwrapRequest
{
    /// <summary>The key-encryption key, hex.</summary>
    [JsonPropertyName("kek")]
    public required string Kek { get; init; }

    /// <summary>The wrapped key, as <see cref="KeyWrapResult.Wrapped"/> returned it.</summary>
    [JsonPropertyName("wrappedKey")]
    public required string WrappedKey { get; init; }

    /// <summary><c>aes-kw</c> or <c>aes-kwp</c>, or <see langword="null"/> for the server's default.</summary>
    [JsonPropertyName("algorithm")]
    public string? Algorithm { get; init; }
}
