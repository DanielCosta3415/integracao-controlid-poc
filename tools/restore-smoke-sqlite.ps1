[CmdletBinding()]
param(
    [string]$BackupDirectory = "",
    [string]$OutputDirectory = ".\artifacts\restore-smoke",
    [ValidateSet("CurrentUser", "LocalMachine")]
    [string]$ProtectionScope = "CurrentUser"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-BackupDirectory {
    param([string]$RequestedDirectory)

    if (-not [string]::IsNullOrWhiteSpace($RequestedDirectory)) {
        $resolved = Resolve-Path -LiteralPath $RequestedDirectory -ErrorAction SilentlyContinue
        if (-not $resolved) {
            throw "Backup directory '$RequestedDirectory' was not found."
        }

        return (Get-Item -LiteralPath $resolved.Path)
    }

    $backupRoot = ".\artifacts\backups"
    if (-not (Test-Path -LiteralPath $backupRoot)) {
        throw "No backup directory was provided and '$backupRoot' does not exist."
    }

    $latest = Get-ChildItem -LiteralPath $backupRoot -Directory |
        Sort-Object LastWriteTimeUtc -Descending |
        Select-Object -First 1

    if (-not $latest) {
        throw "No SQLite backup folders were found under '$backupRoot'."
    }

    return $latest
}

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
    private static extern bool CryptUnprotectData(
        ref DataBlob pDataIn,
        IntPtr ppszDataDescr,
        IntPtr pOptionalEntropy,
        IntPtr pvReserved,
        IntPtr pPromptStruct,
        int dwFlags,
        ref DataBlob pDataOut);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LocalFree(IntPtr hMem);

    public static byte[] Unprotect(byte[] data, bool localMachine)
    {
        DataBlob input = new DataBlob();
        DataBlob output = new DataBlob();
        input.cbData = data.Length;
        input.pbData = Marshal.AllocHGlobal(data.Length);

        try
        {
            Marshal.Copy(data, 0, input.pbData, data.Length);
            int flags = localMachine ? 0x4 : 0;
            if (!CryptUnprotectData(ref input, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, flags, ref output))
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }

            byte[] plainData = new byte[output.cbData];
            Marshal.Copy(output.pbData, plainData, 0, output.cbData);
            return plainData;
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

function Copy-RestoredBackupFile {
    param(
        [string]$SourcePath,
        [string]$DestinationPath,
        [string]$Scope
    )

    if ($SourcePath.EndsWith(".protected", [StringComparison]::OrdinalIgnoreCase)) {
        Initialize-DpapiNative
        $protectedBytes = [IO.File]::ReadAllBytes($SourcePath)
        $plainBytes = [DpapiNative]::Unprotect($protectedBytes, $Scope -eq "LocalMachine")
        [IO.File]::WriteAllBytes($DestinationPath, $plainBytes)
        return
    }

    Copy-Item -LiteralPath $SourcePath -Destination $DestinationPath -Force
}

$backup = Resolve-BackupDirectory -RequestedDirectory $BackupDirectory
$sourceDatabase = Get-ChildItem -LiteralPath $backup.FullName -File -Filter "*.db" | Select-Object -First 1
if (-not $sourceDatabase) {
    $sourceDatabase = Get-ChildItem -LiteralPath $backup.FullName -File -Filter "*.db.protected" | Select-Object -First 1
}

if (-not $sourceDatabase) {
    throw "No .db or .db.protected file was found in backup directory '$($backup.FullName)'."
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$smokeDirectory = Join-Path -Path $OutputDirectory -ChildPath ("sqlite-" + $timestamp)
New-Item -ItemType Directory -Path $smokeDirectory -Force | Out-Null

$restoredDatabaseName = if ($sourceDatabase.Name.EndsWith(".protected", [StringComparison]::OrdinalIgnoreCase)) {
    $sourceDatabase.Name.Substring(0, $sourceDatabase.Name.Length - ".protected".Length)
}
else {
    $sourceDatabase.Name
}

$restoreDatabase = Join-Path -Path $smokeDirectory -ChildPath $restoredDatabaseName
Copy-RestoredBackupFile -SourcePath $sourceDatabase.FullName -DestinationPath $restoreDatabase -Scope $ProtectionScope

foreach ($suffix in @("-wal", "-shm")) {
    $sourceSidecar = if ($sourceDatabase.FullName.EndsWith(".protected", [StringComparison]::OrdinalIgnoreCase)) {
        $sourceDatabase.FullName.Substring(0, $sourceDatabase.FullName.Length - ".protected".Length) + $suffix + ".protected"
    }
    else {
        $sourceDatabase.FullName + $suffix
    }

    if (Test-Path -LiteralPath $sourceSidecar) {
        Copy-RestoredBackupFile -SourcePath $sourceSidecar -DestinationPath ($restoreDatabase + $suffix) -Scope $ProtectionScope
    }
}

$connectionString = "Data Source=$restoreDatabase"
dotnet ef database update --no-build --connection $connectionString

if ($LASTEXITCODE -ne 0) {
    throw "EF migration smoke failed for restored SQLite copy '$restoreDatabase'."
}

$manifestPath = Join-Path -Path $smokeDirectory -ChildPath "restore-smoke-manifest.txt"
Set-Content -LiteralPath $manifestPath -Encoding UTF8 -Value @(
    "CreatedAtUtc=$((Get-Date).ToUniversalTime().ToString('O'))",
    "BackupDirectory=$($backup.FullName)",
    "RestoredCopy=$restoreDatabase",
    "ProtectedInput=$($sourceDatabase.Name.EndsWith('.protected', [StringComparison]::OrdinalIgnoreCase))",
    "Result=EF database update succeeded on restored copy"
)

Write-Host "SQLite restore smoke succeeded at $smokeDirectory"
