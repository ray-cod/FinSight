# Five-Minute Demo Script

## 0:00 — Position the product

“FinSight turns raw financial transactions into understandable signals. The demo uses a deterministic mock bank so the same product behavior can be reproduced without real banking data.”

## 0:30 — Dashboard

Open `/dashboard`.

Point out the financial summary, cash flow, category mix, recent transactions, and open intelligence signals.

Engineering point: the dashboard uses a dedicated backend read model rather than making the browser aggregate the financial domain.

## 1:30 — Transactions

Open `/transactions`, search `Amazon`, and open the large charge.

Point out the classification source and confidence.

Engineering point: transaction search/filtering/pagination is server-side and the browser only handles presentation state.

## 2:15 — Subscriptions

Open `/subscriptions` and select Netflix.

Show the recurring cadence and price history from `$15.99` to `$17.99`.

Engineering point: subscription detection is based on historical cadence and amount consistency rather than hard-coded merchant lists in the UI.

## 3:00 — Anomalies

Open `/anomalies` and select the large-transaction signal.

Show the detection score, confidence, evidence, and source transaction.

Engineering point: the browser presents an explainable result produced by backend statistical rules.

## 3:45 — Insights

Open the linked `/insights` item.

Follow its source back to the anomaly/transaction.

Engineering point: the system separates detection from explanation so downstream consumers can evolve independently.

## 4:20 — Notifications

Open `/notifications` and show the generated alert.

Engineering point: transaction import → asynchronous intelligence → insight → notification is the end-to-end product story.

## 4:45 — Architecture close

Show the architecture diagram and mention PostgreSQL, Redis, RabbitMQ, transactional outbox, workers, OpenAI fallback, observability, security, and CI/CD.

Close with the engineering principle: the frontend is intentionally thin on business rules; authoritative financial logic remains in the backend.
