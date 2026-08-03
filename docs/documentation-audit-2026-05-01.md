# Auditoria documental - 2026-05-01

## Escopo auditado

- `README.md`
- `AGENTS.md`
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
| Entrada para novo dev | README era completo, mas misturava setup, operação e links sem trilha clara | README reestruturado e `docs/developer-onboarding.md` criado |
| Índice de conhecimento | Não havia índice central em `docs/` | `docs/README.md` criado |
| Arquitetura | Estrutura estava distribuída entre README, AGENTS e mapa de arquivos | `docs/architecture-overview.md` criado |
| ADRs | Não havia ADRs versionados | `docs/adrs/` criado com quatro decisões |
| Changelog/PR summary | Havia changelogs antigos, sem resumo da rodada atual | `docs/changelog-2026-05-01.md` e `docs/pr-summary-2026-05-01.md` criados |
| Comandos reais | Comandos estavam no README/AGENTS, mas sem trilha de onboarding | Guia novo referência comandos existentes sem inventar scripts |
| Operação/DR/FinOps | Runbooks existiam, mas precisavam estar indexados | Índice e README conectam runbooks |
| Limitações | Lacunas estavam espalhadas | Lacunas consolidadas e transformadas em controles de release em `docs/residual-risk-closure.md` |

## Consistência verificada

- Comandos documentados existem no repositório.
- Secrets reais não foram adicionados.
- Exemplos usam placeholders.
- Limites de provedor, billing, jurídico/DPO e equipamento físico permanecem
  dependências externas, agora com campos obrigatórios em `ops.example.json` e
  bloqueio por gate estrito.
- `AGENTS.md` continua sendo a regra de governança para agentes.

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

Detalhes em `docs/residual-risk-closure.md`.

## Regras para próximas atualizações

- Atualizar `docs/README.md` quando adicionar/remover documento.
- Criar ADR para decisão estrutural.
- Atualizar changelog e PR summary por rodada relevante.
- Evitar duplicar payload sensível; usar exemplos minimizados.
- Registrar checks executados no resumo final e no PR.
