# Modos de operacao: Standalone, Pro e Enterprise

Este documento explica como a PoC representa, detecta e aplica os modos de operacao Standalone, Pro e Enterprise da API de controle de acesso da Control iD.

Ele complementa os relatorios de validacao existentes em `docs/reports/operation-modes-e2e-runbook-2026-04-14.md` e `docs/reports/operation-modes-homologation-matrix-2026-04-14.md`. A diferenca aqui e o foco: esta documentacao descreve a implementacao dentro da aplicacao.

## Visao geral

Na PoC, os modos de operacao sao tratados como perfis de configuracao aplicados ao equipamento por meio da API oficial.

O ponto central e a combinacao das configuracoes `general.online` e `general.local_identification`:

| Modo | `online` | `local_identification` | Ideia operacional |
| --- | --- | --- | --- |
| Standalone | `0` | `1` | O equipamento opera localmente, sem depender do servidor online para identificar ou autorizar. |
| Pro | `1` | `1` | O equipamento fica online, mas continua fazendo identificacao local e enviando callbacks/eventos para a aplicacao. |
| Enterprise | `1` | `0` | O equipamento fica online e a decisao passa a ser centralizada/orientada ao servidor. |

Essa regra de classificacao esta implementada em `Services/OperationModes/OperationModesProfileResolver.cs`.

## Arquivos envolvidos

| Arquivo | Papel na funcionalidade |
| --- | --- |
| `Controllers/OperationModesController.cs` | Orquestra leitura do estado atual, aplicacao dos perfis, validacao de sessao, upgrades de licenca e montagem da tela. |
| `Services/OperationModes/OperationModesPayloadFactory.cs` | Cria os payloads enviados para `set-configuration` e `create-objects`. |
| `Services/OperationModes/OperationModesProfileResolver.cs` | Traduz `online` e `local_identification` em Standalone, Pro ou Enterprise. |
| `ViewModels/OperationModes/OperationModesViewModel.cs` | Carrega todos os dados exibidos na tela: estado atual, cards, callbacks, server_id, licencas e resposta bruta. |
| `Views/OperationModes/Index.cshtml` | Interface do hub de modos de operacao. |
| `Services/ControlIDApi/OfficialApiCatalogService.cs` | Cataloga os endpoints oficiais usados indiretamente pela funcionalidade. |
| `Services/ControlIDApi/OfficialControlIdApiService.cs` | Executa as chamadas HTTP reais para a API do equipamento. |
| `Services/Database/MonitorEventRepository.cs` | Fornece eventos recentes usados para mostrar prontidao de callbacks e sinais de modo. |

## Como o estado atual e detectado

Quando a tela `OperationModes/Index` e aberta, o controller chama `PrepareViewModelAsync`.

O fluxo e:

1. Aplicar defaults de runtime para `ServerUrl` e `CallbackBaseUrl` com base em `Request.Scheme` e `Request.Host`.
2. Verificar se existe uma conexao ativa com o equipamento por `_apiService.TryGetConnection`.
3. Se nao houver conexao, a tela fica em modo aguardando equipamento.
4. Se houver conexao, a PoC chama `get-configuration` solicitando `general.online`, `general.local_identification`, `online_client.server_id`, `online_client.extract_template` e `online_client.max_request_attempts`.
5. A PoC chama `session-is-valid` para indicar se a sessao oficial ainda esta valida.
6. A PoC chama `system-information` para tentar exibir modelo e numero de serie do equipamento.
7. O par `online` + `local_identification` e entregue ao `OperationModesProfileResolver`, que resolve o modo atual.

O resultado visual aparece na tela como `CurrentModeLabel`, `CurrentModeDescription`, `CurrentModeEvidence` e badge de tom visual.

## Como cada modo e aplicado

As transicoes sao acionadas manualmente por POSTs da tela. Nao existe um job automatico mudando modo em segundo plano.

### Standalone

O botao "Aplicar Standalone" chama `ApplyStandalone`.

Antes de aplicar, o controller valida se existe conexao com equipamento. Depois ele chama:

```csharp
_apiService.InvokeJsonAsync("set-configuration", _payloadFactory.BuildStandaloneSettings())
```

O payload gerado e:

```json
{
  "general": {
    "online": "0",
    "local_identification": "1"
  }
}
```

Na pratica, a PoC desliga o modo online e preserva a identificacao local.

### Pro

O botao "Aplicar Pro" chama `ApplyPro`.

Antes de enviar `set-configuration`, a PoC precisa resolver um `server_id`. Isso e feito por `ResolveServerIdAsync`.

Ha dois caminhos:

| Caminho | Comportamento |
| --- | --- |
| Reutilizar device existente | Usa o valor informado em `ExistingDeviceId`. |
| Criar servidor online | Chama `create-objects` criando um objeto `devices` com `name`, `ip` e `public_key`, depois le o primeiro ID retornado em `ids`. |

Depois de resolver o `server_id`, a PoC chama:

```csharp
_apiService.InvokeJsonAsync(
    "set-configuration",
    _payloadFactory.BuildProSettings(serverId, model.ExtractTemplate, model.MaxRequestAttempts))
```

O payload final segue esta forma:

```json
{
  "general": {
    "online": "1",
    "local_identification": "1"
  },
  "online_client": {
    "server_id": "<server_id>",
    "extract_template": "0 ou 1",
    "max_request_attempts": "<tentativas>"
  }
}
```

