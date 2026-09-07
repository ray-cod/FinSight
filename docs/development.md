# FinSight Development Guide

## Prerequisites

Install:

- .NET 10 SDK;
- Docker Engine + Docker Compose;
- Node.js;
- pnpm 11.

## First-time setup

```bash
git clone <repository>
cd FinSight
cp .env.example .env
```

Start local infrastructure:

```bash
make infra
```

Restore/build:

```bash
dotnet restore
dotnet build FinSight.slnx
```

Apply migrations:

```bash
make migrate
```

## Run backend

API:

```bash
make api
```

Worker:

```bash
make worker
```

## Run frontend

```bash
cd frontend/finsight-web
pnpm install
pnpm dev
```

The current frontend is a scaffold and is not wired to the API yet.

## Make targets

| Target | Command |
|---|---|
| `up` | Start complete Docker stack |
| `down` | Stop Docker stack |
| `infra` | Start PostgreSQL, Redis and RabbitMQ |
| `build` | Run repository build script |
| `test` | Run test script |
| `format` | Run formatting script |
| `security` | Run dependency/security script |
| `check` | Build + test + format |
| `api` | Start API with `dotnet watch` |
| `worker` | Start workers with `dotnet watch` |
| `migrate` | Apply EF migrations |
| `migration` | Add a new migration with `name=<MigrationName>` |
| `logs` | Follow Docker Compose logs |
| `clean` | Clean the solution |

## Adding a database change

Create a migration:

```bash
make migration name=MyChange
```

Review generated SQL and migration metadata before committing.

Apply locally:

```bash
make migrate
```

## Adding a new domain feature

Use this structure:

1. Add domain entities/enums/rules to `FinSight.Domain`.
2. Add Application abstractions and use cases to `FinSight.Application`.
3. Add integration events to `FinSight.Contracts` when asynchronous boundaries are needed.
4. Add infrastructure implementations to `FinSight.Infrastructure`.
5. Add HTTP endpoints to `FinSight.Api` only when the feature is externally exposed.
6. Add worker/consumer code to `FinSight.Workers` when processing should be asynchronous.
7. Add domain/application/infrastructure/integration tests.
8. Update documentation.

## Package management

NuGet versions are centrally managed through `Directory.Packages.props`.

Do not introduce one-off package versions inside individual project files unless there is a deliberate repository-wide reason.

## Coding conventions

- Keep public APIs XML documented.
- Propagate `CancellationToken`.
- Prefer `required` members where appropriate.
- Avoid leaking Infrastructure types into Domain/Application.
- Keep controllers thin.
- Keep user-ownership checks at repository/service boundaries.
- Prefer explicit domain methods to direct property mutation.
- Add tests with every behavior change.
