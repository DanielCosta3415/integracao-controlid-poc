[CmdletBinding()]
param(
    [string]$StubUrl = "http://127.0.0.1:6600",
    [string]$ReportPath = ".\artifacts\reports\controlid-stub-contract-latest.md",
    [int]$TimeoutSec = 20
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$stubProject = Join-Path $root "tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj"
$artifactsDir = Join-Path $root "artifacts\device-contract"
$startedProcess = $null

function Test-StubReady {
    param([Parameter(Mandatory = $true)][string]$BaseUrl)

    try {
        Invoke-WebRequest -Uri ($BaseUrl.TrimEnd("/") + "/system_information.fcgi") -Method Get -TimeoutSec 2 -UseBasicParsing | Out-Null
        return $true
    }
    catch {
        return $false
    }
}

function Wait-StubReady {
    param(
        [Parameter(Mandatory = $true)][string]$BaseUrl,
        [Parameter(Mandatory = $true)][int]$TimeoutSeconds
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        if (Test-StubReady -BaseUrl $BaseUrl) {
            return
        }

        Start-Sleep -Milliseconds 500
    }

    throw "Stub Control iD nao respondeu em $TimeoutSeconds segundos."
}

function Start-HiddenDotnetProcess {
    param(
        [Parameter(Mandatory = $true)][string]$Arguments,
        [Parameter(Mandatory = $true)][string]$WorkingDirectory
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = (Get-Command dotnet).Source
    $startInfo.Arguments = $Arguments
    $startInfo.WorkingDirectory = $WorkingDirectory
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Hidden
    return [System.Diagnostics.Process]::Start($startInfo)
}

Push-Location $root
try {
    if (-not (Test-Path $artifactsDir)) {
        New-Item -ItemType Directory -Force -Path $artifactsDir | Out-Null
    }

    dotnet build $stubProject --no-restore -v:minimal
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    $alreadyRunning = Test-StubReady -BaseUrl $StubUrl
    if (-not $alreadyRunning) {
        $arguments = "run --project `"$stubProject`" --no-build --no-launch-profile"
        $startedProcess = Start-HiddenDotnetProcess -Arguments $arguments -WorkingDirectory $root
        Wait-StubReady -BaseUrl $StubUrl -TimeoutSeconds $TimeoutSec
    }

    powershell -ExecutionPolicy Bypass -File ".\tools\contract-controlid-device.ps1" `
        -DeviceUrl $StubUrl `
        -Username "stub-admin" `
        -Password "stub-password" `
        -ReportPath $ReportPath
}
finally {
    if ($null -ne $startedProcess -and -not $startedProcess.HasExited) {
        Stop-Process -Id $startedProcess.Id -Force
    }

    Pop-Location
}
