# FinSight

## The Smart Subscription & Financial Anomaly Engine

FinSight is a portfolio-grade financial intelligence platform designed to detect the everyday sources of unnecessary spending that traditional budgeting tools often miss: forgotten subscriptions, subscription price increases, unusual transactions, duplicate-looking charges, new merchants, spending spikes, and other financially meaningful events.

The product is intentionally designed around **automatic financial understanding rather than manual budgeting**. A user connects a supported financial institution, FinSight imports transaction data from a mock banking provider, normalizes and classifies transactions, detects recurring subscriptions and suspicious patterns, generates user-readable financial insights, and delivers notifications.

> **Portfolio positioning:** FinSight demonstrates production-oriented backend engineering with ASP.NET Core/.NET, layered architecture, PostgreSQL/EF Core, Redis, RabbitMQ, asynchronous workers, OpenAI-powered transaction categorization, resilience, observability, auditability, and automated testing.

---

## 1. Product Problem

People often lose money through charges they do not actively monitor:

- forgotten or underused subscriptions;
- subscription price increases;
- unusual or unexpectedly large purchases;
- repeated transactions that may represent duplicates;
- new merchants or sudden merchant spending increases;
- category-level spending spikes.

Conventional budgeting applications frequently depend on the user entering, labeling, and reviewing financial activity manually. That creates friction and reduces long-term engagement.

FinSight addresses the problem with an automated pipeline:

```text
Connect account
      |
      v
Mock bank synchronization
      |
      v
Persist transaction safely
      |
      v
Normalize merchant description
      |
      v
Rule classification
      |
      +----> Redis cache hit
      |
      +----> OpenAI classification fallback
      |
      v
Merchant + category persisted
      |
      +----> Subscription detection
      |
      +----> Anomaly detection
      |            |
      |            v
      |       Financial insight
      |            |
      |            v
      |       Notification
      |
      v
User-facing APIs
```

---

## 2. Current Repository Status

This documentation describes the repository snapshot on the `dev` branch at commit `49ddc7b`.

The backend currently contains the following implemented areas:

| Area | Current state |
|---|---|
| CI/CD & engineering automation | Implemented |
| Backend foundation & layered architecture | Implemented |
| Identity, authentication & security | Implemented |
| Financial institutions, accounts & mock banking | Implemented |
| Transaction persistence & processing | Implemented |
| Merchant normalization | Implemented |
| Rule-based transaction categorization | Implemented |
| Redis classification caching | Implemented |
| OpenAI transaction categorization | Implemented |
| Subscription detection | Implemented |
| Subscription price history / price-change eventing | Implemented |
| Anomaly detection | Implemented |
| Financial insights | Implemented |
| Notifications & notification preferences | Implemented |
| Transactional outbox | Implemented |
| Idempotent processed-message tracking | Implemented |
| RabbitMQ retry/dead-letter topology | Implemented |
| Audit events & retention worker | Implemented |
| Serilog logging | Implemented |
| OpenTelemetry / Prometheus / Jaeger / Grafana integration | Implemented |
| API health endpoints | Implemented |
| Backend tests | Present |
| Next.js frontend | Initial scaffold; not yet connected to the backend |

The current source tree shows a much more complete system than the original placeholder README represented.

---

## 3. Architecture Overview

FinSight is implemented as a **modular monolith with asynchronous, event-driven processing** rather than a collection of independently deployed microservices.

The main runtime components are:

```text
                    +-----------------------+
                    |  Next.js Web Client    |
                    |  frontend/finsight-web |
                    +-----------+-----------+
                                |
                                | HTTP/JSON
                                v
                    +-----------------------+
                    |      FinSight.Api      |
                    | Controllers / HTTP     |
                    | Auth / Middleware      |
                    +-----------+-----------+
                                |
                                v
                    +-----------------------+
                    | FinSight.Application   |
                    | Use cases / services   |
                    | abstractions           |
                    +-----------+-----------+
                                |
                    +-----------+-----------+
                    |                       |
                    v                       v
          +-------------------+   +-------------------+
          | FinSight.Domain   |   | FinSight.Contracts|
          | entities / rules  |   | integration events|
          +-------------------+   +-------------------+
                    ^
                    |
                    v
          +-----------------------+
          | FinSight.Infrastructure|
          | EF Core / PostgreSQL   |
          | Identity / JWT         |
          | Redis / RabbitMQ       |
          | OpenAI / SMTP          |
          | Outbox / telemetry     |
          +-----------+------------+
                      |
                      | asynchronous events
                      v
          +-----------------------+
          |    FinSight.Workers   |
          | sync / consumers /    |
          | lifecycle / retention |
          +-----------------------+
```

