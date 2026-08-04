[CmdletBinding()]
param(
    [string]$BaselinePath = ".\tools\maintainability-baseline.json",
    [string]$ReportPath = ".\artifacts\maintainability\latest.md"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

function Resolve-RepoPath {
    param([Parameter(Mandatory = $true)][string]$Path)
    if ([IO.Path]::IsPathRooted($Path)) { return $Path }
    return Join-Path $root ($Path -replace '^[.][\\/]', '')
}

$baseline = Get-Content -LiteralPath (Resolve-RepoPath $BaselinePath) -Raw -Encoding UTF8 | ConvertFrom-Json
$globalLimits = @{}
$baseline.globalLimits.PSObject.Properties | ForEach-Object { $globalLimits[$_.Name] = [int]$_.Value }
$exceptions = @{}
$baseline.exceptions.PSObject.Properties | ForEach-Object { $exceptions[$_.Name.Replace('/', '\')] = [int]$_.Value }
$excludedPattern = '\\(bin|obj|artifacts|wwwroot\\lib|Data\\Migrations)\\'
$files = Get-ChildItem -Path $root -Recurse -File -Include *.cs,*.cshtml,*.css,*.js |
    Where-Object { $_.FullName -notmatch $excludedPattern }
$results = @()
$violations = @()

foreach ($file in $files) {
    $relativePath = $file.FullName.Substring($root.Length + 1)
    $extension = $file.Extension.ToLowerInvariant()
    $lineCount = @(Get-Content -LiteralPath $file.FullName).Count
    $limit = if ($exceptions.ContainsKey($relativePath)) { $exceptions[$relativePath] } else { $globalLimits[$extension] }
    $content = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8
    $decisionCount = [regex]::Matches($content, '\b(if|else if|switch|case|for|foreach|while|catch)\b|&&|\|\|').Count
    $result = [pscustomobject]@{
        Path = $relativePath
        Lines = $lineCount
        Limit = $limit
        Decisions = $decisionCount
        Status = if ($lineCount -le $limit) { 'PASS' } else { 'FAIL' }
    }
    $results += $result
    if ($result.Status -eq 'FAIL') { $violations += $result }
}

$reportFullPath = Resolve-RepoPath $ReportPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $reportFullPath) | Out-Null
$lines = @(
    '# Verificacao de manutenibilidade',
    '',
    "- Status: $(if ($violations.Count -eq 0) { 'PASS' } else { 'FAIL' })",
    "- Arquivos avaliados: $($results.Count)",
    "- Violacoes de tamanho: $($violations.Count)",
    '',
    '## Maiores arquivos',
    '',
    '| Arquivo | Linhas | Limite | Decisoes aproximadas | Status |',
    '| --- | ---: | ---: | ---: | --- |'
)
foreach ($result in ($results | Sort-Object Lines -Descending | Select-Object -First 20)) {
    $lines += "| ``$($result.Path.Replace('\', '/'))`` | $($result.Lines) | $($result.Limit) | $($result.Decisions) | $($result.Status) |"
}
$lines += ''
$lines += 'A contagem de decisoes e um indicador heuristico; compilacao e testes continuam sendo os gates de corretude.'
$lines | Set-Content -LiteralPath $reportFullPath -Encoding UTF8
$lines | ForEach-Object { Write-Host $_ }

if ($violations.Count -gt 0) {
    throw "Arquivos excederam os limites versionados de manutenibilidade: $($violations.Path -join ', ')."
}
