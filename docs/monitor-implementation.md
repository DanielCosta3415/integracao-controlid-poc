# Monitor: implementação na PoC

Este documento explica como a funcionalidade Monitor foi implementada dentro da PoC de integração com a API de controle de acesso da Control iD.

Na PoC, "Monitor" representa a trilha de recebimento, persistência e visualização de callbacks/notificações enviados pelo equipamento para a aplicação. Ele é diferente do Push: no Monitor, o equipamento envia eventos para a PoC; no Push, o equipamento consulta a PoC para buscar comandos pendentes.

## Visão geral

O Monitor foi implementado como um pipeline de entrada HTTP:

```text
Equipamento Control iD
  -> endpoint da PoC
  -> validação de segurança
  -> leitura do corpo da requisição
  -> persistência em SQLite
  -> tela de eventos oficiais
  -> uso por outras telas, como Modos de operação
```

A tela operacional principal é `OfficialEvents`. As rotas legadas de `MonitorWebhook` redirecionam para essa tela consolidada.

## Arquivos envolvidos

| Arquivo | Papel na funcionalidade |
| --- | --- |
| `Controllers/OfficialCallbacksController.cs` | Expõe endpoints oficiais de callback e monitor recebidos pelo equipamento. |
| `Controllers/OfficialEventsController.cs` | Lista, detalha e limpa eventos oficiais persistidos. |
| `Controllers/MonitorWebhookController.cs` | Mantém uma entrada legada para webhook e redireciona consultas para `OfficialEvents`. |
| `Services/Callbacks/CallbackIngressService.cs` | Orquestra validação, leitura e persistência dos callbacks recebidos. |
| `Services/Callbacks/CallbackSecurityEvaluator.cs` | Aplica regras de segurança para callbacks: tamanho, IP permitido e chave compartilhada. |
| `Services/Callbacks/CallbackRequestBodyReader.cs` | Lê o corpo da requisição e converte payload binário/imagem para Base64. |
| `Options/CallbackSecurityOptions.cs` | Define as configurações de segurança usadas pelo pipeline de callback. |
| `Models/Database/MonitorEventLocal.cs` | Entidade local persistida na tabela `MonitorEvents`. |
| `Services/Database/MonitorEventRepository.cs` | Repositório de persistência e consulta dos eventos monitorados. |
| `ViewModels/Monitor/*` | Modelos usados para listagem e detalhe dos eventos. |
| `Views/OfficialEvents/*` | Interface principal para consultar eventos oficiais. |
| `Monitor/MonitorEventHandler.cs` | Handler auxiliar para processar `MonitorEvent` e salvar como evento local. |
| `Monitor/MonitorEventMapper.cs` | Conversores entre modelo de API e entidade local. |
| `Monitor/MonitorEventQueue.cs` | Fila em memória para expansões futuras de processamento assíncrono. |

## Endpoints recebidos pela PoC

O controller `OfficialCallbacksController` cobre três famílias de entrada.

### Eventos de identificação online

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

Quando o evento é aceito, a PoC responde:

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

### Notificações de Monitor

Rota genérica:

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
| `/api/notifications/user_image` | Receber imagem de usuário pelo Monitor. |
| `/api/notifications/template` | Receber template pelo Monitor. |
| `/api/notifications/card` | Receber cartão pelo Monitor. |
| `/api/notifications/operation_mode` | Receber mudança de modo de operação. |
| `/api/notifications/pin` | Receber PIN de cadastro remoto. |
| `/api/notifications/password` | Receber senha de cadastro remoto. |
| `/api/notifications/catra_event` | Receber evento de catraca. |
| `/api/notifications/usb_drive` | Receber evento relacionado a USB. |

## Pipeline de entrada

A lógica principal fica em `CallbackIngressService.PersistAsync`.

O fluxo é:

1. `CallbackSecurityEvaluator.Evaluate` valida a requisição.
2. `CallbackRequestBodyReader.ReadAsync` lê o corpo da requisição.
3. A PoC monta um `MonitorEventLocal`.
4. O evento é persistido por `MonitorEventRepository.AddMonitorEventAsync`.
5. A resposta informa sucesso ou rejeição.

O `EventType` é montado assim:

```text
<família>:<path>
```

Exemplos:

```text
identification:/new_user_identified.fcgi
callback:/device_is_alive.fcgi
monitor:operation_mode:/api/notifications/operation_mode
legacy-webhook:/MonitorWebhook/Receive
```

## Segurança dos callbacks

A segurança do Monitor é configurada por `CallbackSecurityOptions`, carregada da seção `CallbackSecurity` do `appsettings`.

| Opção | Comportamento |
| --- | --- |
| `MaxBodyBytes` | Limita o tamanho máximo do payload. O default é 1 MB. |
| `AllowedRemoteIps` | Quando preenchido, restringe os IPs que podem enviar callbacks. |
| `AllowLoopback` | Permite loopback mesmo quando há filtro de IP. |
| `RequireSharedKey` | Exige uma chave compartilhada no header configurado. |
| `SharedKeyHeaderName` | Nome do header usado para a chave. Default: `X-ControlID-Callback-Key`. |
| `SharedKey` | Valor esperado da chave compartilhada. |

