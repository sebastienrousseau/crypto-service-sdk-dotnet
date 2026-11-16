// SPDX-License-Identifier: Apache-2.0 OR MIT

using CryptoService.Sdk.Internal;

namespace CryptoService.Sdk;

// Service metadata, probes, the algorithm catalogue and the compliance documents.
public sealed partial class CryptoServiceClient
{
    /// <summary>Gets the service metadata (<c>GET /</c>).</summary>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The answer.</returns>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<ServiceInfo> GetServiceInfoAsync(CancellationToken cancellationToken = default) =>
        GetAsync<ServiceInfo>("/", true, CryptoJsonContext.Default.ServiceInfo, cancellationToken);

    /// <summary>Checks the server is up (<c>GET /health</c>, a public route: no credentials are sent).</summary>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The answer.</returns>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<HealthStatus> HealthAsync(CancellationToken cancellationToken = default) =>
        GetAsync<HealthStatus>("/health", false, CryptoJsonContext.Default.HealthStatus, cancellationToken);

    /// <summary>The liveness probe (<c>GET /live</c>, a public route: no credentials are sent).</summary>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The answer.</returns>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<LiveStatus> LiveAsync(CancellationToken cancellationToken = default) =>
        GetAsync<LiveStatus>("/live", false, CryptoJsonContext.Default.LiveStatus, cancellationToken);

    /// <summary>The readiness probe (<c>GET /ready</c>, a public route: no credentials are sent). A server that is not ready answers 503, raised as <see cref="CryptoApiException"/>.</summary>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The answer.</returns>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<ReadyStatus> ReadyAsync(CancellationToken cancellationToken = default) =>
        GetAsync<ReadyStatus>("/ready", false, CryptoJsonContext.Default.ReadyStatus, cancellationToken);

    /// <summary>Lists the supported algorithms by category (<c>GET /v2/algorithms</c>).</summary>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<AlgorithmCatalog> AlgorithmsAsync(CancellationToken cancellationToken = default) =>
        GetDataAsync("/v2/algorithms", CryptoJsonContext.Default.AlgorithmCatalogEnvelope, cancellationToken);

    /// <summary>Gets the CycloneDX 1.6 Cryptographic Bill of Materials (<c>GET /v2/compliance/cbom</c>), which the route returns unwrapped.</summary>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The answer.</returns>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<CbomDocument> GetCbomAsync(CancellationToken cancellationToken = default) =>
        GetAsync<CbomDocument>("/v2/compliance/cbom", true, CryptoJsonContext.Default.CbomDocument, cancellationToken);

    /// <summary>Gets the DORA Article 9 and 13 self-assessment (<c>GET /v2/compliance/dora</c>).</summary>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result.</returns>
    /// <exception cref="CryptoApiException">The server answered outside 2xx.</exception>
    /// <exception cref="CryptoTransportException">No usable answer arrived.</exception>
    public Task<DoraCompliance> GetDoraComplianceAsync(CancellationToken cancellationToken = default) =>
        GetDataAsync("/v2/compliance/dora", CryptoJsonContext.Default.DoraComplianceEnvelope, cancellationToken);
}
