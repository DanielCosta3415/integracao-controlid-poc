# Monitor: implementacao na PoC

Este documento explica como a funcionalidade Monitor foi implementada dentro da PoC de integracao com a API de controle de acesso da Control iD.

Na PoC, "Monitor" representa a trilha de recebimento, persistencia e visualizacao de callbacks/notificacoes enviados pelo equipamento para a aplicacao. Ele e diferente do Push: no Monitor, o equipamento envia eventos para a PoC; no Push, o equipamento consulta a PoC para buscar comandos pendentes.

## Visao geral

O Monitor foi implementado como um pipeline de entrada HTTP:

```text
Equipamento Control iD
  -> endpoint da PoC
  -> validacao de seguranca
  -> leitura do corpo da requisicao
  -> persistencia em SQLite
  -> tela de eventos oficiais
  -> uso por outras telas, como Modos de operacao
```

A tela operacional principal e `OfficialEvents`. As rotas legadas de `MonitorWebhook` redirecionam para essa tela consolidada.

## Arquivos envolvidos

| Arquivo | Papel na funcionalidade |
| --- | --- |
| `Controllers/OfficialCallbacksController.cs` | Expoe endpoints oficiais de callback e monitor recebidos pelo equipamento. |
| `Controllers/OfficialEventsController.cs` | Lista, detalha e limpa eventos oficiais persistidos. |
| `Controllers/MonitorWebhookController.cs` | Mantem uma entrada legada para webhook e redireciona consultas para `OfficialEvents`. |
| `Services/Callbacks/CallbackIngressService.cs` | Orquestra validacao, leitura e persistencia dos callbacks recebidos. |
| `Services/Callbacks/CallbackSecurityEvaluator.cs` | Aplica regras de seguranca para callbacks: tamanho, IP permitido e chave compartilhada. |
| `Services/Callbacks/CallbackRequestBodyReader.cs` | Le o corpo da requisicao e converte payload binario/imagem para Base64. |
| `Options/CallbackSecurityOptions.cs` | Define as configuracoes de seguranca usadas pelo pipeline de callback. |
| `Models/Database/MonitorEventLocal.cs` | Entidade local persistida na tabela `MonitorEvents`. |
| `Services/Database/MonitorEventRepository.cs` | Repositorio de persistencia e consulta dos eventos monitorados. |
| `ViewModels/Monitor/*` | Modelos usados para listagem e detalhe dos eventos. |
| `Views/OfficialEvents/*` | Interface principal para consultar eventos oficiais. |
| `Monitor/MonitorEventHandler.cs` | Handler auxiliar para processar `MonitorEvent` e salvar como evento local. |
| `Monitor/MonitorEventMapper.cs` | Conversores entre modelo de API e entidade local. |
| `Monitor/MonitorEventQueue.cs` | Fila em memoria para expansoes futuras de processamento assincrono. |

## Endpoints recebidos pela PoC

O controller `OfficialCallbacksController` cobre tres familias de entrada.

### Eventos de identificacao online

Rotas:

```text
POST /new_biometric_image.fcgi
POST /new_biometric_template.fcgi
POST /new_card.fcgi
POST /new_qrcode.fcgi
POST /new_uhf_tag.fcgi
POST /new_user_id_and_password.fcgi
POST /new_user_identified.fcgi
```

Essas rotas chamam:

```csharp
_callbackIngressService.PersistAsync(HttpContext, "identification", cancellationToken)
```

Quando o evento e aceito, a PoC responde:

```json
{
  "result": {
    "event": 14
  }
}
```

### Eventos reconhecidos

Rotas:

```text
POST /new_rex_log.fcgi
POST /device_is_alive.fcgi
POST /card_create.fcgi
POST /fingerprint_create.fcgi
POST /template_create.fcgi
POST /face_create.fcgi
POST /pin_create.fcgi
POST /password_create.fcgi
```

Essas rotas chamam:

