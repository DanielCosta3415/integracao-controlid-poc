[CmdletBinding()]
param(
    [string]$AppUrl = "http://localhost:5000",
    [string]$StubUrl = "http://127.0.0.1:6600",
    [string]$ReportPath = ".\docs\reports\localhost-smoke-test-2026-04-13.md"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

$root = Split-Path -Parent $PSScriptRoot
$appProject = Join-Path $root "Integracao.ControlID.PoC.csproj"
$stubProject = Join-Path $root "tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj"
$artifactsDir = Join-Path $root "artifacts\smoke"
$reportFullPath = Join-Path $root ($ReportPath -replace '^[.][\\/]', '')

if (-not (Test-Path $artifactsDir)) {
    New-Item -ItemType Directory -Force -Path $artifactsDir | Out-Null
}

$samplePngPath = Join-Path $artifactsDir "sample.png"
$sampleWavPath = Join-Path $artifactsDir "sample.wav"
$sampleMp4Path = Join-Path $artifactsDir "sample.mp4"
$samplePemPath = Join-Path $artifactsDir "sample.pem"
$sampleBinPath = Join-Path $artifactsDir "sample.bin"

[IO.File]::WriteAllBytes($samplePngPath, [Convert]::FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aR8kAAAAASUVORK5CYII="))
[IO.File]::WriteAllBytes($sampleWavPath, [Text.Encoding]::ASCII.GetBytes("RIFFSTUBWAVEfmt data"))
[IO.File]::WriteAllBytes($sampleMp4Path, [Text.Encoding]::UTF8.GetBytes("FAKE-MP4-DATA"))
[IO.File]::WriteAllText($samplePemPath, "-----BEGIN CERTIFICATE-----`nU1RVQi1DRVJULURBVEE=`n-----END CERTIFICATE-----`n")
[IO.File]::WriteAllBytes($sampleBinPath, [Text.Encoding]::UTF8.GetBytes("stub-biometry"))

$samplePngBase64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($samplePngPath))
$sampleWavBase64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($sampleWavPath))
$sampleMp4Base64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($sampleMp4Path))
$samplePemBase64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($samplePemPath))
$sampleBinBase64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($sampleBinPath))

$results = [System.Collections.Generic.List[object]]::new()
$processes = [System.Collections.Generic.List[System.Diagnostics.Process]]::new()
$webSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession

function Add-Result {
    param(
        [string]$Phase,
        [string]$Target,
        [string]$Status,
        [string]$Detail
    )

    $results.Add([pscustomobject]@{
        Phase  = $Phase
        Target = $Target
        Status = $Status
        Detail = $Detail
    })
}

function Wait-HttpEndpoint {
    param(
        [string]$Url,
        [int]$Attempts = 60
    )

    for ($attempt = 1; $attempt -le $Attempts; $attempt++) {
        try {
            Invoke-RestMethod -Uri $Url -Method Get -TimeoutSec 2 | Out-Null
            return
        }
        catch {
            if ($attempt -eq 1) {
                Write-Host "Wait-HttpEndpoint first failure for ${Url}: $($_.Exception.Message)"
            }
            Start-Sleep -Milliseconds 500
        }
    }

    throw "Endpoint não respondeu a tempo: $Url"
}

function Parse-Attributes {
    param([string]$Markup)

    $attributes = @{}
    $regex = [regex]'(?<name>[\w:-]+)\s*=\s*(?:"(?<value>[^"]*)"|''(?<value>[^'']*)'')'
    foreach ($match in $regex.Matches($Markup)) {
        $attributes[$match.Groups["name"].Value] = [System.Net.WebUtility]::HtmlDecode($match.Groups["value"].Value)
    }

    return $attributes
}

