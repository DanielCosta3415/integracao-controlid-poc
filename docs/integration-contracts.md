# Inventario de integracoes e contratos

Este documento registra os contratos de integracao da PoC sem criar endpoints novos ou alterar contratos publicos. Quando um schema vem de payload livre da Control iD ou da UI tecnica, ele e marcado como inferido.

## Sumario executivo

| Integracao | Tipo | Direcao | Ambiente | Status |
| --- | --- | --- | --- | --- |
| Access API Control iD | API externa HTTP | PoC -> equipamento | Local/laboratorio/equipamento real | Implementada |
| Catalogo oficial da PoC | API interna/UI tecnica | Browser -> PoC | Local/laboratorio | Implementada |
| Callbacks oficiais | Webhook HTTP | equipamento -> PoC | URL acessivel ao equipamento | Implementada |
| Monitor | Webhook HTTP | equipamento -> PoC | URL acessivel ao equipamento | Implementada |
| Push oficial | Fila/polling HTTP | equipamento <-> PoC | URL acessivel ao equipamento | Implementada |
| Push legado | Webhook HTTP legado | equipamento/simulador -> PoC | Local/laboratorio | Implementada |
| SQLite local | Banco de dados | PoC -> arquivo local | Workspace/local | Implementada |
| Sessao ASP.NET + sessao Control iD | Autenticacao/estado | Browser/PoC/equipamento | Local/laboratorio | Implementada |
| Serilog | Observabilidade | PoC -> console/arquivo | Local/laboratorio | Implementada |
| Smoke/stub local | Teste/terceiro simulado | PoC <-> stub | Local | Implementada |
| Swagger/OpenAPI | Documentacao automatica | Browser/cliente -> PoC | Development/configurado | Implementada |
| Cache | Cache de aplicacao | N/A | N/A | Nao aplicavel |
| Mensageria externa | Queue/broker | N/A | N/A | Nao aplicavel |
| Pagamentos | Terceiro | N/A | N/A | Nao aplicavel |
| E-mail/SMS/analytics | Terceiro | N/A | N/A | Nao aplicavel |

## Configuracao e variaveis

Nao ha loader `.env` configurado. Use `appsettings.json`, User Secrets ou variaveis de ambiente ASP.NET Core no formato `Secao__Chave`.

| Chave | Finalidade | Sensivel | Observacao |
| --- | --- | --- | --- |
| `ConnectionStrings__DefaultConnection` | Caminho SQLite local | Nao, salvo caminho sensivel | Default: `integracao_controlid.db` |
| `ControlIDApi__DefaultDeviceUrl` | URL padrao do equipamento | Sim em rede privada | Nao versionar valores reais |
| `ControlIDApi__DefaultUsername` | Usuario sugerido | Sim | Nao versionar |
| `ControlIDApi__DefaultPassword` | Senha sugerida | Sim | Nao versionar |
| `ControlIDApi__ConnectionTimeoutSeconds` | Timeout outbound para Access API | Nao | Normalizado entre 5 e 300 segundos |
| `ControlIDApi__CircuitBreaker__Enabled` | Protecao contra falhas transitorias repetidas | Nao | Default: true |
| `ControlIDApi__CircuitBreaker__FailureThreshold` | Falhas consecutivas para abrir circuito | Nao | Default: 5 |
| `ControlIDApi__CircuitBreaker__BreakDurationSeconds` | Duracao do circuito aberto | Nao | Default: 30 |
| `OpenApi__Enabled` | Habilita Swagger fora de Development | Nao | Default: false; Development habilita automaticamente |
| `CallbackSecurity__MaxBodyBytes` | Limite de body em callbacks/push | Nao | Default: 1048576 |
| `CallbackSecurity__RequireSharedKey` | Exige shared key nos ingressos | Nao | Obrigatorio fora de Development |
| `CallbackSecurity__SharedKeyHeaderName` | Header da chave compartilhada | Nao | Default: `X-ControlID-Callback-Key` |
| `CallbackSecurity__SharedKey` | Segredo do ingresso | Sim | Obrigatorio fora de Development |
| `CallbackSecurity__AllowedRemoteIps__N` | IPs autorizados | Pode ser sensivel | Opcional; vazio aceita qualquer IP |
| `CallbackSecurity__AllowLoopback` | Permite loopback com lista de IP | Nao | Facilita stub/smoke local |
| `CallbackSecurity__RateLimit__PermitLimit` | Limite por janela para ingressos callback/push | Nao | Default: 120 |
| `CallbackSecurity__RateLimit__WindowSeconds` | Janela do rate limit de ingressos | Nao | Default: 60 |
| `Session__IdleTimeout` | Timeout de sessao ASP.NET | Nao | Default: 30 minutos |
| `Session__CookieName` | Nome do cookie de sessao | Nao | Default: `.IntegracaoControlID.Session` |
| `AllowedHosts` | Hosts aceitos pelo ASP.NET Core | Nao | Nao pode ser `*` fora de Development |

