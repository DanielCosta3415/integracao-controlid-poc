# Análise de produto com privacidade

> **Referência** · Público: produto, dados e privacidade · Responsável: Produto · Última validação: 2026-08-12.

Escopo: medir valor, uso, funis e qualidade da PoC de integração Control iD sem
coletar dados pessoais, dados sensíveis, payloads brutos, credenciais ou
identificadores de equipamento/usuário.

A instrumentação atual usa somente métricas internas em memória, expostas em
`GET /metrics` quando habilitado e autorizado. Não há ferramenta externa de
análise externa, cookies de rastreamento, pixel, reprodução de sessão ou envio para terceiros.

## Objetivos de produto

| Objetivo | Valor para usuário | Valor para negócio | Fluxos relacionados |
| --- | --- | --- | --- |
| Ativar uma bancada operacional | Operador entra na PoC, conecta equipamento e entende o estado inicial | Reduz tempo de configuração e suporte técnico | Login local, painel, sessão, configuração do dispositivo |
| Explorar a Access API com segurança | Time técnico consulta contrato, payload e resposta sem chamar endpoint indevido | Acelera homologação e reduz erro contra equipamento físico | Catálogo oficial, invocação, objetos oficiais |
| Validar fluxos críticos do equipamento | Operador confirma modos, hardware, callbacks e push em ambiente controlado | Aumenta confiança de integração antes de produção | Operation modes, callbacks, push, hardware |
| Diagnosticar falhas com rastreabilidade | Time encontra erro por correlation ID, status, flow e evento | Reduz MTTR e retrabalho | Observabilidade, auditoria, histórico |
| Governar privacidade e dados locais | DPO/operação localiza categorias e reduz retenção | Reduz risco LGPD e operacional | Privacy report, purge, backup/restore |

Momentos de ativação:

- Primeiro login local concluído.
- Primeiro login com equipamento concluído.
- Primeira invocação oficial bem-sucedida.
- Primeiro callback recebido e persistido.
- Primeiro comando push enfileirado, entregue e concluído.
- Primeiro relatório de privacidade ou readiness operacional validado.

Fluxos de abandono:

- Login local submetido com 4xx/5xx.
- Login de equipamento sem sessão valida.
- Invocação oficial bloqueada, inválida, com timeout ou 5xx.
- Push enfileirado com payload inválido.
- Callback rejeitado por chave, assinatura, IP ou tamanho.
- `/health/ready` falhando enquanto telas de operação recebem tráfego.

## KPIs

| KPI | Pergunta de negócio | Fonte | Segmentação permitida | Meta inicial |
| --- | --- | --- | --- | --- |
| Taxa de ativação local | Usuários conseguem acessar a PoC? | `local_login_submitted` com outcome `success` | `flow`, `event`, `outcome` | Definir após baseline real |
| Taxa de login no equipamento | Operadores conseguem criar sessão Control iD? | `device_login_submitted` e métricas de auth/device | `flow`, `event`, `outcome` | Definir por ambiente |
| Uso do catálogo oficial | O catálogo é usado para exploração técnica? | `official_catalog_explored` | `action`, `outcome` | Crescimento esperado em homologação |
| Conclusão de invocação oficial | Chamadas oficiais concluem sem erro? | `official_endpoint_invoked`, `controlid_official_api_invocations_total` | `endpoint_id` operacional, `outcome` | Alta taxa de sucesso em bancada estável |
| Adoção de callbacks/monitor | Eventos externos chegam e persistem? | `event_monitoring_used`, `controlid_callback_ingress_total` | `event_family`, `outcome` | Validar por tipo em bancada |
| Adoção do Push | Fila push entrega e recebe resultado? | `push_flow_used`, `controlid_push_operations_total` | `operation`, `outcome` | Validar ciclo completo |
| Tempo de conclusão por fluxo | Onde a experiência está lenta? | `controlid_product_flow_duration_milliseconds` | `flow`, `event`, `action` | Comparar P95 por fluxo |
| Erros por fluxo | Quais fluxos quebram mais? | `controlid_product_flow_events_total` com `outcome` != `success` | `flow`, `event`, `status_group` | Reduzir recorrência |
| Uso de governança de privacidade | Relatórios e checks LGPD estão sendo usados? | `privacy_report_used`, readiness operacional | `flow`, `event` | Obrigatório antes de uso real |
| Saúde operacional percebida | Saúde do sistema acompanha experiência? | produto + health/operational metrics | `flow`, `status_group`, health | Sem readiness fail em fluxo crítico |

