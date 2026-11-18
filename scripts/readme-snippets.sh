#!/usr/bin/env bash
# SPDX-License-Identifier: Apache-2.0 OR MIT
# SPDX-FileCopyrightText: 2026 Sebastien Rousseau
#
# Executes every fenced code block in README.md, so the README cannot
# drift from the package:
#
#   xml     a PackageReference: restored and built in a fresh project
#   sh      a shell command: run in a fresh console project
#   csharp  a program: compiled against the package and run against
#           CRYPTO_SERVICE_URL with CRYPTO_SERVICE_API_KEY
#
# The package comes from a local feed packed from this checkout, and
# every project is created outside the repository, so it sees what a
# user sees. Any other fence language fails the run. Build first.
set -euo pipefail

cd "$(dirname "$0")/.."
: "${CRYPTO_SERVICE_URL:?CRYPTO_SERVICE_URL is unset: start crypto-server first}"
: "${CRYPTO_SERVICE_API_KEY:?CRYPTO_SERVICE_API_KEY is unset}"
config="${CONFIG:-Release}"
root="$(pwd)"
work="$(mktemp -d)"
trap 'rm -rf "${work:?}"' EXIT

version=$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' src/CryptoService.Sdk/CryptoService.Sdk.csproj)
dotnet pack src/CryptoService.Sdk/CryptoService.Sdk.csproj -c "$config" --no-build -o "$work/feed" --nologo -v quiet

python3 - "$root/README.md" "$work/blocks" << 'PY'
import pathlib, re, sys
text = pathlib.Path(sys.argv[1]).read_text(encoding="utf-8")
out = pathlib.Path(sys.argv[2])
out.mkdir()
blocks = re.findall(r"^```([A-Za-z]*)\n(.*?)^```$", text, flags=re.S | re.M)
for i, (lang, body) in enumerate(blocks, 1):
    (out / f"{i:02d}.{lang or 'none'}").write_text(body, encoding="utf-8")
print(f"readme-snippets: {len(blocks)} code blocks")
PY

# project DIR [ITEMS]: a console project on net10.0 that restores
# CryptoService.Sdk from the local feed and everything else from nuget.org.
project() {
  mkdir -p "$1"
  cat > "$1/nuget.config" << XML
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="$work/feed" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="local"><package pattern="CryptoService.Sdk" /></packageSource>
    <packageSource key="nuget.org"><package pattern="*" /></packageSource>
  </packageSourceMapping>
</configuration>
XML
  cat > "$1/app.csproj" << XML
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  ${2:-}
</Project>
XML
}

probe='System.Console.WriteLine("CryptoService.Sdk " + typeof(CryptoService.Sdk.CryptoServiceClient).Assembly.GetName().Version);'
reference="<ItemGroup><PackageReference Include=\"CryptoService.Sdk\" Version=\"$version\" /></ItemGroup>"
ran=0
for block in "$work"/blocks/*; do
  name="$(basename "$block")"
  dir="$work/run-$name"
  echo "::group::README block $name"
  case "$name" in
    *.xml)
      project "$dir" "<ItemGroup>$(cat "$block")</ItemGroup>"
      echo "$probe" > "$dir/Program.cs"
      (cd "$dir" && dotnet run --nologo -v quiet)
      ;;
    *.sh)
      project "$dir"
      echo "$probe" > "$dir/Program.cs"
      (cd "$dir" && sh -eu "$block" && dotnet run --nologo -v quiet)
      ;;
    *.csharp)
      project "$dir" "$reference"
      cp "$block" "$dir/Program.cs"
      (cd "$dir" && dotnet run --nologo -v quiet)
      ;;
    *)
      echo "readme-snippets: $name has no runner; give the fence one of xml, sh or csharp" >&2
      exit 1
      ;;
  esac
  echo "::endgroup::"
  ran=$((ran + 1))
done
echo "readme-snippets: ran all $ran code blocks in README.md"
