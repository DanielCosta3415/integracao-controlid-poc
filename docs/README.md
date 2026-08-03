# Índice da documentação técnica

Este índice orienta desenvolvedores, mantenedores, SREs, QA, DPO/privacidade e
agentes de código no uso seguro da PoC `Integracao.ControlID.PoC`.

## Leitura por papel

| Papel | Comece por |
| --- | --- |
| Novo desenvolvedor | `docs/developer-onboarding.md`, `docs/architecture-overview.md`, `docs/project-file-responsibilities.md` |
| Maintainer | `AGENTS.md`, `docs/adrs/`, `docs/testing-strategy.md`, `docs/changelog-2026-05-01.md` |
| QA/SDET | `docs/product-acceptance-criteria.md`, `docs/testing-strategy.md`, `docs/external-validation-runbook.md` |
| SRE/Operação | `docs/observability-runbook.md`, `docs/deployment-runbook.md`, `docs/incident-response-and-dr.md` |
| DevOps/Platform | `docs/ci-cd-quality-gates.md`, `.github/workflows/ci.yml`, `docs/deployment-runbook.md` |
| Security/AppSec | `docs/security-hardening.md`, `docs/integration-contracts.md`, `docs/external-validation-runbook.md` |
| DPO/Privacidade | `docs/privacy-and-data-retention.md`, `docs/privacy-governance-runbook.md` |
| Data/DB | `docs/data-model-and-recovery.md`, `docs/database-and-runtime-state.md` |
| Produto/Analytics | `docs/product-acceptance-criteria.md`, `docs/product-analytics.md` |
| FinOps/Capacidade | `docs/finops-capacity.md`, `docs/observability-runbook.md` |
| Release/Owner | `docs/residual-risk-closure.md`, `docs/deployment-runbook.md`, `ops.example.json` |

## Desenvolvimento e arquitetura

- `docs/developer-onboarding.md`: setup reproduzível, execução, comandos,
  diagnóstico e entrega.
- `docs/architecture-overview.md`: camadas, fluxos críticos, dependências e
  limites.
- `docs/project-file-responsibilities.md`: mapa detalhado de arquivos e pastas.
- `docs/adrs/`: decisões arquiteturais e suas consequências.

## Produto e requisitos

- `docs/product-acceptance-criteria.md`: requisitos, fluxos, critérios de aceite,
  rastreabilidade, DoR e DoD.
- `docs/product-analytics.md`: KPIs, eventos agregados, dashboards e restrições de
  privacidade.
- `docs/brand.md`: identidade visual, tokens e regras de acessibilidade visual.

## Integrações e dados

- `docs/integration-contracts.md`: inventário de integrações, contratos,
  payloads e riscos.
- `docs/monitor-implementation.md`: callbacks, monitoramento e persistência de
  eventos.
- `docs/push-implementation.md`: fila Push, polling, resultados e estados.
- `docs/operation-modes-implementation.md`: Standalone, Pro, Enterprise e
  transições.
- `docs/data-model-and-recovery.md`: modelo local, índices, migrations, backup e
  restore.
- `docs/database-and-runtime-state.md`: estado de runtime e comandos seguros.

## Segurança, privacidade e cadeia de suprimentos

- `docs/security-hardening.md`: controles de auth, RBAC, HMAC, headers,
  allowlist e estado local.
- `docs/privacy-and-data-retention.md`: inventário de dados pessoais, tratamento,
  retenção, descarte e lacunas LGPD.
- `docs/privacy-governance-runbook.md`: RACI, DSAR, RIPD, DPA e incidente de
  dados.
- `docs/supply-chain-review.md`: NuGet, lockfiles, SBOM, vendor dependencies e
  auditoria.
- `docs/external-validation-runbook.md`: Semgrep, OSV, ZAP, axe e contrato com
  stub/equipamento.

## Operação, release e continuidade

- `docs/testing-strategy.md`: testes, coverage, gates e validação externa.
- `docs/ci-cd-quality-gates.md`: GitHub Actions, gates obrigatórios,
  artefatos, branch protection recomendada e reprodução local.
- `docs/observability-runbook.md`: logs, métricas, tracing, health checks,
  alertas e dashboards.
- `docs/deployment-runbook.md`: ambientes, Docker/Compose, deploy e rollback.
- `docs/incident-response-and-dr.md`: matriz SEV, runbooks, DR e postmortem.
- `docs/equipment-contingency-runbook.md`: contingência física e fallback manual.
- `docs/finops-capacity.md`: custos, limites, capacidade e governança FinOps.
- `docs/residual-risk-closure.md`: fechamento verificável de lacunas externas,
  gates e evidências exigidas para release.

## Changelog e relatórios

- `docs/changelog-2026-04-14.md`: rodada inicial de evolução técnica.
- `docs/changelog-2026-04-15.md`: comentários e observabilidade.
- `docs/changelog-2026-05-01.md`: documentação, governança e readiness.
- `docs/changelog-2026-08-03.md`: fechamento dos 14 riscos full-stack.
- `docs/pr-summary-2026-05-01.md`: resumo de PR/release notes da rodada.
- `docs/documentation-audit-2026-05-01.md`: auditoria documental e lacunas.
- `docs/reports/`: relatórios históricos de smoke, UX, design e auditorias. Use
  somente dados fictícios ou sanitizados.

## Regras de manutenção

- Atualize o índice quando criar, remover ou renomear documentos.
- Não inclua secrets, payloads reais, bancos, logs locais ou dados pessoais.
- Não documente comandos que não existem no repositório.
- Marque dependências de decisão humana, DPO, jurídico, provedor ou equipamento
  físico em `docs/residual-risk-closure.md` e em gates verificáveis.
- Registre decisões estruturais em ADR antes de transformar exceção em padrão.
