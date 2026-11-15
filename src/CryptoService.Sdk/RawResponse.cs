// SPDX-License-Identifier: Apache-2.0 OR MIT

namespace CryptoService.Sdk;

/// <summary>
/// An answer as it arrived, whatever its status: what
/// <see cref="CryptoServiceClient.RequestAsync"/> returns.
/// </summary>
public sealed class RawResponse
{
    private readonly byte[] _body;

    internal RawResponse(int statusCode, string? mediaType, byte[] body)
    {
        StatusCode = statusCode;
        MediaType = mediaType;
        _body = body;
    }

    /// <summary>The HTTP status code.</summary>
    public int StatusCode { get; }

    /// <summary>The <c>Content-Type</c> without parameters, or <see langword="null"/>.</summary>
    public string? MediaType { get; }

    /// <summary>The body's bytes; empty when there was none.</summary>
    public ReadOnlyMemory<byte> Body => _body;
}
