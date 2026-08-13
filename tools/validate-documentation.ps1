[CmdletBinding()]
param(
    [int]$ExpectedMarkdownCount = 82,
    [int]$ExpectedMermaidCount = 41,
    [switch]$CheckExternalUrls
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$licenseRelativePath = "wwwroot/lib/jquery-validation/LICENSE.md"
$expectedLicenseSha256 = "81e1c4930fd618f75a1d0311ab91ee358f6c9e588dcd4ff2d8e5bcc9c9e1197c"
$strictUtf8 = [System.Text.UTF8Encoding]::new($false, $true)
$errors = [System.Collections.Generic.List[string]]::new()
$externalUrls = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$anchorCache = @{}
$incomingMarkdownLinks = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)

function Add-DocumentationError {
    param([Parameter(Mandatory = $true)][string]$Message)

    $errors.Add($Message)
}

function Get-NormalizedRelativePath {
    param([Parameter(Mandatory = $true)][string]$Path)

    $normalized = $Path.Replace("\", "/")
    while ($normalized.StartsWith("./", [StringComparison]::Ordinal)) {
        $normalized = $normalized.Substring(2)
    }
    return $normalized
}

function Test-IsGeneratedOrTemplatePath {
    param([Parameter(Mandatory = $true)][string]$Path)

    return $Path.StartsWith("artifacts/", [StringComparison]::OrdinalIgnoreCase) -or
        $Path.StartsWith("Logs/", [StringComparison]::OrdinalIgnoreCase) -or
        $Path.StartsWith("bin/", [StringComparison]::OrdinalIgnoreCase) -or
        $Path.StartsWith("obj/", [StringComparison]::OrdinalIgnoreCase) -or
        $Path.IndexOf("...", [StringComparison]::Ordinal) -ge 0 -or
        $Path.IndexOf("*", [StringComparison]::Ordinal) -ge 0 -or
        $Path.IndexOf("<", [StringComparison]::Ordinal) -ge 0 -or
        $Path.IndexOf("YYYY", [StringComparison]::OrdinalIgnoreCase) -ge 0 -or
        $Path.EndsWith("/AGENTS.md", [StringComparison]::OrdinalIgnoreCase)
}

function Get-MarkdownAnchors {
    param([Parameter(Mandatory = $true)][string]$Content)

    $anchors = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $occurrences = @{}
    $insideFence = $false

    foreach ($line in ($Content -split '\r?\n')) {
        if ($line -match '^\s*```') {
            $insideFence = -not $insideFence
            continue
        }
        if ($insideFence) {
            continue
        }

        foreach ($idMatch in [regex]::Matches($line, '<[^>]+\sid=["'']([^"'']+)["'']', [Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
            [void]$anchors.Add([Uri]::UnescapeDataString($idMatch.Groups[1].Value))
        }

        if ($line -notmatch '^\s{0,3}#{1,6}\s+(.+?)\s*#*\s*$') {
            continue
        }

        $heading = $Matches[1]
        $heading = [regex]::Replace($heading, '!\[([^\]]*)\]\([^)]*\)', '$1')
        $heading = [regex]::Replace($heading, '\[([^\]]+)\]\([^)]*\)', '$1')
        $heading = [regex]::Replace($heading, '<[^>]+>', '')
        $heading = [Net.WebUtility]::HtmlDecode($heading).Replace('`', '')
        $slug = $heading.ToLowerInvariant().Normalize([Text.NormalizationForm]::FormC)
        $slug = [regex]::Replace($slug, '[^\p{L}\p{M}\p{Nd}\s-]', '')
        $slug = [regex]::Replace($slug.Trim(), '\s+', '-')

        if ([string]::IsNullOrWhiteSpace($slug)) {
            continue
        }

        $baseSlug = $slug
        if ($occurrences.ContainsKey($baseSlug)) {
            $occurrences[$baseSlug]++
            $slug = "$baseSlug-$($occurrences[$baseSlug])"
        }
        else {
            $occurrences[$baseSlug] = 0
        }
        [void]$anchors.Add($slug)
    }

    return $anchors
}

function Test-ExternalUrlAvailable {
    param([Parameter(Mandatory = $true)][string]$Url)

    $acceptedRestrictedStatuses = @(401, 403, 405, 429)
    $curlCommand = Get-Command curl.exe -CommandType Application -ErrorAction SilentlyContinue
    if ($null -eq $curlCommand) {
        $curlCommand = Get-Command curl -CommandType Application -ErrorAction SilentlyContinue
    }
    if ($null -ne $curlCommand) {
        $sink = if ($env:OS -eq 'Windows_NT') { 'NUL' } else { '/dev/null' }
        foreach ($method in @('Head', 'Get')) {
            $curlArguments = @(
                '--silent', '--fail', '--location', '--connect-timeout', '5',
                '--max-time', '15', '--retry', '2', '--retry-delay', '1',
                '--retry-max-time', '20', '--output', $sink, '--write-out',
                '%{http_code}'
            )
            if ($method -eq 'Head') {
                $curlArguments += '--head'
            }
            else {
                $curlArguments += @('--range', '0-0')
            }
            $curlArguments += @('--', $Url)

            $statusOutput = @(& $curlCommand.Source @curlArguments 2>$null)
            $curlExitCode = $LASTEXITCODE
            $statusCode = 0
            [void][int]::TryParse(($statusOutput -join '').Trim(), [ref]$statusCode)
            Write-Verbose "External URL curl check: method=$method url=$Url exit=$curlExitCode status=$statusCode"
            if ($curlExitCode -eq 0 -or $acceptedRestrictedStatuses -contains $statusCode) {
                return $true
            }
        }

        return $false
    }

    if ([Enum]::GetNames([Net.SecurityProtocolType]) -contains 'Tls12') {
        [Net.ServicePointManager]::SecurityProtocol =
            [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12
    }

    foreach ($method in @('Head', 'Get')) {
        try {
            $parameters = @{
                Uri = $Url
                Method = $method
                MaximumRedirection = 5
                TimeoutSec = 20
                UseBasicParsing = $true
                UserAgent = 'IntegracaoControlID-DocumentationValidator/1.0'
                ErrorAction = 'Stop'
            }
            if ($method -eq 'Get') {
                $parameters['Headers'] = @{ Range = 'bytes=0-0' }
            }
            [void](Invoke-WebRequest @parameters)
            return $true
        }
        catch {
            $response = if ($_.Exception.PSObject.Properties.Name -contains 'Response') {
                $_.Exception.Response
            }
            else {
                $null
            }
            if ($null -ne $response) {
                $statusCode = [int]$response.StatusCode
                if ($acceptedRestrictedStatuses -contains $statusCode) {
                    return $true
                }
                if ($method -eq 'Get') {
                    return $false
                }
            }
            elseif ($method -eq 'Get') {
                return $false
            }
        }
    }

    return $false
}

Push-Location $root
try {
    $tracked = @(& git ls-files -- "*.md")
    if ($LASTEXITCODE -ne 0) {
        throw "git ls-files failed while listing Markdown files."
    }

    $untracked = @(& git ls-files --others --exclude-standard -- "*.md")
    if ($LASTEXITCODE -ne 0) {
        throw "git ls-files failed while listing untracked Markdown files."
    }

    $markdownFiles = @($tracked + $untracked | Sort-Object -Unique)
    if ($markdownFiles.Count -ne $ExpectedMarkdownCount) {
        Add-DocumentationError "Expected $ExpectedMarkdownCount Markdown files, found $($markdownFiles.Count)."
    }

    $mermaidCount = 0
    $markdownWithMermaid = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $allowedMermaidTypes = @('flowchart', 'sequenceDiagram', 'stateDiagram-v2', 'classDiagram', 'erDiagram')

    foreach ($markdownPathValue in $markdownFiles) {
        $markdownPath = Get-NormalizedRelativePath $markdownPathValue
        if (-not $markdownPath.StartsWith("docs/", [StringComparison]::OrdinalIgnoreCase) -or
            $markdownPath.Equals("docs/README.md", [StringComparison]::OrdinalIgnoreCase)) {
            continue
        }

        $documentDirectory = Split-Path -Parent $markdownPath
        $documentName = Split-Path -Leaf $markdownPath
        if ($documentName.Equals('README.md', [StringComparison]::OrdinalIgnoreCase)) {
            $parentDirectory = Split-Path -Parent $documentDirectory
            $indexPath = "$parentDirectory/README.md"
            $indexNeedle = "$(Split-Path -Leaf $documentDirectory)/README.md"
        }
        else {
            $indexPath = "$documentDirectory/README.md"
            $indexNeedle = $documentName
        }

        $indexFullPath = Join-Path $root ($indexPath.Replace('/', [IO.Path]::DirectorySeparatorChar))
        if (-not (Test-Path -LiteralPath $indexFullPath)) {
            Add-DocumentationError "Missing domain index for documentation file: $markdownPath -> $indexPath"
            continue
        }

        $indexContent = [IO.File]::ReadAllText($indexFullPath, $strictUtf8)
        if ($indexContent.IndexOf($indexNeedle, [StringComparison]::OrdinalIgnoreCase) -lt 0) {
            Add-DocumentationError "Documentation file missing from domain index: $markdownPath -> $indexPath"
        }
    }

    foreach ($relativePathValue in $markdownFiles) {
        $relativePath = Get-NormalizedRelativePath $relativePathValue
        $fullPath = Join-Path $root ($relativePath.Replace("/", [IO.Path]::DirectorySeparatorChar))
        $bytes = [IO.File]::ReadAllBytes($fullPath)
        $offset = if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) { 3 } else { 0 }

        try {
            $content = $strictUtf8.GetString($bytes, $offset, $bytes.Length - $offset)
        }
        catch {
            Add-DocumentationError "Invalid UTF-8: $relativePath"
            continue
        }

        $anchorCache[$fullPath] = Get-MarkdownAnchors $content

        if ($content -match '\u00c3[\u0080-\u00bf]|\u00c2[\u0080-\u00bf]|\u00e2\u20ac|\u00ef\u00bf\u00bd|\ufffd') {
            Add-DocumentationError "Possible mojibake: $relativePath"
        }

        $fenceCount = ([regex]::Matches($content, '(?m)^\s*```')).Count
        if (($fenceCount % 2) -ne 0) {
            Add-DocumentationError "Unbalanced fenced code block: $relativePath"
        }

        $mermaidMatches = [regex]::Matches($content, '(?ms)^\s*```mermaid\s*\r?\n(?<body>.*?)^\s*```\s*$')
        foreach ($mermaidMatch in $mermaidMatches) {
            $mermaidCount++
            [void]$markdownWithMermaid.Add($relativePath)
            $firstStatement = @($mermaidMatch.Groups['body'].Value -split '\r?\n' |
                Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
                Select-Object -First 1)
            if ($firstStatement.Count -eq 0) {
                Add-DocumentationError "Empty Mermaid diagram: $relativePath"
                continue
            }

            $mermaidType = ($firstStatement[0].Trim() -split '\s+')[0]
            if ($mermaidType -notin $allowedMermaidTypes) {
                Add-DocumentationError "Unsupported Mermaid diagram type in ${relativePath}: $mermaidType"
            }
        }

        if ($relativePath -ne $licenseRelativePath) {
            $firstLines = @($content -split '\r?\n' | Select-Object -First 8)
            $header = $firstLines -join "`n"
            if ($firstLines.Count -eq 0 -or $firstLines[0] -notmatch '^#\s+\S') {
                Add-DocumentationError "Missing initial H1: $relativePath"
            }
            if ($header -notmatch '(?m)^> \*\*.+\*\*' -or
                $header -notmatch 'P\u00fablico:' -or
                $header -notmatch 'Respons\u00e1vel:' -or
                $header -notmatch '(\u00daltima valida\u00e7\u00e3o(?: t\u00e9cnica)?|Refer\u00eancia temporal|Execu\u00e7\u00e3o):') {
                Add-DocumentationError "Incomplete document metadata: $relativePath"
            }
        }

        foreach ($match in [regex]::Matches($content, '(?m)!?\[[^\]]*\]\(([^)]+)\)')) {
            $target = $match.Groups[1].Value.Trim().Trim('<', '>')
            if ([string]::IsNullOrWhiteSpace($target)) {
                continue
            }

            if ($target -match '^https?://') {
                $uri = $null
                if (-not [Uri]::TryCreate($target, [UriKind]::Absolute, [ref]$uri)) {
                    Add-DocumentationError "Invalid URL in ${relativePath}: $target"
                }
                else {
                    [void]$externalUrls.Add($target)
                }
                continue
            }

            if ($target -match '^(mailto:|/)' ) {
                continue
            }

            $target = ($target -split '\s+"')[0]
            $targetParts = $target -split '#', 2
            $pathPart = ($targetParts[0] -split '\?')[0]
            $fragment = if ($targetParts.Count -eq 2) { [Uri]::UnescapeDataString($targetParts[1]) } else { $null }
            $candidate = $fullPath
            if (-not [string]::IsNullOrWhiteSpace($pathPart)) {
                $decodedTarget = [Uri]::UnescapeDataString($pathPart).Replace("/", [IO.Path]::DirectorySeparatorChar)
                $candidate = Join-Path (Split-Path -Parent $fullPath) $decodedTarget
            }
            $candidate = [IO.Path]::GetFullPath($candidate)
            if (-not (Test-Path -LiteralPath $candidate)) {
                Add-DocumentationError "Missing local Markdown link in ${relativePath}: $target"
                continue
            }

            if ([IO.Path]::GetExtension($candidate).Equals('.md', [StringComparison]::OrdinalIgnoreCase) -and
                -not $candidate.Equals($fullPath, [StringComparison]::OrdinalIgnoreCase) -and
                $candidate.StartsWith($root, [StringComparison]::OrdinalIgnoreCase)) {
                $linkedRelativePath = $candidate.Substring($root.Length).TrimStart([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar).Replace('\', '/')
                [void]$incomingMarkdownLinks.Add($linkedRelativePath)
            }

            if (-not [string]::IsNullOrWhiteSpace($fragment) -and [IO.Path]::GetExtension($candidate) -eq '.md') {
                if (-not $anchorCache.ContainsKey($candidate)) {
                    $targetContent = [IO.File]::ReadAllText($candidate, $strictUtf8)
                    $anchorCache[$candidate] = Get-MarkdownAnchors $targetContent
                }
                if (-not $anchorCache[$candidate].Contains($fragment)) {
                    Add-DocumentationError "Missing local Markdown anchor in ${relativePath}: $target"
                }
            }
        }

        foreach ($match in [regex]::Matches($content, 'https?://[^\s<>()`"'']+')) {
            $url = $match.Value.TrimEnd('.', ',', ';', ':')
            if ($url -match '^https?://\+:\d+') {
                continue
            }

            $uri = $null
            if (-not [Uri]::TryCreate($url, [UriKind]::Absolute, [ref]$uri)) {
                Add-DocumentationError "Invalid bare URL in ${relativePath}: $url"
                continue
            }

            if ($uri.Host -notin @('localhost', '127.0.0.1', '0.0.0.0', '::1') -and
                -not $uri.Host.EndsWith('.local', [StringComparison]::OrdinalIgnoreCase) -and
                -not $uri.Host.EndsWith('.invalid', [StringComparison]::OrdinalIgnoreCase)) {
                [void]$externalUrls.Add($url)
            }
        }

        foreach ($match in [regex]::Matches($content, '`((?:docs|tools|Services|tests|wwwroot|Data|Models|Views|Controllers|Options|Middlewares|Helpers|Logging|Mappings|ViewModels|Properties|\.github)/[A-Za-z0-9_./*<>-]+)`')) {
            $referencedPath = Get-NormalizedRelativePath $match.Groups[1].Value
            if (Test-IsGeneratedOrTemplatePath $referencedPath) {
                continue
            }

            $candidate = Join-Path $root ($referencedPath.Replace("/", [IO.Path]::DirectorySeparatorChar))
            if (-not (Test-Path -LiteralPath $candidate)) {
                Add-DocumentationError "Missing repository path in ${relativePath}: $referencedPath"
            }
        }

        foreach ($match in [regex]::Matches($content, '`([^`\r\n]+\.md(?:#[^`\r\n]+)?)`')) {
            $beforeIndex = $match.Index - 1
            $afterIndex = $match.Index + $match.Length
            $isClickableCodeLabel = $beforeIndex -ge 0 -and $content[$beforeIndex] -eq '[' -and
                $afterIndex + 1 -lt $content.Length -and $content.Substring($afterIndex, 2) -eq ']('
            if ($isClickableCodeLabel) {
                continue
            }

            $referencedMarkdown = ($match.Groups[1].Value -split '#', 2)[0].Replace('\', '/')
            $normalizedMarkdown = Get-NormalizedRelativePath $referencedMarkdown
            if (Test-IsGeneratedOrTemplatePath $normalizedMarkdown) {
                continue
            }

            $rootRelativeReference = $normalizedMarkdown.StartsWith('docs/', [StringComparison]::OrdinalIgnoreCase) -or
                $normalizedMarkdown.StartsWith('tools/', [StringComparison]::OrdinalIgnoreCase) -or
                $normalizedMarkdown.StartsWith('Services/', [StringComparison]::OrdinalIgnoreCase) -or
                $normalizedMarkdown.StartsWith('wwwroot/', [StringComparison]::OrdinalIgnoreCase) -or
                $normalizedMarkdown -in @('README.md', 'AGENTS.md', 'CONTRIBUTING.md', 'SECURITY.md', 'SUPPORT.md')
            $referenceBase = if ($rootRelativeReference) { $root } else { Split-Path -Parent $fullPath }
            $referencedFullPath = [IO.Path]::GetFullPath((Join-Path $referenceBase $normalizedMarkdown.Replace('/', [IO.Path]::DirectorySeparatorChar)))
            if (Test-Path -LiteralPath $referencedFullPath -PathType Leaf) {
                Add-DocumentationError "Existing Markdown reference must be a clickable link in ${relativePath}: $referencedMarkdown"
            }
        }
    }

    if ($mermaidCount -ne $ExpectedMermaidCount) {
        Add-DocumentationError "Expected $ExpectedMermaidCount Mermaid diagrams, found $mermaidCount."
    }

    $indexedIncomingMarkdownCount = 0
    foreach ($markdownPathValue in $markdownFiles) {
        $markdownPath = Get-NormalizedRelativePath $markdownPathValue
        if (-not $incomingMarkdownLinks.Contains($markdownPath)) {
            Add-DocumentationError "Orphan Markdown document without incoming link: $markdownPath"
        }
        else {
            $indexedIncomingMarkdownCount++
        }
    }

    $licenseHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $root $licenseRelativePath)).Hash.ToLowerInvariant()
    if ($licenseHash -ne $expectedLicenseSha256) {
        Add-DocumentationError "Vendored jquery-validation license hash changed unexpectedly."
    }

    $mapPath = Join-Path $root 'docs/arquitetura/project-file-responsibilities.md'
    $mapContent = [IO.File]::ReadAllText($mapPath, $strictUtf8)
    $inventoryGeneratorPath = Join-Path $root 'tools/generate-source-inventory.ps1'
    if (-not (Test-Path -LiteralPath $inventoryGeneratorPath -PathType Leaf)) {
        Add-DocumentationError 'Missing source inventory generator: tools/generate-source-inventory.ps1'
    }
    else {
        $inventoryGeneratorContent = [IO.File]::ReadAllText($inventoryGeneratorPath, $strictUtf8)
        foreach ($requiredInventoryContract in @('git -C $root ls-files', 'artifacts/documentation/source-inventory.md')) {
            if ($inventoryGeneratorContent.IndexOf($requiredInventoryContract, [StringComparison]::Ordinal) -lt 0) {
                Add-DocumentationError "Source inventory generator is missing contract: $requiredInventoryContract"
            }
        }
    }
    if ($mapContent.IndexOf('generate-source-inventory.ps1', [StringComparison]::OrdinalIgnoreCase) -lt 0) {
        Add-DocumentationError 'Architecture map does not explain the generated source inventory.'
    }

    $acceptancePath = Join-Path $root 'docs/produto/product-acceptance-criteria.md'
    $acceptanceContent = [IO.File]::ReadAllText($acceptancePath, [Text.Encoding]::UTF8)
    foreach ($number in 1..9) {
        $requirementId = 'REQ-{0:D3}' -f $number
        $headingMatches = [regex]::Matches($acceptanceContent, "(?m)^###\s+$requirementId\s+-")
        if ($headingMatches.Count -ne 1) {
            Add-DocumentationError "Requirement heading must occur exactly once: $requirementId"
        }

        $rowMatches = @([regex]::Matches($acceptanceContent, "(?m)^\|\s*$requirementId\s*\|.*$") |
            Where-Object { $_.Value -match '`tests/Integracao\.ControlID\.PoC\.Tests/' })
        if ($rowMatches.Count -ne 1) {
            Add-DocumentationError "Requirement traceability row must occur exactly once: $requirementId"
            continue
        }

        $testPaths = @([regex]::Matches($rowMatches[0].Value, '`(tests/Integracao\.ControlID\.PoC\.Tests/[^`]+\.cs)`') |
            ForEach-Object { $_.Groups[1].Value } |
            Sort-Object -Unique)
        if ($testPaths.Count -eq 0) {
            Add-DocumentationError "Requirement traceability row has no test evidence: $requirementId"
        }
        foreach ($testPath in $testPaths) {
            if (-not (Test-Path -LiteralPath (Join-Path $root $testPath.Replace('/', [IO.Path]::DirectorySeparatorChar)))) {
                Add-DocumentationError "Requirement traceability references a missing test: $requirementId -> $testPath"
            }
        }
    }

    if ($CheckExternalUrls) {
        $sortedExternalUrls = @($externalUrls | Sort-Object)
        $urlsToRetry = $sortedExternalUrls
        $curlCommand = Get-Command curl.exe -CommandType Application -ErrorAction SilentlyContinue
        if ($null -eq $curlCommand) {
            $curlCommand = Get-Command curl -CommandType Application -ErrorAction SilentlyContinue
        }

        if ($null -ne $curlCommand -and $sortedExternalUrls.Count -gt 0) {
            $sink = if ($env:OS -eq 'Windows_NT') { 'NUL' } else { '/dev/null' }
            $curlArguments = @(
                '--silent', '--location', '--connect-timeout', '5',
                '--max-time', '15', '--retry', '1', '--retry-delay', '1',
                '--range', '0-0', '--max-filesize', '1048576', '--write-out',
                "%{urlnum}`t%{http_code}`n"
            )
            foreach ($url in $sortedExternalUrls) {
                $curlArguments += @('--output', $sink, '--url', $url)
            }

            $batchOutput = @(& $curlCommand.Source @curlArguments 2>$null)
            $statusesByIndex = @{}
            foreach ($line in $batchOutput) {
                if ($line -match '^(\d+)\s+(\d{3})$') {
                    $statusesByIndex[[int]$Matches[1]] = [int]$Matches[2]
                }
            }

            $retryList = [System.Collections.Generic.List[string]]::new()
            foreach ($index in 0..($sortedExternalUrls.Count - 1)) {
                $statusCode = if ($statusesByIndex.ContainsKey($index)) { $statusesByIndex[$index] } else { 0 }
                if (($statusCode -lt 200 -or $statusCode -ge 400) -and
                    $statusCode -notin @(401, 403, 405, 429)) {
                    $retryList.Add($sortedExternalUrls[$index])
                }
            }
            $urlsToRetry = @($retryList)
        }

        foreach ($url in $urlsToRetry) {
            $available = $false
            foreach ($attempt in 1..3) {
                if (Test-ExternalUrlAvailable $url) {
                    $available = $true
                    break
                }
                Start-Sleep -Seconds ([Math]::Pow(2, $attempt - 1))
            }

            if (-not $available) {
                Add-DocumentationError "External URL unavailable: $url"
            }
        }
    }

    if ($errors.Count -gt 0) {
        $errors | ForEach-Object { Write-Error $_ }
        exit 1
    }

    Write-Output "Documentation validation passed."
    Write-Output "Markdown files: $($markdownFiles.Count)"
    Write-Output "Authored documents with metadata: $($markdownFiles.Count - 1)"
    Write-Output "Indexed documents with incoming links: $indexedIncomingMarkdownCount"
    Write-Output "Mermaid diagrams: $mermaidCount in $($markdownWithMermaid.Count) documents"
    Write-Output "Source inventory generator: validated"
    Write-Output "Requirement traceability rows: 9"
    Write-Output "External URLs checked: $(if ($CheckExternalUrls) { $externalUrls.Count } else { 0 })"
    Write-Output "Vendored license SHA-256: $licenseHash"
}
finally {
    Pop-Location
}
