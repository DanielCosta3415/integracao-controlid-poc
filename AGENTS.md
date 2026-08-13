# AGENTS.md

> **Política** · Público: agentes de código e mantenedores · Responsável: Engenharia · Última validação: 2026-08-13.

Regras permanentes para Codex e outros agentes de código neste repositório.

## Visão geral

Este repositório é uma PoC web ASP.NET Core 10 MVC/Razor para integração operacional e técnica com a Access API da Control iD. A aplicação permite conexão com equipamento, autenticação, catálogo de endpoints oficiais, fluxos de hardware, cadastros, callbacks, monitoramento, fila push e persistência local em SQLite.

Trate o projeto como uma PoC operacional com pontos sensíveis de segurança, dados pessoais e integração com dispositivo físico. Diagnostique antes de alterar e registre falhas preexistentes separadamente de falhas introduzidas.

## Tecnologias detectadas

- Linguagem: C#, Razor, HTML, CSS, JavaScript e PowerShell.
- Runtime/SDK: .NET 10 LTS, SDK `10.0.302` pinado em `global.json`.
- Framework: ASP.NET Core MVC/Razor.
- Banco: SQLite via Entity Framework Core.
- Logs: Serilog em console e arquivo.
- OpenAPI/Swagger: Swashbuckle habilitado apenas em `Development` quando `OpenApi:Enabled=true`.
- Testes: xUnit, Playwright e axe.
- Verificação integrada/E2E local: PowerShell + simulador em `tools/ControlIdDeviceStub`.
- Proxy assinador: `tools/ControlIdCallbackSigningProxy` para equipamentos sem HMAC nativo.
- CI: GitHub Actions em `.github/workflows/ci.yml`.
- Package manager: NuGet com `packages.lock.json`.

## Estrutura principal

- `Program.cs`: composição da aplicação, DI, middlewares, SQLite e rotas de infraestrutura.
- `Services/Security/RuntimeSecurityValidator.cs`: invariantes de segurança obrigatórias fora de `Development`.
- `Controllers/`: fluxos MVC, endpoints oficiais auxiliares, callbacks e push.
- `Services/`: integrações Control iD, segurança, repositórios, navegação, factories e casos de uso.
- `Data/`: `IntegracaoControlIDContext`.
- `Models/`: entidades locais e modelos da API Control iD.
- `ViewModels/`: DTOs/view models usados pelas views.
- `Views/`: telas Razor.
- `Middlewares/`: tratamento de erro, logging, headers de segurança e sessão.
- `Options/`: opções de configuração tipadas.
- `tests/`: testes unitários xUnit.
- `tools/`: verificação integrada e simulador local de equipamento.
- `docs/`: documentação técnica, guias operacionais e relatórios.
- `wwwroot/`: assets estáticos e bibliotecas vendorizadas.

## Comandos reais

Execute comandos a partir da raiz do repositório, em PowerShell.

### Configuração

```powershell
dotnet restore .\Integracao.ControlID.PoC.sln --locked-mode
dotnet restore .\tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj --locked-mode
dotnet restore .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --locked-mode
dotnet tool restore
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

O teste integrado também inicia a aplicação e o simulador localmente:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\smoke-localhost.ps1
```

