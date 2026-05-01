[CmdletBinding()]
param(
    [string]$ConfigPath = ".\ops.local.json",
    [string]$ExamplePath = ".\ops.example.json",
    [string]$ReportPath = ".\artifacts\operational-readiness\operational-readiness-latest.md",
    [switch]$RequireConfig
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

function Add-Result {
    param(
        [System.Collections.Generic.List[object]]$Results,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Status,
        [Parameter(Mandatory = $true)][string]$Detail
    )

    $Results.Add([pscustomobject]@{
        Name = $Name
        Status = $Status
        Detail = $Detail
    })
}

function Read-JsonFile {
    param([Parameter(Mandatory = $true)][string]$Path)

    $fullPath = Resolve-RepoPath -Path $Path
    if (-not (Test-Path -LiteralPath $fullPath)) {
        throw "Arquivo nao encontrado: $fullPath"
    }

    return Get-Content -Raw -LiteralPath $fullPath | ConvertFrom-Json
}

function Get-RequiredValue {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $current = $Object
    foreach ($segment in $Path.Split(".")) {
        if ($null -eq $current -or -not $current.PSObject.Properties[$segment]) {
            throw "Campo obrigatorio ausente: $Path"
        }

        $current = $current.$segment
    }

    $value = [string]$current
    if ([string]::IsNullOrWhiteSpace($value)) {
        throw "Campo obrigatorio vazio: $Path"
    }

    return $value
}

function Test-PlaceholderValue {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $true
    }

    $normalized = $Value.Trim().ToLowerInvariant()
    $placeholderTokens = @(
        "replace",
        "placeholder",
        "pending",
        "pendente",
        "todo",
        "tbd",
        "example",
        "necessita",
        "<",
        ">"
    )

    foreach ($token in $placeholderTokens) {
        if ($normalized.Contains($token)) {
            return $true
        }
    }

    return $false
}

function Assert-OperationalConfig {
    param(
        [Parameter(Mandatory = $true)]$Config,
        [switch]$AllowPlaceholders
    )

    $requiredFields = @(
        "contacts.incidentCommander.name",
        "contacts.incidentCommander.escalationChannel",
        "contacts.srePrimary.name",
        "contacts.srePrimary.escalationChannel",
        "contacts.techLead.name",
        "contacts.techLead.escalationChannel",
        "contacts.releaseEngineer.name",
        "contacts.releaseEngineer.escalationChannel",
        "contacts.dpoOrPrivacyOwner.name",
        "contacts.dpoOrPrivacyOwner.escalationChannel",
        "contacts.businessOwner.name",
        "contacts.businessOwner.escalationChannel",
        "communication.incidentChannel",
        "communication.statusCadence",
        "communication.stakeholderList",
        "evidence.repository",
        "evidence.retention",
        "backup.externalBackupTarget",
        "backup.restoreTestCadence",
        "backup.retentionPolicy",
        "backup.encryptionOwner",
        "rtoRpo.validationStatus",
        "rtoRpo.applicationRestartRto",
        "rtoRpo.sqliteRestoreRto",
        "rtoRpo.sqliteBackupRpo",
        "rtoRpo.lastValidationDate",
        "equipment.manualAccessProcedureOwner",
        "equipment.fallbackProcedureLocation",
        "equipment.vendorSupportChannel",
        "equipment.testCadence",
        "securityIncident.evidenceRepository",
        "securityIncident.dpoEscalation",
        "securityIncident.secretRotationOwner"
    )

    foreach ($field in $requiredFields) {
        $value = Get-RequiredValue -Object $Config -Path $field
        if (-not $AllowPlaceholders -and (Test-PlaceholderValue -Value $value)) {
            throw "Campo '$field' ainda parece placeholder ou pendente."
        }
    }

    if (-not $AllowPlaceholders) {
        $status = (Get-RequiredValue -Object $Config -Path "rtoRpo.validationStatus").Trim().ToLowerInvariant()
        if ($status -notin @("validated", "validado", "approved", "aprovado", "homologated", "homologado")) {
            throw "rtoRpo.validationStatus precisa indicar validacao/aprovacao real para release."
        }
    }
}

