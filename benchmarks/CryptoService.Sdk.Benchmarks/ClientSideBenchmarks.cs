// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau

using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using CryptoService.Sdk.Internal;

namespace CryptoService.Sdk.Benchmarks;

/// <summary>
/// The client's own cost per call, no network: encoding a request body,
/// decoding a 2xx answer and decoding an RFC 9457 problem, through the
/// same source-generated contract the client uses.
/// </summary>
[MemoryDiagnoser]
public class ClientSideBenchmarks
{
    private static readonly HashRequest Request = new() { Algorithm = "sha256", Data = new string('a', 1024) };

    private static readonly byte[] Response = Encoding.UTF8.GetBytes(
        """{"data":{"algorithm":"sha256","digest":"ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad","length":32}}""");

    private static readonly byte[] Problem = Encoding.UTF8.GetBytes(
        """{"type":"urn:crypto-service:problem:validation-failed","title":"Validation failed","status":400,"detail":"body must have required property 'data'","instance":"/v2/hash","errors":[{"field":"/data","message":"must have required property 'data'"}]}""");

    [Benchmark(Description = "encode_request")]
    public byte[] EncodeRequest() => JsonSerializer.SerializeToUtf8Bytes(Request, CryptoJsonContext.Default.HashRequest);

    [Benchmark(Description = "decode_response")]
    public HashResult DecodeResponse() => Decoding.DecodeSuccess(Response, CryptoJsonContext.Default.HashResultEnvelope).Data;

    [Benchmark(Description = "decode_problem")]
    public ProblemDetails DecodeProblem() => Decoding.ParseProblem(Problem, 400, "Bad Request");
}