Backup local não destrutivo do SQLite:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\backup-sqlite.ps1
powershell -ExecutionPolicy Bypass -File .\tools\backup-sqlite-operational.ps1 -RunRestoreSmoke
powershell -ExecutionPolicy Bypass -File .\tools\restore-smoke-sqlite.ps1
powershell -ExecutionPolicy Bypass -File .\tools\harden-local-state.ps1
powershell -ExecutionPolicy Bypass -File .\tools\protect-sensitive-sqlite-data.ps1 -CertificatePath <arquivo-pfx> -CertificatePasswordFile <arquivo-senha> -ConfirmProtection
```

### Compilação, análise estática, formatação, verificação de tipos, testes e auditoria

```powershell
dotnet build .\Integracao.ControlID.PoC.sln --no-restore -v:minimal
dotnet build .\tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj --no-restore -v:minimal
dotnet build .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --no-restore -v:minimal
dotnet format .\Integracao.ControlID.PoC.sln --verify-no-changes --no-restore -v:minimal
dotnet format .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --verify-no-changes --no-restore -v:minimal
dotnet test .\tests\Integracao.ControlID.PoC.Tests\Integracao.ControlID.PoC.Tests.csproj --no-build -v:minimal
pwsh .\tests\Integracao.ControlID.PoC.E2E\bin\Debug\net10.0\playwright.ps1 install chromium
dotnet test .\tests\Integracao.ControlID.PoC.E2E\Integracao.ControlID.PoC.E2E.csproj --no-build -v:minimal
dotnet list .\Integracao.ControlID.PoC.sln package --vulnerable --include-transitive
dotnet list .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj package --vulnerable --include-transitive
powershell -ExecutionPolicy Bypass -File .\tools\audit-supply-chain.ps1
powershell -ExecutionPolicy Bypass -File .\tools\scan-secrets.ps1
powershell -ExecutionPolicy Bypass -File .\tools\validate-documentation.ps1
powershell -ExecutionPolicy Bypass -File .\tools\generate-sbom.ps1
powershell -ExecutionPolicy Bypass -File .\tools\observability-check.ps1 -OfflineValidateOnly
powershell -ExecutionPolicy Bypass -File .\tools\operational-readiness-check.ps1
powershell -ExecutionPolicy Bypass -File .\tools\finops-capacity-check.ps1
powershell -ExecutionPolicy Bypass -File .\tools\contract-controlid-stub.ps1
powershell -ExecutionPolicy Bypass -File .\tools\maintainability-check.ps1
powershell -ExecutionPolicy Bypass -File .\tools\performance-baseline.ps1 -FailOnBudget
powershell -ExecutionPolicy Bypass -File .\tools\external-security-scans.ps1 -InventoryOnly
powershell -ExecutionPolicy Bypass -File .\tools\audit-github-security.ps1
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

- Não existe análise estática separada; `dotnet build` com avisos como erro e `dotnet format --verify-no-changes` são as verificações oficiais.
- Não existe verificação de tipos separada; essa verificação é feita pela própria compilação C#.
- Para corrigir formatação, use `dotnet format .\Integracao.ControlID.PoC.sln -v:minimal` e registre o efeito mecânico.
- O teste integrado escreve em `artifacts/`, `Logs/` e no SQLite local; somente
  evidências sanitizadas e aprovadas devem ser promovidas para `docs/historico/relatorios/`.
- O gate `test-readiness-gates.ps1` executa observabilidade offline por padrão; contra app rodando, use `OBSERVABILITY_BASE_URL` e credencial local para `/metrics` quando necessário.
- `ops.example.json` define o contrato de ownership, on-call, backup externo, RTO/RPO, FinOps e contingência física. Copie para `ops.local.json` fora do Git para releases reais; `-ReleaseGate` exige essa configuração sem placeholders.
- `test-readiness-gates.ps1 -ReleaseGate` é o modo estrito para liberação: exige teste integrado, cobertura, cadeia de suprimentos, construção do contêiner, observabilidade on-line, configuração operacional, FinOps/capacidade, contrato físico e analisadores externos.
- A cobertura unitária usa Cobertura XML, com pisos de 28% de linhas e 16% de
  ramificações em `tools/validate-coverage.ps1`.
- Docker/Compose são artefatos de execução reproduzível local/em contêiner; não fazem implantação automática nem configuram provedor de nuvem.
- Scanners externos (`semgrep`, `osv-scanner`, `zap-baseline.py`, `axe`) são orquestrados por `tools/external-security-scans.ps1`; a instalação das CLIs fica no ambiente e deve ser justificada/registrada.
- O contrato com equipamento real é opcional, deve ser habilitado explicitamente e exige as variáveis `CONTROLID_DEVICE_URL`, `CONTROLID_USERNAME` e `CONTROLID_PASSWORD`: `powershell -ExecutionPolicy Bypass -File .\tools\contract-controlid-device.ps1`.

### Comandos indisponíveis ou não padronizados

- `npm`, `pnpm`, `yarn`: não há gerenciador de pacotes do frontend configurado.
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
- Consulte [docs/integracao-controlid/device-compatibility-matrix.md](docs/integracao-controlid/device-compatibility-matrix.md) e
  [docs/integracao-controlid/official-api-version-governance.md](docs/integracao-controlid/official-api-version-governance.md) antes de declarar suporte a produto,
  firmware, licença ou modo.
