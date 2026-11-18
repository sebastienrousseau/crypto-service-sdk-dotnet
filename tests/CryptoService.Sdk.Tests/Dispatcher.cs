// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using CryptoService.Sdk.Internal;

namespace CryptoService.Sdk.Tests;

/// <summary>A typed call ready to send, answering the server's body as JSON (re-encoded from the typed result).</summary>
internal delegate Task<JsonElement> TypedCall(CryptoServiceClient client, CancellationToken cancellationToken);

/// <summary>
/// Routes each kit request to the public method for its route. A body the
/// method's request type cannot hold (a vector that sends invalid input on
/// purpose) has no typed call, and the runner sends it raw.
/// </summary>
internal static class Dispatcher
{
    private static readonly Dictionary<string, Func<JsonElement, TypedCall?>> Routes = new(StringComparer.Ordinal)
    {
        ["GET /"] = GetBare(Ctx.ServiceInfo, (c, ct) => c.GetServiceInfoAsync(ct)),
        ["GET /health"] = GetBare(Ctx.HealthStatus, (c, ct) => c.HealthAsync(ct)),
        ["GET /live"] = GetBare(Ctx.LiveStatus, (c, ct) => c.LiveAsync(ct)),
        ["GET /ready"] = GetBare(Ctx.ReadyStatus, (c, ct) => c.ReadyAsync(ct)),
        ["GET /v2/algorithms"] = GetData(Ctx.AlgorithmCatalogEnvelope, (c, ct) => c.AlgorithmsAsync(ct)),
        ["GET /v2/compliance/cbom"] = GetBare(Ctx.CbomDocument, (c, ct) => c.GetCbomAsync(ct)),
        ["GET /v2/compliance/dora"] = GetData(Ctx.DoraComplianceEnvelope, (c, ct) => c.GetDoraComplianceAsync(ct)),
        ["POST /v2/hash"] = Post(Ctx.HashRequest, Ctx.HashResultEnvelope, (c, r, ct) => c.HashAsync(r, ct)),
        ["POST /v2/hmac"] = Post(Ctx.HmacRequest, Ctx.HmacResultEnvelope, (c, r, ct) => c.MacAsync(r, ct)),
        ["POST /v2/hmac/verify"] = Post(Ctx.HmacVerifyRequest, Ctx.VerifyResultEnvelope, (c, r, ct) => c.MacVerifyAsync(r, ct)),
        ["POST /v2/kdf"] = Post(Ctx.KdfRequest, Ctx.KdfResultEnvelope, (c, r, ct) => c.KdfAsync(r, ct)),
        ["POST /v2/password/hash"] = Post(Ctx.PasswordHashRequest, Ctx.PasswordHashResultEnvelope, (c, r, ct) => c.PasswordHashAsync(r, ct)),
        ["POST /v2/password/verify"] = Post(Ctx.PasswordVerifyRequest, Ctx.PasswordVerifyResultEnvelope, (c, r, ct) => c.PasswordVerifyAsync(r, ct)),
        ["POST /v2/encrypt"] = Post(Ctx.EncryptRequest, Ctx.CiphertextResultEnvelope, (c, r, ct) => c.EncryptAsync(r, ct)),
        ["POST /v2/decrypt"] = Post(Ctx.DecryptRequest, Ctx.PlaintextResultEnvelope, (c, r, ct) => c.DecryptAsync(r, ct)),
        ["POST /v2/secretbox/seal"] = Post(Ctx.SecretboxSealRequest, Ctx.SealResultEnvelope, (c, r, ct) => c.SecretboxSealAsync(r, ct)),
        ["POST /v2/secretbox/open"] = Post(Ctx.SecretboxOpenRequest, Ctx.StringEnvelope, (c, r, ct) => c.SecretboxOpenAsync(r, ct)),
        ["POST /v2/password/encrypt"] = Post(Ctx.PasswordEncryptRequest, Ctx.PasswordEncryptResultEnvelope, (c, r, ct) => c.PasswordEncryptAsync(r, ct)),
        ["POST /v2/password/decrypt"] = Post(Ctx.PasswordDecryptRequest, Ctx.StringEnvelope, (c, r, ct) => c.PasswordDecryptAsync(r, ct)),
        ["POST /v2/keys/wrap"] = Post(Ctx.KeyWrapRequest, Ctx.KeyWrapResultEnvelope, (c, r, ct) => c.KeyWrapAsync(r, ct)),
        ["POST /v2/keys/unwrap"] = Post(Ctx.KeyUnwrapRequest, Ctx.StringEnvelope, (c, r, ct) => c.KeyUnwrapAsync(r, ct)),
        ["POST /v2/keys/generate"] = Post(Ctx.GenerateKeyRequest, Ctx.GenerateKeyResultEnvelope, (c, r, ct) => c.GenerateKeyPairAsync(r, ct)),
        ["POST /v2/keys/export"] = Post(Ctx.KeyIdRequest, Ctx.ExportKeyResultEnvelope, (c, r, ct) => c.ExportKeyAsync(r, ct)),
        ["POST /v2/keys/delete"] = Post(Ctx.KeyIdRequest, Ctx.DeleteKeyResultEnvelope, (c, r, ct) => c.DeleteKeyAsync(r, ct)),
        ["POST /v2/sign"] = Post(Ctx.SignRequest, Ctx.SignResultEnvelope, (c, r, ct) => c.SignAsync(r, ct)),
        ["POST /v2/verify"] = Post(Ctx.VerifyRequest, Ctx.VerifyResultEnvelope, (c, r, ct) => c.VerifyAsync(r, ct)),
        ["POST /v2/stream/sign"] = Post(Ctx.StreamSignRequest, Ctx.StreamSignResultEnvelope, (c, r, ct) => c.StreamSignAsync(r, ct)),
        ["POST /v2/stream/verify"] = Post(Ctx.StreamVerifyRequest, Ctx.StreamVerifyResultEnvelope, (c, r, ct) => c.StreamVerifyAsync(r, ct)),
        ["POST /v2/stream/iso20022"] = Post(Ctx.Iso20022VerifyRequest, Ctx.Iso20022VerifyResultEnvelope, (c, r, ct) => c.StreamIso20022Async(r, ct)),
        ["POST /v2/pq/keygen"] = PostEmpty(Ctx.KeyPairResultEnvelope, (c, ct) => c.MlKemGenerateKeyPairAsync(ct)),
        ["POST /v2/pq/encapsulate"] = Post(Ctx.MlKemEncapsulateRequest, Ctx.MlKemEncapsulateResultEnvelope, (c, r, ct) => c.MlKemEncapsulateAsync(r, ct)),
        ["POST /v2/pq/decapsulate"] = Post(Ctx.MlKemDecapsulateRequest, Ctx.SharedSecretResultEnvelope, (c, r, ct) => c.MlKemDecapsulateAsync(r, ct)),
        ["POST /v2/pq/hybrid/keygen"] = PostEmpty(Ctx.HybridKeyPairResultEnvelope, (c, ct) => c.PqGenerateKeyPairAsync(ct)),
        ["POST /v2/pq/hybrid/encapsulate"] = Post(Ctx.HybridEncapsulateRequest, Ctx.HybridEncapsulateResultEnvelope, (c, r, ct) => c.PqEncapsulateAsync(r, ct)),
        ["POST /v2/pq/hybrid/decapsulate"] = Post(Ctx.HybridDecapsulateRequest, Ctx.SharedSecretResultEnvelope, (c, r, ct) => c.PqDecapsulateAsync(r, ct)),
        ["POST /v2/pq/dsa/keygen"] = Post(Ctx.MlDsaKeygenRequest, Ctx.KeyPairResultEnvelope, (c, r, ct) => c.PqSignKeygenAsync(r, ct)),
        ["POST /v2/pq/dsa/sign"] = Post(Ctx.SignRequest, Ctx.SignResultEnvelope, (c, r, ct) => c.PqSignAsync(r, ct)),
        ["POST /v2/pq/dsa/verify"] = Post(Ctx.MlDsaVerifyRequest, Ctx.VerifyResultEnvelope, (c, r, ct) => c.PqVerifyAsync(r, ct)),
        ["POST /v2/pq/slh-dsa/keygen"] = Post(Ctx.SlhDsaKeygenRequest, Ctx.KeyPairResultEnvelope, (c, r, ct) => c.PqHashSignKeygenAsync(r, ct)),
        ["POST /v2/pq/slh-dsa/sign"] = Post(Ctx.SignRequest, Ctx.SignResultEnvelope, (c, r, ct) => c.PqHashSignAsync(r, ct)),
        ["POST /v2/pq/slh-dsa/verify"] = Post(Ctx.SlhDsaVerifyRequest, Ctx.VerifyResultEnvelope, (c, r, ct) => c.PqHashVerifyAsync(r, ct)),
        ["POST /v2/sealedbox/seal"] = Post(Ctx.SealedboxSealRequest, Ctx.SealResultEnvelope, (c, r, ct) => c.SealedboxSealAsync(r, ct)),
        ["POST /v2/sealedbox/open"] = Post(Ctx.SealedboxOpenRequest, Ctx.StringEnvelope, (c, r, ct) => c.SealedboxOpenAsync(r, ct)),
        ["POST /v2/sealedbox/seal-pq"] = Post(Ctx.SealedboxSealPqRequest, Ctx.SealResultEnvelope, (c, r, ct) => c.SealedboxSealPqAsync(r, ct)),
        ["POST /v2/sealedbox/open-pq"] = Post(Ctx.SealedboxOpenRequest, Ctx.StringEnvelope, (c, r, ct) => c.SealedboxOpenPqAsync(r, ct)),
        ["POST /v2/stream/pq-encrypt"] = Post(Ctx.StreamPqEncryptRequest, Ctx.CiphertextResultEnvelope, (c, r, ct) => c.StreamPqEncryptAsync(r, ct)),
        ["POST /v2/stream/pq-decrypt"] = Post(Ctx.StreamPqDecryptRequest, Ctx.PlaintextResultEnvelope, (c, r, ct) => c.StreamPqDecryptAsync(r, ct)),
        ["POST /v2/stream/multi-pq-encrypt"] = Post(Ctx.StreamMultiPqEncryptRequest, Ctx.StreamMultiPqEncryptResultEnvelope, (c, r, ct) => c.StreamMultiPqEncryptAsync(r, ct)),
        ["POST /v2/stream/multi-pq-decrypt"] = Post(Ctx.StreamMultiPqDecryptRequest, Ctx.StreamMultiPqDecryptResultEnvelope, (c, r, ct) => c.StreamMultiPqDecryptAsync(r, ct)),
        ["POST /v2/multi-recipient/encrypt"] = Post(Ctx.MultiRecipientEncryptRequest, Ctx.MultiRecipientEncryptResultEnvelope, (c, r, ct) => c.MultiRecipientEncryptAsync(r, ct)),
        ["POST /v2/hpke/keygen"] = Post(Ctx.HpkeKeygenRequest, Ctx.KeyPairResultEnvelope, (c, r, ct) => c.HpkeGenerateKeyPairAsync(r, ct)),
        ["POST /v2/hpke/seal"] = Post(Ctx.HpkeSealRequest, Ctx.HpkeSealResultEnvelope, (c, r, ct) => c.HpkeSealAsync(r, ct)),
        ["POST /v2/hpke/open"] = Post(Ctx.HpkeOpenRequest, Ctx.HpkeOpenResultEnvelope, (c, r, ct) => c.HpkeOpenAsync(r, ct)),
        ["POST /v2/kms/create-key"] = Post(Ctx.KmsCreateKeyRequest, Ctx.KmsKeyEnvelope, (c, r, ct) => c.KmsCreateKeyAsync(r, ct)),
        ["POST /v2/kms/wrap"] = Post(Ctx.KmsWrapRequest, Ctx.KmsWrapResultEnvelope, (c, r, ct) => c.KmsWrapAsync(r, ct)),
        ["POST /v2/kms/unwrap"] = Post(Ctx.KmsUnwrapRequest, Ctx.KmsUnwrapResultEnvelope, (c, r, ct) => c.KmsUnwrapAsync(r, ct)),
        ["POST /v2/kms/generate-data-key"] = Post(Ctx.KmsGenerateDataKeyRequest, Ctx.KmsDataKeyEnvelope, (c, r, ct) => c.KmsGenerateDataKeyAsync(r, ct)),
        ["POST /v2/kms/encrypt"] = Post(Ctx.KmsEncryptRequest, Ctx.KmsEncryptResultEnvelope, (c, r, ct) => c.KmsEncryptAsync(r, ct)),
        ["POST /v2/kms/decrypt"] = Post(Ctx.KmsDecryptRequest, Ctx.KmsDecryptResultEnvelope, (c, r, ct) => c.KmsDecryptAsync(r, ct)),
        ["POST /v2/opaque/register/init"] = Post(Ctx.OpaqueRegisterInitRequest, Ctx.OpaqueRegisterInitResultEnvelope, (c, r, ct) => c.OpaqueRegisterInitAsync(r, ct)),
        ["POST /v2/opaque/register/finish"] = Post(Ctx.OpaqueRegisterFinishRequest, Ctx.OpaqueRegisterFinishResultEnvelope, (c, r, ct) => c.OpaqueRegisterFinishAsync(r, ct)),
        ["POST /v2/opaque/login/init"] = Post(Ctx.OpaqueLoginInitRequest, Ctx.OpaqueLoginInitResultEnvelope, (c, r, ct) => c.OpaqueLoginInitAsync(r, ct)),
        ["POST /v2/opaque/login/finish"] = Post(Ctx.OpaqueLoginFinishRequest, Ctx.OpaqueLoginFinishResultEnvelope, (c, r, ct) => c.OpaqueLoginFinishAsync(r, ct)),
    };

