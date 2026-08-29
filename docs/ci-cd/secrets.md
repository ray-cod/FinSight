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

## Rules

1. Never hardcode credentials.
2. Never print secrets to logs.
3. Never place secrets in source code.
4. Use environment-specific credentials.
5. Rotate credentials periodically.
6. Use the minimum required permissions.

GitHub Secret Scanning and Push Protection should be enabled where supported by the repository plan/settings.
