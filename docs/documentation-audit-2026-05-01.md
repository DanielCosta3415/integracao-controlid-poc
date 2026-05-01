# Auditoria documental - 2026-05-01

## Escopo auditado

- `README.md`
- `AGENTS.md`
- `docs/`
- `.env.example`
- `ops.example.json`
- `.github/workflows/ci.yml`
- scripts em `tools/`
- documentos de arquitetura, seguranca, privacidade, observabilidade, release,
  FinOps, testes e dados

## Achados

| Area | Estado encontrado | Acao aplicada |
| --- | --- | --- |
| Entrada para novo dev | README era completo, mas misturava setup, operacao e links sem trilha clara | README reestruturado e `docs/developer-onboarding.md` criado |
| Indice de conhecimento | Nao havia indice central em `docs/` | `docs/README.md` criado |
| Arquitetura | Estrutura estava distribuida entre README, AGENTS e mapa de arquivos | `docs/architecture-overview.md` criado |
| ADRs | Nao havia ADRs versionados | `docs/adrs/` criado com quatro decisoes |
| Changelog/PR summary | Havia changelogs antigos, sem resumo da rodada atual | `docs/changelog-2026-05-01.md` e `docs/pr-summary-2026-05-01.md` criados |
| Comandos reais | Comandos estavam no README/AGENTS, mas sem trilha de onboarding | Guia novo referencia comandos existentes sem inventar scripts |
| Operacao/DR/FinOps | Runbooks existiam, mas precisavam estar indexados | Indice e README conectam runbooks |
| Limitacoes | Lacunas estavam espalhadas | Lacunas consolidadas e transformadas em controles de release em `docs/residual-risk-closure.md` |

## Consistencia verificada

- Comandos documentados existem no repositorio.
- Secrets reais nao foram adicionados.
- Exemplos usam placeholders.
- Limites de provedor, billing, juridico/DPO e equipamento fisico permanecem
  dependencias externas, agora com campos obrigatorios em `ops.example.json` e
  bloqueio por gate estrito.
- `AGENTS.md` continua sendo a regra de governanca para agentes.

## Lacunas restantes controladas

| Lacuna externa | Controle aplicado | Dono sugerido |
| --- | --- | --- |
| Provedor cloud real | Campos `deployment.*` em `ops.example.json` e `operational-readiness-check.ps1 -RequireConfig` | Maintainer/SRE |
| Billing real e budget aprovado | Campos `finops.*`, `finops-capacity-check.ps1 -FailOnWarnings` e release gate | FinOps/Owner |
| RTO/RPO validado em ambiente alvo | Campos `rtoRpo.*`, backup operacional e restore smoke | SRE/DR |
| Bases legais, DPA e RIPD | Campos `privacy.*` e aprovacao bloqueante em readiness | DPO/Juridico |
| Contrato fisico Control iD | Campos `hardwareContract.*` e `test-readiness-gates.ps1 -ReleaseGate` | Integracao/Operacao |
| TLS/DNS produtivo | Campos `deployment.dnsTlsValidationStatus`, donos de DNS/TLS e aprovacao de producao | SRE/Platform |
| Scanners externos | Campos `externalValidation.*`, `external-security-scans.ps1` e release gate | Security/AppSec |

Detalhes em `docs/residual-risk-closure.md`.

## Regras para proximas atualizacoes

- Atualizar `docs/README.md` quando adicionar/remover documento.
- Criar ADR para decisao estrutural.
- Atualizar changelog e PR summary por rodada relevante.
- Evitar duplicar payload sensivel; usar exemplos minimizados.
- Registrar checks executados no resumo final e no PR.
