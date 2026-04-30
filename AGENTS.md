# AGENTS.md

Regras permanentes para Codex e outros agentes de codigo neste repositorio.

## Visao geral

Este repositorio e uma PoC web ASP.NET Core 8 MVC/Razor para integracao operacional e tecnica com a Access API da Control iD. A aplicacao permite conexao com equipamento, autenticacao, catalogo de endpoints oficiais, fluxos de hardware, cadastros, callbacks, monitoramento, fila push e persistencia local em SQLite.

Trate o projeto como uma PoC operacional com pontos sensiveis de seguranca, dados pessoais e integracao com dispositivo fisico. Diagnostique antes de alterar e registre falhas preexistentes separadamente de falhas introduzidas.

## Stack detectada

- Linguagem: C#, Razor, HTML, CSS, JavaScript e PowerShell.
- Runtime/SDK: .NET 8, SDK pinado em `global.json`.
- Framework: ASP.NET Core MVC/Razor.
- Banco: SQLite via Entity Framework Core.
- Logs: Serilog em console e arquivo.
- Testes: xUnit.
- Smoke/E2E local: PowerShell + stub em `tools/ControlIdDeviceStub`.
- CI: GitHub Actions em `.github/workflows/ci.yml`.
- Package manager: NuGet com `packages.lock.json`.

## Estrutura principal

- `Program.cs`: composicao da aplicacao, DI, middlewares, SQLite e validacoes de runtime.
- `Controllers/`: fluxos MVC, endpoints oficiais auxiliares, callbacks e push.
- `Services/`: integracoes Control iD, seguranca, repositorios, navegacao, factories e casos de uso.
- `Data/`: `IntegracaoControlIDContext`.
- `Models/`: entidades locais e modelos da API Control iD.
- `ViewModels/`: DTOs/view models usados pelas views.
- `Views/`: telas Razor.
- `Middlewares/`: tratamento de erro, logging, headers de seguranca e sessao.
- `Options/`: opcoes de configuracao tipadas.
- `tests/`: testes unitarios xUnit.
- `tools/`: smoke test e stub local de equipamento.
- `docs/`: documentacao tecnica, runbooks e relatorios.
- `wwwroot/`: assets estaticos e bibliotecas vendorizadas.

## Comandos reais

Execute comandos a partir da raiz do repositorio, em PowerShell.

### Setup

```powershell
dotnet restore .\Integracao.ControlID.PoC.sln --locked-mode
dotnet restore .\tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj --locked-mode
```

Para desenvolvimento local, configure segredos fora do repositorio, usando placeholders:

```powershell
dotnet user-secrets set "ControlIDApi:DefaultDeviceUrl" "http://<equipamento-ou-host>:8080"
dotnet user-secrets set "ControlIDApi:DefaultUsername" "<usuario>"
dotnet user-secrets set "ControlIDApi:DefaultPassword" "<senha>"
dotnet user-secrets set "CallbackSecurity:SharedKey" "<segredo-local>"
```

### Execucao local

```powershell
dotnet run --project .\Integracao.ControlID.PoC.csproj
dotnet run --project .\tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj --no-launch-profile
```

