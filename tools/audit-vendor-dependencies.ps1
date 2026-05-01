[CmdletBinding()]
param(
    [string]$ManifestPath = ".\wwwroot\lib\vendor-dependencies.json"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-RelativePath {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $prefix = $Root.TrimEnd('\') + '\'
    return $Path.Substring($prefix.Length).Replace('\', '/')
}

function Get-TextSha256 {
    param([Parameter(Mandatory = $true)][string]$Text)

    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
        $hash = $sha.ComputeHash($bytes)
        return ([System.BitConverter]::ToString($hash) -replace "-", "").ToLowerInvariant()
    }
    finally {
        $sha.Dispose()
    }
}

function Get-NormalizedFileSha256 {
    param([Parameter(Mandatory = $true)][string]$Path)

    $bytes = [IO.File]::ReadAllBytes($Path)
    if ($bytes -contains 0) {
        return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
    }

    try {
        $text = [Text.UTF8Encoding]::new($false, $true).GetString($bytes)
    }
    catch {
        return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
    }

    $normalizedText = $text.Replace("`r`n", "`n").Replace("`r", "`n")
    return Get-TextSha256 -Text $normalizedText
}

function Get-DirectorySha256 {
    param([Parameter(Mandatory = $true)][string]$Path)

    $root = (Resolve-Path -LiteralPath $Path).Path
    $lines = Get-ChildItem -LiteralPath $root -Recurse -File |
        Sort-Object FullName |
        ForEach-Object {
            $relative = Get-RelativePath -Root $root -Path $_.FullName
            $hash = Get-NormalizedFileSha256 -Path $_.FullName
            "$relative`t$hash"
        }

    return @{
        FileCount = @($lines).Count
        Sha256 = Get-TextSha256 -Text ([string]::Join("`n", $lines) + "`n")
    }
}

function ConvertTo-VersionValue {
    param([Parameter(Mandatory = $true)][string]$Version)

    return [version]($Version -replace "[^\d\.].*$", "")
}

$manifestFullPath = (Resolve-Path -LiteralPath $ManifestPath).Path
$manifest = Get-Content -LiteralPath $manifestFullPath -Raw | ConvertFrom-Json
$root = (Resolve-Path -LiteralPath ".").Path
$failures = [System.Collections.Generic.List[string]]::new()

$declaredPaths = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$allowedLicenses = @($manifest.allowedLicenses)

foreach ($dependency in $manifest.dependencies) {
    $dependencyPath = Join-Path $root $dependency.path
    $licensePath = Join-Path $root $dependency.licensePath
    $versionFilePath = Join-Path $root $dependency.versionFile

    if (-not (Test-Path -LiteralPath $dependencyPath)) {
        $failures.Add("Vendor path not found for $($dependency.id): $($dependency.path)")
        continue
    }

    [void]$declaredPaths.Add((Resolve-Path -LiteralPath $dependencyPath).Path)

    if (-not (Test-Path -LiteralPath $licensePath)) {
        $failures.Add("License file not found for $($dependency.id): $($dependency.licensePath)")
    }

    if ($allowedLicenses -notcontains [string]$dependency.license) {
        $failures.Add("License '$($dependency.license)' is not allowed for $($dependency.id).")
    }

    if (-not (Test-Path -LiteralPath $versionFilePath)) {
        $failures.Add("Version file not found for $($dependency.id): $($dependency.versionFile)")
    }
    else {
        $versionText = Get-Content -LiteralPath $versionFilePath -Raw
        $match = [regex]::Match($versionText, [string]$dependency.versionPattern)
        if (-not $match.Success) {
            $failures.Add("Version pattern did not match for $($dependency.id).")
        }
        else {
            $detectedVersion = $match.Groups["version"].Value
            if ($detectedVersion -ne [string]$dependency.version) {
                $failures.Add("Version mismatch for $($dependency.id): manifest=$($dependency.version), detected=$detectedVersion.")
            }

            if ((ConvertTo-VersionValue $detectedVersion) -lt (ConvertTo-VersionValue ([string]$dependency.minimumSafeVersion))) {
                $failures.Add("Vendor $($dependency.id) is below minimum safe version $($dependency.minimumSafeVersion): $detectedVersion.")
            }
        }
    }

    $directoryHash = Get-DirectorySha256 -Path $dependencyPath
    if ($directoryHash.FileCount -ne [int]$dependency.fileCount) {
        $failures.Add("File count mismatch for $($dependency.id): manifest=$($dependency.fileCount), detected=$($directoryHash.FileCount).")
    }

    if ($directoryHash.Sha256 -ne [string]$dependency.directorySha256) {
        $failures.Add("Directory SHA256 mismatch for $($dependency.id).")
    }
}

$vendorRoot = Join-Path $root "wwwroot\lib"
$actualVendorDirectories = Get-ChildItem -LiteralPath $vendorRoot -Directory
foreach ($directory in $actualVendorDirectories) {
    if (-not $declaredPaths.Contains($directory.FullName)) {
        $failures.Add("Undeclared vendor dependency directory: $($directory.FullName)")
    }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Host "Vendor dependency audit passed for $(@($manifest.dependencies).Count) dependencies."