### Architectural principle

The dependency direction is intentionally inward:

```text
API ------------> Application ------------> Domain
                    ^
                    |
              Infrastructure

Workers ----------> Infrastructure/Application/Contracts
Contracts ---------> standalone shared event contracts
```

The architecture tests explicitly protect the most important dependency constraints: Domain must not depend on Application or Infrastructure, and Application must not depend on Infrastructure.

---

## 4. Solution Projects

The solution is `FinSight.slnx` and contains six production projects and five test projects.

### Production projects

| Project | Responsibility |
|---|---|
| `FinSight.Domain` | Core entities, value objects, enums, domain lifecycle rules and shared domain primitives |
| `FinSight.Application` | Application services, use cases, contracts, ports/interfaces, validation and orchestration |
| `FinSight.Contracts` | Integration events exchanged through RabbitMQ |
| `FinSight.Infrastructure` | EF Core/PostgreSQL, Identity, JWT/refresh tokens, Redis, RabbitMQ, OpenAI, SMTP, outbox, observability and resilience |
| `FinSight.Api` | HTTP API, authentication boundary, controllers, middleware, CORS, rate limiting and health endpoints |
| `FinSight.Workers` | Background synchronization, RabbitMQ consumers, lifecycle jobs and retention jobs |

### Test projects

| Project | Focus |
|---|---|
| `FinSight.Domain.Tests` | Domain behavior and invariants |
| `FinSight.Application.Tests` | Application-level business logic |
| `FinSight.Infrastructure.Tests` | Infrastructure components such as AI rules, anomaly/insight logic, outbox and telemetry |
| `FinSight.IntegrationTests` | Cross-component infrastructure and container-backed integration scenarios |
| `FinSight.Architecture.Tests` | Layer dependency constraints |

---

## 5. Domain Model

The implemented domain is organized around the following areas.

### Identity & security

- `User`
- `UserId`
- `UserStatus`
- ASP.NET Core Identity `ApplicationUser` / `ApplicationRole`
- `RefreshToken`
- `AuditEvent`
- `SecurityEventType`

### Banking

- `Institution`
- `AccountConnection`
- `FinancialAccount`
- `AccountId`
- `AccountType`
- `AccountStatus`
- `ConnectionStatus`

### Transactions

- `Transaction`
- `TransactionId`
- `TransactionType`
- `TransactionStatus`
- `ClassificationStatus`
- `ClassificationSource`

A transaction keeps both its provider-level description and its normalized description. It can then be associated with a normalized merchant and a category/subcategory.

### Merchants

- `Merchant`
- `MerchantAlias`

Merchant normalization is intentionally separated from categorization so that multiple raw descriptions can resolve to a common merchant identity.

### Categories

- `Category`
- `Subcategory`
- `CategoryType`

The category taxonomy is seeded by `CategorySeedService`.

### Subscriptions

- `Subscription`
- `SubscriptionPriceHistory`
- `BillingFrequency`
- `SubscriptionStatus`

Subscriptions record current and average charge amounts, detection confidence, first/last charge dates, expected next charge, and price-change timing.

### Anomalies

- `Anomaly`
- `AnomalySeverity`
- `AnomalyStatus`
- `AnomalyType`

The anomaly model includes a score, confidence, evidence, lifecycle status, and links back to the relevant transaction/account context.

### Financial insights

- `FinancialInsight`
- `InsightType`
- `InsightSeverity`
- `InsightStatus`

Insights are the user-readable interpretation layer above raw anomalies and other financial events.

### Notifications

- `Notification`
- `NotificationPreference`
- `NotificationType`
- `NotificationChannel`
- `NotificationStatus`

Notifications support in-app and email-oriented delivery, deduplication, retries and terminal dead-letter state.

### Reliability infrastructure

- `OutboxMessage`
- `ProcessedMessage`

These support reliable event publication and duplicate-message protection.

---

## 6. Transaction Intelligence Pipeline

FinSight classifies imported transactions through a deliberate hybrid pipeline.

