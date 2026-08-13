[CmdletBinding()]
param(
    [string]$DatabasePath = ".\integracao_controlid.db",
    [string]$DataProtectionKeyPath = ".\artifacts\data-protection-keys",
    [Parameter(Mandatory = $true)]
    [string]$CertificatePath,
    [Parameter(Mandatory = $true)]
    [string]$CertificatePasswordFile,
    [string]$BackupDirectory = ".\artifacts\backups",
    [switch]$ConfirmProtection
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not $ConfirmProtection) {
    throw "This command rewrites sensitive SQLite columns. Review the backup location and pass -ConfirmProtection explicitly."
}

$root = Split-Path -Parent $PSScriptRoot

function Resolve-RepoPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    if ([IO.Path]::IsPathRooted($Path)) {
        return [IO.Path]::GetFullPath($Path)
    }

    return [IO.Path]::GetFullPath((Join-Path $root ($Path -replace '^[.][\\/]', '')))
}

$database = Resolve-RepoPath -Path $DatabasePath
$keyPath = Resolve-RepoPath -Path $DataProtectionKeyPath
$certificate = Resolve-RepoPath -Path $CertificatePath
$certificatePassphraseFilePath = Resolve-RepoPath -Path $CertificatePasswordFile
$backupDirectoryPath = Resolve-RepoPath -Path $BackupDirectory

foreach ($requiredFile in @($database, $certificate, $certificatePassphraseFilePath)) {
    if (-not (Test-Path -LiteralPath $requiredFile -PathType Leaf)) {
        throw "Required file not found: $requiredFile"
    }
}

& (Join-Path $PSScriptRoot "backup-sqlite-operational.ps1") `
    -DatabasePath $database `
    -OutputDirectory $backupDirectoryPath `
    -RunRestoreSmoke
if ($LASTEXITCODE -ne 0) {
    throw "Protected backup and restore smoke failed. The database was not rewritten."
}

$previousEnvironment = @{}
$settings = @{
    ASPNETCORE_ENVIRONMENT = "Development"
    ConnectionStrings__DefaultConnection = "Data Source=$database;Default Timeout=5;Foreign Keys=True;Pooling=True"
    DataProtection__KeyPath = $keyPath
    DataProtection__CertificatePath = $certificate
    DataProtection__CertificatePasswordFile = $certificatePassphraseFilePath
    Database__ApplyMigrationsOnStartup = "true"
    Database__ExitAfterMigrations = "true"
    Database__Encryption__RequireProtectedSensitiveColumns = "true"
    Database__Encryption__ProtectLegacyDataOnStartup = "true"
}

try {
    foreach ($setting in $settings.GetEnumerator()) {
        $previousEnvironment[$setting.Key] = [Environment]::GetEnvironmentVariable($setting.Key, "Process")
        [Environment]::SetEnvironmentVariable($setting.Key, $setting.Value, "Process")
    }

    Push-Location $root
    dotnet run --project .\Integracao.ControlID.PoC.csproj --no-launch-profile
    if ($LASTEXITCODE -ne 0) {
        throw "Sensitive data protection failed with exit code $LASTEXITCODE. Preserve the backup and investigate before retrying."
    }
}
finally {
    Pop-Location
    foreach ($setting in $previousEnvironment.GetEnumerator()) {
        [Environment]::SetEnvironmentVariable($setting.Key, $setting.Value, "Process")
    }
}

Write-Host "Sensitive SQLite columns were protected successfully. Keep the certificate, password and key ring with the backup set."
