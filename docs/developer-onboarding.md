# Integração técnica de novos desenvolvedores

> **Guia vivo** · Público: novos desenvolvedores · Responsável: mantenedores · Última validação: 2026-08-03.

Este guia leva um novo desenvolvedor do clone ao diagnóstico e entrega segura da
PoC. Ele complementa `README.md` e `AGENTS.md`.

## Visão geral

A PoC integra uma aplicação ASP.NET Core MVC/Razor com a Access API Control iD.
Ela possui UI operacional, cliente genérico de endpoints oficiais, callbacks,
Push, persistência local SQLite, observabilidade, gates de readiness e scripts de
backup/restore.

Princípios para contribuir:

- Diagnostique antes de corrigir.
- Preserve contratos públicos de rotas, callbacks, Push, DTOs e ViewModels.
- Não use credenciais, biometria, fotos, cartões, QR Codes ou payloads reais em
  testes, docs ou screenshots.
- Rode os checks relevantes e documente qualquer falha ou check não executado.

## Ambiente local

0. Clone o repositório, entre na raiz e confirme que não há mudanças locais:

```powershell
git clone <url-do-repositorio>
Set-Location .\integracao-controlid-poc
git status -sb
```

1. Verifique o SDK:

```powershell
dotnet --version
dotnet --list-sdks
```

Resultado esperado: `dotnet --version` informa `10.0.302`, conforme
`global.json`. Instale o SDK .NET 10 por um canal oficial da Microsoft antes de
prosseguir; não altere o arquivo para contornar a ausência do SDK.

2. Restaure dependências:

```powershell
dotnet restore .\Integracao.ControlID.PoC.sln --locked-mode
dotnet restore .\tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj --locked-mode
dotnet restore .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --locked-mode
dotnet tool restore
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

Resultado esperado: restores concluídos em modo locked, sem alteração nos
`packages.lock.json`, e `dotnet-ef` local `10.0.10` disponível pelo manifesto
`.config/dotnet-tools.json`. Falha de restore indica SDK incompatível, lockfile,
manifesto inconsistente ou indisponibilidade do NuGet; não remova o modo locked
para contornar o problema.

## Execução

Aplicação:

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

### Primeiro acesso local

1. Abra `http://localhost:5000` ou `https://localhost:5001`.
2. Se ainda não houver usuário, acesse `/Auth/Register` e cadastre somente dados
   fictícios. O primeiro cadastro é administrador por bootstrap transacional.
3. Faça login em `/Auth/LocalLogin`.
4. Para trabalhar sem hardware, execute o stub em outro terminal; ele escuta em
   `http://127.0.0.1:6600` e aceita `stub-admin`/`stub-password`.
5. Em `/Auth/Login`, conecte a aplicação ao stub e confirme `/Auth/Status`.

Nunca reutilize essas credenciais fictícias fora do stub. O resultado esperado é
um shell autenticado, `/health/ready` saudável e o catálogo oficial acessível.

A conta local autentica a pessoa na PoC; o login em `/Auth/Login` cria uma sessão
separada no equipamento ou stub. O papel `Operator` pode conectar e autenticar o
equipamento, mas operações administrativas e escritas continuam restritas a
`Administrator`. Consulte `local-account-administration.md` e `faq.md` antes de
testar com hardware real.

### Depuração

- Visual Studio/Rider: use o perfil `Integracao.ControlID.PoC` de
  `Properties/launchSettings.json`.
- VS Code: execute `dotnet run --project .\Integracao.ControlID.PoC.csproj` e
  anexe o depurador ao processo .NET quando necessário.
- Breakpoints úteis: `AuthController`, `OfficialApiInvokerService`,
  `CallbackIngressService`, `PushCommandWorkflowService` e repositórios em
  `Services/Database/`.
- Estado descartável: use banco temporário ou cópia de segurança; não apague o
  SQLite de outro usuário ou ambiente.

## Ciclo de desenvolvimento

Fluxo recomendado para mudança comum:

1. Leia `AGENTS.md` e o documento de domínio afetado.
2. Localize arquivos pelo mapa `docs/project-file-responsibilities.md`.
3. Altere o menor conjunto coerente de arquivos.
4. Adicione ou ajuste testes quando mudar comportamento, contrato, segurança,
   observabilidade ou dado.
5. Atualize docs se setup, contrato, risco, comando ou operação mudar.
6. Rode checks relevantes.

Checks básicos:

