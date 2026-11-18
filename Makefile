# SPDX-License-Identifier: Apache-2.0 OR MIT
# SPDX-FileCopyrightText: 2026 Sebastien Rousseau
#
# `make check` runs every local gate CI runs that needs no server:
# format, build (warnings as errors, .NET analysers, CA1502 and
# SonarAnalyzer complexity ceilings), unit tests with the 100 % line and
# branch coverage gate, the vulnerable-package audit, the version check
# and the Halstead check. The conformance tests skip unless
# CRYPTO_SERVICE_URL is set; `make conformance` insists on it, and
# `make regression` builds and starts the pinned server itself.
# DEVELOPMENT.md maps every target to CI.

SOLUTION := CryptoService.Sdk.slnx
CONFIG   := Release

.PHONY: check format format-fix build test coverage audit verify-versions conformance examples readme bench bench-smoke fuzz regression demo docs docs-lint codespell reuse halstead

check: format build coverage audit verify-versions halstead

format:
	dotnet format $(SOLUTION) --verify-no-changes

format-fix:
	dotnet format $(SOLUTION)

build:
	dotnet build $(SOLUTION) -c $(CONFIG)

test: build
	dotnet test $(SOLUTION) -c $(CONFIG) --no-build

# The unit tests with coverlet: fails below 100 % line and branch
# coverage of the library. The report is artifacts/coverage/.
coverage: build
	dotnet test tests/CryptoService.Sdk.Tests -c $(CONFIG) --no-build -p:CollectCoverage=true

# `dotnet list package --vulnerable` exits 0 even when it finds one, so
# the output is checked.
audit:
	@out="$$(dotnet list $(SOLUTION) package --vulnerable --include-transitive 2>&1)"; \
	echo "$$out"; \
	if echo "$$out" | grep -q "has the following vulnerable packages"; then exit 1; fi

# Every file that states the version agrees with the csproj.
verify-versions:
	scripts/verify-release-versions

# The conformance kit against CRYPTO_SERVICE_URL, with
# CRYPTO_SERVICE_API_KEY and the server's JWT_SECRET in
# CRYPTO_SERVICE_JWT_SECRET: each kit file runs as its own principal
# (conformance-<group>) with minted HS256 tokens, as conformance/README.md
# describes. Fails, not skips, when any of the three is unset.
conformance: build
	@test -n "$$CRYPTO_SERVICE_URL" || { echo "CRYPTO_SERVICE_URL is unset: start crypto-server first (DEVELOPMENT.md)" >&2; exit 1; }
	@test -n "$$CRYPTO_SERVICE_API_KEY" || { echo "CRYPTO_SERVICE_API_KEY is unset" >&2; exit 1; }
	@test -n "$$CRYPTO_SERVICE_JWT_SECRET" || { echo "CRYPTO_SERVICE_JWT_SECRET is unset: start the server with JWT_SECRET and export the same value" >&2; exit 1; }
	CRYPTO_SERVICE_CONFORMANCE=required dotnet test tests/CryptoService.Sdk.Tests -c $(CONFIG) --no-build \
	  --filter "FullyQualifiedName~ConformanceTests" --logger "console;verbosity=detailed"

# Every example in examples/, against CRYPTO_SERVICE_URL.
examples: build
	CONFIG=$(CONFIG) scripts/run-examples.sh

# Every code block in README.md, extracted and executed against
# CRYPTO_SERVICE_URL, the package restored from a local feed.
readme: build
	CONFIG=$(CONFIG) scripts/readme-snippets.sh

# BenchmarkDotNet: encode_request, decode_response and decode_problem,
# and round_trip_hash when CRYPTO_SERVICE_URL is set. Results land in
# artifacts/benchmarks/.
BENCH := dotnet run -c $(CONFIG) --no-build --project benchmarks/CryptoService.Sdk.Benchmarks --

bench: build
	$(BENCH) --artifacts artifacts/benchmarks

# One iteration of each (BenchmarkDotNet's Dry job): kept running, not measured.
bench-smoke: build
	$(BENCH) --job Dry --artifacts artifacts/benchmarks-smoke

# Both SharpFuzz targets under libfuzzer-dotnet, FUZZ_SECONDS each.
FUZZ_SECONDS ?= 60

fuzz:
	scripts/fuzz.sh problem $(FUZZ_SECONDS)
	scripts/fuzz.sh success $(FUZZ_SECONDS)

# Everything above against the pinned server (ci/server.ref), which it
# clones, builds and starts: scripts/regression.sh.
regression:
	scripts/regression.sh

# Renders .github/demo.gif from .github/demo.tape against the server at
# CRYPTO_SERVICE_URL.
demo: build
	@command -v vhs > /dev/null || { echo "vhs not found: https://github.com/charmbracelet/vhs" >&2; exit 1; }
	vhs .github/demo.tape

# The manual: API reference from the XML docs, then MkDocs in strict
# mode, into site/. The tools come from docs/requirements.txt.
DOCS_VENV ?= .venv-docs

$(DOCS_VENV)/.installed: docs/requirements.txt
	python3 -m venv $(DOCS_VENV)
	$(DOCS_VENV)/bin/pip install --quiet --require-hashes --no-deps -r docs/requirements.txt
	touch $@

docs: build $(DOCS_VENV)/.installed
	CONFIG=$(CONFIG) scripts/api-docs.sh
	$(DOCS_VENV)/bin/mkdocs build --strict

# Python lint tools (codespell, reuse and pygments), hash-checked from
# ci/lint-requirements.txt, in a local virtual environment.
LINT_VENV ?= .venv-lint

$(LINT_VENV)/.installed: ci/lint-requirements.txt
	python3 -m venv $(LINT_VENV)
	$(LINT_VENV)/bin/pip install --quiet --require-hashes --no-deps -r ci/lint-requirements.txt
	touch $@

# Halstead difficulty of every C# method, ceiling 30. The build enforces
# the other complexity ceilings (CA1502, SonarAnalyzer S3776/S138/S104).
halstead: $(LINT_VENV)/.installed
	$(LINT_VENV)/bin/python scripts/halstead.py

# markdownlint, codespell and REUSE, as the docs-lint workflow runs them.
docs-lint: codespell reuse
	npx --yes markdownlint-cli2@0.23.3 "**/*.md"

codespell: $(LINT_VENV)/.installed
	$(LINT_VENV)/bin/codespell

reuse: $(LINT_VENV)/.installed
	$(LINT_VENV)/bin/reuse lint
