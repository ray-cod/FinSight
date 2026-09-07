# FinSight Web

The FinSight web client is the frontend application for the FinSight financial intelligence platform.

## Current status

The frontend in the current repository snapshot is still the initial Next.js application shell. It is **not yet connected to the FinSight API**.

The backend/API remains the primary implemented product surface today.

## Stack

- Next.js 16.3.3
- React 19.2.8
- TypeScript
- Tailwind CSS 4
- shadcn-style UI components
- TanStack Query
- React Hook Form
- Zod
- Recharts
- Vitest / Testing Library
- Playwright
- pnpm 11.24.0

## Run locally

```bash
pnpm install
pnpm dev
```

Open:

```text
http://localhost:3000
```

## Build

```bash
pnpm build
```

## Lint

```bash
pnpm lint
```

## Planned integration direction

The intended product flow is:

```text
Authentication
   |
   v
Dashboard
   |
   +--> Accounts
   +--> Transactions
   +--> Subscriptions
   +--> Anomalies
   +--> Insights
   +--> Notifications
   +--> Profile / Security
```

The frontend should consume the versioned backend API under `/api/v1` through a typed client and use secure session/token handling appropriate to the eventual deployment model.
# FinSight Web

The FinSight web client is the frontend application for the FinSight financial intelligence platform.

## Current status

The frontend in the current repository snapshot is still the initial Next.js application shell. It is **not yet connected to the FinSight API**.

The backend/API remains the primary implemented product surface today.

## Stack

- Next.js 16.3.3
- React 19.2.8
- TypeScript
- Tailwind CSS 4
- shadcn-style UI components
- TanStack Query
- React Hook Form
- Zod
- Recharts
- Vitest / Testing Library
- Playwright
- pnpm 11.24.0

## Run locally

```bash
pnpm install
pnpm dev
```

Open:

```text
http://localhost:3000
```

## Build

```bash
pnpm build
```

## Lint

```bash
pnpm lint
```

## Planned integration direction

The intended product flow is:

```text
Authentication
   |
   v
Dashboard
   |
   +--> Accounts
   +--> Transactions
   +--> Subscriptions
   +--> Anomalies
   +--> Insights
   +--> Notifications
   +--> Profile / Security
```

The frontend should consume the versioned backend API under `/api/v1` through a typed client and use secure session/token handling appropriate to the eventual deployment model.
