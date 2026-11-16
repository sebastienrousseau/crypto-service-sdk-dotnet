// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Text.Json.Serialization;

namespace CryptoService.Sdk;

/// <summary>The body of <c>POST /v2/opaque/register/init</c>.</summary>
public sealed class OpaqueRegisterInitRequest
{
    /// <summary>The credential identifier (1 to 256 characters).</summary>
    [JsonPropertyName("credentialIdentifier")]
    public required string CredentialIdentifier { get; init; }

    /// <summary>The client's blinded registration request, hex.</summary>
    [JsonPropertyName("request")]
    public required string Request { get; init; }

    /// <summary><c>P256-SHA256</c> or <c>ristretto255-SHA512</c>, or <see langword="null"/>.</summary>
    [JsonPropertyName("suite")]
    public string? Suite { get; init; }
}

/// <summary>The server's registration response.</summary>
public sealed class OpaqueRegisterInitResult
{
    /// <summary>The registration response, lowercase hex.</summary>
    [JsonPropertyName("response")]
    public required string Response { get; init; }
}

/// <summary>The body of <c>POST /v2/opaque/register/finish</c>.</summary>
public sealed class OpaqueRegisterFinishRequest
{
    /// <summary>The credential identifier.</summary>
    [JsonPropertyName("credentialIdentifier")]
    public required string CredentialIdentifier { get; init; }

    /// <summary>The client's registration record, hex.</summary>
    [JsonPropertyName("record")]
    public required string Record { get; init; }

    /// <summary><c>P256-SHA256</c> or <c>ristretto255-SHA512</c>, or <see langword="null"/>.</summary>
    [JsonPropertyName("suite")]
    public string? Suite { get; init; }
}

/// <summary>A stored registration.</summary>
public sealed class OpaqueRegisterFinishResult
{
    /// <summary>The credential identifier.</summary>
    [JsonPropertyName("credentialIdentifier")]
    public required string CredentialIdentifier { get; init; }

    /// <summary>Always <c>registered</c>.</summary>
    [JsonPropertyName("status")]
    public required string Status { get; init; }
}

/// <summary>The body of <c>POST /v2/opaque/login/init</c>.</summary>
public sealed class OpaqueLoginInitRequest
{
    /// <summary>The credential identifier.</summary>
    [JsonPropertyName("credentialIdentifier")]
    public required string CredentialIdentifier { get; init; }

    /// <summary>The client's KE1 message, hex.</summary>
    [JsonPropertyName("ke1")]
    public required string Ke1 { get; init; }

    /// <summary><c>P256-SHA256</c> or <c>ristretto255-SHA512</c>, or <see langword="null"/>.</summary>
    [JsonPropertyName("suite")]
    public string? Suite { get; init; }
}

/// <summary>The server's KE2 message and the login session.</summary>
public sealed class OpaqueLoginInitResult
{
    /// <summary>The KE2 message, lowercase hex.</summary>
    [JsonPropertyName("ke2")]
    public required string Ke2 { get; init; }

    /// <summary>The transient login session's ID.</summary>
    [JsonPropertyName("sessionId")]
    public required string SessionId { get; init; }
}

/// <summary>The body of <c>POST /v2/opaque/login/finish</c>.</summary>
public sealed class OpaqueLoginFinishRequest
{
    /// <summary>The client's KE3 message, hex.</summary>
    [JsonPropertyName("ke3")]
    public required string Ke3 { get; init; }

    /// <summary>The login session's ID.</summary>
    [JsonPropertyName("sessionId")]
    public required string SessionId { get; init; }
}

/// <summary>The session key of a finished login.</summary>
/// <remarks><see cref="SessionKey"/> is key material: never log it.</remarks>
public sealed class OpaqueLoginFinishResult
{
    /// <summary>The shared session key, lowercase hex.</summary>
    [JsonPropertyName("sessionKey")]
    public required string SessionKey { get; init; }
}
