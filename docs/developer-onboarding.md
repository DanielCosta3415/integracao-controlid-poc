# Onboarding tecnico

Este guia leva um novo desenvolvedor do clone ao diagnostico e entrega segura da
PoC. Ele complementa `README.md` e `AGENTS.md`.

## Visao geral

A PoC integra uma aplicacao ASP.NET Core MVC/Razor com a Access API Control iD.
Ela possui UI operacional, cliente generico de endpoints oficiais, callbacks,
Push, persistencia local SQLite, observabilidade, gates de readiness e scripts de
backup/restore.

Principios para contribuir:

- Diagnostique antes de corrigir.
- Preserve contratos publicos de rotas, callbacks, Push, DTOs e ViewModels.
- Nao use credenciais, biometria, fotos, cartoes, QR Codes ou payloads reais em
  testes, docs ou screenshots.
- Rode os checks relevantes e documente qualquer falha ou check nao executado.

## Ambiente local

1. Verifique o SDK:

```powershell
dotnet --version
dotnet --list-sdks
```

2. Restaure dependencias:

```powershell
dotnet restore .\Integracao.ControlID.PoC.sln --locked-mode
dotnet restore .\tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj --locked-mode
dotnet restore .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --locked-mode
```

3. Configure segredos fora do Git:

```powershell
dotnet user-secrets set "ControlIDApi:DefaultDeviceUrl" "http://<equipamento-ou-host>:8080"
dotnet user-secrets set "ControlIDApi:DefaultUsername" "<usuario>"
dotnet user-secrets set "ControlIDApi:DefaultPassword" "<senha>"
dotnet user-secrets set "CallbackSecurity:SharedKey" "<segredo-local>"
```

Para container, copie `.env.example` para `.env` fora do Git e substitua todos os
placeholders.

## Execucao

Aplicacao:

```powershell
dotnet run --project .\Integracao.ControlID.PoC.csproj
```

Stub:

```powershell
dotnet run --project .\tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj --no-launch-profile
```

Proxy assinador opcional:

```powershell
dotnet run --project .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --urls http://localhost:6700
```

Container:

```powershell
docker build -t integracao-controlid-poc:local .
docker compose config
docker compose up --build
```

## Ciclo de desenvolvimento

Fluxo recomendado para mudanca comum:

1. Leia `AGENTS.md` e o documento de dominio afetado.
2. Localize arquivos pelo mapa `docs/project-file-responsibilities.md`.
3. Altere o menor conjunto coerente de arquivos.
4. Adicione ou ajuste testes quando mudar comportamento, contrato, seguranca,
   observabilidade ou dado.
5. Atualize docs se setup, contrato, risco, comando ou operacao mudar.
6. Rode checks relevantes.

Checks basicos:

```powershell
dotnet build .\Integracao.ControlID.PoC.sln --no-restore -v:minimal
dotnet test .\Integracao.ControlID.PoC.sln --no-build -v:minimal
dotnet format .\Integracao.ControlID.PoC.sln --verify-no-changes --no-restore -v:minimal
git diff --check
powershell -ExecutionPolicy Bypass -File .\tools\scan-secrets.ps1
```

Gate padrao:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1
```

Gate estrito de release:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -ReleaseGate
```

## Diagnostico rapido

| Sintoma | Primeiro lugar para olhar |
| --- | --- |
| App nao sobe | Console, `Logs/app_log.txt`, validacoes de startup em `Program.cs` |
| SQLite indisponivel | `/health/ready`, permissao do arquivo, `docs/data-model-and-recovery.md` |
| Equipamento nao responde | `OfficialApiInvokerService`, timeout, allowlist, rede e sessao |
| Callback rejeitado | `CallbackSecurityEvaluator`, shared key, HMAC, IP permitido e tamanho |
| Push nao entrega | `PushCenter`, `/push`, `/result`, status em `PushCommands` |
| `/metrics` bloqueado | Auth local admin e `Observability:Metrics:AllowAnonymous=false` |
| Build/test falha apos doc change | Testes de contrato podem validar docs, CI e scripts |

## Onde mudar

| Necessidade | Arquivos provaveis |
| --- | --- |
| Nova rota/tela | `Controllers/`, `Views/`, `ViewModels/`, `Services/Navigation/` |
| Chamada oficial Control iD | `Services/ControlIDApi/`, `docs/integration-contracts.md` |
| Callback/monitor | `Controllers/OfficialCallbacksController.cs`, `Services/Callbacks/`, `Monitor/` |
| Push | `Controllers/PushCenterController.cs`, `Services/Push/`, `Models/Database/PushCommandLocal.cs` |
| Banco/schema | `Models/Database/`, `Data/`, `Data/Migrations/`, `Services/Database/` |
| Segurança | `Services/Security/`, `Middlewares/`, `Options/CallbackSecurityOptions.cs` |
| Observabilidade | `Services/Observability/`, `Middlewares/RequestLoggingMiddleware.cs`, `docs/observability/` |
| FinOps/capacidade | `Services/Observability/RuntimeCapacityMetricsProvider.cs`, `tools/finops-capacity-check.ps1` |
| Docs/governanca | `docs/`, `AGENTS.md`, `README.md`, `docs/adrs/` |

## Dados e privacidade

Nunca versionar:

- `.env`, `ops.local.json`, secrets, cookies, tokens e shared keys.
- `integracao_controlid.db*`.
- `Logs/`, `artifacts/`, backups e restore temporario.
- Fotos, biometria, cartoes, QR Codes, payloads reais ou screenshots com dados.

Antes de tocar dados pessoais ou sensiveis, leia:

- `docs/privacy-and-data-retention.md`
- `docs/privacy-governance-runbook.md`
- `docs/data-model-and-recovery.md`

## Entrega segura

Definition of Done pratica:

- Codigo/documentacao alterado dentro do escopo.
- Contratos publicos preservados ou alteracao versionada/documentada.
- Testes relevantes adicionados ou atualizados.
- Checks executados e resultado registrado.
- Docs atualizadas quando setup, comportamento, risco ou operacao mudou.
- Riscos residuais explicitados.
- Arquivos alterados listados no resumo final.

Commit e push exigem confirmacao humana explicita.
