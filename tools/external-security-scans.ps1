[CmdletBinding()]
param(
    [string]$BaseUrl = $env:EXTERNAL_SCAN_BASE_URL,
    [string]$ReportPath = ".\artifacts\external-scans\external-security-scans-latest.md",
    [switch]$InventoryOnly,
    [switch]$RequireTools,
    [switch]$SkipDast,
    [switch]$SkipAccessibility
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$results = [System.Collections.Generic.List[object]]::new()
$hasFailure = $false
$resolvedCommands = @{}

function Resolve-RepoPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    if ([IO.Path]::IsPathRooted($Path)) {
        return $Path
    }

    return Join-Path $root ($Path -replace '^[.][\\/]', '')
}

function Add-Result {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Status,
        [Parameter(Mandatory = $true)][string]$Detail,
        [string]$Artifact = ""
    )

    $results.Add([pscustomobject]@{
        Name = $Name
        Status = $Status
        Detail = $Detail
        Artifact = $Artifact
    })
}

function Test-CommandAvailable {
    param([Parameter(Mandatory = $true)][string]$CommandName)

    return -not [string]::IsNullOrWhiteSpace((Resolve-ExternalCommand -CommandName $CommandName))
}

function Resolve-FirstExistingPath {
    param([Parameter(Mandatory = $true)][string[]]$Candidates)

    foreach ($candidate in $Candidates) {
        if (-not [string]::IsNullOrWhiteSpace($candidate) -and (Test-Path -LiteralPath $candidate -PathType Leaf)) {
            return $candidate
        }
    }

    return ""
}

function Resolve-ExternalCommand {
    param([Parameter(Mandatory = $true)][string]$CommandName)

    if ($resolvedCommands.ContainsKey($CommandName)) {
        return $resolvedCommands[$CommandName]
    }

    $command = Get-Command $CommandName -ErrorAction SilentlyContinue
    if ($command) {
        $resolvedCommands[$CommandName] = $command.Source
        return $command.Source
    }

    $candidate = switch ($CommandName) {
        "semgrep" {
            Resolve-FirstExistingPath -Candidates @(
                (Join-Path $env:APPDATA "Python\Python312\Scripts\semgrep.exe"),
                (Join-Path $env:APPDATA "Python\Python313\Scripts\semgrep.exe")
            )
        }
        "osv-scanner" {
            $wingetRoot = Join-Path $env:LOCALAPPDATA "Microsoft\WinGet\Packages"
            $match = Get-ChildItem -Path $wingetRoot -Recurse -Filter "osv-scanner.exe" -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($match) { $match.FullName } else { "" }
        }
        "zap-baseline.py" {
            Resolve-FirstExistingPath -Candidates @(
                "C:\Program Files\ZAP\Zed Attack Proxy\zap-baseline.py",
                "C:\Program Files\ZAP\Zed Attack Proxy\zap.bat",
                "C:\Program Files (x86)\ZAP\Zed Attack Proxy\zap.bat"
            )
        }
        "axe" {
            Resolve-FirstExistingPath -Candidates @(
                (Join-Path $env:APPDATA "npm\axe.cmd"),
                (Join-Path $env:APPDATA "npm\axe.ps1")
            )
        }
        default {
            ""
        }
    }

    $resolvedCommands[$CommandName] = $candidate
    return $candidate
}

function Resolve-BrowserDriverManagerPath {
    param([Parameter(Mandatory = $true)][string]$Kind)

    $base = Join-Path $env:USERPROFILE ".browser-driver-manager"
    if (-not (Test-Path -LiteralPath $base)) {
        return ""
    }

    $filter = if ($Kind -eq "chrome") { "chrome.exe" } else { "chromedriver.exe" }
    $match = Get-ChildItem -Path (Join-Path $base $Kind) -Recurse -Filter $filter -ErrorAction SilentlyContinue |
        Sort-Object FullName -Descending |
        Select-Object -First 1

    if ($match) {
        return $match.FullName
    }

    return ""
}