- Consulte [docs/integracao-controlid/network-topologies.md](docs/integracao-controlid/network-topologies.md) antes de alterar URL, porta, callback,
  Monitor, Push, proxy, DNS ou direção de comunicação.
- Consulte [docs/primeiros-passos/stub-scenarios.md](docs/primeiros-passos/stub-scenarios.md), [docs/primeiros-passos/validation-without-device.md](docs/primeiros-passos/validation-without-device.md) e
  [docs/integracao-controlid/endpoint-validation-matrix.md](docs/integracao-controlid/endpoint-validation-matrix.md) antes de promover evidência simulada para
  compatibilidade física.
- Preserve compatibilidade de callbacks oficiais e endpoints push (`/push`, `/result`, `Push/Receive`).
- Normalize entradas de URL, consulta, corpo e arquivo usando utilitários existentes quando disponíveis.
- Quando usar `ControlIdCallbackSigningProxy`, mantenha allowlist de IP, limite de body e remoção/reassinatura de headers sensíveis antes do encaminhamento.

### Banco de dados

- O SQLite local é o estado de execução. Arquivos `integracao_controlid.db*` não devem ser versionados.
- Novas gravações em colunas sensíveis usam proteção de dados por finalidade; preserve o chaveiro junto do banco em backup, restauração e reversão.
- Em `Development`, o chaveiro padrão fica em `artifacts/runtime/data-protection-keys`; não o apague se o banco local precisar continuar legível.
- A conversão de valores legados em texto simples exige backup, ensaio de restauração e `tools/protect-sensitive-sqlite-data.ps1 -ConfirmProtection`.
- `Program.cs` aplica `Database.Migrate()` quando `Database:ApplyMigrationsOnStartup=true` (por padrão em `Development`); iniciar a aplicação nessa condição altera o estado local.
- Mudanças de schema exigem documentação e testes. Migrações destrutivas exigem confirmação humana.
- Consulte [docs/dados/data-model-and-recovery.md](docs/dados/data-model-and-recovery.md) antes de tocar tabelas, índices, migrações, cópia de segurança, restauração ou retenção.
- Listagens locais devem aplicar limite padrão de `LocalDataQueryLimits.DefaultListLimit`; use métodos de expurgo/limpeza confirmados para operações destrutivas.

### Segurança

- Preserve a separação entre conta local da PoC e sessão oficial do equipamento;
  a matriz atual de papéis está em [docs/seguranca-privacidade/local-account-administration.md](docs/seguranca-privacidade/local-account-administration.md).
- A invocação manual do catálogo oficial é exclusiva de administrador e deve usar a sessão mantida no servidor; nunca devolva ou aceite o token pelo HTML.
- Fora de `Development`, `AllowedHosts` não pode ser `*`, `OpenApi:Enabled` deve ser `false`, HTTPS deve ser obrigatório, `CallbackSecurity:RequireSharedKey` e `CallbackSecurity:RequireSignedRequests` devem ser `true`, `SharedKey` deve existir e `ControlIDApi` deve exigir HTTPS e listar hosts permitidos.
- Preserve validação de callbacks, push e `user_get_image.fcgi` via `CallbackSecurityEvaluator` e `CallbackSignatureValidator`.
- Não enfraqueça headers de segurança, validação antiforgery ou sanitização sem justificativa forte.

### LGPD e privacidade

- Considere usuários, fotos, biometria, cartões, QR Codes, logs de acesso e callbacks como dados pessoais ou sensíveis.
- Minimize persistência e registro de cargas úteis pessoais. Mascare ou trunque quando possível.
- Não adicione dados reais a testes, documentos, verificações integradas ou dados de teste.
- Siga [docs/seguranca-privacidade/privacy-and-data-retention.md](docs/seguranca-privacidade/privacy-and-data-retention.md) ao tocar `MonitorEvents`, `PushCommands`, logs, payloads brutos ou limpeza de histórico local.

### Dependências

