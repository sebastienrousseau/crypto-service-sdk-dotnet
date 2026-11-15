#!/usr/bin/env bash
# SPDX-License-Identifier: Apache-2.0 OR MIT
#
# Runs every example in examples/ against the server at
# CRYPTO_SERVICE_URL with CRYPTO_SERVICE_API_KEY, and fails on the first
# one that exits non-zero. Each example checks its own result. Build
# first (make build); CI's conformance job calls this after the build.
set -euo pipefail

cd "$(dirname "$0")/.."
: "${CRYPTO_SERVICE_URL:?CRYPTO_SERVICE_URL is unset: start crypto-server first}"
: "${CRYPTO_SERVICE_API_KEY:?CRYPTO_SERVICE_API_KEY is unset}"
config="${CONFIG:-Release}"

for name in Hash AeadRoundTrip SignVerify MlKem ErrorHandling Concurrent; do
  echo "::group::examples/$name"
  dotnet run --project "examples/$name" -c "$config" --no-build
  echo "::endgroup::"
done

echo "every example ran against $CRYPTO_SERVICE_URL"
