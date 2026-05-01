# Visao de arquitetura

Este documento resume a arquitetura atual da PoC e aponta onde cada tipo de
mudanca deve acontecer. Ele nao substitui os documentos de dominio; serve como
mapa de alto nivel.

## Estilo arquitetural

A aplicacao e uma monolito web ASP.NET Core MVC/Razor com:

- apresentacao em controllers, views e ViewModels;
- services para integracao, regras reutilizaveis, observabilidade e seguranca;
- repositorios EF Core/SQLite para estado local;
- scripts PowerShell para diagnostico, smoke, readiness e operacao;
- Docker/Compose para execucao reproduzivel sem definir provedor cloud.

## Camadas

| Camada | Responsabilidade | Exemplos |
| --- | --- | --- |
| Apresentacao | Rotas MVC, input da UI e renderizacao Razor | `Controllers/`, `Views/`, `ViewModels/` |
| Aplicacao/services | Orquestracao de fluxos, validacoes reutilizaveis e composicao | `Services/ControlIDApi/`, `Services/Callbacks/`, `Services/Push/` |
| Dominio de integracao | Contratos e modelos da Access API Control iD | `Models/ControlIDApi/`, catalogo oficial |
| Persistencia local | Estado runtime, historicos, Push, callbacks e usuarios locais | `Data/`, `Models/Database/`, `Services/Database/` |
| Cross-cutting | Segurança, logs, metricas, headers, correlation ID e performance | `Middlewares/`, `Services/Observability/`, `Services/Security/` |
| Operacao | Smoke, backups, scanners, readiness, FinOps e DR | `tools/`, `docs/*runbook*.md` |

## Fluxos criticos

| Fluxo | Entrada | Orquestracao | Persistencia/saida |
| --- | --- | --- | --- |
| Login local | `AuthController.LocalLogin` | Auth local, cookie, rate limit | Cookie auth e metricas de auth |
| Login Control iD | `AuthController.Login` | `OfficialControlIdApiService`/invoker | Sessao ASP.NET e logs seguros |
| Catalogo oficial | `OfficialApiController` | `OfficialApiCatalogService` e docs de contrato | ViewModels e resposta visual |
| Invocacao oficial | `OfficialApiController.Invoke` | `OfficialApiInvokerService`, timeout, circuit breaker | Resultado sanitizado, logs e metricas |
| Callback/monitor | callbacks `.fcgi` e `/api/notifications/*` | `CallbackIngressService`, body reader e security evaluator | `MonitorEvents`, logs e metricas |
| Push | `GET /push`, `POST /result`, `PushCenter` | `PushCommandWorkflowService` | `PushCommands`, estados e metricas |
| Banco/backup | startup, repositorios e scripts | EF Core migrations, backup/restore smoke | SQLite local e artefatos fora do Git |
| Observabilidade | middlewares e `/metrics` | `OperationalMetrics`, `PrometheusMetricsWriter` | Prometheus text, dashboards e alertas |

## Dependencias externas

| Dependencia | Tipo | Controle |
| --- | --- | --- |
| Equipamento Control iD | API HTTP local/externa | Timeout, allowlist, HMAC em callbacks, contrato stub e contrato fisico opt-in |
| SQLite | Arquivo local/volume | Health check, migrations, indices, backup e restore-smoke |
| GitHub Actions/NuGet | CI/dependencias | Lockfiles, audit e supply-chain review |
| Scanners externos | Ferramentas opcionais | `tools/external-security-scans.ps1` e release gate |

Nao ha provedor cloud, storage externo, cache distribuido, broker, e-mail/SMS ou
analytics externo versionado.

## Trust boundaries

- Browser do operador -> aplicacao MVC.
- Aplicacao -> equipamento Control iD.
- Equipamento -> callbacks/Push expostos pela PoC.
- Aplicacao -> SQLite local.
- Operador/CI -> scripts PowerShell e artefatos locais.
- Host/provedor futuro -> logs, volumes, metricas e backups.

Fora de `Development`, a aplicacao deve falhar no startup se configuracoes
criticas estiverem inseguras: `AllowedHosts=*`, metrics anonimo, shared key
ausente, assinatura HMAC ausente, OpenAPI habilitado sem decisao e forwarded
headers sem proxy conhecido.

## Contratos a preservar

- Rotas MVC usadas pela UI.
- Endpoints oficiais auxiliares, callbacks `.fcgi`, `/push`, `/result` e
  `Push/Receive`.
- Payloads e nomes de campos esperados pela Access API Control iD.
- ViewModels publicamente consumidos pelas views.
- Metricas e labels ja documentadas em `docs/observability-runbook.md`.
- Scripts oficiais citados em README, AGENTS e CI.

## Decisoes arquiteturais

ADRs atuais:

- `docs/adrs/0001-local-sqlite-runtime-state.md`
- `docs/adrs/0002-secure-controlid-ingress-and-egress.md`
- `docs/adrs/0003-in-process-observability-and-readiness-gates.md`
- `docs/adrs/0004-release-governance-with-local-scripts.md`

Crie novo ADR quando uma decisao alterar padrao de arquitetura, provedor,
persistencia, seguranca, observabilidade, release ou contrato publico.

## Limites conhecidos

- SQLite simplifica a PoC, mas nao substitui um desenho de banco multi-instancia.
- Contrato com equipamento real depende de hardware, rede, firmware e credenciais
  fora do Git; release estrito bloqueia sem contrato fisico validado.
- Billing, DNS, TLS real e sizing de producao dependem de provedor escolhido;
  `ops.example.json` e `operational-readiness-check.ps1 -RequireConfig` exigem
  donos, status e evidencias antes de release.
- Bases legais, DPA, RIPD e titulares reais dependem de DPO/juridico; os campos
  `privacy.*` agora sao obrigatorios em configuracao operacional real.
- Scanners externos dependem de instalacao local ou ambiente CI preparado; o gate
  `-ReleaseGate` exige as ferramentas e os relatorios.
- O fechamento detalhado desses riscos externos fica em
  `docs/residual-risk-closure.md`.
