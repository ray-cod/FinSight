# FinSight Rollback Strategy

## Application Rollback

If a deployment fails:

1. Stop further promotion.
2. Identify the last known-good release.
3. Redeploy the previous release.
4. Verify health checks.
5. Verify critical application workflows.
6. Document the incident.

## Database Rollback

Database migrations are forward-only by default.

A migration should only be reversed when the rollback is known to be safe.

Prefer:

1. Forward-compatible migrations.
2. Backward-compatible application changes.
3. Separate destructive database changes from application deployment.

## Incident Principle

Never perform an emergency database rollback blindly in production.
