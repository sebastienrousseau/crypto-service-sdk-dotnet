// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace CryptoService.Sdk.Tests;

/// <summary>The vendored kit is the one crypto-service ships, byte for byte.</summary>
public sealed class KitTests
{
    [Fact]
    public void EveryKitFileMatchesItsPinnedHash()
    {
        string[] pinned = File.ReadAllLines(Path.Join(Kit.Root, "SHA256SUMS"));
        var listed = new SortedSet<string>(StringComparer.Ordinal);

        foreach (string[] parts in pinned.Select(line => line.Split("  ", 2)))
        {
            listed.Add(parts[1]);
            byte[] hash = SHA256.HashData(File.ReadAllBytes(Path.Join(Kit.Root, parts[1])));
            Assert.Equal(parts[0], Convert.ToHexStringLower(hash));
        }

        var present = new SortedSet<string>(
            Directory.EnumerateFiles(Kit.Root).Select(Path.GetFileName).Where(f => f != "SHA256SUMS")!,
            StringComparer.Ordinal);
        Assert.Equal(present, listed);
    }

    [Fact]
    public void TheServerRefIsAFullCommitSha()
    {
        string sha = File.ReadAllText(Path.Join(Kit.RepositoryRoot, "ci", "server.ref"), Encoding.UTF8).Trim();

        Assert.Matches("^[0-9a-f]{40}$", sha);
    }

    [Fact]
    public void TheKitHasUniqueIdsPerFile()
    {
        foreach (KitFile file in Kit.Load())
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            Assert.NotEmpty(file.Vectors);
            foreach (KitVector vector in file.Vectors)
            {
                Assert.True(ids.Add(vector.Id), string.Create(CultureInfo.InvariantCulture, $"{file.Name}: {vector.Id}"));
            }
        }
    }
}
