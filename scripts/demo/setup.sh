#!/usr/bin/env bash
set -euo pipefail

API_BASE_URL="${API_BASE_URL:-http://localhost:5059}"
WEB_BASE_URL="${WEB_BASE_URL:-http://localhost:3000}"
DEMO_EMAIL="${DEMO_EMAIL:-demo@finsight.local}"
DEMO_PASSWORD="${DEMO_PASSWORD:-FinSightDemo!2026}"
DEMO_DISPLAY_NAME="${DEMO_DISPLAY_NAME:-FinSight Demo User}"
MIN_TRANSACTIONS="${MIN_TRANSACTIONS:-30}"

require_command() {
  command -v "$1" >/dev/null 2>&1 || {
    echo "Required command not found: $1" >&2
    exit 1
  }
}

require_command curl
require_command python3

api() {
  curl --fail-with-body --silent --show-error \
    --header "Accept: application/json" \
    "$@"
}

post_json() {
  local url="$1"
  local body="$2"
  curl --silent --show-error --header "Content-Type: application/json" \
    --header "Accept: application/json" --data "$body" "$url"
}

echo "Checking FinSight API health at ${API_BASE_URL}..."
for _ in $(seq 1 30); do
  if curl --silent --fail "${API_BASE_URL}/health/live" >/dev/null; then
    break
  fi
  sleep 1
done
curl --silent --fail "${API_BASE_URL}/health/live" >/dev/null || {
  echo "FinSight API is not reachable. Start the API first." >&2
  exit 1
}

echo "Preparing portfolio demo account: ${DEMO_EMAIL}"
register_payload=$(python3 - <<'PYSETUP'
import json, os
print(json.dumps({
    "email": os.environ.get("DEMO_EMAIL", "demo@finsight.local"),
    "password": os.environ.get("DEMO_PASSWORD", "FinSightDemo!2026"),
    "displayName": os.environ.get("DEMO_DISPLAY_NAME", "FinSight Demo User"),
}))
PYSETUP
)
post_json "${API_BASE_URL}/api/v1/auth/register" "$register_payload" >/dev/null 2>&1 || true

login_payload=$(python3 - <<'PYSETUP'
import json, os
print(json.dumps({
    "email": os.environ.get("DEMO_EMAIL", "demo@finsight.local"),
    "password": os.environ.get("DEMO_PASSWORD", "FinSightDemo!2026"),
}))
PYSETUP
)

auth_json=$(post_json "${API_BASE_URL}/api/v1/auth/login" "$login_payload" || true)
access_token=$(printf '%s' "$auth_json" | python3 -c 'import json,sys; data=json.load(sys.stdin); print(data["accessToken"])' 2>/dev/null || true)

if [[ -z "$access_token" ]]; then
  echo "Could not authenticate the demo account." >&2
  exit 1
fi

auth_header=(--header "Authorization: Bearer ${access_token}")
connections_json=$(api "${auth_header[@]}" "${API_BASE_URL}/api/v1/accounts/connections")
connection_id=$(printf '%s' "$connections_json" | python3 -c 'import json,sys; data=json.load(sys.stdin); print(data[0]["id"] if data else "")')

if [[ -z "$connection_id" ]]; then
  echo "Connecting the deterministic demo bank..."
  connect_json=$(curl --fail-with-body --silent --show-error \
    --header "Authorization: Bearer ${access_token}" \
    --header "Content-Type: application/json" \
    --header "Accept: application/json" \
    --data '{"institutionCode":"MOCK_BANK"}' \
    "${API_BASE_URL}/api/v1/accounts/connections")
  connection_id=$(printf '%s' "$connect_json" | python3 -c 'import json,sys; data=json.load(sys.stdin); print(data["id"] if isinstance(data, dict) and "id" in data else data)')
fi

echo "Synchronizing deterministic mock-bank data..."
curl --fail-with-body --silent --show-error \
  --header "Authorization: Bearer ${access_token}" \
  --header "Accept: application/json" \
  --request POST "${API_BASE_URL}/api/v1/accounts/connections/${connection_id}/sync" >/dev/null

printf 'Waiting for transaction intelligence workers'
transactions_ready=0
subscriptions_ready=0
anomalies_ready=0
insights_ready=0
for _ in $(seq 1 45); do
  transactions_json=$(api "${auth_header[@]}" "${API_BASE_URL}/api/v1/transactions?page=1&pageSize=1")
  transaction_count=$(printf '%s' "$transactions_json" | python3 -c 'import json,sys; print(json.load(sys.stdin).get("totalCount",0))')
  subscriptions_json=$(api "${auth_header[@]}" "${API_BASE_URL}/api/v1/subscriptions")
  subscription_count=$(printf '%s' "$subscriptions_json" | python3 -c 'import json,sys; print(len(json.load(sys.stdin)))')
  anomalies_json=$(api "${auth_header[@]}" "${API_BASE_URL}/api/v1/anomalies")
  anomaly_count=$(printf '%s' "$anomalies_json" | python3 -c 'import json,sys; print(len(json.load(sys.stdin)))')
  insights_json=$(api "${auth_header[@]}" "${API_BASE_URL}/api/v1/insights")
  insight_count=$(printf '%s' "$insights_json" | python3 -c 'import json,sys; print(len(json.load(sys.stdin)))')

  [[ "$transaction_count" -ge "$MIN_TRANSACTIONS" ]] && transactions_ready=1
  [[ "$subscription_count" -ge 2 ]] && subscriptions_ready=1
  [[ "$anomaly_count" -ge 2 ]] && anomalies_ready=1
  [[ "$insight_count" -ge 1 ]] && insights_ready=1

  if [[ "$transactions_ready" -eq 1 && "$subscriptions_ready" -eq 1 && "$anomalies_ready" -eq 1 && "$insights_ready" -eq 1 ]]; then
    break
  fi
  printf '.'
  sleep 1
done
printf '
'

echo
echo "Demo URL:      ${WEB_BASE_URL}/login"
echo "Email:         ${DEMO_EMAIL}"
echo "Password:      ${DEMO_PASSWORD}"
echo
echo "Suggested flow:"
echo "  1. Dashboard       -> financial overview"
echo "  2. Transactions    -> inspect the large Amazon charge"
echo "  3. Subscriptions   -> review Netflix price change"
echo "  4. Anomalies       -> inspect explainable evidence"
echo "  5. Insights        -> follow signal back to source"
echo "  6. Notifications   -> review the generated alert"
