#!/usr/bin/env bash
# SPDX-License-Identifier: Apache-2.0 OR MIT
# SPDX-FileCopyrightText: 2026 Sebastien Rousseau
#
# Renders the API reference for the manual: scripts/api_docs.py reads the
# XML documentation file the build writes beside CryptoService.Sdk.dll and
# writes one Markdown page per public type into docs/api/, which is
# generated and not committed. `make docs` and the docs workflow run it
# before `mkdocs build`.
set -euo pipefail

cd "$(dirname "$0")/.."
config="${CONFIG:-Release}"
xml="src/CryptoService.Sdk/bin/$config/net10.0/CryptoService.Sdk.xml"
[ -f "$xml" ] || dotnet build src/CryptoService.Sdk/CryptoService.Sdk.csproj -c "$config"

rm -rf docs/api
python3 scripts/api_docs.py "$xml" docs/api
