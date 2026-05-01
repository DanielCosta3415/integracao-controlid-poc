# Contingencia operacional do equipamento Control iD

Escopo: continuidade operacional quando o equipamento Control iD, a rede, a
Access API, callbacks, push ou mecanismos de identificacao falham. Este runbook
nao substitui procedimento fisico aprovado pelo cliente, politica de seguranca
patrimonial ou validacao do fornecedor.

## Responsabilidades a preencher

| Campo | Valor esperado |
| --- | --- |
| Dono da operacao fisica | Preencher em `ops.local.json` como `equipment.manualAccessProcedureOwner`. |
| Local do procedimento manual aprovado | Preencher em `ops.local.json` como `equipment.fallbackProcedureLocation`. |
| Canal de suporte Control iD/fornecedor | Preencher em `ops.local.json` como `equipment.vendorSupportChannel`. |
| Cadencia de teste de contingencia | Preencher em `ops.local.json` como `equipment.testCadence`. |
| Status do contrato fisico | Preencher em `ops.local.json` como `hardwareContract.validationStatus`. |
| Evidencia do contrato fisico | Preencher em `ops.local.json` como `hardwareContract.reportLocation`. |

## Sinais de acionamento

- `OBS-004` ou `OBS-005` acionado por timeout/circuit breaker da Access API.
- Equipamento sem resposta a `system_information.fcgi`, login ou logout.
- Callback/push sem eventos em janela esperada.
- Falha recorrente de cartao, QR code, biometria, face ou senha.
- Queda de energia, rede, firmware instavel ou manutencao fisica.

## Manual fallback

Use somente quando aprovado por responsavel humano autorizado:

1. Classificar impacto e severidade no `docs/incident-response-and-dr.md`.
2. Confirmar identidade do solicitante por procedimento fisico aprovado fora do
   sistema.
3. Registrar manualmente: horario, local, operador, motivo, autorizador, pessoa
   liberada e evidencia minima sem dado sensivel desnecessario.
4. Evitar coleta de foto, biometria, documento ou payload bruto no registro
   manual, salvo exigencia formal aprovada.
5. Manter dupla aprovacao para liberacao excepcional quando envolver area critica.
6. Reconciliar o registro manual com a PoC/equipamento quando a integracao voltar.
7. Abrir postmortem se a contingencia durar mais que a janela aprovada ou se
   houver divergencia de auditoria.

## Diagnostico seguro

1. Verificar `/health/live` e `/health/ready` da PoC.
2. Conferir se a falha e local, de SQLite, rede, DNS, proxy, firmware ou energia.
3. Executar contrato fisico somente com credenciais de ambiente seguro:

```powershell
$env:CONTROLID_DEVICE_URL = "http://<ip-ou-host-do-equipamento>:8080"
$env:CONTROLID_USERNAME = "<usuario-autorizado>"
$env:CONTROLID_PASSWORD = "<senha-autorizada>"
powershell -ExecutionPolicy Bypass -File .\tools\contract-controlid-device.ps1
```

4. Nao colar session string, senha, shared key, payload de usuario, foto ou
   biometria em tickets.
5. Se o equipamento estiver comprometido, isolar rede/host e acionar fornecedor.

## Recuperacao e reconciliacao

| Etapa | Validacao |
| --- | --- |
| Conectividade restaurada | `contract-controlid-device.ps1` passa em leitura/sessao segura. |
| PoC saudavel | `/health/live`, `/health/ready` e logs sem 5xx recorrente. |
| Callbacks/push normalizados | Evento ficticio/autorizado chega e persiste sem `persistence_failed`. |
| Registros manuais reconciliados | Operacao confirma divergencias resolvidas ou documentadas. |
| Privacidade revisada | Nenhum registro manual contem dado pessoal excessivo. |

## Contingency validation

Antes de uso real, executar em bancada:

- Simular equipamento offline e validar escalonamento.
- Simular perda de rede e validar manual fallback.
- Simular callback rejeitado por assinatura/IP e validar diagnostico.
- Simular retorno do equipamento e reconciliacao de registros manuais.
- Registrar evidencias minimizadas e atualizar `ops.local.json`.
- Para release operacional, `tools/test-readiness-gates.ps1 -ReleaseGate` deve
  executar o contrato fisico e `tools/operational-readiness-check.ps1 -RequireConfig`
  deve validar os campos `hardwareContract.*`.
