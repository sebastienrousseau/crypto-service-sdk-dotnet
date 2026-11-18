// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace CryptoService.Sdk.Internal;

/// <summary>Reads answers: a 2xx body into its typed result, an error body into a problem.</summary>
internal static class Decoding
{
    /// <summary>
    /// The most of an error body read. A longer one is not a problem the
    /// server sent, and becomes an <c>about:blank</c> problem.
    /// </summary>
    internal const int MaxErrorBodyBytes = 64 * 1024;

    /// <summary>Decodes a 2xx body as it streams in.</summary>
    /// <exception cref="CryptoTransportException">The body is not the expected JSON.</exception>
    internal static async Task<T> ReadSuccessAsync<T>(HttpContent content, JsonTypeInfo<T> info, CancellationToken cancellationToken)
    {
        Stream stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using (stream.ConfigureAwait(false))
        {
            try
            {
                return Present(await JsonSerializer.DeserializeAsync(stream, info, cancellationToken).ConfigureAwait(false));
            }
            catch (JsonException ex)
            {
                throw Undecodable(ex);
            }
        }
    }

    /// <summary>Decodes a 2xx body held in memory: the success-response fuzz target.</summary>
    /// <exception cref="CryptoTransportException">The body is not the expected JSON.</exception>
    internal static T DecodeSuccess<T>(ReadOnlySpan<byte> body, JsonTypeInfo<T> info)
    {
        try
        {
            return Present(JsonSerializer.Deserialize(body, info));
        }
        catch (JsonException ex)
        {
            throw Undecodable(ex);
        }
    }

    /// <summary>Reads an error answer into the exception the call raises.</summary>
    internal static async Task<CryptoApiException> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        byte[] body = await ReadCappedAsync(response.Content, MaxErrorBodyBytes, cancellationToken).ConfigureAwait(false);
        int status = (int)response.StatusCode;
        return new CryptoApiException(status, ParseProblem(body, status, response.ReasonPhrase));
    }

    /// <summary>
    /// The problem an error body holds: the problem-body fuzz target. A body
    /// that is not a JSON problem (HTML from a proxy, an empty body, JSON of
    /// another shape) becomes an <c>about:blank</c> problem for the status.
    /// Never throws.
    /// </summary>
    internal static ProblemDetails ParseProblem(ReadOnlySpan<byte> body, int status, string? reasonPhrase)
    {
        try
        {
            return JsonSerializer.Deserialize(body, CryptoJsonContext.Default.ProblemDetails)
                ?? ProblemDetails.Blank(status, reasonPhrase);
        }
        catch (JsonException)
        {
            return ProblemDetails.Blank(status, reasonPhrase);
        }
    }

    /// <summary>Reads at most <paramref name="cap"/> bytes; a longer body reads as empty.</summary>
    internal static async Task<byte[]> ReadCappedAsync(HttpContent content, int cap, CancellationToken cancellationToken)
    {
        Stream stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using (stream.ConfigureAwait(false))
        {
            byte[] buffer = new byte[cap + 1];
            int total = 0;
            int read;
            while (total < buffer.Length
                && (read = await stream.ReadAsync(buffer.AsMemory(total), cancellationToken).ConfigureAwait(false)) > 0)
            {
                total += read;
            }

            return total > cap ? [] : buffer[..total];
        }
    }

    private static T Present<T>(T? value) => value ?? throw Undecodable(null);

    private static CryptoTransportException Undecodable(Exception? cause) =>
        new("The server's 2xx answer is not the JSON the operation documents.", cause);
}
