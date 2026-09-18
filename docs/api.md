# FinSight API Reference

Base path:

```text
/api/v1
```

All authenticated endpoints require a valid bearer access token unless explicitly stated otherwise.

## Authentication

| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/auth/register` | Create a user and issue tokens |
| POST | `/auth/login` | Authenticate a user |
| POST | `/auth/refresh` | Rotate refresh token and issue a new access token |
| POST | `/auth/logout` | Revoke a supplied refresh token |
| POST | `/auth/change-password` | Change the authenticated user's password |
| GET | `/auth/sessions` | List active authentication sessions |
| POST | `/auth/sessions/revoke-others` | Revoke every active session except the current one |
| POST | `/auth/forgot-password` | Request a password reset |
| POST | `/auth/reset-password` | Reset password using a valid token |

### Auth response

```json
{
  "accessToken": "...",
  "refreshToken": "...",
  "accessTokenExpiresAt": "2026-09-06T18:30:00Z",
  "userId": "00000000-0000-0000-0000-000000000000",
  "sessionId": "11111111-1111-1111-1111-111111111111"
}
```

## Users

`GET /users/me`

Returns the authenticated user's profile.

`PATCH /users/me`

Request:

```json
{
  "displayName": "New Display Name"
}
```


## Security activity

`GET /security/activity?limit=25`

Returns recent user-owned security activity. The response intentionally excludes raw IP addresses, trace IDs, correlation IDs and other audit metadata.

### Active sessions

`GET /auth/sessions`

Returns active refresh-token sessions for the authenticated user. Each response includes creation/expiration timestamps, the originating IP address, and whether the session corresponds to the current access-token session identifier.

`POST /auth/sessions/revoke-others`

Revokes all active refresh-token sessions except the current session and returns the number revoked. Refresh-token revocation prevents those sessions from establishing a new access token; already-issued access tokens naturally remain valid until their configured expiration.

Password changes revoke all active refresh-token sessions and the client immediately clears its local session before requiring a new sign-in.

## Dashboard

`GET /dashboard`

Returns the aggregated financial read model used by the authenticated FinSight overview. Monetary summaries and trends are calculated in the user's primary currency; account balances and individual transactions retain their own currency.

The response includes:

- currency-grouped account balances
- six-month income/spending trend
- current-period category spending
- recent transactions
- upcoming active subscriptions
- prioritized anomalies and insights
- dashboard headline metrics

## Institutions

`GET /institutions`

Returns active supported financial institutions.

## Accounts

`GET /accounts`

Returns the authenticated user's financial accounts, including the owning connection identifier.

`GET /accounts/connections`

Returns the authenticated user's active institution connections with connection status, last synchronization timestamps, synchronization errors and account counts.

`GET /accounts/{accountId}`

Returns one owned account. The response includes the owning connection identifier so clients can coordinate account-level synchronization controls without exposing another ownership query.

`POST /accounts/connections`

Request:

```json
{
  "institutionCode": "MOCK_BANK"
}
```

`POST /accounts/connections/{connectionId}/sync`

Returns:

```json
{
  "importedTransactions": 12
}
```

`DELETE /accounts/connections/{connectionId}`

Disconnects the institution connection.

`GET /accounts/{accountId}/transactions?limit=100`

Returns transactions for the owned account.

## Categories

`GET /categories`

Returns the active category taxonomy with nested subcategories.

## Transactions

`GET /transactions`

Returns a filtered, sorted and paginated transaction read model with account, institution, merchant, category and classification context.

`GET /transactions/{transactionId}`

Returns the transaction including raw/normalized descriptions, contextual account/institution/merchant/category information, classification source/confidence and lifecycle state.

`PUT /transactions/{transactionId}/classification`

Request:

```json
{
  "categoryId": "00000000-0000-0000-0000-000000000000",
  "subcategoryId": null
}
```

## Subscriptions

`GET /subscriptions`

Returns detected subscriptions.

`GET /subscriptions/{subscriptionId}`

Returns one subscription.

`GET /subscriptions/{subscriptionId}/price-history?limit=24`

Returns observed subscription charges.

`POST /subscriptions/{subscriptionId}/dismiss`

Dismisses a subscription finding.

## Anomalies

`GET /anomalies?includeResolved=false&limit=100`

Returns anomaly signals using string enum names for type, severity and lifecycle status.

`GET /anomalies/{anomalyId}`

`POST /anomalies/{anomalyId}/resolve`

`POST /anomalies/{anomalyId}/dismiss`

## Insights

`GET /insights?includeDismissed=false&limit=100`

Returns financial insights using string enum names for type, severity and lifecycle status.

`GET /insights/{insightId}`

`POST /insights/{insightId}/seen`

`POST /insights/{insightId}/dismiss`

## Notifications

`GET /notifications?includeRead=false&limit=100`

Returns user-owned **in-app** notifications. Email delivery records are intentionally excluded from the in-app notification center.

`GET /notifications/unread-count`

Returns the unread in-app notification count used by the application shell.

`POST /notifications/read-all`

Marks all delivered, failed, and dead-lettered in-app notifications as read. Pending notifications are left pending so background delivery can continue.

`POST /notifications/{notificationId}/read`

`POST /notifications/{notificationId}/dismiss`

Dismisses a notification from the active attention queue while retaining its history.

`GET /notifications/preferences`

`PUT /notifications/preferences`

The preference request contains `inAppEnabled`, `emailEnabled`, `anomalyNotificationsEnabled`, `subscriptionNotificationsEnabled`, and `insightNotificationsEnabled`. Security notifications remain enabled in-app.

Request:

```json
{
  "inAppEnabled": true,
  "emailEnabled": true,
  "anomalyNotificationsEnabled": true,
  "subscriptionNotificationsEnabled": true,
  "insightNotificationsEnabled": true
}
```

## System and health

`GET /system/info`

Returns service name, version and environment.

`GET /health/live`

Process-level liveness endpoint.

`GET /health/ready`

Dependency readiness endpoint.

## Error/security behavior

Password-reset requests intentionally return the same accepted response regardless of whether the email exists, preventing account enumeration.

Authenticated data access is scoped to the current user through repository/service ownership checks.

## Transaction query API

`GET /api/v1/transactions` provides a user-owned, projection-based transaction read model. It supports bounded pagination (`page`, `pageSize`), free-text search (`search`), account/category filters, transaction and classification status filters, inclusive `from`/exclusive `to` dates, minimum and maximum absolute amount filters, and a fixed set of safe sort fields (`date`, `amount`, `merchant`) with explicit descending order control.

The response contains `items`, `page`, `pageSize`, `totalCount`, `totalPages`, `hasNextPage`, and `hasPreviousPage`. Each list item contains contextual account/institution, merchant, category, amount, transaction lifecycle, and classification information.

`GET /api/v1/transactions/{transactionId}` returns the contextual transaction detail read model. `PUT /api/v1/transactions/{transactionId}/classification` remains the authoritative user-correction command and requires an active category plus an optional subcategory belonging to that category.
