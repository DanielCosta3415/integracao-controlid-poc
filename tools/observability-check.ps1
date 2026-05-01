[CmdletBinding()]
param(
    [string]$BaseUrl = $env:OBSERVABILITY_BASE_URL,
    [string]$RulesPath = ".\docs\observability\alert-rules.json",
    [string]$DashboardPath = ".\docs\observability\dashboard.json",
    [string]$ReportPath = ".\artifacts\observability\observability-check-latest.md",
    [string]$MetricsCookie = $env:OBSERVABILITY_METRICS_COOKIE,
    [string]$MetricsBearerToken = $env:OBSERVABILITY_METRICS_BEARER_TOKEN,
    [switch]$RequireMetrics,
    [switch]$RequireHardwareContract,
    [switch]$OfflineValidateOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($BaseUrl)) {
    $BaseUrl = "http://localhost:5000"
}

function Resolve-RepoPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return $Path
    }

    return Join-Path $root ($Path -replace '^[.][\\/]', '')
}

function Read-JsonFile {
    param([Parameter(Mandatory = $true)][string]$Path)

    $fullPath = Resolve-RepoPath $Path
    if (-not (Test-Path -LiteralPath $fullPath)) {
        throw "Arquivo nao encontrado: $fullPath"
    }

    return Get-Content -Raw -LiteralPath $fullPath | ConvertFrom-Json
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

function Assert-AlertRules {
    param([Parameter(Mandatory = $true)]$Rules)

    if ($Rules.version -lt 1 -or -not $Rules.alerts -or $Rules.alerts.Count -eq 0) {
        throw "Alert rules precisam de version e pelo menos um alerta."
    }

    foreach ($alert in $Rules.alerts) {
        foreach ($property in @("id", "name", "source", "severity", "action")) {
            if (-not $alert.PSObject.Properties[$property] -or [string]::IsNullOrWhiteSpace([string]$alert.$property)) {
                throw "Alerta invalido: propriedade obrigatoria ausente '$property'."
            }
        }

        if ($alert.source -eq "metrics") {
            foreach ($property in @("metric", "operator", "threshold")) {
                if (-not $alert.PSObject.Properties[$property]) {
                    throw "Alerta metricas '$($alert.id)' sem '$property'."
                }
            }
        }
    }
}

function Assert-Dashboard {
    param([Parameter(Mandatory = $true)]$Dashboard)

    if ($Dashboard.version -lt 1 -or -not $Dashboard.dashboards -or $Dashboard.dashboards.Count -eq 0) {
        throw "Dashboard precisa de version e pelo menos um dashboard."
    }

    foreach ($dashboardItem in $Dashboard.dashboards) {
        foreach ($property in @("id", "title", "panels")) {
            if (-not $dashboardItem.PSObject.Properties[$property]) {
                throw "Dashboard invalido: propriedade obrigatoria ausente '$property'."
            }
        }
    }
}

function Join-Url {
    param(
        [Parameter(Mandatory = $true)][string]$Base,
        [Parameter(Mandatory = $true)][string]$Path
    )

    return $Base.TrimEnd("/") + "/" + $Path.TrimStart("/")
}

function Invoke-HealthProbe {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$ExpectedStatus
    )

    $uri = Join-Url -Base $BaseUrl -Path $Path
    $response = Invoke-RestMethod -Uri $uri -Method Get -TimeoutSec 10
    $status = [string]$response.status
    return [pscustomobject]@{
        Uri = $uri
        Passed = [string]::Equals($status, $ExpectedStatus, [StringComparison]::OrdinalIgnoreCase)
        Status = $status
    }
}

function Get-MetricsHeaders {
    $headers = @{}
    if (-not [string]::IsNullOrWhiteSpace($MetricsBearerToken)) {
        $headers["Authorization"] = "Bearer $MetricsBearerToken"
    }

    if (-not [string]::IsNullOrWhiteSpace($MetricsCookie)) {
        $headers["Cookie"] = $MetricsCookie
    }

    return $headers
}

