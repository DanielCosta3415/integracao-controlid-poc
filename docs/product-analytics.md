# Product analytics privacy-aware

Escopo: medir valor, uso, funis e qualidade da PoC de integracao Control iD sem
coletar dados pessoais, dados sensiveis, payloads brutos, credenciais ou
identificadores de equipamento/usuario.

A instrumentacao atual usa somente metricas internas em memoria, expostas em
`GET /metrics` quando habilitado e autorizado. Nao ha ferramenta externa de
analytics, cookies de tracking, pixel, session replay ou envio para terceiros.

## Objetivos de produto

| Objetivo | Valor para usuario | Valor para negocio | Fluxos relacionados |
| --- | --- | --- | --- |
| Ativar uma bancada operacional | Operador entra na PoC, conecta equipamento e entende estado inicial | Reduz tempo de setup e suporte tecnico | Login local, dashboard, sessao, device setup |
| Explorar a Access API com seguranca | Time tecnico consulta contrato, payload e resposta sem chamar endpoint indevido | Acelera homologacao e reduz erro contra equipamento fisico | Catalogo oficial, invocacao, objetos oficiais |
| Validar fluxos criticos do equipamento | Operador confirma modos, hardware, callbacks e push em ambiente controlado | Aumenta confianca de integracao antes de producao | Operation modes, callbacks, push, hardware |
| Diagnosticar falhas com rastreabilidade | Time encontra erro por correlation ID, status, flow e evento | Reduz MTTR e retrabalho | Observabilidade, auditoria, historico |
| Governar privacidade e dados locais | DPO/operacao localiza categorias e reduz retencao | Reduz risco LGPD e operacional | Privacy report, purge, backup/restore |

Momentos de ativacao:

- Primeiro login local concluido.
- Primeiro login com equipamento concluido.
- Primeira invocacao oficial bem-sucedida.
- Primeiro callback recebido e persistido.
- Primeiro comando push enfileirado, entregue e concluido.
- Primeiro relatorio de privacidade ou readiness operacional validado.

Fluxos de abandono:

- Login local submetido com 4xx/5xx.
- Login de equipamento sem sessao valida.
- Invocacao oficial bloqueada, invalida, com timeout ou 5xx.
- Push enfileirado com payload invalido.
- Callback rejeitado por chave, assinatura, IP ou tamanho.
- `/health/ready` falhando enquanto telas de operacao recebem trafego.

## KPIs

| KPI | Pergunta de negocio | Fonte | Segmentacao permitida | Meta inicial |
| --- | --- | --- | --- | --- |
| Taxa de ativacao local | Usuarios conseguem acessar a PoC? | `local_login_submitted` com outcome `success` | `flow`, `event`, `outcome` | Definir apos baseline real |
| Taxa de login no equipamento | Operadores conseguem criar sessao Control iD? | `device_login_submitted` e metricas de auth/device | `flow`, `event`, `outcome` | Definir por ambiente |
| Uso do catalogo oficial | O catalogo e usado para exploracao tecnica? | `official_catalog_explored` | `action`, `outcome` | Crescimento esperado em homologacao |
| Conclusao de invocacao oficial | Chamadas oficiais concluem sem erro? | `official_endpoint_invoked`, `controlid_official_api_invocations_total` | `endpoint_id` operacional, `outcome` | Alta taxa de sucesso em bancada estavel |
| Adocao de callbacks/monitor | Eventos externos chegam e persistem? | `event_monitoring_used`, `controlid_callback_ingress_total` | `event_family`, `outcome` | Validar por tipo em bancada |
| Adocao do Push | Fila push entrega e recebe resultado? | `push_flow_used`, `controlid_push_operations_total` | `operation`, `outcome` | Validar ciclo completo |
| Tempo de conclusao por fluxo | Onde a experiencia esta lenta? | `controlid_product_flow_duration_milliseconds` | `flow`, `event`, `action` | Comparar P95 por fluxo |
| Erros por fluxo | Quais fluxos quebram mais? | `controlid_product_flow_events_total` com `outcome` != `success` | `flow`, `event`, `status_group` | Reduzir recorrencia |
| Uso de governanca de privacidade | Relatorios e checks LGPD estao sendo usados? | `privacy_report_used`, readiness operacional | `flow`, `event` | Obrigatorio antes de uso real |
| Saude operacional percebida | Saude do sistema acompanha experiencia? | produto + health/operational metrics | `flow`, `status_group`, health | Sem readiness fail em fluxo critico |

Retencao recomendada para agregados: curto prazo operacional em ambiente local
ou conforme politica aprovada em `ops.local.json`. Nao exportar series com
labels novas sem revisao de privacidade.

## Catalogo de eventos

Todos os eventos abaixo sao agregados, sem identificador de usuario, IP, e-mail,
device real, session, payload ou query string.

