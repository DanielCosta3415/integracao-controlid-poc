# ADR 0004 - Governança de release por scripts locais versionados

Status: Aceita

Data: 2026-05-01

## Contexto

O projeto ainda não possui provedor cloud versionado nem pipeline de deploy. Mesmo
assim, precisa separar falhas preexistentes, validar segurança e impedir release
operacional sem evidências mínimas.

## Decisão

Centralizar readiness em scripts PowerShell versionados, principalmente
`tools/test-readiness-gates.ps1`, com modo padrão e `-ReleaseGate` estrito.
Configurações humanas reais devem ficar em `ops.local.json`, fora do Git, baseado
em `ops.example.json`.

## Alternativas consideradas

- Confiar apenas na CI: insuficiente para equipamento físico, scanners externos,
  billing real e operação local.
- Criar deploy automático agora: rejeitado por falta de decisão de provedor.
- Manter checks somente em texto: reduz reprodutibilidade.

## Consequências

- Readiness fica reproduzível em dev e CI.
- Release real falha quando faltam contrato físico, observabilidade online,
  scanners externos, FinOps/capacidade ou configuração operacional.
- Alguns checks são opt-in por dependerem de ferramentas, credenciais ou hardware.
- Mudanças nos scripts devem ser refletidas em README, AGENTS e docs.

## Evidências

- `tools/test-readiness-gates.ps1`
- `tools/operational-readiness-check.ps1`
- `tools/finops-capacity-check.ps1`
- `tools/external-security-scans.ps1`
- `.github/workflows/ci.yml`
- `ops.example.json`
