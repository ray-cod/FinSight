#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cd "$ROOT_DIR"

echo "==> Restoring solution..."
dotnet restore FinSight.slnx

echo "==> Building solution..."
dotnet build FinSight.slnx \
    --configuration Release \
    --no-restore

echo "==> Build completed successfully."
