# Changelog técnico - 2026-05-01

## O que mudou

- Criado índice central de documentação em `docs/README.md`.
- Criado guia de onboarding técnico em `docs/developer-onboarding.md`.
- Criada visão de arquitetura em `docs/architecture-overview.md`.
- Criados ADRs para SQLite local, segurança de ingress/egress Control iD,
  observabilidade/readiness e governança de release por scripts locais.
- Reestruturado `README.md` para setup, comandos, operação e links principais.
- Criado resumo de PR em `docs/pr-summary-2026-05-01.md`.
- Criada auditoria documental em `docs/documentation-audit-2026-05-01.md`.
- Criado fechamento verificável de riscos residuais externos em
  `docs/residual-risk-closure.md`.
- Expandido `ops.example.json` e `tools/operational-readiness-check.ps1` para
  bloquear release sem provedor/DNS/TLS/sizing, DPO/jurídico, scanners e
  contrato físico validados.

## Por que mudou

A documentação cresceu junto com hardening, testes, observabilidade, privacidade,
FinOps e runbooks. Faltava uma trilha única para novo desenvolvedor entender o
projeto sem reconstruir contexto pela conversa ou por arquivos dispersos.

## Como validar

```powershell
dotnet build .\Integracao.ControlID.PoC.sln --no-restore -v:minimal
dotnet test .\Integracao.ControlID.PoC.sln --no-build -v:minimal
dotnet format .\Integracao.ControlID.PoC.sln --verify-no-changes --no-restore -v:minimal
git diff --check
powershell -ExecutionPolicy Bypass -File .\tools\scan-secrets.ps1
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1
```

## Riscos

- Decisões humanas continuam necessárias para provedor, faturamento, DPO ou departamento jurídico e
  equipamento físico real, mas agora são bloqueios explicitos em
  `ops.local.json` e no release gate.
- README foi reescrito para ASCII limpo; se algum consumidor dependia de texto
  anterior, deve usar os documentos técnicos agora indexados.

## Dependências externas controladas

- Preencher `ops.local.json` fora do Git em ambiente real.
- Validar contrato com equipamento físico.
- Validar RTO/RPO e backup externo em ambiente alvo.
- Formalizar bases legais, DPA e RIPD com DPO/jurídico.
- Instalar/aprovar scanners externos e registrar relatórios restritos.

O release sem exceções deve usar:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -ReleaseGate
```
