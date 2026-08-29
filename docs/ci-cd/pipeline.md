# FinSight CI/CD Pipeline

## Continuous Integration

CI runs on:

- Pull requests to `dev`
- Pull requests to `staging`
- Pull requests to `main`
- Pushes to `dev`
- Pushes to `staging`
- Pushes to `main`

## CI stages

1. Restore
2. Build
3. Test
4. Formatting
5. Dependency security

## Dependency Review

Pull requests are checked for newly introduced vulnerable dependencies.

High-severity dependency vulnerabilities fail the dependency review.

## Security

Dependency vulnerability checks run on pushes, pull requests, and on a weekly schedule.

## Continuous Deployment

`dev` represents active development.

`staging` represents the pre-production environment.

`main` represents production.

Deployment workflows are separated so that staging and production credentials remain isolated.
