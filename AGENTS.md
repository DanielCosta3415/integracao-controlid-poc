# AGENTS.md

Regras permanentes para Codex e outros agentes de código neste repositório.

## Visão geral

Este repositório é uma PoC web ASP.NET Core 8 MVC/Razor para integração operacional e técnica com a Access API da Control iD. A aplicação permite conexão com equipamento, autenticação, catálogo de endpoints oficiais, fluxos de hardware, cadastros, callbacks, monitoramento, fila push e persistência local em SQLite.

Trate o projeto como uma PoC operacional com pontos sensíveis de segurança, dados pessoais e integração com dispositivo físico. Diagnostique antes de alterar e registre falhas preexistentes separadamente de falhas introduzidas.

## Stack detectada

- Linguagem: C#, Razor, HTML, CSS, JavaScript e PowerShell.
- Runtime/SDK: .NET 8, SDK pinado em `global.json`.
- Framework: ASP.NET Core MVC/Razor.
- Banco: SQLite via Entity Framework Core.
- Logs: Serilog em console e arquivo.
- OpenAPI/Swagger: Swashbuckle habilitado apenas em `Development` quando `OpenApi:Enabled=true`.
- Testes: xUnit.
- Smoke/E2E local: PowerShell + stub em `tools/ControlIdDeviceStub`.
- Proxy assinador: `tools/ControlIdCallbackSigningProxy` para equipamentos sem HMAC nativo.
- CI: GitHub Actions em `.github/workflows/ci.yml`.
- Package manager: NuGet com `packages.lock.json`.

## Estrutura principal

- `Program.cs`: composição da aplicação, DI, middlewares, SQLite e validações de runtime.
- `Controllers/`: fluxos MVC, endpoints oficiais auxiliares, callbacks e push.
- `Services/`: integrações Control iD, segurança, repositórios, navegação, factories e casos de uso.
- `Data/`: `IntegracaoControlIDContext`.
- `Models/`: entidades locais e modelos da API Control iD.
- `ViewModels/`: DTOs/view models usados pelas views.
- `Views/`: telas Razor.
- `Middlewares/`: tratamento de erro, logging, headers de segurança e sessão.
- `Options/`: opções de configuração tipadas.
- `tests/`: testes unitários xUnit.
- `tools/`: smoke test e stub local de equipamento.
- `docs/`: documentação técnica, runbooks e relatórios.
- `wwwroot/`: assets estáticos e bibliotecas vendorizadas.

## Comandos reais

Execute comandos a partir da raiz do repositório, em PowerShell.

### Configuração

```powershell
dotnet restore .\Integracao.ControlID.PoC.sln --locked-mode
dotnet restore .\tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj --locked-mode
dotnet restore .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --locked-mode
```

Para desenvolvimento local, configure segredos fora do repositório, usando placeholders:

```powershell
dotnet user-secrets set "ControlIDApi:DefaultDeviceUrl" "http://<equipamento-ou-host>:8080"
dotnet user-secrets set "ControlIDApi:DefaultUsername" "<usuario>"
dotnet user-secrets set "ControlIDApi:DefaultPassword" "<senha>"
dotnet user-secrets set "CallbackSecurity:SharedKey" "<segredo-local>"
dotnet user-secrets set "CallbackSecurity:RequireSharedKey" "true"
dotnet user-secrets set "CallbackSecurity:RequireSignedRequests" "true"
dotnet user-secrets set "ControlIDApi:RequireAllowedDeviceHosts" "true"
dotnet user-secrets set "ControlIDApi:AllowedDeviceHosts:0" "<equipamento-ou-host>"
```

Para equipamentos sem assinatura HMAC nativa, configure o proxy assinador com segredos fora do repositório:

```powershell
dotnet user-secrets set --project .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj "Proxy:SharedKey" "<mesmo-segredo-da-poc>"
dotnet run --project .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --urls http://localhost:6700
```

### Execução local

```powershell
dotnet run --project .\Integracao.ControlID.PoC.csproj
dotnet run --project .\tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj --no-launch-profile
dotnet run --project .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --urls http://localhost:6700
```

O smoke test também sobe app e stub localmente:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\smoke-localhost.ps1
```

Backup local não destrutivo do SQLite:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\backup-sqlite.ps1
powershell -ExecutionPolicy Bypass -File .\tools\backup-sqlite-operational.ps1 -RunRestoreSmoke
powershell -ExecutionPolicy Bypass -File .\tools\restore-smoke-sqlite.ps1
powershell -ExecutionPolicy Bypass -File .\tools\harden-local-state.ps1
```