function Get-HtmlForms {
    param([string]$Html)

    $forms = @()
    $formRegex = [regex]'<form\b(?<attrs>[^>]*)>(?<inner>[\s\S]*?)</form>'
    foreach ($formMatch in $formRegex.Matches($Html)) {
        $attrs = Parse-Attributes $formMatch.Groups["attrs"].Value
        $inner = $formMatch.Groups["inner"].Value
        $fields = @()

        foreach ($inputMatch in ([regex]'<input\b(?<attrs>[^>]*)/?>').Matches($inner)) {
            $inputAttrs = Parse-Attributes $inputMatch.Groups["attrs"].Value
            if (-not $inputAttrs.ContainsKey("name")) {
                continue
            }

            $fields += [pscustomobject]@{
                Name    = $inputAttrs["name"]
                Type    = if ($inputAttrs.ContainsKey("type")) { $inputAttrs["type"].ToLowerInvariant() } else { "text" }
                Value   = if ($inputAttrs.ContainsKey("value")) { $inputAttrs["value"] } else { "" }
                Options = @()
            }
        }

        foreach ($textAreaMatch in ([regex]'<textarea\b(?<attrs>[^>]*)>(?<value>[\s\S]*?)</textarea>').Matches($inner)) {
            $areaAttrs = Parse-Attributes $textAreaMatch.Groups["attrs"].Value
            if (-not $areaAttrs.ContainsKey("name")) {
                continue
            }

            $fields += [pscustomobject]@{
                Name    = $areaAttrs["name"]
                Type    = "textarea"
                Value   = [System.Net.WebUtility]::HtmlDecode($textAreaMatch.Groups["value"].Value)
                Options = @()
            }
        }

        foreach ($selectMatch in ([regex]'<select\b(?<attrs>[^>]*)>(?<inner>[\s\S]*?)</select>').Matches($inner)) {
            $selectAttrs = Parse-Attributes $selectMatch.Groups["attrs"].Value
            if (-not $selectAttrs.ContainsKey("name")) {
                continue
            }

            $options = @()
            foreach ($optionMatch in ([regex]'<option\b(?<attrs>[^>]*)>(?<text>[\s\S]*?)</option>').Matches($selectMatch.Groups["inner"].Value)) {
                $optionAttrs = Parse-Attributes $optionMatch.Groups["attrs"].Value
                $options += [pscustomobject]@{
                    Value    = if ($optionAttrs.ContainsKey("value")) { $optionAttrs["value"] } else { "" }
                    Selected = $optionMatch.Groups["attrs"].Value -match 'selected'
                }
            }

            $selected = $options | Where-Object { $_.Selected } | Select-Object -First 1
            if (-not $selected) {
                $selected = $options | Where-Object { -not [string]::IsNullOrWhiteSpace($_.Value) } | Select-Object -First 1
            }

            $fields += [pscustomobject]@{
                Name    = $selectAttrs["name"]
                Type    = "select"
                Value   = if ($selected) { $selected.Value } else { "" }
                Options = $options
            }
        }

        $forms += [pscustomobject]@{
            Action = if ($attrs.ContainsKey("action")) { $attrs["action"] } else { "" }
            Method = if ($attrs.ContainsKey("method")) { $attrs["method"].ToUpperInvariant() } else { "GET" }
            Fields = $fields
        }
    }

    return $forms
}

function Resolve-ActionUrl {
    param(
        [string]$PageUrl,
        [string]$Action
    )

    if ([string]::IsNullOrWhiteSpace($Action)) {
        return $PageUrl
    }

    return ([Uri]::new([Uri]$PageUrl, $Action)).AbsoluteUri
}

