# Fortalecimento da segurança

> **Documento vivo** · Público: desenvolvimento, AppSec e operação · Responsável: AppSec · Última validação: 2026-08-04.

## Controles implementados

- Autenticação local obrigatória por cookie para controllers MVC/Razor.
- Bootstrap seguro: o primeiro usuário local cadastrado recebe papel `Administrator`; cadastros posteriores exigem administrador autenticado e recebem `Operator`.
- RBAC para operações administrativas, dados sensíveis, gestão de sessão pelo
  `SessionController`, biometria, cartões, mídia, configuração, hardware, objetos
  oficiais mutáveis, Push manual e limpeza/expurgo de eventos. Conexão e
  login/logout oficial pelo `AuthController` permanecem disponíveis a qualquer
  usuário local autenticado; a matriz exata está em
  `local-account-administration.md`.
- Hash de senha local migrado para PBKDF2-HMAC-SHA256 com suporte de leitura para hashes SHA256 legados. Hash legado válido e regravado em PBKDF2 no próximo login local.
- Endpoints externos de callback e Push permanecem anônimos para compatibilidade com o equipamento, mas passam por validação de IP, chave compartilhada e limite de requisições, além de poderem exigir assinatura HMAC.
- Assinatura HMAC de ingressos externos usa `X-ControlID-Signature`, `X-ControlID-Timestamp` e `X-ControlID-Nonce`, com janela de tempo e cache anti-replay limitado por `CallbackSecurity:MaxTrackedNonces`.
- `user_get_image.fcgi` agora usa a mesma avaliação de segurança e assinatura dos ingressos externos antes de retornar foto local.
- Egress para equipamentos pode ser limitado por allowlist em `ControlIDApi:AllowedDeviceHosts`.
- Fora de `Development`, a aplicação exige `AllowedHosts` explícito, shared key de callback, assinatura HMAC, OpenAPI desabilitado e allowlist de equipamentos habilitada.
- Cabeçalhos HTTP reforçados com CSP sem `unsafe-inline`, `Permissions-Policy`, `frame-ancestors`, `nosniff`, COOP, `Referrer-Policy` e HSTS fora de `Development`.
- `Referrer-Policy` usa `no-referrer` para reduzir vazamento acidental de URLs internas, inclusive quando a Access API exige `session` em query string.
- Rate limit global por usuário autenticado ou IP cobre a UI e atua junto das políticas específicas de login local e ingressos externos.
- Logs de request incluem referências pseudonimizadas de usuário/IP e trace id; logs de push legado não gravam corpo bruto; URLs oficiais exibidas/registradas mascaram `session`, tokens e segredos em query string.
- Mensagens públicas de erro de API não exibem corpo bruto retornado pelo equipamento.
- Uploads administrativos validam allowlist de extensão, tamanho, content-type declarado e assinatura/conteúdo quando aplicável para PNG/JPG, MP4, WAV, PEM e OpenVPN.
- Cópias de segurança do SQLite geradas por `tools/backup-sqlite.ps1` são protegidas por DPAPI por padrão; o teste smoke de restauração descriptografa cópias protegidas para validar a recuperação.
- `tools/harden-local-state.ps1` restringe permissões locais de SQLite, logs e backups para o usuário atual, Administrators e SYSTEM no Windows.
- `tools/ControlIdCallbackSigningProxy` fornece uma ponte assinadora para equipamentos que não conseguem gerar HMAC nativamente, com allowlist de paths, bloqueio de headers sensíveis encaminhados e limite de resposta.

O endereço-base do equipamento aceita somente `http` ou `https`, sem
credenciais, fragmento, caminho ou consulta. A normalização rejeita esquemas
alternativos e sufixos ambíguos após IPv6, evitando reinterpretação silenciosa
do destino. O limitador por equipamento permite até quatro operações
simultâneas e fila de 16; excesso retorna erro local controlado sem atingir o
dispositivo.

## Configuração de produção ou ambiente exposto

Valores reais devem ser configurados por variáveis de ambiente, User Secrets ou provedor seguro equivalente:

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

## Cadeia canônica da assinatura

A assinatura HMAC-SHA256 e calculada em Base64 sobre:

```text
METHOD
PATH
QUERY_STRING
TIMESTAMP
NONCE
BASE64(SHA256(BODY_BYTES_EXATOS))
```

O cliente envia o resultado em `X-ControlID-Signature`. O prefixo opcional `sha256=` e aceito. O `TIMESTAMP` pode ser Unix seconds ou data ISO-8601 UTC. O `NONCE` deve ser único em toda a superfície de callbacks dentro da janela configurada. O hash usa os bytes recebidos, inclusive para imagens e octet-stream, sem conversão intermediaria para texto.

## Equipamentos sem HMAC nativo

Quando o equipamento não puder assinar as chamadas diretamente, execute o proxy local:

```powershell
dotnet user-secrets set --project .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj "Proxy:SharedKey" "<mesmo-segredo-da-poc>"
dotnet user-secrets set --project .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj "Proxy:ForwardBaseUrl" "http://localhost:5000"
dotnet user-secrets set --project .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj "Proxy:AllowedRemoteIps:0" "<ip-do-equipamento>"
dotnet run --project .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --urls http://localhost:6700
```

O equipamento deve chamar o proxy. O proxy assina e encaminha para a PoC, mantendo a PoC com `RequireSignedRequests=true`.

O proxy remove headers de chave compartilhada, assinatura, timestamp, nonce e chave inbound recebidos do equipamento antes de inserir a assinatura própria. Isso evita que um header enviado pelo cliente cause duplicidade ou vaze a chave do proxy para a aplicação de destino.

## Estado local e recuperação

Backups protegidos:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\backup-sqlite.ps1
powershell -ExecutionPolicy Bypass -File .\tools\restore-smoke-sqlite.ps1
```

Permissões locais:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\harden-local-state.ps1
```

Esses controles não substituem isolamento de rede e governança de acesso ao host, mas deixam o repositório com implementações reproduzíveis para assinatura, backup protegido, restore validável e restrição de arquivos locais.

## Validação com equipamento físico

A validação real do hardware não deve usar credenciais versionadas. Configure as variáveis apenas no terminal local ou no cofre do ambiente:

```powershell
$env:CONTROLID_DEVICE_URL = "http://<ip-ou-host-do-equipamento>:8080"
$env:CONTROLID_USERNAME = "<usuario>"
$env:CONTROLID_PASSWORD = "<senha>"
powershell -ExecutionPolicy Bypass -File .\tools\contract-controlid-device.ps1
```

O script executa apenas operações de leitura e sessão: `system_information.fcgi`, `login.fcgi`, `session_is_valid.fcgi` e `logout.fcgi`. O valor da sessão não é exibido e o relatório padrão fica em `artifacts/`, fora do Git. Sem equipamento e credenciais reais, esta validação permanece bloqueada pelo ambiente, não pelo código.

## Verificações específicas

```powershell
dotnet restore .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --locked-mode
dotnet build .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --no-restore -v:minimal
dotnet format .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --verify-no-changes --no-restore -v:minimal
powershell -ExecutionPolicy Bypass -File .\tools\scan-secrets.ps1
powershell -ExecutionPolicy Bypass -File .\tools\smoke-localhost.ps1 -ReportPath .\artifacts\smoke\localhost-smoke-ci.md
```

## Matriz ameaça, controle e teste

| ID | Ameaça | Categoria | Controle principal | Evidência automatizada | Risco residual |
| --- | --- | --- | --- | --- | --- |
| SEC-001 | Falsificação de callback | STRIDE: falsificação | Chave compartilhada, HMAC, timestamp, nonce e IP | `CallbackSignatureValidatorTests` | Relógio, rede e equipamento reais |
| SEC-002 | SSRF de saída | OWASP: SSRF | URL normalizada e lista de hosts permitidos | `ControlIdInputSanitizerTests` e invocador | Mudança de topologia exige revisão |
| SEC-003 | Quebra de autorização | OWASP: controle de acesso | Cookie global e RBAC administrativo | Testes de controladores | Provedor corporativo ainda não escolhido |
| SEC-004 | Injeção ou XSS | OWASP: injeção | Vinculação de modelos, codificação Razor e validação | Testes renderizados e de controladores | Nova carga útil requer análise contextual |
| SEC-005 | Exposição de segredo | OWASP: falha criptográfica/configuração | Configuração externa, mascaramento e scan | `tools/scan-secrets.ps1` | Rotação e cofre dependem do ambiente |
| SEC-006 | Negação de serviço | STRIDE: negação de serviço | Limite de requisições, corpo/resposta e circuit breaker | Testes de limite e circuit breaker | Capacidade do host precisa de linha de base |
| SEC-007 | Vazamento em logs | STRIDE: divulgação | Pseudonimização e lista de contexto permitido | `PrivacyLogHelperTests` | Revisão humana de novos eventos |

## Rotação de segredos

1. Identifique escopo e dependências sem exibir o valor.
2. Gere segredo forte no cofre ou mecanismo aprovado.
3. Atualize PoC, proxy e equipamento em janela controlada.
4. Revogue o valor anterior e invalide sessões quando aplicável.
5. Valide callback assinado, login e métricas de falha.
6. Registre responsável, horário, sistemas e resultado em local restrito.

Nunca registre valor antigo ou novo em issue, relatório, screenshot ou comando
versionado. Comprometimento segue o cenário IR-14 de
`docs/incident-response-and-dr.md`.

Mudança que afete uma ameaça deve citar o ID `SEC-*` no resumo, atualizar o teste
e reavaliar o risco residual. A matriz é técnica e não declara conformidade total
com OWASP, ASVS ou qualquer norma externa.
