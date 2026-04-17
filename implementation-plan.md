
# Implementation Plan of the Next Steps

Source: `Next Steps` list in [README.md](README.md#next-steps).

## 1. Fix failing CI build

- Inspect the failing run: [workflow run 24535778885](https://github.com/mstroppel/LigaImmich/actions/runs/24535778885/job/71730310921) via `gh run view 24535778885 --log-failed`.
- Reproduce locally with `dotnet build FL.LigaImmich.sln` using the SDK pinned in [global.json](global.json) (`10.0.0`, latestMinor).
- Fix the root cause rather than relaxing the build (e.g. no `ContinueOnError`, no suppressed warnings).
- Verify by re-running the workflow on a PR before moving on.

## 2. Update README.md

- Replace the "ASP.NET Core web app" description with the current reality: a cron-scheduled background worker (see commit `fde54d0`).
- Document:
  - What the worker does (Immich integration for Film-Liga).
  - How to configure it (`appsettings.json`, required env vars / secrets).
  - How to run locally (`dotnet run --project FL.LigaImmich`) and via Docker ([Dockerfile](Dockerfile)).
  - How the Immich client is regenerated (forward-link to section 8).
- Keep the Next Steps list in README short; link to this plan for details.

### 2a. Status badges

- README already shows the GHCR deploy badge. Extend with badges for every workflow in [.github/workflows/](.github/workflows/):
  - [build-and-test.yml](.github/workflows/build-and-test.yml) — build & test status.
  - [docker-publish.yml](.github/workflows/docker-publish.yml) — image publish status.
- Once Dependabot (section 7) and a coverage step (section 9) are in place, add badges for them too (e.g. Codecov / `shields.io`).
- Group badges at the very top of the README so CI health is visible at a glance.

## 3. Review plan and collect suggestions

- After sections 1-2 land, walk through each remaining section with the user and refine scope/ordering.
- Capture any new follow-ups here rather than back in the README.

## 4. Proper setup of `.claude/settings.json`

- Current allowlist ([settings.json](.claude/settings.json)) covers `gh *`, `git *`, `dotnet *`. Review which additional commands are actually used in this repo (e.g. `docker *`, `pwsh *`, `nswag *`).
- Consider running the `less-permission-prompts` skill on recent transcripts to derive a data-driven allowlist.
- Split between user-level and project-level settings: project settings should only contain rules specific to this repo.
- Add hooks if we want automated behaviors (e.g. run `dotnet format` on stop) — only if the user asks.

## 5. Migrate to `.slnx`

- Convert [FL.LigaImmich.sln](FL.LigaImmich.sln) to the new XML-based `.slnx` format via `dotnet sln migrate` (requires .NET 9 SDK or newer — covered by `global.json`).
- Verify all three projects still load:
  - [FL.LigaImmich](FL.LigaImmich/FL.LigaImmich.csproj)
  - [FL.LigaImmich.ImmichClient](FL.LigaImmich.ImmichClient/FL.LigaImmich.ImmichClient.csproj)
  - [FL.LigaImmich.ImmichClient.Generation](FL.LigaImmich.ImmichClient.Generation/FL.LigaImmich.ImmichClient.Generation.csproj)
- Update CI workflows and [Dockerfile](Dockerfile) to reference the new solution file.
- Delete the old `.sln` once green.

## 6. Upgrade dependencies

- Run `dotnet list package --outdated` across the solution and record the diff.
- Upgrade in logical batches (framework → Immich SDK helpers → tooling) so regressions are bisectable.
- Update target framework to the latest LTS if we are not already on it and confirm container base image in [Dockerfile](Dockerfile) matches.
- Run the worker end-to-end after each batch.

## 7. Setup Dependabot

- Add `.github/dependabot.yml` with:
  - `nuget` ecosystem, weekly schedule, grouped minor/patch updates.
  - `github-actions` ecosystem for the workflows under `.github/workflows/`.
  - `docker` ecosystem for [Dockerfile](Dockerfile) base images.
- Gate auto-merge behind green CI once section 1 is stable.

## 8. Improve Immich client generation

### 8a. Review and improve `Download-ImmichOpenApiSpec.ps1`

- Current script ([Download-ImmichOpenApiSpec.ps1](FL.LigaImmich.ImmichClient/Download-ImmichOpenApiSpec.ps1)) is a single `Invoke-WebRequest` against `main`.
- Improvements:
  - Pin to a released Immich tag (configurable parameter, default to latest release via GitHub API).
  - Fail fast on HTTP errors (`-ErrorAction Stop`) and validate the downloaded JSON.
  - Emit the resolved version into a side file so generated code is traceable to a spec version.
  - Make the script idempotent and cross-platform (works under `pwsh` on Linux CI).

### 8b. Generate client at build time, stop committing it to git

- Today [ImmichClient.g.cs](FL.LigaImmich.ImmichClient/ImmichClient.g.cs) is committed and regenerated manually via [FL.LigaImmich.ImmichClient.Generation](FL.LigaImmich.ImmichClient.Generation/).
- Target flow:
  - Move spec download + NSwag generation into an MSBuild `BeforeCompile` target inside [FL.LigaImmich.ImmichClient.csproj](FL.LigaImmich.ImmichClient/FL.LigaImmich.ImmichClient.csproj).
  - Output generated files into `obj/` so they are not tracked.
  - Cache the downloaded spec by hash to keep incremental builds fast and offline-friendly (skip download if hash unchanged).
  - Retire the standalone `FL.LigaImmich.ImmichClient.Generation` project once the build-time flow works.
- Git hygiene: add `ImmichClient.g.cs` and `immich-openapi-specs.json` to `.gitignore` and remove them from history in the same PR.
- CI: ensure the build agent has network access for the download step, or publish the spec as a build artifact we can restore offline.

## 9. Refactor configuration

- Today [appsettings.json](FL.LigaImmich/appsettings.json) holds defaults (Immich base URL, API key placeholder, scheduler cron). Goal: same config bindable from three sources without code changes — `appsettings.json`, environment variables (Docker), and local dev overrides.
- Strongly-typed options:
  - Introduce `ImmichOptions` and `SchedulerOptions` records bound via `services.AddOptions<T>().BindConfiguration("Immich")` etc., with `DataAnnotations` validation (`Required` `BaseUrl`, `ApiKey`) and `ValidateOnStart()` so misconfiguration fails fast at worker startup.
  - Centralize binding in one extension method so new options are added in a single place.
- Docker / environment variables:
  - Document the mapping explicitly, e.g. `Immich__BaseUrl`, `Immich__ApiKey`, `Scheduler__TimeZone`, `Scheduler__Tasks__SyncAlbums__Cron` (double underscore = section separator).
  - Update [Dockerfile](Dockerfile) / compose samples with a documented `.env` example (no real secrets committed).
  - Remove the empty `ApiKey` default from `appsettings.json` so a missing env var surfaces as a validation error instead of silently using `""`.
- Developer ergonomics:
  - Add `appsettings.Development.json` (committed, non-secret defaults only) and enable user-secrets on [FL.LigaImmich.csproj](FL.LigaImmich/FL.LigaImmich.csproj) via `<UserSecretsId>` so `dotnet user-secrets set "Immich:ApiKey" ...` just works.
  - Ensure host builder loads `appsettings.{Environment}.json`, user-secrets (Development only), and environment variables in that precedence order.
  - Commit a `.env.example` at the repo root listing every override key with inline comments.
  - README section: a 5-line "first run" recipe for both local dev (user-secrets) and Docker (`.env`).
- Validate both paths with a smoke test: worker boots from `dotnet run` using user-secrets, and from `docker run -e Immich__ApiKey=...` using env vars only.

## 10. Add a test project

- Create `FL.LigaImmich.Tests` alongside the existing projects, using xUnit + `Microsoft.NET.Test.Sdk` and `FluentAssertions` (pick whatever lines up with current .NET SDK norms).
- Wire it into the solution (or `.slnx` after section 5) so `dotnet test` from the repo root discovers it.
- Seed initial coverage on the parts most likely to regress:
  - Scheduling logic under [FL.LigaImmich/Scheduling/](FL.LigaImmich/Scheduling/) (cron parsing, next-run calculation).
  - Task orchestration under [FL.LigaImmich/Tasks/](FL.LigaImmich/Tasks/) with the Immich client faked/mocked.
  - Configuration binding from `appsettings.json`.
- Keep external calls (HTTP to Immich) behind an interface so tests never hit the network.
- Update [build-and-test.yml](.github/workflows/build-and-test.yml) to run `dotnet test` and upload a TRX / coverage report as an artifact.
- Once green, add a test-status badge (ties into section 2a).

## Suggested execution order

1, 2, 2a, 4 → 5 → 9 → 10 → 6 → 7 → 8a → 8b. Section 3 runs continuously as each step lands.
