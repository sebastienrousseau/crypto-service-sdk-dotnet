// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Text.Json;

namespace CryptoService.Sdk.Tests;

/// <summary>One vector of the conformance kit, as the kit's README describes it.</summary>
internal sealed record KitVector(string Id, JsonElement Auth, JsonElement Request, JsonElement Expect, JsonElement Capture);

/// <summary>One file of the kit: its group and its vectors, run in order.</summary>
internal sealed record KitFile(string Name, string Group, IReadOnlyList<KitVector> Vectors);

/// <summary>The vendored conformance kit in <c>conformance/</c>.</summary>
internal static class Kit
{
    /// <summary>The repository root: the first directory up from the test binary with a <c>conformance/</c>.</summary>
    internal static string RepositoryRoot { get; } = FindRoot();

    /// <summary>The kit directory.</summary>
    internal static string Root => Path.Join(RepositoryRoot, "conformance");

    /// <summary>Every vector file, sorted by name.</summary>
    internal static IReadOnlyList<KitFile> Load() =>
        [.. Directory.EnumerateFiles(Root, "*.json").Order(StringComparer.Ordinal).Select(Read)];

    /// <summary>One vector file by name.</summary>
    internal static KitFile Load(string name) => Read(Path.Join(Root, name));

    private static KitFile Read(string path)
    {
        using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(path));
        JsonElement root = document.RootElement.Clone();
        var vectors = new List<KitVector>();
        foreach (JsonElement v in root.GetProperty("vectors").EnumerateArray())
        {
            vectors.Add(new KitVector(
                v.GetProperty("id").GetString()!,
                v.TryGetProperty("auth", out JsonElement auth) ? auth : default,
                v.GetProperty("request"),
                v.GetProperty("expect"),
                v.TryGetProperty("capture", out JsonElement capture) ? capture : default));
        }

        return new KitFile(Path.GetFileName(path), root.GetProperty("group").GetString()!, vectors);
    }

    private static string FindRoot()
    {
        for (DirectoryInfo? dir = new(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Join(dir.FullName, "conformance", "SHA256SUMS")))
            {
                return dir.FullName;
            }
        }

        throw new InvalidOperationException("conformance/SHA256SUMS not found above " + AppContext.BaseDirectory);
    }
}
