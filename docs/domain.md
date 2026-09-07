# FinSight Domain Model

## Domain map

```text
User
 |
 +--> AccountConnection --> Institution
 |          |
 |          +--> FinancialAccount
 |                    |
 |                    +--> Transaction
 |                              |
 |                              +--> Merchant
 |                              +--> Category / Subcategory
 |                              |
 |                              +--> Anomaly
 |                              |      |
 |                              |      +--> FinancialInsight
 |                              |
 |                              +--> Subscription
 |                                     |
 |                                     +--> SubscriptionPriceHistory
 |
 +--> NotificationPreference
 |
 +--> Notification
 |
 +--> AuditEvent
 |
 +--> RefreshToken

Infrastructure support:
OutboxMessage
ProcessedMessage
```

## Transaction lifecycle

```text
Pending
   |
   v
Classification in progress
   |
   +--> Rule
   +--> Cache
   +--> AI
   |
   v
Classified
   |
   +--> UserCorrected
   |
   +--> ClassificationFailed
```

User-corrected transactions are intentionally protected from automatic reclassification.

## Subscription lifecycle

A subscription is created from recurring transaction evidence rather than from manual entry. It records the inferred billing cadence, confidence, average/current amount, observed history, and expected next charge.

Material price changes are represented explicitly and can trigger downstream intelligence.

## Anomaly lifecycle

Anomalies have an explicit status and can be resolved or dismissed through the API.

## Insight lifecycle

```text
Active -> Seen
Active -> Dismissed
Active -> Expired
Seen   -> Dismissed
```

The domain implementation prevents an already-dismissed insight from being overwritten by expiry.

## Notification lifecycle

```text
Pending
  |
  +--> Delivered --> Read
  |
  +--> Failed --> retry --> Delivered
                   |
                   +--> DeadLettered
```

## Domain principles

- Entities protect their state through methods rather than public setters.
- Domain state changes are represented by explicit status enums.
- Probability/confidence values are constrained to valid ranges.
- Empty identifiers are rejected where they would violate invariants.
- User ownership is carried through the domain/application boundaries.
