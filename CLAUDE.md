# CLAUDE.md

This file provides guidance to Claude when working with this repository.

## Project Overview

**Liga Immich** is a .NET application that integrates with [Immich](https://immich.app/) (a self-hosted photo and video backup solution) for a sports league. It manages photo albums for league games via the Immich API.

## Tech Stack

- **Language**: C# / .NET 10
- **Solution file**: `FL.LigaImmich.slnx`
- **Projects**:
  - `FL.LigaImmich/` – Main application
  - `FL.LigaImmich.ImmichClient/` – Immich API client
  - `FL.LigaImmich.Tests/` – Test project
- **Container**: Docker (see `Dockerfile`)

## Development Commands

```bash
# Restore dependencies
dotnet restore FL.LigaImmich.slnx

# Build
dotnet build FL.LigaImmich.slnx --configuration Release --no-restore

# Run tests
dotnet test FL.LigaImmich.slnx --configuration Release --no-build --verbosity normal
```

## Environment

Copy `.env.example` to `.env` and fill in the required values before running locally.

## GitHub Workflow

- CI runs on pull requests targeting `main` for any `.cs` or `.csproj` changes
- Docker images are published on push to `main`
- Claude can be triggered via `@claude` in issue or PR comments

## Conventions

- Follow standard C# naming conventions (PascalCase for types/methods, camelCase for locals)
- Write tests for new features in `FL.LigaImmich.Tests/`
- Keep the Immich API client in `FL.LigaImmich.ImmichClient/`
