#!/usr/bin/env bash
# SPDX-License-Identifier: Apache-2.0 OR MIT
#
# API breakage check: packs CryptoService.Sdk with .NET package validation,
# which runs Microsoft.DotNet.ApiCompat against the last published
# package (PackageValidationBaselineVersion) and fails on a breaking
# change to the public API.
#
# The baseline is the newest v* tag before HEAD whose package is on
# nuget.org. Until the first release is published there is none, and
# the check is a no-op that says so; package validation still runs its
# own consistency checks on the new package.
#
#   scripts/api-compat.sh          (after make build)
set -euo pipefail

cd "$(dirname "$0")/.."
config="${CONFIG:-Release}"
baseline=""
for tag in $(git tag --list 'v*' --sort=-v:refname); do
  version="${tag#v}"
  if git merge-base --is-ancestor "$tag" HEAD &&
    [ "$version" != "$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' src/CryptoService.Sdk/CryptoService.Sdk.csproj)" ] &&
    curl -fsS "https://api.nuget.org/v3-flatcontainer/cryptoservice.sdk/index.json" 2> /dev/null |
    grep -q "\"$version\""; then
    baseline="$version"
    break
  fi
done

out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT
if [ -z "$baseline" ]; then
  echo "api-compat: no published CryptoService.Sdk release yet, so no baseline: the breakage check is a no-op until the first one."
  dotnet pack src/CryptoService.Sdk/CryptoService.Sdk.csproj -c "$config" --no-build -o "$out" -p:EnablePackageValidation=true
else
  echo "api-compat: comparing the public API with CryptoService.Sdk $baseline"
  dotnet pack src/CryptoService.Sdk/CryptoService.Sdk.csproj -c "$config" -o "$out" \
    -p:EnablePackageValidation=true -p:PackageValidationBaselineVersion="$baseline"
fi
