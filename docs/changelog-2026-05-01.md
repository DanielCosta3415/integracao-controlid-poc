# Changelog tecnico - 2026-05-01

## O que mudou

- Criado indice central de documentacao em `docs/README.md`.
- Criado guia de onboarding tecnico em `docs/developer-onboarding.md`.
- Criada visao de arquitetura em `docs/architecture-overview.md`.
- Criados ADRs para SQLite local, seguranca de ingress/egress Control iD,
  observabilidade/readiness e governanca de release por scripts locais.
- Reestruturado `README.md` para setup, comandos, operacao e links principais.
- Criado resumo de PR em `docs/pr-summary-2026-05-01.md`.
- Criada auditoria documental em `docs/documentation-audit-2026-05-01.md`.
- Criado fechamento verificavel de riscos residuais externos em
  `docs/residual-risk-closure.md`.
- Expandido `ops.example.json` e `tools/operational-readiness-check.ps1` para
  bloquear release sem provedor/DNS/TLS/sizing, DPO/juridico, scanners e
  contrato fisico validados.

## Por que mudou

A documentacao cresceu junto com hardening, testes, observabilidade, privacidade,
FinOps e runbooks. Faltava uma trilha unica para novo desenvolvedor entender o
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

- Decisoes humanas continuam necessarias para provedor, billing, DPO/juridico e
  equipamento fisico real, mas agora sao bloqueios explicitos em
  `ops.local.json` e no release gate.
- README foi reescrito para ASCII limpo; se algum consumidor dependia de texto
  anterior, deve usar os documentos tecnicos agora indexados.

## Dependencias externas controladas

- Preencher `ops.local.json` fora do Git em ambiente real.
- Validar contrato com equipamento fisico.
- Validar RTO/RPO e backup externo em ambiente alvo.
- Formalizar bases legais, DPA e RIPD com DPO/juridico.
- Instalar/aprovar scanners externos e registrar relatorios restritos.

O release sem excecoes deve usar:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -ReleaseGate
```