    private static CryptoJsonContext Ctx => CryptoJsonContext.Default;

    /// <summary>The typed call for a request, or <see langword="null"/> to send it raw.</summary>
    internal static TypedCall? Prepare(string method, string path, JsonElement body) =>
        Routes.TryGetValue(method + " " + path, out Func<JsonElement, TypedCall?>? prepare) ? prepare(body) : null;

    private static Func<JsonElement, TypedCall?> Post<TRequest, TResult>(
        JsonTypeInfo<TRequest> requestInfo,
        JsonTypeInfo<DataEnvelope<TResult>> resultInfo,
        Func<CryptoServiceClient, TRequest, CancellationToken, Task<TResult>> call)
        where TRequest : class =>
        body =>
        {
            TRequest? request = TryRead(body, requestInfo);
            return request is null ? null : async (client, ct) => Wrap(await call(client, request, ct), resultInfo);
        };

    private static Func<JsonElement, TypedCall?> PostEmpty<TResult>(
        JsonTypeInfo<DataEnvelope<TResult>> resultInfo,
        Func<CryptoServiceClient, CancellationToken, Task<TResult>> call) =>
        body => body.ValueKind == JsonValueKind.Object && !body.EnumerateObject().Any()
            ? async (client, ct) => Wrap(await call(client, ct), resultInfo)
            : null;