function Get-SmokeFieldValue {
    param(
        [pscustomobject]$Field,
        [hashtable]$Context
    )

    $name = $Field.Name
    $type = $Field.Type
    $value = $Field.Value

    if ($type -eq "hidden" -and -not [string]::IsNullOrWhiteSpace($value)) {
        return $value
    }

    if ($name -eq "__RequestVerificationToken") {
        return $value
    }

    switch -Regex ($name) {
        'DeviceAddress' { return $Context.DeviceAddress }
        'User(name)?|Login' { return 'admin' }
        'Password|ConfirmPassword' { return 'admin' }
        '^Name$' { return 'Smoke User' }
        'Registration' { return '9001' }
        'Email' { return 'smoke@example.com' }
        'Phone' { return '5511999999999' }
        'Status' { if ([string]::IsNullOrWhiteSpace($value)) { return 'active' } else { return $value } }
        '^Id$|UserId|GroupId|DeviceId|ReaderId|PortalId|Door' { if ([string]::IsNullOrWhiteSpace($value)) { return '1' } else { return $value } }
        'Event' { if ([string]::IsNullOrWhiteSpace($value)) { return '7' } else { return $value } }
        'ActionName' { if ([string]::IsNullOrWhiteSpace($value)) { return 'door' } else { return $value } }
        'ActionParameters' { if ([string]::IsNullOrWhiteSpace($value)) { return 'door=1' } else { return $value } }
        'Message' { return 'Smoke message' }
        'ConnectionHost|PingHost|NslookupHost|Host' { return '127.0.0.1' }
        'ConnectionPort|Port' { if ([string]::IsNullOrWhiteSpace($value)) { return '80' } else { return $value } }
        'ChunkSizeKb' { return '64' }
        'Type' { if ([string]::IsNullOrWhiteSpace($value)) { return 'face' } else { return $value } }
        'BeginTime|EndTime' { return '2026-04-13T10:30' }
        'GetPayload' { if ([string]::IsNullOrWhiteSpace($value)) { return '{"general":{},"video_player":{},"monitor":{},"sec_box":{}}' } else { return $value } }
        'SetPayload' { if ([string]::IsNullOrWhiteSpace($value)) { return '{"general":{"show_logo":"1"}}' } else { return $value } }
        'RequestBody' { if ([string]::IsNullOrWhiteSpace($value)) { return '{}' } else { return $value } }
        'AdditionalQuery' { if ([string]::IsNullOrWhiteSpace($value)) { return '' } else { return $value } }
    }

    switch ($type) {
        "checkbox" { return "true" }
        "select" { return $value }
        "number" { if ([string]::IsNullOrWhiteSpace($value)) { return "1" } else { return $value } }
        "datetime-local" { return "2026-04-13T10:30" }
        "date" { return "2026-04-13" }
        "textarea" { if ([string]::IsNullOrWhiteSpace($value)) { return "{}" } else { return $value } }
        default { return $value }
    }
}

function Get-FileForField {
    param([pscustomobject]$Field)

    switch -Regex ($Field.Name) {
        'SipAudio|AccessAudio' { return Get-Item $sampleWavPath }
        'BatchFiles|TestFile|Photo|Image|Logo' { return Get-Item $samplePngPath }
        'Video' { return Get-Item $sampleMp4Path }
        'Certificate|Pem|Vpn' { return Get-Item $samplePemPath }
        'Biometr|Template|File' { return Get-Item $sampleBinPath }
    }

    return $null
}

function Submit-FormsFromPage {
    param(
        [string]$Path,
        [string]$Phase
    )

    $pageUrl = [Uri]::new([Uri]$AppUrl, $Path).AbsoluteUri
    try {
        $response = Invoke-WebRequest -Uri $pageUrl -WebSession $webSession -Method Get -UseBasicParsing
        Add-Result $Phase $Path "PASS" "GET $($response.StatusCode)"
    }
    catch {
        Add-Result $Phase $Path "FAIL" $_.Exception.Message
        return
    }

    $forms = Get-HtmlForms $response.Content
    $context = @{
        DeviceAddress = $StubUrl
    }

    foreach ($form in $forms) {
        if ($form.Method -ne "POST") {
            continue
        }

        $hasFile = $form.Fields | Where-Object { $_.Type -eq "file" } | Select-Object -First 1
        if ($hasFile) {
            Add-Result $Phase $Path "SKIP" "Formulário com upload binário foi coberto pela trilha do catálogo oficial."
            continue
        }

        $payload = @{}
        foreach ($field in $form.Fields) {
            $payload[$field.Name] = Get-SmokeFieldValue -Field $field -Context $context
        }

        $targetUrl = Resolve-ActionUrl -PageUrl $pageUrl -Action $form.Action

        if ($targetUrl -like "$AppUrl/Session/Clear*") {
            Add-Result $Phase $targetUrl "SKIP" "Sessão não é limpa durante o smoke para preservar o contexto dos demais fluxos."
            continue
        }

        try {
            $postResponse = Invoke-WebRequest -Uri $targetUrl -WebSession $webSession -Method Post -Body $payload -UseBasicParsing
            Add-Result $Phase $targetUrl "PASS" "POST $($postResponse.StatusCode)"
        }
        catch {
            Add-Result $Phase $targetUrl "FAIL" $_.Exception.Message
        }
    }
}