### Compilação, lint, formatação, verificação de tipos, testes e auditoria

```powershell
dotnet build .\Integracao.ControlID.PoC.sln --no-restore -v:minimal
dotnet build .\tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj --no-restore -v:minimal
dotnet build .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --no-restore -v:minimal
dotnet format .\Integracao.ControlID.PoC.sln --verify-no-changes --no-restore -v:minimal
dotnet format .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --verify-no-changes --no-restore -v:minimal
dotnet test .\Integracao.ControlID.PoC.sln --no-build -v:minimal
dotnet list .\Integracao.ControlID.PoC.sln package --vulnerable --include-transitive
dotnet list .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj package --vulnerable --include-transitive
powershell -ExecutionPolicy Bypass -File .\tools\audit-supply-chain.ps1
powershell -ExecutionPolicy Bypass -File .\tools\scan-secrets.ps1
powershell -ExecutionPolicy Bypass -File .\tools\generate-sbom.ps1
powershell -ExecutionPolicy Bypass -File .\tools\observability-check.ps1 -OfflineValidateOnly
powershell -ExecutionPolicy Bypass -File .\tools\operational-readiness-check.ps1
powershell -ExecutionPolicy Bypass -File .\tools\finops-capacity-check.ps1
powershell -ExecutionPolicy Bypass -File .\tools\contract-controlid-stub.ps1
powershell -ExecutionPolicy Bypass -File .\tools\external-security-scans.ps1 -InventoryOnly
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunCoverage
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunContainerBuild
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunObservabilityOnline -RequireObservabilityMetrics
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunExternalScanners -RequireExternalScanners
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -ReleaseGate
docker build -t integracao-controlid-poc:local .
docker compose config
git diff --check
```

Notas:

- Lint separado não existe; `dotnet build` com warnings como erro e `dotnet format --verify-no-changes` são os checks oficiais.
- Typecheck separado não existe; o typecheck é o próprio build C#.
- Para corrigir formatação, use `dotnet format .\Integracao.ControlID.PoC.sln -v:minimal` e registre o efeito mecânico.
- O smoke test escreve em `docs/reports/`, `artifacts/`, `Logs/` e no SQLite local.
- O gate `test-readiness-gates.ps1` executa observabilidade offline por padrão; contra app rodando, use `OBSERVABILITY_BASE_URL` e credencial local para `/metrics` quando necessário.
- `ops.example.json` define o contrato de ownership, on-call, backup externo, RTO/RPO, FinOps e contingência física. Copie para `ops.local.json` fora do Git para releases reais; `-ReleaseGate` exige essa configuração sem placeholders.
- `test-readiness-gates.ps1 -ReleaseGate` é o modo estrito para release: exige smoke, cobertura, cadeia de suprimentos, construção do contêiner, observabilidade on-line, configuração operacional, FinOps/capacidade, contrato físico e scanners externos.
- Docker/Compose são artefatos de execução reproduzível local/container; não fazem deploy automático nem configuram provedor cloud.
- Scanners externos (`semgrep`, `osv-scanner`, `zap-baseline.py`, `axe`) são orquestrados por `tools/external-security-scans.ps1`; a instalação das CLIs fica no ambiente e deve ser justificada/registrada.
- O contrato com equipamento real é opcional, deve ser habilitado explicitamente e exige as variáveis `CONTROLID_DEVICE_URL`, `CONTROLID_USERNAME` e `CONTROLID_PASSWORD`: `powershell -ExecutionPolicy Bypass -File .\tools\contract-controlid-device.ps1`.

### Comandos indisponíveis ou não padronizados

- `npm`, `pnpm`, `yarn`: não há frontend package manager configurado.
- Migrations CLI destrutivas: não há fluxo documentado; não execute sem aprovação humana.
- Deploy automático/provedor cloud: não há provedor/manifesto de hospedagem versionado.

## Regras obrigatórias

- Preserve contratos públicos de rotas, payloads, callbacks, push e ViewModels, salvo pedido explícito ou versionamento documentado.
- Não altere regra de negócio sem evidência em README, docs, testes, código existente ou confirmação humana.
- Não remova dependências sem análise de impacto, busca de uso e validação dos checks.
- Não adicione dependências sem justificar necessidade, alternativa e risco.
- Não execute migrações destrutivas, exclusão de dados ou limpeza de banco sem confirmação humana.
- Não apague dados locais versionados ou relatórios históricos sem confirmação humana.
- Não crie abstrações prematuras; siga os padrões existentes de controller, service, repository e ViewModel.
- Não misture refatoração ampla com feature ou bugfix pontual.
- Não use `catch` vazio e não engula exceções. Logue contexto seguro e retorne erro apropriado.
- Não logue senha, shared key, token, certificado privado, biometria bruta ou dado pessoal desnecessário.
- Não exponha secrets em código, docs, logs, commits, exemplos reais ou screenshots.
- Sempre rode checks relevantes antes de finalizar. Se algum check não for executado, explique o motivo.

