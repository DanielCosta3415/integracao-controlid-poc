[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string]$Root = "",
    [string[]]$Paths = @(
        ".\integracao_controlid.db",
        ".\integracao_controlid.db-wal",
        ".\integracao_controlid.db-shm",
        ".\Logs",
        ".\artifacts\backups",
        ".\artifacts\restore-smoke"
    )
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

$rootFullPath = [IO.Path]::GetFullPath($Root)

function Resolve-StatePath {
    param([string]$Path)

    if ([IO.Path]::IsPathRooted($Path)) {
        return [IO.Path]::GetFullPath($Path)
    }

    return [IO.Path]::GetFullPath((Join-Path -Path $rootFullPath -ChildPath $Path))
}

function Invoke-Icacls {
    param([string[]]$Arguments)

    & icacls.exe @Arguments | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "icacls failed with exit code $LASTEXITCODE for arguments: $($Arguments -join ' ')"
    }
}

function Protect-WindowsPath {
    param([IO.FileSystemInfo]$Item)

    $currentUserSid = [System.Security.Principal.WindowsIdentity]::GetCurrent().User.Value
    $path = $Item.FullName

    if ($Item.PSIsContainer) {
        $currentUserGrant = ("*{0}:(OI)(CI)(F)" -f $currentUserSid)
        $adminsGrant = "*S-1-5-32-544:(OI)(CI)(F)"
        $systemGrant = "*S-1-5-18:(OI)(CI)(F)"
        Invoke-Icacls -Arguments @($path, "/inheritance:r", "/grant:r", $currentUserGrant, $adminsGrant, $systemGrant, "/T", "/C")
    }
    else {
        $currentUserGrant = ("*{0}:F" -f $currentUserSid)
        Invoke-Icacls -Arguments @($path, "/inheritance:r", "/grant:r", $currentUserGrant, "*S-1-5-32-544:F", "*S-1-5-18:F", "/C")
    }
}

function Protect-UnixPath {
    param([IO.FileSystemInfo]$Item)

    if ($Item.PSIsContainer) {
        chmod 700 $Item.FullName
        Get-ChildItem -LiteralPath $Item.FullName -Recurse -Force | ForEach-Object {
            if ($_.PSIsContainer) {
                chmod 700 $_.FullName
            }
            else {
                chmod 600 $_.FullName
            }
        }
    }
    else {
        chmod 600 $Item.FullName
    }
}

$protected = [System.Collections.Generic.List[string]]::new()
$skipped = [System.Collections.Generic.List[string]]::new()
$isWindowsPlatform = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
    [System.Runtime.InteropServices.OSPlatform]::Windows)

foreach ($path in $Paths) {
    $fullPath = Resolve-StatePath -Path $path
    if (-not (Test-Path -LiteralPath $fullPath)) {
        $skipped.Add($fullPath)
        continue
    }

    $item = Get-Item -LiteralPath $fullPath -Force
    if ($PSCmdlet.ShouldProcess($item.FullName, "Restrict local runtime-state permissions")) {
        if ($isWindowsPlatform -or $env:OS -eq "Windows_NT") {
            Protect-WindowsPath -Item $item
        }
        else {
            Protect-UnixPath -Item $item
        }
    }

    $protected.Add($item.FullName)
}

Write-Host "Protected local state paths:"
foreach ($item in $protected) {
    Write-Host "- $item"
}

if ($skipped.Count -gt 0) {
    Write-Host "Skipped missing paths:"
    foreach ($item in $skipped) {
        Write-Host "- $item"
    }
}