A comparação da chave usa `CryptographicOperations.FixedTimeEquals`, evitando comparação simples de strings para o segredo.

O `Program.cs` também faz uma verificação de sanidade no startup:

| Condição | Resultado |
| --- | --- |
| `RequireSharedKey=true` e `SharedKey` vazio | Log de erro de segurança. |
| `RequireSharedKey=false` fora de Development | Log de warning recomendando restrição. |

## Leitura do corpo da requisição

`CallbackRequestBodyReader` trata dois cenários:

| Tipo de corpo | Tratamento |
| --- | --- |
| Texto/JSON/form | Lido como UTF-8 e salvo como string. |
| `application/octet-stream` ou `image/*` | Convertido para Base64 antes de persistir. |

Isso permite que eventos com imagem, template ou binário sejam inspecionados posteriormente sem quebrar a persistência textual no SQLite.

## Persistência local

Os eventos são salvos na tabela `MonitorEvents`.

O `Program.cs` garante a criação da tabela quando a aplicação inicia:

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
| `RawJson` | Corpo bruto lido da requisição. |
| `EventType` | Família + path do callback. |
| `DeviceId` | Query string `device_id`, quando enviada. |
| `UserId` | Query string `user_id`, quando enviada. |
| `Payload` | Mesmo conteúdo lido do corpo, salvo para exibição rápida. |
| `Status` | Atualmente definido como `received` no pipeline principal. |

## Interface de consulta

A interface principal está em:

```text
GET /OfficialEvents
GET /OfficialEvents/Details/{id}
POST /OfficialEvents/Clear
```

`OfficialEventsController.Index` consulta todos os eventos por `MonitorEventRepository.GetAllMonitorEventsAsync`, ordenados do mais recente para o mais antigo.

`Details` abre um evento específico.

`Clear` remove todos os eventos persistidos.

`MonitorWebhookController` mantém compatibilidade com a rota legada:

| Rota | Comportamento |
| --- | --- |
| `GET /MonitorWebhook` | Redireciona para `OfficialEvents/Index`. |
| `GET /MonitorWebhook/Details/{id}` | Redireciona para `OfficialEvents/Details/{id}`. |
| `POST /MonitorWebhook/Receive` | Recebe webhook legado e persiste como `legacy-webhook`. |
| `POST /MonitorWebhook/Clear` | Limpa eventos e redireciona para `OfficialEvents`. |

## Relação com Modos de operação

A tela de modos usa eventos do Monitor para mostrar prontidão e sinais recentes.

`OperationModesController` consulta `MonitorEventRepository.GetAllMonitorEventsAsync` e verifica eventos cujo `EventType` termina com:

```text
/new_user_identified.fcgi
/new_card.fcgi
/new_biometric_image.fcgi
/device_is_alive.fcgi
/api/notifications/operation_mode
```

Isso permite que a PoC mostre se os endpoints que sustentam Pro e Enterprise já receberam eventos reais.

## Componentes auxiliares de Monitor

A pasta `Monitor/` possui estruturas auxiliares:

| Arquivo | Situação na implementação atual |
| --- | --- |
| `MonitorEventHandler.cs` | Converte um `MonitorEvent` de API em `MonitorEventLocal` e persiste. Serve como ponto de extensão para processamento programático. |
| `MonitorEventMapper.cs` | Faz conversão entre `MonitorEvent` e `MonitorEventLocal`. |
| `MonitorEventQueue.cs` | Implementa fila em memória com `ConcurrentQueue` e `SemaphoreSlim`. |

O pipeline HTTP principal de callbacks usa `CallbackIngressService` diretamente. A fila em memória existe como base para evolução futura, por exemplo processamento assíncrono, SignalR ou notificação em tempo real.

## Cobertura de testes

| Teste | Cobre |
| --- | --- |
| `CallbackSecurityEvaluatorTests.cs` | Validação de IP, tamanho, loopback e chave compartilhada. |
| `CallbackRequestBodyReaderTests.cs` | Leitura de payload textual, vazio, binário e limite de tamanho. |
| `CallbackIngressServiceTests.cs` | Fluxo de persistência/rejeição dos callbacks recebidos. |

## Limitações atuais

| Ponto | Observação |
| --- | --- |
| Equipamento real | A validação completa depende de um dispositivo Control iD enviando callbacks reais. |
| Tempo real | A PoC persiste e lista eventos, mas ainda não publica eventos via SignalR/websocket. |
| Processamento assíncrono | A fila em memória existe, mas o pipeline principal persiste diretamente no banco. |
| Exposicao pública | Para receber callbacks reais, a URL da PoC precisa estar acessível pelo equipamento. |
