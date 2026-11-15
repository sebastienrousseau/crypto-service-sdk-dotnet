#!/usr/bin/env bash
# SPDX-License-Identifier: Apache-2.0 OR MIT
#
# Runs one SharpFuzz target under libfuzzer-dotnet for a bounded time:
# builds the harness (fuzz/CryptoService.Sdk.Fuzz), instruments
# CryptoService.Sdk.dll with the pinned sharpfuzz tool, downloads the
# pinned libfuzzer-dotnet driver (SHA-256 checked), and fuzzes from the
# seeds in fuzz/corpus/<target>. A crash is written under
# artifacts/fuzz/findings/ and fails the run; replay one with
#   dotnet run --project fuzz/CryptoService.Sdk.Fuzz -- <target> --replay <dir>
#
#   scripts/fuzz.sh problem 60     (target: problem or success; seconds)
#
# The driver is a Linux x86-64 binary; on another platform the script
# says so and stops. CI runs both targets for 60 s (fuzz.yml).
set -euo pipefail

cd "$(dirname "$0")/.."
target="${1:?usage: scripts/fuzz.sh <problem|success> [seconds]}"
seconds="${2:-60}"
case "$target" in problem | success) ;; *)
  echo "fuzz: unknown target $target" >&2
  exit 2
  ;;
esac

driver_version="v2025.05.02.0904"
driver_sha256="c2c2a90d94c409a4af339a0d4f244e0442c5a5d249be0f1252ba07871f285958"
if [ "$(uname -s)-$(uname -m)" != "Linux-x86_64" ]; then
  echo "fuzz: libfuzzer-dotnet ships for Linux x86-64 only; run the properties instead (make coverage)" >&2
  exit 1
fi

# The harness starts through its apphost, which needs to find the runtime
# when the SDK is not in a default location (mise, a user install).
DOTNET_ROOT="${DOTNET_ROOT:-$(dirname "$(readlink -f "$(command -v dotnet)")")}"
export DOTNET_ROOT

out="artifacts/fuzz"
bin="$out/bin-$target"
mkdir -p "$out/findings" "$out/corpus-$target"
dotnet tool restore > /dev/null
rm -rf "${bin:?}"
dotnet build fuzz/CryptoService.Sdk.Fuzz -c Release -o "$bin" --nologo -v quiet
dotnet sharpfuzz "$bin/CryptoService.Sdk.dll"

driver="$out/libfuzzer-dotnet-$driver_version"
if [ ! -x "$driver" ]; then
  curl -fsSL -o "$driver.tmp" \
    "https://github.com/Metalnem/libfuzzer-dotnet/releases/download/$driver_version/libfuzzer-dotnet-ubuntu"
  echo "$driver_sha256  $driver.tmp" | sha256sum -c - > /dev/null
  chmod +x "$driver.tmp"
  mv "$driver.tmp" "$driver"
fi

"$driver" --target_path="$bin/CryptoService.Sdk.Fuzz" --target_arg="$target" \
  -max_total_time="$seconds" -timeout=10 -rss_limit_mb=4096 \
  -artifact_prefix="$out/findings/$target-" \
  "$out/corpus-$target" "fuzz/corpus/$target"
