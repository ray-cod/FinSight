#!/usr/bin/env bash
set -euo pipefail

echo "FinSight demo data is reset through the normal local database lifecycle."
echo "The portfolio setup script is intentionally idempotent; rerun ./scripts/demo/setup.sh to reuse the demo account and synchronize the deterministic fixture."