### Step 1: Merchant normalization

The raw transaction description is normalized before classification. Normalization is handled through the `IMerchantNormalizer` abstraction and the infrastructure implementation.

### Step 2: Deterministic rule engine

The first classification attempt uses `MerchantCategoryRuleEngine`.

The currently seeded deterministic examples include patterns for:

- Netflix → Entertainment / Streaming
- Spotify → Entertainment / Streaming
- Amazon / AMZN → Shopping / Online Shopping
- Uber → Transportation / Rideshare
- Joe's Coffee → Food & Dining / Coffee
- Woolworths → Food & Dining / Groceries

Rule matches carry a confidence of `0.99`.

### Step 3: Redis cache

If no deterministic rule matches, FinSight checks Redis using a normalized-description cache key.

The classification cache is configured with a 30-day expiration in the transaction-processing service.

### Step 4: AI fallback

On a cache miss, the transaction is sent to the OpenAI-backed `ITransactionCategorizer` implementation.

The request includes:

- raw description;
- normalized description;
- amount;
- currency;
- transaction type;
- active category/subcategory options.

The AI is required to return structured JSON containing a merchant, category code, optional subcategory code, confidence, and rationale.

The result is validated before persistence. In particular, FinSight rejects:

- empty merchants;
- confidence outside `0..1`;
- unknown category codes;
- invalid subcategory/category combinations.

### Step 5: Merchant resolution and persistence

A merchant is resolved through `MerchantResolutionService`, classification metadata is stored on the transaction, and a `TransactionCategorizedEvent` is emitted with the source and confidence.

### User correction rule

Transactions explicitly corrected by a user are marked `UserCorrected` and are skipped by automated reclassification.

---

## 7. Subscription Intelligence

Subscription detection is implemented as a deterministic recurring-payment analysis over transaction history for a normalized merchant and currency.

The detector currently:

1. examines up to the most recent 36 transactions for the merchant;
2. orders the transactions chronologically;
3. computes the intervals between charges;
4. infers weekly, bi-weekly, monthly, quarterly, semi-annual or annual cadence from the median interval;
5. requires a minimum number of recurring charges;
6. evaluates cadence consistency;
7. evaluates amount consistency;
8. calculates a confidence score;
9. estimates the next expected charge date;
10. identifies a material price change when the latest charge changes by at least 5% from the prior charge.

For annual subscriptions, the implementation allows detection from two charges; other frequencies require at least three recurring charges.

A recurring pattern is rejected when cadence consistency or calculated confidence is below the configured threshold implemented by the detector.

### Subscription price-change flow

```text
Transaction categorized
        |
        v
Subscription detector
        |
        +---- recurring subscription found
        |
        v
Subscription persisted/updated
        |
        +---- material price change
                 |
                 v
       subscription.price.changed
                 |
                 v
      Insight / anomaly processing
                 |
                 v
            Notification
```

---

## 8. Anomaly Detection

Anomaly detection is implemented as deterministic statistical/business-rule analysis rather than an LLM-only decision layer.

The domain currently supports anomaly categories represented by the `AnomalyType` enum, including:

- large transactions;
- merchant spending spikes;
- category spending spikes;
- new merchants;
- possible duplicate transactions.

Detected anomalies are persisted with severity, score, confidence, title, description, evidence, status, and detection time.

The resulting `AnomalyDetectedEvent` is then consumed asynchronously.

---

## 9. Financial Insights

The insight layer translates technical anomaly output into human-readable financial explanations.

`InsightGenerator` maps anomaly types to insight types such as:

| Anomaly | Insight |
|---|---|
| Large transaction | Unusual transaction |
| Merchant spending spike | Merchant spending increase |
| Category spending spike | Category spending increase |
| New merchant | New merchant |
| Duplicate transaction | Possible duplicate |

Severity is propagated into an insight severity appropriate for user-facing presentation.

Insights support an explicit lifecycle:

```text
Active -> Seen
   |
   +----> Dismissed
   |
   +----> Expired
```

---

## 10. Notifications

FinSight creates notifications as persisted domain records rather than treating delivery as a side effect of a controller request.

Supported delivery concepts include:

- in-app notifications;
- email delivery;
- notification preferences;
- deduplication keys;
- delivery attempt counters;
- failure tracking;
- dead-lettered notifications.

Notification delivery is asynchronous and is driven by the `notification.created` event.

