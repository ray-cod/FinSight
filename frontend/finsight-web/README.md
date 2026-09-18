# FinSight Web

The FinSight web client is the Next.js frontend for the FinSight financial intelligence platform.

Phases 1–10 establish the production frontend foundation and the currently implemented financial product workflows, including authentication, the dashboard, accounts, transactions, subscriptions, anomalies, financial insights, notifications, profile management and security controls.

## Stack

- Next.js 16.3.3
- React 19.2.8
- TypeScript (strict)
- Tailwind CSS 4
- shadcn-style UI primitives
- TanStack Query
- React Hook Form
- Zod
- Recharts
- Vitest / Testing Library
- Playwright
- pnpm 11.24.0

## Frontend architecture

```text
Next.js App Router
        |
        +--> Application Shell
        |      +--> Sidebar navigation
        |      +--> Header
        |      +--> Responsive mobile navigation
        |      +--> Shared page states
        |
        +--> Providers
        |      +--> TanStack Query
        |      +--> Authentication/session provider
        |
        +--> API layer
        |      +--> Typed HTTP client
        |      +--> /api/v1 versioning
        |      +--> Correlation IDs
        |      +--> Problem-details normalization
        |      +--> Access-token injection
        |      +--> One-time refresh + request retry
        |
        +--> Domain types
        |      +--> Identity
        |      +--> Accounts
        |      +--> Transactions
        |      +--> Subscriptions
        |      +--> Anomalies
        |      +--> Insights
        |      +--> Notifications
        |
        +--> Product routes
               +--> Dashboard
               +--> Accounts
               +--> Transactions
               +--> Subscriptions
               +--> Anomalies
               +--> Insights
               +--> Notifications
               +--> Settings

## Authentication

Phase 2 connects the frontend to the existing FinSight Identity API. The browser session uses the backend's access/refresh-token contract, persists the token pair in `sessionStorage`, refreshes an expired access token once after an HTTP 401, and clears the client session when refresh fails. The token handling is isolated behind `lib/auth/token-store.ts` so it can be replaced by a secure HttpOnly-cookie transport when the backend exposes that contract.

Authentication routes are:

- `/login`
- `/register`
- `/forgot-password`
- `/reset-password`

Protected workspace routes are guarded by the shared `AuthGuard`; public authentication routes are guarded by `PublicOnlyGuard`. The login flow supports a safe `returnUrl` so users return to the originally requested workspace location.


## API integration

Set `NEXT_PUBLIC_API_BASE_URL` to the browser-reachable FinSight API origin. The shared client automatically prefixes relative resource paths with `/api/v1`.

The client also sends an `X-Correlation-ID` header for every request and normalizes ASP.NET Core problem-details responses into `ApiError`. Authentication and token rotation are implemented in the shared auth provider and API client. Authenticated requests inject the current access token, and a single 401 can trigger a coordinated refresh before the original request is retried once.

## Run locally

```bash
pnpm install
pnpm dev
```

Open `http://localhost:3000`.

## Quality checks

```bash
pnpm lint
pnpm typecheck
pnpm test
pnpm build
pnpm test:e2e
```

## Current phase boundary

Implemented in Phase 1:

- production application shell
- responsive navigation
- shared visual tokens and UI foundations
- typed API client
- API error normalization
- environment validation
- TanStack Query provider and cache conventions
- domain-facing API types
- reusable loading and error states
- Vitest and Playwright setup
- real top-level product routes for the upcoming feature phases

Implemented in Phase 2:

- login, registration, forgot-password and reset-password routes
- browser-session token storage abstraction
- authenticated API token injection
- coordinated 401 refresh and retry behavior
- protected-route and public-only guards
- current-user bootstrap and session restoration
- account menu with authenticated user identity and sign-out
- authentication validation and E2E coverage

Implemented in Phase 3:

- aggregated `/api/v1/dashboard` read model
- six-month income and spending trend
- primary-currency financial summaries with multi-currency balance separation
- current-month category spending breakdown
- recent transaction activity
- upcoming recurring payments
- prioritized anomaly and insight signals
- dashboard KPI cards and workspace pulse
- responsive financial dashboard with loading, empty and error states
- dashboard service/query and unit-test foundation

Implemented in Phase 4:

- financial accounts workspace
- supported-institution discovery
- account connection and immediate initial synchronization workflow
- active connection status and synchronization health
- manual synchronization controls
- connection disconnect workflow with confirmation
- multi-currency account balance summaries
- account detail route with recent account activity
- account API service, query/mutation hooks and unit-test coverage
- backend connection read model for frontend ownership-safe connection management

