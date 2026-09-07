# FinSight Architecture

This document describes the architecture implemented in the current repository snapshot.

## Architectural style

FinSight is a **layered modular monolith with event-driven background processing**.

It deliberately avoids premature microservice decomposition while still separating synchronous request handling from expensive, asynchronous, retryable operations.

## Runtime components

```text
Browser / Next.js
       |
       v
FinSight.Api
       |
       v
FinSight.Application
       |
       +------------------+
       |                  |
       v                  v
FinSight.Domain     Infrastructure ports
                          |
                          +--> PostgreSQL
                          +--> Redis
                          +--> RabbitMQ
                          +--> OpenAI
                          +--> SMTP
                          +--> OpenTelemetry

RabbitMQ
   |
   v
FinSight.Workers
```

## Dependency direction

The intended dependency rule is:

```text
Domain <- Application <- Infrastructure
              ^
              |
             API
```

Workers depend on Application/Infrastructure/Contracts as needed for background execution.

The Architecture Test project verifies that:

- Domain does not reference Infrastructure;
- Domain does not reference Application;
- Application does not reference Infrastructure;
- Application does reference Domain.

## Application boundaries

### Domain

Owns business entities and lifecycle behavior.

### Application

Owns orchestration, use cases and abstractions for persistence, banking, AI, caching, messaging, notifications and observability.

### Infrastructure

Owns implementation details: EF Core, PostgreSQL, Identity, JWT, Redis, RabbitMQ, OpenAI, SMTP, health checks, outbox and telemetry.

### API

Owns HTTP concerns only: controllers, middleware, authentication/authorization boundary, CORS, rate limiting and health endpoints.

### Workers

Owns asynchronous processing and scheduled jobs so expensive work is not coupled to web request latency.

## Synchronous vs asynchronous responsibilities

### Synchronous

- registration/login/refresh/logout;
- profile updates;
- account listing and connection initiation;
- reading accounts/transactions/subscriptions/anomalies/insights/notifications;
- correcting transaction classification;
- resolving/dismissing financial findings;
- reading/updating notification preferences.

### Asynchronous

- scheduled bank synchronization;
- imported transaction categorization;
- subscription detection;
- anomaly detection;
- insight generation;
- subscription price-change analysis;
- notification delivery;
- anomaly/insight lifecycle jobs;
- audit retention;
- outbox retention;
- outbox publication to RabbitMQ.

## Transaction intelligence architecture

```text
Imported transaction
        |
        v
MerchantNormalizer
        |
        v
CategoryRuleEngine
        |
        +---- match -> persist classification
        |
        +---- no match
                  |
                  v
                Redis
                  |
          +-------+-------+
          |               |
        hit              miss
          |               |
          |               v
          |            OpenAI
          |               |
          +-------+-------+
                  |
                  v
        Validate + resolve merchant
                  |
                  v
          Persist transaction
                  |
                  v
      transaction.categorized
```

## Reliability architecture

The reliability chain is:

```text
DB write + OutboxMessage
          |
          v
   OutboxDispatcher
          |
          v
       RabbitMQ
          |
          v
      Consumer
          |
          v
 ProcessedMessage guard
          |
          v
   Business operation
```

Failed outbox publication uses backoff; unprocessable RabbitMQ messages have a dead-letter path.

## Observability architecture

```text
API / Worker
   |
   +--> Serilog console logs
   |
   +--> OpenTelemetry traces
   |
   +--> OpenTelemetry metrics
              |
              v
      OTLP Collector
         |        |
         v        v
      Jaeger   Prometheus
                   |
                   v
                Grafana
```