## Regras por frente

### Arquitetura

- Mantenha controllers finos quando possível; regras reutilizáveis devem ficar em `Services/`.
- Repositórios em `Services/Database/` devem encapsular acesso EF/SQLite.
- Evite acoplamento novo entre controllers; compartilhe via services existentes.

### APIs e integrações

- Trate a Access API Control iD como contrato externo. Não renomeie endpoints, campos ou rotas `.fcgi` sem evidência.
- Preserve compatibilidade de callbacks oficiais e endpoints push (`/push`, `/result`, `Push/Receive`).
- Normalize entradas de URL, query, body e arquivo usando utilitários existentes quando disponíveis.
- Quando usar `ControlIdCallbackSigningProxy`, mantenha allowlist de IP, limite de body e remoção/reassinatura de headers sensíveis antes do encaminhamento.

### Banco de dados

- O SQLite local é o estado de execução. Arquivos `integracao_controlid.db*` não devem ser versionados.
- `Program.cs` aplica `Database.Migrate()` quando `Database:ApplyMigrationsOnStartup=true` (por padrão em `Development`); iniciar a aplicação nessa condição altera o estado local.
- Mudanças de schema exigem documentação e testes. Migrações destrutivas exigem confirmação humana.
- Consulte `docs/data-model-and-recovery.md` antes de tocar tabelas, índices, migrations, backup, restore ou retenção.
- Listagens locais devem aplicar limite padrão de `LocalDataQueryLimits.DefaultListLimit`; use métodos de expurgo/limpeza confirmados para operações destrutivas.

### Segurança

- Fora de `Development`, `AllowedHosts` não pode ser `*`, `OpenApi:Enabled` deve ser `false`, `CallbackSecurity:RequireSharedKey` e `CallbackSecurity:RequireSignedRequests` devem ser `true`, `SharedKey` deve existir e `ControlIDApi:RequireAllowedDeviceHosts` deve listar hosts permitidos.
- Preserve validação de callbacks, push e `user_get_image.fcgi` via `CallbackSecurityEvaluator` e `CallbackSignatureValidator`.
- Não enfraqueça headers de segurança, validação antiforgery ou sanitização sem justificativa forte.

### LGPD e privacidade

- Considere usuários, fotos, biometria, cartões, QR Codes, logs de acesso e callbacks como dados pessoais ou sensíveis.
- Minimize persistência e logging de payloads pessoais. Mascarar ou truncar quando possível.
- Não adicione dados reais a testes, docs, smoke ou fixtures.
- Siga `docs/privacy-and-data-retention.md` ao tocar `MonitorEvents`, `PushCommands`, logs, payloads brutos ou limpeza de histórico local.

### Dependências

- Use NuGet lockfiles. A CI usa restore em modo locked.
- Atualização de pacote exige build, testes, format check e auditoria de supply chain.
- Dependências frontend vendorizadas em `wwwroot/lib` devem estar declaradas em `wwwroot/lib/vendor-dependencies.json` e validadas por `tools/audit-vendor-dependencies.ps1`.
- Preferir patches compatíveis com .NET 8 a upgrades amplos de major version.

### Desempenho

- Preserve compressão de resposta e evite carregar catálogos/payloads grandes desnecessariamente.
- Não adicione chamadas HTTP em loop sem timeout, cancelamento ou limite claro.
- Evite leitura integral de payloads grandes fora dos leitores com limite.

### UX e acessibilidade

- Preserve padrões Razor existentes, navegação do shell e mensagens de erro seguras.
- Não exponha stack trace, segredo, IP interno sensível ou payload bruto em tela.
- Ao alterar UI, valide texto, estados de erro, responsividade e acessibilidade básica.

### Testes

- Para regra nova, bugfix ou hardening, adicione/atualize testes unitários relevantes.
- Para fluxos HTTP amplos, rode smoke local quando aplicável.
- Não marque teste como skip sem justificativa documentada.

### Observabilidade

- Use `ILogger`/Serilog com contexto operacional seguro.
- Logs devem ajudar diagnóstico de endpoint, status, duração, command id e device id quando seguro.
- Nunca logue credenciais, shared key ou biometria bruta.

