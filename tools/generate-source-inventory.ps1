[CmdletBinding()]
param(
    [string]$OutputPath = "artifacts/documentation/source-inventory.md"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$fullOutputPath = if ([IO.Path]::IsPathRooted($OutputPath)) {
    $OutputPath
}
else {
    Join-Path $root $OutputPath
}

$tracked = @(& git -C $root ls-files)
if ($LASTEXITCODE -ne 0) {
    throw "Não foi possível listar os arquivos versionados."
}

$sourcePattern = '(?i)(\.cs$|\.cshtml$|\.ps1$|\.js$|\.css$|\.yml$|\.yaml$|\.json$|\.csproj$|\.sln$|(^|/)Dockerfile$)'
$files = @($tracked |
    ForEach-Object { $_.Replace('\', '/') } |
    Where-Object { $_ -match $sourcePattern -and $_ -notmatch '(^|/)(bin|obj|artifacts|Logs)/' } |
    Sort-Object)

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add('# Inventário gerado de arquivos-fonte')
$lines.Add('')
$lines.Add('> Artefato gerado localmente; não versionar. Fonte: `git ls-files`.')
$lines.Add('')
$lines.Add("- Gerado em UTC: $([DateTimeOffset]::UtcNow.ToString('u'))")
$lines.Add("- Commit: $(& git -C $root rev-parse HEAD)")
$lines.Add("- Arquivos: $($files.Count)")
$lines.Add('')

foreach ($group in ($files | Group-Object { if ($_ -match '/') { ($_ -split '/')[0] } else { '(raiz)' } })) {
    $lines.Add("## $($group.Name)")
    $lines.Add('')
    foreach ($file in $group.Group) {
        $lines.Add("- ``$file``")
    }
    $lines.Add('')
}

$outputDirectory = Split-Path -Parent $fullOutputPath
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
[IO.File]::WriteAllLines($fullOutputPath, $lines, [Text.UTF8Encoding]::new($false))

Write-Output "Inventário gerado: $fullOutputPath"
Write-Output "Arquivos-fonte: $($files.Count)"
