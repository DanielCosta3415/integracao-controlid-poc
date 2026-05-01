[CmdletBinding()]
param(
    [string]$ReportPath = ".\artifacts\finops-capacity\finops-capacity-latest.md",
    [int]$MaxSqliteMegabytes = 512,
    [int]$MaxLogsMegabytes = 256,
    [int]$MaxArtifactsMegabytes = 1024,
    [int]$MaxReportsMegabytes = 128,
    [switch]$FailOnWarnings
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$results = [System.Collections.Generic.List[object]]::new()
$script:HasFailure = $false
$script:HasWarning = $false

function Resolve-RepoPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    if ([IO.Path]::IsPathRooted($Path)) {
        return [IO.Path]::GetFullPath($Path)
    }

    return [IO.Path]::GetFullPath((Join-Path -Path $root -ChildPath ($Path -replace '^[.][\\/]', '')))
}

function Add-Result {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][ValidateSet("PASS", "WARN", "FAIL")][string]$Status,
        [Parameter(Mandatory = $true)][string]$Detail
    )

    $results.Add([pscustomobject]@{
        Name = $Name
        Status = $Status
        Detail = $Detail
    })

    if ($Status -eq "FAIL") {
        $script:HasFailure = $true
    }

    if ($Status -eq "WARN") {
        $script:HasWarning = $true
    }
}

function Convert-ToMegabytes {
    param([int64]$Bytes)

    return [math]::Round($Bytes / 1MB, 2)
}

function Get-DirectorySizeBytes {
    param([Parameter(Mandatory = $true)][string]$Path)

    $fullPath = Resolve-RepoPath -Path $Path
    if (-not (Test-Path -LiteralPath $fullPath)) {
        return 0
    }

    $items = @(Get-ChildItem -LiteralPath $fullPath -Recurse -File -Force -ErrorAction SilentlyContinue)
    if ($items.Count -eq 0) {
        return 0
    }

    $sum = ($items | Measure-Object -Property Length -Sum).Sum
    if ($null -eq $sum) {
        return 0
    }

    return [int64]$sum
}

function Get-RootFileSetSizeBytes {
    param([Parameter(Mandatory = $true)][string]$Filter)

    $items = @(Get-ChildItem -LiteralPath $root -File -Filter $Filter -Force -ErrorAction SilentlyContinue)
    if ($items.Count -eq 0) {
        return 0
    }

    $sum = ($items | Measure-Object -Property Length -Sum).Sum
    if ($null -eq $sum) {
        return 0
    }

    return [int64]$sum
}

