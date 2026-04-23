
# LigaImmich

[![Build and Test](https://github.com/filmliga66/liga-immich/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/filmliga66/liga-immich/actions/workflows/build-and-test.yml)
[![Docker Publish](https://github.com/filmliga66/liga-immich/actions/workflows/docker-publish.yml/badge.svg)](https://github.com/filmliga66/liga-immich/actions/workflows/docker-publish.yml)

.NET background worker that automates [Immich](https://immich.app/) for Film-Liga needs. Runs on a cron schedule and executes scheduled tasks against an Immich server via its REST API.

## Projects

- [FL.LigaImmich](FL.LigaImmich/) — the worker host (cron scheduler + scheduled tasks).
- [FL.LigaImmich.ImmichClient](FL.LigaImmich.ImmichClient/) — typed Immich API client generated from the upstream OpenAPI spec.
- [FL.LigaImmich.ImmichClient.Generation](FL.LigaImmich.ImmichClient.Generation/) — helper project used to regenerate the client.

## Running locally

```bash
# one-time: store your Immich API key outside of source control
dotnet user-secrets --project FL.LigaImmich set "Immich:ApiKey" "<your-api-key>"

dotnet run --project FL.LigaImmich
```

The Immich base URL defaults to `http://localhost:2283/api` in `Development` (see [appsettings.Development.json](FL.LigaImmich/appsettings.Development.json)); override it with user-secrets or environment variables if needed. Missing/empty config fails fast at startup via options validation.

## Running in Docker

Images are published to `ghcr.io/filmliga66/liga-immich`. See the [Dockerfile](Dockerfile).

```bash
# copy .env.example to .env and fill in your values
cp .env.example .env

docker run --rm --env-file .env ghcr.io/filmliga66/liga-immich:latest
```

Or pass overrides directly:

```bash
docker run --rm \
  -e IMMICH_BASE_URL="https://immich.example.com/api" \
  -e IMMICH_API_KEY="<your-api-key>" \
  ghcr.io/filmliga66/liga-immich:latest
```

## Configuration

The worker exposes a Docker-friendly `SCREAMING_SNAKE_CASE` env var for each setting in [appsettings.json](FL.LigaImmich/appsettings.json). These are mapped to the underlying .NET config keys at startup (see [EnvironmentVariableConfiguration.cs](FL.LigaImmich/EnvironmentVariableConfiguration.cs)):

| Setting                                              | Env var                                            | Description                                                          |
| ---------------------------------------------------- | -------------------------------------------------- | -------------------------------------------------------------------- |
| `Immich:BaseUrl`                                     | `IMMICH_BASE_URL`                                  | Immich API base URL (e.g. `https://immich.example.com/api`).         |
| `Immich:ApiKey`                                      | `IMMICH_API_KEY`                                   | Immich API key.                                                      |
| `Scheduler:TimeZone`                                 | `SCHEDULER_TIMEZONE`                               | IANA time zone used for cron expressions (default: `Europe/Berlin`). |
| `Scheduler:Tasks:TagAssetsByClub:Cron`               | `SCHEDULER_TAG_ASSETS_BY_CLUB_CRON`                | Cron expression for the club-tag task.                               |
| `Scheduler:Tasks:TagAssetsByClub:Enabled`            | `SCHEDULER_TAG_ASSETS_BY_CLUB_ENABLED`             | Enable/disable the club-tag task.                                    |
| `Scheduler:Tasks:TagAssetsByEvent:Cron`              | `SCHEDULER_TAG_ASSETS_BY_EVENT_CRON`               | Cron expression for the event-tag task.                              |
| `Scheduler:Tasks:TagAssetsByEvent:Enabled`           | `SCHEDULER_TAG_ASSETS_BY_EVENT_ENABLED`            | Enable/disable the event-tag task.                                   |
| `Scheduler:Tasks:TagAssetsByFolderStructure:Cron`    | `SCHEDULER_TAG_ASSETS_BY_FOLDER_STRUCTURE_CRON`    | Cron expression for the folder-structure-tag task.                   |
| `Scheduler:Tasks:TagAssetsByFolderStructure:Enabled` | `SCHEDULER_TAG_ASSETS_BY_FOLDER_STRUCTURE_ENABLED` | Enable/disable the folder-structure-tag task.                        |
| `Scheduler:Tasks:TagAssetsByYear:Cron`               | `SCHEDULER_TAG_ASSETS_BY_YEAR_CRON`                | Cron expression for the year-tag task.                               |
| `Scheduler:Tasks:TagAssetsByYear:Enabled`            | `SCHEDULER_TAG_ASSETS_BY_YEAR_ENABLED`             | Enable/disable the year-tag task.                                    |

## Regenerating the Immich client

The typed client in [FL.LigaImmich.ImmichClient](FL.LigaImmich.ImmichClient/) is produced by NSwag at build time from the committed [immich-openapi-specs.json](FL.LigaImmich.ImmichClient/immich-openapi-specs.json), so builds stay deterministic and offline-capable. The generated `ImmichClient.g.cs` is gitignored — don't commit it.

Bumping the Immich version is a manual, two-step dev action:

```bash
# pin a specific tag (or run with no -Ref to fetch latest release)
pwsh FL.LigaImmich.ImmichClient/Download-ImmichOpenApiSpec.ps1 -Ref v1.130.0

# rebuild to regenerate the client, then commit the updated spec + ref
dotnet build FL.LigaImmich.slnx
git add FL.LigaImmich.ImmichClient/immich-openapi-specs.json FL.LigaImmich.ImmichClient/immich-openapi-specs.ref
```

## Next Steps

See [implementation-plan.md](implementation-plan.md) for the current backlog and execution order.
