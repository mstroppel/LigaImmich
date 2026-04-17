
# LigaImmich

[![Build and Test](https://github.com/mstroppel/LigaImmich/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/mstroppel/LigaImmich/actions/workflows/build-and-test.yml)
[![Docker Publish](https://github.com/mstroppel/LigaImmich/actions/workflows/docker-publish.yml/badge.svg)](https://github.com/mstroppel/LigaImmich/actions/workflows/docker-publish.yml)

.NET background worker that automates [Immich](https://immich.app/) for Film-Liga needs. Runs on a cron schedule and executes scheduled tasks (currently: album synchronisation) against an Immich server via its REST API.

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

Images are published to `ghcr.io/mstroppel/liga-immich`. See the [Dockerfile](Dockerfile).

```bash
# copy .env.example to .env and fill in your values
cp .env.example .env

docker run --rm --env-file .env ghcr.io/mstroppel/liga-immich:latest
```

Or pass overrides directly:

```bash
docker run --rm \
  -e Immich__BaseUrl="https://immich.example.com/api" \
  -e Immich__ApiKey="<your-api-key>" \
  ghcr.io/mstroppel/liga-immich:latest
```

## Configuration

All settings in [appsettings.json](FL.LigaImmich/appsettings.json) can be overridden via environment variables using the .NET double-underscore convention (`Section__Key`):

| Setting                              | Env var                                 | Description                                                          |
| ------------------------------------ | --------------------------------------- | -------------------------------------------------------------------- |
| `Immich:BaseUrl`                     | `Immich__BaseUrl`                       | Immich API base URL (e.g. `https://immich.example.com/api`).         |
| `Immich:ApiKey`                      | `Immich__ApiKey`                        | Immich API key.                                                      |
| `Scheduler:TimeZone`                 | `Scheduler__TimeZone`                   | IANA time zone used for cron expressions (default: `Europe/Berlin`). |
| `Scheduler:Tasks:SyncAlbums:Cron`    | `Scheduler__Tasks__SyncAlbums__Cron`    | Cron expression for the album sync task.                             |
| `Scheduler:Tasks:SyncAlbums:Enabled` | `Scheduler__Tasks__SyncAlbums__Enabled` | Enable/disable the album sync task.                                  |

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