O Pro liga o modo online, mas mantem a identificacao local ativa.

### Enterprise

O botao "Aplicar Enterprise" chama `ApplyEnterprise`.

O fluxo de `server_id` e o mesmo do Pro: a PoC pode reutilizar um device existente ou criar o servidor online por `create-objects`.

Depois disso, a PoC chama:

```csharp
_apiService.InvokeJsonAsync(
    "set-configuration",
    _payloadFactory.BuildEnterpriseSettings(serverId, model.ExtractTemplate, model.MaxRequestAttempts))
```

O payload final segue esta forma:

```json
{
  "general": {
    "online": "1",
    "local_identification": "0"
  },
  "online_client": {
    "server_id": "<server_id>",
    "extract_template": "0 ou 1",
    "max_request_attempts": "<tentativas>"
  }
}
```

O Enterprise liga o modo online e desliga a identificacao local, deixando a operacao orientada ao servidor.

## Como a transicao entre modos funciona

A transicao e uma chamada oficial de configuracao aplicada ao equipamento.

O ciclo e:

1. O usuario acessa `OperationModes/Index`.
2. A PoC identifica o modo atual via `get-configuration`.
3. O usuario escolhe Standalone, Pro ou Enterprise.
4. O controller valida conexao e sessao operacional.
5. Para Pro/Enterprise, a PoC resolve ou cria o `server_id`.
6. A PoC envia `set-configuration` com o payload do modo escolhido.
7. A resposta oficial e formatada por `OfficialApiResultPresentationService`.
8. A tela recarrega o estado remoto para mostrar o modo detectado apos a alteracao.

Importante: a PoC nao guarda uma tabela propria de "modo atual". A fonte de verdade e o equipamento, lido por `get-configuration`. O banco local entra apenas como apoio para eventos/callbacks recentes usados na observabilidade da tela.

## Licencas e upgrades

A tela tambem inclui acoes de licenciamento, mas elas sao separadas da aplicacao de perfil.

| Acao | Metodo no controller | Endpoint catalogado |
| --- | --- | --- |
| Upgrade Pro do iDFace | `UpgradeProLicense` | `upgrade-idface-pro`, caminho oficial `/upgrade_ten_thousand_face_templates.fcgi` |
| Upgrade Enterprise | `UpgradeEnterpriseLicense` | `upgrade-idflex-enterprise`, caminho oficial `/idflex_upgrade_enterprise.fcgi` |

Essas acoes enviam um payload simples:

```json
{
  "password": "<licenca-control-id>"
}
```

Na PoC, essas chamadas apenas solicitam o upgrade ao equipamento e exibem a resposta. A disponibilidade real depende de produto, firmware e licenca fornecida pela Control iD.

## Relacao com callbacks e monitoramento

Os modos online dependem de endpoints que recebem eventos do equipamento. Por isso, a tela tambem mostra uma grade de prontidao com rotas relevantes:

| Rota | Uso na PoC |
| --- | --- |
| `/new_user_identified.fcgi` | Evento de usuario identificado localmente em modo Pro. |
| `/new_card.fcgi` | Evento online por cartao. |
| `/new_biometric_image.fcgi` | Evento de imagem biometrica. |
| `/device_is_alive.fcgi` | Heartbeat/keep-alive do equipamento. |
| `/api/notifications/operation_mode` | Notificacao de mudanca de modo via Monitor. |

Esses sinais sao lidos do `MonitorEventRepository`. A tela usa `BuildReadiness` para mostrar se cada rota ja recebeu algum evento, e `BuildRecentSignals` para exibir os ultimos sinais relacionados aos modos.

## Tratamento de erro e observabilidade

| Situacao | Comportamento |
| --- | --- |
| Sem equipamento conectado | A tela informa que e necessario conectar e autenticar antes de aplicar modo. |
| Falha ao ler configuracao | O erro e registrado como warning e a tela continua renderizando o que for possivel. |
| Falha ao aplicar modo | A mensagem para o usuario e sanitizada por `SecurityTextHelper.BuildSafeUserMessage`. |
| Resposta oficial bem-sucedida | A resposta bruta e exibida no painel `_RawResponsePanel`. |
| Erros tecnicos | Serilog registra o contexto no logger do controller. |

## Cobertura de testes

| Teste | Cobre |
| --- | --- |
| `OperationModesPayloadFactoryTests.cs` | Payloads de Standalone, Pro, Enterprise e criacao de servidor online. |
| `OperationModesProfileResolverTests.cs` | Resolucao do modo a partir de `online` e `local_identification`. |

Tambem existem roteiros e relatorios de smoke/homologacao em `docs/reports/`, usados como referencia operacional.

## Limitacoes atuais

| Ponto | Observacao |
| --- | --- |
| Homologacao fisica | A validacao completa depende de um equipamento real Control iD. |
| Historico de mudanca de modo | A PoC nao persiste uma tabela de transicoes; ela consulta o estado atual no equipamento. |
| Licenca | A PoC dispara os endpoints de upgrade, mas nao consegue simular a liberacao real sem produto/licenca compativel. |
| Callbacks | A prontidao dos callbacks depende de a URL publica da PoC estar acessivel pelo equipamento. |

