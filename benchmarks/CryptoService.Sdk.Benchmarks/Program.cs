// SPDX-License-Identifier: Apache-2.0 OR MIT
//
// The suite's benchmark scenarios, named alike in every Crypto Service
// SDK: encode_request, decode_response and decode_problem (the client's
// own work, no network), and round_trip_hash (one loopback call to a
// running crypto-server), which runs only when CRYPTO_SERVICE_URL is set.
// Arguments go to BenchmarkDotNet; `--job Dry` is the CI smoke run.
//
//   dotnet run -c Release --project benchmarks/CryptoService.Sdk.Benchmarks
//   dotnet run -c Release --project benchmarks/CryptoService.Sdk.Benchmarks -- --job Dry

using BenchmarkDotNet.Running;
using CryptoService.Sdk.Benchmarks;

var types = new List<Type> { typeof(ClientSideBenchmarks) };
if (LoopbackBenchmarks.BaseUrl is null)
{
    Console.WriteLine("CRYPTO_SERVICE_URL is unset: skipping round_trip_hash (start crypto-server and set it to run it).");
}
else
{
    types.Add(typeof(LoopbackBenchmarks));
}

// In process (--inProcess): no generated child project to restore, which
// locked-mode restore in CI would refuse.
List<string> arguments = [.. args];
if (!arguments.Contains("--filter"))
{
    arguments.AddRange(["--filter", "*"]);
}

if (!arguments.Contains("--inProcess"))
{
    arguments.Add("--inProcess");
}

BenchmarkSwitcher.FromTypes([.. types]).Run([.. arguments]);
