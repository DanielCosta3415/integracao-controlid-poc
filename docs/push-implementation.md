# Push: implementação na PoC

Este documento explica como a funcionalidade Push foi implementada dentro da PoC de integração com a API de controle de acesso da Control iD.

Na PoC, Push representa o fluxo em que o equipamento consulta a aplicação para buscar comandos pendentes e depois devolve o resultado da execução. Isso é diferente do Monitor, em que o equipamento envia notificações diretamente para a PoC.

## Visão geral

O Push foi implementado como uma fila persistida em SQLite.

O ciclo principal é:

```text
Usuário enfileira comando na PoC
  -> comando fica com status pending
  -> equipamento chama GET /push
  -> PoC entrega o payload e marca como delivered
  -> equipamento executa comando
  -> equipamento chama POST /result
  -> PoC registra o resultado e marca como completed ou status recebido
```

A tela operacional principal é `PushCenter`.

## Arquivos envolvidos

| Arquivo | Papel na funcionalidade |
| --- | --- |
| `Controllers/PushCenterController.cs` | Implementa a central atual de Push, incluindo fila, polling `GET /push`, retorno `POST /result`, detalhes e limpeza. |
| `Controllers/PushController.cs` | Mantém rotas legadas, redirecionamentos e recebimento antigo em `POST /Push/Receive`. |
| `Models/Database/PushCommandLocal.cs` | Entidade persistida na tabela `PushCommands`. |
| `Models/ControlIDApi/PushCommand.cs` | Modelo de API para representar comando/evento Push. |
| `Services/Database/PushCommandRepository.cs` | Repositório responsável por inserir, consultar, atualizar e remover comandos Push. |
| `ViewModels/Push/PushQueueCommandViewModel.cs` | Campos usados para enfileirar um novo comando. |
| `ViewModels/Push/PushEventListViewModel.cs` | Dados da listagem da central Push. |
| `ViewModels/Push/PushEventViewModel.cs` | Dados de detalhe de um comando/evento Push. |
| `Views/PushCenter/Index.cshtml` | Tela de criação de comando e histórico da fila. |
| `Views/PushCenter/Details.cshtml` | Tela de inspeção de payload e JSON bruto. |
| `Services/ControlIDApi/OfficialApiCatalogService.cs` | Cataloga `GET /push`, `POST /result` e endpoints relacionados a Push. |

## Endpoints implementados

### Central web

| Rota | Controller | Finalidade |
| --- | --- | --- |
| `GET /PushCenter` | `PushCenterController.Index` | Lista comandos/eventos e exibe formulário para enfileirar comando. |
| `GET /PushCenter/Details/{id}` | `PushCenterController.Details` | Exibe payload, JSON bruto e metadados de um item. |
| `POST /PushCenter/Queue` | `PushCenterController.Queue` | Cria comando pendente para o equipamento consumir. |
| `POST /PushCenter/Clear` | `PushCenterController.Clear` | Limpa a fila persistida. |

### Endpoints oficiais servidos pela PoC

| Rota | Controller | Finalidade |
| --- | --- | --- |
| `GET /push` | `PushCenterController.Poll` | Entrega o próximo comando pendente para um equipamento. |
| `POST /result` | `PushCenterController.Result` | Recebe o resultado de execução de um comando Push. |

Essas rotas aparecem no catálogo oficial como:

| ID no catálogo | Rota | Descrição |
| --- | --- | --- |
| `push-poll` | `GET /push` | Endpoint servido pela PoC para o equipamento buscar comandos pendentes. |
| `push-result` | `POST /result` | Endpoint servido pela PoC para o equipamento devolver o resultado. |
| `change-idcloud-code` | `/change_idcloud_code.fcgi` | Endpoint oficial relacionado a Push/iDCloud, invocado no equipamento. |

### Rotas legadas

`PushController` mantém compatibilidade com rotas antigas:

| Rota | Comportamento |
| --- | --- |
| `GET /Push` | Redireciona para `PushCenter/Index`. |
| `GET /Push/Details/{id}` | Redireciona para `PushCenter/Details/{id}`. |
| `POST /Push/Receive` | Recebe um evento Push legado, tenta interpretar JSON e persiste como item recebido. |
| `POST /Push/Clear` | Limpa comandos e redireciona para a central. |

## Como um comando é enfileirado

O usuário preenche o formulário da central com:

| Campo | Origem |
| --- | --- |
| `DeviceId` | Dispositivo de destino. |
| `CommandType` | Tipo lógico do comando. |
| `UserId` | Usuário relacionado, quando aplicável. |
| `Payload` | JSON que será entregue ao equipamento. |

`PushCenterController.Queue` valida o `PushQueueCommandViewModel` e cria um `PushCommandLocal` com:

```text
Status = pending
RawJson = Payload
Payload = Payload
CreatedAt = DateTime.UtcNow
CommandId = Guid.NewGuid()
```

Depois o comando é salvo por `PushCommandRepository.AddPushCommandAsync`.

## Como o equipamento busca comandos

O equipamento chama:

```text
GET /push?device_id=<id-do-equipamento>
```

A PoC também aceita o parâmetro legado:

```text
GET /push?deviceid=<id-do-equipamento>
```

O controller resolve o dispositivo usando `device_id` ou `deviceid` e chama:

```csharp
_pushCommandRepository.GetNextPendingCommandAsync(resolvedDeviceId)
```

A consulta pega o primeiro comando com `Status == "pending"`, ordenado por `CreatedAt`.

Se `device_id` foi informado, a PoC entrega comandos com:

```text
command.DeviceId == deviceId
```

ou comandos sem dispositivo específico:

```text
string.IsNullOrEmpty(command.DeviceId)
```

Quando encontra um comando:

1. O status muda para `delivered`.
2. `UpdatedAt` recebe `DateTime.UtcNow`.
3. O comando é atualizado no SQLite.
4. O `Payload` é retornado como `application/json`.

Se não houver comando, a PoC retorna:

```json
{}
```

## Como o resultado é recebido

Depois de executar o comando, o equipamento pode chamar:

```text
POST /result?command_id=<guid>
```

O corpo da requisição é lido como texto bruto.

O fluxo de `PushCenterController.Result` é:

1. Ler `command_id` da query string, quando enviado.
2. Buscar o comando existente no banco.
3. Se existir, atualizar `RawJson`, `Payload`, `Status` e `UpdatedAt`.
4. Se não existir, criar um novo registro do tipo `result`.
5. Se a query `status` não vier preenchida, usar `completed`.
6. Retornar `200 OK`.

Exemplo:

```text
POST /result?command_id=00000000-0000-0000-0000-000000000001&status=completed
```

## Persistência local

Os comandos ficam na tabela `PushCommands`.

O `Program.cs` garante a criação da tabela quando a aplicação inicia:

```text
PushCommands(
  CommandId,
  ReceivedAt,
  CommandType,
  RawJson,
  Status,
  Payload,
  DeviceId,
  UserId,
  CreatedAt,
  UpdatedAt
)
```

Campos principais:

| Campo | Uso |
| --- | --- |
| `CommandId` | Identificador GUID do comando/evento. |
| `ReceivedAt` | Data/hora de recebimento ou registro. |
| `CommandType` | Tipo do comando, por exemplo `custom`, `result` ou tipo enviado no evento legado. |
| `RawJson` | JSON bruto ou corpo recebido. |
| `Status` | Estado operacional do item. |
| `Payload` | Conteúdo entregue ao equipamento ou resultado recebido. |
| `DeviceId` | Dispositivo alvo ou origem. |
| `UserId` | Usuário relacionado, quando houver. |
| `CreatedAt` | Data/hora de criação local. |
| `UpdatedAt` | Data/hora da última atualização. |

## Estados usados pela PoC

