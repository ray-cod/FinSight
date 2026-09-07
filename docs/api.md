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
| POST | `/auth/forgot-password` | Request a password reset |
| POST | `/auth/reset-password` | Reset password using a valid token |

### Auth response

```json
{
  "accessToken": "...",
  "refreshToken": "...",
  "accessTokenExpiresAt": "2026-09-06T18:30:00Z",
  "userId": "00000000-0000-0000-0000-000000000000"
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

## Institutions

`GET /institutions`

Returns active supported financial institutions.

## Accounts

`GET /accounts`

Returns the authenticated user's financial accounts.

`GET /accounts/{accountId}`

Returns one owned account.

`POST /accounts/connections`

Request:

```json
{
  "institutionCode": "mock-bank"
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

`GET /transactions/{transactionId}`

Returns the transaction including raw/normalized descriptions, merchant/category references, classification source/confidence and lifecycle state.

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

`GET /anomalies/{anomalyId}`

`POST /anomalies/{anomalyId}/resolve`

`POST /anomalies/{anomalyId}/dismiss`

## Insights

`GET /insights?includeDismissed=false&limit=100`

`GET /insights/{insightId}`

`POST /insights/{insightId}/seen`

`POST /insights/{insightId}/dismiss`

## Notifications

`GET /notifications?includeRead=false&limit=100`

`POST /notifications/{notificationId}/read`

`GET /notifications/preferences`

`PUT /notifications/preferences`

Request:

```json
{
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
