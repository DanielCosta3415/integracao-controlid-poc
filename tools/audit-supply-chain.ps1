[CmdletBinding()]
param(
    [string[]]$NuGetTargets = @(
        ".\Integracao.ControlID.PoC.sln",
        ".\tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj",
        ".\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj"
    ),
    [string]$SbomOutputPath = ".\artifacts\sbom\sbom.spdx.json"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Invoke-DotNetPackageCheck {
    param(
        [Parameter(Mandatory = $true)][string]$Target,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$FailureMessage
    )

    $output = & dotnet list $Target package @Arguments 2>&1
    $output | Write-Output

    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    $text = $output -join "`n"
    if ($text -match '(?m)^\s*>\s+\S+\s+') {
        Write-Error "$FailureMessage Target: $Target"
        exit 1
    }
}

foreach ($target in $NuGetTargets) {
    Invoke-DotNetPackageCheck `
        -Target $target `
        -Arguments @("--vulnerable", "--include-transitive") `
        -FailureMessage "Vulnerable NuGet packages were found."
}

Invoke-DotNetPackageCheck `
    -Target ".\Integracao.ControlID.PoC.sln" `
    -Arguments @("--deprecated") `
    -FailureMessage "Deprecated NuGet packages were found."

Invoke-DotNetPackageCheck `
    -Target ".\Integracao.ControlID.PoC.sln" `
    -Arguments @("--outdated", "--highest-patch") `
    -FailureMessage "Patch-level NuGet updates are available."

& ".\tools\audit-vendor-dependencies.ps1"
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

& ".\tools\generate-sbom.ps1" -OutputPath $SbomOutputPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host "Supply-chain audit completed successfully."
