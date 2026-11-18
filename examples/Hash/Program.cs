// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau
//
// Hashes "abc" with SHA-256 and BLAKE3 on the server and checks the
// SHA-256 digest against the FIPS 180-2 test vector. The digest arrives
// as lowercase hex; GetDigestBytes() decodes it.
//
//   dotnet run --project examples/Hash

using CryptoService.Examples;
using CryptoService.Sdk;

await using CryptoServiceClient client = Example.CreateClient();

HashResult sha256 = await client.HashAsync(new HashRequest { Algorithm = "sha256", Data = "abc" }).ConfigureAwait(false);
HashResult blake3 = await client.HashAsync(new HashRequest { Algorithm = "blake3", Data = "abc" }).ConfigureAwait(false);

Console.WriteLine($"sha256 {sha256.Digest} ({sha256.GetDigestBytes().Length} bytes)");
Console.WriteLine($"blake3 {blake3.Digest} ({blake3.Length} bytes)");

return sha256.Digest == "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad"
    ? 0
    : Example.Fail("the SHA-256 digest of abc is not the FIPS 180-2 one");
