# Fechamento de riscos residuais externos

Este documento transforma as lacunas residuais da rodada técnica em controles
versionados, comandos verificáveis e bloqueios de release. Ele não substitui
decisões humanas, DPO/jurídico, contrato com fornecedor, hardware físico, conta
cloud, DNS, TLS real ou scanners instalados; ele impede que esses pontos sejam
tratados como "resolvidos" sem evidência.

## Estado de fechamento

| Risco residual | Correção implementada no repositório | Verificação | Bloqueio externo restante |
| --- | --- | --- | --- |
| Provedor cloud, DNS e TLS produtivos | `ops.example.json` agora exige `deployment.provider`, `productionHost`, donos de DNS/TLS, renovação, sizing, rollback e status de aprovação. | `tools/operational-readiness-check.ps1 -RequireConfig` falha se `ops.local.json` mantiver placeholders ou status pendente. | Escolha/aprovação real de provedor, domínio, certificado e responsáveis. |
| Sizing e capacidade de produção | Template operacional exige base de sizing e status validado; `docs/finops-capacity.md` define limites e alertas. | `tools/finops-capacity-check.ps1 -FailOnWarnings` e release gate. | Medidas do host/provedor real e decisão de capacidade. |
| RTO/RPO e restore real | `ops.example.json` já exige RTO/RPO, backup externo e data de validação; restore local tem smoke seguro. | `tools/backup-sqlite-operational.ps1 -RunRestoreSmoke` e `tools/operational-readiness-check.ps1 -RequireConfig`. | Restore real em ambiente alvo, destino off-host e aprovação de RTO/RPO. |
| Bases legais, DPA e RIPD | Template operacional exige status de base legal, DPA, RIPD, canal do titular e evidência DPO. | `tools/operational-readiness-check.ps1 -RequireConfig` bloqueia status pendente. | Validação jurídica/DPO formal e evidências fora do Git. |
| Contrato físico Control iD | `ops.example.json` exige dono do equipamento, firmware, rede de bancada, data e evidência; release gate exige contrato físico. | `tools/test-readiness-gates.ps1 -ReleaseGate` chama `tools/contract-controlid-device.ps1`. | Hardware, firmware, rede e credenciais reais fora do Git. |
| Scanners externos SAST/OSV/DAST/a11y | Template operacional exige ownership, status por scanner, data e relatório; runbook e script orquestram ferramentas. | `tools/external-security-scans.ps1 -InventoryOnly -RequireTools` e `tools/test-readiness-gates.ps1 -ReleaseGate`. | Instalação/aprovação das ferramentas e URL local/staging controlada. |
| Billing e budget real | `ops.example.json` exige budget, dashboard, alertas e fonte de gasto real; FinOps check valida o contrato documental. | `tools/finops-capacity-check.ps1 -FailOnWarnings` e `tools/operational-readiness-check.ps1 -RequireConfig`. | Conta/provedor real, budget aprovado e dono de custo. |

## Gate mínimo sem ambiente real

Use este gate durante desenvolvimento local. Ele valida somente o que pode ser
testado sem credenciais reais, hardware ou scanners externos:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1
```

## Gate estrito sem exceções

Use este gate para release operacional. Ele deve falhar quando faltar qualquer
dependência externa obrigatória:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -ReleaseGate
```

O `-ReleaseGate` exige:

- smoke local;
- cobertura;
- auditoria de supply chain;
- build de container;
- observabilidade online;
- `ops.local.json` fora do Git, preenchido e sem placeholders;
- FinOps/capacidade sem warnings;
- contrato com equipamento físico real;
- scanners externos instalados e executados.

## Configuração operacional local

1. Copie `ops.example.json` para `ops.local.json` fora do Git.
2. Substitua todos os placeholders por referências internas reais, sem secrets.
3. Use status `approved`, `validated`, `homologated`, `accepted` ou equivalentes
   em português somente quando houver evidência real.
4. Use `not-applicable` apenas quando a decisão externa estiver registrada e
   aprovada.
5. Rode:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\operational-readiness-check.ps1 -RequireConfig
```

## Regras de evidência

- Não versionar `ops.local.json`.
- Não versionar relatórios com segredo, IP sensível, payload pessoal, foto,
  biometria, cartão, QR Code ou banco SQLite.
- Guardar evidências reais em repositório restrito definido em `ops.local.json`.
- Registrar qualquer exceção como risco aceito por dono humano; não remover o
  bloqueio do gate para fazer release passar.

## Estado final desta correção

Todas as lacunas residuais conhecidas agora possuem pelo menos um destes
controles:

- campo obrigatório em `ops.example.json`;
- validação em `tools/operational-readiness-check.ps1 -RequireConfig`;
- bloqueio em `tools/test-readiness-gates.ps1 -ReleaseGate`;
- runbook com comando real e artefato esperado;
- teste automatizado de governança documental.

O que permanece fora do alcance do repositório é a execução real de decisões
externas: contratar provedor, emitir certificado, validar juridicamente bases,
rodar hardware físico e instalar scanners no host/CI. Essas dependências agora
são bloqueios explicitos, não lacunas silenciosas.
