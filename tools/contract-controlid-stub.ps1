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
    if ($LASTEXITCODE -ne 0) {
        throw "O contrato principal com o simulador falhou."
    }

    $managementUrl = $StubUrl.TrimEnd("/") + "/__stub"
    $catalog = Invoke-RestMethod -Uri "$managementUrl/catalog" -Method Get -TimeoutSec $TimeoutSec
    $requiredScenarios = @("normal", "slow", "bad-request", "unauthorized", "rate-limited", "invalid-json", "oversized-response", "session-expired", "network-drop")
    foreach ($scenario in $requiredScenarios) {
        if ($catalog.scenarios -notcontains $scenario) {
            throw "Cenario obrigatorio ausente no simulador: $scenario"
        }
    }

    $resetBody = @{ profile = "idface"; datasetSize = 1000 } | ConvertTo-Json -Compress
    $reset = Invoke-RestMethod -Uri "$managementUrl/reset" -Method Post -ContentType "application/json" -Body $resetBody -TimeoutSec $TimeoutSec
    if ($reset.dataset_size -ne 1000 -or $reset.profile.name -ne "idface") {
        throw "O simulador nao aplicou perfil e massa deterministica."
    }

    $scenarioBody = @{ name = "bad-request"; endpoint = "/system_information.fcgi" } | ConvertTo-Json -Compress
    Invoke-RestMethod -Uri "$managementUrl/scenario" -Method Post -ContentType "application/json" -Body $scenarioBody -TimeoutSec $TimeoutSec | Out-Null
    try {
        Invoke-WebRequest -Uri "$StubUrl/system_information.fcgi" -Method Get -UseBasicParsing -TimeoutSec $TimeoutSec | Out-Null
        throw "O cenario bad-request nao alterou o status HTTP."
    }
    catch {
        $statusCode = [int]$_.Exception.Response.StatusCode
        if ($statusCode -ne 400) {
            throw
        }
    }

    $slowBody = @{ name = "slow"; endpoint = "/system_information.fcgi"; delayMs = 100 } | ConvertTo-Json -Compress
    Invoke-RestMethod -Uri "$managementUrl/scenario" -Method Post -ContentType "application/json" -Body $slowBody -TimeoutSec $TimeoutSec | Out-Null
    $elapsed = Measure-Command {
        Invoke-WebRequest -Uri "$StubUrl/system_information.fcgi" -Method Get -UseBasicParsing -TimeoutSec $TimeoutSec | Out-Null
    }
    if ($elapsed.TotalMilliseconds -lt 75) {
        throw "O cenario slow nao aplicou a latencia configurada."
    }

    Invoke-RestMethod -Uri "$managementUrl/reset" -Method Post -ContentType "application/json" -Body (@{ profile = "legacy"; datasetSize = 1 } | ConvertTo-Json -Compress) -TimeoutSec $TimeoutSec | Out-Null
    $legacyInfo = Invoke-RestMethod -Uri "$StubUrl/system_information.fcgi" -Method Get -TimeoutSec $TimeoutSec
    if ([string]$legacyInfo.product_name -notmatch "Legacy") {
        throw "O perfil legacy nao foi refletido nas informacoes do sistema."
    }

    Invoke-RestMethod -Uri "$managementUrl/reset" -Method Post -ContentType "application/json" -Body "{}" -TimeoutSec $TimeoutSec | Out-Null
    Invoke-WebRequest -Uri "$StubUrl/system_information.fcgi" -Method Get -UseBasicParsing -TimeoutSec $TimeoutSec | Out-Null
    $status = Invoke-RestMethod -Uri "$managementUrl/status" -Method Get -TimeoutSec $TimeoutSec
    if ($status.scenario.name -ne "normal" -or @($status.requests.PSObject.Properties).Count -lt 1) {
        throw "O estado final do simulador e inconsistente."
    }

    Add-Content -LiteralPath $ReportPath -Encoding UTF8 -Value @"

## Motor deterministico do simulador

- Catalogo de cenarios obrigatorios: aprovado.
- Massa sintetica de 1.000 usuarios: aprovada.
- Perfil `idface`: aprovado.
- Perfil `legacy`: aprovado.
- Falha HTTP 400 direcionada: aprovada.
- Latencia configuravel: aprovada.
- Restauracao para o estado normal: aprovada.
"@
}
finally {
    if ($null -ne $startedProcess -and -not $startedProcess.HasExited) {
        Stop-Process -Id $startedProcess.Id -Force
    }

    Pop-Location
}