function Write-TextArtifact {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)]$Content
    )

    $fullPath = Resolve-RepoPath $Path
    $directory = Split-Path -Parent $fullPath
    if (-not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $lines = @($Content | ForEach-Object { [string]$_ })
    [IO.File]::WriteAllLines($fullPath, $lines, [Text.UTF8Encoding]::new($true))
    return $fullPath
}

function Invoke-ExternalCommand {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$CommandPath,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$ArtifactPath,
        [string]$WorkingDirectory = $root,
        [int[]]$AllowedExitCodes = @(0)
    )

    $previousPath = $env:PATH
    $previousErrorPreference = $ErrorActionPreference
    $commandDirectory = Split-Path -Parent $CommandPath
    if (-not [string]::IsNullOrWhiteSpace($commandDirectory)) {
        $env:PATH = "$commandDirectory;$env:PATH"
    }

    try {
        $ErrorActionPreference = "Continue"
        Push-Location $WorkingDirectory
        $output = & $CommandPath @Arguments 2>&1
        $exitCode = $LASTEXITCODE
    }
    finally {
        Pop-Location
        $env:PATH = $previousPath
        $ErrorActionPreference = $previousErrorPreference
    }

    $artifact = Write-TextArtifact -Path $ArtifactPath -Content $output

    if ($AllowedExitCodes -contains $exitCode) {
        Add-Result -Name $Name -Status "PASS" -Detail "Executado com exit code $exitCode." -Artifact $artifact
        return $true
    }

    Add-Result -Name $Name -Status "FAIL" -Detail "Falhou com exit code $exitCode." -Artifact $artifact
    return $false
}