function Assert-TextContains {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string[]]$Patterns
    )

    $fullPath = Resolve-RepoPath -Path $Path
    if (-not (Test-Path -LiteralPath $fullPath)) {
        throw "Arquivo nao encontrado: $fullPath"
    }

    $content = Get-Content -Raw -LiteralPath $fullPath
    foreach ($pattern in $Patterns) {
        if ($content.IndexOf($pattern, [StringComparison]::Ordinal) -lt 0) {
            throw "Arquivo '$Path' nao contem marcador obrigatorio: $pattern"
        }
    }
}

function Write-Report {
    param([System.Collections.Generic.List[object]]$Results)

    $fullPath = Resolve-RepoPath -Path $ReportPath
    $directory = Split-Path -Parent $fullPath
    if (-not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add("# Operational readiness check")
    $lines.Add("")
    $lines.Add("Data: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss zzz")")
    $lines.Add("RequireConfig: $RequireConfig")
    $lines.Add("")

    foreach ($result in $Results) {
        $lines.Add("- [$($result.Status)] $($result.Name): $($result.Detail)")
    }

    [IO.File]::WriteAllLines($fullPath, $lines, [Text.UTF8Encoding]::new($true))
    Write-Host "Relatorio: $fullPath"
}

$results = [System.Collections.Generic.List[object]]::new()
$hasFailure = $false

try {
    Assert-TextContains -Path "docs\incident-response-and-dr.md" -Patterns @(
        "Matriz de severidade",
        "RTO/RPO",
        "ops.local.json",
        "IR-14 Secret comprometido"
    )
    Add-Result -Results $results -Name "incident-runbook" -Status "PASS" -Detail "Runbook de incidentes cobre SEV, RTO/RPO e configuracao operacional."
}
catch {
    $hasFailure = $true
    Add-Result -Results $results -Name "incident-runbook" -Status "FAIL" -Detail $_.Exception.Message
}

try {
    Assert-TextContains -Path "docs\equipment-contingency-runbook.md" -Patterns @(
        "Manual fallback",
        "Control iD",
        "Contingency validation"
    )
    Add-Result -Results $results -Name "equipment-contingency" -Status "PASS" -Detail "Runbook de contingencia fisica encontrado."
}
catch {
    $hasFailure = $true
    Add-Result -Results $results -Name "equipment-contingency" -Status "FAIL" -Detail $_.Exception.Message
}

try {
    $example = Read-JsonFile -Path $ExamplePath
    Assert-OperationalConfig -Config $example -AllowPlaceholders
    Add-Result -Results $results -Name "ops-example" -Status "PASS" -Detail "Exemplo operacional versionado contem todos os campos obrigatorios."
}
catch {
    $hasFailure = $true
    Add-Result -Results $results -Name "ops-example" -Status "FAIL" -Detail $_.Exception.Message
}

$configFullPath = Resolve-RepoPath -Path $ConfigPath
if (Test-Path -LiteralPath $configFullPath) {
    try {
        $config = Read-JsonFile -Path $ConfigPath
        Assert-OperationalConfig -Config $config
        Add-Result -Results $results -Name "ops-local" -Status "PASS" -Detail "Configuracao operacional local esta preenchida e sem placeholders."
    }
    catch {
        $hasFailure = $true
        Add-Result -Results $results -Name "ops-local" -Status "FAIL" -Detail $_.Exception.Message
    }
}
elseif ($RequireConfig) {
    $hasFailure = $true
    Add-Result -Results $results -Name "ops-local" -Status "FAIL" -Detail "Config obrigatoria nao encontrada em $configFullPath. Crie a partir de ops.example.json fora do Git."
}
else {
    Add-Result -Results $results -Name "ops-local" -Status "SKIP" -Detail "Use -RequireConfig para bloquear release sem ops.local.json preenchido."
}

Write-Report -Results $results

if ($hasFailure) {
    exit 1
}

Write-Host "Operational readiness validation completed."
