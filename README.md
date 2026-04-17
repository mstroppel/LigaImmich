
# LigaImmich

[![Deploy Images to GHCR](https://github.com/mstroppel/LigaImmich/actions/workflows/deploy-image-to-ghcr.yml/badge.svg)](https://github.com/mstroppel/LigaImmich/actions/workflows/deploy-image-to-ghcr.yml)

ASP.NET Core web app that utilizes Immich.app for Film-Liga needs.

## Next Steps

- [ ] fix build https://github.com/mstroppel/LigaImmich/actions/runs/24535778885/job/71730310921
- [ ] update README.md
- [ ] review everything and suggestions to the [implementation plan](implementation-plan.md)
- [ ] proper setup of [Claude settings](.claude/settings.json)
- [ ] migrate to slnx
- [ ] upgrade dependencies
- [ ] setup dependabot
- [ ] improve immich client generation
  - [ ] review and improve the Download-ImmichOpenApiSpec.ps1 script
  - [ ] setup that generated client is generated along compiling and no longer committed to GIT
