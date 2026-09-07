# FinSight Documentation

This directory contains the detailed technical documentation for the current FinSight implementation.

## Documentation map

| Document | Purpose |
|---|---|
| [`architecture.md`](architecture.md) | Runtime architecture, layers, dependencies, synchronous/asynchronous boundaries and reliability flow |
| [`domain.md`](domain.md) | Domain entities, lifecycles and business-model relationships |
| [`api.md`](api.md) | Current versioned HTTP API endpoints and important request shapes |
| [`development.md`](development.md) | Local setup, commands, package/migration conventions and feature-development workflow |
| [`testing.md`](testing.md) | Test projects, testing pyramid and important scenarios |
| [`operations.md`](operations.md) | Local service map, health, observability, outbox, failures and operational guidance |
| [`ci-cd/`](ci-cd/) | Branching, CI/CD, deployment, rollback and secrets management |

## Source of truth

The documentation is derived from the implementation represented by the repository's current `dev` branch snapshot.

For API behavior, the controllers and application services are authoritative.
For domain behavior, the Domain entities are authoritative.
For infrastructure/runtime behavior, Infrastructure, Worker, configuration and Docker Compose code are authoritative.