Retenção recomendada para agregados: curto prazo operacional em ambiente local
ou conforme política aprovada em `ops.local.json`. Não exportar series com
labels novas sem revisão de privacidade.

## Catálogo de eventos

Todos os eventos abaixo são agregados, sem identificador de usuário, IP, e-mail,
device real, session, payload ou query string.

| Evento | Descrição | Fluxo | Quando dispara | Propriedades permitidas | Propriedades proibidas | Dados pessoais | Destino | Retenção |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `dashboard_viewed` | Uso do dashboard inicial | `activation` | `GET /` ou `GET /Home/*` | `flow`, `event`, `action`, `outcome`, `status_group`, duração | user, IP, query, cookie | Não | `/metrics` | Curta |
| `workspace_explored` | Navegação pelo mapa funcional | `activation` | `Workspace/*` | agregadas | termos de busca livres com dado pessoal | Não | `/metrics` | Curta |
| `local_login_viewed` | Tela de login local aberta | `activation` | `GET /Auth/LocalLogin` | agregadas | username, email, returnUrl bruto | Não | `/metrics` | Curta |
| `local_login_submitted` | Login local submetido | `activation` | `POST /Auth/LocalLogin` | agregadas | username, email, senha, rememberMe individual | Não | `/metrics` | Curta |
| `local_registration_viewed` | Registro local aberto | `activation` | `GET /Auth/Register` | agregadas | nome, email, telefone | Não | `/metrics` | Curta |
| `local_registration_submitted` | Registro local submetido | `activation` | `POST /Auth/Register` | agregadas | nome, email, telefone, senha | Não | `/metrics` | Curta |
| `device_login_viewed` | Tela de login do equipamento aberta | `device_session` | `GET /Auth/Login` | agregadas | device URL, username, senha | Não | `/metrics` | Curta |
| `device_login_submitted` | Login Control iD submetido | `device_session` | `POST /Auth/Login` | agregadas | device URL, session, username, senha | Não | `/metrics` | Curta |
| `logout_requested` | Logout local/equipamento solicitado | `device_session` | `Auth/Logout` ou `Auth/LocalLogout` | agregadas | usuário, cookie, session | Não | `/metrics` | Curta |
| `auth_status_viewed` | Status de autenticação consultado | `device_session` | `Auth/Status` | agregadas | usuário, session, cookie | Não | `/metrics` | Curta |
| `credential_change_requested` | Alteração de credencial local solicitada | `security` | `Auth/ChangePassword` | agregadas | senha atual, senha nova, usuário | Não | `/metrics` | Curta |
| `device_session_managed` | Status/ações de sessão | `device_session` | `Session/*` | agregadas | session string, device IP | Não | `/metrics` | Curta |
| `device_registry_managed` | Cadastro/consulta de equipamento local | `device_setup` | `Devices/*` | agregadas | IP, serial, nome real | Não | `/metrics` | Curta |
| `official_catalog_explored` | Catálogo oficial consultado | `official_api` | `OfficialApi/Index` | agregadas | filtros livres com dado pessoal | Não | `/metrics` | Curta |
| `official_endpoint_invoked` | Endpoint oficial invocado pela UI | `official_api` | `OfficialApi/Invoke` | agregadas; detalhe por endpoint vem de métricas operacionais já sanitizadas | body, query, session, device URL | Não | `/metrics` | Curta |
| `official_objects_managed` | Objetos oficiais gerenciados | `official_objects` | `OfficialObjects/*` | agregadas | objeto/payload bruto se contiver dado pessoal | Não | `/metrics` | Curta |
| `operation_modes_managed` | Modos Standalone/Pro/Enterprise usados | `operation_modes` | `OperationModes/*` | agregadas | server id real, licença/senha | Não | `/metrics` | Curta |
| `product_specific_flow_used` | Fluxos específicos por produto usados | `product_specific` | `ProductSpecific/*` | agregadas | payload, license key, host | Não | `/metrics` | Curta |
| `advanced_official_flow_used` | Recursos oficiais avançados usados | `advanced_official` | `AdvancedOfficial/*` | agregadas | payload, imagem, arquivo | Não | `/metrics` | Curta |
| `hardware_flow_used` | Ações de hardware abertas/submetidas | `hardware` | `Hardware/*` | agregadas | device id real, parâmetro sensível | Não | `/metrics` | Curta |
| `system_flow_used` | Operações de sistema usadas | `system` | `System/*` | agregadas | rede, VPN, certificados, senha | Não | `/metrics` | Curta |
| `identity_credential_flow_used` | Fluxos de usuário, grupo, cartão, QR, biometria e mídia | `identity_credentials` | `Users`, `Cards`, `QRCodes`, `BiometricTemplates`, `Media`, `Logo`, `Groups`, `AccessRules` | agregadas | nome, registration, biometria, foto, cartão, QR, PIN | Não | `/metrics` | Curta |
| `event_monitoring_used` | Telas/endpoints de monitoramento usados | `callbacks_monitoring` | `OfficialEvents`, `Monitor`, `MonitorWebhook`, `/api/*` | agregadas | payload bruto, user_id, device_id | Não | `/metrics` | Curta |
| `push_flow_used` | Push Center, `/push` e `/result` usados | `push` | `PushCenter/*`, `/push`, `/result` | agregadas | command id, device id, user id, payload | Não | `/metrics` | Curta |
| `privacy_report_used` | Relatório LGPD acessado | `privacy_governance` | `Privacy/*` | agregadas | termo de busca, email, telefone, ID | Não | `/metrics` | Curta |
| `audit_history_used` | Históricos de auditoria acessados | `audit_history` | `AccessLogs`, `ChangeLogs`, `Errors` | agregadas | actor, user, IP, stack, payload | Não | `/metrics` | Curta |
| `documentation_explored` | Documentação funcional interna usada | `documentation` | `DocumentedFeatures/*` | agregadas | N/A | Não | `/metrics` | Curta |