The delivery service allows up to five delivery attempts before the notification is moved to a terminal dead-lettered state.

For local development, SMTP can be routed to **Mailpit**.

---

## 11. Event-Driven Processing

RabbitMQ is the backbone for background processing between application steps.

### Exchanges

- `finsight.events` — primary durable topic exchange
- `finsight.events.retry` — retry exchange name reserved by topology/configuration
- `finsight.events.dlx` — dead-letter exchange

### Queues

| Queue | Routing key | Consumer responsibility |
|---|---|---|
| `finsight.transaction-categorization` | `transaction.imported` | AI/rule transaction classification |
| `finsight.subscription-detection` | `transaction.categorized` | Subscription detection |
| `finsight.anomaly-detection` | `transaction.categorized` | Anomaly evaluation |
| `finsight.insight-generation` | `anomaly.detected` | Financial insight generation |
| `finsight.subscription-price-anomaly` | `subscription.price.changed` | Price-change intelligence |
| `finsight.notification-delivery` | `notification.created` | Notification delivery |
| `finsight.events.dead-letter` | `#` on DLX | Failed/unprocessable messages |

The worker processes also use `ProcessedMessage` to provide an application-level guard against duplicate processing.

---

## 12. Transactional Outbox

FinSight uses a transactional outbox to reduce the reliability gap between database persistence and RabbitMQ publication.

The pattern is:

```text
Application operation
      |
      +---- update business data
      |
      +---- add OutboxMessage
      |
      v
   DB transaction
      |
      v
OutboxDispatcher
      |
      v
RabbitMQ
```

`OutboxDispatcher` currently:

- reads pending messages in batches of 50;
- publishes through the reliable RabbitMQ publisher;
- marks successfully published messages;
- retries failed messages using exponential backoff up to a five-minute cap;
- dead-letters messages after ten attempts.

Outbox retention is handled by a dedicated background worker.

---

## 13. Reliability and Resilience

The current backend includes multiple reliability mechanisms:

- idempotent transaction persistence using provider identifiers/fingerprints;
- transactional outbox publication;
- processed-message storage;
- RabbitMQ dead-letter routing;
- retry/backoff for outbox publication;
- notification delivery retries;
- HTTP resilience package support;
- health checks for critical infrastructure dependencies;
- correlation IDs for request tracing;
- centralized exception handling;
- application lifecycle logging.

These mechanisms are intentionally visible in the architecture because they demonstrate concerns expected in production systems rather than in a basic CRUD portfolio application.

---

## 14. Authentication and Security

FinSight uses ASP.NET Core Identity for identity management and an application-level authentication API around JWT access tokens and rotated refresh tokens.

### Authentication flow

```text
Register/Login
      |
      v
ASP.NET Core Identity
      |
      v
JWT access token + refresh token
      |
      v
Authenticated API call
```

Refresh tokens are persisted as hashes and indexed uniquely. Refresh token rotation and revocation are handled by the identity infrastructure.

### Implemented security controls

- authentication and authorization policies;
- ownership-scoped repository/service access;
- password change and reset endpoints;
- rate limiting on authentication endpoints;
- API-wide rate limiting policy;
- security headers middleware;
- HTTPS/HSTS outside development;
- CORS allow-list configuration;
- security/audit events;
- secret separation through configuration/environment variables;
- no account-existence disclosure through password-reset request responses.

The security model is:

```text
Authentication -> Authorization -> Ownership
```

Authentication answers **who are you?**
Authorization answers **what may you do?**
Ownership answers **which records may you access?**

---

## 15. API

All business endpoints use the versioned prefix:

```text
/api/v1
```

### Authentication

| Method | Endpoint | Auth |
|---|---|---|
| POST | `/api/v1/auth/register` | Anonymous |
| POST | `/api/v1/auth/login` | Anonymous |
| POST | `/api/v1/auth/refresh` | Anonymous |
| POST | `/api/v1/auth/logout` | Anonymous |
| POST | `/api/v1/auth/change-password` | Authenticated |
| POST | `/api/v1/auth/forgot-password` | Anonymous |
| POST | `/api/v1/auth/reset-password` | Anonymous |

### Users

| Method | Endpoint | Auth |
|---|---|---|
| GET | `/api/v1/users/me` | Authenticated |
| PATCH | `/api/v1/users/me` | Authenticated |

