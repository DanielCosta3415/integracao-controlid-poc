# Runbook de observabilidade e operabilidade

Escopo: PoC ASP.NET Core MVC/Razor para integracao com a Access API Control iD.
Este runbook define sinais operacionais, eventos criticos, metricas, alertas e
dashboards sem expor dados pessoais, credenciais, payloads completos ou detalhes
internos ao usuario final.

## Endpoints operacionais

| Endpoint | Finalidade | Dependencia | Exposicao recomendada |
| --- | --- | --- | --- |
| `GET /health/live` | Verifica se o processo ASP.NET Core responde | Processo web | Pode ser usado por supervisor local ou load balancer |
| `GET /health/ready` | Verifica se o SQLite local pode ser acessado | SQLite/runtime state | Usar para readiness antes de enviar trafego |
| `GET /metrics` | Exporta snapshot Prometheus text das metricas locais | Auth local/RBAC | Protegido por administrador por padrao |

As respostas de health check sao JSON minimizado com `status`, duracao e nomes dos
checks. Excecoes, paths locais, connection string e stack trace nao sao serializados.

O endpoint `/metrics` fica habilitado por `Observability:Metrics:Enabled=true` e
exige `AdministratorOnly` por padrao. `Observability:Metrics:AllowAnonymous=true`
so deve ser usado em `Development`; fora de `Development` a aplicacao bloqueia o
startup se essa opcao estiver ativa.

## Correlacao e tracing

- Header inbound/outbound: `X-Correlation-ID`.
- Valores aceitos: ate 128 caracteres, somente letras, numeros, `-`, `_`, `.`, `:`
  e `/`.
- Valor invalido, vazio ou longo demais e substituido por um identificador gerado.
- Toda resposta HTTP recebe `X-Correlation-ID`.
- O middleware registra `CorrelationId` e `TraceId` no escopo de log.
- Chamadas outbound para a Access API recebem o mesmo `X-Correlation-ID` quando a
  request atual possui contexto HTTP.

## Eventos criticos

| Evento | Origem | Nivel esperado | Dados permitidos |
| --- | --- | --- | --- |
| Login local concluido | `AuthController.LocalLogin` | Information | user ref pseudonimizado, role |
| Falha de login local | `AuthController.LocalLogin` | Warning | identificador pseudonimizado |
| Logout local | `AuthController.LocalLogout` | Information | correlation id |
| Login/logout no equipamento | `AuthController`, `OfficialApiInvokerService` | Information/Warning/Error | endpoint, status, device ref pseudonimizado |
| Chamada oficial Control iD | `OfficialApiInvokerService` | Information | endpoint id, metodo, path oficial, target pseudonimizado, status, duracao |
| Timeout/falha oficial | `OfficialApiInvokerService` | Warning/Error | endpoint id, status group, duracao, target pseudonimizado |
| Callback aceito | `CallbackIngressService` | Information | path, event id, event family, device ref pseudonimizado |
| Callback bloqueado | `CallbackIngressService`, `Push*Controller` | Warning | path, status, motivo funcional |
| Falha de persistencia de callback/push | Services/controllers de callback/push | Error | path, event family, command id quando aplicavel |
| Push enfileirado/entregue/resultado | `PushCommandWorkflowService`, `PushCenterController` | Information | command id, device ref pseudonimizado, status, bytes |
| Limpeza/expurgo manual | `PushCommandWorkflowService`, monitor repositories | Warning | quantidade removida e cutoff |
| Erro 5xx nao tratado | `ExceptionHandlingMiddleware` | Error | correlation id, trace id; detalhes apenas no log |
| Request HTTP concluida | `RequestLoggingMiddleware` | Information/Warning | metodo, path sem query, status, duracao, IP/user refs |

## Metricas instrumentadas

As metricas sao publicadas via `System.Diagnostics.Metrics` no meter
`Integracao.ControlID.PoC.Operations` e tambem expostas em `/metrics` em formato
Prometheus text, sem dependencia externa. Labels de rota substituem segmentos
numericos/GUID por `{id}` para reduzir cardinalidade e evitar identificadores em
claro.