## Instrumentação aplicada

- `ProductAnalyticsEventClassifier` classifica apenas rotas em allowlist.
- `RequestLoggingMiddleware` registra evento de produto junto da duração da
  requisição, depois de conhecer o status HTTP final.
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
- `action`: `view` para GET e `submit` para demais métodos.
- `outcome`: `success`, `redirect`, `blocked_or_invalid`, `server_error` ou
  `unknown`.
- `status_group`: `2xx`, `3xx`, `4xx`, `5xx` ou `unknown`.
- duração agregada em milissegundos.

Dados proibidos:

- Nome, e-mail, telefone, registration, documento, usuário bruto.
- IP, host, serial, device id, user id, command id ou session.
- Senha, shared key, HMAC, token, cookie, API key, certificado privado.
- Foto, biometria, template, cartão, QR code, PIN, payload bruto, query string,
  body, header de auth ou stack trace.

## Painéis sugeridos

### Produto

- Eventos por `flow` e `event`.
- Top fluxos por uso.
- Tendência de `success` vs `blocked_or_invalid`.
- Fluxos com maior duração máxima ou acumulada.

### Funil

- `local_login_viewed` -> `local_login_submitted`.
- `device_login_viewed` -> `device_login_submitted`.
- `official_catalog_explored` -> `official_endpoint_invoked`.
- `push_flow_used` com `action=submit` -> métricas de push delivered/completed.

### Erros por fluxo

- `controlid_product_flow_events_total{outcome!="success"}` por `flow`.
- Correlacionar com `controlid_http_requests_total{status_group="5xx"}`.
- Correlacionar `official_api` com timeouts/circuit breaker da Access API.

### Uso por funcionalidade