### Institutions and accounts

| Method | Endpoint | Auth |
|---|---|---|
| GET | `/api/v1/institutions` | Authenticated |
| GET | `/api/v1/accounts` | Authenticated |
| GET | `/api/v1/accounts/{accountId}` | Authenticated |
| POST | `/api/v1/accounts/connections` | Authenticated |
| POST | `/api/v1/accounts/connections/{connectionId}/sync` | Authenticated |
| DELETE | `/api/v1/accounts/connections/{connectionId}` | Authenticated |
| GET | `/api/v1/accounts/{accountId}/transactions` | Authenticated |

### Categories and transactions

| Method | Endpoint | Auth |
|---|---|---|
| GET | `/api/v1/categories` | Authenticated |
| GET | `/api/v1/transactions/{transactionId}` | Authenticated |
| PUT | `/api/v1/transactions/{transactionId}/classification` | Authenticated |

### Subscriptions

| Method | Endpoint | Auth |
|---|---|---|
| GET | `/api/v1/subscriptions` | Authenticated |
| GET | `/api/v1/subscriptions/{subscriptionId}` | Authenticated |
| GET | `/api/v1/subscriptions/{subscriptionId}/price-history` | Authenticated |
| POST | `/api/v1/subscriptions/{subscriptionId}/dismiss` | Authenticated |

### Anomalies

| Method | Endpoint | Auth |
|---|---|---|
| GET | `/api/v1/anomalies` | Authenticated |
| GET | `/api/v1/anomalies/{anomalyId}` | Authenticated |
| POST | `/api/v1/anomalies/{anomalyId}/resolve` | Authenticated |
| POST | `/api/v1/anomalies/{anomalyId}/dismiss` | Authenticated |

### Insights

| Method | Endpoint | Auth |
|---|---|---|
| GET | `/api/v1/insights` | Authenticated |
| GET | `/api/v1/insights/{insightId}` | Authenticated |
| POST | `/api/v1/insights/{insightId}/seen` | Authenticated |
| POST | `/api/v1/insights/{insightId}/dismiss` | Authenticated |

### Notifications

| Method | Endpoint | Auth |
|---|---|---|
| GET | `/api/v1/notifications` | Authenticated |
| POST | `/api/v1/notifications/{notificationId}/read` | Authenticated |
| GET | `/api/v1/notifications/preferences` | Authenticated |
| PUT | `/api/v1/notifications/preferences` | Authenticated |

### System / health

| Method | Endpoint | Auth |
|---|---|---|
| GET | `/api/v1/system/info` | Public |
| GET | `/health/live` | Public |
| GET | `/health/ready` | Public |

The development API also exposes OpenAPI metadata through the development-only OpenAPI mapping in `FinSight.Api`.

---

## 16. Important API Request Shapes

### Register

```json
{
  "email": "user@example.com",
  "password": "StrongPasswordHere",
  "displayName": "Example User"
}
```

### Login

```json
{
  "email": "user@example.com",
  "password": "StrongPasswordHere"
}
```

### Refresh

```json
{
  "refreshToken": "..."
}
```

### Connect institution

```json
{
  "institutionCode": "mock-bank"
}
```

### Correct transaction classification

```json
{
  "categoryId": "00000000-0000-0000-0000-000000000000",
  "subcategoryId": null
}
```

### Update notification preferences

```json
{
  "emailEnabled": true,
  "anomalyNotificationsEnabled": true,
  "subscriptionNotificationsEnabled": true,
  "insightNotificationsEnabled": true
}
```

---

## 17. HTTP Middleware Pipeline

The API currently applies the following middleware/security layers:

```text
Correlation ID
    -> Security headers
    -> Exception handler
    -> Status code pages
    -> HSTS (non-development)
    -> CORS
    -> Routing
    -> Authentication
    -> Authorization
    -> Rate limiting
    -> Serilog request logging
    -> Endpoints
```

The Kestrel request body limit is set to 10 MB.

---

## 18. Database and Persistence

PostgreSQL is the primary relational store, accessed through EF Core and Npgsql.

`FinSightDbContext` combines ASP.NET Core Identity persistence with the FinSight domain tables.

The current migration history is:

