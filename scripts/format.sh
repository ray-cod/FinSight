#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cd "$ROOT_DIR"

echo "==> Checking formatting..."

dotnet format FinSight.slnx \
    --verify-no-changes \
    --no-restore

echo "==> Formatting check passed."
