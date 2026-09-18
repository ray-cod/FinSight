# FinSight Portfolio Pack

## Product positioning

FinSight is a financial intelligence platform. It turns raw transaction data into understandable signals: recurring commitments, price changes, unusual spending, explanations and notifications.

### Thirty-second pitch

FinSight is a .NET 10 modular monolith with event-driven background processing. It uses PostgreSQL/EF Core for durable financial state, Redis for classification caching, RabbitMQ for asynchronous workflows, a rules→cache→AI transaction categorization pipeline, deterministic anomaly detection, explainable insights, notification delivery, security/audit controls, OpenTelemetry-based observability, CI quality gates, and a production-grade Next.js application.

## Technical story

The strongest engineering discussions are:

- **Architecture:** why a modular monolith plus asynchronous workers is more appropriate here than prematurely splitting into microservices.
- **Reliability:** transactional outbox, idempotent transaction/message processing, retry and dead-letter handling.
- **AI:** deterministic rules and cache before model inference, with strict validation of AI output.
- **Financial intelligence:** recurring-payment cadence/amount analysis and explainable anomaly heuristics.
- **Security:** JWT access tokens, refresh-token rotation, session revocation, ownership checks, rate limiting and audit events.
- **Operations:** Serilog, OpenTelemetry, Prometheus, Grafana, Jaeger, container releases and CI/CD gates.
- **Frontend:** server-side query models, URL-persisted filters, resilient state handling, accessible workflows, and strict separation of presentation from financial business rules.

## Live demo

Use [`docs/demo/README.md`](../demo/README.md). The recommended path is Dashboard → Transactions → Subscriptions → Anomalies → Insights → Notifications.

## Visual assets

The source-controlled SVG previews under [`assets/`](assets/) are representative product visuals that keep the Git repository lightweight. Replace them with real 1440×900 captures from the hosted demo when a production environment is available.

For a spoken walkthrough, use [`DEMO_SCRIPT.md`](DEMO_SCRIPT.md).