## Contratos mapeados

### INT-001 - Access API Control iD

- Tipo: API externa HTTP.
- Finalidade: conectar, autenticar, consultar e alterar estado/configuracao/objetos do equipamento.
- Ponto de chamada: `OfficialApiInvokerService` via `OfficialControlIdApiService`.
- Endpoints: catalogados em `OfficialApiCatalogService` como paths `.fcgi`, por exemplo `/login.fcgi`, `/load_objects.fcgi`, `/set_configuration.fcgi`, `/reboot.fcgi`.
- Metodo: definido por endpoint (`GET` ou `POST`).
- Headers: `Content-Type` conforme `OfficialApiEndpointDefinition.ContentType`; session vai na query real `session=...` quando requerida, mas URLs exibidas em tela/logs devem mascarar esse valor.
- Autenticacao: login oficial retorna `session`; endpoints com `RequiresSession=true` exigem sessao ativa.
- Request: JSON, multipart, binario/base64 ou vazio, conforme `BodyKind`.
- Response: texto/JSON ou binario preservado em Base64 quando Content-Type nao parece texto/json/xml.
- DTO/schema: `OfficialApiEndpointDefinition`, `OfficialApiInvocationResult`; schemas de payload sao inferidos do catalogo e docs oficiais.
- Status codes: propagados do equipamento em `OfficialApiInvocationResult.StatusCode`.
- Erros esperados: endpoint ausente no catalogo, device address invalido, sessao ausente, timeout, HTTP nao 2xx, JSON inesperado.
- Timeout: `ControlIDApi:ConnectionTimeoutSeconds`, normalizado entre 5 e 300 segundos.
- Retry/backoff: nao existe; seguro porque muitas operacoes oficiais nao sao idempotentes.
- Idempotencia: depende do endpoint externo; `load/get` tendem a ser seguros, `create/modify/destroy/reboot/reset` nao devem ser repetidos automaticamente.
- Rate limit: nao implementado.
- Circuit breaker/fallback: `OfficialApiCircuitBreaker` abre circuito por endpoint/equipamento apos falhas transitorias repetidas (`408`, `429`, `5xx`, timeout ou falha inesperada).
- Logs: endpoint id, metodo, path, target sem query/session, status e duracao.
- Dados sensiveis: credenciais, session, fotos, biometria, cartoes, QR, payloads de usuarios.

### INT-002 - Catalogo oficial da PoC

- Tipo: API interna/UI tecnica MVC.
- Finalidade: expor catalogo de endpoints oficiais, exemplos e invocacao assistida.
- Ponto de chamada: `OfficialApiController`, `OfficialApiCatalogService`, `OfficialApiContractDocumentationService`.
- Metodo: MVC `GET` para catalogo/detalhe e `POST` para invocacao.
- Headers: cookie de sessao ASP.NET e antiforgery em formularios.
- Autenticacao/autorizacao: sessao da PoC e RBAC por papel; invocacao assistida exige perfil autorizado conforme controller.
- Request: ViewModels de `ViewModels/OfficialApi/*`.
- Response: views Razor com resposta oficial formatada.
- Status codes: MVC padrao; erros aparecem em tela.
- Erros esperados: endpoint invocavel falso, JSON invalido, sessao ausente, falha oficial.
- Timeout/retry: delegados ao INT-001; sem retry automatico.
- Idempotencia: nao garantida para endpoints oficiais.
- Logs: via invoker e controllers relacionados.
- Dados sensiveis: payloads e respostas oficiais podem conter dados pessoais; nao usar exemplos reais.

### INT-003 - Callbacks oficiais

