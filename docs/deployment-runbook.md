# Deployment, ambientes e resiliencia

Escopo: PoC ASP.NET Core 8 MVC/Razor com SQLite local e integracao com equipamento
Control iD. Este documento descreve execucao reproduzivel fora do ambiente local
sem criar deploy automatico, DNS real ou credenciais versionadas.

## Ambientes mapeados

| Ambiente | Estado | Evidencia | Observacao |
| --- | --- | --- | --- |
| Local | Existente | `launchSettings.json`, `README.md`, `tools/smoke-localhost.ps1` | Usa `Development`, SQLite local e User Secrets/env vars. |
| Development | Existente | `appsettings.Development.json` | Habilita OpenAPI somente neste ambiente. |
| Staging | Configurado | `appsettings.Staging.json`, `.env.example`, Docker/Compose | Requer secrets e hosts via ambiente. |
| Production | Configurado | `appsettings.Production.json`, `.env.example`, Docker/Compose | Startup falha se hosts, shared key, assinatura e egress allowlist nao forem configurados. |
| Preview | Ausente | Sem provedor/manifesto dedicado | Use branch/servico efemero com as mesmas variaveis de Staging. |

Nao ha provedor cloud versionado. Qualquer Render, Azure, AWS, GCP, Fly.io, VPS ou
Kubernetes deve ser configurado por decisao humana e sem credenciais no Git.

## Configuracao obrigatoria fora de Development

Use variaveis no formato nativo do ASP.NET Core. `.env.example` contem placeholders
seguros para Compose; copie para `.env` e substitua todos os valores antes de uso.

Variaveis minimas:

- `ASPNETCORE_ENVIRONMENT=Staging` ou `Production`.
- `ASPNETCORE_URLS=http://+:8080` no container.
- `AllowedHosts` com host real, sem `*`, `localhost` ou placeholders.
- `ConnectionStrings__DefaultConnection=Data Source=/app/data/integracao_controlid.db`
  ou caminho de volume persistente equivalente.
- `CallbackSecurity__RequireSharedKey=true`.
- `CallbackSecurity__SharedKey` com valor real, nao placeholder, minimo de 32 caracteres.
- `CallbackSecurity__RequireSignedRequests=true`.
- `CallbackSecurity__AllowLoopback=false` em ambiente exposto.
- `ControlIDApi__RequireAllowedDeviceHosts=true`.
- `ControlIDApi__AllowedDeviceHosts__0` com o host/IP permitido do equipamento.
- `OpenApi__Enabled=false`.
- `Observability__Metrics__AllowAnonymous=false`.
- `Serilog__WriteTo__1__Args__retainedFileCountLimit=14` ou valor aprovado.
- `Serilog__WriteTo__1__Args__fileSizeLimitBytes=10000000` ou limite aprovado.

Reverse proxy:

- `ForwardedHeaders__Enabled=false` por padrao.
- Habilite apenas atras de proxy confiavel.
- Quando habilitar fora de Development, configure `ForwardedHeaders__KnownProxies__0`
  com IP real do proxy ou load balancer.

## Infraestrutura container

Artefatos versionados:

- `Dockerfile`: multi-stage build, imagem runtime Alpine, usuario nao root, porta
  `8080`, volume esperado para `/app/data` e `/app/Logs`, healthcheck em
  `/health/live`.
- `.dockerignore`: remove Git, bin/obj, logs, artefatos, `.env` e SQLite local do
  contexto de build.
- `docker-compose.yml`: execucao local/container com volumes nomeados, portas,
  healthcheck e variaveis obrigatorias.

Comandos:

```powershell
docker build -t integracao-controlid-poc:local .
docker compose config
docker compose up --build
```

Health checks:

- Liveness: `GET /health/live`.
- Readiness: `GET /health/ready`.
- Metricas: `GET /metrics` com usuario administrador.

## Procedimento de deploy

1. Criar ou atualizar `.env` fora do Git com base em `.env.example`.
2. Garantir volume persistente para `/app/data` e `/app/Logs`.
3. Validar a configuracao sem iniciar:

```powershell
docker compose config
```

4. Construir a imagem:

```powershell
docker build --pull -t integracao-controlid-poc:<versao> .
```

5. Subir em Staging:

```powershell
docker compose up --build
```

6. Validar:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\observability-check.ps1 -OfflineValidateOnly
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunContainerBuild
```

7. Contra ambiente rodando, validar health/readiness e metricas com credencial local:

```powershell
$env:OBSERVABILITY_BASE_URL = "http://localhost:8080"
powershell -ExecutionPolicy Bypass -File .\tools\observability-check.ps1
```

8. Para release sem excecoes, rode o gate estrito em ambiente preparado:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -ReleaseGate
```

O `-ReleaseGate` exige tambem `ops.local.json` preenchido fora do Git, baseado em
`ops.example.json`, para bloquear release sem ownership, on-call, RTO/RPO,
backup externo, provedor/DNS/TLS/sizing, DPO/juridico quando aplicavel,
scanners externos, FinOps/capacidade e contingencia fisica validados.

O fechamento das lacunas externas fica em `docs/residual-risk-closure.md`.

## Rollback tecnico

Para incidentes ativos, use tambem `docs/incident-response-and-dr.md`, que define
severidade, comunicacao, escalonamento, preservacao de evidencias e validacao
pos-rollback.

1. Preservar volume `/app/data` antes de trocar versao.
2. Manter a imagem anterior tagueada, por exemplo `integracao-controlid-poc:<versao-anterior>`.
3. Se o novo container falhar em `/health/ready`, parar somente o servico novo.
4. Subir a tag anterior com o mesmo `.env` e os mesmos volumes.
5. Validar `/health/live`, `/health/ready`, login local e logs.
6. Se a falha envolver schema SQLite, restaurar copia apenas em ambiente controlado
   usando `tools/restore-smoke-sqlite.ps1`; nao sobrescreva dados reais sem
   confirmacao humana.

Para preparacao operacional, gere backup com restore-smoke e espelhamento:

```powershell
$env:CONTROLID_BACKUP_MIRROR_DIRECTORY = "\\servidor-seguro\backups\controlid-poc"
powershell -ExecutionPolicy Bypass -File .\tools\backup-sqlite-operational.ps1 -RunRestoreSmoke
```

## Riscos de ambiente

| Risco | Severidade | Controle atual |
| --- | --- | --- |
| Provedor cloud ausente | Media | Docker/Compose e runbook; escolha de provedor requer decisao humana. |
| TLS/DNS fora do repo | Alta | Deve ser terminado no proxy/provedor; app bloqueia configs inseguras fora de Development. |
| Equipamento fisico nao disponivel na CI | Alta | `-RequireHardwareContract` e `-ReleaseGate` bloqueiam release quando exigido. |
| Secrets reais fora do Git | Alta | `.env.example`, User Secrets, secret scan e validacao contra placeholders. |
| SQLite local em container sem volume | Alta | Compose usa volume nomeado para `/app/data`; docs exigem volume persistente. |
| Forwarded headers com proxy nao confiavel | Alta | Desabilitado por padrao; exige `KnownProxies` fora de Development. |
