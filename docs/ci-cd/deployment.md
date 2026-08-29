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
