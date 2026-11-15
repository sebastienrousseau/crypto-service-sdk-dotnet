// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Text.Json.Serialization;

namespace CryptoService.Sdk.Internal;

/// <summary>The <c>{ "data": ... }</c> wrapper of every v2 answer except the CBOM.</summary>
/// <typeparam name="T">The wrapped result.</typeparam>
internal sealed class DataEnvelope<T>
{
    /// <summary>The result.</summary>
    [JsonPropertyName("data")]
    public required T Data { get; init; }
}

/// <summary>The empty JSON object, the body of the parameterless key-generation routes.</summary>
internal sealed class EmptyRequest
{
    /// <summary>The one instance.</summary>
    internal static readonly EmptyRequest Instance = new();
}
