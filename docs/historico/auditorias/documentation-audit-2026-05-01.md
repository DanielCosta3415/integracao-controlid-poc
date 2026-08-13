# Auditoria documental - 2026-05-01

> **Registro histórico** · Público: manutenção e auditoria · Responsável: Engenharia · Referência temporal: 2026-05-01.

## Escopo auditado

- [README.md](../../../README.md)
- [AGENTS.md](../../../AGENTS.md)
- `docs/`
- `.env.example`
- `ops.example.json`
- `.github/workflows/ci.yml`
- scripts em `tools/`
- documentos de arquitetura, segurança, privacidade, observabilidade, release,
  FinOps, testes e dados

## Achados

| Área | Estado encontrado | Ação aplicada |
| --- | --- | --- |
| Entrada para novo dev | README era completo, mas misturava setup, operação e links sem trilha clara | README reestruturado e [docs/primeiros-passos/developer-onboarding.md](../../primeiros-passos/developer-onboarding.md) criado |
| Índice de conhecimento | Não havia índice central em `docs/` | [docs/README.md](../../README.md) criado |
| Arquitetura | Estrutura estava distribuída entre README, AGENTS e mapa de arquivos | [docs/arquitetura/architecture-overview.md](../../arquitetura/architecture-overview.md) criado |
| ADRs | Não havia ADRs versionados | `docs/adrs/` criado com quatro decisões |
| Changelog/PR summary | Havia changelogs antigos, sem resumo da rodada atual | [docs/historico/changelogs/changelog-2026-05-01.md](../changelogs/changelog-2026-05-01.md) e [docs/historico/auditorias/pr-summary-2026-05-01.md](pr-summary-2026-05-01.md) criados |
| Comandos reais | Comandos estavam no README/AGENTS, mas sem trilha de onboarding | Guia novo referência comandos existentes sem inventar scripts |
| Operação/recuperação de desastres/FinOps | Guias operacionais existiam, mas precisavam estar indexados | Índice e README conectam os guias operacionais |
| Limitações | Lacunas estavam espalhadas | Lacunas consolidadas e transformadas em controles de release em [docs/operacao/residual-risk-closure.md](../../operacao/residual-risk-closure.md) |

## Consistência verificada

- Comandos documentados existem no repositório.
- Secrets reais não foram adicionados.
- Exemplos usam placeholders.
- Limites de provedor, billing, jurídico/DPO e equipamento físico permanecem
  dependências externas, agora com campos obrigatórios em `ops.example.json` e
  bloqueio por gate estrito.
- [AGENTS.md](../../../AGENTS.md) continua sendo a regra de governança para agentes.

## Lacunas restantes controladas

| Lacuna externa | Controle aplicado | Dono sugerido |
| --- | --- | --- |
| Provedor cloud real | Campos `deployment.*` em `ops.example.json` e `operational-readiness-check.ps1 -RequireConfig` | Maintainer/SRE |
| Billing real e budget aprovado | Campos `finops.*`, `finops-capacity-check.ps1 -FailOnWarnings` e release gate | FinOps/Owner |
| RTO/RPO validado em ambiente alvo | Campos `rtoRpo.*`, backup operacional e restore smoke | SRE/DR |
| Bases legais, DPA e RIPD | Campos `privacy.*` e aprovação bloqueante em readiness | DPO/Jurídico |
| Contrato físico Control iD | Campos `hardwareContract.*` e `test-readiness-gates.ps1 -ReleaseGate` | Integração/Operação |
| TLS/DNS produtivo | Campos `deployment.dnsTlsValidationStatus`, donos de DNS/TLS e aprovação de produção | SRE/Platform |
| Scanners externos | Campos `externalValidation.*`, `external-security-scans.ps1` e release gate | Security/AppSec |

Detalhes em [docs/operacao/residual-risk-closure.md](../../operacao/residual-risk-closure.md).

## Regras para próximas atualizações

- Atualizar [docs/README.md](../../README.md) quando adicionar/remover documento.
- Criar ADR para decisão estrutural.
- Atualizar changelog e PR summary por rodada relevante.
- Evitar duplicar payload sensível; usar exemplos minimizados.
- Registrar checks executados no resumo final e no PR.

## Validade desta auditoria

Esta evidência foi superada em 2026-08-03 pela revisão individual dos 49 arquivos
Markdown e pela introdução de metadados e validação automática. Preserve-a para
histórico, mas use [docs/README.md](../../README.md), [README.md](../../../README.md) e
`tools/validate-documentation.ps1` como referências atuais.

Limitações do registro original: não contém hash do commit auditado, inventário
individual de relatórios nem validação de caminhos em crases.

## Critério para nova auditoria

Uma auditoria sucessora deve registrar inventário exato, classificação, público,
responsável, data, UTF-8, links, âncoras, caminhos, comandos, rastreabilidade,
licenças vendorizadas e divergências semânticas encontradas. O validador fornece
evidência estrutural; ortografia e coerência de comportamento ainda exigem revisão.

## Navegação documental

- [Voltar ao índice deste domínio](README.md).
- [Abrir a central de documentação](../../README.md).