function Invoke-OfficialCatalog {
    $catalogUrl = [Uri]::new([Uri]$AppUrl, "/OfficialApi").AbsoluteUri
    $catalog = Invoke-WebRequest -Uri $catalogUrl -WebSession $webSession -Method Get -UseBasicParsing
    Add-Result "OfficialApi" "/OfficialApi" "PASS" "GET $($catalog.StatusCode)"

    $endpointIds = [regex]::Matches($catalog.Content, 'data-endpoint-id="(?<id>[^"]+)"') |
        ForEach-Object { $_.Groups["id"].Value } |
        Sort-Object -Unique

    if (-not $endpointIds) {
        $endpointIds = [regex]::Matches($catalog.Content, 'OfficialApi/Invoke\?id=(?<id>[^"&]+)') |
            ForEach-Object { $_.Groups["id"].Value } |
            Sort-Object -Unique
    }

    foreach ($endpointId in $endpointIds) {
        $invokeUrl = [Uri]::new([Uri]$catalogUrl, "/OfficialApi/Invoke?id=$endpointId").AbsoluteUri
        $invokePage = Invoke-WebRequest -Uri $invokeUrl -WebSession $webSession -Method Get -UseBasicParsing
        $forms = Get-HtmlForms $invokePage.Content
        $form = $forms | Select-Object -First 1

        if (-not $form) {
            Add-Result "OfficialApi" $invokeUrl "SKIP" "Callback local sem formulário de invocação manual."
            continue
        }

        $payload = @{}
        foreach ($field in $form.Fields) {
            $payload[$field.Name] = $field.Value
        }

        $endpointId = $payload["EndpointId"]
        $payload["DeviceAddress"] = $StubUrl
        $payload["SessionString"] = "stub-session"

        switch ($endpointId) {
            "alarm-status" { $payload["RequestBody"] = '{"stop":false}' }
            "change-idcloud-code" { $payload["RequestBody"] = "" }
            "export-objects" { $payload["RequestBody"] = '{"object":"users"}' }
            "create-or-modify-objects" { $payload["RequestBody"] = '{"object":"areas","values":[{"id":1,"name":"Lobby Smoke"}]}' }
            "report-generate" { $payload["RequestBody"] = '{"object":"users","order":["ascending","name"],"where":{"users":{},"groups":{},"time_zones":{}},"delimiter":";","line_break":"\r\n","header":"Name (User);Id (User)","file_name":"","join":"LEFT","columns":[{"type":"object_field","object":"users","field":"name"},{"type":"object_field","object":"users","field":"id"}]}' }
            "export-afd" { $payload["AdditionalQuery"] = "mode=671"; $payload["RequestBody"] = '{"initial_nsr":1}' }
            "export-audit-logs" { $payload["RequestBody"] = '{"config":1,"api":1,"usb":1,"network":1,"time":1,"online":1,"menu":1}' }
            "upgrade-idface-pro" { $payload["RequestBody"] = '{"password":"ABCDE12345"}' }
            "upgrade-idflex-enterprise" { $payload["RequestBody"] = '{"password":"ABCDE12345"}' }
            "set-pjsip-audio-message" { $payload["AdditionalQuery"] = "current=1&total=1"; $payload["RequestBody"] = $sampleWavBase64 }
            "make-sip-call" { $payload["RequestBody"] = '{"target":"503"}' }
            "set-audio-access-message" { $payload["AdditionalQuery"] = "event=authorized&current=1&total=1"; $payload["RequestBody"] = $sampleWavBase64 }
            "get-audio-access-message" { $payload["RequestBody"] = '{"event":"authorized"}' }
            "set-network-interlock" { $payload["RequestBody"] = '{"interlock_enabled":1,"api_bypass_enabled":0,"rex_bypass_enabled":0}' }
            "user-get-image" { $payload["AdditionalQuery"] = "user_id=1" }
            "user-get-image-list" { $payload["RequestBody"] = '{"user_ids":[1]}' }
            "logo-get" { $payload["AdditionalQuery"] = "id=1" }
            "logo-change" { $payload["AdditionalQuery"] = "id=1"; $payload["RequestBody"] = $samplePngBase64 }
            "logo-destroy" { $payload["AdditionalQuery"] = "id=1" }
            "send-video" { $payload["AdditionalQuery"] = "current=1&total=1"; $payload["RequestBody"] = $sampleMp4Base64 }
            "save-screenshot" { $payload["RequestBody"] = '{"frame_type":"camera","camera":"rgb"}' }
            "set-vpn-file" { $payload["AdditionalQuery"] = "file_type=config"; $payload["RequestBody"] = $samplePemBase64 }
            "ssl-certificate-change" { $payload["RequestBody"] = $samplePemBase64 }
            "user-set-image-list" { $payload["RequestBody"] = ('{"match":false,"user_images":[{"user_id":1,"timestamp":1713000000,"image":"' + $samplePngBase64 + '"}]}') }
            "user-test-image" { $payload["RequestBody"] = $samplePngBase64 }
            "remote-led-control" { $payload["RequestBody"] = '{"sec_box":{"color":"4278255360","event":4}}' }
            "validate-biometry" { $payload["RequestBody"] = $sampleBinBase64 }
            "user-set-image" {
                $payload["RequestBody"] = (@{
                    fields = @{ user_id = "1" }
                    files  = @(@{
                            name          = "image"
                            fileName      = "sample.png"
                            contentType   = "image/png"
                            base64Content = $samplePngBase64
                        })
                } | ConvertTo-Json -Depth 5)
            }
        }

        if (-not $payload.ContainsKey("RequestBody") -or [string]::IsNullOrWhiteSpace([string]$payload["RequestBody"])) {
            $payload["RequestBody"] = "{}"
        }

        try {
            $postResponse = Invoke-WebRequest -Uri $invokeUrl -WebSession $webSession -Method Post -Body $payload -UseBasicParsing
            $status = if ($postResponse.Content -match 'bg-danger') { "FAIL" } else { "PASS" }
            $detail = if ($status -eq "PASS") { "Invocação concluída." } else { "Página retornou resultado de erro." }
            Add-Result "OfficialApi" $endpointId $status $detail
        }
        catch {
            Add-Result "OfficialApi" $endpointId "FAIL" $_.Exception.Message
        }
    }
}