```csharp
_callbackIngressService.PersistAsync(HttpContext, "callback", cancellationToken)
```

Quando aceitas, retornam `200 OK` sem payload adicional.

### Notificacoes de Monitor

Rota generica:

```text
POST /api/notifications/{topic}
```

Essa rota chama:

```csharp
_callbackIngressService.PersistAsync(HttpContext, $"monitor:{topic}", cancellationToken)
```

Exemplos catalogados:

| Rota | Finalidade |
| --- | --- |
| `/api/notifications/user_image` | Receber imagem de usuario pelo Monitor. |
| `/api/notifications/template` | Receber template pelo Monitor. |
| `/api/notifications/card` | Receber cartao pelo Monitor. |
| `/api/notifications/operation_mode` | Receber mudanca de modo de operacao. |
| `/api/notifications/pin` | Receber PIN de cadastro remoto. |
| `/api/notifications/password` | Receber senha de cadastro remoto. |
| `/api/notifications/catra_event` | Receber evento de catraca. |
| `/api/notifications/usb_drive` | Receber evento relacionado a USB. |

## Pipeline de entrada

A logica principal fica em `CallbackIngressService.PersistAsync`.

O fluxo e:

1. `CallbackSecurityEvaluator.Evaluate` valida a requisicao.
2. `CallbackRequestBodyReader.ReadAsync` le o corpo da requisicao.
3. A PoC monta um `MonitorEventLocal`.
4. O evento e persistido por `MonitorEventRepository.AddMonitorEventAsync`.
5. A resposta informa sucesso ou rejeicao.

O `EventType` e montado assim:

```text
<familia>:<path>
```

Exemplos:

```text
identification:/new_user_identified.fcgi
callback:/device_is_alive.fcgi
monitor:operation_mode:/api/notifications/operation_mode
legacy-webhook:/MonitorWebhook/Receive
```

## Seguranca dos callbacks

A seguranca do Monitor e configurada por `CallbackSecurityOptions`, carregada da secao `CallbackSecurity` do `appsettings`.

| Opcao | Comportamento |
| --- | --- |
| `MaxBodyBytes` | Limita o tamanho maximo do payload. O default e 1 MB. |
| `AllowedRemoteIps` | Quando preenchido, restringe os IPs que podem enviar callbacks. |
| `AllowLoopback` | Permite loopback mesmo quando ha filtro de IP. |
| `RequireSharedKey` | Exige uma chave compartilhada no header configurado. |
| `SharedKeyHeaderName` | Nome do header usado para a chave. Default: `X-ControlID-Callback-Key`. |
| `SharedKey` | Valor esperado da chave compartilhada. |

A comparacao da chave usa `CryptographicOperations.FixedTimeEquals`, evitando comparacao simples de strings para o segredo.

O `Program.cs` tambem faz uma verificacao de sanidade no startup:

| Condicao | Resultado |
| --- | --- |
| `RequireSharedKey=true` e `SharedKey` vazio | Log de erro de seguranca. |
| `RequireSharedKey=false` fora de Development | Log de warning recomendando restricao. |

## Leitura do corpo da requisicao

`CallbackRequestBodyReader` trata dois cenarios:

| Tipo de corpo | Tratamento |
| --- | --- |
| Texto/JSON/form | Lido como UTF-8 e salvo como string. |
| `application/octet-stream` ou `image/*` | Convertido para Base64 antes de persistir. |

Isso permite que eventos com imagem, template ou binario sejam inspecionados posteriormente sem quebrar a persistencia textual no SQLite.

## Persistencia local

Os eventos sao salvos na tabela `MonitorEvents`.

O `Program.cs` garante a criacao da tabela quando a aplicacao inicia:

```text
MonitorEvents(
  EventId,
  ReceivedAt,
  RawJson,
  EventType,
  DeviceId,
  UserId,
  Payload,
  Status,
  CreatedAt,
  UpdatedAt
)
```