- Use NuGet lockfiles. A CI usa restore em modo locked.
- Atualização de pacote exige compilação, testes, verificação de formatação e auditoria da cadeia de suprimentos.
- Dependências frontend vendorizadas em `wwwroot/lib` devem estar declaradas em `wwwroot/lib/vendor-dependencies.json` e validadas por `tools/audit-vendor-dependencies.ps1`.
- Preferir patches compatíveis com .NET 10 a atualizações amplas de versão major.

### Desempenho

- Preserve compressão de resposta e evite carregar catálogos/payloads grandes desnecessariamente.
- Preserve o limitador por equipamento, o streaming binário e a política SQLite
  documentados em [docs/qualidade/performance-baseline.md](docs/qualidade/performance-baseline.md).
- Não adicione chamadas HTTP em loop sem timeout, cancelamento ou limite claro.
- Evite leitura integral de payloads grandes fora dos leitores com limite.

### UX e acessibilidade

- Preserve padrões Razor existentes, navegação do shell e mensagens de erro seguras.
- Não exponha stack trace, segredo, IP interno sensível ou payload bruto em tela.
- Ao alterar UI, valide texto, estados de erro, responsividade e acessibilidade básica.

### Testes

- Para regra nova, correção ou fortalecimento, adicione ou atualize testes unitários relevantes.
- Para fluxos HTTP amplos, rode smoke local quando aplicável.
- Não marque teste como skip sem justificativa documentada.

### Observabilidade

- Use `ILogger`/Serilog com contexto operacional seguro.
- Logs devem ajudar diagnóstico de endpoint, status, duração, command id e device id quando seguro.
- Nunca logue credenciais, shared key ou biometria bruta.

### Infraestrutura

- Dockerfile/Compose existem para execução reproduzível e validação de container; mantenha usuário não root, porta 8080, volumes `/app/data` e `/app/Logs`, e healthcheck em `/health/ready`.
- Fora de `Development`, preserve `DataProtection:KeyPath` em armazenamento
  persistente e protegido por certificado PKCS#12; em contêiner,
  `/app/data/data-protection-keys` deve acompanhar o SQLite nos procedimentos de
  backup, restauração e rollback.
- Fora de `Development`, ateste criptografia do volume que contém SQLite, logs e chaveiro; proteção de colunas não substitui criptografia integral do armazenamento.
- Não versione `ops.local.json`; ele pode conter nomes, canais privados, local de evidências e detalhes operacionais.
- Não crie implantação automática, DNS real ou provedor de nuvem sem pedido explícito.
- Não reduza retenção, registros de segurança ou redundância operacional apenas por custo; documente a contrapartida em [docs/operacao/finops-capacity.md](docs/operacao/finops-capacity.md).
- Fora de `Development`, não use `AllowedHosts=*`, chave compartilhada de exemplo, OpenAPI habilitado, métricas anônimas ou cabeçalhos encaminhados sem proxy conhecido.
- Mudanças em CI devem refletir comandos reais locais.
- Artefatos `bin/`, `obj/`, `Logs/`, `artifacts/` e banco local devem permanecer fora do Git.

### Documentação

- Use [docs/primeiros-passos/faq.md](docs/primeiros-passos/faq.md) como resposta canônica de primeiro contato e
  [docs/primeiros-passos/persona-guides.md](docs/primeiros-passos/persona-guides.md) como percurso por público; evite duplicar respostas
  extensas em documentos especializados.
- Atualize README/docs quando mudar setup, comando, segurança, banco, contrato externo, FinOps/capacidade ou fluxo operacional.
- Atualize [docs/README.md](docs/README.md) quando criar, remover ou renomear documento técnico.
- Atualize [docs/arquitetura/diagramas.md](docs/arquitetura/diagramas.md) quando
  criar, remover, renomear ou alterar o escopo de um diagrama; mantenha a visão
  no documento canônico do domínio e não duplique desenhos extensos.
- Registre decisão estrutural em `docs/adrs/` quando alterar padrão de arquitetura, persistência, segurança, observabilidade, release ou provedor.
- Registre mudanças amplas em `docs/historico/changelogs/` e resumos/auditorias
  datados em `docs/historico/auditorias/`.
