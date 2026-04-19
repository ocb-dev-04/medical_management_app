# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### Running Services (Development)
```bash
# Start all services + gateway via convenience script
./scripts/run_to_test.sh

# Run a single service
dotnet run --project src/services/auth/Services.Auth.Api -c Development
dotnet run --project src/services/doctor/Services.Doctor.Api -c Development
dotnet run --project src/services/patient/Services.Patient.Api -c Development
dotnet run --project src/services/diagnosis/Services.Diagnoses.Api -c Development
dotnet run --project src/gateway/Doctor.Management.Gateway -c Development
```

### Infrastructure (Docker)
```bash
docker-compose -f docker_docs/docker-compose.dev.yml up    # development
docker-compose -f docker_docs/docker-compose.prod.yml up   # production
```

### Testing
```bash
dotnet test src/ --no-restore --verbosity normal            # all tests
dotnet test src/tests/Services.Auth.Application.UnitTests   # single project
dotnet test src/tests/Services.Diagnoses.Application.UnitTests
```

### Build Docker Images
```bash
./scripts/build_docker_images.sh   # publishes all services for linux-x64 in Release mode
```

## Architecture

### Microservices with Clean Architecture

Each service (`auth`, `doctor`, `patient`, `diagnosis`) follows strict layered Clean Architecture:

```
Services.{Name}.Api          → ASP.NET Core host, DI registration, middleware
Services.{Name}.Presentation → Controllers, request/response DTOs
Services.{Name}.Application  → CQRS handlers (MediatR), FluentValidation, interfaces
Services.{Name}.Domain       → Entities, domain exceptions, value objects
Services.{Name}.Persistence  → EF Core DbContext, repositories, EF migrations
```

The `src/services/shared/` libraries provide cross-cutting concerns:
- `Shared.Domain` — base entity, aggregate root, result pattern
- `Shared.Global.Sources` — source generators
- `Shared.Consul.Configuration` — Consul-based service registration/discovery
- `Shared.Message.Queue.Requests` — MassTransit message contracts

### Request Flow

```
Client → API Gateway (YARP) → Consul (service discovery) → Microservice
                                                              ↓
                                                        MediatR Pipeline
                                                    (Validation → Handler)
                                                              ↓
                                                     Repository (+ Redis cache decorator)
                                                              ↓
                                                    PostgreSQL / MongoDB / Elasticsearch
```

Inter-service async communication goes through RabbitMQ via MassTransit.

### Key Patterns

- **CQRS**: Commands and Queries dispatched through MediatR. Use `CQRS.MediatR.Helper` for dispatch.
- **Repository + Cache Decorator**: Each repository interface has a concrete EF Core implementation and a Redis caching decorator registered over it.
- **Strongly-typed HTTP clients**: Cross-service synchronous calls use Refit clients.
- **Result pattern**: Operations return a `Result<T>` type from `Shared.Domain` rather than throwing for expected failures.
- **EF Core Fluent API**: All entity configurations live in `Persistence/Configurations/`, never data annotations. Split queries are enabled for performance.
- **Compiled Queries**: Used in persistence layer for hot-path queries.

### Infrastructure Services

| Service | Port | Purpose |
|---------|------|---------|
| PostgreSQL | 5432 | Primary relational store |
| MongoDB | 27017 | Document store |
| Redis | 6379 | Distributed cache |
| RabbitMQ | 5672 / 15672 | Message broker / management UI |
| Consul | 8500 | Service discovery |
| Elasticsearch | 9200 | Full-text search (Doctor, Diagnoses) |
| OTLP Collector | — | OpenTelemetry aggregation → Prometheus → Grafana |

### Configuration

Each service has `appsettings.json` and `appsettings.Development.json`. Key settings classes: `RelationalDatabaseSettings`, `CacheDatabaseSettings`, `MessageQueueSettings`, `ElasticSettings`, `ConsulSettings`, `ServiceRegistrationSettings`. The OTLP endpoint is controlled by the `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable.

### Testing

Unit tests use xUnit + Moq + FluentAssertions + Bogus (fake data). Tests live in `src/tests/` and target the `Application` layer only — no infrastructure dependencies in unit tests. See `UNIT_TEST_SPECIFICATION_STEPS.md` for the project's testing conventions.

### CI/CD

GitHub Actions (`.github/workflows/ci.yml`) runs on PRs to `sprint*`/`development` branches and pushes to `main`: restores NuGet (requires GitHub token for private packages), builds, and runs all tests with .NET 8.x.