| Metrica | Tipo | Tags | Uso |
| --- | --- | --- | --- |
| `controlid.http.requests` | Counter | `method`, `path`, `status_group` | Taxa de requests e erros por rota |
| `controlid.http.request.duration` | Histogram | `method`, `path`, `status_group` | Latencia HTTP local |
| `controlid.local_auth.attempts` | Counter | `outcome`, `role` | Falhas/sucessos de auth local e device auth |
| `controlid.official_api.invocations` | Counter | `endpoint_id`, `method`, `outcome`, `status_group` | Disponibilidade da Access API |
| `controlid.official_api.duration` | Histogram | `endpoint_id`, `method`, `outcome`, `status_group` | Latencia de equipamento/firmware/rede |
| `controlid.callback.ingress` | Counter | `event_family`, `path`, `outcome`, `status_group` | Aceite/rejeicao de callbacks, monitor e push ingress |
| `controlid.push.operations` | Counter | `operation`, `outcome` | Fila push, polling, resultado, clear e purge |
| `controlid.product.flow.events` | Counter | `flow`, `event`, `action`, `outcome`, `status_group` | Uso privacy-aware de fluxos de produto sem usuario, IP, query, body ou payload |
| `controlid.product.flow.duration` | Histogram | `flow`, `event`, `action`, `outcome`, `status_group` | Tempo percebido por fluxo de produto |
| `controlid.runtime.process.memory.bytes` | Gauge | `scope` | Memoria de processo sem expor host/path |
| `controlid.runtime.managed_heap.bytes` | Gauge | `scope` | Heap gerenciado .NET |
| `controlid.runtime.storage.local.bytes` | Gauge | `scope` | Tamanho agregado de SQLite, logs, artifacts e reports, sem path real |
| `controlid.runtime.disk.total.bytes` | Gauge | `scope` | Capacidade total do disco/volume para dados e logs |
| `controlid.runtime.disk.free.bytes` | Gauge | `scope` | Espaco livre do disco/volume para dados e logs |
| `controlid.runtime.disk.free.percent` | Gauge | `scope` | Percentual livre para alertas de capacidade |

O catalogo de eventos, KPIs e propriedades permitidas fica versionado em
`docs/product-analytics.md`. Nao adicione labels livres, identificadores reais
ou propriedades de analytics sem revisao de privacidade.

As metricas runtime/FinOps sao calculadas no momento da coleta de `/metrics` e
usam apenas labels fixas como `sqlite`, `logs`, `artifacts`, `reports`, `data` e
`working_set`. Paths locais, connection string, nome de arquivo e host real nao
sao serializados.

## Alertas recomendados

Regras versionadas: `docs/observability/alert-rules.json`.

| Alerta | Sinal | Threshold inicial | Severidade | Acao esperada |
| --- | --- | --- | --- | --- |
| Aplicacao indisponivel | `/health/live` != Healthy por 2 checks | 2 min | Critico | Reiniciar processo, verificar porta, logs de startup |
| Runtime state indisponivel | `/health/ready` != Healthy por 2 checks | 2 min | Critico | Verificar arquivo SQLite, lock, permissao e disco |
| Erros HTTP 5xx | `controlid.http.requests{status_group="5xx"}` | > 0 em 5 min | Alto | Buscar `CorrelationId`, verificar excecao e rota |
| Timeout Control iD | `official_api.invocations{outcome="timeout"}` | >= 3 em 10 min por endpoint | Alto | Conferir IP/porta/rede/firmware/sessao do equipamento |
| Circuit breaker aberto | `outcome="blocked_circuit_open"` | >= 1 em 5 min | Alto | Pausar operacao, validar disponibilidade do equipamento |
| Falha de auth local | `local_auth.attempts{outcome="failed"}` | >= 10 em 5 min por origem | Medio | Conferir abuso, rate limit e usuario afetado |
| Callback rejeitado | `callback.ingress{outcome=~".*_rejected"}` | >= 5 em 5 min | Alto | Validar shared key, assinatura, IP permitido e payload |
| Falha de persistencia | logs `CallbackPersistenceFailed` ou `Push result persist_failed` | qualquer ocorrencia | Critico | Verificar SQLite, migrations, disco e permissao |
| Resultado push sem command id | logs de `/result` sem id/chave | >= 1 em 10 min | Medio | Revisar firmware/configuracao do equipamento |
| Expurgo/limpeza manual | evento `PushQueueCleared` | qualquer ocorrencia | Medio | Confirmar operador, janela e impacto esperado |
| Storage/logs acima do budget | `tools/finops-capacity-check.ps1` ou monitor do host | ver `FIN-*` | Medio/Alto | Revisar SQLite, logs, backups e artifacts sem apagar dados sem confirmacao |

## Dashboards sugeridos

Especificacao versionada: `docs/observability/dashboard.json`.

### Saude do processo

- Status de `/health/live` e `/health/ready`.
- Requests por minuto por `status_group`.
- P95/P99 de `controlid.http.request.duration`.
- Top rotas 5xx por `path`.