- Atualize [docs/operacao/residual-risk-closure.md](docs/operacao/residual-risk-closure.md) quando uma lacuna externa virar gate, aprovação, exceção ou risco aceito.
- Atualize [docs/produto/product-acceptance-criteria.md](docs/produto/product-acceptance-criteria.md) quando um fluxo crítico ganhar, perder ou mudar critério verificável.
- Atualize [docs/integracao-controlid/api-error-catalog.md](docs/integracao-controlid/api-error-catalog.md), [docs/operacao/troubleshooting-controlid.md](docs/operacao/troubleshooting-controlid.md) e
  [docs/integracao-controlid/data-synchronization-ownership.md](docs/integracao-controlid/data-synchronization-ownership.md) quando mudar erro, diagnóstico ou fonte
  de verdade.
- Relatórios locais são gerados em `artifacts/`; promova apenas evidências
  sanitizadas para `docs/historico/relatorios/`, com data e contexto original.
- Não documente comandos que não existem no repositório.
- Preserve a classificação documental logo após o H1; documentos vivos devem
  refletir o código atual e evidências históricas devem permanecer datadas.
- Valide links Markdown, caminhos em crases, UTF-8, mojibake, blocos cercados e
  inventário com `tools/validate-documentation.ps1`.

### CI/CD e liberação

- A CI deve permanecer capaz de executar restauração bloqueada, compilação, teste, verificação de formatação e auditoria.
- Referencie GitHub Actions por SHA completo e comentário de versão; atualizações devem passar pelos mesmos gates locais.
- Preserve o CodeQL gerenciado pelo GitHub com conjunto estendido e os controles remotos de alertas, varredura de segredos e proteção contra envio de segredos; valide-os com `tools/audit-github-security.ps1`.
- Mudanças em `.github/workflows/ci.yml` devem manter [docs/qualidade/ci-cd-quality-gates.md](docs/qualidade/ci-cd-quality-gates.md) e os testes de governança de CI sincronizados.
- A liberação local mínima exige compilação limpa, testes aprovados, verificação de formatação limpa, auditoria sem vulnerabilidades conhecidas e riscos residuais documentados.
- A liberação operacional real exige `tools/test-readiness-gates.ps1 -ReleaseGate`, `ops.local.json` preenchido, cópia externa validada, RTO/RPO aprovado, FinOps/capacidade sem avisos, DPO/jurídico quando aplicável, analisadores externos e contingência do equipamento testada.
- Mudanças em `tools/ControlIdCallbackSigningProxy` exigem restauração bloqueada, compilação e verificação de formatação do projeto do proxy.
- Não publique uma liberação sem teste integrado quando a mudança tocar callbacks, push, catálogo oficial, autenticação ou banco.

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
- Limpeza destrutiva da área de trabalho (`git reset --hard`, `git clean -fdx`, exclusão recursiva).

## Matriz mínima de verificações

| Mudança | Verificações mínimas |
| --- | --- |
| Texto ou documentação | `tools/validate-documentation.ps1` e `git diff --check` |
| C# ou Razor | Compilação, formatação e testes relacionados |
| Banco ou migração | Compilação, testes de dados, cópia de segurança e ensaio não destrutivo |
| Callback, Push ou contrato Control iD | Testes relacionados e contrato com stub; equipamento real quando exigido para liberação |
| Segurança, privacidade ou logs | Testes relacionados, scan de segredos e revisão dos dados emitidos |
| CI, infraestrutura ou operação | Gate local correspondente e documentação operacional sincronizada |

Use o gate completo quando uma alteração atravessar mais de uma frente ou tocar
fluxo crítico. A matriz define o mínimo, não limita verificações adicionais.

## AGENTS.md por subdiretório

Não crie AGENTS.md adicionais sem evidência clara de regras divergentes. No estado atual, este arquivo raiz cobre o repositório.

Sugestões futuras, caso a área cresça:

- `tools/ControlIdDeviceStub/AGENTS.md`: regras específicas do stub e contratos simulados.
- `docs/historico/relatorios/AGENTS.md`: política específica apenas se o volume
  futuro exigir regras diferentes das definidas neste arquivo.
- `tests/AGENTS.md`: convenções de fixtures, nomes e cobertura mínima.

Crie esses arquivos apenas se houver necessidade concreta e documente o motivo no PR/resumo.
