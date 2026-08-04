# Resumo da PR - documentação técnica e governança

> **Evidência histórica** · Público: manutenção e auditoria · Referência temporal: 2026-05-01 · Responsável: mantenedores · Não substitui o estado atual nem um novo resumo de PR.

## Resumo

Esta rodada reorganiza a documentação técnica para onboarding, manutenção e
operação segura da PoC Control iD. Não altera regra de negócio nem contrato
público de API.

## Mudanças principais

- README reestruturado com stack, setup, comandos oficiais, operação, variáveis,
  banco, observabilidade, container, fluxos e troubleshooting.
- Novo índice em `docs/README.md`.
- Novo guia de onboarding em `docs/developer-onboarding.md`.
- Nova visão de arquitetura em `docs/architecture-overview.md`.
- ADRs criados em `docs/adrs/`.
- Changelog e auditoria documental de 2026-05-01 adicionados.
- Fechamento de riscos residuais externos em `docs/residual-risk-closure.md`.
- `ops.example.json` e readiness operacional agora exigem aprovações e
  evidências de deployment, DPO/jurídico, scanners externos e contrato físico.

## Como validar

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1
```

Checks complementares recomendados para review:

```powershell
git diff --check
powershell -ExecutionPolicy Bypass -File .\tools\scan-secrets.ps1
```

## Riscos

- Nenhum comportamento de runtime foi alterado.
- Risco principal e documental: manter README, `docs/README.md` e ADRs
  sincronizados em rodadas futuras.

## Pendências conhecidas

- Provedor cloud, DNS/TLS real e billing real continuam fora do repositório, mas
  são bloqueados por `ops.local.json` e readiness estrito até aprovação.
- Bases legais, DPA, RIPD e comunicações externas exigem DPO/jurídico, com
  status obrigatório em `privacy.*`.
- Contrato com equipamento físico exige bancada real e credenciais fora do Git,
  com bloqueio por `test-readiness-gates.ps1 -ReleaseGate`.
- Scanners externos exigem ferramentas instaladas/aprovadas e URL controlada; o
  release gate falha se estiverem ausentes.

## Validade e rastreabilidade

- PR e commit originais: não registrados neste resumo histórico.
- Escopo temporal: estado conhecido em 2026-05-01.
- Mudanças posteriores: consulte `docs/changelog-2026-08-03.md`.
- Validação atual: execute os comandos do `README.md` e não reutilize resultados
  antigos como aprovação de release.

Um novo PR deve gerar seu próprio resumo com título, motivação, impacto, checks,
riscos e reversão; este arquivo não é um modelo vivo.

## Campos mínimos do próximo resumo

- commit-base e commit final;
- requisito, fluxo e risco afetados;
- arquivos por categoria e contratos preservados;
- migração, configuração e impacto operacional;
- comandos executados, resultado e artefato;
- risco residual, reversão e aprovação humana necessária.
