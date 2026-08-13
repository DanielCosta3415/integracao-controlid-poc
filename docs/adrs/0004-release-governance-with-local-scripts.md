# ADR 0004 - Governança de liberação por scripts locais versionados

> **Decisão** · Público: arquitetura e release · Responsável: Engenharia · Última validação: 2026-08-12.

Estado: aceita

- Data da decisão: 2026-05-01
- Substitui: nenhuma decisão
- Substituída por: nenhuma decisão

## Direcionadores

- mesma validação básica no notebook e na CI;
- separação explícita entre integração contínua e implantação;
- falha visível quando hardware, provedor ou aprovação humana estiverem ausentes;
- evidências versionadas sem segredos ou dados de produção.

## Contexto

O projeto ainda não possui provedor de nuvem versionado nem fluxo de implantação. Mesmo
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
- Criar implantação automática agora: rejeitado por falta de decisão de provedor.
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
- `tests/Integracao.ControlID.PoC.Tests/Tools/ReadinessGateContractTests.cs`
- `tests/Integracao.ControlID.PoC.Tests/Platform/CiQualityGateContractTests.cs`

## Critério de revisão

Reavalie ao escolher plataforma de implantação ou orquestrador. Uma futura CD
deve continuar separada da CI, exigir aprovação humana para produção e preservar
os gates locais como caminho de reprodução.

## Evolução da decisão

- Substitui: nenhuma decisão anterior.
- Substituída por: nenhuma até esta validação.
- Exceção: exige risco, responsável humano, prazo, mitigação e evidência; nunca
  é obtida removendo uma verificação do script.
- Gatilho de revisão: adoção de plataforma de implantação, assinatura de
  artefatos ou política corporativa com controles equivalentes ou mais fortes.

## Navegação documental

- [Voltar ao índice deste domínio](README.md).
- [Abrir a central de documentação](../README.md).
