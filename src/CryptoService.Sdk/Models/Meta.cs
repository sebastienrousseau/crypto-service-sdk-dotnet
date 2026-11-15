// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoService.Sdk;

/// <summary>The service metadata <c>GET /</c> answers.</summary>
public sealed class ServiceInfo
{
    /// <summary>The service's description.</summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>More detail about the service.</summary>
    [JsonPropertyName("details")]
    public required string Details { get; init; }

    /// <summary>The service instance's UUID.</summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>The service's licence.</summary>
    [JsonPropertyName("license")]
    public required string License { get; init; }

    /// <summary>The service's name.</summary>
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    /// <summary>The service's home page.</summary>
    [JsonPropertyName("url")]
    public required string Url { get; init; }

    /// <summary>The server's version.</summary>
    [JsonPropertyName("version")]
    public required string Version { get; init; }
}

/// <summary>What <c>GET /health</c> answers.</summary>
public sealed class HealthStatus
{
    /// <summary>Always <c>ok</c> on a 200 answer.</summary>
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    /// <summary>The HTTP status, repeated in the body (200).</summary>
    [JsonPropertyName("statusCode")]
    public required int StatusCode { get; init; }

    /// <summary>Seconds since the server started.</summary>
    [JsonPropertyName("uptime")]
    public required double Uptime { get; init; }
}

/// <summary>What <c>GET /live</c> answers.</summary>
public sealed class LiveStatus
{
    /// <summary>Always <c>alive</c>.</summary>
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    /// <summary>Whole seconds since the server started.</summary>
    [JsonPropertyName("uptime")]
    public required long Uptime { get; init; }
}

/// <summary>What <c>GET /ready</c> answers.</summary>
public sealed class ReadyStatus
{
    /// <summary>Each readiness check by name (<c>accepting</c>, <c>keyStore</c>, ...).</summary>
    [JsonPropertyName("checks")]
    public required IReadOnlyDictionary<string, bool> Checks { get; init; }

    /// <summary><c>ready</c> or <c>not_ready</c>.</summary>
    [JsonPropertyName("status")]
    public required string Status { get; init; }
}

/// <summary>The algorithms the v2 API supports, by category (<c>GET /v2/algorithms</c>).</summary>
public sealed class AlgorithmCatalog
{
    /// <summary>Symmetric encryption algorithms.</summary>
    [JsonPropertyName("encryption")]
    public required IReadOnlyList<string> Encryption { get; init; }

    /// <summary>Hash algorithms.</summary>
    [JsonPropertyName("hashing")]
    public required IReadOnlyList<string> Hashing { get; init; }

    /// <summary>Key derivation functions.</summary>
    [JsonPropertyName("kdf")]
    public required IReadOnlyList<string> Kdf { get; init; }

    /// <summary>Key exchange algorithms.</summary>
    [JsonPropertyName("keyExchange")]
    public required IReadOnlyList<string> KeyExchange { get; init; }

    /// <summary>MAC algorithms.</summary>
    [JsonPropertyName("mac")]
    public required IReadOnlyList<string> Mac { get; init; }

    /// <summary>Post-quantum algorithms.</summary>
    [JsonPropertyName("postQuantum")]
    public required IReadOnlyList<string> PostQuantum { get; init; }

    /// <summary>Signature algorithms.</summary>
    [JsonPropertyName("signing")]
    public required IReadOnlyList<string> Signing { get; init; }

    /// <summary>Any further category a newer server lists, by name.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? OtherCategories { get; set; }
}

/// <summary>
/// The CycloneDX 1.6 Cryptographic Bill of Materials
/// (<c>GET /v2/compliance/cbom</c>), informational input for CRA and DORA
/// work.
/// </summary>
public sealed class CbomDocument
{
    /// <summary>Always <c>CycloneDX</c>.</summary>
    [JsonPropertyName("bomFormat")]
    public required string BomFormat { get; init; }

    /// <summary>The components: one per algorithm or library.</summary>
    [JsonPropertyName("components")]
    public required IReadOnlyList<CbomComponent> Components { get; init; }

    /// <summary>The document metadata, as the server sent it.</summary>
    [JsonPropertyName("metadata")]
    public required JsonElement Metadata { get; init; }

