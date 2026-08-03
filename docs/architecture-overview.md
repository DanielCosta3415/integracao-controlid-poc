# Visão de arquitetura

Este documento resume a arquitetura atual da PoC e aponta onde cada tipo de
mudança deve acontecer. Ele não substitui os documentos de domínio; serve como
mapa de alto nível.

## Estilo arquitetural

A aplicação é um monólito web ASP.NET Core MVC/Razor com:

- apresentação em controllers, views e ViewModels;
- services para integração, regras reutilizáveis, observabilidade e segurança;
- repositórios EF Core/SQLite para estado local;
- scripts PowerShell para diagnóstico, smoke, readiness e operação;
- Docker/Compose para execução reproduzível sem definir provedor cloud.

## Camadas

| Camada | Responsabilidade | Exemplos |
| --- | --- | --- |
| Apresentação | Rotas MVC, input da UI e renderização Razor | `Controllers/`, `Views/`, `ViewModels/` |
| Aplicação/services | Orquestração de fluxos, validações reutilizáveis e composição | `Services/ControlIDApi/`, `Services/Callbacks/`, `Services/Push/` |
| Domínio de integração | Contratos e modelos da Access API Control iD | `Models/ControlIDApi/`, catálogo oficial |
| Persistência local | Estado runtime, históricos, Push, callbacks e usuários locais | `Data/`, `Models/Database/`, `Services/Database/` |
| Cross-cutting | Segurança, logs, métricas, headers, correlation ID e performance | `Middlewares/`, `Services/Observability/`, `Services/Security/` |
| Operação | Smoke, backups, scanners, readiness, FinOps e DR | `tools/`, `docs/*runbook*.md` |

## Fluxos críticos

| Fluxo | Entrada | Orquestração | Persistência/saída |
| --- | --- | --- | --- |
| Login local | `AuthController.LocalLogin` | Auth local, cookie, rate limit | Cookie auth e métricas de auth |
| Login Control iD | `AuthController.Login` | `OfficialControlIdApiService`/invoker | Sessão ASP.NET e logs seguros |
| Catálogo oficial | `OfficialApiController` | `OfficialApiCatalogService` e docs de contrato | ViewModels e resposta visual |
| Invocação oficial | `OfficialApiController.Invoke` | `OfficialApiInvokerService`, timeout, circuit breaker | Resultado sanitizado, logs e métricas |
| Callback/monitor | callbacks `.fcgi` e `/api/notifications/*` | `CallbackIngressService`, body reader e security evaluator | `MonitorEvents`, logs e métricas |
| Push | `GET /push`, `POST /result`, `PushCenter` | `PushCommandWorkflowService` | `PushCommands`, estados e métricas |
| Banco/backup | startup, repositórios e scripts | EF Core migrations, backup/restore smoke | SQLite local e artefatos fora do Git |
| Observabilidade | middlewares e `/metrics` | `OperationalMetrics`, `PrometheusMetricsWriter` | Prometheus text, dashboards e alertas |

## Dependências externas

| Dependência | Tipo | Controle |
| --- | --- | --- |
| Equipamento Control iD | API HTTP local/externa | Timeout, allowlist, HMAC em callbacks, contrato stub e contrato físico opt-in |
| SQLite | Arquivo local/volume | Health check, migrations, índices, backup e restore-smoke |
| GitHub Actions/NuGet | CI/dependências | Lockfiles, audit e supply-chain review |
| Scanners externos | Ferramentas opcionais | `tools/external-security-scans.ps1` e release gate |

Não há provedor cloud, storage externo, cache distribuído, broker, e-mail/SMS ou
analytics externo versionado.

## Fronteiras de confiança

- Browser do operador -> aplicação MVC.
- Aplicação -> equipamento Control iD.
- Equipamento -> callbacks/Push expostos pela PoC.
- Aplicação -> SQLite local.
- Operador/CI -> scripts PowerShell e artefatos locais.
- Host/provedor futuro -> logs, volumes, métricas e backups.

Fora de `Development`, a aplicação deve falhar no startup se configurações
críticas estiverem inseguras: `AllowedHosts=*`, métricas anônimas, chave compartilhada
ausente, assinatura HMAC ausente, OpenAPI habilitado sem decisão e forwarded
headers sem proxy conhecido.

## Contratos a preservar

- Rotas MVC usadas pela UI.
- Endpoints oficiais auxiliares, callbacks `.fcgi`, `/push`, `/result` e
  `Push/Receive`.
- Payloads e nomes de campos esperados pela Access API Control iD.
- ViewModels publicamente consumidos pelas views.
- Métricas e labels já documentadas em `docs/observability-runbook.md`.
- Scripts oficiais citados em README, AGENTS e CI.

## Decisões arquiteturais

ADRs atuais:

- `docs/adrs/0001-local-sqlite-runtime-state.md`
- `docs/adrs/0002-secure-controlid-ingress-and-egress.md`
- `docs/adrs/0003-in-process-observability-and-readiness-gates.md`
- `docs/adrs/0004-release-governance-with-local-scripts.md`

Crie novo ADR quando uma decisão alterar padrão de arquitetura, provedor,
persistência, segurança, observabilidade, release ou contrato público.

## Limites conhecidos

- SQLite simplifica a PoC, mas não substitui um desenho de banco multi-instancia.
- Contrato com equipamento real depende de hardware, rede, firmware e credenciais
  fora do Git; release estrito bloqueia sem contrato físico validado.
- Billing, DNS, TLS real e sizing de produção dependem de provedor escolhido;
  `ops.example.json` e `operational-readiness-check.ps1 -RequireConfig` exigem
  donos, status e evidências antes de release.
- Bases legais, DPA, RIPD e titulares reais dependem de DPO/jurídico; os campos
  `privacy.*` agora são obrigatórios em configuração operacional real.
- Scanners externos dependem de instalação local ou ambiente CI preparado; o gate
  `-ReleaseGate` exige as ferramentas e os relatórios.
- O fechamento detalhado desses riscos externos fica em
  `docs/residual-risk-closure.md`.