function Invoke-CallbackRoutes {
    $callbackPayloads = @(
        @{ Method = "POST"; Path = "/new_user_identified.fcgi"; Body = @{ user_id = 1; user_name = "Ada" }; ContentType = "application/json" },
        @{ Method = "POST"; Path = "/new_card.fcgi"; Body = @{ card = "1234567890"; user_id = 1 }; ContentType = "application/json" },
        @{ Method = "POST"; Path = "/new_biometric_image.fcgi"; Body = "stub-biometric"; ContentType = "application/octet-stream" },
        @{ Method = "POST"; Path = "/new_qrcode.fcgi"; Body = @{ qrcode = "QR-CODE-1"; user_id = 1 }; ContentType = "application/json" },
        @{ Method = "POST"; Path = "/new_biometric_template.fcgi"; Body = "stub-template"; ContentType = "application/octet-stream" },
        @{ Method = "POST"; Path = "/new_uhf_tag.fcgi"; Body = @{ tag = "UHF-1" }; ContentType = "application/json" },
        @{ Method = "POST"; Path = "/new_user_id_and_password.fcgi"; Body = @{ user_id = 1 }; ContentType = "application/json" },
        @{ Method = "POST"; Path = "/device_is_alive.fcgi"; Body = @{ alive = $true }; ContentType = "application/json" },
        @{ Method = "POST"; Path = "/card_create.fcgi"; Body = @{ user_id = 1; card = "1234567890" }; ContentType = "application/json" },
        @{ Method = "POST"; Path = "/fingerprint_create.fcgi"; Body = @{ user_id = 1 }; ContentType = "application/json" },
        @{ Method = "POST"; Path = "/template_create.fcgi"; Body = @{ user_id = 1 }; ContentType = "application/json" },
        @{ Method = "POST"; Path = "/face_create.fcgi"; Body = @{ user_id = 1 }; ContentType = "application/json" },
        @{ Method = "POST"; Path = "/pin_create.fcgi"; Body = @{ user_id = 1; pin = "1234" }; ContentType = "application/json" },
        @{ Method = "POST"; Path = "/password_create.fcgi"; Body = @{ user_id = 1; password = "admin" }; ContentType = "application/json" },
        @{ Method = "POST"; Path = "/new_rex_log.fcgi"; Body = @{ event = "rex" }; ContentType = "application/json" },
        @{ Method = "POST"; Path = "/api/notifications/user_image"; Body = @{ user_id = 1; payload = "image" }; ContentType = "application/json" },
        @{ Method = "POST"; Path = "/api/notifications/template"; Body = @{ user_id = 1; payload = "template" }; ContentType = "application/json" },
        @{ Method = "POST"; Path = "/api/notifications/card"; Body = @{ user_id = 1; payload = "card" }; ContentType = "application/json" },
        @{ Method = "POST"; Path = "/api/notifications/operation_mode"; Body = @{ mode = "standalone" }; ContentType = "application/json" },
        @{ Method = "POST"; Path = "/api/notifications/pin"; Body = @{ user_id = 1; pin = "1234" }; ContentType = "application/json" },
        @{ Method = "POST"; Path = "/api/notifications/password"; Body = @{ user_id = 1; password = "admin" }; ContentType = "application/json" },
        @{ Method = "POST"; Path = "/api/notifications/catra_event"; Body = @{ user_id = 1; event = "clockwise" }; ContentType = "application/json" },
        @{ Method = "POST"; Path = "/api/notifications/usb_drive"; Body = @{ usb_drive = @{ event = "export ok" } }; ContentType = "application/json" },
        @{ Method = "GET"; Path = "/push?device_id=smoke-device"; Body = $null; ContentType = "" },
        @{ Method = "POST"; Path = "/result?device_id=smoke-device&status=completed"; Body = '{"result":"ok"}'; ContentType = "application/json" }
    )

    foreach ($callback in $callbackPayloads) {
        $uri = [Uri]::new([Uri]$AppUrl, $callback.Path).AbsoluteUri
        try {
            if ($callback.Method -eq "GET") {
                Invoke-RestMethod -Uri $uri -WebSession $webSession -Method Get | Out-Null
                Add-Result "Callbacks" $callback.Path "PASS" "GET 200"
            }
            else {
                $body = $callback.Body
                if ($callback.ContentType -eq "application/json" -and $body -isnot [string]) {
                    $body = $body | ConvertTo-Json -Depth 5
                }

                Invoke-RestMethod -Uri $uri -WebSession $webSession -Method Post -Body $body -ContentType $callback.ContentType | Out-Null
                Add-Result "Callbacks" $callback.Path "PASS" "POST 200"
            }
        }
        catch {
            Add-Result "Callbacks" $callback.Path "FAIL" $_.Exception.Message
        }
    }
}

