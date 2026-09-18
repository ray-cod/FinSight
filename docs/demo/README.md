# FinSight Portfolio Demo

FinSight’s demo is reproducible locally without real banking credentials. The mock bank fixture is deterministic relative to the current date and the setup helper drives the same public API and background-processing path used by the application.

## Start

Start infrastructure and the API/worker processes, then run:

```bash
make up
make demo
```

Open `http://localhost:3000/login` with the credentials printed by the setup script. Default local-only credentials are `demo@finsight.local` / `FinSightDemo!2026`. Do not reuse these credentials in shared or deployed environments.

## Five-minute story

1. **Dashboard** — establish the overall financial picture and point out the open intelligence signals.
2. **Transactions** — search for `Amazon`, open the materially larger charge, and show classification provenance.
3. **Subscriptions** — open Netflix and explain the recurring cadence and price history.
4. **Anomalies** — show the large-transaction, category-spike and duplicate evidence.
5. **Insights** — follow an explanation back to the anomaly and source transaction.
6. **Notifications** — show how the generated signal reaches the user-facing notification center.

## Fixture scenario

The deterministic mock data intentionally contains normal context plus meaningful signals:

- 8 monthly Netflix observations, with a price increase from `$15.99` to `$17.99`;
- 8 monthly Spotify observations;
- 6 recurring fitness-membership observations;
- 4 months of grocery history followed by a materially higher current-month charge;
- 6 normal Amazon purchases followed by a materially larger purchase;
- a potential duplicate coffee purchase;
- salary, transfers, rideshare, coffee, groceries, shopping and entertainment activity.

The deterministic classification rules cover the demo merchants, so the walkthrough does not require an OpenAI key. OpenAI remains an active production capability for uncategorized transactions.

## Reproducibility

The setup script is safe to rerun. Existing provider transactions are protected by the backend’s idempotency rules. The script waits for transactions, subscriptions, anomalies and insights to become available; slower environments may continue processing in the background after the script prints the credentials.