- Tipo: Webhook HTTP.
- Finalidade: receber eventos oficiais de identificacao online e cadastros remotos.
- Ponto de chamada: `OfficialCallbacksController`.
- Endpoints: `/new_biometric_image.fcgi`, `/new_biometric_template.fcgi`, `/new_card.fcgi`, `/new_qrcode.fcgi`, `/new_uhf_tag.fcgi`, `/new_user_id_and_password.fcgi`, `/new_user_identified.fcgi`, `/new_rex_log.fcgi`, `/device_is_alive.fcgi`, `/card_create.fcgi`, `/fingerprint_create.fcgi`, `/template_create.fcgi`, `/face_create.fcgi`, `/pin_create.fcgi`, `/password_create.fcgi`.
- Metodo: `POST`.
- Headers: `X-ControlID-Callback-Key` quando `RequireSharedKey=true`.
- Autenticacao/autorizacao: `CallbackSecurityEvaluator` por shared key, IP permitido e limite de body.
- Request: body textual, JSON, form, imagem ou octet-stream; schema oficial/inferido por endpoint.
- Response: eventos de identificacao retornam `{ "result": { "event": 14 } }`; eventos reconhecidos retornam `200 OK` sem payload.
- DTO/schema: persistencia em `MonitorEventLocal`; leitura por `CallbackRequestBodyReader`.
- Status codes: `200`, `401`, `403`, `413`, `500`.
- Erros esperados: shared key ausente/invalida, IP bloqueado, payload acima do limite, falha SQLite.
- Timeout: nao ha timeout proprio; leitura aceita cancellation token do ASP.NET Core.
- Retry/backoff: nao implementado na PoC; retry deve ser decidido pelo equipamento/origem.
- Idempotencia: nao ha chave idempotente; cada callback aceito gera novo `EventId`.
- Rate limit: policy `CallbackIngress`, particionada por IP remoto.
- Circuit breaker/fallback: nao implementado.
- Logs: aceite/rejeicao com path, event id, familia e device id quando seguro.
- Dados sensiveis: imagens, templates, identificadores de usuario, eventos de acesso.

### INT-004 - Monitor

- Tipo: Webhook HTTP.
- Finalidade: receber notificacoes de topicos Monitor.
- Ponto de chamada: `OfficialCallbacksController.ReceiveMonitorNotification`.
- Endpoint: `POST /api/notifications/{topic}`.
- Headers/autenticacao: iguais aos callbacks oficiais.
- Request: JSON ou payload bruto de monitor; schema inferido por topico.
- Response: `200 OK` sem payload em sucesso.
- DTO/schema: `MonitorEventLocal`, `WebhookEventViewModel`.
- Status codes/erros/timeout/retry/idempotencia/logs/dados: iguais ao INT-003.
- Topicos documentados: `user_image`, `template`, `card`, `operation_mode`, `pin`, `password`, `catra_event`, `usb_drive`.

### INT-005 - Push oficial

- Tipo: fila persistida com polling HTTP.
- Finalidade: equipamento busca comandos pendentes e devolve resultado.
- Pontos de chamada: `PushCenterController.Poll`, `PushCenterController.Result`, `PushCommandWorkflowService`, `PushCommandRepository`.
- Endpoints:
  - `GET /push?device_id=<id>` ou `GET /push?deviceid=<id>`.
  - `POST /result?command_id=<guid>&status=<status>&device_id=<id>&user_id=<id>`.
- Headers: `X-ControlID-Callback-Key` quando `RequireSharedKey=true`; `Content-Type: application/json` recomendado no resultado.
- Autenticacao/autorizacao: `CallbackSecurityEvaluator`.
- Request:
  - `/push`: query opcional de dispositivo.
  - `/result`: body bruto do resultado; query opcional `command_id`, `status`, `device_id`, `user_id`.
- Response:
  - `/push` com comando: payload JSON enfileirado.
  - `/push` sem comando: `{}`.
  - `/result`: `200 OK` sem payload.
- DTO/schema: `PushCommandLocal`, `PushQueueCommandViewModel`, `PushEventViewModel`; payload do comando e resultado e inferido/livre.
- Status codes: `200`, `401`, `403`, `413`, `500`.
- Erros esperados: shared key ausente/invalida, IP bloqueado, payload acima do limite, falha de persistencia.
- Timeout: leitura de body limitada por request/cancellation token; limite por `CallbackSecurity:MaxBodyBytes`.
- Retry/backoff: nao implementado; retry de `/result` pode criar ou atualizar registro conforme `command_id`.
- Idempotencia:
  - `/push` nao e idempotente: muda `pending` para `delivered`.
  - `/result` com `command_id` e idempotente por sobrescrita do mesmo registro.
  - `/result` sem `command_id` aceita `Idempotency-Key` ou `idempotency_key` e atualiza o mesmo registro derivado da chave.
  - `/result` sem `command_id` e sem chave idempotente cria registro novo.
- Rate limit: policy `CallbackIngress`, particionada por IP remoto.
- Circuit breaker: nao aplicavel a polling ingress.
- Logs: command id, device id, status e bytes; payload bruto nao deve ser logado.
- Dados sensiveis: payloads podem conter dados pessoais/operacionais.

