# Indice da documentacao tecnica

Este indice orienta desenvolvedores, mantenedores, SREs, QA, DPO/privacidade e
agentes de codigo no uso seguro da PoC `Integracao.ControlID.PoC`.

## Leitura por papel

| Papel | Comece por |
| --- | --- |
| Novo desenvolvedor | `docs/developer-onboarding.md`, `docs/architecture-overview.md`, `docs/project-file-responsibilities.md` |
| Maintainer | `AGENTS.md`, `docs/adrs/`, `docs/testing-strategy.md`, `docs/changelog-2026-05-01.md` |
| QA/SDET | `docs/product-acceptance-criteria.md`, `docs/testing-strategy.md`, `docs/external-validation-runbook.md` |
| SRE/Operacao | `docs/observability-runbook.md`, `docs/deployment-runbook.md`, `docs/incident-response-and-dr.md` |
| DevOps/Platform | `docs/ci-cd-quality-gates.md`, `.github/workflows/ci.yml`, `docs/deployment-runbook.md` |
| Security/AppSec | `docs/security-hardening.md`, `docs/integration-contracts.md`, `docs/external-validation-runbook.md` |
| DPO/Privacidade | `docs/privacy-and-data-retention.md`, `docs/privacy-governance-runbook.md` |
| Data/DB | `docs/data-model-and-recovery.md`, `docs/database-and-runtime-state.md` |
| Produto/Analytics | `docs/product-acceptance-criteria.md`, `docs/product-analytics.md` |
| FinOps/Capacidade | `docs/finops-capacity.md`, `docs/observability-runbook.md` |
| Release/Owner | `docs/residual-risk-closure.md`, `docs/deployment-runbook.md`, `ops.example.json` |

## Desenvolvimento e arquitetura

- `docs/developer-onboarding.md`: setup reproduzivel, execucao, comandos,
  diagnostico e entrega.
- `docs/architecture-overview.md`: camadas, fluxos criticos, dependencias e
  limites.
- `docs/project-file-responsibilities.md`: mapa detalhado de arquivos e pastas.
- `docs/adrs/`: decisoes arquiteturais e suas consequencias.

## Produto e requisitos

- `docs/product-acceptance-criteria.md`: requisitos, fluxos, criterios de aceite,
  rastreabilidade, DoR e DoD.
- `docs/product-analytics.md`: KPIs, eventos agregados, dashboards e restricoes de
  privacidade.
- `docs/brand.md`: identidade visual, tokens e regras de acessibilidade visual.

## Integracoes e dados

- `docs/integration-contracts.md`: inventario de integracoes, contratos,
  payloads e riscos.
- `docs/monitor-implementation.md`: callbacks, monitoramento e persistencia de
  eventos.
- `docs/push-implementation.md`: fila Push, polling, resultados e estados.
- `docs/operation-modes-implementation.md`: Standalone, Pro, Enterprise e
  transicoes.
- `docs/data-model-and-recovery.md`: modelo local, indices, migrations, backup e
  restore.
- `docs/database-and-runtime-state.md`: estado de runtime e comandos seguros.

## Seguranca, privacidade e supply chain

- `docs/security-hardening.md`: controles de auth, RBAC, HMAC, headers,
  allowlist e estado local.
- `docs/privacy-and-data-retention.md`: inventario de dados pessoais, tratamento,
  retencao, descarte e lacunas LGPD.
- `docs/privacy-governance-runbook.md`: RACI, DSAR, RIPD, DPA e incidente de
  dados.
- `docs/supply-chain-review.md`: NuGet, lockfiles, SBOM, vendor dependencies e
  auditoria.
- `docs/external-validation-runbook.md`: Semgrep, OSV, ZAP, axe e contrato com
  stub/equipamento.

## Operacao, release e continuidade

- `docs/testing-strategy.md`: testes, coverage, gates e validacao externa.
- `docs/ci-cd-quality-gates.md`: GitHub Actions, gates obrigatorios,
  artefatos, branch protection recomendada e reproducao local.
- `docs/observability-runbook.md`: logs, metricas, tracing, health checks,
  alertas e dashboards.
- `docs/deployment-runbook.md`: ambientes, Docker/Compose, deploy e rollback.
- `docs/incident-response-and-dr.md`: matriz SEV, runbooks, DR e postmortem.
- `docs/equipment-contingency-runbook.md`: contingencia fisica e fallback manual.
- `docs/finops-capacity.md`: custos, limites, capacidade e governanca FinOps.
- `docs/residual-risk-closure.md`: fechamento verificavel de lacunas externas,
  gates e evidencias exigidas para release.

## Changelog e relatorios

- `docs/changelog-2026-04-14.md`: rodada inicial de evolucao tecnica.
- `docs/changelog-2026-04-15.md`: comentarios e observabilidade.
- `docs/changelog-2026-05-01.md`: documentacao, governanca e readiness.
- `docs/pr-summary-2026-05-01.md`: resumo de PR/release notes da rodada.
- `docs/documentation-audit-2026-05-01.md`: auditoria documental e lacunas.
- `docs/reports/`: relatorios historicos de smoke, UX, design e auditorias. Use
  somente dados ficticios ou sanitizados.

## Regras de manutencao

- Atualize o indice quando criar, remover ou renomear documentos.
- Nao inclua secrets, payloads reais, bancos, logs locais ou dados pessoais.
- Nao documente comandos que nao existem no repositorio.
- Marque dependencias de decisao humana, DPO, juridico, provedor ou equipamento
  fisico em `docs/residual-risk-closure.md` e em gates verificaveis.
- Registre decisoes estruturais em ADR antes de transformar excecao em padrao.