1. `20260831112859_InitialCreate`
2. `20260901095449_IdentityAndRefreshTokens`
3. `20260901140137_FinancialAccountsAndTransactions`
4. `20260902122606_TransactionIntelligence`
5. `20260903102618_SubscriptionIntelligence`
6. `20260904095953_AnomalyDetectionAndFinancialInsights`
7. `20260905165039_ProductionHardeningNotificationsOutboxAudit`

The repository also contains an exported migration SQL artifact at:

```text
artifacts/finsight-migrations.sql
```

### Persistence responsibilities

Repositories in `FinSight.Infrastructure/Persistence/Repositories` isolate persistence concerns behind Application interfaces.

Current repositories include support for:

- institutions;
- account connections;
- financial accounts;
- transactions;
- categories;
- merchants;
- subscriptions;
- anomalies;
- insights;
- notifications;
- notification preferences;
- outbox messages;
- processed messages.

---

## 19. Redis

Redis is used as the classification cache and is configured through the `Redis` settings section.

Local default:

```text
localhost:6379
```

The default key prefix is:

```text
FinSight:
```

The primary implemented cache use case is transaction classification reuse to reduce repeated AI calls and latency.

---

## 20. Mock Banking

The project deliberately uses mock banking infrastructure for portfolio demonstration purposes.

The abstraction boundary is:

- `IBankProvider`
- `IBankTransactionProvider`

The current infrastructure implementation is under:

```text
src/FinSight.Infrastructure/Banking/MockBank
```

This makes the financial integration replaceable without coupling the Application layer to a specific external banking provider.

### Sync lifecycle

```text
User requests sync
      |
      v
AccountSyncService
      |
      v
Mock bank provider
      |
      v
Idempotent transaction persistence
      |
      v
transaction.imported events
      |
      v
Async intelligence pipeline
```

The worker layer also contains `BankSyncWorker` for scheduled synchronization.

---

## 21. Background Workers

`FinSight.Workers` registers these hosted services:

- `BankSyncWorker`
- `TransactionImportedConsumer`
- `TransactionCategorizedConsumer`
- `SubscriptionLifecycleWorker`
- `TransactionCategorizedAnomalyConsumer`
- `AnomalyDetectedConsumer`
- `SubscriptionPriceChangedAnomalyConsumer`
- `AnomalyLifecycleWorker`
- `AuditRetentionWorker`
- `NotificationCreatedConsumer`
- `OutboxRetentionWorker`

The design intentionally keeps user-facing HTTP requests separate from expensive or failure-prone background work.

---

## 22. Observability

FinSight uses structured logging and OpenTelemetry.

### Logging

Serilog is configured for console logging with contextual enrichment.

### Telemetry

The current infrastructure includes telemetry support for:

- ASP.NET Core requests;
- outbound HTTP activity;
- runtime metrics;
- application-specific financial processing counters;
- AI classification request/failure/duration telemetry;
- notification delivery counters;
- anomaly/transaction processing metrics.

### Local observability stack

Docker Compose provisions:

| Component | Default port | Purpose |
|---|---:|---|
| OpenTelemetry Collector | 4317 / 4318 | OTLP ingestion |
| Jaeger | 16686 | Distributed tracing UI |
| Prometheus | 9090 | Metrics collection/query |
| Grafana | 3000 | Metrics dashboards |
| Mailpit | 8025 | Local email UI |

Prometheus and the OpenTelemetry Collector configuration live under `ops/`.

---

## 23. Health Checks

The API exposes two distinct health concepts:

### Liveness

```text
GET /health/live
```

This endpoint answers whether the API process itself is alive and intentionally excludes dependency checks.

### Readiness

```text
GET /health/ready
```

This endpoint executes health checks tagged as `ready`, covering the dependencies registered by the infrastructure layer, including PostgreSQL, Redis, RabbitMQ and worker-related readiness checks.

---

## 24. Local Development

### Prerequisites

Install:

- .NET 10 SDK;
- Docker Engine;
- Docker Compose;
- Node.js compatible with the frontend toolchain;
- pnpm 11 for the web project.

### Start infrastructure

From the repository root:

```bash
make infra
```

Or start the complete local stack:

```bash
make up
```

### Restore and build

```bash
dotnet restore
dotnet build FinSight.slnx
```

### Apply migrations

```bash
make migrate
```

Or:

```bash
dotnet ef database update \
  --project src/FinSight.Infrastructure \
  --startup-project src/FinSight.Api
```

### Run the API

```bash
make api
```