function Read-Metrics {
    $uri = Join-Url -Base $BaseUrl -Path "/metrics"
    $headers = Get-MetricsHeaders
    return Invoke-WebRequest -Uri $uri -Method Get -Headers $headers -TimeoutSec 10
}

function Parse-Metrics {
    param([Parameter(Mandatory = $true)][string]$Text)

    $metrics = [System.Collections.Generic.List[object]]::new()
    foreach ($line in ($Text -split "`r?`n")) {
        $trimmed = $line.Trim()
        if ([string]::IsNullOrWhiteSpace($trimmed) -or $trimmed.StartsWith("#", [StringComparison]::Ordinal)) {
            continue
        }

        if ($trimmed -notmatch '^(?<name>[a-zA-Z_:][a-zA-Z0-9_:]*)(?<labels>\{[^}]*\})?\s+(?<value>[-+]?[0-9]*\.?[0-9]+)') {
            continue
        }

        $labels = @{}
        $labelText = $Matches.labels
        if (-not [string]::IsNullOrWhiteSpace($labelText)) {
            foreach ($labelMatch in [regex]::Matches($labelText.Trim("{}"), '([^=,]+)="((?:\\"|[^"])*)"')) {
                $labels[$labelMatch.Groups[1].Value] = $labelMatch.Groups[2].Value.Replace('\"', '"')
            }
        }

        $metrics.Add([pscustomobject]@{
            Name = $Matches.name
            Labels = $labels
            Value = [double]::Parse($Matches.value, [System.Globalization.CultureInfo]::InvariantCulture)
        })
    }

    return $metrics
}

function Test-LabelFilters {
    param(
        [Parameter(Mandatory = $true)]$Metric,
        [Parameter(Mandatory = $true)]$Filters
    )

    if ($null -eq $Filters) {
        return $true
    }

    foreach ($filter in $Filters.PSObject.Properties) {
        if (-not $Metric.Labels.ContainsKey($filter.Name)) {
            return $false
        }

        if (-not [string]::Equals([string]$Metric.Labels[$filter.Name], [string]$filter.Value, [StringComparison]::OrdinalIgnoreCase)) {
            return $false
        }
    }

    return $true
}

function Test-Threshold {
    param(
        [double]$Actual,
        [string]$Operator,
        [double]$Threshold
    )

    switch ($Operator) {
        ">" { return $Actual -gt $Threshold }
        ">=" { return $Actual -ge $Threshold }
        "<" { return $Actual -lt $Threshold }
        "<=" { return $Actual -le $Threshold }
        "==" { return [double]::Equals($Actual, $Threshold) }
        default { throw "Operador de threshold nao suportado: $Operator" }
    }
}

