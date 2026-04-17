
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
dotnet run --project FL.LigaImmich
```

Configure the Immich endpoint and API key in [FL.LigaImmich/appsettings.json](FL.LigaImmich/appsettings.json) or via environment variables (see below).

## Running in Docker

Images are published to `ghcr.io/mstroppel/liga-immich`. See the [Dockerfile](Dockerfile).

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

The typed client in [FL.LigaImmich.ImmichClient](FL.LigaImmich.ImmichClient/) is produced by NSwag from Immich's OpenAPI spec.

1. Refresh the spec: `pwsh FL.LigaImmich.ImmichClient/Download-ImmichOpenApiSpec.ps1`
2. Rebuild `FL.LigaImmich.ImmichClient.Generation` to regenerate the client: `dotnet build FL.LigaImmich.ImmichClient.Generation`

## Next Steps

See [implementation-plan.md](implementation-plan.md) for the current backlog and execution order.
