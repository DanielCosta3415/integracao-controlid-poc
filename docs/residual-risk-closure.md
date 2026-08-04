# Fechamento de riscos residuais externos

> **Registro vivo de riscos** · Público: liderança, release e auditoria · Responsável: Risk Owner/Release · Última validação: 2026-08-04.

Este documento transforma as lacunas residuais da rodada técnica em controles
versionados, comandos verificáveis e bloqueios de release. Ele não substitui
decisões humanas, DPO/jurídico, contrato com fornecedor, hardware físico, conta
cloud, DNS, TLS real ou scanners instalados; ele impede que esses pontos sejam
tratados como "resolvidos" sem evidência.

## Estado de fechamento

| Risco residual | Correção implementada no repositório | Verificação | Bloqueio externo restante |
| --- | --- | --- | --- |
| Provedor de nuvem, DNS e TLS produtivos | `ops.example.json` agora exige `deployment.provider`, `productionHost`, responsáveis por DNS/TLS, renovação, dimensionamento, reversão e estado de aprovação. | `tools/operational-readiness-check.ps1 -RequireConfig` falha se `ops.local.json` mantiver valores de exemplo ou estado pendente. | Escolha/aprovação real de provedor, domínio, certificado e responsáveis. |
| Sizing e capacidade de produção | Template operacional exige base de sizing e status validado; `docs/finops-capacity.md` define limites e alertas. | `tools/finops-capacity-check.ps1 -FailOnWarnings` e release gate. | Medidas do host/provedor real e decisão de capacidade. |
| RTO/RPO e restauração real | `ops.example.json` já exige RTO/RPO, cópia externa e data de validação; a restauração local tem teste seguro. | `tools/backup-sqlite-operational.ps1 -RunRestoreSmoke` e `tools/operational-readiness-check.ps1 -RequireConfig`. | Restauração real no ambiente-alvo, destino fora do host e aprovação de RTO/RPO. |
| Bases legais, DPA e RIPD | Template operacional exige status de base legal, DPA, RIPD, canal do titular e evidência DPO. | `tools/operational-readiness-check.ps1 -RequireConfig` bloqueia status pendente. | Validação jurídica/DPO formal e evidências fora do Git. |
| Contrato físico Control iD | `ops.example.json` exige dono do equipamento, firmware, rede de bancada, data e evidência; release gate exige contrato físico. | `tools/test-readiness-gates.ps1 -ReleaseGate` chama `tools/contract-controlid-device.ps1`. | Hardware, firmware, rede e credenciais reais fora do Git. |
| Analisadores externos SAST/OSV/DAST/acessibilidade | Semgrep, OSV, axe e ZAP foram executados localmente em 2026-08-04; o ZAP não encontrou alertas altos, médios ou baixos, e o axe não encontrou violações na página pública. O E2E mantém axe autenticado. | `tools/external-security-scans.ps1 -RequireTools`, E2E e `tools/test-readiness-gates.ps1 -ReleaseGate`. | Repetição em URL de homologação controlada por release. |
| Faturamento e orçamento real | `ops.example.json` exige orçamento, painel, alertas e fonte de gasto real; a verificação FinOps valida o contrato documental. | `tools/finops-capacity-check.ps1 -FailOnWarnings` e `tools/operational-readiness-check.ps1 -RequireConfig`. | Conta/provedor real, orçamento aprovado e responsável pelo custo. |

## Critério mínimo sem ambiente real

Use este gate durante desenvolvimento local. Ele valida somente o que pode ser
testado sem credenciais reais, hardware ou scanners externos:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1
```

## Critério estrito sem exceções

Use este gate para release operacional. Ele deve falhar quando faltar qualquer
dependência externa obrigatória:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -ReleaseGate
```

O `-ReleaseGate` exige:

- smoke local;
- cobertura;
- auditoria da cadeia de suprimentos;
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
- guia operacional com comando real e artefato esperado;
- teste automatizado de governança documental.

O que permanece fora do alcance do repositório é a execução real de decisões
externas: contratar provedor, emitir certificado, validar juridicamente bases,
rodar hardware físico, manter scanners no host/CI e executar DAST no ambiente
alvo. Essas dependências agora
são bloqueios explícitos, não lacunas silenciosas.

## Registro e validade do aceite

| ID | Risco | Dono sugerido | Revisão mínima | Expiração do aceite |
| --- | --- | --- | --- | --- |
| RR-001 | Provedor, DNS e TLS | Platform/SRE | Antes de cada release | Mudança de host, certificado ou provedor |
| RR-002 | Capacidade e custo | FinOps/Owner | Mensal e antes de release | Mudança de carga, plano ou orçamento |
| RR-003 | RTO/RPO e restore | SRE/DR | Trimestral | Falha de restore ou mudança de storage |
| RR-004 | Bases legais, DPA e RIPD | DPO/Jurídico | Conforme política aprovada | Novo tratamento, terceiro ou titular vulnerável |
| RR-005 | Contrato físico Control iD | Integração/Operação | Por firmware/modelo | Atualização de firmware, licença ou rede |
| RR-006 | Scanners externos | AppSec/QA | Por release | Mudança relevante de superfície ou ferramenta |
| RR-007 | Billing real | FinOps/Owner | Mensal | Desvio de orçamento ou preço do fornecedor |

Aceite de risco deve registrar ID, decisão, dono humano, data, validade, evidência
restrita e mitigação compensatória. Aceite vencido volta a bloquear a release; a
ausência de prazo não significa aceite permanente.

## Estado padrão e evidência

Enquanto `ops.local.json` não registrar decisão aprovada e evidência válida,
`RR-001` a `RR-007` permanecem **bloqueados externamente**, embora seus controles
de repositório estejam implementados. O estado permitido é:

- `open`: decisão ou execução ainda ausente;
- `mitigated`: controle executado, risco residual ainda ativo;
- `accepted`: aceite humano com data de expiração;
- `closed`: evidência comprova encerramento e o gate correspondente passa.

Estado textual sem relatório, dono e validade não altera o risco nem libera a
produção.