function Write-Report {
    param(
        [System.Collections.Generic.List[object]]$Results
    )

    $fullPath = Resolve-RepoPath $ReportPath
    $directory = Split-Path -Parent $fullPath
    if (-not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add("# Observability check")
    $lines.Add("")
    $lines.Add("Data: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss zzz")")
    $lines.Add("BaseUrl: $BaseUrl")
    $lines.Add("")
    foreach ($result in $Results) {
        $lines.Add("- [$($result.Status)] $($result.Name): $($result.Detail)")
    }

    [System.IO.File]::WriteAllLines($fullPath, $lines, [System.Text.UTF8Encoding]::new($true))
    Write-Host "Relatorio: $fullPath"
}

$rules = Read-JsonFile $RulesPath
$dashboard = Read-JsonFile $DashboardPath
Assert-AlertRules $rules
Assert-Dashboard $dashboard

$results = [System.Collections.Generic.List[object]]::new()
Add-Result -Results $results -Name "alert-rules" -Status "PASS" -Detail "Regras versionadas validas: $($rules.alerts.Count)."
Add-Result -Results $results -Name "dashboard-spec" -Status "PASS" -Detail "Dashboards versionados validos: $($dashboard.dashboards.Count)."

if ($OfflineValidateOnly) {
    Write-Report -Results $results
    Write-Host "Observability offline validation completed."
    exit 0
}

$hasFailure = $false
foreach ($alert in ($rules.alerts | Where-Object { $_.source -eq "health" })) {
    try {
        $probe = Invoke-HealthProbe -Path $alert.target -ExpectedStatus $alert.expectedStatus
        if ($probe.Passed) {
            Add-Result -Results $results -Name $alert.id -Status "PASS" -Detail "$($alert.target) retornou $($probe.Status)."
        }
        else {
            $hasFailure = $true
            Add-Result -Results $results -Name $alert.id -Status "FAIL" -Detail "$($alert.target) retornou $($probe.Status), esperado $($alert.expectedStatus). Acao: $($alert.action)"
        }
    }
    catch {
        $hasFailure = $true
        Add-Result -Results $results -Name $alert.id -Status "FAIL" -Detail "Probe de health falhou: $($_.Exception.Message)"
    }
}

$metricsText = ""
$parsedMetrics = @()
try {
    $metricsResponse = Read-Metrics
    $metricsText = [string]$metricsResponse.Content
    $parsedMetrics = Parse-Metrics -Text $metricsText
    Add-Result -Results $results -Name "metrics-endpoint" -Status "PASS" -Detail "Endpoint /metrics respondeu com $($parsedMetrics.Count) series."
}
catch {
    if ($RequireMetrics) {
        $hasFailure = $true
        Add-Result -Results $results -Name "metrics-endpoint" -Status "FAIL" -Detail "Endpoint /metrics indisponivel ou sem autorizacao: $($_.Exception.Message)"
    }
    else {
        Add-Result -Results $results -Name "metrics-endpoint" -Status "SKIP" -Detail "Endpoint /metrics nao acessivel sem credencial; use OBSERVABILITY_METRICS_COOKIE ou OBSERVABILITY_METRICS_BEARER_TOKEN."
    }
}

if ($parsedMetrics.Count -gt 0) {
    foreach ($alert in ($rules.alerts | Where-Object { $_.source -eq "metrics" })) {
        $series = $parsedMetrics | Where-Object {
            $_.Name -eq $alert.metric -and (Test-LabelFilters -Metric $_ -Filters $alert.labelFilters)
        }
        $measure = $series | Measure-Object -Property Value -Sum
        $actual = if ($null -eq $measure.Sum) { 0.0 } else { [double]$measure.Sum }
        $triggered = Test-Threshold -Actual $actual -Operator $alert.operator -Threshold ([double]$alert.threshold)

        if ($triggered) {
            $hasFailure = $true
            Add-Result -Results $results -Name $alert.id -Status "FAIL" -Detail "$($alert.name): valor $actual $($alert.operator) $($alert.threshold). Acao: $($alert.action)"
        }
        else {
            Add-Result -Results $results -Name $alert.id -Status "PASS" -Detail "$($alert.name): valor $actual dentro do threshold."
        }
    }
}

if ($RequireHardwareContract) {
    try {
        powershell -ExecutionPolicy Bypass -File (Join-Path $root "tools\contract-controlid-device.ps1")
        if ($LASTEXITCODE -ne 0) {
            throw "contract-controlid-device.ps1 exited with code $LASTEXITCODE."
        }

        Add-Result -Results $results -Name "hardware-contract" -Status "PASS" -Detail "Contrato fisico executado sem falha."
    }
    catch {
        $hasFailure = $true
        Add-Result -Results $results -Name "hardware-contract" -Status "FAIL" -Detail "Contrato fisico falhou ou nao foi configurado: $($_.Exception.Message)"
    }
}
else {
    Add-Result -Results $results -Name "hardware-contract" -Status "SKIP" -Detail "Use -RequireHardwareContract para bloquear release sem equipamento real."
}

Write-Report -Results $results

if ($hasFailure) {
    exit 1
}

Write-Host "Observability check completed."
