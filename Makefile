.PHONY: \
	up \
	down \
	infra \
	build \
	test \
	format \
	security \
	check \
	api \
	worker \
	migrate \
	migration \
	logs \
	clean

up:
	docker compose up -d

down:
	docker compose down

infra:
	docker compose up -d postgres redis rabbitmq

build:
	./scripts/build.sh

test:
	./scripts/test.sh

format:
	./scripts/format.sh

security:
	./scripts/security-check.sh

check:
	./scripts/build.sh
	./scripts/test.sh
	./scripts/format.sh

api:
	dotnet watch --project src/FinSight.Api

worker:
	dotnet watch --project src/FinSight.Workers

migrate:
	dotnet ef database update \
		--project src/FinSight.Infrastructure \
		--startup-project src/FinSight.Api

migration:
	dotnet ef migrations add $(name) \
		--project src/FinSight.Infrastructure \
		--startup-project src/FinSight.Api

logs:
	docker compose logs -f

clean:
	dotnet clean FinSight.slnx