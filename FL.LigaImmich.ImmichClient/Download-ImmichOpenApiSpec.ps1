#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Downloads the Immich OpenAPI spec into this project directory.

.PARAMETER Ref
    Git ref (tag, branch, or commit SHA) of the immich-app/immich repository
    to download the spec from. Defaults to the latest published release tag.

.PARAMETER OutputFile
    Destination file. Defaults to immich-openapi-specs.json next to this script.

.EXAMPLE
    pwsh ./Download-ImmichOpenApiSpec.ps1
    pwsh ./Download-ImmichOpenApiSpec.ps1 -Ref v1.118.0
#>
[CmdletBinding()]
param(
    [string]$Ref,
    [string]$OutputFile
)

$ErrorActionPreference = 'Stop'

$scriptDir = $PSScriptRoot
if (-not $OutputFile) {
    $OutputFile = Join-Path $scriptDir 'immich-openapi-specs.json'
}

if (-not $Ref) {
    Write-Host "Resolving latest immich-app/immich release..."
    $release = Invoke-RestMethod -Uri 'https://api.github.com/repos/immich-app/immich/releases/latest' -Headers @{ 'User-Agent' = 'LigaImmich' }
    $Ref = $release.tag_name
    Write-Host "Using release tag $Ref"
}

$uri = "https://github.com/immich-app/immich/raw/$Ref/open-api/immich-openapi-specs.json"
Write-Host "Downloading $uri"
Invoke-WebRequest -Uri $uri -OutFile $OutputFile

# Validate the download is actual JSON (GitHub serves a 404 HTML page on bad refs).
try {
    $null = Get-Content $OutputFile -Raw | ConvertFrom-Json
}
catch {
    Remove-Item $OutputFile -ErrorAction SilentlyContinue
    throw "Downloaded file is not valid JSON (ref '$Ref' may not exist): $($_.Exception.Message)"
}

# Record the resolved spec version alongside the file so the generated client is traceable.
Set-Content -Path (Join-Path $scriptDir 'immich-openapi-specs.ref') -Value $Ref -NoNewline

Write-Host "Wrote $OutputFile (ref: $Ref)"
