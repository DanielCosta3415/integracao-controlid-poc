[CmdletBinding()]
param(
    [string]$DatabasePath = ".\integracao_controlid.db",
    [string]$OutputDirectory = ".\artifacts\backups",
    [string]$MirrorDirectory = $env:CONTROLID_BACKUP_MIRROR_DIRECTORY,
    [ValidateSet("CurrentUser", "LocalMachine")]
    [string]$ProtectionScope = "CurrentUser",
    [switch]$Unprotected,
    [switch]$RunRestoreSmoke,
    [int]$RetentionDays = 0,
    [switch]$ApplyRetention,
    [string]$RetentionConfirmation = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot

function Resolve-RepoPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    if ([IO.Path]::IsPathRooted($Path)) {
        return [IO.Path]::GetFullPath($Path)
    }

    return [IO.Path]::GetFullPath((Join-Path -Path $root -ChildPath ($Path -replace '^[.][\\/]', '')))
}

function Get-BackupDirectories {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return @()
    }

    return @(Get-ChildItem -LiteralPath $Path -Directory -Filter "sqlite-*" | Sort-Object LastWriteTimeUtc -Descending)
}

$databaseFullPath = Resolve-RepoPath -Path $DatabasePath
$outputFullPath = Resolve-RepoPath -Path $OutputDirectory

$before = Get-BackupDirectories -Path $outputFullPath | ForEach-Object { $_.FullName }

$backupScript = Join-Path $PSScriptRoot "backup-sqlite.ps1"
$backupArguments = @(
    "-ExecutionPolicy", "Bypass",
    "-File", $backupScript,
    "-DatabasePath", $databaseFullPath,
    "-OutputDirectory", $outputFullPath,
    "-ProtectionScope", $ProtectionScope
)

if ($Unprotected) {
    $backupArguments += "-Unprotected"
}

powershell @backupArguments
if ($LASTEXITCODE -ne 0) {
    throw "backup-sqlite.ps1 failed with exit code $LASTEXITCODE."
}

$after = Get-BackupDirectories -Path $outputFullPath
$createdBackup = $after | Where-Object { $before -notcontains $_.FullName } | Select-Object -First 1
if (-not $createdBackup) {
    $createdBackup = $after | Select-Object -First 1
}

if (-not $createdBackup) {
    throw "Backup script completed but no sqlite-* backup directory was found under $outputFullPath."
}

Write-Host "Operational backup: $($createdBackup.FullName)"

if (-not [string]::IsNullOrWhiteSpace($MirrorDirectory)) {
    $mirrorRoot = Resolve-RepoPath -Path $MirrorDirectory
    if (-not (Test-Path -LiteralPath $mirrorRoot)) {
        New-Item -ItemType Directory -Force -Path $mirrorRoot | Out-Null
    }

    $mirrorDestination = Join-Path -Path $mirrorRoot -ChildPath $createdBackup.Name
    if (Test-Path -LiteralPath $mirrorDestination) {
        throw "Mirror destination already exists: $mirrorDestination"
    }

    Copy-Item -LiteralPath $createdBackup.FullName -Destination $mirrorDestination -Recurse
    Write-Host "Operational backup mirrored to: $mirrorDestination"
}
else {
    Write-Host "SKIP mirror: set CONTROLID_BACKUP_MIRROR_DIRECTORY or pass -MirrorDirectory for off-host copy."
}

if ($RunRestoreSmoke) {
    $restoreScript = Join-Path $PSScriptRoot "restore-smoke-sqlite.ps1"
    powershell -ExecutionPolicy Bypass -File $restoreScript -BackupDirectory $createdBackup.FullName -ProtectionScope $ProtectionScope
    if ($LASTEXITCODE -ne 0) {
        throw "restore-smoke-sqlite.ps1 failed with exit code $LASTEXITCODE."
    }
}
else {
    Write-Host "SKIP restore smoke: pass -RunRestoreSmoke to validate the created backup copy."
}

if ($RetentionDays -gt 0) {
    $cutoff = (Get-Date).AddDays(-$RetentionDays)
    $expired = @(Get-BackupDirectories -Path $outputFullPath | Where-Object { $_.LastWriteTime -lt $cutoff })

    if ($expired.Count -eq 0) {
        Write-Host "Retention: no backup directories older than $RetentionDays day(s)."
    }
    elseif ($ApplyRetention -and $RetentionConfirmation -eq "EXCLUIR BACKUPS ANTIGOS") {
        foreach ($directory in $expired) {
            $fullName = [IO.Path]::GetFullPath($directory.FullName)
            if (-not $fullName.StartsWith($outputFullPath, [StringComparison]::OrdinalIgnoreCase)) {
                throw "Refusing to remove backup outside output directory: $fullName"
            }

            Remove-Item -LiteralPath $fullName -Recurse -Force
            Write-Host "Retention removed: $fullName"
        }
    }
    else {
        Write-Host "Retention dry-run: $($expired.Count) backup directory/directories older than $RetentionDays day(s)."
        Write-Host "Pass -ApplyRetention -RetentionConfirmation 'EXCLUIR BACKUPS ANTIGOS' to remove them intentionally."
    }
}
