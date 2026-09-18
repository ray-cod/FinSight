#!/usr/bin/env bash

set -euo pipefail

: "${WEB_DEPLOY_WEBHOOK_URL:?WEB_DEPLOY_WEBHOOK_URL must be configured for web deployment}"
: "${WEB_DEPLOY_WEBHOOK_TOKEN:?WEB_DEPLOY_WEBHOOK_TOKEN must be configured for web deployment}"
: "${WEB_IMAGE:?WEB_IMAGE must be provided}"
: "${WEB_ENVIRONMENT:?WEB_ENVIRONMENT must be provided}"
: "${WEB_GIT_SHA:?WEB_GIT_SHA must be provided}"

payload=$(cat <<JSON
{
  "application": "finsight-web",
  "environment": "${WEB_ENVIRONMENT}",
  "image": "${WEB_IMAGE}",
  "gitSha": "${WEB_GIT_SHA}"
}
JSON
)

curl --fail-with-body --silent --show-error \
  --max-time 30 \
  --request POST \
  --url "${WEB_DEPLOY_WEBHOOK_URL}" \
  --header "Authorization: Bearer ${WEB_DEPLOY_WEBHOOK_TOKEN}" \
  --header "Content-Type: application/json" \
  --data "${payload}"

echo "FinSight Web deployment request accepted for ${WEB_ENVIRONMENT}: ${WEB_IMAGE}"