- `identity_credentials`, `operation_modes`, `hardware`, `push`,
  `callbacks_monitoring`, `privacy_governance`.
- Adoção por ambiente deve ser inferida pelo ambiente do coletor, não por label
  livre dentro da app.

### Saúde operacional ligada à experiência

- Fluxos com `server_error` junto de `/health/ready` unhealthy.
- Latência de `official_api` junto de `controlid_official_api_duration`.
- Rejeições de callback junto de `event_monitoring_used`.

## Riscos e controles de privacidade

| Risco | Severidade | Controle aplicado | Acompanhamento |
| --- | --- | --- | --- |
| Analytics virar rastreamento de usuário | Alta | Sem user id, IP, cookie, session ou query/body; apenas labels allowlist | Revisar qualquer label nova em PR. |
| Cardinalidade excessiva | Média | Classificador por rotas fixas e eventos finitos | `observability-check` e testes de contrato. |
| Dado pessoal em filtro de URL | Alta | Query string descartada antes de classificar | Testes com `email`, `user_id` e `session`. |
| Envio a terceiro sem DPA | Alta | Sem ferramenta externa de analytics | Decisão humana e DPO antes de exportar a terceiros. |
| Interpretar PoC como produto monetizado | Baixa | KPIs de receita/custo marcados como não aplicáveis no estado atual | Reavaliar se houver modelo comercial. |

## Validação

- Testes de `ProductAnalyticsEventClassifier` garantem mapeamento e descarte de
  identificadores/query.
- Testes de `OperationalMetrics` garantem export Prometheus sem termos
  sensíveis comuns.
- `tools/observability-check.ps1 -OfflineValidateOnly` valida o painel versionado.
- `tools/test-readiness-gates.ps1` executa build, testes, format, scan de
  secrets, observabilidade e readiness operacional.

## Fórmulas, qualidade e propriedade

| Métrica | Fórmula | Janela inicial | Responsável | Regra de qualidade |
| --- | --- | --- | --- | --- |
| Ativação local | logins locais com sucesso / tentativas válidas | 7 dias | Produto/QA | Excluir health checks e automação identificada |
| Login no equipamento | sessões criadas / tentativas de login | 7 dias por ambiente | Integração | Separar timeout, rejeição e circuito aberto |
| Conclusão oficial | invocações `success` / invocações iniciadas | 24 h e 7 dias | Integração | Agrupar somente por endpoint allowlist |
| Conclusão Push | comandos concluídos / comandos enfileirados | 24 h e 7 dias | Operação | Deduplicar pela chave idempotente |
| Erro por fluxo | eventos com resultado diferente de sucesso / total | 24 h | Produto/SRE | Não usar identificador de pessoa ou equipamento |
| P95 de fluxo | percentil 95 da duração agregada | 24 h | SRE | Exigir amostra mínima antes de comparar |

Metas permanecem “a definir” até existir linha de base representativa. O responsável registra
período, ambiente, tamanho da amostra, alterações de instrumentação e decisão;
não use métricas de bancada como promessa de produção.

Exemplo de consulta Prometheus sem dimensão pessoal:

```promql
sum(rate(controlid_product_flow_events_total{outcome!="success"}[15m]))
/
sum(rate(controlid_product_flow_events_total[15m]))
```

## Contrato de qualidade dos indicadores

| Verificação | Regra |
| --- | --- |
| Completude | Eventos de sucesso e falha do mesmo fluxo usam o mesmo denominador documentado |
| Cardinalidade | Rótulos pertencem a lista finita; não usar usuário, IP, URL, consulta ou dispositivo real |
| Atualidade | Painel informa janela e atraso da fonte |
| Comparabilidade | Mudança de nome, fórmula ou instrumento cria nova linha de base |
| Privacidade | Amostra de métricas não contém dado pessoal nem segredo |

Meta sem fonte, janela e responsável permanece hipótese. O produto não deve
otimizar conversão às custas de segurança, consentimento ou minimização.

## Navegação documental

- [Voltar ao índice deste domínio](README.md).
- [Abrir a central de documentação](../README.md).
