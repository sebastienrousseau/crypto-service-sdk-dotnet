// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using CryptoService.Sdk.Internal;

namespace CryptoService.Sdk;

/// <summary>
/// A typed client for the Crypto Service v2 REST API (crypto-server). It
/// performs no cryptography: every method is one HTTP call to the server,
/// which holds the keys and does the work.
/// </summary>
/// <remarks>
/// <para>
/// One instance is safe to share across threads and concurrent calls:
/// its state is fixed at construction, and it sends through one pooled
/// <see cref="HttpClient"/>. Create it once and reuse it; a client per
/// call wastes connections.
/// </para>
/// <para>
/// Every method is asynchronous and takes a
/// <see cref="CancellationToken"/>; there is no blocking variant, which is
/// the .NET idiom for I/O. A status outside 2xx raises
/// <see cref="CryptoApiException"/> with the server's RFC 9457 problem; no
/// usable answer raises <see cref="CryptoTransportException"/>; a
/// cancelled token raises <see cref="OperationCanceledException"/>.
/// </para>
/// </remarks>
public sealed partial class CryptoServiceClient : IDisposable, IAsyncDisposable
{
    private readonly HttpClient _http;
    private readonly bool _ownsHttp;
    private readonly Transport _transport;

    /// <summary>
    /// Creates a client with its own pooled connection handler
    /// (<see cref="CreateHandler"/>), disposed with the client.
    /// </summary>
    /// <param name="options">The server, credentials, timeouts and retry policy.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The base URL is missing or relative,
    /// or a timeout is not positive.</exception>
    public CryptoServiceClient(CryptoServiceClientOptions options)
        : this(CreateHandler(), options, disposeHandler: true)
    {
    }

    /// <summary>
    /// Creates a client that sends through <paramref name="handler"/>, for
    /// a custom transport or a test.
    /// </summary>
    /// <param name="handler">The HTTP message handler.</param>
    /// <param name="options">The server, credentials, timeouts and retry policy.</param>
    /// <param name="disposeHandler">Whether disposing the client disposes the handler.</param>
    /// <exception cref="ArgumentNullException">An argument is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The base URL is missing or relative,
    /// or a timeout is not positive.</exception>
    public CryptoServiceClient(HttpMessageHandler handler, CryptoServiceClientOptions options, bool disposeHandler = false)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(options);
        var http = new HttpClient(handler, disposeHandler) { Timeout = Timeout.InfiniteTimeSpan };
        try
        {
            _transport = new Transport(http, options);
        }
        catch
        {
            http.Dispose();
            throw;
        }

