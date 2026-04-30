[CmdletBinding()]
param(
    [string]$DatabasePath = ".\integracao_controlid.db",
    [string]$OutputDirectory = ".\artifacts\backups"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$resolvedDatabase = Resolve-Path -LiteralPath $DatabasePath -ErrorAction SilentlyContinue
if (-not $resolvedDatabase) {
    Write-Error "SQLite database not found at '$DatabasePath'. Run this command from the repository root or pass -DatabasePath."
    exit 2
}

$databaseItem = Get-Item -LiteralPath $resolvedDatabase.Path
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$backupDirectory = Join-Path -Path $OutputDirectory -ChildPath ("sqlite-" + $timestamp)

New-Item -ItemType Directory -Path $backupDirectory -Force | Out-Null

$copiedFiles = New-Object System.Collections.Generic.List[string]
$candidatePaths = @(
    $databaseItem.FullName,
    ($databaseItem.FullName + "-wal"),
    ($databaseItem.FullName + "-shm")
)

foreach ($candidatePath in $candidatePaths) {
    if (Test-Path -LiteralPath $candidatePath) {
        $destination = Join-Path -Path $backupDirectory -ChildPath ([System.IO.Path]::GetFileName($candidatePath))
        Copy-Item -LiteralPath $candidatePath -Destination $destination -Force
        $copiedFiles.Add($destination)
    }
}

$manifestPath = Join-Path -Path $backupDirectory -ChildPath "backup-manifest.txt"
$manifest = @(
    "CreatedAtUtc=$((Get-Date).ToUniversalTime().ToString('O'))",
    "SourceDatabase=$($databaseItem.FullName)",
    "CopiedFiles="
)

foreach ($copiedFile in $copiedFiles) {
    $manifest += "- $copiedFile"
}

Set-Content -LiteralPath $manifestPath -Value $manifest -Encoding UTF8

Write-Host "SQLite backup created at $backupDirectory"