| Evento | Descricao | Fluxo | Quando dispara | Propriedades permitidas | Propriedades proibidas | Dados pessoais | Destino | Retencao |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `dashboard_viewed` | Uso do dashboard inicial | `activation` | `GET /` ou `GET /Home/*` | `flow`, `event`, `action`, `outcome`, `status_group`, duracao | user, IP, query, cookie | Nao | `/metrics` | Curta |
| `workspace_explored` | Navegacao pelo mapa funcional | `activation` | `Workspace/*` | agregadas | termos de busca livres com dado pessoal | Nao | `/metrics` | Curta |
| `local_login_viewed` | Tela de login local aberta | `activation` | `GET /Auth/LocalLogin` | agregadas | username, email, returnUrl bruto | Nao | `/metrics` | Curta |
| `local_login_submitted` | Login local submetido | `activation` | `POST /Auth/LocalLogin` | agregadas | username, email, senha, rememberMe individual | Nao | `/metrics` | Curta |
| `local_registration_viewed` | Registro local aberto | `activation` | `GET /Auth/Register` | agregadas | nome, email, telefone | Nao | `/metrics` | Curta |
| `local_registration_submitted` | Registro local submetido | `activation` | `POST /Auth/Register` | agregadas | nome, email, telefone, senha | Nao | `/metrics` | Curta |
| `device_login_viewed` | Tela de login do equipamento aberta | `device_session` | `GET /Auth/Login` | agregadas | device URL, username, senha | Nao | `/metrics` | Curta |
| `device_login_submitted` | Login Control iD submetido | `device_session` | `POST /Auth/Login` | agregadas | device URL, session, username, senha | Nao | `/metrics` | Curta |
| `logout_requested` | Logout local/equipamento solicitado | `device_session` | `Auth/Logout` ou `Auth/LocalLogout` | agregadas | usuario, cookie, session | Nao | `/metrics` | Curta |
| `auth_status_viewed` | Status de autenticacao consultado | `device_session` | `Auth/Status` | agregadas | usuario, session, cookie | Nao | `/metrics` | Curta |
| `credential_change_requested` | Alteracao de credencial local solicitada | `security` | `Auth/ChangePassword` | agregadas | senha atual, senha nova, usuario | Nao | `/metrics` | Curta |
| `device_session_managed` | Status/acoes de sessao | `device_session` | `Session/*` | agregadas | session string, device IP | Nao | `/metrics` | Curta |
| `device_registry_managed` | Cadastro/consulta de equipamento local | `device_setup` | `Devices/*` | agregadas | IP, serial, nome real | Nao | `/metrics` | Curta |
| `official_catalog_explored` | Catalogo oficial consultado | `official_api` | `OfficialApi/Index` | agregadas | filtros livres com dado pessoal | Nao | `/metrics` | Curta |
| `official_endpoint_invoked` | Endpoint oficial invocado pela UI | `official_api` | `OfficialApi/Invoke` | agregadas; detalhe por endpoint vem de metricas operacionais ja sanitizadas | body, query, session, device URL | Nao | `/metrics` | Curta |
| `official_objects_managed` | Objetos oficiais gerenciados | `official_objects` | `OfficialObjects/*` | agregadas | objeto/payload bruto se contiver dado pessoal | Nao | `/metrics` | Curta |
| `operation_modes_managed` | Modos Standalone/Pro/Enterprise usados | `operation_modes` | `OperationModes/*` | agregadas | server id real, licenca/senha | Nao | `/metrics` | Curta |
| `product_specific_flow_used` | Fluxos especificos por produto usados | `product_specific` | `ProductSpecific/*` | agregadas | payload, license key, host | Nao | `/metrics` | Curta |
| `advanced_official_flow_used` | Recursos oficiais avancados usados | `advanced_official` | `AdvancedOfficial/*` | agregadas | payload, imagem, arquivo | Nao | `/metrics` | Curta |
| `hardware_flow_used` | Acoes de hardware abertas/submetidas | `hardware` | `Hardware/*` | agregadas | device id real, parametro sensivel | Nao | `/metrics` | Curta |
| `system_flow_used` | Operacoes de sistema usadas | `system` | `System/*` | agregadas | rede, VPN, certificados, senha | Nao | `/metrics` | Curta |
| `identity_credential_flow_used` | Fluxos de usuario, grupo, cartao, QR, biometria e midia | `identity_credentials` | `Users`, `Cards`, `QRCodes`, `BiometricTemplates`, `Media`, `Logo`, `Groups`, `AccessRules` | agregadas | nome, registration, biometria, foto, cartao, QR, PIN | Nao | `/metrics` | Curta |
| `event_monitoring_used` | Telas/endpoints de monitoramento usados | `callbacks_monitoring` | `OfficialEvents`, `Monitor`, `MonitorWebhook`, `/api/*` | agregadas | payload bruto, user_id, device_id | Nao | `/metrics` | Curta |
| `push_flow_used` | Push Center, `/push` e `/result` usados | `push` | `PushCenter/*`, `/push`, `/result` | agregadas | command id, device id, user id, payload | Nao | `/metrics` | Curta |
| `privacy_report_used` | Relatorio LGPD acessado | `privacy_governance` | `Privacy/*` | agregadas | termo de busca, email, telefone, ID | Nao | `/metrics` | Curta |
| `audit_history_used` | Historicos de auditoria acessados | `audit_history` | `AccessLogs`, `ChangeLogs`, `Errors` | agregadas | actor, user, IP, stack, payload | Nao | `/metrics` | Curta |
| `documentation_explored` | Documentacao funcional interna usada | `documentation` | `DocumentedFeatures/*` | agregadas | N/A | Nao | `/metrics` | Curta |

