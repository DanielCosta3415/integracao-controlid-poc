# PR summary - documentacao tecnica e governanca

## Resumo

Esta rodada reorganiza a documentacao tecnica para onboarding, manutencao e
operacao segura da PoC Control iD. Nao altera regra de negocio nem contrato
publico de API.

## Mudancas principais

- README reestruturado com stack, setup, comandos oficiais, operacao, variaveis,
  banco, observabilidade, container, fluxos e troubleshooting.
- Novo indice em `docs/README.md`.
- Novo guia de onboarding em `docs/developer-onboarding.md`.
- Nova visao de arquitetura em `docs/architecture-overview.md`.
- ADRs criados em `docs/adrs/`.
- Changelog e auditoria documental de 2026-05-01 adicionados.
- Fechamento de riscos residuais externos em `docs/residual-risk-closure.md`.
- `ops.example.json` e readiness operacional agora exigem aprovacoes e
  evidencias de deployment, DPO/juridico, scanners externos e contrato fisico.

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

## Pendencias conhecidas

- Provedor cloud, DNS/TLS real e billing real continuam fora do repositorio, mas
  sao bloqueados por `ops.local.json` e readiness estrito ate aprovacao.
- Bases legais, DPA, RIPD e comunicacoes externas exigem DPO/juridico, com
  status obrigatorio em `privacy.*`.
- Contrato com equipamento fisico exige bancada real e credenciais fora do Git,
  com bloqueio por `test-readiness-gates.ps1 -ReleaseGate`.
- Scanners externos exigem ferramentas instaladas/aprovadas e URL controlada; o
  release gate falha se estiverem ausentes.
