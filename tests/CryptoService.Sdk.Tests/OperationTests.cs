// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using System.Net;
using System.Reflection;
using System.Text.Json;

namespace CryptoService.Sdk.Tests;

/// <summary>Every public method sends its route and decodes its answer.</summary>
public sealed class OperationTests
{
    public static TheoryData<string> Names()
    {
        var names = new TheoryData<string>();
        foreach (Operation operation in Operations.All)
        {
            names.Add(operation.Name);
        }

        return names;
    }

    [Theory]
    [MemberData(nameof(Names))]
    public async Task SendsTheRouteAndDecodesTheAnswer(string name)
    {
        Operation operation = Operations.All.Single(o => o.Name == name);
        var handler = StubHandler.Answer(HttpStatusCode.OK, operation.Answer);
        using CryptoServiceClient client = StubHandler.Client(handler, o => o.Token = "synthetic-token");

        object result = await operation.Invoke(client, CancellationToken.None);

        SentRequest sent = handler.Last;
        Assert.Equal(operation.Method, sent.Method.Method);
        Assert.Equal("http://crypto.test:3105" + operation.Path, sent.Uri!.AbsoluteUri);
        Assert.Equal(operation.Authenticated ? "synthetic-api-key" : null, sent.Header("x-api-key"));
        Assert.Equal(operation.Authenticated ? "Bearer synthetic-token" : null, sent.Header("Authorization"));
        Assert.Equal(operation.Method == "POST", sent.Body is not null);
        if (sent.Body is not null)
        {
            Assert.Equal("application/json", sent.ContentType);
            Assert.Equal(JsonValueKind.Object, JsonDocument.Parse(sent.Body).RootElement.ValueKind);
        }

        Assert.NotNull(result);
        DecodeEveryBinaryField(result);
    }

    [Fact]
    public void EveryOperationIsListedOnce()
    {
        Assert.Equal(Operations.All.Length, Operations.All.Select(o => o.Name).Distinct().Count());

        // 59 v2 operations, the root and three probes, and three optional-request calls made with null.
        Assert.Equal(66, Operations.All.Length);
        Assert.Equal(59, Operations.All.Where(o => !o.Name.EndsWith("(null)", StringComparison.Ordinal)).Count(o => o.Path.StartsWith("/v2/", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task AnOptionalRequestLeftNullSendsAnEmptyObject()
    {
        var handler = StubHandler.Answer(HttpStatusCode.OK, "{\"data\":" + Samples.Of("GenerateKeyResult") + "}");
        using CryptoServiceClient client = StubHandler.Client(handler);

        await client.GenerateKeyPairAsync(cancellationToken: CancellationToken.None);

        Assert.Equal("{}", handler.Last.Body);
    }

    [Fact]
    public async Task OptionalMembersLeftNullAreOmitted()
    {
        var handler = StubHandler.Answer(HttpStatusCode.OK, "{\"data\":" + Samples.Of("SealResult") + "}");
        using CryptoServiceClient client = StubHandler.Client(handler);

        await client.SecretboxSealAsync(new SecretboxSealRequest { Key = "00", Plaintext = "p" }, CancellationToken.None);

        Assert.Equal("""{"key":"00","plaintext":"p"}""", handler.Last.Body);
    }

    [Fact]
    public async Task KmsEncryptStatesTheEncoding()
    {
        var handler = StubHandler.Answer(HttpStatusCode.OK, "{\"data\":" + Samples.Of("KmsEncryptResult") + "}");
        using CryptoServiceClient client = StubHandler.Client(handler);

        await client.KmsEncryptAsync(KmsEncryptRequest.FromBytes("local-1", [0x00, 0xff]), CancellationToken.None);

        Assert.Equal("""{"keyId":"local-1","plaintext":"00ff","encoding":"hex"}""", handler.Last.Body);
    }

    [Fact]
    public async Task ANullRequestIsRefused()
    {
        using CryptoServiceClient client = StubHandler.Client(StubHandler.Answer(HttpStatusCode.OK, "{}"));

        await Assert.ThrowsAsync<ArgumentNullException>(() => client.HashAsync(null!, CancellationToken.None));
    }

    // Every Get*Bytes() method of the result decodes its field.
    private static void DecodeEveryBinaryField(object result)
    {
        IEnumerable<MethodInfo> decoders = result.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name.StartsWith("Get", StringComparison.Ordinal) && m.Name.EndsWith("Bytes", StringComparison.Ordinal));
        foreach (MethodInfo method in decoders)
        {
            byte[] bytes = (byte[])method.Invoke(result, null)!;
            Assert.Equal(new byte[] { 0x00, 0xff }, bytes);
        }
    }
}
