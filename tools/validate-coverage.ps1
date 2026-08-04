[CmdletBinding()]
param(
    [string]$CoveragePath = ".\artifacts\test-readiness\coverage",
    [double]$MinimumLineRate = 0.28,
    [double]$MinimumBranchRate = 0.16,
    [string]$ReportPath = ".\artifacts\test-readiness\coverage-summary.md"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot

function Resolve-RepoPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    if ([IO.Path]::IsPathRooted($Path)) {
        return $Path
    }

    return Join-Path $root ($Path -replace '^[.][\\/]', '')
}

$coverageRoot = Resolve-RepoPath $CoveragePath
$coverageFile = Get-ChildItem -Path $coverageRoot -Recurse -File -Filter "coverage.cobertura.xml" -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1

if (-not $coverageFile) {
    throw "Nenhum coverage.cobertura.xml foi encontrado em $coverageRoot."
}

[xml]$document = [IO.File]::ReadAllText($coverageFile.FullName, [Text.Encoding]::UTF8)
$coverage = $document.coverage
$lineRate = [double]::Parse($coverage.'line-rate', [Globalization.CultureInfo]::InvariantCulture)
$branchRate = [double]::Parse($coverage.'branch-rate', [Globalization.CultureInfo]::InvariantCulture)
$linesCovered = [int]$coverage.'lines-covered'
$linesValid = [int]$coverage.'lines-valid'
$branchesCovered = [int]$coverage.'branches-covered'
$branchesValid = [int]$coverage.'branches-valid'

$reportFullPath = Resolve-RepoPath $ReportPath
$reportDirectory = Split-Path -Parent $reportFullPath
if (-not (Test-Path -LiteralPath $reportDirectory)) {
    New-Item -ItemType Directory -Force -Path $reportDirectory | Out-Null
}

$status = if ($lineRate -ge $MinimumLineRate -and $branchRate -ge $MinimumBranchRate) { "PASS" } else { "FAIL" }
$branchesLabel = "Ramifica$([char]0x00E7)$([char]0x00F5)es"
$minimumLabel = "M$([char]0x00ED)nimo"
$lines = @(
    "# Cobertura automatizada",
    "",
    "- Status: $status",
    "- Linhas: $linesCovered/$linesValid ($([Math]::Round($lineRate * 100, 2))%)",
    "- ${branchesLabel}: $branchesCovered/$branchesValid ($([Math]::Round($branchRate * 100, 2))%)",
    "- $minimumLabel de linhas: $([Math]::Round($MinimumLineRate * 100, 2))%",
    "- $minimumLabel de $($branchesLabel.ToLowerInvariant()): $([Math]::Round($MinimumBranchRate * 100, 2))%",
    "- Artefato: $($coverageFile.FullName)"
)
[IO.File]::WriteAllLines($reportFullPath, $lines, [Text.UTF8Encoding]::new($true))
$lines | ForEach-Object { Write-Host $_ }

if ($status -eq "FAIL") {
    throw "Cobertura abaixo do piso versionado."
}
