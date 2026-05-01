[CmdletBinding()]
param(
    [switch]$RunCoverage,
    [switch]$RunSupplyChainAudit,
    [switch]$RunSmoke,
    [switch]$RunContainerBuild,
    [switch]$RunExternalScanners,
    [switch]$RunObservabilityOnline,
    [switch]$RequireObservabilityMetrics,
    [switch]$RequireOperationalConfig,
    [switch]$RequireHardwareContract,
    [switch]$RequireExternalScanners,
    [switch]$ReleaseGate,
    [string]$ObservabilityBaseUrl = $env:OBSERVABILITY_BASE_URL,
    [string]$ExternalScanBaseUrl = $env:EXTERNAL_SCAN_BASE_URL
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$artifactsRoot = Join-Path $root "artifacts\test-readiness"

if ($ReleaseGate) {
    $RunCoverage = $true
    $RunSupplyChainAudit = $true
    $RunSmoke = $true
    $RunContainerBuild = $true
    $RunExternalScanners = $true
    $RunObservabilityOnline = $true
    $RequireObservabilityMetrics = $true
    $RequireOperationalConfig = $true
    $RequireHardwareContract = $true
    $RequireExternalScanners = $true
}

if (-not (Test-Path $artifactsRoot)) {
    New-Item -ItemType Directory -Force -Path $artifactsRoot | Out-Null
}

function Invoke-Step {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][scriptblock]$Script
    )

    Write-Host "== $Name =="
    & $Script
    if ($LASTEXITCODE -ne 0) {
        throw "$Name failed with exit code $LASTEXITCODE."
    }
}

function Test-CommandAvailable {
    param([Parameter(Mandatory = $true)][string]$CommandName)

    return $null -ne (Get-Command $CommandName -ErrorAction SilentlyContinue)
}

Push-Location $root
try {
    Invoke-Step "build" {
        dotnet build ".\Integracao.ControlID.PoC.sln" --no-restore -v:minimal
    }

    Invoke-Step "tests" {
        dotnet test ".\Integracao.ControlID.PoC.sln" --no-build -v:minimal
    }

    Invoke-Step "format-check" {
        dotnet format ".\Integracao.ControlID.PoC.sln" --verify-no-changes --no-restore -v:minimal
    }

    Invoke-Step "whitespace-check" {
        git diff --check
    }

    Invoke-Step "secret-scan" {
        powershell -ExecutionPolicy Bypass -File ".\tools\scan-secrets.ps1"
    }

    Invoke-Step "observability-offline" {
        powershell -ExecutionPolicy Bypass -File ".\tools\observability-check.ps1" -OfflineValidateOnly
    }

    Invoke-Step "operational-readiness" {
        $arguments = @(
            "-ExecutionPolicy", "Bypass",
            "-File", ".\tools\operational-readiness-check.ps1"
        )

        if ($RequireOperationalConfig) {
            $arguments += "-RequireConfig"
        }

        powershell @arguments
    }

    Invoke-Step "simulated-device-contract" {
        powershell -ExecutionPolicy Bypass -File ".\tools\contract-controlid-stub.ps1"
    }

    if ($RunObservabilityOnline -or $RequireObservabilityMetrics) {
        Invoke-Step "observability-online" {
            $arguments = @(
                "-ExecutionPolicy", "Bypass",
                "-File", ".\tools\observability-check.ps1"
            )

            if (-not [string]::IsNullOrWhiteSpace($ObservabilityBaseUrl)) {
                $arguments += @("-BaseUrl", $ObservabilityBaseUrl)
            }

            if ($RequireObservabilityMetrics) {
                $arguments += "-RequireMetrics"
            }

            powershell @arguments
        }
    }

    if ($RunCoverage) {
        $coverageDir = Join-Path $artifactsRoot "coverage"
        if (-not (Test-Path $coverageDir)) {
            New-Item -ItemType Directory -Force -Path $coverageDir | Out-Null
        }

        Invoke-Step "coverage-collector" {
            dotnet test ".\Integracao.ControlID.PoC.sln" --no-build --collect "Code Coverage" --results-directory $coverageDir -v:minimal
        }

        $coverageFiles = Get-ChildItem -Path $coverageDir -Recurse -File -Include "*.coverage", "*.xml", "*.cobertura.xml" -ErrorAction SilentlyContinue
        if (-not $coverageFiles) {
            throw "Coverage collector completed but no coverage artifact was produced under $coverageDir."
        }

        Write-Host "Coverage artifacts:"
        $coverageFiles | ForEach-Object { Write-Host " - $($_.FullName)" }
    }

    if ($RunSupplyChainAudit) {
        Invoke-Step "supply-chain-audit" {
            powershell -ExecutionPolicy Bypass -File ".\tools\audit-supply-chain.ps1"
        }
    }

    if ($RunSmoke) {
        Invoke-Step "localhost-smoke" {
            powershell -ExecutionPolicy Bypass -File ".\tools\smoke-localhost.ps1" -ReportPath ".\artifacts\smoke\localhost-smoke-readiness.md"
        }
    }

    if ($RunContainerBuild) {
        if (-not (Test-CommandAvailable "docker")) {
            throw "Docker is required for -RunContainerBuild."
        }

        Invoke-Step "container-build" {
            docker build --pull -t "integracao-controlid-poc:readiness" "."
        }
    }

    if ($RunExternalScanners -or $RequireExternalScanners) {
        Invoke-Step "external-security-scans" {
            $arguments = @(
                "-ExecutionPolicy", "Bypass",
                "-File", ".\tools\external-security-scans.ps1"
            )

            if (-not [string]::IsNullOrWhiteSpace($ExternalScanBaseUrl)) {
                $arguments += @("-BaseUrl", $ExternalScanBaseUrl)
            }

            if ($RequireExternalScanners) {
                $arguments += "-RequireTools"
            }

            powershell @arguments
        }
    }
    else {
        Write-Host "INFO external-security-scans: not requested. Use -RunExternalScanners or -RequireExternalScanners."
    }

    if ($RequireHardwareContract) {
        Invoke-Step "physical-device-contract" {
            powershell -ExecutionPolicy Bypass -File ".\tools\contract-controlid-device.ps1"
        }
    }
    elseif ($env:CONTROLID_DEVICE_URL -and $env:CONTROLID_USERNAME -and $env:CONTROLID_PASSWORD) {
        Invoke-Step "physical-device-contract" {
            powershell -ExecutionPolicy Bypass -File ".\tools\contract-controlid-device.ps1"
        }
    }
    else {
        Write-Host "INFO physical-device-contract: real hardware not configured; simulated-device-contract already ran. Use -RequireHardwareContract for lab release."
    }

    Write-Host "Test readiness gates completed."
}
finally {
    Pop-Location
}