## Instrumentacao aplicada

- `ProductAnalyticsEventClassifier` classifica apenas rotas em allowlist.
- `RequestLoggingMiddleware` registra evento de produto junto da duracao da
  requisicao, depois de conhecer o status HTTP final.
- `OperationalMetrics.RecordProductFlow` grava:
  - `controlid.product.flow.events`
  - `controlid.product.flow.duration`
- Prometheus exporta:
  - `controlid_product_flow_events_total`
  - `controlid_product_flow_duration_milliseconds_count`
  - `controlid_product_flow_duration_milliseconds_sum`
  - `controlid_product_flow_duration_milliseconds_max`

Propriedades permitidas:

- `flow`: categoria do fluxo, por exemplo `official_api`.
- `event`: nome do evento allowlist, por exemplo `official_endpoint_invoked`.
- `action`: `view` para GET e `submit` para demais metodos.
- `outcome`: `success`, `redirect`, `blocked_or_invalid`, `server_error` ou
  `unknown`.
- `status_group`: `2xx`, `3xx`, `4xx`, `5xx` ou `unknown`.
- duracao agregada em milissegundos.

Dados proibidos:

- Nome, e-mail, telefone, registration, documento, usuario bruto.
- IP, host, serial, device id, user id, command id ou session.
- Senha, shared key, HMAC, token, cookie, API key, certificado privado.
- Foto, biometria, template, cartao, QR code, PIN, payload bruto, query string,
  body, header de auth ou stack trace.

## Dashboards sugeridos

### Produto

- Eventos por `flow` e `event`.
- Top fluxos por uso.
- Tendencia de `success` vs `blocked_or_invalid`.
- Fluxos com maior duracao maxima/soma.

### Funil

- `local_login_viewed` -> `local_login_submitted`.
- `device_login_viewed` -> `device_login_submitted`.
- `official_catalog_explored` -> `official_endpoint_invoked`.
- `push_flow_used` com `action=submit` -> metricas de push delivered/completed.

### Erros por fluxo

- `controlid_product_flow_events_total{outcome!="success"}` por `flow`.
- Correlacionar com `controlid_http_requests_total{status_group="5xx"}`.
- Correlacionar `official_api` com timeouts/circuit breaker da Access API.

### Uso por funcionalidade

- `identity_credentials`, `operation_modes`, `hardware`, `push`,
  `callbacks_monitoring`, `privacy_governance`.
- Adocao por ambiente deve ser inferida pelo ambiente do coletor, nao por label
  livre dentro da app.

### Saude operacional ligada a experiencia

- Fluxos com `server_error` junto de `/health/ready` unhealthy.
- Latencia de `official_api` junto de `controlid_official_api_duration`.
- Rejeicoes de callback junto de `event_monitoring_used`.

## Riscos e controles de privacidade

| Risco | Severidade | Controle aplicado | Acompanhamento |
| --- | --- | --- | --- |
| Analytics virar rastreamento de usuario | Alta | Sem user id, IP, cookie, session ou query/body; apenas labels allowlist | Revisar qualquer label nova em PR. |
| Cardinalidade excessiva | Media | Classificador por rotas fixas e eventos finitos | `observability-check` e testes de contrato. |
| Dado pessoal em filtro de URL | Alta | Query string descartada antes de classificar | Testes com `email`, `user_id` e `session`. |
| Envio a terceiro sem DPA | Alta | Sem ferramenta externa de analytics | Decisao humana e DPO antes de exportar a terceiros. |
| Interpretar PoC como produto monetizado | Baixa | KPIs de receita/custo marcados como nao aplicaveis no estado atual | Reavaliar se houver modelo comercial. |

## Validacao

- Testes de `ProductAnalyticsEventClassifier` garantem mapeamento e descarte de
  identificadores/query.
- Testes de `OperationalMetrics` garantem export Prometheus sem termos
  sensiveis comuns.
- `tools/observability-check.ps1 -OfflineValidateOnly` valida dashboard versionado.
- `tools/test-readiness-gates.ps1` executa build, testes, format, scan de
  secrets, observabilidade e readiness operacional.
