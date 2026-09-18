# Dashboard frontend architecture

The dashboard consumes the authenticated `GET /api/v1/dashboard` read model. Aggregation remains on the backend so the browser does not issue a fan-out of account, transaction, subscription, anomaly, insight, category, and notification requests for every overview render.

The dashboard treats currency as an explicit domain boundary: monetary summary and trend calculations use the API-selected primary currency, while individual recent transactions and account balances retain their own currency.
