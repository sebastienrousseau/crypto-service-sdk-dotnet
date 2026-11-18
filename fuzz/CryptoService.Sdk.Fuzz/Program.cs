// SPDX-License-Identifier: Apache-2.0 OR MIT
// SPDX-FileCopyrightText: 2026 Sebastien Rousseau
//
// Fuzz targets for the bytes a server sends back. Each one fails only by
// crashing: an exception other than the documented one is a finding.
//
//   problem   Decoding.ParseProblem: any error body becomes a problem,
//             never an exception.
//   success   Decoding.DecodeSuccess over several result contracts: a 2xx
//             body decodes or raises CryptoTransportException, nothing else.
//
// Under libfuzzer-dotnet (scripts/fuzz.sh) the target name is the one
// argument. With `--replay <dir>` it runs every file of a directory once,
// uninstrumented, which is how a finding is reproduced.
//
//   dotnet run --project fuzz/CryptoService.Sdk.Fuzz -- problem --replay fuzz/corpus/problem

using CryptoService.Sdk;
using CryptoService.Sdk.Internal;
using SharpFuzz;

if (args.Length == 0 || Targets.Find(args[0]) is not { } target)
{
    Console.Error.WriteLine("usage: CryptoService.Sdk.Fuzz <problem|success> [--replay <dir>]");
    return 2;
}

if (args.Length == 3 && args[1] == "--replay")
{
    foreach (string file in Directory.EnumerateFiles(args[2]))
    {
        target(File.ReadAllBytes(file));
    }

    Console.WriteLine($"replayed {args[2]}: no finding");
    return 0;
}

Fuzzer.LibFuzzer.Run(span => target(span));
return 0;

/// <summary>The fuzz targets by name.</summary>
internal static class Targets
{
    /// <summary>The target named <paramref name="name"/>, or <see langword="null"/>.</summary>
    internal static ReadOnlySpanAction? Find(string name) =>
        name switch
        {
            "problem" => Problem,
            "success" => Success,
            _ => null,
        };

    private static void Problem(ReadOnlySpan<byte> body)
    {
        ProblemDetails problem = Decoding.ParseProblem(body, 500, "Internal Server Error");
        if (problem.Type is null || problem.Title is null || problem.Detail is null)
        {
            throw new InvalidOperationException("A parsed problem lacks a required member.");
        }
    }

    // The first byte picks the contract, so one corpus reaches them all.
    private static void Success(ReadOnlySpan<byte> body)
    {
        if (body.IsEmpty)
        {
            return;
        }

        try
        {
            Decode(body[0] % 7, body[1..]);
        }
        catch (CryptoTransportException)
        {
            // The documented failure for a body that breaks the contract.
        }
    }

    private static void Decode(int contract, ReadOnlySpan<byte> body)
    {
        CryptoJsonContext json = CryptoJsonContext.Default;
        _ = contract switch
        {
            0 => (object)Decoding.DecodeSuccess(body, json.HashResultEnvelope),
            1 => Decoding.DecodeSuccess(body, json.GenerateKeyResultEnvelope),
            2 => Decoding.DecodeSuccess(body, json.ExportKeyResultEnvelope),
            3 => Decoding.DecodeSuccess(body, json.StreamVerifyResultEnvelope),
            4 => Decoding.DecodeSuccess(body, json.KmsKeyEnvelope),
            5 => Decoding.DecodeSuccess(body, json.StringEnvelope),
            _ => Decoding.DecodeSuccess(body, json.CbomDocument),
        };
    }
}
