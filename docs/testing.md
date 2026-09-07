# FinSight Testing Strategy

## Test pyramid

```text
                 +----------------------+
                 | Integration tests    |
                 +----------------------+
                 | Infrastructure tests |
                 +----------------------+
                 | Application tests    |
                 +----------------------+
                 | Domain tests          |
                 +----------------------+
                 | Architecture tests   |
                 +----------------------+
```

## Projects

### Domain tests

`tests/FinSight.Domain.Tests`

Current coverage areas include:

- accounts;
- transactions;
- subscriptions;
- anomalies;
- financial insights;
- notifications.

### Application tests

`tests/FinSight.Application.Tests`

Current example:

- subscription detection service behavior.

### Infrastructure tests

`tests/FinSight.Infrastructure.Tests`

Current areas include:

- merchant/category rule classification;
- anomaly detection;
- insight generation;
- outbox persistence;
- telemetry.

### Integration tests

`tests/FinSight.IntegrationTests`

Uses Testcontainers support for PostgreSQL and RabbitMQ.

### Architecture tests

`tests/FinSight.Architecture.Tests`

Protects dependency direction and layer boundaries.

## Running tests

```bash
dotnet test FinSight.slnx
```

Or:

```bash
make test
```

## Test design expectations

New functionality should normally include tests at the lowest relevant level:

- invariants → Domain;
- orchestration → Application;
- adapter/provider behavior → Infrastructure;
- cross-component behavior → IntegrationTests;
- architectural constraints → ArchitectureTests.

## Important scenarios

### Transactions

- idempotent import;
- user ownership isolation;
- normalization;
- rule match;
- cache hit;
- AI fallback;
- invalid AI result;
- user correction prevents automatic reclassification.

### Subscriptions

- monthly/quarterly/annual cadence inference;
- insufficient history;
- inconsistent cadence;
- inconsistent amount;
- material price changes.

### Anomalies

- threshold detection;
- severity mapping;
- evidence persistence;
- lifecycle transitions;
- duplicate-message protection.

### Notifications

- preference behavior;
- deduplication;
- successful delivery;
- retryable failure;
- dead-letter behavior;
- read lifecycle.

### Reliability

- outbox persistence;
- publication retry/backoff;
- dead-letter threshold;
- processed-message idempotency.