### INT-006 - Push legado

- Tipo: webhook HTTP legado.
- Finalidade: manter compatibilidade com `POST /Push/Receive`.
- Endpoint: `POST /Push/Receive`.
- Headers/autenticacao: iguais ao Push oficial.
- Request: body bruto; se JSON, campos inferidos `command_type`, `type`, `event`, `status`, `device_id`, `deviceid`, `user_id`, `userid`, `payload`, `data`.
- Response: `{ "status": "received", "eventId": "<guid>" }`.
- DTO/schema: `PushCommandLocal`.
- Status codes: `200`, `401`, `403`, `413`, `500`.
- Timeout/retry/backoff/idempotencia: sem retry; aceita `Idempotency-Key` ou `idempotency_key` para atualizar o mesmo evento legado, mas cada aceite sem chave cria registro.
- Logs: body truncado em ate 500 caracteres no legado; nao use payload real sensivel em ambiente exposto.
- Dados sensiveis: payload bruto.

### INT-007 - SQLite local

- Tipo: banco de dados local.
- Finalidade: persistir estado da PoC, eventos, push, usuarios locais e artefatos.
- Ponto de chamada: `IntegracaoControlIDContext`, repositories em `Services/Database`.
- Schema: EF Core migrations em `Data/Migrations`; compatibilidade idempotente em `Program.cs`.
- Autenticacao: arquivo local; sem usuario/senha.
- Timeout/retry/backoff: defaults do SQLite/EF Core; sem retry customizado.
- Idempotencia: depende do repository; inserts geram novos ids, updates por chave.
- Logs: repositories registram falhas.
- Dados sensiveis: usuarios, fotos, biometria, cartoes, QR, callbacks e push.
- Ambiente: local/workspace; arquivos `integracao_controlid.db*` nao devem ser versionados.

### INT-008 - Sessao ASP.NET e sessao Control iD

- Tipo: autenticacao/estado.
- Finalidade: guardar device address e session string oficial para chamadas autenticadas.
- Chaves: `ControlID_DeviceAddress`, `ControlID_SessionString`.
- Cookie: `Session:CookieName`, HttpOnly, SameSite Strict, Secure Always fora de Development.
- Request/response: MVC com antiforgery nos POSTs.
- Timeout: `Session:IdleTimeout`.
- Retry/backoff: nao aplicavel.
- Idempotencia: logout/clear sao tolerantes a ausencia de sessao.
- Dados sensiveis: session string oficial; nao logar.
- Controle: auth local global com RBAC por papel; session string oficial deve aparecer apenas mascarada em URLs de diagnostico.

### INT-009 - Observabilidade Serilog

- Tipo: logs.
- Destino: console e `Logs/app_log.txt`.
- Payload: mensagens estruturadas com endpoint, device id, command id, status, duracao e excecoes.
- Dados sensiveis: nao logar credenciais, shared key, biometria bruta ou payload integral.
- Retencao: configurada por `Logging__File__RetainedFileCountLimit`/Serilog.

### INT-010 - OpenAPI/Swagger local

- Tipo: documentacao automatica HTTP.
- Finalidade: expor especificacao e UI tecnica dos contratos HTTP locais da PoC.
- Endpoint: `/swagger/v1/swagger.json` e `/swagger`.
- Ambiente: habilitado automaticamente em `Development`; fora de Development exige `OpenApi:Enabled=true`.
- Autenticacao/autorizacao: nao adiciona autenticacao propria; nao habilite fora de rede controlada sem protecao externa.
- DTO/schema: gerado pelo Swashbuckle a partir dos controllers MVC e metadados ASP.NET Core.
- Dados sensiveis: exemplos reais nao devem ser colocados em atributos, docs ou responses.

### INT-011 - Check opt-in de contrato com equipamento real

- Tipo: script de validacao externa.
- Ponto de chamada: `tools/contract-controlid-device.ps1`.
- Finalidade: validar `login.fcgi`, `session_is_valid.fcgi` e `system_information.fcgi` contra equipamento real sem versionar credenciais.
- Ambiente: local/laboratorio; exige `CONTROLID_DEVICE_URL`, `CONTROLID_USERNAME` e `CONTROLID_PASSWORD`.
- Persistencia: gera relatorio em `artifacts/reports/controlid-device-contract-latest.md` por padrao, fora do Git, omitindo host real, credenciais e session.
- Restricao: nao roda na CI porque depende de equipamento fisico e credenciais reais.

## Exemplos de payloads

### Access API - login valido

Request:

