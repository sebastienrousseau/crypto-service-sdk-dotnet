// SPDX-License-Identifier: Apache-2.0 OR MIT
//
// Ed25519 with a server-held key: generate a key pair (the private key
// stays on the server and the client gets a keyId), sign by keyId, verify
// with the public key, show that a changed message fails, then delete
// the key.
//
//   dotnet run --project examples/SignVerify

using CryptoService.Examples;
using CryptoService.Sdk;

const string Message = "pain.001 batch 42, total 1250.00 EUR";

await using CryptoServiceClient client = Example.CreateClient();

GenerateKeyResult key = await client.GenerateKeyPairAsync(new GenerateKeyRequest { Algorithm = "ed25519" }).ConfigureAwait(false);
Console.WriteLine($"generated {key.Algorithm} key {key.KeyId}");
try
{
    SignResult signature = await client.SignAsync(new SignRequest { KeyId = key.KeyId, Message = Message }).ConfigureAwait(false);
    VerifyResult good = await client.VerifyAsync(new VerifyRequest
    {
        PublicKey = key.PublicKey,
        Message = Message,
        Signature = signature.Signature,
    }).ConfigureAwait(false);
    VerifyResult tampered = await client.VerifyAsync(new VerifyRequest
    {
        PublicKey = key.PublicKey,
        Message = Message + " ",
        Signature = signature.Signature,
    }).ConfigureAwait(false);

    Console.WriteLine($"signature {signature.GetSignatureBytes().Length} bytes; valid {good.Valid}; tampered message valid {tampered.Valid}");
    return good.Valid && !tampered.Valid ? 0 : Example.Fail("the verdicts are wrong");
}
finally
{
    DeleteKeyResult deleted = await client.DeleteKeyAsync(new KeyIdRequest { KeyId = key.KeyId }).ConfigureAwait(false);
    Console.WriteLine($"deleted {deleted.KeyId}: {deleted.Deleted}");
}
