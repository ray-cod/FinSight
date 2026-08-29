#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cd "$ROOT_DIR"

echo "==> Checking vulnerable NuGet packages..."

dotnet list FinSight.slnx package \
    --vulnerable \
    --include-transitive

echo "==> Security dependency check completed."
