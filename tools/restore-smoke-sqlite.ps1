[CmdletBinding()]
param(
    [string]$BackupDirectory = "",
    [string]$OutputDirectory = ".\artifacts\restore-smoke"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-BackupDirectory {
    param([string]$RequestedDirectory)

    if (-not [string]::IsNullOrWhiteSpace($RequestedDirectory)) {
        $resolved = Resolve-Path -LiteralPath $RequestedDirectory -ErrorAction SilentlyContinue
        if (-not $resolved) {
            throw "Backup directory '$RequestedDirectory' was not found."
        }

        return (Get-Item -LiteralPath $resolved.Path)
    }

    $backupRoot = ".\artifacts\backups"
    if (-not (Test-Path -LiteralPath $backupRoot)) {
        throw "No backup directory was provided and '$backupRoot' does not exist."
    }

    $latest = Get-ChildItem -LiteralPath $backupRoot -Directory |
        Sort-Object LastWriteTimeUtc -Descending |
        Select-Object -First 1

    if (-not $latest) {
        throw "No SQLite backup folders were found under '$backupRoot'."
    }

    return $latest
}

$backup = Resolve-BackupDirectory -RequestedDirectory $BackupDirectory
$sourceDatabase = Get-ChildItem -LiteralPath $backup.FullName -File -Filter "*.db" | Select-Object -First 1
if (-not $sourceDatabase) {
    throw "No .db file was found in backup directory '$($backup.FullName)'."
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$smokeDirectory = Join-Path -Path $OutputDirectory -ChildPath ("sqlite-" + $timestamp)
New-Item -ItemType Directory -Path $smokeDirectory -Force | Out-Null

$restoreDatabase = Join-Path -Path $smokeDirectory -ChildPath $sourceDatabase.Name
Copy-Item -LiteralPath $sourceDatabase.FullName -Destination $restoreDatabase -Force

foreach ($suffix in @("-wal", "-shm")) {
    $sourceSidecar = $sourceDatabase.FullName + $suffix
    if (Test-Path -LiteralPath $sourceSidecar) {
        Copy-Item -LiteralPath $sourceSidecar -Destination ($restoreDatabase + $suffix) -Force
    }
}

$connectionString = "Data Source=$restoreDatabase"
dotnet ef database update --no-build --connection $connectionString

if ($LASTEXITCODE -ne 0) {
    throw "EF migration smoke failed for restored SQLite copy '$restoreDatabase'."
}

$manifestPath = Join-Path -Path $smokeDirectory -ChildPath "restore-smoke-manifest.txt"
Set-Content -LiteralPath $manifestPath -Encoding UTF8 -Value @(
    "CreatedAtUtc=$((Get-Date).ToUniversalTime().ToString('O'))",
    "BackupDirectory=$($backup.FullName)",
    "RestoredCopy=$restoreDatabase",
    "Result=EF database update succeeded on restored copy"
)

Write-Host "SQLite restore smoke succeeded at $smokeDirectory"