function Write-Report {
    $fullPath = Resolve-RepoPath $ReportPath
    $directory = Split-Path -Parent $fullPath
    if (-not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add("# External security and quality scans")
    $lines.Add("")
    $lines.Add("Data: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss zzz")")
    $lines.Add("BaseUrl: $(if ([string]::IsNullOrWhiteSpace($BaseUrl)) { 'nao configurado' } else { $BaseUrl })")
    $lines.Add("")
    foreach ($result in $results) {
        $artifactText = if ([string]::IsNullOrWhiteSpace([string]$result.Artifact)) { "" } else { " Artefato: $($result.Artifact)" }
        $lines.Add("- [$($result.Status)] $($result.Name): $($result.Detail)$artifactText")
    }

    [IO.File]::WriteAllLines($fullPath, $lines, [Text.UTF8Encoding]::new($true))
    Write-Host "Relatorio: $fullPath"
}

$toolManifest = @(
    [pscustomobject]@{ Name = "semgrep"; Purpose = "SAST"; RequiredFor = "codigo C#/Razor" },
    [pscustomobject]@{ Name = "osv-scanner"; Purpose = "OSV dependency scan"; RequiredFor = "lockfiles NuGet" },
    [pscustomobject]@{ Name = "zap-baseline.py"; Purpose = "DAST baseline"; RequiredFor = "aplicacao em execucao" },
    [pscustomobject]@{ Name = "axe"; Purpose = "accessibility CLI"; RequiredFor = "aplicacao em execucao" }
)

$availability = @{}
foreach ($tool in $toolManifest) {
    $commandPath = Resolve-ExternalCommand -CommandName $tool.Name
    $available = -not [string]::IsNullOrWhiteSpace($commandPath)
    $availability[$tool.Name] = $available
    $status = if ($available) { "READY" } else { "NOT_INSTALLED" }
    $detail = "$($tool.Purpose); requerido para $($tool.RequiredFor)."
    if ($available) {
        $detail = "$detail Caminho: $commandPath"
    }

    Add-Result -Name $tool.Name -Status $status -Detail $detail
}

if ($InventoryOnly) {
    Write-Report
    if ($RequireTools -and ($availability.Values -contains $false)) {
        exit 1
    }

    Write-Host "External scanner inventory completed."
    exit 0
}

if ($RequireTools -and ($availability.Values -contains $false)) {
    $missing = ($toolManifest | Where-Object { -not $availability[$_.Name] } | ForEach-Object { $_.Name }) -join ", "
    Add-Result -Name "required-tools" -Status "FAIL" -Detail "Ferramentas obrigatorias ausentes: $missing."
    $hasFailure = $true
}

if ($availability["semgrep"]) {
    $semgrepJson = Resolve-RepoPath ".\artifacts\external-scans\semgrep.json"
    $ok = Invoke-ExternalCommand `
        -Name "semgrep-sast" `
        -CommandPath (Resolve-ExternalCommand -CommandName "semgrep") `
        -Arguments @("scan", "--config", ".\.semgrep.yml", "--error", "--json", "--output", $semgrepJson, ".") `
        -ArtifactPath ".\artifacts\external-scans\semgrep.console.txt"
    if (-not $ok) { $hasFailure = $true }
}

if ($availability["osv-scanner"]) {
    $osvJson = Resolve-RepoPath ".\artifacts\external-scans\osv-scanner.json"
    $ok = Invoke-ExternalCommand `
        -Name "osv-dependency-scan" `
        -CommandPath (Resolve-ExternalCommand -CommandName "osv-scanner") `
        -Arguments @("scan", "source", "--recursive", "--format", "json", "--output", $osvJson, ".") `
        -ArtifactPath ".\artifacts\external-scans\osv-scanner.console.txt"
    if (-not $ok) { $hasFailure = $true }
}

$onlineScansNeedBaseUrl = (-not $SkipDast) -or (-not $SkipAccessibility)
if ($onlineScansNeedBaseUrl -and [string]::IsNullOrWhiteSpace($BaseUrl)) {
    $status = if ($RequireTools) { "FAIL" } else { "SKIP" }
    Add-Result -Name "online-scan-base-url" -Status $status -Detail "Configure EXTERNAL_SCAN_BASE_URL ou passe -BaseUrl para DAST/a11y."
    if ($RequireTools) { $hasFailure = $true }
}

if (-not $SkipDast -and -not [string]::IsNullOrWhiteSpace($BaseUrl) -and $availability["zap-baseline.py"]) {
    $zapHtml = Resolve-RepoPath ".\artifacts\external-scans\zap-baseline.html"
    $zapJson = Resolve-RepoPath ".\artifacts\external-scans\zap-baseline.json"
    $zapCommand = Resolve-ExternalCommand -CommandName "zap-baseline.py"
    $zapArguments = if ([IO.Path]::GetFileName($zapCommand).Equals("zap.bat", [StringComparison]::OrdinalIgnoreCase)) {
        @("-cmd", "-quickurl", $BaseUrl, "-quickout", $zapHtml, "-quickprogress")
    }
    else {
        @("-t", $BaseUrl, "-r", $zapHtml, "-J", $zapJson)
    }

    $ok = Invoke-ExternalCommand `
        -Name "zap-dast-baseline" `
        -CommandPath $zapCommand `
        -Arguments $zapArguments `
        -ArtifactPath ".\artifacts\external-scans\zap-baseline.console.txt" `
        -WorkingDirectory (Split-Path -Parent $zapCommand)
    if (-not $ok) { $hasFailure = $true }
}

if (-not $SkipAccessibility -and -not [string]::IsNullOrWhiteSpace($BaseUrl) -and $availability["axe"]) {
    $axeArguments = @($BaseUrl, "--exit", "--save", "artifacts/external-scans/axe.json")
    $managedChromeDriver = Resolve-BrowserDriverManagerPath -Kind "chromedriver"
    $managedChrome = Resolve-BrowserDriverManagerPath -Kind "chrome"
    if (-not [string]::IsNullOrWhiteSpace($managedChromeDriver)) {
        $axeArguments += @("--chromedriver-path", $managedChromeDriver)
    }

    if (-not [string]::IsNullOrWhiteSpace($managedChrome)) {
        $axeArguments += @("--chrome-path", $managedChrome)
    }

    $ok = Invoke-ExternalCommand `
        -Name "axe-accessibility" `
        -CommandPath (Resolve-ExternalCommand -CommandName "axe") `
        -Arguments $axeArguments `
        -ArtifactPath ".\artifacts\external-scans\axe.console.txt"
    if (-not $ok) { $hasFailure = $true }
}

Write-Report
if ($hasFailure) {
    exit 1
}

Write-Host "External security scans completed."
