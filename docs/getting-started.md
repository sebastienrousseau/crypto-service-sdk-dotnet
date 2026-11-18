<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->
<!-- SPDX-FileCopyrightText: 2026 Sebastien Rousseau -->

# Getting started

## Install

```sh
dotnet add package CryptoService.Sdk --version 0.0.41
```

The package targets `net10.0` and has no dependency beyond the base
class library. It is on
[NuGet](https://www.nuget.org/packages/CryptoService.Sdk) from 0.0.40;
the snippet names the version `main` prepares, so use the newest
published one until it is out, or reference
`src/CryptoService.Sdk/CryptoService.Sdk.csproj` from a clone.

## A server

The SDK calls crypto-server; [Development](development.md#a-server)
shows how to build and start the pinned one on loopback. The examples
below read its address from `CRYPTO_SERVICE_URL` and its API key from
`CRYPTO_SERVICE_API_KEY`.

## The first call

```csharp
using CryptoService.Sdk;

await using var client = new CryptoServiceClient(new CryptoServiceClientOptions
{
    BaseUrl = new Uri(Environment.GetEnvironmentVariable("CRYPTO_SERVICE_URL") ?? "http://127.0.0.1:3000"),
    ApiKey = Environment.GetEnvironmentVariable("CRYPTO_SERVICE_API_KEY"),
});

KeyPairResult recipient = await client.MlKemGenerateKeyPairAsync();
MlKemEncapsulateResult sender = await client.MlKemEncapsulateAsync(
    new MlKemEncapsulateRequest { PublicKey = recipient.PublicKey });
SharedSecretResult received = await client.MlKemDecapsulateAsync(
    new MlKemDecapsulateRequest { KeyId = recipient.KeyId, Ciphertext = sender.Ciphertext });
```

The secret key never leaves the server: the client holds a `keyId`.

## With IHttpClientFactory

The client takes an `HttpClient`, so it registers as a typed client.
Give the factory the SDK's handler so redirects stay refused:

```csharp
services.AddSingleton(new CryptoServiceClientOptions { BaseUrl = new Uri("https://crypto.internal.example"), ApiKey = key });
services.AddHttpClient<CryptoServiceClient>()
    .ConfigurePrimaryHttpMessageHandler(CryptoServiceClient.CreateHandler);
```

`IHttpClientFactory` lives in `Microsoft.Extensions.Http`, a package of
your application; the SDK does not depend on it.

## Errors

```csharp
try
{
    await client.SignAsync(new SignRequest { KeyId = "k_AAAAAAAAAAAAAAAAAAAAAA", Message = "x" });
}
catch (CryptoApiException error)
{
    // error.StatusCode == 404, error.Problem.Type == "urn:crypto-service:problem:key-not-found"
}
catch (CryptoTransportException error) when (error.TimedOut)
{
    // the attempt timed out, or the deadline passed
}
```

See [ADR 0003](adr/0003-error-model.md) for the full model.