O smoke test tambem sobe app e stub localmente:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\smoke-localhost.ps1
```

### Build, lint, format, typecheck, testes e auditoria

```powershell
dotnet build .\Integracao.ControlID.PoC.sln --no-restore -v:minimal
dotnet build .\tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj --no-restore -v:minimal
dotnet format .\Integracao.ControlID.PoC.sln --verify-no-changes --no-restore -v:minimal
dotnet test .\Integracao.ControlID.PoC.sln --no-build -v:minimal
dotnet list .\Integracao.ControlID.PoC.sln package --vulnerable --include-transitive
git diff --check
```

Notas:

- Lint separado nao existe; `dotnet build` com warnings como erro e `dotnet format --verify-no-changes` sao os checks oficiais.
- Typecheck separado nao existe; o typecheck e o build C#.
- Para corrigir formatacao, use `dotnet format .\Integracao.ControlID.PoC.sln -v:minimal` e registre o efeito mecanico.
- O smoke test escreve em `docs/reports/`, `artifacts/`, `Logs/` e no SQLite local.

### Comandos indisponiveis ou nao padronizados

- `npm`, `pnpm`, `yarn`: nao ha frontend package manager configurado.
- Docker/Compose: nao ha Dockerfile ou compose.
- Secret scanner dedicado: nao ha ferramenta configurada no repo.
- Migrations CLI destrutivas: nao ha fluxo documentado; nao execute sem aprovacao humana.
- Deploy: nao ha provedor/manifesto de hospedagem versionado.

## Regras obrigatorias

- Preserve contratos publicos de rotas, payloads, callbacks, push e ViewModels, salvo pedido explicito ou versionamento documentado.
- Nao altere regra de negocio sem evidencia em README, docs, testes, codigo existente ou confirmacao humana.
- Nao remova dependencias sem analise de impacto, busca de uso e validacao dos checks.
- Nao adicione dependencias sem justificar necessidade, alternativa e risco.
- Nao execute migracoes destrutivas, exclusao de dados ou limpeza de banco sem confirmacao humana.
- Nao apague dados locais versionados ou relatorios historicos sem confirmacao humana.
- Nao crie abstracoes prematuras; siga os padroes existentes de controller, service, repository e ViewModel.
- Nao misture refatoracao ampla com feature ou bugfix pontual.
- Nao use `catch` vazio e nao engula excecoes. Logue contexto seguro e retorne erro apropriado.
- Nao logue senha, shared key, token, certificado privado, biometria bruta ou dado pessoal desnecessario.
- Nao exponha secrets em codigo, docs, logs, commits, exemplos reais ou screenshots.
- Sempre rode checks relevantes antes de finalizar. Se algum check nao for executado, explique o motivo.

## Regras por frente

### Arquitetura

- Mantenha controllers finos quando possivel; regras reutilizaveis devem ficar em `Services/`.
- Repositorios em `Services/Database/` devem encapsular acesso EF/SQLite.
- Evite acoplamento novo entre controllers; compartilhe via services existentes.

### APIs e integracoes

- Trate a Access API Control iD como contrato externo. Nao renomeie endpoints, campos ou rotas `.fcgi` sem evidencia.
- Preserve compatibilidade de callbacks oficiais e endpoints push (`/push`, `/result`, `Push/Receive`).
- Normalize entradas de URL, query, body e arquivo usando utilitarios existentes quando disponiveis.

### Banco de dados

- O SQLite local e estado runtime. Arquivos `integracao_controlid.db*` nao devem ser versionados.
- `Program.cs` aplica `Database.Migrate()` e cria tabelas auxiliares idempotentes; iniciar app altera estado local.
- Mudancas de schema exigem documentacao e testes. Migracoes destrutivas exigem confirmacao humana.

### Seguranca

- Fora de `Development`, `AllowedHosts` nao pode ser `*`, `CallbackSecurity:RequireSharedKey` deve ser `true` e `SharedKey` deve existir.
- Preserve validacao de callbacks e push via `CallbackSecurityEvaluator`.
- Nao enfraqueca headers de seguranca, validacao antiforgery ou sanitizacao sem justificativa forte.

### LGPD e privacidade

- Considere usuarios, fotos, biometria, cartoes, QR Codes, logs de acesso e callbacks como dados pessoais ou sensiveis.
- Minimize persistencia e logging de payloads pessoais. Mascarar ou truncar quando possivel.
- Nao adicione dados reais a testes, docs, smoke ou fixtures.
- Siga `docs/privacy-and-data-retention.md` ao tocar `MonitorEvents`, `PushCommands`, logs, payloads brutos ou limpeza de historico local.

### Dependencias

- Use NuGet lockfiles. A CI usa restore em modo locked.
- Atualizacao de pacote exige build, testes, format check e auditoria de vulnerabilidade.
- Preferir patches compativeis com .NET 8 a upgrades amplos de major version.

### Performance

- Preserve compressao de resposta e evite carregar catalogos/payloads grandes desnecessariamente.
- Nao adicione chamadas HTTP em loop sem timeout, cancelamento ou limite claro.
- Evite leitura integral de payloads grandes fora dos leitores com limite.

### UX e acessibilidade

- Preserve padroes Razor existentes, navegacao do shell e mensagens de erro seguras.
- Nao exponha stack trace, segredo, IP interno sensivel ou payload bruto em tela.
- Ao alterar UI, valide texto, estados de erro, responsividade e acessibilidade basica.

### Testes

- Para regra nova, bugfix ou hardening, adicione/atualize testes unitarios relevantes.
- Para fluxos HTTP amplos, rode smoke local quando aplicavel.
- Nao marque teste como skip sem justificativa documentada.

### Observabilidade

- Use `ILogger`/Serilog com contexto operacional seguro.
- Logs devem ajudar diagnostico de endpoint, status, duracao, command id e device id quando seguro.
- Nunca logue credenciais, shared key ou biometria bruta.

### Infraestrutura

- Nao invente Docker, deploy ou provedor sem pedido explicito.
- Mudancas em CI devem refletir comandos reais locais.
- Artefatos `bin/`, `obj/`, `Logs/`, `artifacts/` e banco local devem permanecer fora do Git.

### Documentacao

- Atualize README/docs quando mudar setup, comando, seguranca, banco, contrato externo ou fluxo operacional.
- Atualize `docs/product-acceptance-criteria.md` quando um fluxo critico ganhar, perder ou mudar criterio verificavel.
- Relatorios em `docs/reports/` podem ser gerados por smoke/auditoria; registre data e resultado.
- Nao documente comandos que nao existem no repositorio.

### CI/CD e release

- A CI deve permanecer capaz de rodar restore locked, build, teste, format check e auditoria.
- Release local minima exige build limpo, testes passando, format check limpo, auditoria sem vulnerabilidades conhecidas e riscos residuais documentados.
- Nao publique release sem smoke quando a mudanca tocar callbacks, push, catalogo oficial, autenticacao ou banco.

## Definition of Done tecnica

Antes de finalizar uma tarefa, confirme:

- Codigo ou documentacao implementado conforme escopo.
- Contratos publicos preservados ou alteracao versionada/documentada.
- Testes relevantes criados ou atualizados.
- Checks relevantes executados e resultado informado.
- Documentacao atualizada quando o comportamento/setup mudou.
- Riscos residuais e checks nao executados documentados.
- Arquivos alterados listados no resumo final.

## Acoes proibidas sem confirmacao humana

- Commit.
- Push.
- Deploy ou publicacao de release.
- Migracao destrutiva.
- Exclusao de dados, logs historicos ou relatorios versionados.
- Troca de provedor de hospedagem, banco ou CI.
- Alteracao de contrato publico de API, rota, callback ou payload.
- Remocao de dependencia central.
- Exposicao, copia ou persistencia de secrets reais.
- Alteracao de configuracao de producao.
- Limpeza destrutiva de workspace (`git reset --hard`, `git clean -fdx`, delecao recursiva).

## AGENTS.md por subdiretorio

Nao crie AGENTS.md adicionais sem evidencia clara de regras divergentes. No estado atual, este arquivo raiz cobre o repositorio.

Sugestoes futuras, caso a area cresca:

- `tools/ControlIdDeviceStub/AGENTS.md`: regras especificas do stub e contratos simulados.
- `docs/reports/AGENTS.md`: politica de relatorios gerados, datas e preservacao historica.
- `tests/AGENTS.md`: convencoes de fixtures, nomes e cobertura minima.

Crie esses arquivos apenas se houver necessidade concreta e documente o motivo no PR/resumo.