    /// <summary>The document's <c>urn:uuid:</c> serial number.</summary>
    [JsonPropertyName("serialNumber")]
    public required string SerialNumber { get; init; }

    /// <summary>Always <c>1.6</c>.</summary>
    [JsonPropertyName("specVersion")]
    public required string SpecVersion { get; init; }

    /// <summary>The document version.</summary>
    [JsonPropertyName("version")]
    public required int Version { get; init; }

    /// <summary>Any other member of the document, by name.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? Extensions { get; set; }
}

/// <summary>One CBOM component.</summary>
public sealed class CbomComponent
{
    /// <summary>The component's name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Name and value pairs describing the component.</summary>
    [JsonPropertyName("properties")]
    public IReadOnlyList<CbomProperty>? Properties { get; init; }

    /// <summary>The CycloneDX component type.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>The component's version.</summary>
    [JsonPropertyName("version")]
    public string? Version { get; init; }

    /// <summary>Any other member of the component, by name.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? Extensions { get; set; }
}

/// <summary>One CBOM name and value pair.</summary>
public sealed class CbomProperty
{
    /// <summary>The property name.</summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>The property value.</summary>
    [JsonPropertyName("value")]
    public required string Value { get; init; }
}

/// <summary>
/// The DORA cryptographic inventory (<c>GET /v2/compliance/dora</c>): a
/// self-assessment, not a compliance verdict under Regulation (EU)
/// 2022/2554.
/// </summary>
public sealed class DoraCompliance
{
    /// <summary>Primitives in use.</summary>
    [JsonPropertyName("activePrimitivesCount")]
    public required int ActivePrimitivesCount { get; init; }

    /// <summary>Recommended sunset dates by algorithm.</summary>
    [JsonPropertyName("algorithmDeprecationSchedule")]
    public required IReadOnlyList<DoraDeprecation> AlgorithmDeprecationSchedule { get; init; }

    /// <summary>The articles covered: <c>Article 9 &amp; Article 13</c>.</summary>
    [JsonPropertyName("article")]
    public required string Article { get; init; }

    /// <summary>The cryptographic packages and their status.</summary>
    [JsonPropertyName("cryptographicInventory")]
    public required IReadOnlyList<DoraInventoryItem> CryptographicInventory { get; init; }

    /// <summary>What the document is not.</summary>
    [JsonPropertyName("disclaimer")]
    public required string Disclaimer { get; init; }

    /// <summary>Post-quantum primitives in use.</summary>
    [JsonPropertyName("postQuantumPrimitivesCount")]
    public required int PostQuantumPrimitivesCount { get; init; }

    /// <summary>Post-quantum primitives over all primitives.</summary>
    [JsonPropertyName("quantumResistanceRatio")]
    public required double QuantumResistanceRatio { get; init; }

    /// <summary>The regulation: <c>DORA (EU 2022/2554)</c>.</summary>
    [JsonPropertyName("standard")]
    public required string Standard { get; init; }

    /// <summary>The self-assessed status.</summary>
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    /// <summary>When the document was produced.</summary>
    [JsonPropertyName("timestamp")]
    public required DateTimeOffset Timestamp { get; init; }
}

/// <summary>A recommended sunset date for one algorithm.</summary>
public sealed class DoraDeprecation
{
    /// <summary>The algorithm.</summary>
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    /// <summary><c>symmetric</c> or <c>asymmetric</c>.</summary>
    [JsonPropertyName("category")]
    public required string Category { get; init; }

    /// <summary>What to migrate to.</summary>
    [JsonPropertyName("recommendedMigration")]
    public required string RecommendedMigration { get; init; }

    /// <summary>The recommended sunset date.</summary>
    [JsonPropertyName("sunsetDate")]
    public required string SunsetDate { get; init; }
}

/// <summary>One cryptographic package in the DORA inventory.</summary>
public sealed class DoraInventoryItem
{
    /// <summary>The FIPS status the server reports.</summary>
    [JsonPropertyName("fipsCompliance")]
    public required string FipsCompliance { get; init; }

    /// <summary>The package name.</summary>
    [JsonPropertyName("package")]
    public required string Package { get; init; }

    /// <summary><c>implemented</c>, <c>partial</c> or <c>placeholder</c>.</summary>
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    /// <summary>The package version.</summary>
    [JsonPropertyName("version")]
    public required string Version { get; init; }
}