```http
POST /login.fcgi HTTP/1.1
Content-Type: application/json

{
  "login": "<usuario>",
  "password": "<senha>"
}
```

Response de sucesso inferido:

```json
{
  "session": "<session>"
}
```

Erro esperado: credencial invalida ou resposta sem `session`; a PoC nao cria sessao local.

### Callback com shared key ausente

Request invalido:

```http
POST /device_is_alive.fcgi HTTP/1.1
Content-Type: application/json

{}
```

Response quando `RequireSharedKey=true`:

```http
401 Unauthorized
Callback shared key is missing.
```

### Monitor valido

Request valido:

```http
POST /api/notifications/operation_mode?device_id=123 HTTP/1.1
X-ControlID-Callback-Key: <segredo>
Content-Type: application/json

{
  "online": 1,
  "local_identification": 0
}
```

Response:

```http
200 OK
```

Persistencia esperada: `EventType = "monitor:operation_mode:/api/notifications/operation_mode"`, `Status = "received"`.

### Push - comando disponivel

Request:

```http
GET /push?device_id=device-1 HTTP/1.1
X-ControlID-Callback-Key: <segredo>
```

Response:

```json
{
  "actions": []
}
```

Efeito: comando muda de `pending` para `delivered`.

### Push - fila vazia

```http
HTTP/1.1 200 OK
Content-Type: application/json

{}
```

### Push - resultado valido

Request:

```http
POST /result?command_id=00000000-0000-0000-0000-000000000001&status=completed&device_id=device-1 HTTP/1.1
X-ControlID-Callback-Key: <segredo>
Content-Type: application/json

{
  "ok": true
}
```

Response:

```http
200 OK
```

### Payload excessivo

Quando o body ultrapassa `CallbackSecurity:MaxBodyBytes`, mesmo sem `Content-Length`, callbacks e Push retornam:

```http
413 Payload Too Large
```

### Falha de rede/timeout outbound

Quando o equipamento nao responde dentro de `ControlIDApi:ConnectionTimeoutSeconds`, a PoC retorna mensagem funcional segura:

```text
Tempo limite excedido ao comunicar com o equipamento.
```

### Resposta inesperada

Quando um fluxo espera JSON estruturado e o equipamento retorna corpo nao parseavel, a PoC mantem o resultado bruto e registra warning; o fluxo consumidor deve tratar `JsonDocument` nulo.

## Riscos mitigados nesta revisao

| Risco | Mitigacao |
| --- | --- |
| Contratos de integracao espalhados | Documento unico de inventario e exemplos |
| Payload Push sem `Content-Length` podendo exceder limite antes da persistencia | `CallbackRequestBodyReader` agora limita leitura de `/result` e `/Push/Receive` |
| DTO/status Push implicitos | `PushCommandWorkflowService` e `PushCommandStatuses` centralizam estados e workflow |
| OpenAPI presumido | Swagger/OpenAPI habilitado em Development ou via `OpenApi:Enabled=true` |
| Sem circuit breaker outbound | `OfficialApiCircuitBreaker` protege endpoint/equipamento apos falhas transitorias repetidas |
| Sem idempotency key para Push sem `command_id` e legado | `Idempotency-Key`/`idempotency_key` geram chave deterministica e atualizam o mesmo registro |
| Sem secret scanner dedicado | `tools/scan-secrets.ps1` roda localmente e na CI |
| Sem check contra equipamento real | `tools/contract-controlid-device.ps1` valida contrato real de forma opt-in, sem credenciais versionadas |
| Sem rate limit para ingressos | Policy `CallbackIngress` limita callbacks e push por IP remoto |
| Ingressos sem autenticidade criptografica | `CallbackSignatureValidator` exige HMAC/timestamp/nonce quando configurado e `ControlIdCallbackSigningProxy` assina equipamentos sem HMAC nativo |
| UI sem autorizacao por perfil | Cookie auth global e RBAC por papel protegem operacoes administrativas e dados sensiveis |

## Riscos controlados e limites externos

| Item | Prioridade | Controle |
| --- | --- | --- |
| `/push` altera estado por natureza do contrato de polling | Alta | Mantido sem retry automatico para evitar replay de operacoes fisicas; resultados usam idempotency key quando o equipamento envia ou quando a PoC deriva chave segura |
| Operacoes oficiais outbound podem nao ser idempotentes | Media | Sem retry automatico generico; timeout e circuit breaker reduzem repeticao perigosa e falhas em cascata |
| Contratos oficiais dependem de firmware/modelo/licenca | Alta | Check opt-in real existe em `tools/contract-controlid-device.ps1`; validacao final exige equipamento fisico e credenciais fora do Git |
