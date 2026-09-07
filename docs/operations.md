# FinSight Operations Guide

## Local service map

| Service | Port | Purpose |
|---|---:|---|
| API | 5059 | HTTP application API |
| PostgreSQL | 5432 | Primary data store |
| Redis | 6379 | Classification cache |
| RabbitMQ | 5672 | Event broker |
| RabbitMQ Management | 15672 | Broker administration |
| OTLP gRPC | 4317 | OpenTelemetry input |
| OTLP HTTP | 4318 | OpenTelemetry input |
| Jaeger | 16686 | Trace UI |
| Prometheus | 9090 | Metrics |
| Grafana | 3000 | Metrics dashboards |
| Mailpit SMTP | 1025 | Local email delivery |
| Mailpit UI | 8025 | Local email inspection |

## Startup order

Recommended local startup:

```text
PostgreSQL / Redis / RabbitMQ
            |
            v
         API + Workers
            |
            v
     Telemetry/monitoring
```

Docker health checks are defined for PostgreSQL, Redis and RabbitMQ.

## Readiness

Use:

```text
GET /health/ready
```

to verify application dependencies that are tagged for readiness.

## Logging

FinSight uses Serilog with console sinks and request logging. Correlation IDs are added by middleware to connect HTTP activity with downstream work.

## Tracing and metrics

OpenTelemetry exports through OTLP. Jaeger provides local trace visualization, while Prometheus/Grafana provide local metrics observation.

## Outbox operations

The outbox dispatcher processes messages in batches and retries failures with exponential backoff. Messages that exceed the maximum attempt count are marked dead-lettered for operational inspection.

## RabbitMQ dead letters

Unprocessable messages are routed to:

```text
finsight.events.dlx
```

and:

```text
finsight.events.dead-letter
```

## Notification delivery failures

Notifications are retried up to five attempts. After the terminal threshold is reached, the notification status becomes `DeadLettered`.

## Database changes

Database migrations are forward-oriented. In production, prefer:

- backward-compatible application changes;
- expand/contract migration patterns;
- forward fixes instead of blind destructive rollback.

## Secrets

Secrets belong in environment variables or deployment-secret stores. The development signing key and sample credentials must never be promoted to production.

## Incident rollback

Application rollback:

1. stop further promotion;
2. identify the last known-good build;
3. redeploy it;
4. verify liveness/readiness;
5. verify critical business flows;
6. record the incident.

Database rollback must not be attempted blindly in production.