    private static Func<JsonElement, TypedCall?> GetData<TResult>(
        JsonTypeInfo<DataEnvelope<TResult>> resultInfo,
        Func<CryptoServiceClient, CancellationToken, Task<TResult>> call) =>
        _ => async (client, ct) => Wrap(await call(client, ct), resultInfo);

    private static Func<JsonElement, TypedCall?> GetBare<TResult>(
        JsonTypeInfo<TResult> resultInfo,
        Func<CryptoServiceClient, CancellationToken, Task<TResult>> call) =>
        _ => async (client, ct) => JsonSerializer.SerializeToElement(await call(client, ct), resultInfo);

    private static JsonElement Wrap<TResult>(TResult result, JsonTypeInfo<DataEnvelope<TResult>> info) =>
        JsonSerializer.SerializeToElement(new DataEnvelope<TResult> { Data = result }, info);

    private static T? TryRead<T>(JsonElement body, JsonTypeInfo<T> info)
        where T : class
    {
        // The typed call must send exactly what the vector sends: a body with
        // a member the request type drops (an unknown property on purpose)
        // goes raw.
        try
        {
            T? request = body.Deserialize(info);
            return request is not null && JsonElement.DeepEquals(body, JsonSerializer.SerializeToElement(request, info)) ? request : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
