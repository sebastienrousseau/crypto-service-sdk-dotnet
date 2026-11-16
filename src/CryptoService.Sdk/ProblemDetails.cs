// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoService.Sdk;

/// <summary>
/// An RFC 9457 problem (<c>application/problem+json</c>), as crypto-server
/// answers every error.
/// </summary>
public sealed class ProblemDetails
{
    /// <summary>
    /// The problem type: <c>urn:crypto-service:problem:&lt;slug&gt;</c> from
    /// the server, stable and machine-readable; <c>about:blank</c> when the
    /// answer carried no problem.
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>A short summary of the problem type.</summary>
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    /// <summary>The HTTP status code.</summary>
    [JsonPropertyName("status")]
    public required int Status { get; init; }

    /// <summary>An explanation of this occurrence.</summary>
    [JsonPropertyName("detail")]
    public required string Detail { get; init; }

    /// <summary>The request path, without the query string.</summary>
    [JsonPropertyName("instance")]
    public string? Instance { get; init; }

    /// <summary>
    /// The error code from crypto-lib, the key store or a KMS provider, for
    /// example <c>INVALID_HEX</c> or <c>KEY_NOT_FOUND</c>.
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    /// <summary>Each field that failed validation (<c>validation-failed</c>).</summary>
    [JsonPropertyName("errors")]
    public IReadOnlyList<ValidationError>? Errors { get; init; }

    /// <summary>Requests a tenant may make per minute (<c>rate-limited</c>).</summary>
    [JsonPropertyName("limit")]
    public int? Limit { get; init; }

    /// <summary>Seconds until the tenant quota resets (<c>rate-limited</c>).</summary>
    [JsonPropertyName("resetSeconds")]
    public int? ResetSeconds { get; init; }

    /// <summary>Any other member the problem carries, by name.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? Extensions { get; set; }

    /// <summary>
    /// The problem for an answer that carried none: <c>about:blank</c>, the
    /// reason phrase as title, and the status.
    /// </summary>
    /// <param name="status">The HTTP status code.</param>
    /// <param name="reasonPhrase">The HTTP reason phrase, or <see langword="null"/>.</param>
    /// <returns>The <c>about:blank</c> problem.</returns>
    public static ProblemDetails Blank(int status, string? reasonPhrase) =>
        new()
        {
            Type = "about:blank",
            Title = string.IsNullOrEmpty(reasonPhrase) ? "HTTP error" : reasonPhrase,
            Status = status,
            Detail = string.Create(CultureInfo.InvariantCulture, $"The server answered {status} without a problem body"),
        };
}

/// <summary>One field that failed validation.</summary>
public sealed class ValidationError
{
    /// <summary>The field, as a JSON pointer or a body field name.</summary>
    [JsonPropertyName("field")]
    public required string Field { get; init; }

    /// <summary>Why the field was refused.</summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }
}
