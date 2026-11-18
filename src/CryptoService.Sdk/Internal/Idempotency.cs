// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using System.Text.RegularExpressions;

namespace CryptoService.Sdk.Internal;

/// <summary>
/// Which calls are safe to repeat after the server may have acted: the
/// retry-safety table of the TypeScript reference SDK (<c>request.ts</c>),
/// shared by every Crypto Service SDK.
/// </summary>
internal static partial class Idempotency
{
    /// <summary>
    /// Whether repeating <paramref name="method"/> <paramref name="path"/>
    /// cannot create or delete anything twice. Every <c>GET</c> is; a
    /// <c>POST</c> is unless it generates or deletes a key, creates a KMS key
    /// or registers an OPAQUE credential.
    /// </summary>
    internal static bool IsIdempotent(HttpMethod method, string path) =>
        method == HttpMethod.Get || !StatefulRoute().IsMatch(path);

    [GeneratedRegex("/(keygen|keys/generate|keys/delete|kms/create-key|opaque/register/)", RegexOptions.CultureInvariant)]
    private static partial Regex StatefulRoute();
}
