# FinSight CI/CD Pipeline

## Continuous Integration

CI runs on:

- Pull requests to `dev`
- Pull requests to `staging`
- Pull requests to `main`
- Pushes to `dev`
- Pushes to `staging`
- Pushes to `main`

## CI stages

1. Restore
2. Build
3. Test
4. Formatting
5. Dependency security

## Dependency Review

Pull requests are checked for newly introduced vulnerable dependencies.

High-severity dependency vulnerabilities fail the dependency review.

## Security

Dependency vulnerability checks run on pushes, pull requests, and on a weekly schedule.

## Continuous Deployment

`dev` represents active development.

`staging` represents the pre-production environment.

`main` represents production.

Deployment workflows are separated so that staging and production credentials remain isolated.

## Frontend CI

The CI workflow also validates `frontend/finsight-web` on every push and pull request:

1. Install with the committed pnpm lockfile.
2. Lint.
3. Typecheck.
4. Run unit tests.
5. Produce a production Next.js build.
6. Run Playwright against that production build.

## Frontend release

After a successful CI workflow run on `staging` or `main`, the corresponding CD workflow publishes the frontend as an OCI image from the exact verified CI SHA to GHCR using an immutable Git SHA tag plus an environment tag. The image includes provenance/SBOM attestations and is built from the Next.js standalone output.

Deployment is triggered through an environment-specific webhook when `WEB_DEPLOYMENT_ENABLED=true`. A health smoke test calls `/api/health` after deployment.

A manual workflow supports deploying or rolling back to an existing immutable frontend image tag.
