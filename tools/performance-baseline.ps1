[CmdletBinding()]
param(
    [string]$StubUrl = "http://127.0.0.1:6610",
    [int[]]$DatasetSizes = @(100, 1000, 10000),
    [int]$LatencySamples = 20,
    [int]$ConcurrentRequests = 20,
    [switch]$IncludeMaximumDataset,
    [switch]$FailOnBudget,
    [string]$ReportPath = ".\artifacts\performance\baseline-latest.md",
    [string]$JsonPath = ".\artifacts\performance\baseline-latest.json"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
Add-Type -AssemblyName System.Net.Http
$stubProject = Join-Path $root "tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj"
$stubDll = Join-Path $root "tools\ControlIdDeviceStub\bin\Debug\net10.0\ControlIdDeviceStub.dll"
$startedProcess = $null

function Resolve-RepoPath {
    param([Parameter(Mandatory = $true)][string]$Path)
    if ([IO.Path]::IsPathRooted($Path)) { return $Path }
    return Join-Path $root ($Path -replace '^[.][\\/]', '')
}

function Get-Percentile {
    param([double[]]$Values, [double]$Percentile)
    if ($Values.Count -eq 0) { return 0 }
    $sorted = @($Values | Sort-Object)
    $index = [Math]::Ceiling($Percentile * $sorted.Count) - 1
    return [Math]::Round($sorted[[Math]::Max(0, $index)], 2)
}

function Wait-Ready {
    $deadline = (Get-Date).AddSeconds(20)
    while ((Get-Date) -lt $deadline) {
        try {
            Invoke-WebRequest -Uri ($StubUrl.TrimEnd('/') + '/__stub/status') -UseBasicParsing -TimeoutSec 2 | Out-Null
            return
        }
        catch {
            Start-Sleep -Milliseconds 250
        }
    }
    throw "O simulador de desempenho nao ficou pronto no tempo limite."
}

function Invoke-LoadObjects {
    param(
        [System.Net.Http.HttpClient]$Client,
        [int]$DatasetSize
    )
    $offset = [Math]::Max(0, $DatasetSize - 100)
    $content = [System.Net.Http.StringContent]::new(
        "{`"object`":`"users`",`"limit`":100,`"offset`":$offset}",
        [Text.Encoding]::UTF8,
        'application/json')
    $response = $Client.PostAsync('/load_objects.fcgi', $content).GetAwaiter().GetResult()
    try {
        $bytes = $response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
        if (-not $response.IsSuccessStatusCode) {
            throw "Resposta HTTP inesperada: $([int]$response.StatusCode)."
        }
        return $bytes.LongLength
    }
    finally {
        $response.Dispose()
        $content.Dispose()
    }
}

Push-Location $root
try {
    if ($IncludeMaximumDataset -and $DatasetSizes -notcontains 100000) {
        $DatasetSizes += 100000
    }

    dotnet build $stubProject --no-restore -v:minimal
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = (Get-Command dotnet).Source
    $startInfo.Arguments = "`"$stubDll`""
    $startInfo.WorkingDirectory = $root
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $previousStubUrl = $env:CONTROLID_STUB_URL
    $env:CONTROLID_STUB_URL = $StubUrl
    try {
        $startedProcess = [Diagnostics.Process]::Start($startInfo)
    }
    finally {
        $env:CONTROLID_STUB_URL = $previousStubUrl
    }
    Wait-Ready

    [System.Net.ServicePointManager]::DefaultConnectionLimit = [Math]::Max(2, $ConcurrentRequests)
    $handler = [System.Net.Http.HttpClientHandler]::new()
    $handler.MaxConnectionsPerServer = [Math]::Max(2, $ConcurrentRequests)
    $client = [System.Net.Http.HttpClient]::new($handler)
    $client.BaseAddress = [Uri]$StubUrl
    $client.Timeout = [TimeSpan]::FromSeconds(30)
    $results = @()

    foreach ($datasetSize in $DatasetSizes) {
        $resetBody = @{ profile = 'idface'; datasetSize = $datasetSize } | ConvertTo-Json -Compress
        Invoke-RestMethod -Uri ($StubUrl.TrimEnd('/') + '/__stub/reset') -Method Post -ContentType 'application/json' -Body $resetBody -TimeoutSec 30 | Out-Null

        1..3 | ForEach-Object { Invoke-LoadObjects -Client $client -DatasetSize $datasetSize | Out-Null }
        $latencies = @()
        $responseBytes = 0L
        for ($sample = 0; $sample -lt $LatencySamples; $sample++) {
            $watch = [Diagnostics.Stopwatch]::StartNew()
            $responseBytes = Invoke-LoadObjects -Client $client -DatasetSize $datasetSize
            $watch.Stop()
            $latencies += $watch.Elapsed.TotalMilliseconds
        }

        $startedProcess.Refresh()
        $cpuBefore = $startedProcess.TotalProcessorTime
        $workingSetBefore = $startedProcess.WorkingSet64
        $throughputWatch = [Diagnostics.Stopwatch]::StartNew()
        $tasks = @()
        $offset = [Math]::Max(0, $datasetSize - 100)
        for ($requestIndex = 0; $requestIndex -lt $ConcurrentRequests; $requestIndex++) {
            $content = [System.Net.Http.StringContent]::new("{`"object`":`"users`",`"limit`":100,`"offset`":$offset}", [Text.Encoding]::UTF8, 'application/json')
            $tasks += $client.PostAsync('/load_objects.fcgi', $content)
        }
        [Threading.Tasks.Task]::WaitAll([Threading.Tasks.Task[]]$tasks)
        foreach ($task in $tasks) {
            $response = $task.Result
            if (-not $response.IsSuccessStatusCode) { throw "Falha na rajada concorrente: $([int]$response.StatusCode)." }
            $response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult() | Out-Null
            $response.Dispose()
        }
        $throughputWatch.Stop()
        $startedProcess.Refresh()

        $results += [pscustomobject]@{
            datasetSize = $datasetSize
            samples = $LatencySamples
            responseBytes = $responseBytes
            p50Ms = Get-Percentile -Values $latencies -Percentile 0.50
            p95Ms = Get-Percentile -Values $latencies -Percentile 0.95
            p99Ms = Get-Percentile -Values $latencies -Percentile 0.99
            requestsPerSecond = [Math]::Round($ConcurrentRequests / [Math]::Max(0.001, $throughputWatch.Elapsed.TotalSeconds), 2)
            cpuMilliseconds = [Math]::Round(($startedProcess.TotalProcessorTime - $cpuBefore).TotalMilliseconds, 2)
            workingSetMiB = [Math]::Round($startedProcess.WorkingSet64 / 1MB, 2)
            workingSetDeltaMiB = [Math]::Round(($startedProcess.WorkingSet64 - $workingSetBefore) / 1MB, 2)
        }
    }

    $client.Dispose()
    $handler.Dispose()
    $reportFullPath = Resolve-RepoPath $ReportPath
    $jsonFullPath = Resolve-RepoPath $JsonPath
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $reportFullPath) | Out-Null
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $jsonFullPath) | Out-Null
    $results | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $jsonFullPath -Encoding UTF8

    $lines = @(
        '# Linha de base de desempenho sem equipamento',
        '',
        "- Data UTC: $([DateTimeOffset]::UtcNow.ToString('O'))",
        "- Simulador: $StubUrl",
        "- Amostras sequenciais por massa: $LatencySamples",
        "- Requisicoes na rajada: $ConcurrentRequests",
        '- Consulta medida: ultima pagina de 100 usuarios, incluindo a varredura completa ate o deslocamento.',
        '',
        '| Massa | Resposta | p50 | p95 | p99 | req/s | CPU | Memoria | Delta |',
        '| ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |'
    )
    foreach ($result in $results) {
        $lines += "| $($result.datasetSize) | $($result.responseBytes) B | $($result.p50Ms) ms | $($result.p95Ms) ms | $($result.p99Ms) ms | $($result.requestsPerSecond) | $($result.cpuMilliseconds) ms | $($result.workingSetMiB) MiB | $($result.workingSetDeltaMiB) MiB |"
    }
    $lines += ''
    $lines += '- Orcamento local: p95 <= 1.000 ms e memoria do simulador <= 768 MiB.'
    $lines | Set-Content -LiteralPath $reportFullPath -Encoding UTF8
    $lines | ForEach-Object { Write-Host $_ }

    if ($FailOnBudget) {
        $violations = @($results | Where-Object { $_.p95Ms -gt 1000 -or $_.workingSetMiB -gt 768 })
        if ($violations.Count -gt 0) {
            throw "A linha de base excedeu o orcamento local de desempenho."
        }
    }
}
finally {
    if ($null -ne $startedProcess -and -not $startedProcess.HasExited) {
        Stop-Process -Id $startedProcess.Id -Force
    }
    Pop-Location
}
