# ADR 0004 - Governanca de release por scripts locais versionados

Status: Aceita

Data: 2026-05-01

## Contexto

O projeto ainda nao possui provedor cloud versionado nem pipeline de deploy. Mesmo
assim, precisa separar falhas preexistentes, validar seguranca e impedir release
operacional sem evidencias minimas.

## Decisao

Centralizar readiness em scripts PowerShell versionados, principalmente
`tools/test-readiness-gates.ps1`, com modo padrao e `-ReleaseGate` estrito.
Configuracoes humanas reais devem ficar em `ops.local.json`, fora do Git, baseado
em `ops.example.json`.

## Alternativas consideradas

- Confiar apenas na CI: insuficiente para equipamento fisico, scanners externos,
  billing real e operacao local.
- Criar deploy automatico agora: rejeitado por falta de decisao de provedor.
- Manter checks somente em texto: reduz reprodutibilidade.

## Consequencias

- Readiness fica reproduzivel em dev e CI.
- Release real falha quando faltam contrato fisico, observabilidade online,
  scanners externos, FinOps/capacidade ou configuracao operacional.
- Alguns checks sao opt-in por dependerem de ferramentas, credenciais ou hardware.
- Mudancas nos scripts devem ser refletidas em README, AGENTS e docs.

## Evidencias

- `tools/test-readiness-gates.ps1`
- `tools/operational-readiness-check.ps1`
- `tools/finops-capacity-check.ps1`
- `tools/external-security-scans.ps1`
- `.github/workflows/ci.yml`
- `ops.example.json`