| Status | Quando aparece |
| --- | --- |
| `pending` | Comando criado pela tela e aguardando consumo pelo equipamento. |
| `delivered` | Comando entregue por `GET /push`. |
| `completed` | Resultado recebido em `POST /result` sem status explícito. |
| `received` | Evento recebido pela rota legada `POST /Push/Receive`. |
| Valor enviado em `status` | Quando `POST /result` informa um status específico na query string. |

A UI também possui espaço visual para estados como erro/concluído, mas o fluxo atual usa os estados acima de forma direta.

## Recebimento legado em `/Push/Receive`

A rota `POST /Push/Receive` aceita um corpo bruto e tenta interpretar alguns campos JSON:

| Campo tentado | Uso |
| --- | --- |
| `command_type`, `type` ou `event` | Define `CommandType`. |
| `status` | Define `Status`. |
| `device_id` ou `deviceid` | Define `DeviceId`. |
| `user_id` ou `userid` | Define `UserId`. |
| `payload` ou `data` | Define `Payload`. |

Se o JSON for inválido, a PoC registra warning e ainda salva o corpo bruto como evento legado com `CommandType = "legacy_push_event"` e `Status = "received"`.

## Relação com Monitor

Push e Monitor compartilham a área de observabilidade da PoC, mas não usam a mesma tabela.

| Funcionalidade | Tabela | Direção principal |
| --- | --- | --- |
| Monitor | `MonitorEvents` | Equipamento envia eventos para a PoC. |
| Push | `PushCommands` | Equipamento busca comandos na PoC e devolve resultado. |

Essa separação deixa mais claro o ciclo de vida:

| Monitor | Push |
| --- | --- |
| Evento recebido e registrado. | Comando criado, entregue e finalizado. |
| Foco em callbacks/notificações. | Foco em fila de comandos. |
| UI principal: `OfficialEvents`. | UI principal: `PushCenter`. |

## Segurança e considerações operacionais

O endpoint legado `POST /Push/Receive` limita o corpo em aproximadamente 1 MB.

Os endpoints oficiais servidos pela PoC (`GET /push` e `POST /result`) foram implementados para facilitar a demonstração funcional. Diferentemente do pipeline de callbacks, eles não passam atualmente por `CallbackSecurityEvaluator`.

Para uso fora de PoC, seria recomendável:

- exigir chave compartilhada ou assinatura;
- restringir IPs de origem;
- usar HTTPS em uma URL acessível pelo equipamento;
- validar formato do payload antes de entregar ao dispositivo;
- registrar tentativa de polling e resultados com mais metadados.

## Cobertura de testes

No estado atual, a funcionalidade Push é validada principalmente pelos smoke tests locais e pela estrutura de repositório/fluxo da aplicação.

Ainda não há uma suíte unitária dedicada para `PushCenterController`, `PushController` ou `PushCommandRepository`. Uma evolução natural seria cobrir:

| Cenário | Teste sugerido |
| --- | --- |
| Enfileirar comando válido | Deve criar item `pending`. |
| Poll com comando pendente | Deve retornar payload e marcar `delivered`. |
| Poll sem comando | Deve retornar `{}`. |
| Resultado com `command_id` existente | Deve atualizar item existente. |
| Resultado sem `command_id` | Deve criar registro `result`. |
| Evento legado inválido | Deve persistir corpo bruto sem quebrar. |

## Limitações atuais

| Ponto | Observação |
| --- | --- |
| Equipamento real | O ciclo completo depende de um dispositivo consultando `GET /push` e enviando `POST /result`. |
| Autenticação dos endpoints Push | Ainda não foi aplicada a mesma camada de segurança dos callbacks. |
| Status padronizados | A PoC aceita status livres vindos de `/result`, o que é flexível, mas pode exigir normalização futura. |
| Concorrência | O fluxo entrega o primeiro comando pendente; em produção, seria interessante reforcar controle transacional para múltiplos polls simultaneos. |

