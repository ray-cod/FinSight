# FinSight Secrets

Secrets must never be committed to the repository.

## Local development

Use:

.env

The file must remain ignored by Git.

## CI/CD

Use GitHub Actions Secrets / Environment Secrets.

Expected categories:

- Database credentials
- JWT signing secrets
- Redis credentials
- RabbitMQ credentials
- AI provider credentials
- Email provider credentials
- Deployment credentials
- Frontend deployment webhook token

## Rules

1. Never hardcode credentials.
2. Never print secrets to logs.
3. Never place secrets in source code.
4. Use environment-specific credentials.
5. Rotate credentials periodically.
6. Use the minimum required permissions.

GitHub Secret Scanning and Push Protection should be enabled where supported by the repository plan/settings.

## Frontend Environment Variables

The frontend release pipeline also uses GitHub Environment Variables:

- `WEB_API_BASE_URL`
- `WEB_CLIENT_ERROR_ENDPOINT`
- `WEB_BASE_URL`
- `WEB_DEPLOYMENT_ENABLED`

The deployment webhook URL and bearer token belong in Environment Secrets as `WEB_DEPLOY_WEBHOOK_URL` and `WEB_DEPLOY_WEBHOOK_TOKEN`.