The planned frontend product phases through portfolio demonstration readiness are implemented in the current repository snapshot. The next work should be driven by real deployment feedback, additional business requirements, or deeper automated coverage rather than adding placeholder product surfaces.

## Phase 5 — Transactions Experience

The transaction workspace is now implemented around the existing FinSight transaction-processing model. `/transactions` uses a dedicated server-side query endpoint with bounded pagination, text search, account/category/type/status/classification/date/amount filters, stable sort fields, and URL-persisted state. `/transactions/[transactionId]` exposes transaction provenance, classification context, and user correction controls. Classification edits invalidate the transaction, transaction list, and dashboard query caches so downstream views remain consistent.


## Phase 6 — Subscription Intelligence

The subscription workspace is implemented around the existing FinSight subscription-detection model. `/subscriptions` loads the authenticated user's detected recurring commitments, supports URL-persisted merchant/status/frequency/currency filters, keeps recurring totals separated by currency, presents cadence confidence and upcoming-charge timing, and links to detailed subscription analysis. `/subscriptions/[subscriptionId]` adds observed price history, comparable price-change analysis, recurring commitment estimates, transaction links for observed charges, and a confirmed dismissal workflow. Price-history data is loaded only on the detail route to avoid an N+1 request pattern across subscription cards.

## Phase 7 — Anomalies & Financial Insights

Implemented the anomaly and financial insight workspaces, detail routes, URL-persisted filters, evidence/context views, and lifecycle actions against the existing API. The intelligence surfaces trace signals back to their source anomaly and transaction instead of duplicating business logic in the browser.


## Phase 8 — Notifications & User Preferences

The notification center is implemented against the authenticated notification API. The frontend supports all/recent/unread views, notification-type filtering, delivery-state presentation, single and bulk mark-as-read actions, and semantic navigation to the originating FinSight capability.

The settings route now contains a production notification-preferences panel for in-app/email channels plus anomaly, subscription, and insight categories. Preferences are saved as one versioned document so channel/category changes remain consistent. Security events remain enabled in-app.

The global application header now uses the server-side unread count endpoint rather than inferring unread state from a local notification list. This keeps the chrome accurate without requiring the full notification history to be loaded.


Phase 8 adds Playwright coverage for the protected notification route, unread review/bulk read behavior, and saving notification preferences.


## Phase 9 — Settings, Profile & Security Management

The settings experience is implemented as one consolidated route with deep-linkable `section` state for Profile, Security and Notifications. The Profile section edits the authenticated display name and presents the account email and verification state as read-only data. The Security section provides password changes, active-session review, confirmation-gated sign-out of other sessions, and recent user-scoped security activity.

The browser does not store or transmit refresh tokens through URLs. Access tokens now carry an opaque session identifier tied to the persisted refresh-token record, allowing the API to preserve the current session while revoking other active sessions. Password changes revoke all refresh-token sessions and the frontend clears the browser session before redirecting back to sign-in.


## Phase 10 — Cross-Application UX & Production Polish

The frontend now includes shared empty/loading/error treatments, authenticated route error boundaries, offline status feedback, global toast feedback, persisted system/light/dark appearance selection, client runtime diagnostics with optional remote reporting, reduced-motion/accessibility hardening, security response headers, and consistent route-level loading states.

The optional `NEXT_PUBLIC_CLIENT_ERROR_ENDPOINT` environment variable can point to a trusted client-error collector. It is intentionally disabled by default.

## Phase 11 — Frontend CI/CD & Release Integration

The frontend is integrated into the repository CI gate and release pipeline. CD consumes only successful CI runs and checks out the exact verified commit SHA.

CI executes linting, strict TypeScript typechecking, unit tests, a production Next.js build, and Playwright against the production server (`pnpm start`). The Playwright configuration retains the local development server by default and switches to the production server when `PLAYWRIGHT_USE_PRODUCTION_SERVER=true`.

For staging and production releases, the frontend is built from `Dockerfile` using the existing Next.js standalone output. The runtime image uses a non-root user and exposes `/api/health` for container and deployment smoke checks. Images are published to GHCR with an immutable Git SHA tag and an environment tag.

The deployment hook is intentionally provider-neutral. Configure the GitHub Environment variables and secrets documented in [`docs/ci-cd/frontend.md`](../../docs/ci-cd/frontend.md) to enable actual deployment. The manual `Deploy Web` workflow can deploy or roll back to a previously published immutable image tag.


## Portfolio demonstration

Use the parent repository’s [`docs/demo/README.md`](../../docs/demo/README.md) to populate the deterministic mock-bank scenario and follow the end-to-end product demo. [`docs/portfolio/README.md`](../../docs/portfolio/README.md) contains the technical storytelling guide.
