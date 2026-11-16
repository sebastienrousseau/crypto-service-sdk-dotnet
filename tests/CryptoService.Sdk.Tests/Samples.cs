// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Text.Json;

namespace CryptoService.Sdk.Tests;

/// <summary>One operation: its route, a valid answer, and how to call it.</summary>
/// <param name="Name">The method, without <c>Async</c>.</param>
/// <param name="Method">The HTTP method it must send.</param>
/// <param name="Path">The path it must send.</param>
/// <param name="Sample">A valid result, as JSON.</param>
/// <param name="Wrapped">Whether the server wraps the result in <c>{ "data": ... }</c>.</param>
/// <param name="Authenticated">Whether it sends credentials.</param>
/// <param name="Invoke">Calls the method.</param>
internal sealed record Operation(
    string Name,
    string Method,
    string Path,
    string Sample,
    bool Wrapped,
    bool Authenticated,
    Func<CryptoServiceClient, CancellationToken, Task<object>> Invoke)
{
    /// <summary>The body the server answers.</summary>
    internal string Answer => Wrapped ? "{\"data\":" + Sample + "}" : Sample;

    public override string ToString() => Name;
}

/// <summary>Synthetic answers, one per result type: hex <c>00ff</c>, base64 <c>AP8=</c>.</summary>
internal static class Samples
{
    private static readonly Dictionary<string, string> ByType = new(StringComparer.Ordinal)
    {
        ["ServiceInfo"] = """{"description":"d","details":"d","id":"00000000-0000-4000-8000-000000000000","license":"Apache-2.0 OR MIT","title":"t","url":"https://crypto.test","version":"0.0.40"}""",
        ["HealthStatus"] = """{"status":"ok","statusCode":200,"uptime":1.5}""",
        ["LiveStatus"] = """{"status":"alive","uptime":2}""",
        ["ReadyStatus"] = """{"status":"ready","checks":{"accepting":true,"keyStore":true}}""",
        ["AlgorithmCatalog"] = """{"encryption":["xchacha20-poly1305"],"hashing":["sha256"],"kdf":[],"keyExchange":[],"mac":[],"postQuantum":[],"signing":[],"later":["x"]}""",
        ["CbomDocument"] = """{"bomFormat":"CycloneDX","components":[{"name":"ml-kem-768","type":"cryptographic-asset","version":"1","properties":[{"name":"k","value":"v"}],"bom-ref":"r"}],"metadata":{"timestamp":"t"},"serialNumber":"urn:uuid:00000000-0000-4000-8000-000000000000","specVersion":"1.6","version":1,"dependencies":[]}""",
        ["DoraCompliance"] = """{"activePrimitivesCount":3,"algorithmDeprecationSchedule":[{"algorithm":"rsa","category":"asymmetric","recommendedMigration":"ml-kem","sunsetDate":"2030"}],"article":"Article 9 & Article 13","cryptographicInventory":[{"fipsCompliance":"n/a","package":"p","status":"implemented","version":"1"}],"disclaimer":"d","postQuantumPrimitivesCount":1,"quantumResistanceRatio":0.5,"standard":"DORA (EU 2022/2554)","status":"s","timestamp":"2026-11-15T11:40:20.415Z"}""",
        ["HashResult"] = """{"algorithm":"sha256","digest":"00ff","length":2}""",
        ["HmacResult"] = """{"algorithm":"sha256","mac":"00ff"}""",
        ["VerifyResult"] = """{"algorithm":"ed25519","valid":true}""",
        ["KdfResult"] = """{"algorithm":"hkdf-sha256","derivedKey":"00ff","keyLength":2,"salt":"00"}""",
        ["PasswordHashResult"] = """{"algorithm":"argon2id","hash":"00","params":{"m":19456,"p":1,"t":2},"phc":"$argon2id$v=19$m=19456,t=2,p=1$AA$AA","salt":"00"}""",
        ["PasswordVerifyResult"] = """{"valid":true}""",
        ["CiphertextResult"] = """{"algorithm":"xchacha20-poly1305","ciphertext":"AP8="}""",
        ["PlaintextResult"] = """{"plaintext":"p"}""",
        ["SealResult"] = """{"algorithm":"xchacha20-poly1305","sealed":"AP8="}""",
        ["string"] = "\"p\"",
        ["PasswordEncryptResult"] = """{"algorithm":"argon2id-xchacha20-poly1305","encrypted":"AP8="}""",
        ["KeyWrapResult"] = """{"algorithm":"aes-kw","wrapped":"AP8="}""",
        ["GenerateKeyResult"] = """{"algorithm":"ed25519","keyId":"k_AAAAAAAAAAAAAAAAAAAAAA","kid":"kid","metadata":{"kid":"kid","use":"sig"},"publicKey":"00ff"}""",
        ["KeyPairResult"] = """{"algorithm":"ml-kem-768","keyId":"k_AAAAAAAAAAAAAAAAAAAAAA","publicKey":"00ff"}""",
        ["ExportKeyResult"] = """{"algorithm":"x25519-ml-kem-768","keyId":"k_AAAAAAAAAAAAAAAAAAAAAA","x25519PrivateKey":"00","futureKey":"00"}""",
        ["DeleteKeyResult"] = """{"deleted":true,"keyId":"k_AAAAAAAAAAAAAAAAAAAAAA"}""",
        ["SignResult"] = """{"algorithm":"ed25519","signature":"00ff"}""",
        ["StreamSignResult"] = """{"count":1,"signatures":[{"algorithm":"ed25519","id":"1","signature":"00"}]}""",
        ["StreamVerifyResult"] = """{"allValid":true,"count":1,"results":[{"id":"1","valid":true}],"validCount":1}""",
        ["Iso20022VerifyResult"] = """{"classicalValid":true,"digestMatches":true,"messageId":"m","postQuantumValid":true,"valid":true}""",
        ["MlKemEncapsulateResult"] = """{"algorithm":"ml-kem-768","ciphertext":"00","sharedSecret":"00ff"}""",
        ["SharedSecretResult"] = """{"algorithm":"ml-kem-768","sharedSecret":"00ff"}""",
        ["HybridKeyPairResult"] = """{"algorithm":"x25519-ml-kem-768","keyId":"k_AAAAAAAAAAAAAAAAAAAAAA","mlKemPublicKey":"00","x25519PublicKey":"00"}""",
        ["HybridEncapsulateResult"] = """{"algorithm":"x25519-ml-kem-768","combiner":"crypto-service/hybrid-kem/v2","mlKemCiphertext":"00","sharedSecret":"00ff","x25519EphemeralPublic":"00"}""",
        ["StreamMultiPqEncryptResult"] = """{"algorithm":"multi","ciphertext":"AP8=","recipientCount":1}""",
        ["StreamMultiPqDecryptResult"] = """{"plaintext":"p","recipientId":"r"}""",
        ["MultiRecipientEncryptResult"] = """{"algorithm":"multi-recipient-secretbox","ciphertext":"AP8=","recipients":[{"ephemeralPublicKey":"00","type":"x25519","wrappedKey":"AP8="}]}""",
        ["HpkeSealResult"] = """{"ciphertext":"00","encapsulatedKey":"00"}""",
        ["HpkeOpenResult"] = """{"hex":"00ff","plaintext":"p"}""",
        ["KmsKey"] = """{"algorithm":"aes-256-gcm","createdAt":"2026-11-15T11:40:20.415Z","currentVersion":1,"enabled":true,"keyId":"local-1","provider":"local","usage":"encrypt"}""",
        ["KmsEncryptResult"] = """{"ciphertext":"c","keyId":"local-1","provider":"local","keyVersion":1}""",
        ["KmsDecryptResult"] = """{"hex":"00ff","keyId":"local-1","plaintext":"p","provider":"local"}""",
        ["KmsDataKey"] = """{"ciphertext":"c","keyId":"local-1","plaintext":"00ff","provider":"local"}""",
        ["KmsWrapResult"] = """{"keyId":"local-1","provider":"local","wrappedKey":"w"}""",
        ["KmsUnwrapResult"] = """{"keyId":"local-1","provider":"local","unwrappedKey":"00ff"}""",
        ["OpaqueRegisterInitResult"] = """{"response":"00"}""",
        ["OpaqueRegisterFinishResult"] = """{"credentialIdentifier":"c","status":"registered"}""",
        ["OpaqueLoginInitResult"] = """{"ke2":"00","sessionId":"s"}""",
        ["OpaqueLoginFinishResult"] = """{"sessionKey":"00"}""",
    };

    /// <summary>The sample answer for a result type.</summary>
    internal static string Of(string type) => ByType[type];

    /// <summary>A synthetic ISO 20022 envelope (the keys and signatures are not real).</summary>
    internal static Iso20022VerifyRequest Iso20022Request() =>
        new()
        {
            Envelope = new Iso20022Envelope
            {
                Classical = new Iso20022Signature { Algorithm = "ed25519", PublicKey = "00", Signature = "00" },
                DigestAlgorithm = "sha256",
                MessageId = "m",
                MessageType = "pain.001",
                PayloadDigest = "00",
                PostQuantum = new Iso20022Signature { Algorithm = "ml-dsa-65", PublicKey = "00", Signature = "00" },
                Timestamp = "2026-11-15T00:00:00Z",
            },
            Payload = JsonDocument.Parse("""{"amount":"1.00"}""").RootElement.Clone(),
            TrustedKeys = new Iso20022TrustedKeys { ClassicalPublicKey = "00", PostQuantumPublicKey = "00" },
        };
}
