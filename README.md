# Smart Subscription & Financial Anomaly Engine

A personal finance intelligence platform that ingests transaction data, automatically categorizes spending, detects recurring charges and unusual patterns, and alerts users to potential money leaks before they become financial problems.

## Why this project exists

Most people do not fail at managing money because they do not care. They fail because spending is fragmented, subscriptions are easy to forget, and manual budgeting tools depend on constant effort. This project solves that problem by turning raw transaction data into clear, actionable financial insight.

## What the system does

* Imports transaction data from CSV uploads, mock bank APIs, or manual entry
* Automatically categorizes transactions using a rules + AI-assisted classification pipeline
* Detects likely subscriptions and recurring charges
* Identifies unusual spending patterns and suspicious charges
* Sends alerts when spending behavior changes
* Presents clear dashboards that both technical and non-technical users can understand

## Product goals

* Help users understand where their money goes
* Reduce forgotten subscriptions and recurring charges
* Surface unusual spending early
* Remove the need for manual transaction tagging
* Present financial data in a simple, trustworthy way

## Architecture overview

This repository is organized as a monorepo containing three main applications:

* **Frontend** — Next.js application for the user dashboard and workflows
* **Backend** — NestJS application that powers auth, ingestion, categorization, anomalies, notifications, and read models
* **AI Service** — Python FastAPI service that helps classify messy transaction descriptions

Supporting packages and infrastructure live alongside the apps to keep the system maintainable and production-oriented.

### Tech stack

* **Frontend:** Next.js, React, TypeScript, Tailwind CSS, shadcn/ui, TanStack Query, Recharts
* **Backend:** NestJS, TypeScript, Prisma, PostgreSQL, Redis, RabbitMQ
* **AI Service:** Python, FastAPI, sentence-transformers, scikit-learn
* **Infrastructure:** Docker, Docker Compose, AWS, S3, CloudWatch/Sentry

## Repository structure

```bash
smart-subscription-engine/
├── apps/
│   ├── backend/
│   ├── frontend/
│   └── ai-service/
├── infra/
│   ├── docker/
│   ├── kubernates/
│   ├── nginx/
│   └── terraform/
├── docs/
├── .github/
├── .env.example
├── docker-compose.yml
└── README.md
```

## Key features

### Transaction intelligence

Transaction data is normalized, categorized, and enriched so the user can immediately understand spending patterns without manual bookkeeping.

### Subscription detection

The system identifies likely recurring payments by comparing merchant names, amounts, and timing patterns.

### Anomaly detection

The system flags unusual spending behavior such as spikes, duplicate charges, and price increases on recurring items.

### Dashboard and alerts

Users get a clear dashboard, monthly insights, and notifications that explain what changed and why it matters.

## Data flow

1. A user uploads transactions or data is ingested from a mock banking source.
2. The backend stores the raw import and publishes an event.
3. Background consumers process categorization, recurring-payment detection, and anomaly detection.
4. The AI service classifies unclear transaction descriptions.
5. The dashboard reads from optimized summary data.
6. Alerts are generated when the system finds meaningful changes.

## Local development

### Prerequisites

* Node.js 20+
* npm
* Python 3.11+
* Docker and Docker Compose
* PostgreSQL and Redis running locally, or via Docker Compose

### Setup

```bash
npm install
```

Copy the environment file:

```bash
cp .env.example .env
```

Start the local infrastructure:

```bash
docker compose up -d
```

Run the applications in development mode:

```bash
npm dev
```

## Environment variables

Create a `.env` file at the root and configure the required values for each service.

Example values include:

* database connection strings
* JWT secrets
* Redis connection details
* RabbitMQ connection details
* AI service base URL
* storage configuration
* email/notification provider settings

See `.env.example` for the full list.

## Development philosophy

This project is built to show more than technical skill. It is designed to show:

* product thinking
* system design
* backend architecture
* event-driven processing
* practical AI integration
* performance awareness
* user-centered problem solving

## What is intentionally out of scope

To keep the project focused and finishable, this product does **not** include:

* live bank integrations with real providers
* payment initiation or money transfers
* investing features
* credit score monitoring
* debt management
* tax filing
* family or joint accounts
* subscription cancellation inside the app
* social sharing features
* crypto tracking

## Roadmap

The project is being built as a complete portfolio-grade product with clear service boundaries and production-minded engineering.

## Screenshots

Add dashboard and workflow screenshots here once the UI is implemented.

## API documentation

* Backend OpenAPI docs: `apps/backend`
* AI service docs: `apps/ai-service`
* System design notes: `docs/architecture`

## Contributing

This is a personal portfolio project, but the codebase is structured as if it could be maintained by a team.

## License

Add a license here if you plan to open-source the project publicly.

## Author

Built by Raimi Dikamona Lassissi.