By default, the development launch configuration uses:

```text
http://localhost:5059
```

### Run workers

```bash
make worker
```

### Run the backend test suite

```bash
make test
```

### Run formatting/security checks

```bash
make format
make security
make check
```

---

## 25. Local Infrastructure

`docker-compose.yml` defines the following local services:

- PostgreSQL 17 Alpine
- Redis 8 Alpine
- RabbitMQ 4 Management Alpine
- OpenTelemetry Collector
- Jaeger
- Prometheus
- Grafana
- Mailpit

Default local ports:

| Service | Port |
|---|---:|
| FinSight API | 5059 |
| PostgreSQL | 5432 |
| Redis | 6379 |
| RabbitMQ | 5672 |
| RabbitMQ Management UI | 15672 |
| OTLP gRPC | 4317 |
| OTLP HTTP | 4318 |
| Jaeger UI | 16686 |
| Prometheus | 9090 |
| Grafana | 3000 |
| Mailpit SMTP | 1025 |
| Mailpit UI | 8025 |
| Next.js frontend | 3000 |

The Grafana/Next.js port overlap is a reminder that these are independently runnable components; adjust the local port mapping when running both simultaneously.

---

## 26. Configuration

Configuration is environment-specific and should never contain production secrets in source control.

Key sections include:

```text
Database
Redis
RabbitMq
Jwt
OpenAI
Smtp
Telemetry
Cors
```

The repository provides `.env.example` as a high-level template for environment variables.

### JWT defaults in development

Development configuration currently uses:

- access token lifetime: 15 minutes;
- refresh token lifetime: 30 days.

The signing key shown in `appsettings.Development.json` is explicitly a development placeholder and must be replaced in any real environment.

### AI configuration

The current development configuration targets:

```text
Provider: OpenAI
Model: gpt-5-mini
MaxOutputTokens: 500
```

AI processing safeguards also define a default daily request limit of 500 requests per user and a classification timeout of 20 seconds.

### SMTP configuration

Local development is configured for Mailpit on port 1025 with TLS disabled.

---

## 27. Frontend

The frontend lives under:

```text
frontend/finsight-web
```

It is a Next.js application using:

- Next.js 16.3.3;
- React 19.2.8;
- TypeScript;
- Tailwind CSS 4;
- shadcn-style UI primitives;
- TanStack Query;
- React Hook Form;
- Zod;
- Recharts;
- Playwright/Vitest/testing-library dependencies.

At the current repository snapshot, the frontend is still the initial application shell and **is not yet integrated with the FinSight API**. The backend therefore remains the primary implemented product surface at this stage.

To run it locally:

```bash
cd frontend/finsight-web
pnpm install
pnpm dev
```

---

## 28. Testing Strategy

FinSight uses multiple testing levels rather than relying only on controller tests.

### Domain tests

Validate entity behavior and state transitions such as transaction classification, subscription lifecycle, anomalies, insights and notifications.

### Application tests

Validate orchestration and business use cases such as subscription detection.

### Infrastructure tests

Cover components such as:

- rule-based transaction classification;
- anomaly detection;
- insight generation;
- transactional outbox persistence;
- telemetry instrumentation.

### Integration tests

Use Testcontainers packages for PostgreSQL and RabbitMQ-backed scenarios.

### Architecture tests

Protect the layered dependency model so that infrastructure details do not leak inward into the Domain/Application layers.

---

## 29. CI/CD

The repository contains GitHub Actions workflows for:

- CI;
- dependency review;
- security checks;
- staging deployment;
- production deployment.

The branch flow is:

```text
feature/* / fix/*
          |
          v
         dev
          |
          v
       staging
          |
          v
         main
```

### Branch responsibilities

- `dev` — active development integration branch;
- `staging` — pre-production integration/validation branch;
- `main` — production branch.

Production changes are expected to originate from `staging` through pull-request promotion.

### CI stages

1. Restore
2. Build
3. Test
4. Formatting
5. Dependency security
6. Dependency review on pull requests

Secrets are stored through GitHub Actions secret/environment mechanisms rather than source-controlled configuration.

Detailed operational documentation is under `docs/ci-cd/`.

---

## 30. Database and Event Evolution

The implementation has been built incrementally through explicit phases:

