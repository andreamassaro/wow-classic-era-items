# wow-classic-era-items

An API that catalogs World of Warcraft: Classic Era items, backed by data pulled
from the Blizzard Game Data APIs and stored in SQL Server.

## Architecture

The solution is split into layered projects, wired together as
`Api -> Services -> Repository -> Domain`:

```
src/
  WowClassicEraItems.Api/          Minimal API endpoints, middleware, host/pipeline setup
  WowClassicEraItems.Services/     Business logic
  WowClassicEraItems.Repository/   DbContext, migrations, EF entities, Blizzard HTTP clients
  WowClassicEraItems.Domain/       Plain models shared across layers
tests/
  <mirrors the src/ project structure above>
infra/
  Bicep IaC for the Azure resources this app runs on
```

- `Api` only references `Services`, `Services` only references `Repository`.
Endpoints/Services depend on interfaces defined in `Repository`, never on
`DbContext` or the Blizzard SDK clients directly.
- Full conventions for contributing/extending this codebase (folder layout,
  DI registration patterns, testing conventions, etc.) are documented in
  [`AGENTS.md`](./AGENTS.md).

## What it does

- `GET /items` - lists all items (latest known snapshot of each).
- `GET /items/{id}` - returns a single item by its Blizzard item id, resolving
  and caching its icon URL from Blizzard's media API on first access.
- `POST /items/load` - fetches the full WoW Classic Era item catalog from
  Blizzard's item search API and stores it. Always inserts new snapshot rows
  (never updates), so item history can be tracked over time.

Swagger UI is available at `/swagger` in development (the root `/` redirects
there).

## Running locally

### 1. Start SQL Server

```bash
docker compose up -d
```

This starts a SQL Server 2022 container (default port `1433`, overridable via
a `SQL_PORT` env var / `.env` file - see `compose.yaml`).

### 2. Configure secrets

The API needs Blizzard API credentials (get them from
[develop.battle.net](https://develop.battle.net)). Store them in .NET User
Secrets rather than committing them:

```bash
cd src/WowClassicEraItems.Api
dotnet user-secrets set "Blizzard:ClientId" "<your-client-id>"
dotnet user-secrets set "Blizzard:ClientSecret" "<your-client-secret>"
```

The SQL Server connection string lives in `appsettings.Development.json`.

### 3. Apply database migrations

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet tool run dotnet-ef database update \
  --project src/WowClassicEraItems.Repository \
  --startup-project src/WowClassicEraItems.Api
```

(`dotnet-ef` is a local tool, see `.config/dotnet-tools.json` - restore it
with `dotnet tool restore` if needed.)

### 4. Run the API

```bash
dotnet run --project src/WowClassicEraItems.Api
```

Then hit `POST /items/load` once to populate the database from Blizzard.

## Running the tests

```bash
dotnet test
```

Tests use fakes/in-memory doubles (no live network or database dependency).

## Deploying to Azure

`infra/` contains Bicep templates that provision a free-tier Azure SQL
Database, an Azure Container Registry, a Key Vault (holding the SQL
connection string and Blizzard credentials, read by the Container App via
its system-assigned managed identity), and a Container App to run the API.
See the deployment instructions at the top of `infra/main.bicep`.
