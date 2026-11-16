// SPDX-License-Identifier: Apache-2.0 OR MIT
//
// How a call fails, and what each failure carries:
//
// - CryptoApiException: the server answered outside 2xx. Problem is its
//   RFC 9457 problem: a stable type URN, a title, the status, a detail,
//   and for a validation failure the fields in Errors.
// - CryptoTransportException: no usable answer (connection refused, a
//   timeout, a deadline); TimedOut says which.
//
// Both derive from CryptoServiceException. Their messages carry no
// credential, key or body, so they are safe to log.
//
//   dotnet run --project examples/ErrorHandling

using CryptoService.Examples;
using CryptoService.Sdk;

int failures = 0;
await using CryptoServiceClient client = Example.CreateClient();

try
{
    await client.HashAsync(new HashRequest { Algorithm = "md5", Data = "abc" }).ConfigureAwait(false);
    failures += Example.Fail("md5 was accepted");
}
catch (CryptoApiException error) when (error.Problem.Type == "urn:crypto-service:problem:validation-failed")
{
    Console.WriteLine($"{error.StatusCode} {error.Problem.Title}");
    foreach (ValidationError field in error.Problem.Errors ?? [])
    {
        Console.WriteLine($"  {field.Field}: {field.Message}");
    }
}

try
{
    await client.SignAsync(new SignRequest { KeyId = "k_AAAAAAAAAAAAAAAAAAAAAA", Message = "x" }).ConfigureAwait(false);
    failures += Example.Fail("an unknown key signed");
}
catch (CryptoApiException error)
{
    Console.WriteLine($"{error.StatusCode} {error.Problem.Type} (code {error.Problem.Code})");
    failures += error.StatusCode == 404 ? 0 : Example.Fail("an unknown key is not a 404");
}

await using (CryptoServiceClient wrongKey = Example.CreateClient(o => o.ApiKey = "not-the-server-key"))
{
    try
    {
        await wrongKey.AlgorithmsAsync().ConfigureAwait(false);
        failures += Example.Fail("a wrong API key was accepted");
    }
    catch (CryptoApiException error)
    {
        Console.WriteLine($"{error.StatusCode} {error.Problem.Title}: {error.Message}");
    }
}

// Port 9 (discard) has no listener on a loopback in practice.
await using (CryptoServiceClient nowhere = Example.CreateClient(o => o.BaseUrl = new Uri("http://127.0.0.1:9")))
{
    try
    {
        await nowhere.HealthAsync().ConfigureAwait(false);
        failures += Example.Fail("a closed port answered");
    }
    catch (CryptoTransportException error)
    {
        Console.WriteLine($"transport error, timed out: {error.TimedOut}: {error.Message}");
    }
}

return failures;
