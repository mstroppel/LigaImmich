<#
.SYNOPSIS
    Deletes ALL tags from an Immich instance, using credentials from a .env file.

.DESCRIPTION
    Reads IMMICH_BASE_URL and IMMICH_API_KEY from the given .env file
    (default: .env next to this script), lists every tag via GET /tags,
    and deletes each one via DELETE /tags/{id}.

    This is DESTRUCTIVE: tag-to-asset associations are lost, and any
    tag hierarchy is removed. Assets themselves are kept.

.PARAMETER EnvFile
    Path to the .env file. Defaults to ./.env next to this script.

.PARAMETER Force
    Skip the interactive confirmation prompt.

.EXAMPLE
    ./Remove-AllTags.ps1

.EXAMPLE
    ./Remove-AllTags.ps1 -EnvFile ./.env.staging -Force
#>
[CmdletBinding()]
param(
    [string]$EnvFile = (Join-Path $PSScriptRoot '.env'),
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

function Read-DotEnv {
    param([Parameter(Mandatory)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw ".env file not found: $Path"
    }

    $values = @{}
    foreach ($line in Get-Content -LiteralPath $Path) {
        $trimmed = $line.Trim()
        if ($trimmed -eq '' -or $trimmed.StartsWith('#')) { continue }

        $eq = $trimmed.IndexOf('=')
        if ($eq -lt 1) { continue }

        $key = $trimmed.Substring(0, $eq).Trim()
        $val = $trimmed.Substring($eq + 1).Trim()

        # Strip surrounding quotes if present
        if (($val.StartsWith('"') -and $val.EndsWith('"')) -or
            ($val.StartsWith("'") -and $val.EndsWith("'"))) {
            $val = $val.Substring(1, $val.Length - 2)
        }

        $values[$key] = $val
    }
    return $values
}

$envValues = Read-DotEnv -Path $EnvFile

$baseUrl = $envValues['IMMICH_BASE_URL']
$apiKey  = $envValues['IMMICH_API_KEY']

if ([string]::IsNullOrWhiteSpace($baseUrl)) { throw "IMMICH_BASE_URL missing in $EnvFile" }
if ([string]::IsNullOrWhiteSpace($apiKey))  { throw "IMMICH_API_KEY missing in $EnvFile"  }

$baseUrl = $baseUrl.TrimEnd('/')
if ($baseUrl -notmatch '/api$') {
    $baseUrl = "$baseUrl/api"
}

$headers = @{
    'x-api-key' = $apiKey
    'Accept'    = 'application/json'
}

Write-Host "Fetching tags from $baseUrl ..."
$tags = Invoke-RestMethod -Method Get -Uri "$baseUrl/tags" -Headers $headers

if (-not $tags -or $tags.Count -eq 0) {
    Write-Host "No tags found. Nothing to do."
    return
}

Write-Host ("Found {0} tag(s):" -f $tags.Count)
$tags | Sort-Object value | ForEach-Object {
    Write-Host ("  - {0}  ({1})" -f $_.value, $_.id)
}

if (-not $Force) {
    $answer = Read-Host "Delete ALL $($tags.Count) tags? Type 'yes' to confirm"
    if ($answer -ne 'yes') {
        Write-Host "Aborted."
        return
    }
}

$deleted = 0
$failed  = 0
foreach ($tag in $tags) {
    try {
        Invoke-RestMethod -Method Delete -Uri "$baseUrl/tags/$($tag.id)" -Headers $headers | Out-Null
        $deleted++
        Write-Host ("Deleted {0} ({1})" -f $tag.value, $tag.id)
    } catch {
        $failed++
        Write-Warning ("Failed to delete {0} ({1}): {2}" -f $tag.value, $tag.id, $_.Exception.Message)
    }
}

Write-Host ""
Write-Host ("Done. Deleted {0}/{1} tag(s). Failed: {2}." -f $deleted, $tags.Count, $failed)
