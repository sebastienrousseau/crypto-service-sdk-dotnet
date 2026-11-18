// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau
//
// An authenticated-encryption round trip with XChaCha20-Poly1305 on the
// server: encrypt, then decrypt, under a 256-bit key sent as hex; then
// the same with a secretbox bound to additional authenticated data. The
// key here is a synthetic constant: a real one comes from a key store,
// never from source code.
//
//   dotnet run --project examples/AeadRoundTrip

using CryptoService.Examples;
using CryptoService.Sdk;

const string Key = "000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f";
const string Message = "settlement batch 42";

await using CryptoServiceClient client = Example.CreateClient();

CiphertextResult sealedText = await client.EncryptAsync(new EncryptRequest { Key = Key, Plaintext = Message }).ConfigureAwait(false);
PlaintextResult opened = await client.DecryptAsync(new DecryptRequest { Key = Key, Ciphertext = sealedText.Ciphertext }).ConfigureAwait(false);
Console.WriteLine($"{sealedText.Algorithm}: {sealedText.GetCiphertextBytes().Length} ciphertext bytes, round trip {(opened.Plaintext == Message ? "ok" : "FAILED")}");

SealResult box = await client.SecretboxSealAsync(new SecretboxSealRequest { Key = Key, Plaintext = Message, Aad = "batch-42" }).ConfigureAwait(false);
string unboxed = await client.SecretboxOpenAsync(new SecretboxOpenRequest { Key = Key, Ciphertext = box.Sealed, Aad = "batch-42" }).ConfigureAwait(false);
Console.WriteLine($"secretbox with AAD: round trip {(unboxed == Message ? "ok" : "FAILED")}");

return opened.Plaintext == Message && unboxed == Message ? 0 : Example.Fail("a round trip changed the message");
