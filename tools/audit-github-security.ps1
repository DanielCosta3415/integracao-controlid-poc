[CmdletBinding()]
param(
    [string]$Repository = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw "GitHub CLI (gh) não foi encontrado no ambiente."
}

function Invoke-GitHubApi {
    param([Parameter(Mandatory)][string]$Endpoint)

    $output = @(& gh api $Endpoint 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "Falha ao consultar '$Endpoint': $($output -join ' ')"
    }

    return ($output -join "`n").Trim()
}

if ([string]::IsNullOrWhiteSpace($Repository)) {
    $origin = (& git remote get-url origin).Trim()
    if ($LASTEXITCODE -ne 0 -or
        $origin -notmatch 'github\.com[/:](?<repository>[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+?)(?:\.git)?$') {
        throw "Não foi possível identificar o repositório GitHub pelo remote origin."
    }

    $Repository = $Matches.repository
}

if ($Repository -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$') {
    throw "Repository deve usar o formato owner/name."
}

$failures = [System.Collections.Generic.List[string]]::new()
$warnings = [System.Collections.Generic.List[string]]::new()
$repositoryState = (Invoke-GitHubApi -Endpoint "repos/$Repository") | ConvertFrom-Json

if ($repositoryState.security_and_analysis.dependabot_security_updates.status -ne "enabled") {
    $failures.Add("Dependabot security updates está desativado.")
}

if ($repositoryState.security_and_analysis.secret_scanning.status -ne "enabled") {
    $failures.Add("Secret scanning está desativado.")
}

if ($repositoryState.security_and_analysis.secret_scanning_push_protection.status -ne "enabled") {
    $failures.Add("Push protection está desativado.")
}

if ($repositoryState.security_and_analysis.secret_scanning_non_provider_patterns.status -ne "enabled") {
    $warnings.Add("Non-provider patterns não está disponível ou permanece desativado.")
}

if ($repositoryState.security_and_analysis.secret_scanning_validity_checks.status -ne "enabled") {
    $warnings.Add("Validity checks não está disponível ou permanece desativado.")
}

$null = Invoke-GitHubApi -Endpoint "repos/$Repository/vulnerability-alerts"
$null = Invoke-GitHubApi -Endpoint "repos/$Repository/automated-security-fixes"

$codeQl = (Invoke-GitHubApi -Endpoint "repos/$Repository/code-scanning/default-setup") | ConvertFrom-Json
if ($codeQl.state -ne "configured") {
    $failures.Add("CodeQL Default Setup não está configurado.")
}

if ($codeQl.query_suite -ne "extended") {
    $failures.Add("CodeQL não usa o conjunto estendido.")
}

if ($codeQl.threat_model -ne "remote_and_local") {
    $failures.Add("CodeQL não analisa fontes remotas e locais.")
}

if (@($codeQl.languages) -notcontains "csharp") {
    $failures.Add("CodeQL não inclui C#.")
}

$rulesetSummaries = @((Invoke-GitHubApi -Endpoint "repos/$Repository/rulesets") | ConvertFrom-Json)
$hasIntegrityRuleset = $false
foreach ($summary in $rulesetSummaries) {
    $ruleset = (Invoke-GitHubApi -Endpoint "repos/$Repository/rulesets/$($summary.id)") | ConvertFrom-Json
    $types = @($ruleset.rules | ForEach-Object { $_.type })
    $includesMain = @($ruleset.conditions.ref_name.include) -contains "refs/heads/main"
    if ($ruleset.enforcement -eq "active" -and
        $includesMain -and
        $types -contains "deletion" -and
        $types -contains "non_fast_forward" -and
        $types -contains "required_linear_history") {
        $hasIntegrityRuleset = $true
        break
    }
}

if (-not $hasIntegrityRuleset) {
    $failures.Add("Nenhuma regra ativa protege main contra exclusão, force-push e histórico não linear.")
}

Write-Host "Auditoria de segurança remota: $Repository"
Write-Host "- Dependabot alerts e security updates: habilitados"
Write-Host "- Secret scanning e push protection: habilitados"
Write-Host "- CodeQL Default Setup: extended, remote_and_local, C#"
Write-Host "- Integridade de main: exclusão e force-push bloqueados; histórico linear"

foreach ($warning in $warnings) {
    Write-Warning $warning
}

if ($failures.Count -gt 0) {
    foreach ($failure in $failures) {
        Write-Error $failure
    }

    exit 1
}

Write-Host "Auditoria remota aprovada sem falhas obrigatórias."
