# FinSight Deployment Strategy

## Environments

### Development

Local developer environment.

Infrastructure:

- PostgreSQL
- Redis
- RabbitMQ

### Staging

Production-like environment used for validation.

### Production

Live application environment.

## Deployment Principle

Application artifacts are built once and promoted between environments where possible.

Production deployment requires:

1. CI success
2. Security checks passing
3. Staging validation
4. Pull request approval
5. Production deployment authorization

## Frontend Deployment

The Next.js frontend is packaged as an OCI image and published to GitHub Container Registry during staging and production release workflows.

The canonical release identifier is the Git SHA image tag. Environment tags are convenience aliases and should not be used as rollback identifiers.

When the target environment has `WEB_DEPLOYMENT_ENABLED=true`, the workflow invokes the environment-specific deployment webhook with the immutable frontend image reference. The receiving deployment platform is responsible for rolling out that image using its native strategy.

After a successful deployment request, CI calls the frontend `/api/health` endpoint using `WEB_BASE_URL`.
