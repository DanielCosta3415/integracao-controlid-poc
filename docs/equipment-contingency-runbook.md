# Contingência operacional do equipamento Control iD

Escopo: continuidade operacional quando o equipamento Control iD, a rede, a
Access API, callbacks, push ou mecanismos de identificação falham. Este runbook
não substitui procedimento físico aprovado pelo cliente, política de segurança
patrimonial ou validação do fornecedor.

## Responsabilidades a preencher

| Campo | Valor esperado |
| --- | --- |
| Dono da operação física | Preencher em `ops.local.json` como `equipment.manualAccessProcedureOwner`. |
| Local do procedimento manual aprovado | Preencher em `ops.local.json` como `equipment.fallbackProcedureLocation`. |
| Canal de suporte Control iD/fornecedor | Preencher em `ops.local.json` como `equipment.vendorSupportChannel`. |
| Cadência de teste de contingência | Preencher em `ops.local.json` como `equipment.testCadence`. |
| Status do contrato físico | Preencher em `ops.local.json` como `hardwareContract.validationStatus`. |
| Evidência do contrato físico | Preencher em `ops.local.json` como `hardwareContract.reportLocation`. |

## Sinais de acionamento

- `OBS-004` ou `OBS-005` acionado por timeout/circuit breaker da Access API.
- Equipamento sem resposta a `system_information.fcgi`, login ou logout.
- Callback/push sem eventos em janela esperada.
- Falha recorrente de cartão, QR code, biometria, face ou senha.
- Queda de energia, rede, firmware instável ou manutenção física.

## Contingência manual

Use somente quando aprovado por responsável humano autorizado:

1. Classificar impacto e severidade no `docs/incident-response-and-dr.md`.
2. Confirmar identidade do solicitante por procedimento físico aprovado fora do
   sistema.
3. Registrar manualmente: horário, local, operador, motivo, autorizador, pessoa
   liberada e evidência mínima sem dado sensível desnecessário.
4. Evitar coleta de foto, biometria, documento ou payload bruto no registro
   manual, salvo exigência formal aprovada.
5. Manter dupla aprovação para liberação excepcional quando envolver área crítica.
6. Reconciliar o registro manual com a PoC/equipamento quando a integração voltar.
7. Abrir postmortem se a contingência durar mais que a janela aprovada ou se
   houver divergência de auditoria.

## Diagnóstico seguro

1. Verificar `/health/live` e `/health/ready` da PoC.
2. Conferir se a falha é local, do SQLite, da rede, do DNS, do proxy, do firmware ou da energia.
3. Executar contrato físico somente com credenciais de ambiente seguro:

```powershell
$env:CONTROLID_DEVICE_URL = "http://<ip-ou-host-do-equipamento>:8080"
$env:CONTROLID_USERNAME = "<usuario-autorizado>"
$env:CONTROLID_PASSWORD = "<senha-autorizada>"
powershell -ExecutionPolicy Bypass -File .\tools\contract-controlid-device.ps1
```

4. Não colar session string, senha, shared key, payload de usuário, foto ou
   biometria em tickets.
5. Se o equipamento estiver comprometido, isolar rede/host e acionar fornecedor.

## Recuperação e reconciliação

| Etapa | Validação |
| --- | --- |
| Conectividade restaurada | `contract-controlid-device.ps1` passa em leitura/sessão segura. |
| PoC saudável | `/health/live`, `/health/ready` e logs sem 5xx recorrente. |
| Callbacks/push normalizados | Evento fictício/autorizado chega e persiste sem `persistence_failed`. |
| Registros manuais reconciliados | Operação confirma divergências resolvidas ou documentadas. |
| Privacidade revisada | Nenhum registro manual contém dado pessoal excessivo. |

## Validação da contingência

Antes de uso real, executar em bancada:

- Simular equipamento offline e validar escalonamento.
- Simular perda de rede e validar manual fallback.
- Simular callback rejeitado por assinatura/IP e validar diagnóstico.
- Simular retorno do equipamento e reconciliação de registros manuais.
- Registrar evidências minimizadas e atualizar `ops.local.json`.
- Para release operacional, `tools/test-readiness-gates.ps1 -ReleaseGate` deve
  executar o contrato físico e `tools/operational-readiness-check.ps1 -RequireConfig`
  deve validar os campos `hardwareContract.*`.
