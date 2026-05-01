# Fechamento de riscos residuais externos

Este documento transforma as lacunas residuais da rodada tecnica em controles
versionados, comandos verificaveis e bloqueios de release. Ele nao substitui
decisoes humanas, DPO/juridico, contrato com fornecedor, hardware fisico, conta
cloud, DNS, TLS real ou scanners instalados; ele impede que esses pontos sejam
tratados como "resolvidos" sem evidencia.

## Estado de fechamento

| Risco residual | Correcao implementada no repositorio | Verificacao | Bloqueio externo restante |
| --- | --- | --- | --- |
| Provedor cloud, DNS e TLS produtivos | `ops.example.json` agora exige `deployment.provider`, `productionHost`, donos de DNS/TLS, renovacao, sizing, rollback e status de aprovacao. | `tools/operational-readiness-check.ps1 -RequireConfig` falha se `ops.local.json` mantiver placeholders ou status pendente. | Escolha/aprovacao real de provedor, dominio, certificado e responsaveis. |
| Sizing e capacidade de producao | Template operacional exige base de sizing e status validado; `docs/finops-capacity.md` define limites e alertas. | `tools/finops-capacity-check.ps1 -FailOnWarnings` e release gate. | Medidas do host/provedor real e decisao de capacidade. |
| RTO/RPO e restore real | `ops.example.json` ja exige RTO/RPO, backup externo e data de validacao; restore local tem smoke seguro. | `tools/backup-sqlite-operational.ps1 -RunRestoreSmoke` e `tools/operational-readiness-check.ps1 -RequireConfig`. | Restore real em ambiente alvo, destino off-host e aprovacao de RTO/RPO. |
| Bases legais, DPA e RIPD | Template operacional exige status de base legal, DPA, RIPD, canal do titular e evidencia DPO. | `tools/operational-readiness-check.ps1 -RequireConfig` bloqueia status pendente. | Validacao juridica/DPO formal e evidencias fora do Git. |
| Contrato fisico Control iD | `ops.example.json` exige dono do equipamento, firmware, rede de bancada, data e evidencia; release gate exige contrato fisico. | `tools/test-readiness-gates.ps1 -ReleaseGate` chama `tools/contract-controlid-device.ps1`. | Hardware, firmware, rede e credenciais reais fora do Git. |
| Scanners externos SAST/OSV/DAST/a11y | Template operacional exige ownership, status por scanner, data e relatorio; runbook e script orquestram ferramentas. | `tools/external-security-scans.ps1 -InventoryOnly -RequireTools` e `tools/test-readiness-gates.ps1 -ReleaseGate`. | Instalacao/aprovacao das ferramentas e URL local/staging controlada. |
| Billing e budget real | `ops.example.json` exige budget, dashboard, alertas e fonte de gasto real; FinOps check valida o contrato documental. | `tools/finops-capacity-check.ps1 -FailOnWarnings` e `tools/operational-readiness-check.ps1 -RequireConfig`. | Conta/provedor real, budget aprovado e dono de custo. |

## Gate minimo sem ambiente real

Use este gate durante desenvolvimento local. Ele valida somente o que pode ser
testado sem credenciais reais, hardware ou scanners externos:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1
```

## Gate estrito sem excecoes

Use este gate para release operacional. Ele deve falhar quando faltar qualquer
dependencia externa obrigatoria:

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
- contrato com equipamento fisico real;
- scanners externos instalados e executados.

## Configuracao operacional local

1. Copie `ops.example.json` para `ops.local.json` fora do Git.
2. Substitua todos os placeholders por referencias internas reais, sem secrets.
3. Use status `approved`, `validated`, `homologated`, `accepted` ou equivalentes
   em portugues somente quando houver evidencia real.
4. Use `not-applicable` apenas quando a decisao externa estiver registrada e
   aprovada.
5. Rode:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\operational-readiness-check.ps1 -RequireConfig
```

## Regras de evidencia

- Nao versionar `ops.local.json`.
- Nao versionar relatorios com segredo, IP sensivel, payload pessoal, foto,
  biometria, cartao, QR Code ou banco SQLite.
- Guardar evidencias reais em repositorio restrito definido em `ops.local.json`.
- Registrar qualquer excecao como risco aceito por dono humano; nao remover o
  bloqueio do gate para fazer release passar.

## Estado final desta correcao

Todas as lacunas residuais conhecidas agora possuem pelo menos um destes
controles:

- campo obrigatorio em `ops.example.json`;
- validacao em `tools/operational-readiness-check.ps1 -RequireConfig`;
- bloqueio em `tools/test-readiness-gates.ps1 -ReleaseGate`;
- runbook com comando real e artefato esperado;
- teste automatizado de governanca documental.

O que permanece fora do alcance do repositorio e a execucao real de decisoes
externas: contratar provedor, emitir certificado, validar juridicamente bases,
rodar hardware fisico e instalar scanners no host/CI. Essas dependencias agora
sao bloqueios explicitos, nao lacunas silenciosas.
