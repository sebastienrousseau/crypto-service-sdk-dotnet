#!/usr/bin/env bash
# SPDX-License-Identifier: Apache-2.0 OR MIT
#
# The full gate against a real server, the same steps CI runs:
#
#   1. crypto-server: $CRYPTO_SERVICE_CHECKOUT (a built checkout), or
#      sebastienrousseau/crypto-service at the commit in ci/server.ref,
#      cloned into a temporary directory and built with pnpm;
#   2. that server on a free loopback port, with a synthetic API key and
#      JWT secret made up for this run;
#   3. make check (format, build, coverage, audit, versions, Halstead);
#   4. the conformance kit, every example, every README code block, and
#      the benchmark smoke run against the server.
#
# The server is stopped and the temporary files removed on exit, failure
# included, so it is safe to run again.
#
#   scripts/regression.sh        (or: make regression)
#
# Needs Node 24 and pnpm 11.9.0 (mise install); CRYPTO_SERVICE_PORT picks
# the port instead of a free one.
set -euo pipefail

cd "$(dirname "$0")/.."
work="$(mktemp -d)"
pid=""

cleanup() {
  if [ -n "$pid" ]; then
    kill "$pid" 2> /dev/null || true
    wait "$pid" 2> /dev/null || true
  fi
  rm -rf "${work:?}"
}
trap cleanup EXIT

free_port() {
  python3 -c 'import socket; s = socket.socket(); s.bind(("127.0.0.1", 0)); print(s.getsockname()[1]); s.close()'
}

checkout="${CRYPTO_SERVICE_CHECKOUT:-}"
if [ -z "$checkout" ]; then
  ref="$(tr -d '[:space:]' < ci/server.ref)"
  if ! [[ "$ref" =~ ^[0-9a-f]{40}$ ]]; then
    echo "regression: ci/server.ref must hold a full 40-character commit SHA" >&2
    exit 1
  fi
  echo "regression: building crypto-service at $ref"
  checkout="$work/crypto-service"
  git init -q "$checkout"
  git -C "$checkout" fetch -q --depth 1 https://github.com/sebastienrousseau/crypto-service.git "$ref"
  git -C "$checkout" checkout -q FETCH_HEAD
  (cd "$checkout" && pnpm install --frozen-lockfile && pnpm -r run build)
fi

port="${CRYPTO_SERVICE_PORT:-$(free_port)}"
key="regression-key-$RANDOM$RANDOM$RANDOM"
secret="regression-jwt-secret-$RANDOM$RANDOM$RANDOM$RANDOM"
(cd "$checkout/packages/crypto-server" &&
  exec env PORT="$port" HOST=127.0.0.1 NODE_ENV=development RATE_LIMIT_MAX=100000 \
    CRYPTO_API_KEY="$key" JWT_SECRET="$secret" node dist/index.js) > "$work/server.log" 2>&1 &
pid="$!"
up=0
for _ in $(seq 1 60); do
  if curl -fsS "http://127.0.0.1:$port/health" > /dev/null 2>&1; then
    up=1
    break
  fi
  sleep 1
done
if [ "$up" -ne 1 ]; then
  echo "regression: crypto-server did not start:" >&2
  cat "$work/server.log" >&2
  exit 1
fi

export CRYPTO_SERVICE_URL="http://127.0.0.1:$port"
export CRYPTO_SERVICE_API_KEY="$key"
export CRYPTO_SERVICE_JWT_SECRET="$secret"
echo "regression: crypto-server on $CRYPTO_SERVICE_URL"

make check
make conformance
make examples
make readme
make bench-smoke

echo "regression: every gate passed against crypto-service $(git -C "$checkout" rev-parse --short HEAD 2> /dev/null || echo checkout)"