        _http = http;
        _ownsHttp = true;
    }

    /// <summary>
    /// Creates a client that sends through <paramref name="httpClient"/>,
    /// which the caller owns and disposes: the typed-client pattern of
    /// <c>IHttpClientFactory</c>. The client never changes its settings;
    /// configure its primary handler with <see cref="CreateHandler"/> so it
    /// refuses redirects.
    /// </summary>
    /// <param name="httpClient">The HTTP client to send through.</param>
    /// <param name="options">The server, credentials, timeouts and retry policy.</param>
    /// <exception cref="ArgumentNullException">An argument is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The base URL is missing or relative,
    /// or a timeout is not positive.</exception>
    public CryptoServiceClient(HttpClient httpClient, CryptoServiceClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        _transport = new Transport(httpClient, options);
        _http = httpClient;
    }

    /// <summary>The <c>User-Agent</c> the client sends: <c>crypto-service-sdk-dotnet/&lt;version&gt;</c>.</summary>
    public static string UserAgent => Transport.UserAgent;

    /// <summary>
    /// A connection handler with the client's defaults: pooled keep-alive
    /// connections recycled every five minutes, HTTP/2 when the server
    /// offers it over TLS, no redirects and no cookies. TLS certificates are
    /// always verified.
    /// </summary>
    /// <returns>A new handler; the caller owns it.</returns>
    public static SocketsHttpHandler CreateHandler() =>
        new()
        {
            AllowAutoRedirect = false,
            UseCookies = false,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            EnableMultipleHttp2Connections = true,
            AutomaticDecompression = DecompressionMethods.None,
        };

    /// <summary>
    /// Sends one request, once, and returns the answer whatever its status:
    /// for a route this client has no method for. Never retried.
    /// </summary>
    /// <param name="method">The HTTP method, for example <c>POST</c>.</param>
    /// <param name="path">The path from the base URL, starting with <c>/</c>.</param>
    /// <param name="body">The body's bytes, or <see langword="null"/> for none.</param>
    /// <param name="mediaType">The body's <c>Content-Type</c>; defaults to <c>application/json</c>.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The status, media type and body.</returns>
    /// <exception cref="ArgumentException">The method is empty, or the path
    /// does not start with <c>/</c>.</exception>
    /// <exception cref="CryptoTransportException">No answer arrived.</exception>
    public Task<RawResponse> RequestAsync(
        string method,
        string path,
        ReadOnlyMemory<byte>? body = null,
        string? mediaType = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(method);
        ArgumentException.ThrowIfNullOrEmpty(path);
        if (path[0] != '/')
        {
            throw new ArgumentException("The path must start with '/'.", nameof(path));
        }

        var call = new Call(new HttpMethod(method.ToUpperInvariant()), path, body?.ToArray(), Authenticate: true);
        return _transport.SendRawAsync(call, mediaType, cancellationToken);
    }

    /// <summary>
    /// Disposes the HTTP client when this client created it; an
    /// <see cref="HttpClient"/> passed in is left alone.
    /// </summary>
    public void Dispose()
    {
        if (_ownsHttp)
        {
            _http.Dispose();
        }
    }

    /// <summary>Disposes the client; see <see cref="Dispose"/>.</summary>
    /// <returns>A completed task.</returns>
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    /// <summary>Describes the client by its server; never its credentials.</summary>
    /// <returns>A description safe to log.</returns>
    public override string ToString() => $"CryptoServiceClient {{ BaseUrl = {_transport.BaseUrl} }}";

    private async Task<TResult> PostAsync<TRequest, TResult>(
        string path,
        TRequest request,
        JsonTypeInfo<TRequest> requestInfo,
        JsonTypeInfo<DataEnvelope<TResult>> resultInfo,
        CancellationToken cancellationToken)
        where TRequest : class
    {
        ArgumentNullException.ThrowIfNull(request);
        var call = new Call(HttpMethod.Post, path, JsonSerializer.SerializeToUtf8Bytes(request, requestInfo), Authenticate: true);
        return Unwrap(await _transport.SendAsync(call, resultInfo, cancellationToken).ConfigureAwait(false));
    }

    private Task<TResult> PostOptionalAsync<TRequest, TResult>(
        string path,
        TRequest? request,
        JsonTypeInfo<TRequest> requestInfo,
        JsonTypeInfo<DataEnvelope<TResult>> resultInfo,
        CancellationToken cancellationToken)
        where TRequest : class =>
        request is null
            ? PostAsync(path, EmptyRequest.Instance, CryptoJsonContext.Default.EmptyRequest, resultInfo, cancellationToken)
            : PostAsync(path, request, requestInfo, resultInfo, cancellationToken);

    private Task<TResult> GetAsync<TResult>(
        string path,
        bool authenticate,
        JsonTypeInfo<TResult> resultInfo,
        CancellationToken cancellationToken) =>
        _transport.SendAsync(new Call(HttpMethod.Get, path, null, authenticate), resultInfo, cancellationToken);

    private async Task<TResult> GetDataAsync<TResult>(
        string path,
        JsonTypeInfo<DataEnvelope<TResult>> resultInfo,
        CancellationToken cancellationToken)
    {
        return Unwrap(await GetAsync(path, true, resultInfo, cancellationToken).ConfigureAwait(false));
    }

    // A generic member's nullability is not known to the serializer, so a
    // "data": null is refused here.
    private static TResult Unwrap<TResult>(DataEnvelope<TResult> envelope) =>
        envelope.Data ?? throw new CryptoTransportException("The server's 2xx answer is not the JSON the operation documents.");
}