```powershell
dotnet build .\Integracao.ControlID.PoC.sln --no-restore -v:minimal
dotnet test .\Integracao.ControlID.PoC.sln --no-build -v:minimal
dotnet format .\Integracao.ControlID.PoC.sln --verify-no-changes --no-restore -v:minimal
git diff --check
powershell -ExecutionPolicy Bypass -File .\tools\validate-documentation.ps1
powershell -ExecutionPolicy Bypass -File .\tools\scan-secrets.ps1
```

Critério padrão:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1
```

Critério estrito de liberação:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -ReleaseGate
```

## Diagnóstico rápido

| Sintoma | Primeiro lugar para olhar |
| --- | --- |
| App não sobe | Console, `Logs/app_log.txt`, validações de startup em `Program.cs` |
| SQLite indisponível | `/health/ready`, permissão do arquivo, `docs/data-model-and-recovery.md` |
| Equipamento não responde | `OfficialApiInvokerService`, timeout, allowlist, rede e sessão |
| Callback rejeitado | `CallbackSecurityEvaluator`, shared key, HMAC, IP permitido e tamanho |
| Push não entrega | `PushCenter`, `/push`, `/result`, status em `PushCommands` |
| `/metrics` bloqueado | Auth local admin e `Observability:Metrics:AllowAnonymous=false` |
| Build/test falha após doc change | Testes de contrato podem validar docs, CI e scripts |

## Onde mudar

| Necessidade | Arquivos prováveis |
| --- | --- |
| Nova rota/tela | `Controllers/`, `Views/`, `ViewModels/`, `Services/Navigation/` |
| Chamada oficial Control iD | `Services/ControlIDApi/`, `docs/integration-contracts.md` |
| Callback/monitor | `Controllers/OfficialCallbacksController.cs`, `Services/Callbacks/`, `Monitor/` |
| Push | `Controllers/PushCenterController.cs`, `Services/Push/`, `Models/Database/PushCommandLocal.cs` |
| Banco/schema | `Models/Database/`, `Data/`, `Data/Migrations/`, `Services/Database/` |
| Segurança | `Services/Security/`, `Middlewares/`, `Options/CallbackSecurityOptions.cs` |
| Observabilidade | `Services/Observability/`, `Middlewares/RequestLoggingMiddleware.cs`, `docs/observability/` |
| FinOps/capacidade | `Services/Observability/RuntimeCapacityMetricsProvider.cs`, `tools/finops-capacity-check.ps1` |
| Docs/governança | `docs/`, `AGENTS.md`, `README.md`, `docs/adrs/` |

## Dados e privacidade

Nunca versionar:

- `.env`, `ops.local.json`, secrets, cookies, tokens e shared keys.
- `integracao_controlid.db*`.
- `Logs/`, `artifacts/`, backups e restore temporário.
- Fotos, biometria, cartões, QR Codes, payloads reais ou screenshots com dados.

Antes de tocar dados pessoais ou sensíveis, leia:

- `docs/privacy-and-data-retention.md`
- `docs/privacy-governance-runbook.md`
- `docs/data-model-and-recovery.md`

## Entrega segura

Definição de Pronto prática:

- Código/documentação alterado dentro do escopo.
- Contratos públicos preservados ou alteração versionada/documentada.
- Testes relevantes adicionados ou atualizados.
- Checks executados e resultado registrado.
- Docs atualizadas quando setup, comportamento, risco ou operação mudou.
- Riscos residuais explicitados.
- Arquivos alterados listados no resumo final.

Commit e push exigem confirmação humana explícita.

## Evidência do primeiro sucesso

| Marco | Evidência esperada | Tempo indicativo |
| --- | --- | ---: |
| Dependências restauradas | Restore em modo bloqueado sem alteração de lockfile | 2 a 10 min |
| Aplicação iniciada | `/health/live` retorna sucesso | até 2 min |
| Usuário local criado | Login local concluído com dados fictícios | até 5 min |
| Stub autenticado | `/Auth/Status` apresenta sessão Control iD válida | até 5 min |
| Contrato básico | `system_information.fcgi` retorna resposta simulada | até 2 min |
| Ambiente validado | `tools/test-readiness-gates.ps1` termina com código zero | conforme a máquina |

Os tempos são referências de diagnóstico, não objetivos de desempenho. Registre
SDK, sistema operacional e etapa quando houver desvio relevante; não contorne
restore bloqueado, segurança ou testes para reduzir o tempo de configuração.
