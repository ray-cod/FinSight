# FinSight Frontend CI/CD

## Continuous Integration

Frontend validation is part of the repository CI workflow for pushes and pull requests targeting `dev`, `staging`, and `main`.

The frontend CI gate runs:

1. `pnpm install --frozen-lockfile`
2. ESLint
3. TypeScript typecheck
4. Vitest unit tests
5. Production `next build`
6. Chromium installation
7. Playwright E2E tests against the production Next.js server

The E2E suite uses intercepted API contracts, so it verifies the deployed web application behavior without requiring external financial-provider infrastructure.

## Frontend release artifact

The frontend is released as an OCI container image built from `frontend/finsight-web/Dockerfile`.

Each staging/production release publishes two tags:

- immutable commit tag: `<git-sha>`
- environment tag: `staging` or `production`

The commit tag is the release-of-record and is suitable for rollback. The container is published to GitHub Container Registry under:

```text
 ghcr.io/<github-owner>/finsight-web:<git-sha>
```

The image is built with provenance and SBOM attestations and uses the Next.js standalone output with a non-root runtime user.

## Environment configuration

Each GitHub Environment should define:

| Variable | Purpose |
|---|---|
| `WEB_API_BASE_URL` | Browser-reachable FinSight API origin baked into the frontend bundle |
| `WEB_CLIENT_ERROR_ENDPOINT` | Optional browser error telemetry endpoint |
| `WEB_BASE_URL` | Public frontend URL used by deployment smoke tests |
| `WEB_DEPLOYMENT_ENABLED` | Set to `true` to enable the deployment webhook step |

Environment secrets:

| Secret | Purpose |
|---|---|
| `WEB_DEPLOY_WEBHOOK_URL` | Provider/platform deployment webhook |
| `WEB_DEPLOY_WEBHOOK_TOKEN` | Bearer token for the deployment webhook |

The deployment hook receives a JSON document containing the application, environment, immutable image reference, and Git SHA. This keeps the repository independent of a particular hosting vendor while still giving CI a concrete deployment contract.

## Deployment flow

```text
PR
 ↓
CI
 ├── frontend lint
 ├── frontend typecheck
 ├── frontend unit tests
 ├── frontend production build
 └── frontend production E2E
 ↓
staging CI success
 ↓
Build backend artifacts + frontend image from the verified CI SHA
 ↓
Publish frontend image to GHCR
 ↓
Deployment webhook (when enabled)
 ↓
/api/health smoke test
 ↓
production CI success / promotion
```

## Manual deployment / rollback

`.github/workflows/deploy-web.yml` is a manual operator workflow.

Supply an environment and an immutable image tag. This supports both a targeted redeployment and rollback to an earlier known-good frontend image without rebuilding the application.

## Release rules

1. Never deploy an untagged local image directly to an environment.
2. Prefer immutable Git SHA image tags for production.
3. Keep staging and production GitHub Environment credentials separate.
4. Do not store API secrets in `NEXT_PUBLIC_*` variables; they are intentionally browser-visible.
5. Keep deployment webhook credentials in GitHub Environment Secrets.
6. Verify the web health endpoint after deployment.