| Phase | Theme | Repository evidence |
|---|---|---|
| 1 | CI/CD & engineering automation | GitHub Actions, Makefile, scripts |
| 2 | Backend foundation & architecture | Layered solution, shared build configuration |
| 3 | Identity, users & security | Identity, JWT, refresh tokens, ownership, audit/security abstractions |
| 4 | Financial accounts & mock banking | Institutions, connections, accounts, sync, transactions |
| 5 | Transaction processing & AI categorization | normalization, rules, Redis, OpenAI, categorization events |
| 6 | Subscription intelligence | recurring detection, price history, subscription events |
| 7 | Anomaly detection & financial insights | anomaly engine, insight generator, related events/APIs |
| 8 | Notifications, observability & production hardening | notifications, outbox, audit retention, telemetry, consumers |

The current `dev` branch additionally contains the small follow-up import-order fix represented by commit `49ddc7b`.

---

## 31. Repository Structure

```text
FinSight/
├── .github/
│   └── workflows/
├── artifacts/
├── docs/
│   ├── architecture.md
│   ├── api.md
│   ├── domain.md
│   ├── development.md
│   ├── testing.md
│   ├── operations.md
│   └── ci-cd/
├── frontend/
│   └── finsight-web/
├── ops/
│   ├── otel-collector/
│   └── prometheus/
├── scripts/
├── src/
│   ├── FinSight.Api/
│   ├── FinSight.Application/
│   ├── FinSight.Contracts/
│   ├── FinSight.Domain/
│   ├── FinSight.Infrastructure/
│   └── FinSight.Workers/
├── tests/
│   ├── FinSight.Application.Tests/
│   ├── FinSight.Architecture.Tests/
│   ├── FinSight.Domain.Tests/
│   ├── FinSight.Infrastructure.Tests/
│   └── FinSight.IntegrationTests/
├── Directory.Build.props
├── Directory.Packages.props
├── docker-compose.yml
├── FinSight.slnx
├── global.json
├── Makefile
└── README.md
```

---

## 32. Engineering Conventions

The current codebase emphasizes:

- nullable reference types;
- implicit usings;
- XML documentation on public APIs;
- centralized NuGet package versions;
- dependency inversion through Application abstractions;
- repositories behind interfaces;
- cancellation-token propagation;
- structured logging;
- explicit DTOs for API/application boundaries;
- explicit domain status/lifecycle enums;
- idempotency and ownership isolation;
- infrastructure concerns kept out of Domain/Application where possible.

`Directory.Build.props` and `Directory.Packages.props` provide repository-wide build and package-management conventions.

---

## 33. Security and Operational Rules

Production environments must at minimum follow these repository conventions:

1. Never commit credentials or signing keys.
2. Never use the development JWT signing key in production.
3. Use environment-specific secrets.
4. Keep database, RabbitMQ, Redis, AI and email credentials outside source control.
5. Do not log sensitive credentials or tokens.
6. Keep authentication endpoints rate-limited.
7. Preserve ownership checks when adding repositories/services/controllers.
8. Prefer forward-compatible database migrations for production rollouts.
9. Avoid blind production database rollback.
10. Promote application changes through the documented branch flow.

---

## 34. Known Current Limitations

These are important to distinguish from implemented backend capabilities:

- Banking integration is still mock-provider based.
- The web frontend is still a scaffold and is not connected to the backend.
- The AI categorization provider is OpenAI-specific at the current infrastructure implementation level, although the Application layer depends on an abstraction.
- The current anomaly detector is deterministic/statistical rather than a learned anomaly model.
- Notification delivery currently provides in-app/email-oriented infrastructure rather than a broad multi-channel communications platform.
- The repository snapshot includes generated `obj` outputs from the development environment; these are build artifacts, not part of the intended source architecture.

---

## 35. Next Engineering Direction

A natural next step is to move from backend completeness to **product-surface integration**:

```text
Backend domain/API
      |
      v
Typed frontend API client
      |
      v
Authentication/session management
      |
      v
Dashboard
      |
      +---- Accounts
      +---- Transactions
      +---- Subscriptions
      +---- Anomalies
      +---- Insights
      +---- Notifications
      +---- Profile/security
```

After frontend integration, the project can evolve toward more sophisticated anomaly models, real banking adapters, richer notification channels, stronger end-to-end tests, dashboards, and production deployment infrastructure.

---

## 36. License

The repository includes a `LICENSE` file. See that file for the authoritative license terms.
