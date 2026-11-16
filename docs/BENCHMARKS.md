<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->

# Benchmarks

`benchmarks/CryptoService.Sdk.Benchmarks` (BenchmarkDotNet) runs the
scenarios every Crypto Service SDK names alike, so results can be put
side by side later:

| Scenario | What it measures | Needs a server |
| :--- | :--- | :--- |
| `encode_request` | Serializing a `HashRequest` with 1 KiB of data through the source-generated contract | No |
| `decode_response` | Decoding a `/v2/hash` answer into `HashResult` | No |
| `decode_problem` | Parsing a `validation-failed` problem | No |
| `round_trip_hash` | One `HashAsync` call to crypto-server on loopback | Yes (`CRYPTO_SERVICE_URL`) |

## Method

```sh
make build
make bench          # full run; results in artifacts/benchmarks/
make bench-smoke    # one iteration each, as CI runs it
```

BenchmarkDotNet's default job, in process (`--inProcess`), with
`[MemoryDiagnoser]`. CI runs the smoke job on every change to keep the
benchmarks compiling and running; it asserts and records nothing.

## Results

No numbers are published yet. This page will be generated from a full
run, with the machine, runtime and server version beside the table;
until then, run `make bench` on your own hardware.
