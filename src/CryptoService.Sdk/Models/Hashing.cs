// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using System.Text.Json.Serialization;

namespace CryptoService.Sdk;

/// <summary>The body of <c>POST /v2/hash</c>.</summary>
public sealed class HashRequest
{
    /// <summary>
    /// <c>sha256</c>, <c>sha384</c>, <c>sha512</c>, <c>sha3-256</c>,
    /// <c>sha3-512</c>, <c>blake2b</c> or <c>blake3</c>.
    /// </summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The text to hash, as UTF-8 (1 byte to 10 MiB).</summary>
    [JsonPropertyName("data")]
    public required string Data { get; init; }
}

/// <summary>A digest.</summary>
public sealed class HashResult
{
    /// <summary>The algorithm used.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The digest, lowercase hex.</summary>
    [JsonPropertyName("digest")]
    public required string Digest { get; init; }

    /// <summary>The digest length in bytes.</summary>
    [JsonPropertyName("length")]
    public required int Length { get; init; }

    /// <summary>The digest's bytes.</summary>
    /// <returns>The decoded <see cref="Digest"/>.</returns>
    /// <exception cref="FormatException">The server sent a value that is not valid hex.</exception>
    public byte[] GetDigestBytes() => Convert.FromHexString(Digest);
}

/// <summary>The body of <c>POST /v2/hmac</c>.</summary>
public sealed class HmacRequest
{
    /// <summary><c>sha256</c>, <c>sha384</c>, <c>sha512</c>, <c>sha3-256</c> or <c>sha3-512</c>.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The key, hex.</summary>
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    /// <summary>The message, UTF-8 text.</summary>
    [JsonPropertyName("data")]
    public required string Data { get; init; }
}

/// <summary>An HMAC.</summary>
public sealed class HmacResult
{
    /// <summary>The algorithm used.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The MAC, lowercase hex.</summary>
    [JsonPropertyName("mac")]
    public required string Mac { get; init; }

    /// <summary>The MAC's bytes.</summary>
    /// <returns>The decoded <see cref="Mac"/>.</returns>
    /// <exception cref="FormatException">The server sent a value that is not valid hex.</exception>
    public byte[] GetMacBytes() => Convert.FromHexString(Mac);
}

/// <summary>The body of <c>POST /v2/hmac/verify</c>.</summary>
public sealed class HmacVerifyRequest
{
    /// <summary><c>sha256</c>, <c>sha384</c>, <c>sha512</c>, <c>sha3-256</c> or <c>sha3-512</c>.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The key, hex.</summary>
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    /// <summary>The message, UTF-8 text.</summary>
    [JsonPropertyName("data")]
    public required string Data { get; init; }

    /// <summary>The MAC to check, hex.</summary>
    [JsonPropertyName("mac")]
    public required string Mac { get; init; }
}

/// <summary>The body of <c>POST /v2/kdf</c>.</summary>
public sealed class KdfRequest
{
    /// <summary><c>scrypt</c>, <c>hkdf-sha256</c> or <c>pbkdf2-sha256</c>.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The password or input keying material, UTF-8 text.</summary>
    [JsonPropertyName("password")]
    public required string Password { get; init; }

    /// <summary>The salt, or <see langword="null"/> for a random one.</summary>
    [JsonPropertyName("salt")]
    public string? Salt { get; init; }

    /// <summary>The derived key length in bytes (16 to 64), or <see langword="null"/>.</summary>
    [JsonPropertyName("keyLength")]
    public int? KeyLength { get; init; }

    /// <summary>Algorithm parameters, or <see langword="null"/> for the server's defaults.</summary>
    [JsonPropertyName("params")]
    public KdfParameters? Parameters { get; init; }
}

/// <summary>KDF parameters; the server enforces OWASP floors.</summary>
public sealed class KdfParameters
{
    /// <summary>The scrypt cost; only 131072 is accepted.</summary>
    [JsonPropertyName("N")]
    public int? Cost { get; init; }

    /// <summary>The scrypt block size; only 8 is accepted.</summary>
    [JsonPropertyName("r")]
    public int? BlockSize { get; init; }

