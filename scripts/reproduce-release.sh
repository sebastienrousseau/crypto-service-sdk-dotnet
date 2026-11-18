#!/usr/bin/env bash
# SPDX-License-Identifier: Apache-2.0 OR MIT
# SPDX-FileCopyrightText: 2026 Sebastien Rousseau
#
# Rebuilds a released version from its tag, outside CI, and compares the
# result with the assets of its GitHub release, byte for byte.
#
#   scripts/reproduce-release.sh 0.0.40
#
# Needs git, curl, python3 and the .NET SDK the tag's global.json names
# (`mise install` gives it). Both sides go through
# scripts/normalise-release.py with the tagged commit's time first, which
# changes nothing in a release from 0.0.41 on (the release workflow
# already ran it) and, for 0.0.40, sets the zip entry times and the SBOM
# serial and timestamp, the only bytes that depend on when the build ran.
# docs/REPRODUCING.md explains the procedure and the expected hashes.
set -euo pipefail

version="${1:?usage: scripts/reproduce-release.sh <version, for example 0.0.40>}"
repo="https://github.com/sebastienrousseau/crypto-service-sdk-dotnet"
here="$(cd "$(dirname "$0")" && pwd)"
work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT

echo "== Clone v$version"
# The origin must be the GitHub URL: SourceLink writes it into the PDB.
git clone --quiet --depth 1 --branch "v$version" "$repo.git" "$work/src"
cd "$work/src"

want="$(sed -n 's/.*"version": *"\([^"]*\)".*/\1/p' global.json)"
have="$(dotnet --version)"
if [ "$want" != "$have" ]; then
  echo "reproduce-release: the tag pins .NET SDK $want, this machine runs $have" >&2
  exit 1
fi

echo "== Build and pack with .NET SDK $have"
export CI=true DOTNET_NOLOGO=true DOTNET_CLI_TELEMETRY_OPTOUT=true
project=src/CryptoService.Sdk/CryptoService.Sdk.csproj
dotnet restore "$project" --locked-mode > /dev/null
dotnet build "$project" -c Release --no-restore > /dev/null
dotnet pack "$project" -c Release --no-build -o "$work/rebuilt" > /dev/null
dotnet tool restore > /dev/null
dotnet CycloneDX "$project" --output "$work/rebuilt" \
  --filename "CryptoService.Sdk.$version.cdx.json" --output-format Json \
  --set-name CryptoService.Sdk --set-version "$version" --set-type Library > /dev/null

echo "== Download the release assets and check SHA256SUMS"
mkdir "$work/released"
curl -fsSL -o "$work/released/SHA256SUMS" "$repo/releases/download/v$version/SHA256SUMS"
while read -r _ name; do
  curl -fsSL -o "$work/released/$name" "$repo/releases/download/v$version/$name"
done < "$work/released/SHA256SUMS"
(cd "$work/released" && sha256sum --check --quiet SHA256SUMS)

echo "== Normalise both sides to the tagged commit's time"
epoch="$(git log -1 --format=%ct)"
python3 -I "$here/normalise-release.py" "$work/rebuilt" "$epoch" > /dev/null
python3 -I "$here/normalise-release.py" "$work/released" "$epoch" > /dev/null

echo "== Compare"
status=0
while read -r _ name; do
  released="$(sha256sum < "$work/released/$name" | cut -d' ' -f1)"
  rebuilt="$(sha256sum < "$work/rebuilt/$name" | cut -d' ' -f1)"
  if [ "$released" = "$rebuilt" ]; then
    echo "identical  $rebuilt  $name"
  else
    echo "DIFFERENT  $name: released $released, rebuilt $rebuilt"
    status=1
  fi
done < "$work/released/SHA256SUMS"
exit "$status"
