#!/usr/bin/env bash

set -euo pipefail

: "${WEB_BASE_URL:?WEB_BASE_URL must be configured for web smoke tests}"

WEB_BASE_URL="${WEB_BASE_URL%/}"
HEALTH_URL="${WEB_BASE_URL}/api/health"

response=$(curl --fail-with-body --silent --show-error \
  --retry 10 \
  --retry-all-errors \
  --retry-delay 3 \
  --max-time 20 \
  "${HEALTH_URL}")

printf '%s' "${response}" | grep -F '"status":"ok"' >/dev/null
printf '%s' "${response}" | grep -F '"service":"finsight-web"' >/dev/null

echo "FinSight Web smoke test passed: ${HEALTH_URL}"
