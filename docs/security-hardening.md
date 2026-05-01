# Security hardening - 2026-04-30

## Controles implementados

- Autenticacao local obrigatoria por cookie para controllers MVC/Razor.
- Bootstrap seguro: o primeiro usuario local cadastrado recebe papel `Administrator`; cadastros posteriores exigem administrador autenticado e recebem `Operator`.
- RBAC para operacoes administrativas, dados sensiveis, sessoes, biometria, cartoes, midia, configuracao, hardware, objetos oficiais mutaveis, push manual e limpeza/expurgo de eventos.
- Hash de senha local migrado para PBKDF2-HMAC-SHA256 com suporte de leitura para hashes SHA256 legados. Hash legado valido e regravado em PBKDF2 no proximo login local.
- Endpoints externos de callback/push permanecem anonimos para compatibilidade de equipamento, mas passam por IP/shared key/rate limit e podem exigir assinatura HMAC.
- Assinatura HMAC de ingressos externos usa `X-ControlID-Signature`, `X-ControlID-Timestamp` e `X-ControlID-Nonce`, com janela de tempo e cache anti-replay.
- `user_get_image.fcgi` agora usa a mesma avaliacao de seguranca e assinatura dos ingressos externos antes de retornar foto local.
- Egress para equipamentos pode ser limitado por allowlist em `ControlIDApi:AllowedDeviceHosts`.
- Fora de `Development`, a aplicacao exige `AllowedHosts` explicito, shared key de callback, assinatura HMAC, OpenAPI desabilitado e allowlist de equipamentos habilitada.
- Headers HTTP reforcados com CSP sem `unsafe-inline`, Permissions-Policy, frame-ancestors, nosniff, COOP, Referrer-Policy e HSTS fora de `Development`.
- `Referrer-Policy` usa `no-referrer` para reduzir vazamento acidental de URLs internas, inclusive quando a Access API exige `session` em query string.
- Rate limit global por usuario autenticado ou IP cobre a UI e atua junto das politicas especificas de login local e ingressos externos.
- Logs de request incluem referencias pseudonimizadas de usuario/IP e trace id; logs de push legado nao gravam corpo bruto; URLs oficiais exibidas/registradas mascaram `session`, tokens e segredos em query string.
- Mensagens publicas de erro de API nao exibem corpo bruto retornado pelo equipamento.
- Uploads administrativos validam allowlist de extensao, tamanho, content-type declarado e assinatura/conteudo quando aplicavel para PNG/JPG, MP4, WAV, PEM e OpenVPN.
- Backups SQLite gerados por `tools/backup-sqlite.ps1` sao protegidos por DPAPI por padrao; o restore-smoke descriptografa copias protegidas para validar recuperacao.
- `tools/harden-local-state.ps1` restringe permissoes locais de SQLite, logs e backups para o usuario atual, Administrators e SYSTEM no Windows.
- `tools/ControlIdCallbackSigningProxy` fornece uma ponte assinadora para equipamentos que nao conseguem gerar HMAC nativamente, com allowlist de paths, bloqueio de headers sensiveis encaminhados e limite de resposta.

## Configuracao de producao ou ambiente exposto

Valores reais devem ser configurados por variaveis de ambiente, User Secrets ou provedor seguro equivalente:

```json
{
  "AllowedHosts": "poc.exemplo.local",
  "ControlIDApi": {
    "RequireAllowedDeviceHosts": true,
    "AllowedDeviceHosts": [ "192.168.0.10", "controlid.exemplo.local" ]
  },
  "CallbackSecurity": {
    "RequireSharedKey": true,
    "SharedKey": "<segredo-forte>",
    "AllowedRemoteIps": [ "192.168.0.10" ],
    "RequireSignedRequests": true
  },
  "OpenApi": {
    "Enabled": false
  }
}
```

## Canonical string da assinatura

A assinatura HMAC-SHA256 e calculada em Base64 sobre:

```text
METHOD
PATH
QUERY_STRING
TIMESTAMP
NONCE
BASE64(SHA256(BODY_NORMALIZADO))
```

O cliente envia o resultado em `X-ControlID-Signature`. O prefixo opcional `sha256=` e aceito. O `TIMESTAMP` pode ser Unix seconds ou data ISO-8601 UTC. O `NONCE` deve ser unico dentro da janela configurada.

## Equipamentos sem HMAC nativo

Quando o equipamento nao puder assinar as chamadas diretamente, execute o proxy local:

```powershell
dotnet user-secrets set --project .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj "Proxy:SharedKey" "<mesmo-segredo-da-poc>"
dotnet user-secrets set --project .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj "Proxy:ForwardBaseUrl" "http://localhost:5000"
dotnet user-secrets set --project .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj "Proxy:AllowedRemoteIps:0" "<ip-do-equipamento>"
dotnet run --project .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --urls http://localhost:6700
```

O equipamento deve chamar o proxy. O proxy assina e encaminha para a PoC, mantendo a PoC com `RequireSignedRequests=true`.

O proxy remove headers de chave compartilhada, assinatura, timestamp, nonce e chave inbound recebidos do equipamento antes de inserir a assinatura propria. Isso evita que um header enviado pelo cliente cause duplicidade ou vaze a chave do proxy para a aplicacao de destino.

## Estado local e recuperacao

Backups protegidos:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\backup-sqlite.ps1
powershell -ExecutionPolicy Bypass -File .\tools\restore-smoke-sqlite.ps1
```

Permissoes locais:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\harden-local-state.ps1
```

Esses controles nao substituem isolamento de rede e governanca de acesso ao host, mas deixam o repositorio com implementacoes reproduziveis para assinatura, backup protegido, restore validavel e restricao de arquivos locais.

## Validacao com equipamento fisico

A validacao real do hardware nao deve usar credenciais versionadas. Configure as variaveis apenas no terminal local ou no cofre do ambiente:

```powershell
$env:CONTROLID_DEVICE_URL = "http://<ip-ou-host-do-equipamento>:8080"
$env:CONTROLID_USERNAME = "<usuario>"
$env:CONTROLID_PASSWORD = "<senha>"
powershell -ExecutionPolicy Bypass -File .\tools\contract-controlid-device.ps1
```

O script executa apenas operacoes de leitura/sessao: `system_information.fcgi`, `login.fcgi`, `session_is_valid.fcgi` e `logout.fcgi`. O valor da sessao nao e exibido e o relatorio padrao fica em `artifacts/`, fora do Git. Sem equipamento e credenciais reais, esta validacao permanece bloqueada por ambiente, nao por codigo.

## Checks especificos

```powershell
dotnet restore .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --locked-mode
dotnet build .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --no-restore -v:minimal
dotnet format .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --verify-no-changes --no-restore -v:minimal
powershell -ExecutionPolicy Bypass -File .\tools\scan-secrets.ps1
powershell -ExecutionPolicy Bypass -File .\tools\smoke-localhost.ps1 -ReportPath .\artifacts\smoke\localhost-smoke-ci.md
```