    /// <summary>The scrypt parallelism (1 to 4).</summary>
    [JsonPropertyName("p")]
    public int? Parallelism { get; init; }

    /// <summary>The PBKDF2 iterations (600,000 to 1,000,000).</summary>
    [JsonPropertyName("iterations")]
    public int? Iterations { get; init; }

    /// <summary>The HKDF info string.</summary>
    [JsonPropertyName("info")]
    public string? Info { get; init; }
}

/// <summary>A derived key.</summary>
public sealed class KdfResult
{
    /// <summary>The algorithm used.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The derived key, lowercase hex.</summary>
    [JsonPropertyName("derivedKey")]
    public required string DerivedKey { get; init; }

    /// <summary>The derived key length in bytes.</summary>
    [JsonPropertyName("keyLength")]
    public required int KeyLength { get; init; }

    /// <summary>The salt used, lowercase hex.</summary>
    [JsonPropertyName("salt")]
    public required string Salt { get; init; }

    /// <summary>The derived key's bytes.</summary>
    /// <returns>The decoded <see cref="DerivedKey"/>.</returns>
    /// <exception cref="FormatException">The server sent a value that is not valid hex.</exception>
    public byte[] GetDerivedKeyBytes() => Convert.FromHexString(DerivedKey);
}

/// <summary>The body of <c>POST /v2/password/hash</c> (Argon2id).</summary>
public sealed class PasswordHashRequest
{
    /// <summary>The password, UTF-8 text (1 to 1024 characters).</summary>
    [JsonPropertyName("password")]
    public required string Password { get; init; }

    /// <summary>Passes (2 to 10), or <see langword="null"/> for 3.</summary>
    [JsonPropertyName("timeCost")]
    public int? TimeCost { get; init; }

    /// <summary>Memory in KiB (19456 to 262144), or <see langword="null"/> for 64 MiB.</summary>
    [JsonPropertyName("memoryCost")]
    public int? MemoryCost { get; init; }

    /// <summary>Lanes (1 to 8), or <see langword="null"/> for 4.</summary>
    [JsonPropertyName("parallelism")]
    public int? Parallelism { get; init; }
}

/// <summary>Argon2id parameters as the server reports and accepts them.</summary>
public sealed class Argon2Parameters
{
    /// <summary>Memory in KiB.</summary>
    [JsonPropertyName("m")]
    public required int Memory { get; init; }

    /// <summary>Parallelism.</summary>
    [JsonPropertyName("p")]
    public required int Parallelism { get; init; }

    /// <summary>Passes.</summary>
    [JsonPropertyName("t")]
    public required int Time { get; init; }
}

/// <summary>An Argon2id password hash.</summary>
public sealed class PasswordHashResult
{
    /// <summary>Always <c>argon2id</c>.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary>The raw hash, lowercase hex.</summary>
    [JsonPropertyName("hash")]
    public required string Hash { get; init; }

    /// <summary>The parameters used.</summary>
    [JsonPropertyName("params")]
    public required Argon2Parameters Parameters { get; init; }

    /// <summary>The PHC string (<c>$argon2id$v=19$...</c>).</summary>
    [JsonPropertyName("phc")]
    public required string Phc { get; init; }

    /// <summary>The salt, lowercase hex.</summary>
    [JsonPropertyName("salt")]
    public required string Salt { get; init; }
}

/// <summary>The body of <c>POST /v2/password/verify</c>.</summary>
public sealed class PasswordVerifyRequest
{
    /// <summary>The password to check.</summary>
    [JsonPropertyName("password")]
    public required string Password { get; init; }

    /// <summary>The stored raw hash, hex.</summary>
    [JsonPropertyName("hash")]
    public required string Hash { get; init; }

    /// <summary>The stored salt, hex.</summary>
    [JsonPropertyName("salt")]
    public required string Salt { get; init; }

    /// <summary>The stored parameters.</summary>
    [JsonPropertyName("params")]
    public required Argon2Parameters Parameters { get; init; }
}

/// <summary>Whether a password matched.</summary>
public sealed class PasswordVerifyResult
{
    /// <summary>Whether the password matches the hash.</summary>
    [JsonPropertyName("valid")]
    public required bool Valid { get; init; }
}
