[CmdletBinding()]
param(
    [string]$Root = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($Root)) {
    $scriptRoot = if (-not [string]::IsNullOrWhiteSpace($PSScriptRoot)) {
        $PSScriptRoot
    }
    else {
        Split-Path -Parent $MyInvocation.MyCommand.Path
    }

    $Root = Split-Path -Parent $scriptRoot
}

$allowedExtensions = @(
    ".config",
    ".cs",
    ".cshtml",
    ".csproj",
    ".editorconfig",
    ".gitattributes",
    ".gitignore",
    ".json",
    ".md",
    ".props",
    ".ps1",
    ".sln",
    ".targets",
    ".txt",
    ".yaml",
    ".yml"
)

$excludedPathPattern = '(^|/)(bin|obj|Logs|artifacts)/|(^|/)packages\.lock\.json$'

$patterns = @(
    [pscustomobject]@{
        Name = "AWS access key"
        Regex = [regex]'AKIA[0-9A-Z]{16}'
        UsesCapturedValue = $false
    },
    [pscustomobject]@{
        Name = "GitHub token"
        Regex = [regex]'gh[pousr]_[A-Za-z0-9_]{36,}'
        UsesCapturedValue = $false
    },
    [pscustomobject]@{
        Name = "Private key"
        Regex = [regex]'-----BEGIN (RSA |EC |OPENSSH |DSA |)?PRIVATE KEY-----'
        UsesCapturedValue = $false
    },
    [pscustomobject]@{
        Name = "Generic secret assignment"
        Regex = [regex]'(?i)(password|passwd|pwd|secret|token|api[_-]?key|sharedkey)\s*[:=]\s*["'']?(?<value>[^"'',;\s]+)'
        UsesCapturedValue = $true
    }
)

function Test-PlaceholderValue {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $true
    }

    $normalized = $Value.Trim().Trim('"', "'")
    if ($normalized.Length -lt 12) {
        return $true
    }

    if ($normalized.StartsWith("$") -or
        $normalized -match '[(){}]' -or
        $normalized -match '^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)+$') {
        return $true
    }

    return $normalized -match '^[<{].*[>}]$' -or
        $normalized -match '(?i)^(changeme|change-me|example|placeholder|dummy|sample|test|local|dev|admin|senha|usuario|segredo|token|secret|password)$' -or
        $normalized -match '(?i)(<|>|placeholder|example|dummy|sample|segredo-local|senha|usuario)'
}

function Get-ScannableFiles {
    $gitFiles = git -C $Root ls-files --cached --others --exclude-standard
    foreach ($relativePath in $gitFiles) {
        if ([string]::IsNullOrWhiteSpace($relativePath)) {
            continue
        }

        $normalized = $relativePath -replace '\\', '/'
        if ($normalized -match $excludedPathPattern) {
            continue
        }

        $extension = [IO.Path]::GetExtension($relativePath)
        if ($allowedExtensions -notcontains $extension -and -not [string]::IsNullOrWhiteSpace($extension)) {
            continue
        }

        Join-Path $Root $relativePath
    }
}

$findings = [System.Collections.Generic.List[object]]::new()
$rootFullPath = [IO.Path]::GetFullPath($Root).TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)

foreach ($file in Get-ScannableFiles) {
    if (-not (Test-Path $file -PathType Leaf)) {
        continue
    }

    $fileFullPath = [IO.Path]::GetFullPath($file)
    $relative = if ($fileFullPath.StartsWith($rootFullPath, [StringComparison]::OrdinalIgnoreCase)) {
        $fileFullPath.Substring($rootFullPath.Length).TrimStart([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)
    }
    else {
        $fileFullPath
    }
    $lineNumber = 0

    foreach ($line in Get-Content -LiteralPath $file -ErrorAction Stop) {
        $lineNumber++
        foreach ($pattern in $patterns) {
            foreach ($match in $pattern.Regex.Matches($line)) {
                $value = if ($pattern.UsesCapturedValue -and $match.Groups["value"].Success) {
                    $match.Groups["value"].Value
                }
                else {
                    $match.Value
                }

                if ($pattern.UsesCapturedValue -and (Test-PlaceholderValue -Value $value)) {
                    continue
                }

                $findings.Add([pscustomobject]@{
                    File = $relative
                    Line = $lineNumber
                    Rule = $pattern.Name
                })
            }
        }
    }
}

if ($findings.Count -gt 0) {
    Write-Host "Potential secrets were found:"
    foreach ($finding in $findings) {
        Write-Host ("{0}:{1} [{2}]" -f $finding.File, $finding.Line, $finding.Rule)
    }

    exit 1
}

Write-Host "Secret scan passed. No high-confidence secrets found."
