[CmdletBinding()]
param(
    [string]$DeviceUrl = $env:CONTROLID_DEVICE_URL,
    [string]$Username = $env:CONTROLID_USERNAME,
    [string]$Password = $env:CONTROLID_PASSWORD,
    [int]$TimeoutSec = 10,
    [string]$ReportPath = ".\artifacts\reports\controlid-device-contract-latest.md"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptRoot = if (-not [string]::IsNullOrWhiteSpace($PSScriptRoot)) {
    $PSScriptRoot
}
else {
    Split-Path -Parent $MyInvocation.MyCommand.Path
}
$root = Split-Path -Parent $scriptRoot
$results = [System.Collections.Generic.List[object]]::new()
$deviceBaseUri = $null

function Add-Result {
    param(
        [string]$Name,
        [string]$Status,
        [string]$Detail
    )

    $results.Add([pscustomobject]@{
        Name = $Name
        Status = $Status
        Detail = $Detail
    })
}

function Assert-Configured {
    if ([string]::IsNullOrWhiteSpace($DeviceUrl) -or
        [string]::IsNullOrWhiteSpace($Username) -or
        [string]::IsNullOrWhiteSpace($Password)) {
        Write-Error "Configure CONTROLID_DEVICE_URL, CONTROLID_USERNAME e CONTROLID_PASSWORD para validar contrato contra equipamento real."
        exit 2
    }
}

function Get-ControlIdBaseUri {
    param([string]$RawUrl)

    $uri = [Uri]($RawUrl.Trim())
    if (-not $uri.IsAbsoluteUri -or ($uri.Scheme -ne "http" -and $uri.Scheme -ne "https")) {
        throw "CONTROLID_DEVICE_URL deve ser uma URL absoluta http/https."
    }

    if (-not [string]::IsNullOrWhiteSpace($uri.UserInfo) -or
        -not [string]::IsNullOrWhiteSpace($uri.Query) -or
        -not [string]::IsNullOrWhiteSpace($uri.Fragment)) {
        throw "CONTROLID_DEVICE_URL nao deve conter credenciais, query string ou fragmento."
    }

    return [Uri]($uri.GetLeftPart([UriPartial]::Authority).TrimEnd("/") + "/")
}

function Resolve-DeviceUri {
    param([string]$Path)

    return ([Uri]::new($deviceBaseUri, $Path.TrimStart("/"))).AbsoluteUri
}

function Invoke-ControlIdPost {
    param(
        [string]$Path,
        [object]$Body = $null,
        [string]$Session = ""
    )

    $uri = Resolve-DeviceUri $Path
    if (-not [string]::IsNullOrWhiteSpace($Session)) {
        $builder = [UriBuilder]$uri
        $builder.Query = "session=$([Uri]::EscapeDataString($Session))"
        $uri = $builder.Uri.AbsoluteUri
    }

    if ($null -eq $Body) {
        return Invoke-RestMethod -Uri $uri -Method Post -TimeoutSec $TimeoutSec
    }

    return Invoke-RestMethod -Uri $uri -Method Post -Body ($Body | ConvertTo-Json -Depth 10) -ContentType "application/json" -TimeoutSec $TimeoutSec
}

function Write-Report {
    $reportFullPath = Join-Path $root ($ReportPath -replace '^[.][\\/]', '')
    $reportDir = Split-Path -Parent $reportFullPath
    if (-not (Test-Path $reportDir)) {
        New-Item -ItemType Directory -Force -Path $reportDir | Out-Null
    }

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add("# Control iD device contract check")
    $lines.Add("")
    $lines.Add("Data: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss zzz")")
    $lines.Add("DeviceUrl: configurado; host omitido por seguranca.")
    $lines.Add("")
    foreach ($result in $results) {
        $lines.Add("- [$($result.Status)] $($result.Name): $($result.Detail)")
    }

    [IO.File]::WriteAllLines($reportFullPath, $lines, [Text.UTF8Encoding]::new($true))
    Write-Host "Relatorio: $reportFullPath"
}

Assert-Configured
$deviceBaseUri = Get-ControlIdBaseUri -RawUrl $DeviceUrl

$session = ""
try {
    $login = Invoke-ControlIdPost -Path "/login.fcgi" -Body @{ login = $Username; password = $Password }
    $session = [string]$login.session
    if ([string]::IsNullOrWhiteSpace($session)) {
        Add-Result "login.fcgi" "FAIL" "Resposta sem campo session."
        Write-Report
        exit 1
    }

    Add-Result "login.fcgi" "PASS" "Sessao retornada sem expor credencial."

    Invoke-ControlIdPost -Path "/session_is_valid.fcgi" -Session $session | Out-Null
    Add-Result "session_is_valid.fcgi" "PASS" "Contrato respondeu sem expor session."

    $systemInformation = Invoke-ControlIdPost -Path "/system_information.fcgi"
    Add-Result "system_information.fcgi" "PASS" "Contrato respondeu com propriedades: $((($systemInformation | Get-Member -MemberType NoteProperty).Name) -join ', ')"

    Write-Report
}
catch {
    Add-Result "contract-check" "FAIL" $_.Exception.Message
    Write-Report
    exit 1
}
finally {
    if (-not [string]::IsNullOrWhiteSpace($session)) {
        try {
            Invoke-ControlIdPost -Path "/logout.fcgi" -Session $session | Out-Null
            Write-Host "Logout executado."
        }
        catch {
            Write-Warning "Nao foi possivel executar logout da sessao de contrato: $($_.Exception.Message)"
        }
    }
}