function Add-SizeResult {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][int64]$Bytes,
        [Parameter(Mandatory = $true)][int]$MaxMegabytes,
        [Parameter(Mandatory = $true)][string]$Scope
    )

    $megabytes = Convert-ToMegabytes -Bytes $Bytes
    if ($megabytes -gt $MaxMegabytes) {
        Add-Result -Name $Name -Status "WARN" -Detail "$Scope usa $megabytes MB, acima do limite recomendado de $MaxMegabytes MB. Nao foi removido nenhum dado."
    }
    else {
        Add-Result -Name $Name -Status "PASS" -Detail "$Scope usa $megabytes MB de $MaxMegabytes MB recomendados."
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

try {
    Assert-TextContains -Path "docs\finops-capacity.md" -Patterns @(
        "Inventario de custos",
        "Riscos de capacidade",
        "Governanca FinOps",
        "Alertas e limites sugeridos"
    )
    Add-Result -Name "finops-runbook" -Status "PASS" -Detail "Runbook FinOps/capacidade cobre custos, capacidade, governanca e limites."
}
catch {
    Add-Result -Name "finops-runbook" -Status "FAIL" -Detail $_.Exception.Message
}

try {
    Assert-TextContains -Path "ops.example.json" -Patterns @(
        '"finops"',
        '"costOwner"',
        '"monthlyBudget"',
        '"billingDashboard"',
        '"actualSpendReviewSource"',
        '"billingAlertOwner"',
        '"capacityReviewCadence"'
    )
    Add-Result -Name "finops-ops-example" -Status "PASS" -Detail "ops.example.json inclui ownership, budget, alertas e revisao de capacidade."
}
catch {
    Add-Result -Name "finops-ops-example" -Status "FAIL" -Detail $_.Exception.Message
}

try {
    Assert-TextContains -Path "docs\observability\alert-rules.json" -Patterns @(
        '"FIN-001"',
        '"FIN-002"',
        '"FIN-003"'
    )
    Add-Result -Name "finops-alerts" -Status "PASS" -Detail "Alertas de custo/capacidade estao versionados."
}
catch {
    Add-Result -Name "finops-alerts" -Status "FAIL" -Detail $_.Exception.Message
}

try {
    Assert-TextContains -Path "docs\observability\dashboard.json" -Patterns @(
        '"finops-capacity"',
        '"Uso de armazenamento local"',
        '"Custo de chamadas e retentativas"'
    )
    Add-Result -Name "finops-dashboard" -Status "PASS" -Detail "Dashboard sugerido possui paineis de storage, logs, retentativas e gates."
}
catch {
    Add-Result -Name "finops-dashboard" -Status "FAIL" -Detail $_.Exception.Message
}

try {
    Assert-TextContains -Path "appsettings.json" -Patterns @(
        '"retainedFileCountLimit"',
        '"fileSizeLimitBytes"',
        '"RateLimit"',
        '"CircuitBreaker"'
    )
    Add-Result -Name "bounded-runtime-config" -Status "PASS" -Detail "Configuracao possui rotacao de logs, rate limit e circuit breaker."
}
catch {
    Add-Result -Name "bounded-runtime-config" -Status "FAIL" -Detail $_.Exception.Message
}

try {
    Assert-TextContains -Path "docker-compose.yml" -Patterns @(
        "controlid-data:/app/data",
        "controlid-logs:/app/Logs",
        "Serilog__WriteTo__1__Args__retainedFileCountLimit",
        "Serilog__WriteTo__1__Args__fileSizeLimitBytes"
    )
    Add-Result -Name "container-cost-controls" -Status "PASS" -Detail "Compose mantem volumes persistentes e limites de log configuraveis por ambiente."
}
catch {
    Add-Result -Name "container-cost-controls" -Status "FAIL" -Detail $_.Exception.Message
}

try {
    Assert-TextContains -Path "Services\Database\LocalDataQueryLimits.cs" -Patterns @(
        "DefaultListLimit",
        "MaxListLimit",
        "NormalizeRetentionDays"
    )
    Add-Result -Name "query-and-retention-limits" -Status "PASS" -Detail "Listagens locais e retencao possuem limites centralizados."
}
catch {
    Add-Result -Name "query-and-retention-limits" -Status "FAIL" -Detail $_.Exception.Message
}

try {
    Assert-TextContains -Path "tools\backup-sqlite-operational.ps1" -Patterns @(
        "RetentionDays",
        "ApplyRetention",
        "RetentionConfirmation",
        "RunRestoreSmoke"
    )
    Add-Result -Name "backup-retention-controls" -Status "PASS" -Detail "Backup operacional possui dry-run de retencao e confirmacao textual para remocao."
}
catch {
    Add-Result -Name "backup-retention-controls" -Status "FAIL" -Detail $_.Exception.Message
}

Add-SizeResult -Name "sqlite-runtime-size" -Bytes (Get-RootFileSetSizeBytes -Filter "integracao_controlid.db*") -MaxMegabytes $MaxSqliteMegabytes -Scope "SQLite local"
Add-SizeResult -Name "logs-size" -Bytes (Get-DirectorySizeBytes -Path "Logs") -MaxMegabytes $MaxLogsMegabytes -Scope "Logs locais"
Add-SizeResult -Name "artifacts-size" -Bytes (Get-DirectorySizeBytes -Path "artifacts") -MaxMegabytes $MaxArtifactsMegabytes -Scope "Artifacts locais"
Add-SizeResult -Name "versioned-reports-size" -Bytes (Get-DirectorySizeBytes -Path "docs\reports") -MaxMegabytes $MaxReportsMegabytes -Scope "Relatorios versionados"

$reportFullPath = Resolve-RepoPath -Path $ReportPath
$reportDirectory = Split-Path -Parent $reportFullPath
if (-not (Test-Path -LiteralPath $reportDirectory)) {
    New-Item -ItemType Directory -Force -Path $reportDirectory | Out-Null
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# FinOps capacity check")
$lines.Add("")
$lines.Add("Data: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss zzz")")
$lines.Add("FailOnWarnings: $FailOnWarnings")
$lines.Add("")
$lines.Add("| Check | Status | Detail |")
$lines.Add("| --- | --- | --- |")

foreach ($result in $results) {
    $detail = ([string]$result.Detail).Replace("|", "\|")
    $lines.Add("| $($result.Name) | $($result.Status) | $detail |")
}

[IO.File]::WriteAllLines($reportFullPath, $lines, [Text.UTF8Encoding]::new($true))
Write-Host "Relatorio: $reportFullPath"

if ($script:HasFailure -or ($FailOnWarnings -and $script:HasWarning)) {
    exit 1
}

Write-Host "FinOps capacity validation completed."
