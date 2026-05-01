[CmdletBinding()]
param(
    [string]$DatabasePath = ".\integracao_controlid.db",
    [string]$OutputDirectory = ".\artifacts\backups",
    [ValidateSet("CurrentUser", "LocalMachine")]
    [string]$ProtectionScope = "CurrentUser",
    [switch]$Unprotected
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$resolvedDatabase = Resolve-Path -LiteralPath $DatabasePath -ErrorAction SilentlyContinue
if (-not $resolvedDatabase) {
    Write-Error "SQLite database not found at '$DatabasePath'. Run this command from the repository root or pass -DatabasePath."
    exit 2
}

$databaseItem = Get-Item -LiteralPath $resolvedDatabase.Path
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$backupDirectory = Join-Path -Path $OutputDirectory -ChildPath ("sqlite-" + $timestamp)

New-Item -ItemType Directory -Path $backupDirectory -Force | Out-Null

function Initialize-DpapiNative {
    if (-not ("DpapiNative" -as [type])) {
        Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public static class DpapiNative
{
    [StructLayout(LayoutKind.Sequential)]
    private struct DataBlob
    {
        public int cbData;
        public IntPtr pbData;
    }

    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CryptProtectData(
        ref DataBlob pDataIn,
        string szDataDescr,
        IntPtr pOptionalEntropy,
        IntPtr pvReserved,
        IntPtr pPromptStruct,
        int dwFlags,
        ref DataBlob pDataOut);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LocalFree(IntPtr hMem);

    public static byte[] Protect(byte[] data, bool localMachine)
    {
        DataBlob input = new DataBlob();
        DataBlob output = new DataBlob();
        input.cbData = data.Length;
        input.pbData = Marshal.AllocHGlobal(data.Length);

        try
        {
            Marshal.Copy(data, 0, input.pbData, data.Length);
            int flags = localMachine ? 0x4 : 0;
            if (!CryptProtectData(ref input, "integracao-controlid-poc-sqlite-backup", IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, flags, ref output))
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }

            byte[] protectedData = new byte[output.cbData];
            Marshal.Copy(output.pbData, protectedData, 0, output.cbData);
            return protectedData;
        }
        finally
        {
            if (input.pbData != IntPtr.Zero) Marshal.FreeHGlobal(input.pbData);
            if (output.pbData != IntPtr.Zero) LocalFree(output.pbData);
        }
    }
}
"@
    }
}

function Protect-BackupFile {
    param(
        [string]$SourcePath,
        [string]$DestinationPath,
        [string]$Scope
    )

    Initialize-DpapiNative
    $bytes = [IO.File]::ReadAllBytes($SourcePath)
    $protectedBytes = [DpapiNative]::Protect($bytes, $Scope -eq "LocalMachine")
    [IO.File]::WriteAllBytes($DestinationPath, $protectedBytes)
}

$copiedFiles = New-Object System.Collections.Generic.List[string]
$candidatePaths = @(
    $databaseItem.FullName,
    ($databaseItem.FullName + "-wal"),
    ($databaseItem.FullName + "-shm")
)

foreach ($candidatePath in $candidatePaths) {
    if (Test-Path -LiteralPath $candidatePath) {
        $fileName = [System.IO.Path]::GetFileName($candidatePath)
        $destination = Join-Path -Path $backupDirectory -ChildPath $fileName

        if ($Unprotected) {
            Copy-Item -LiteralPath $candidatePath -Destination $destination -Force
        }
        else {
            $destination = $destination + ".protected"
            Protect-BackupFile -SourcePath $candidatePath -DestinationPath $destination -Scope $ProtectionScope
        }

        $copiedFiles.Add($destination)
    }
}

$manifestPath = Join-Path -Path $backupDirectory -ChildPath "backup-manifest.txt"
$manifest = @(
    "CreatedAtUtc=$((Get-Date).ToUniversalTime().ToString('O'))",
    "SourceDatabase=$($databaseItem.FullName)",
    "Protected=$(-not $Unprotected)",
    "Protection=DPAPI",
    "ProtectionScope=$ProtectionScope",
    "CopiedFiles="
)

foreach ($copiedFile in $copiedFiles) {
    $manifest += "- $copiedFile"
}

Set-Content -LiteralPath $manifestPath -Value $manifest -Encoding UTF8

Write-Host "SQLite backup created at $backupDirectory"
