// SPDX-License-Identifier: Apache-2.0 OR MIT
//
// ML-KEM-768 (FIPS 203) with a server-held key: generate the key pair
// (the client gets a keyId and the public key), encapsulate a shared
// secret to the public key, decapsulate it by keyId, and check both
// sides hold the same secret. The secret is compared, never printed.
//
//   dotnet run --project examples/MlKem

using CryptoService.Examples;
using CryptoService.Sdk;

await using CryptoServiceClient client = Example.CreateClient();

KeyPairResult recipient = await client.MlKemGenerateKeyPairAsync().ConfigureAwait(false);
Console.WriteLine($"generated {recipient.Algorithm} key {recipient.KeyId} ({recipient.GetPublicKeyBytes().Length}-byte public key)");
try
{
    MlKemEncapsulateResult sender = await client.MlKemEncapsulateAsync(new MlKemEncapsulateRequest { PublicKey = recipient.PublicKey }).ConfigureAwait(false);
    SharedSecretResult received = await client.MlKemDecapsulateAsync(new MlKemDecapsulateRequest
    {
        KeyId = recipient.KeyId,
        Ciphertext = sender.Ciphertext,
    }).ConfigureAwait(false);

    bool same = sender.GetSharedSecretBytes().AsSpan().SequenceEqual(received.GetSharedSecretBytes());
    Console.WriteLine($"{sender.Ciphertext.Length / 2}-byte ciphertext; shared secrets match: {same}");
    return same ? 0 : Example.Fail("the decapsulated secret differs");
}
finally
{
    await client.DeleteKeyAsync(new KeyIdRequest { KeyId = recipient.KeyId }).ConfigureAwait(false);
}
