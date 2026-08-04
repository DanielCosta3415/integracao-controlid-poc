# Índice da documentação técnica

> **Índice vivo** · Público: todos os papéis · Responsável: mantenedores · Última validação: 2026-08-04.

Este índice orienta desenvolvedores, mantenedores, SREs, QA, DPO/privacidade e
agentes de código no uso seguro da PoC `Integracao.ControlID.PoC`.

Para escolher um percurso objetivo, consulte [Rotas de leitura e cadência](#rotas-de-leitura-e-cadência).

## Primeiro contato

- `docs/faq.md`: 96 respostas diretas sobre produto, contas, rede, API,
  equipamentos, dados, segurança e operação.
- `docs/persona-guides.md`: percursos de leitura e execução por perfil.
- `docs/local-account-administration.md`: diferença entre conta local e sessão
  Control iD, papéis e recuperação.
- `docs/device-compatibility-matrix.md`: o que está implementado, validado com
  stub ou pendente de equipamento físico.
- `docs/network-topologies.md`: quem inicia cada conexão e quando a PoC precisa
  ser alcançável pelo equipamento.
- `docs/validation-without-device.md`: o que pode ser concluído sem aparelho e
  onde começa a homologação física.
- `docs/stub-scenarios.md`: cenários, perfis, massas e administração do
  simulador determinístico.

## Como a documentação está organizada

1. `README.md` e `docs/developer-onboarding.md` formam a entrada para o projeto.
2. Documentos vivos descrevem o comportamento que deve coincidir com o código.
3. ADRs registram decisões aceitas e não devem ser reescritos para apagar o
   contexto histórico; uma mudança cria nova decisão ou marca substituição.
4. Changelogs, resumos de PR e `docs/reports/` são evidências históricas. Eles
   não substituem os documentos vivos nem garantem o estado atual.
5. `wwwroot/lib/jquery-validation/LICENSE.md` é texto legal vendorizado e deve
   permanecer literal.

Quando houver conflito, prevalecem nesta ordem: contrato executável e testes,
código atual, ADR aceita, documento vivo e evidência histórica. Inconsistências
devem ser corrigidas, não apenas explicadas.

## Leitura por papel

| Papel | Comece por |
| --- | --- |
| Primeiro contato/avaliação | `docs/faq.md`, `docs/persona-guides.md`, `README.md` |
| Novo desenvolvedor | `docs/developer-onboarding.md`, `docs/architecture-overview.md`, `docs/project-file-responsibilities.md` |
| Maintainer | `AGENTS.md`, `docs/adrs/`, `docs/testing-strategy.md`, `docs/changelog-2026-05-01.md` |
| QA/SDET | `docs/product-acceptance-criteria.md`, `docs/testing-strategy.md`, `docs/external-validation-runbook.md` |
| SRE/Operação | `docs/troubleshooting-controlid.md`, `docs/observability-runbook.md`, `docs/incident-response-and-dr.md` |
| DevOps/Plataforma | `docs/network-topologies.md`, `docs/ci-cd-quality-gates.md`, `docs/deployment-runbook.md` |
| Segurança/AppSec | `docs/security-hardening.md`, `docs/integration-contracts.md`, `docs/external-validation-runbook.md` |
| DPO/Privacidade | `docs/privacy-and-data-retention.md`, `docs/privacy-governance-runbook.md` |
| Data/DB | `docs/data-synchronization-ownership.md`, `docs/data-model-and-recovery.md`, `docs/database-and-runtime-state.md` |
| Produto/Analytics | `docs/product-acceptance-criteria.md`, `docs/product-analytics.md` |
| FinOps/Capacidade | `docs/finops-capacity.md`, `docs/observability-runbook.md` |
| Liberação/Responsável | `docs/residual-risk-closure.md`, `docs/deployment-runbook.md`, `ops.example.json` |

## Desenvolvimento e arquitetura

- `docs/developer-onboarding.md`: setup reproduzível, execução, comandos,
  diagnóstico e entrega.
- `docs/architecture-overview.md`: camadas, fluxos críticos, dependências e
  limites.
- `docs/project-file-responsibilities.md`: mapa detalhado de arquivos e pastas.
- `docs/adrs/`: decisões arquiteturais e suas consequências.

ADRs atuais:

- `docs/adrs/0001-local-sqlite-runtime-state.md`: SQLite como estado local.
- `docs/adrs/0002-secure-controlid-ingress-and-egress.md`: limites de entrada e saída.
- `docs/adrs/0003-in-process-observability-and-readiness-gates.md`: sinais operacionais.
- `docs/adrs/0004-release-governance-with-local-scripts.md`: gates de release.
- `docs/adrs/0005-dotnet-10-lts-runtime.md`: runtime .NET 10 LTS coordenado.
- `docs/adrs/0006-deterministic-simulator-and-browser-validation.md`: simulador,
  Playwright, axe e separação da evidência física.

## Produto e requisitos

- `docs/faq.md`: perguntas frequentes de primeiro contato e integração.
- `docs/persona-guides.md`: percursos guiados por perfil e objetivo.
- `docs/product-acceptance-criteria.md`: requisitos, fluxos, critérios de aceite,
  rastreabilidade, DoR e DoD.
- `docs/product-analytics.md`: KPIs, eventos agregados, painéis e restrições de
  privacidade.
- `docs/brand.md`: identidade visual, tokens e regras de acessibilidade visual.

## Integrações e dados

- `docs/device-compatibility-matrix.md`: compatibilidade por linha, firmware,
  licença e nível de evidência.
- `docs/network-topologies.md`: topologias, fluxos, portas e controles de rede.
- `docs/integration-contracts.md`: inventário de integrações, contratos,
  payloads e riscos.
- `docs/api-error-catalog.md`: catálogo de erros por camada e conduta segura.
- `docs/data-synchronization-ownership.md`: fontes de verdade, sincronização,
  conflitos e reconciliação.
- `docs/official-api-version-governance.md`: fontes oficiais, cadência e
  revalidação de firmware/contratos.
- `docs/endpoint-validation-matrix.md`: nível de evidência por família de
  endpoint.
- `docs/monitor-implementation.md`: callbacks, monitoramento e persistência de
  eventos.
- `docs/push-implementation.md`: fila Push, polling, resultados e estados.
- `docs/operation-modes-implementation.md`: Standalone, Pro, Enterprise e
  transições.
- `docs/data-model-and-recovery.md`: modelo local, índices, migrations, backup e
  restore.
- `docs/database-and-runtime-state.md`: estado de runtime e comandos seguros.

## Segurança, privacidade e cadeia de suprimentos

- `docs/local-account-administration.md`: contas locais, papéis, sessões e
  limitações de recuperação.
- `docs/security-hardening.md`: controles de autenticação, RBAC, HMAC, cabeçalhos,
  allowlist e estado local.
- `docs/privacy-and-data-retention.md`: inventário de dados pessoais, tratamento,
  retenção, descarte e lacunas LGPD.
- `docs/privacy-governance-runbook.md`: RACI, DSAR, RIPD, DPA e incidente de
  dados.
- `docs/supply-chain-review.md`: NuGet, lockfiles, SBOM, vendor dependencies e
  auditoria.
- `docs/external-validation-runbook.md`: Semgrep, OSV, ZAP, axe e contrato com
  stub/equipamento.

## Operação, liberação e continuidade

- `docs/troubleshooting-controlid.md`: diagnóstico por sintoma e evidência segura.
- `docs/testing-strategy.md`: testes, coverage, gates e validação externa.
- `docs/ci-cd-quality-gates.md`: GitHub Actions, gates obrigatórios,
  artefatos, proteção recomendada da ramificação e reprodução local.
- `docs/observability-runbook.md`: registros, métricas, rastreamento, verificações
  de saúde, alertas e painéis.
- `docs/deployment-runbook.md`: ambientes, Docker/Compose, implantação e reversão.
- `docs/incident-response-and-dr.md`: matriz SEV, guias operacionais, recuperação de desastres e análise pós-incidente.
- `docs/equipment-contingency-runbook.md`: contingência física e alternativa manual.
- `docs/finops-capacity.md`: custos, limites, capacidade e governança FinOps.
- `docs/performance-baseline.md`: método, complexidade, orçamento e resultados
  locais de desempenho.
- `docs/residual-risk-closure.md`: fechamento verificável de lacunas externas,
  gates e evidências exigidas para release.

## Registros de alterações e relatórios

- `docs/changelog-2026-04-14.md`: rodada inicial de evolução técnica.
- `docs/changelog-2026-04-15.md`: comentários e observabilidade.
- `docs/changelog-2026-05-01.md`: documentação, governança e readiness.
- `docs/changelog-2026-08-03.md`: fechamento dos 14 riscos da solução completa.
- `docs/changelog-2026-08-04.md`: otimizações dos 11 gargalos e validação funcional/visual.
- `docs/pr-summary-2026-05-01.md`: resumo de PR/release notes da rodada.
- `docs/documentation-audit-2026-05-01.md`: auditoria documental e lacunas.
- `docs/reports/`: relatórios históricos de smoke, UX, design e auditorias. Use
  somente dados fictícios ou sanitizados.

Catálogo de evidências históricas:

- `docs/reports/controlid-api-audit-2026-04-13.md`
- `docs/reports/design-system-accessibility-audit-2026-04-14.md`
- `docs/reports/heuristic-ui-audit-2026-04-14.md`
- `docs/reports/localhost-smoke-test-2026-04-13.md`
- `docs/reports/localhost-smoke-test-2026-04-14.md`
- `docs/reports/operation-modes-e2e-runbook-2026-04-14.md`
- `docs/reports/operation-modes-homologation-matrix-2026-04-14.md`
- `docs/reports/visual-inventory-2026-04-14.md`

## Regras de manutenção

- Atualize o índice quando criar, remover ou renomear documentos.
- Não inclua secrets, payloads reais, bancos, logs locais ou dados pessoais.
- Não documente comandos que não existem no repositório.
- Marque dependências de decisão humana, DPO, jurídico, provedor ou equipamento
  físico em `docs/residual-risk-closure.md` e em gates verificáveis.
- Registre decisões estruturais em ADR antes de transformar exceção em padrão.
- Execute `tools/validate-documentation.ps1` após criar, remover, renomear ou
  editar documentação.
- Use `tools/validate-documentation.ps1 -CheckExternalUrls` em auditorias
  conectadas para verificar também a disponibilidade das referências externas;
  a CI mantém o modo off-line para evitar falhas causadas por terceiros.
- Todo documento autoral deve informar classificação, público, responsável e
  data de validação logo após o título principal.
- O inventário atual possui 65 arquivos Markdown: 64 documentos autorais e uma
  licença vendorizada que deve permanecer literal.

## Rotas de leitura e cadência

| Objetivo | Tempo de leitura aproximado | Evidência de conclusão |
| --- | ---: | --- |
| Entender a PoC | 15 a 25 minutos | FAQ e percurso da persona concluídos |
| Primeiro uso local | 20 a 30 minutos | Stub consultado e gate local aprovado |
| Mudança funcional | 30 a 45 minutos | Requisito, fluxo, código e teste identificados |
| Revisão de liberação | 45 a 90 minutos | Riscos externos, implantação, reversão e verificações registrados |
| Incidente | Conforme a severidade | Guia do cenário aberto, responsável definido e correlação preservada |

Documentos vivos devem ser revisados quando o código relacionado mudar. ADRs e
evidências históricas preservam o contexto original; correções factuais devem
ser adendos identificados, nunca reescrita silenciosa do resultado observado.
