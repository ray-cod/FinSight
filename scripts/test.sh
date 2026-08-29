#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cd "$ROOT_DIR"

echo "==> Running tests..."

dotnet test FinSight.slnx \
    --configuration Release \
    --no-restore \
    --logger "trx;LogFileName=test-results.trx" \
    --collect:"XPlat Code Coverage"

echo "==> Tests completed successfully."
