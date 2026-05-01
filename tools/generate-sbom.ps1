[CmdletBinding()]
param(
    [string[]]$Lockfile = @(
        ".\packages.lock.json",
        ".\tests\Integracao.ControlID.PoC.Tests\packages.lock.json",
        ".\tools\ControlIdDeviceStub\packages.lock.json",
        ".\tools\ControlIdCallbackSigningProxy\packages.lock.json"
    ),
    [string]$VendorManifestPath = ".\wwwroot\lib\vendor-dependencies.json",
    [string]$OutputPath = ".\artifacts\sbom\sbom.spdx.json"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function ConvertTo-SpdxId {
    param([Parameter(Mandatory = $true)][string]$Value)

    $normalized = $Value -replace "[^A-Za-z0-9\.\-]", "-"
    return "SPDXRef-Package-$normalized"
}

function Get-ChildText {
    param(
        [Parameter(Mandatory = $true)]$Node,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $child = $Node.ChildNodes | Where-Object { $_.LocalName -eq $Name } | Select-Object -First 1
    if ($null -eq $child) {
        return $null
    }

    return [string]$child.InnerText
}

function Get-ChildNode {
    param(
        [Parameter(Mandatory = $true)]$Node,
        [Parameter(Mandatory = $true)][string]$Name
    )

    return $Node.ChildNodes | Where-Object { $_.LocalName -eq $Name } | Select-Object -First 1
}

function Get-NuspecMetadata {
    param(
        [Parameter(Mandatory = $true)][string]$Id,
        [Parameter(Mandatory = $true)][string]$Version
    )

    $packageRoot = Join-Path $env:USERPROFILE ".nuget\packages"
    $idLower = $Id.ToLowerInvariant()
    $versionLower = $Version.ToLowerInvariant()
    $nuspecPath = Join-Path $packageRoot "$idLower\$versionLower\$idLower.nuspec"

    $result = [ordered]@{
        LicenseDeclared = "NOASSERTION"
        LicenseComments = $null
        Supplier = "NOASSERTION"
        ProjectUrl = $null
        RepositoryUrl = $null
        NuspecPath = $nuspecPath
    }

    if (-not (Test-Path -LiteralPath $nuspecPath)) {
        $result.LicenseComments = "Nuspec not found in local NuGet cache."
        return $result
    }

    [xml]$nuspec = Get-Content -LiteralPath $nuspecPath
    $metadata = $nuspec.package.metadata

    $authors = Get-ChildText -Node $metadata -Name "authors"
    if ($authors) {
        $result.Supplier = "Organization: $authors"
    }

    $licenseNode = Get-ChildNode -Node $metadata -Name "license"
    if ($licenseNode) {
        $license = [string]$licenseNode.InnerText
        if (-not [string]::IsNullOrWhiteSpace($license)) {
            $result.LicenseDeclared = $license.Trim()
        }
    }
    else {
        $licenseUrl = Get-ChildText -Node $metadata -Name "licenseUrl"
        if ($licenseUrl) {
            if ($licenseUrl -eq "https://github.com/dotnet/corefx/blob/master/LICENSE.TXT") {
                $result.LicenseDeclared = "MIT"
                $result.LicenseComments = "License inferred from legacy NuGet license URL: $licenseUrl"
            }
            elseif ($licenseUrl -eq "https://raw.githubusercontent.com/xunit/xunit/master/license.txt") {
                $result.LicenseDeclared = "Apache-2.0"
                $result.LicenseComments = "License inferred from legacy NuGet license URL: $licenseUrl"
            }
            else {
                $result.LicenseComments = "License URL declared by nuspec: $licenseUrl"
            }
        }
    }

    $projectUrl = Get-ChildText -Node $metadata -Name "projectUrl"
    if ($projectUrl) {
        $result.ProjectUrl = $projectUrl
    }

    $repositoryNode = Get-ChildNode -Node $metadata -Name "repository"
    if ($repositoryNode -and $repositoryNode.Attributes["url"]) {
        $result.RepositoryUrl = [string]$repositoryNode.Attributes["url"].Value
    }

    return $result
}

$packageMap = [ordered]@{}

foreach ($lockfilePath in $Lockfile) {
    if (-not (Test-Path -LiteralPath $lockfilePath)) {
        Write-Warning "Lockfile not found: $lockfilePath"
        continue
    }

    $lock = Get-Content -LiteralPath $lockfilePath -Raw | ConvertFrom-Json
    foreach ($targetFramework in $lock.dependencies.PSObject.Properties) {
        foreach ($dependency in $targetFramework.Value.PSObject.Properties) {
            $id = $dependency.Name
            $resolvedProperty = $dependency.Value.PSObject.Properties["resolved"]
            if ($null -eq $resolvedProperty) {
                continue
            }

            $version = [string]$resolvedProperty.Value

            if ([string]::IsNullOrWhiteSpace($version)) {
                continue
            }

            $key = "$id@$version"
            if (-not $packageMap.Contains($key)) {
                $metadata = Get-NuspecMetadata -Id $id -Version $version
                $typeProperty = $dependency.Value.PSObject.Properties["type"]
                $packageMap[$key] = [ordered]@{
                    Id = $id
                    Version = $version
                    Type = if ($null -ne $typeProperty) { [string]$typeProperty.Value } else { "Unknown" }
                    SourceLockfiles = [System.Collections.Generic.List[string]]::new()
                    Metadata = $metadata
                }
            }

            $packageMap[$key].SourceLockfiles.Add((Resolve-Path -LiteralPath $lockfilePath).Path)
        }
    }
}

$created = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
$documentNamespace = "https://spdx.org/spdxdocs/integracao-controlid-poc-$($created -replace '[:]', '-')"

$projectPackage = [ordered]@{
    name = "integracao-controlid-poc"
    SPDXID = "SPDXRef-Project"
    versionInfo = "NOASSERTION"
    supplier = "NOASSERTION"
    downloadLocation = "NOASSERTION"
    filesAnalyzed = $false
    licenseConcluded = "NOASSERTION"
    licenseDeclared = "NOASSERTION"
    copyrightText = "NOASSERTION"
}

$dependencyPackages = foreach ($entry in $packageMap.Values) {
    $spdxId = ConvertTo-SpdxId -Value "$($entry.Id)-$($entry.Version)"
    $package = [ordered]@{
        name = $entry.Id
        SPDXID = $spdxId
        versionInfo = $entry.Version
        supplier = $entry.Metadata.Supplier
        downloadLocation = "NOASSERTION"
        filesAnalyzed = $false
        licenseConcluded = "NOASSERTION"
        licenseDeclared = $entry.Metadata.LicenseDeclared
        copyrightText = "NOASSERTION"
        externalRefs = @(
            [ordered]@{
                referenceCategory = "PACKAGE-MANAGER"
                referenceType = "purl"
                referenceLocator = "pkg:nuget/$($entry.Id)@$($entry.Version)"
            }
        )
    }

    if ($entry.Metadata.LicenseComments) {
        $package.licenseComments = $entry.Metadata.LicenseComments
    }

    $package
}

$vendorPackages = @()
if (Test-Path -LiteralPath $VendorManifestPath) {
    $vendorManifest = Get-Content -LiteralPath $VendorManifestPath -Raw | ConvertFrom-Json
    $vendorPackages = foreach ($dependency in $vendorManifest.dependencies) {
        [ordered]@{
            name = $dependency.name
            SPDXID = ConvertTo-SpdxId -Value "Vendor-$($dependency.id)-$($dependency.version)"
            versionInfo = $dependency.version
            supplier = "Organization: $($dependency.supplier)"
            downloadLocation = if ($dependency.sourceUrl) { [string]$dependency.sourceUrl } else { "NOASSERTION" }
            filesAnalyzed = $false
            licenseConcluded = "NOASSERTION"
            licenseDeclared = $dependency.license
            copyrightText = "NOASSERTION"
            checksums = @(
                [ordered]@{
                    algorithm = "SHA256"
                    checksumValue = $dependency.directorySha256
                }
            )
            externalRefs = @(
                [ordered]@{
                    referenceCategory = "PACKAGE-MANAGER"
                    referenceType = "purl"
                    referenceLocator = "pkg:generic/$($dependency.id)@$($dependency.version)"
                }
            )
            comment = "Vendored dependency path: $($dependency.path); files: $($dependency.fileCount)."
        }
    }
}

$allDependencyPackages = @($dependencyPackages) + @($vendorPackages)

$relationships = foreach ($package in $allDependencyPackages) {
    [ordered]@{
        spdxElementId = "SPDXRef-Project"
        relationshipType = "DEPENDS_ON"
        relatedSpdxElement = $package.SPDXID
    }
}

$sbom = [ordered]@{
    spdxVersion = "SPDX-2.3"
    dataLicense = "CC0-1.0"
    SPDXID = "SPDXRef-DOCUMENT"
    name = "integracao-controlid-poc"
    documentNamespace = $documentNamespace
    creationInfo = [ordered]@{
        creators = @("Tool: tools/generate-sbom.ps1")
        created = $created
    }
    documentDescribes = @("SPDXRef-Project")
    packages = @($projectPackage) + @($allDependencyPackages)
    relationships = @($relationships)
}

$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

$sbom | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $OutputPath -Encoding UTF8
Write-Host "SBOM generated at $OutputPath with $($packageMap.Count) unique NuGet packages and $(@($vendorPackages).Count) vendored packages."