### Integracao Control iD

- Invocacoes por `endpoint_id` e `outcome`.
- P95/P99 de `controlid.official_api.duration`.
- Timeouts e circuit breaker aberto por endpoint.
- Status groups retornados pelo equipamento.

### Ingressos externos

- Callbacks aceitos/rejeitados por `event_family` e `path`.
- Rejeicoes por motivo logado: shared key, assinatura, IP, payload grande.
- Volume de monitor/push por janela.

### Push operacional

- Comandos enfileirados, entregues, vazios e resultados persistidos.
- Falhas de persistencia.
- Clear/purge manuais.

### Seguranca e privacidade

- Falhas de login local.
- Falhas de autorizacao/403 por rota.
- Requests 401/403/429.
- Eventos de startup de configuracao insegura.
- Amostras de logs revisadas sem payload bruto, senha, token ou biometria.

## Dados proibidos em logs

Nunca registrar:

- Senhas, session string oficial, tokens, API keys, shared key, assinaturas HMAC.
- Headers `Authorization`, cookies, chaves de callback ou secrets.
- Documentos, cartoes, QR Codes, fotos, biometria/template, payload bruto completo.
- IP real ou identificador de usuario em claro quando uma referencia pseudonimizada
  for suficiente.
- Connection string, path local sensivel, stack trace ou excecao completa em resposta
  ao usuario final.

## Procedimento de incidente

Runbooks detalhados por cenario, matriz SEV, continuidade, DR e template de
postmortem ficam em `docs/incident-response-and-dr.md`. Use a sequencia abaixo
como triagem inicial e escale para o runbook dedicado quando houver alerta real.

1. Copiar o `X-Correlation-ID` da resposta, log ou tela de erro.
2. Buscar o correlation id em `Logs/app_log.txt` ou no coletor externo.
3. Identificar rota, status, duracao, usuario/IP pseudonimizados e evento operacional.
4. Se envolver equipamento, correlacionar endpoint id, target pseudonimizado, status e
   timeout/circuit breaker.
5. Se envolver callback/push, validar shared key, assinatura, IP permitido, tamanho de
   payload e permissao de escrita SQLite.
6. Se envolver dado pessoal, acionar `docs/privacy-governance-runbook.md` e registrar
   evidencias sem dados reais no ticket/incidente.
7. Depois da mitigacao, rodar build, testes e smoke relevante antes de liberar nova
   versao.

## Monitor local versionado

Validacao offline dos artefatos de observabilidade:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\observability-check.ps1 -OfflineValidateOnly
```

Validacao contra uma aplicacao local:

```powershell
$env:OBSERVABILITY_BASE_URL = "http://localhost:5000"
powershell -ExecutionPolicy Bypass -File .\tools\observability-check.ps1
```

Se `/metrics` estiver protegido por cookie local, informe apenas no ambiente:

```powershell
$env:OBSERVABILITY_METRICS_COOKIE = ".IntegracaoControlID.Auth=<cookie-local>"
powershell -ExecutionPolicy Bypass -File .\tools\observability-check.ps1 -RequireMetrics
```

Para bloquear release sem equipamento fisico:

```powershell
$env:CONTROLID_DEVICE_URL = "http://<ip-ou-host-do-equipamento>:8080"
$env:CONTROLID_USERNAME = "<usuario>"
$env:CONTROLID_PASSWORD = "<senha>"
powershell -ExecutionPolicy Bypass -File .\tools\observability-check.ps1 -RequireHardwareContract
```

O relatorio padrao fica em `artifacts/observability/`, fora do Git.

## Controles para dependencias externas

- Exporter OTLP/Prometheus externo continua opcional, mas o repo agora possui
  `/metrics` Prometheus text sem dependencia adicional.
- Dashboards e alertas existem como JSON versionado independente de fornecedor.
- Health checks de processo/SQLite continuam separados do contrato fisico; o gate
  `-RequireHardwareContract` torna essa validacao obrigatoria quando houver
  decisao de release para equipamento real.
- O gate geral `tools/test-readiness-gates.ps1` executa a validacao offline de
  observabilidade por padrao e pode bloquear a coleta online de `/metrics` com
  `-RunObservabilityOnline -RequireObservabilityMetrics`.
- O modo `tools/test-readiness-gates.ps1 -ReleaseGate` torna essa validacao
  obrigatoria junto com smoke, cobertura, supply chain, container build, contrato
  fisico, FinOps/capacidade e scanners externos.