### Infraestrutura

- Dockerfile/Compose existem para execução reproduzível e validação de container; mantenha usuário não root, porta 8080, volumes `/app/data` e `/app/Logs`, e healthcheck em `/health/ready`.
- Não versione `ops.local.json`; ele pode conter nomes, canais privados, local de evidências e detalhes operacionais.
- Não crie deploy automático, DNS real ou provedor cloud sem pedido explícito.
- Não reduzir retenção, logs de segurança ou redundancia operacional apenas por custo; documente trade-off em `docs/finops-capacity.md`.
- Fora de `Development`, não use `AllowedHosts=*`, chave compartilhada de exemplo, OpenAPI habilitado, métricas anônimas ou cabeçalhos encaminhados sem proxy conhecido.
- Mudanças em CI devem refletir comandos reais locais.
- Artefatos `bin/`, `obj/`, `Logs/`, `artifacts/` e banco local devem permanecer fora do Git.

### Documentação

- Atualize README/docs quando mudar setup, comando, segurança, banco, contrato externo, FinOps/capacidade ou fluxo operacional.
- Atualize `docs/README.md` quando criar, remover ou renomear documento técnico.
- Registre decisão estrutural em `docs/adrs/` quando alterar padrão de arquitetura, persistência, segurança, observabilidade, release ou provedor.
- Atualize `docs/changelog-YYYY-MM-DD.md` ou `docs/pr-summary-YYYY-MM-DD.md` em rodadas amplas de governança/documentação.
- Atualize `docs/residual-risk-closure.md` quando uma lacuna externa virar gate, aprovação, exceção ou risco aceito.
- Atualize `docs/product-acceptance-criteria.md` quando um fluxo crítico ganhar, perder ou mudar criterio verificável.
- Relatórios em `docs/reports/` podem ser gerados por smoke/auditoria; registre data e resultado.
- Não documente comandos que não existem no repositório.

### CI/CD e release

- A CI deve permanecer capaz de rodar restore locked, build, teste, format check e auditoria.
- Mudanças em `.github/workflows/ci.yml` devem manter `docs/ci-cd-quality-gates.md` e os testes de governança de CI sincronizados.
- Release local mínima exige build limpo, testes passando, format check limpo, auditoria sem vulnerabilidades conhecidas e riscos residuais documentados.
- Release operacional real exige `tools/test-readiness-gates.ps1 -ReleaseGate`, `ops.local.json` preenchido, backup externo validado, RTO/RPO aprovado, FinOps/capacidade sem warnings, DPO/jurídico quando aplicável, scanners externos e contingência do equipamento testada.
- Mudanças em `tools/ControlIdCallbackSigningProxy` exigem restore locked, build e format check do projeto do proxy.
- Não publique release sem smoke quando a mudança tocar callbacks, push, catálogo oficial, autenticação ou banco.

## Definição Técnica de Pronto

Antes de finalizar uma tarefa, confirme:

- Código ou documentação implementado conforme escopo.
- Contratos públicos preservados ou alteração versionada/documentada.
- Testes relevantes criados ou atualizados.
- Checks relevantes executados e resultado informado.
- Documentação atualizada quando o comportamento/setup mudou.
- Riscos residuais e checks não executados documentados.
- Arquivos alterados listados no resumo final.

## Ações proibidas sem confirmação humana

- Commit.
- Push.
- Deploy ou publicação de release.
- Migração destrutiva.
- Exclusão de dados, logs históricos ou relatórios versionados.
- Troca de provedor de hospedagem, banco ou CI.
- Alteração de contrato público de API, rota, callback ou payload.
- Remoção de dependência central.
- Exposição, cópia ou persistência de secrets reais.
- Alteração de configuração de produção.
- Limpeza destrutiva de workspace (`git reset --hard`, `git clean -fdx`, deleção recursiva).

## AGENTS.md por subdiretório

Não crie AGENTS.md adicionais sem evidência clara de regras divergentes. No estado atual, este arquivo raiz cobre o repositório.

Sugestoes futuras, caso a área cresca:

- `tools/ControlIdDeviceStub/AGENTS.md`: regras específicas do stub e contratos simulados.
- `docs/reports/AGENTS.md`: política de relatórios gerados, datas e preservação histórica.
- `tests/AGENTS.md`: convenções de fixtures, nomes e cobertura mínima.

Crie esses arquivos apenas se houver necessidade concreta e documente o motivo no PR/resumo.