function Write-Report {
    $total = ($results | Measure-Object).Count
    $passed = ($results | Where-Object { $_.Status -eq "PASS" } | Measure-Object).Count
    $failed = ($results | Where-Object { $_.Status -eq "FAIL" } | Measure-Object).Count
    $skipped = ($results | Where-Object { $_.Status -eq "SKIP" } | Measure-Object).Count

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("# Smoke test localhost da PoC Control iD")
    $lines.Add("")
    $lines.Add("Data: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss zzz")")
    $lines.Add("")
    $lines.Add("## Resumo")
    $lines.Add("")
    $lines.Add("- Total: $total")
    $lines.Add("- PASS: $passed")
    $lines.Add("- FAIL: $failed")
    $lines.Add("- SKIP: $skipped")
    $lines.Add("")

    foreach ($group in ($results | Group-Object Phase | Sort-Object Name)) {
        $lines.Add("## $($group.Name)")
        $lines.Add("")
        foreach ($item in $group.Group) {
            $lines.Add("- [$($item.Status)] $($item.Target): $($item.Detail)")
        }
        $lines.Add("")
    }

    [IO.File]::WriteAllLines($reportFullPath, $lines)
    return @{
        Total = $total
        Passed = $passed
        Failed = $failed
        Skipped = $skipped
    }
}