Campos principais:

| Campo | Origem |
| --- | --- |
| `EventId` | GUID gerado pela PoC. |
| `ReceivedAt` | Data/hora UTC de recebimento. |
| `RawJson` | Corpo bruto lido da requisicao. |
| `EventType` | Familia + path do callback. |
| `DeviceId` | Query string `device_id`, quando enviada. |
| `UserId` | Query string `user_id`, quando enviada. |
| `Payload` | Mesmo conteudo lido do corpo, salvo para exibicao rapida. |
| `Status` | Atualmente definido como `received` no pipeline principal. |

## Interface de consulta

A interface principal esta em:

```text
GET /OfficialEvents
GET /OfficialEvents/Details/{id}
POST /OfficialEvents/Clear
```

`OfficialEventsController.Index` consulta todos os eventos por `MonitorEventRepository.GetAllMonitorEventsAsync`, ordenados do mais recente para o mais antigo.

`Details` abre um evento especifico.

`Clear` remove todos os eventos persistidos.

`MonitorWebhookController` mantem compatibilidade com a rota legada:

| Rota | Comportamento |
| --- | --- |
| `GET /MonitorWebhook` | Redireciona para `OfficialEvents/Index`. |
| `GET /MonitorWebhook/Details/{id}` | Redireciona para `OfficialEvents/Details/{id}`. |
| `POST /MonitorWebhook/Receive` | Recebe webhook legado e persiste como `legacy-webhook`. |
| `POST /MonitorWebhook/Clear` | Limpa eventos e redireciona para `OfficialEvents`. |

## Relacao com Modos de operacao

A tela de modos usa eventos do Monitor para mostrar prontidao e sinais recentes.

`OperationModesController` consulta `MonitorEventRepository.GetAllMonitorEventsAsync` e verifica eventos cujo `EventType` termina com:

```text
/new_user_identified.fcgi
/new_card.fcgi
/new_biometric_image.fcgi
/device_is_alive.fcgi
/api/notifications/operation_mode
```

Isso permite que a PoC mostre se os endpoints que sustentam Pro e Enterprise ja receberam eventos reais.

## Componentes auxiliares de Monitor

A pasta `Monitor/` possui estruturas auxiliares:

| Arquivo | Situacao na implementacao atual |
| --- | --- |
| `MonitorEventHandler.cs` | Converte um `MonitorEvent` de API em `MonitorEventLocal` e persiste. Serve como ponto de extensao para processamento programatico. |
| `MonitorEventMapper.cs` | Faz conversao entre `MonitorEvent` e `MonitorEventLocal`. |
| `MonitorEventQueue.cs` | Implementa fila em memoria com `ConcurrentQueue` e `SemaphoreSlim`. |

O pipeline HTTP principal de callbacks usa `CallbackIngressService` diretamente. A fila em memoria existe como base para evolucao futura, por exemplo processamento assincrono, SignalR ou notificacao em tempo real.

## Cobertura de testes

| Teste | Cobre |
| --- | --- |
| `CallbackSecurityEvaluatorTests.cs` | Validacao de IP, tamanho, loopback e chave compartilhada. |
| `CallbackRequestBodyReaderTests.cs` | Leitura de payload textual, vazio, binario e limite de tamanho. |
| `CallbackIngressServiceTests.cs` | Fluxo de persistencia/rejeicao dos callbacks recebidos. |

## Limitacoes atuais

| Ponto | Observacao |
| --- | --- |
| Equipamento real | A validacao completa depende de um dispositivo Control iD enviando callbacks reais. |
| Tempo real | A PoC persiste e lista eventos, mas ainda nao publica eventos via SignalR/websocket. |
| Processamento assincrono | A fila em memoria existe, mas o pipeline principal persiste diretamente no banco. |
| Exposicao publica | Para receber callbacks reais, a URL da PoC precisa estar acessivel pelo equipamento. |