try {
    dotnet build $stubProject -clp:ErrorsOnly | Out-Host
    dotnet build $appProject -clp:ErrorsOnly | Out-Host

    $stubStdOut = Join-Path $artifactsDir "stub.stdout.log"
    $stubStdErr = Join-Path $artifactsDir "stub.stderr.log"
    $appStdOut = Join-Path $artifactsDir "app.stdout.log"
    $appStdErr = Join-Path $artifactsDir "app.stderr.log"

    $stubArguments = "run --project `"$stubProject`" --no-build --no-launch-profile"
    $stubProcess = Start-Process dotnet -ArgumentList $stubArguments -WorkingDirectory $root -RedirectStandardOutput $stubStdOut -RedirectStandardError $stubStdErr -PassThru
    $processes.Add($stubProcess)
    Wait-HttpEndpoint -Url "$StubUrl/system_information.fcgi"

    $appArguments = "run --project `"$appProject`" --no-build --launch-profile Integracao.ControlID.PoC"
    $appProcess = Start-Process dotnet -ArgumentList $appArguments -WorkingDirectory $root -RedirectStandardOutput $appStdOut -RedirectStandardError $appStdErr -PassThru
    $processes.Add($appProcess)
    Wait-HttpEndpoint -Url "$AppUrl/"

    Submit-FormsFromPage -Path "/" -Phase "Bootstrap"
    Submit-FormsFromPage -Path "/Auth/Login" -Phase "Bootstrap"
    Submit-FormsFromPage -Path "/Session/Status" -Phase "Bootstrap"

    $getPages = @(
        "/Auth/Status",
        "/Users", "/Users/Details/1", "/Users/Create", "/Users/Edit/1", "/Users/Delete/1",
        "/Groups", "/Groups/Details/1", "/Groups/Create", "/Groups/Edit/1", "/Groups/Delete/1",
        "/Cards", "/Cards/Details/1", "/Cards/Create", "/Cards/Edit/1", "/Cards/Delete/1",
        "/BiometricTemplates", "/BiometricTemplates/Details/1", "/BiometricTemplates/Create", "/BiometricTemplates/Edit/1", "/BiometricTemplates/Delete/1",
        "/QRCodes", "/QRCodes/Details/1", "/QRCodes/Create", "/QRCodes/Edit/1", "/QRCodes/Delete/1",
        "/Devices", "/Devices/Details/1", "/Devices/Create", "/Devices/Edit/1", "/Devices/Delete/1",
        "/AccessRules", "/AccessRules/Details/1", "/AccessRules/Create", "/AccessRules/Edit/1", "/AccessRules/Delete/1",
        "/AccessLogs", "/AccessLogs/Details/1",
        "/ChangeLogs", "/ChangeLogs/Details/1",
        "/Catra", "/Catra/Details/1", "/Catra/Delete/1",
        "/Config", "/Config/Details/1", "/Config/Create", "/Config/Edit/1", "/Config/Delete/1", "/Config/Diagnostics", "/Config/Official",
        "/Hardware/Status", "/Hardware/Gpio", "/Hardware/DoorState", "/Hardware/RelayAction", "/Hardware/ValidateBiometry",
        "/System/Info", "/System/HashPassword", "/System/LoginCredentials", "/System/Network", "/System/Vpn",
        "/Media", "/Media/Details/1", "/Media/Upload", "/Media/Delete/1", "/Media/AdMode",
        "/Logo", "/Logo/Details/1", "/Logo/Upload", "/Logo/Delete/1",
        "/AdvancedOfficial", "/AdvancedOfficial/ExportObjects", "/AdvancedOfficial/NetworkInterlock", "/AdvancedOfficial/CameraCapture", "/AdvancedOfficial/FacialEnroll", "/AdvancedOfficial/RemoteLedControl",
        "/DocumentedFeatures",
        "/OfficialObjects",
        "/ProductSpecific",
        "/RemoteActions", "/RemoteActions/Authorization", "/RemoteActions/Enroll", "/RemoteActions/Details?action=open_door",
        "/OfficialEvents",
        "/PushCenter"
    )

    foreach ($page in $getPages) {
        try {
            $response = Invoke-WebRequest -Uri ([Uri]::new([Uri]$AppUrl, $page).AbsoluteUri) -WebSession $webSession -Method Get -UseBasicParsing
            Add-Result "Pages" $page "PASS" "GET $($response.StatusCode)"
        }
        catch {
            Add-Result "Pages" $page "FAIL" $_.Exception.Message
        }
    }

    $postPages = @(
        "/Users/Create", "/Users/Edit/1", "/Users/Delete/1",
        "/Groups/Create", "/Groups/Edit/1", "/Groups/Delete/1",
        "/Cards/Create", "/Cards/Edit/1", "/Cards/Delete/1",
        "/BiometricTemplates/Create", "/BiometricTemplates/Edit/1", "/BiometricTemplates/Delete/1",
        "/QRCodes/Create", "/QRCodes/Edit/1", "/QRCodes/Delete/1",
        "/Devices/Create", "/Devices/Edit/1", "/Devices/Delete/1",
        "/AccessRules/Create", "/AccessRules/Edit/1", "/AccessRules/Delete/1",
        "/Config/Create", "/Config/Edit/1", "/Config/Delete/1", "/Config/Diagnostics", "/Config/Official",
        "/Hardware/Gpio", "/Hardware/DoorState", "/Hardware/RelayAction",
        "/System/HashPassword", "/System/LoginCredentials", "/System/Network", "/System/Vpn",
        "/AdvancedOfficial/ExportObjects", "/AdvancedOfficial/NetworkInterlock", "/AdvancedOfficial/CameraCapture", "/AdvancedOfficial/FacialEnroll", "/AdvancedOfficial/RemoteLedControl",
        "/DocumentedFeatures",
        "/OfficialObjects",
        "/ProductSpecific",
        "/RemoteActions/Authorization", "/RemoteActions/Enroll",
        "/PushCenter", "/OfficialEvents"
    )

    foreach ($page in $postPages) {
        Submit-FormsFromPage -Path $page -Phase "Forms"
    }

    Invoke-OfficialCatalog
    Invoke-CallbackRoutes

    $summary = Write-Report
    Write-Host "Smoke concluído. PASS=$($summary.Passed) FAIL=$($summary.Failed) SKIP=$($summary.Skipped)"

    if ($summary.Failed -gt 0) {
        exit 1
    }
}
finally {
    foreach ($process in $processes) {
        if ($null -ne $process -and -not $process.HasExited) {
            Stop-Process -Id $process.Id -Force
        }
    }
}
