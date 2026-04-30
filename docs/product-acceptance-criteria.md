# Critérios de aceite, requisitos e rastreabilidade

Este documento transforma o mapeamento de produto, os fluxos críticos e as regras observadas no código em critérios de aceite objetivos e rastreáveis.

Escopo: PoC ASP.NET Core MVC para integração operacional com a Access API da Control iD. Este documento não cria regra de negócio nova; quando uma regra depende de equipamento real, licença, firmware ou decisão futura, ela é marcada como lacuna.

Fontes usadas: `README.md`, `docs/monitor-implementation.md`, `docs/push-implementation.md`, `docs/operation-modes-implementation.md`, controllers MVC, serviços de integração, entidades locais e suíte `tests/Integracao.ControlID.PoC.Tests`.

## Fluxos críticos e critérios de aceite

### F01 - Conexão, login e sessão

Critérios:

- AC-F01-01: Dado que não há dispositivo conectado, quando o usuário tenta autenticar no equipamento, então a PoC deve rejeitar a ação com mensagem funcional, e não deve criar sessão local.
- AC-F01-02: Dado que há dispositivo conectado, quando o login retorna sucesso e contém `session`, então a PoC deve armazenar a sessão ASP.NET e redirecionar para a tela inicial.
- AC-F01-03: Dado que a resposta de login não contém `session`, quando a autenticação retorna, então a PoC deve exibir erro funcional e manter o usuário na tela de login.
- AC-F01-04: Dado que existe sessão ativa, quando o usuário valida a sessão, então a PoC deve chamar `session-is-valid` e informar se a sessão está válida ou expirada.
- AC-F01-05: Dado que não há sessão ativa, quando o usuário valida ou limpa sessão, então a PoC deve responder com aviso seguro e não chamar endpoint autenticado indevidamente.
- AC-F01-06: Dado que o logout é acionado por navegação GET fora da própria origem, quando a requisição chega, então a PoC deve bloquear o logout e pedir confirmação pela interface.

### F02 - Catálogo e invocação da API oficial

Critérios:

- AC-F02-01: Dado que um endpoint oficial existe no catálogo, quando o usuário abre a tela técnica, então a PoC deve exibir método, rota, query/body esperados e exemplos quando disponíveis.
- AC-F02-02: Dado que o endpoint selecionado é servido pela própria PoC, como callback, monitor ou push, quando o usuário tenta invocá-lo pela tela técnica, então a PoC deve informar que ele é um endpoint de entrada local e não deve chamar o equipamento.
- AC-F02-03: Dado que a chamada oficial exige sessão, quando não há sessão Control iD ativa, então a PoC deve bloquear a invocação com mensagem segura.
- AC-F02-04: Dado que o corpo JSON informado é inválido, quando o usuário tenta invocar endpoint com body, então a PoC deve rejeitar a entrada antes de chamar o equipamento.
- AC-F02-05: Dado que a resposta oficial é binária ou não textual, quando a chamada retorna, então a PoC deve preservar o conteúdo em Base64/download e não tentar renderizar como texto inseguro.

### F03 - Objetos oficiais

Critérios:

- AC-F03-01: Dado que o usuário seleciona um objeto oficial, quando carrega exemplos, então a PoC deve manter o objeto selecionado e apresentar metadados do catálogo.
- AC-F03-02: Dado JSON válido em `load-objects`, `create-objects`, `create-or-modify-objects` ou `modify-objects`, quando há conexão ativa, então a PoC deve montar o payload oficial e chamar o equipamento.
- AC-F03-03: Dado JSON inválido em qualquer operação de objeto, quando o formulário é enviado, então a PoC deve exibir erro e não deve chamar o equipamento.
- AC-F03-04: Dado operação `destroy-objects`, quando a confirmação textual não corresponde a `DESTROY <objeto>`, então a PoC deve bloquear a chamada.
- AC-F03-05: Dado operação `destroy-objects` com JSON válido e confirmação correta, quando há conexão ativa, então a PoC pode chamar o equipamento e deve exibir a resposta oficial.

### F04 - Operações administrativas de alto impacto

Critérios:

- AC-F04-01: Dado ação de reboot, modo update, remoção de admins, reset de fábrica, alteração de rede ou destroy de objeto, quando a confirmação textual está ausente ou incorreta, então a PoC deve bloquear a chamada e preservar o estado remoto.
- AC-F04-02: Dado confirmação correta e conexão ativa, quando o usuário executa uma operação administrativa, então a PoC deve chamar o endpoint oficial correspondente e exibir sucesso ou erro sanitizado.
- AC-F04-03: Dado alteração de rede aplicada com sucesso, quando IP/porta podem mudar, então a mensagem deve orientar reconexão.
- AC-F04-04: Dado reset de fábrica, quando `keepNetworkInfo` é informado, então a PoC deve enviar esse valor ao endpoint oficial sem inferir outro comportamento.

### F05 - Modos de operação Standalone, Pro e Enterprise

Critérios:

- AC-F05-01: Dado que há conexão ativa, quando a tela de modos abre, então a PoC deve ler `general.online`, `general.local_identification`, `online_client.server_id`, sessão e informações do sistema quando disponíveis.
- AC-F05-02: Dado `online=0` e `local_identification=1`, quando o estado é resolvido, então o modo deve ser apresentado como Standalone.
- AC-F05-03: Dado `online=1` e `local_identification=1`, quando o estado é resolvido, então o modo deve ser apresentado como Pro.
- AC-F05-04: Dado `online=1` e `local_identification=0`, quando o estado é resolvido, então o modo deve ser apresentado como Enterprise.
- AC-F05-05: Dado aplicação de Standalone, quando há sessão/conexão, então a PoC deve enviar `set-configuration` com `online=0` e `local_identification=1`.
- AC-F05-06: Dado aplicação de Pro ou Enterprise, quando não há `server_id` existente, então a PoC deve criar ou resolver o servidor online antes de enviar `set-configuration`.
- AC-F05-07: Dado upgrade de licença, quando senha/licença é enviada, então a PoC deve chamar o endpoint oficial de upgrade e exibir a resposta, sem garantir ativação real sem equipamento/licença compatível.

### F06 - Monitor, callbacks e eventos oficiais

Critérios:

- AC-F06-01: Dado callback com IP permitido, tamanho permitido e shared key válida quando exigida, quando o endpoint recebe a requisição, então a PoC deve persistir evento com `EventType`, `DeviceId`, `UserId`, payload bruto e status `received`.
- AC-F06-02: Dado shared key obrigatória ausente ou inválida, quando o callback chega, então a PoC deve rejeitar a requisição e não persistir evento.
- AC-F06-03: Dado payload acima de `CallbackSecurity:MaxBodyBytes`, quando o callback chega, então a PoC deve responder rejeição por tamanho e não persistir evento.
- AC-F06-04: Dado corpo binário ou imagem, quando o callback é aceito, então a PoC deve salvar o conteúdo em Base64.
- AC-F06-05: Dado limpeza de eventos, quando a confirmação textual não corresponde a `LIMPAR EVENTOS`, então a PoC deve bloquear a exclusão.
- AC-F06-06: Dado limpeza de eventos com confirmação correta, quando executada, então a PoC deve remover somente o histórico local de eventos.

### F07 - Push: fila, polling e resultado

Critérios:

- AC-F07-01: Dado payload JSON válido, quando o usuário enfileira comando no Push Center, então a PoC deve criar `PushCommandLocal` com status `pending`.
- AC-F07-02: Dado payload JSON inválido, quando o usuário tenta enfileirar comando, então a PoC deve rejeitar antes de persistir.
- AC-F07-03: Dado comando `pending` elegível para o dispositivo, quando o equipamento chama `GET /push`, então a PoC deve retornar o payload como JSON e marcar o comando como `delivered`.
- AC-F07-04: Dado ausência de comando elegível, quando o equipamento chama `GET /push`, então a PoC deve retornar `{}`.
- AC-F07-05: Dado `POST /result` com `command_id` existente, quando o resultado chega, então a PoC deve atualizar payload, raw JSON, status e `UpdatedAt`.
- AC-F07-06: Dado `POST /result` sem `command_id`, quando o resultado chega, então a PoC deve criar registro do tipo `result` com status `completed` se nenhum status for informado.
- AC-F07-07: Dado evento legado em `POST /Push/Receive` com JSON inválido, quando o corpo é recebido, então a PoC deve persistir o corpo bruto como `legacy_push_event` com status `received`.
- AC-F07-08: Dado limpeza da fila Push, quando a confirmação textual não corresponde a `LIMPAR PUSH`, então a PoC deve bloquear a exclusão.
- AC-F07-09: Dado `POST /result` ou `POST /Push/Receive` com `Idempotency-Key` ou `idempotency_key`, quando a mesma chave é reenviada, então a PoC deve atualizar o mesmo registro em vez de criar duplicata.

### F08 - Privacidade, segurança de ingress e runtime

Critérios:

- AC-F08-01: Dado ambiente diferente de `Development`, quando a aplicação inicia sem `AllowedHosts` explícito, então o startup deve falhar para evitar exposição ampla.
- AC-F08-02: Dado ambiente diferente de `Development`, quando `CallbackSecurity:RequireSharedKey` não está habilitado ou `SharedKey` está ausente, então o startup deve falhar ou bloquear a execução conforme a validação de configuração.
- AC-F08-03: Dado payload com dado pessoal ou sensível, quando for persistido em Monitor ou Push, então a PoC deve tratá-lo como dado local sensível e não deve versionar exemplos reais.
- AC-F08-04: Dado log ou mensagem ao usuário, quando ocorre falha técnica, então a PoC deve preferir mensagem segura/sanitizada e registrar detalhe técnico apenas no log.
- AC-F08-05: Dado rajada de callbacks ou push acima do limite configurado, quando a origem excede `CallbackSecurity:RateLimit`, então a PoC deve responder `429` sem persistir novo payload.
- AC-F08-06: Dado um segredo acidental com padrão reconhecido, quando a CI executa, então o secret scan deve falhar antes da auditoria de release.

### F09 - Banco local e evolução de schema

Critérios:

- AC-F09-01: Dado banco SQLite local inexistente, quando a aplicação inicia, então as migrations devem criar o schema local necessário.
- AC-F09-02: Dado banco SQLite local já existente da PoC, quando a migration inicial roda, então a criação idempotente deve preservar tabelas existentes.
- AC-F09-03: Dado alteração futura de schema, quando implementada, então deve haver migration versionada ou script SQL revisável.
- AC-F09-04: Dado tabelas `MonitorEvents` e `PushCommands`, quando armazenam payloads reais, então devem seguir a política de privacidade e retenção documentada.

## Requisitos rastreáveis

### REQ-001 - Conexão e sessão Control iD

- Descrição: permitir conexão operacional, login, validação e encerramento de sessão com o equipamento.
- Fonte/evidência: `README.md`, `Controllers/AuthController.cs`, `Controllers/SessionController.cs`.
- Prioridade: Crítica.
- Fluxo associado: F01.
- Regra de negócio associada: endpoints autenticados dependem de dispositivo e sessão ativos.
- Critérios de aceite: AC-F01-01 a AC-F01-06.
- Dados válidos: endereço de dispositivo em sessão, credenciais informadas, resposta com `session`.
- Dados inválidos: ausência de dispositivo, resposta sem `session`, sessão expirada.
- Estados esperados: sem dispositivo, autenticado, sessão válida, sessão expirada, sessão limpa.
- Erros esperados: falha de login, resposta inesperada, falha de validação de sessão.
- Permissões esperadas: usuário com acesso à UI da PoC; não há RBAC documentado.
- Testes existentes: não há teste automatizado direto para `AuthController`/`SessionController`.
- Testes ausentes: controller tests para login sem dispositivo, resposta sem sessão, logout cross-origin e validação de sessão.

### REQ-002 - Catálogo e invocação da API oficial

- Descrição: expor catálogo assistido de endpoints oficiais e invocação segura contra o equipamento.
- Fonte/evidência: `Controllers/OfficialApiController.cs`, `Services/ControlIDApi/README.md`, `Services/ControlIDApi/*`.
- Prioridade: Alta.
- Fluxo associado: F02.
- Regra de negócio associada: callbacks/push são servidos pela PoC e não devem ser invocados como chamada outbound ao equipamento.
- Critérios de aceite: AC-F02-01 a AC-F02-05.
- Dados válidos: endpoint catalogado, query/body compatíveis, sessão ativa.
- Dados inválidos: endpoint inexistente, JSON inválido, ausência de sessão.
- Estados esperados: endpoint selecionado, chamada pronta, resposta textual, resposta binária, erro sanitizado.
- Erros esperados: endpoint inválido, timeout, falha HTTP, payload inválido.
- Permissões esperadas: usuário com acesso à UI técnica da PoC; sem RBAC documentado.
- Testes existentes: `OfficialApiContractDocumentationServiceTests.cs`, `OfficialApiBinaryFileResultFactoryTests.cs`.
- Testes ausentes: integração/controller para bloqueio de endpoints servidos pela PoC, JSON inválido e ausência de sessão.

### REQ-003 - Operação de objetos oficiais

- Descrição: permitir CRUD técnico de objetos oficiais com validação de JSON e proteção para destruição.
- Fonte/evidência: `Controllers/OfficialObjectsController.cs`, `Views/OfficialObjects/Index.cshtml`, `Helpers/HighImpactOperationGuard.cs`.
- Prioridade: Alta.
- Fluxo associado: F03.
- Regra de negócio associada: `destroy-objects` exige confirmação `DESTROY <objeto>`.
- Critérios de aceite: AC-F03-01 a AC-F03-05.
- Dados válidos: objeto do catálogo, JSON de filtro/valores válido, confirmação correta para destroy.
- Dados inválidos: JSON inválido, objeto não selecionado, confirmação ausente/incorreta.
- Estados esperados: objeto selecionado, erro local, resposta oficial exibida.
- Erros esperados: JSON inválido, sem conexão, falha oficial.
- Permissões esperadas: usuário com acesso à UI técnica; sem RBAC documentado.
- Testes existentes: `HighImpactOperationGuardTests.cs`.
- Testes ausentes: controller tests para JSON inválido, confirmação de destroy e não chamada ao equipamento.

### REQ-004 - Operações administrativas de alto impacto

- Descrição: proteger reboot, rede, modo update, reset de fábrica, remoção de admins e limpezas locais contra ação acidental.
- Fonte/evidência: `Controllers/SystemController.cs`, `Controllers/OfficialEventsController.cs`, `Controllers/PushCenterController.cs`, `Helpers/HighImpactOperationGuard.cs`.
- Prioridade: Crítica.
- Fluxo associado: F04.
- Regra de negócio associada: operações destrutivas ou de alto impacto exigem frase de confirmação.
- Critérios de aceite: AC-F04-01 a AC-F04-04, AC-F06-05, AC-F07-08.
- Dados válidos: frase exata esperada, conexão/sessão quando aplicável.
- Dados inválidos: frase vazia, frase incorreta, sem conexão.
- Estados esperados: bloqueado por confirmação, executado, erro oficial, histórico local preservado ou limpo.
- Erros esperados: confirmação requerida, sem conexão, falha oficial.
- Permissões esperadas: usuário com acesso à UI; sem RBAC documentado.
- Testes existentes: `HighImpactOperationGuardTests.cs`, `OfficialEventsControllerTests.cs`, `PushCenterControllerTests.cs`.
- Testes ausentes: controller tests para todas as ações de `SystemController`.

### REQ-005 - Modos Standalone, Pro e Enterprise

- Descrição: detectar e aplicar perfis de operação do equipamento com base em configurações oficiais.
- Fonte/evidência: `docs/operation-modes-implementation.md`, `Controllers/OperationModesController.cs`, `Services/OperationModes/*`.
- Prioridade: Alta.
- Fluxo associado: F05.
- Regra de negócio associada: o modo é inferido por `general.online` + `general.local_identification`; a fonte de verdade é o equipamento.
- Critérios de aceite: AC-F05-01 a AC-F05-07.
- Dados válidos: `online` e `local_identification` como `0`/`1`, `server_id` existente ou dados para criação, senha de licença quando aplicável.
- Dados inválidos: combinação desconhecida, ausência de conexão, licença inválida, falha ao criar servidor online.
- Estados esperados: Standalone, Pro, Enterprise, desconhecido/indisponível, aplicado com resposta oficial.
- Erros esperados: falha de leitura, falha de sessão, falha ao aplicar modo, licença não aceita.
- Permissões esperadas: usuário com acesso operacional à UI; sem RBAC documentado.
- Testes existentes: `OperationModesPayloadFactoryTests.cs`, `OperationModesProfileResolverTests.cs`.
- Testes ausentes: controller/integration tests com stub para transições, server_id e resposta oficial; E2E com equipamento real.

### REQ-006 - Monitor e callbacks

- Descrição: receber, validar, persistir e consultar eventos enviados pelo equipamento.
- Fonte/evidência: `docs/monitor-implementation.md`, `Controllers/OfficialCallbacksController.cs`, `Services/Callbacks/*`, `Services/Database/MonitorEventRepository.cs`.
- Prioridade: Crítica.
- Fluxo associado: F06.
- Regra de negócio associada: callbacks devem passar por validação de tamanho, IP e shared key quando configurado.
- Critérios de aceite: AC-F06-01 a AC-F06-06.
- Dados válidos: body textual/binário dentro do limite, IP permitido, shared key correta quando exigida.
- Dados inválidos: shared key ausente/incorreta, IP bloqueado, payload acima do limite.
- Estados esperados: `received`, rejeitado, persistido, limpo localmente.
- Erros esperados: `401/403` conforme política de segurança, `413` por tamanho, erro de persistência.
- Permissões esperadas: origem HTTP autorizada por IP/shared key; usuário UI para consulta/limpeza.
- Testes existentes: `CallbackSecurityEvaluatorTests.cs`, `CallbackRequestBodyReaderTests.cs`, `CallbackIngressServiceTests.cs`, `OfficialEventsControllerTests.cs`.
- Testes ausentes: teste E2E com equipamento real e URL pública; testes de UI para listagem/detalhe.

### REQ-007 - Push

- Descrição: manter fila persistida para comandos buscados pelo equipamento e registrar resultados.
- Fonte/evidência: `docs/push-implementation.md`, `Controllers/PushCenterController.cs`, `Controllers/PushController.cs`, `Services/Database/PushCommandRepository.cs`.
- Prioridade: Crítica.
- Fluxo associado: F07.
- Regra de negócio associada: fila usa estados `pending`, `delivered`, `completed` e `received`; endpoints Push usam a mesma segurança de ingress dos callbacks.
- Critérios de aceite: AC-F07-01 a AC-F07-09.
- Dados válidos: JSON de payload, `device_id`, `command_id` GUID, status opcional.
- Dados inválidos: JSON inválido na fila UI, shared key inválida quando exigida, payload acima do limite.
- Estados esperados: `pending`, `delivered`, `completed`, `received`, status livre recebido em `/result`.
- Erros esperados: rejeição por segurança, erro de persistência, fila vazia retorna `{}`.
- Permissões esperadas: usuário UI para enfileirar/limpar; origem HTTP autorizada para `/push`, `/result` e `/Push/Receive`.
- Testes existentes: `PushCenterControllerTests.cs`, `PushControllerTests.cs`, `PushCommandRepositoryTests.cs`, `PushIdempotencyKeyResolverTests.cs`.
- Testes ausentes: concorrência em múltiplos polls simultâneos, E2E com equipamento real e smoke cobrindo autenticação de shared key em ambiente exposto.

### REQ-008 - Privacidade, segurança e runtime fora de desenvolvimento

- Descrição: impedir exposição acidental de callbacks/push e tratar payloads locais como dados sensíveis.
- Fonte/evidência: `README.md`, `docs/privacy-and-data-retention.md`, `Program.cs`, `Options/CallbackSecurityOptions.cs`.
- Prioridade: Crítica.
- Fluxo associado: F08.
- Regra de negócio associada: fora de `Development`, `AllowedHosts`, `RequireSharedKey` e `SharedKey` devem estar configurados.
- Critérios de aceite: AC-F08-01 a AC-F08-04.
- Dados válidos: `AllowedHosts` explícito, `RequireSharedKey=true`, segredo fora do repositório, IPs permitidos quando aplicável.
- Dados inválidos: wildcard em ambiente exposto, segredo ausente, payload real versionado.
- Estados esperados: startup permitido, startup bloqueado, callback aceito/rejeitado.
- Erros esperados: falha de configuração no startup, rejeição de ingress.
- Permissões esperadas: gestão de configuração por operador técnico; segredo nunca versionado.
- Testes existentes: `CallbackSecurityEvaluatorTests.cs`; auditoria NuGet, secret scan na CI e docs.
- Testes ausentes: teste automatizado de startup para ambientes não Development e teste de middleware para rate limit.

### REQ-009 - Banco local e schema

- Descrição: manter schema local versionado e compatível com bancos SQLite da PoC.
- Fonte/evidência: `docs/database-and-runtime-state.md`, `Data/Migrations/*`, `Program.cs`.
- Prioridade: Alta.
- Fluxo associado: F09.
- Regra de negócio associada: alterações futuras de schema devem ser versionadas e revisáveis.
- Critérios de aceite: AC-F09-01 a AC-F09-04.
- Dados válidos: SQLite local, migrations EF Core, payloads locais persistidos.
- Dados inválidos: mudança de schema sem migration/script, exclusão sem plano de backup.
- Estados esperados: banco criado, banco existente preservado, migration registrada.
- Erros esperados: falha de acesso ao SQLite, schema incompatível pré-existente.
- Permissões esperadas: escrita local no workspace; sem credencial de produção.
- Testes existentes: `SqliteTestDatabase.cs`, `PushCommandRepositoryTests.cs`, `CallbackIngressServiceTests.cs`, validação de script de migration.
- Testes ausentes: teste automatizado aplicando migration em banco legado parcial.

## Matriz de rastreabilidade

| Requisito | Fluxo | Código | Teste existente | Critérios | Risco | Mitigação |
| --- | --- | --- | --- | --- | --- | --- |
| REQ-001 | F01 | `AuthController`, `SessionController` | Ausente direto | AC-F01-01..06 | Sessão inválida liberar ação autenticada | Adicionar controller tests e smoke com stub |
| REQ-002 | F02 | `OfficialApiController`, `Services/ControlIDApi/*` | Contract docs e binary result tests | AC-F02-01..05 | Chamar endpoint errado ou expor resposta insegura | Testes de controller e contrato por endpoint crítico |
| REQ-003 | F03 | `OfficialObjectsController`, `HighImpactOperationGuard` | Guard tests | AC-F03-01..05 | Destruição acidental de registros remotos | Testar bloqueio de destroy e JSON inválido |
| REQ-004 | F04 | `SystemController`, `OfficialEventsController`, `PushCenterController` | Guard, PushCenter e OfficialEvents tests | AC-F04-01..04 | Reboot/reset/rede por erro humano | Frase de confirmação e testes por ação |
| REQ-005 | F05 | `OperationModesController`, `OperationModesPayloadFactory`, `OperationModesProfileResolver` | Payload/profile resolver tests | AC-F05-01..07 | Aplicar modo incorreto no equipamento | Testes de controller com stub e roteiro manual |
| REQ-006 | F06 | `OfficialCallbacksController`, `CallbackIngressService`, `CallbackSecurityEvaluator`, `MonitorEventRepository` | Callback security/body/ingress tests | AC-F06-01..06 | Persistir payload não autorizado ou perder evento crítico | Shared key/IP/limite + teste E2E |
| REQ-007 | F07 | `PushCenterController`, `PushController`, `PushCommandRepository` | Push controller/repository/idempotency tests | AC-F07-01..09 | Entrega duplicada, payload inválido ou fila apagada | Teste de concorrência e smoke com equipamento/stub |
| REQ-008 | F08 | `Program.cs`, `CallbackSecurityOptions`, docs de privacidade | Security evaluator + secret scan | AC-F08-01..06 | Exposição pública sem shared key ou vazamento de dados | Startup tests, rate limit middleware test e revisão LGPD |
| REQ-009 | F09 | `Data/Migrations/*`, `IntegracaoControlIDContext`, `Program.cs` | SQLite em memória e script validado | AC-F09-01..04 | Banco local incompatível ou schema não rastreado | Migration idempotente e teste de banco legado |

## Definition of Ready

Uma mudança só está pronta para implementação quando:

- o objetivo funcional está claro e vinculado a um requisito `REQ-*`;
- o fluxo afetado está identificado;
- critérios de aceite `AC-*` existem ou foram adicionados;
- dados válidos, inválidos e estados esperados estão conhecidos;
- dependências internas e externas estão mapeadas;
- impactos de API, banco, UI e docs foram avaliados;
- riscos de segurança, privacidade e operação foram classificados;
- comportamento ambíguo está marcado como lacuna, não como requisito fechado;
- testes existentes e testes ausentes estão listados.

## Definition of Done

Uma mudança só está concluída quando:

- código ou documentação foram implementados no escopo aprovado;
- requisitos e critérios de aceite associados foram atendidos;
- contratos públicos foram preservados ou versionados;
- testes relevantes foram criados ou ajustados;
- build, format, testes e auditorias aplicáveis foram executados;
- segurança de ingress, secrets e mensagens de erro foi avaliada;
- LGPD/privacidade e retenção local foram consideradas quando houver payload pessoal/sensível;
- documentação funcional e operacional foi atualizada;
- observabilidade/logs foram considerados para falhas relevantes;
- riscos residuais e testes não executados foram documentados;
- arquivos alterados foram listados no fechamento da tarefa.

## Lacunas de aceite e validação

| Lacuna | Impacto | Prioridade | Recomendação |
| --- | --- | --- | --- |
| Autenticação/sessão sem testes de controller | Regressões podem quebrar login/validação sem alerta unitário | Alta | Criar testes para `AuthController` e `SessionController` |
| `SystemController` sem testes por operação crítica | Confirmações podem regredir em ações remotas perigosas | Alta | Testar reboot, rede, factory reset, recovery e delete admins com mock do serviço oficial |
| Operation Modes sem teste de controller com stub | Payloads têm teste, mas orquestração pode regredir | Alta | Criar integração com stub para Standalone, Pro e Enterprise |
| Push sem teste de concorrência | Dois polls simultâneos podem disputar o mesmo comando | Média | Testar transação/locking ou implementar controle explícito antes de produção |
| Status de `/result` é livre | Relatórios podem ficar inconsistentes | Média | Definir enum/normalização se o produto exigir relatórios formais |
| RBAC/permissões de UI não documentadas | Usuário com acesso à PoC pode acionar telas críticas | Alta | Definir papéis ou declarar escopo restrito de laboratório |
| Retenção de dados não automatizada | Payload sensível pode ficar no SQLite indefinidamente | Alta | Implementar rotina de purge/backup quando houver requisito operacional |
| Smoke/E2E dependente de equipamento real | Aceite final de integração não é totalmente automatizado | Crítica | Manter roteiro manual com modelo, firmware, licença, rede e URL pública |
| Secret scan automatizado com heurística local | Falsos negativos ainda são possíveis sem ferramenta externa dedicada | Média | Revisar achados manualmente e considerar ferramenta especializada antes de produção |

## Estratégia de validação

Unitária:

- validar helpers de confirmação, sanitização e factories de payload;
- validar resolução de modos por combinações `online`/`local_identification`;
- validar segurança de callback por IP, shared key e tamanho;
- validar repositórios com SQLite em memória.

Integração:

- controller tests para login/sessão, objetos oficiais, operações administrativas e modos;
- testes de migration aplicando schema em banco vazio e banco legado parcial;
- testes de Push com polling e resultado usando repositório real em SQLite.

Contrato:

- snapshots ou asserts estruturados para endpoints catalogados em `OfficialApiCatalogService`;
- validar exemplos de payload documentados contra payloads gerados por factories;
- garantir que endpoints servidos pela PoC não sejam tratados como chamadas outbound.

E2E/smoke:

- manter `tools/smoke-localhost.ps1` com stub local para happy path e edge cases;
- adicionar smoke com shared key obrigatória;
- executar roteiro manual com equipamento real para callbacks, push e modos.

Manual guiada:

- validar conexão, login, sessão e logout;
- aplicar cada modo em bancada controlada;
- enviar callbacks reais e confirmar persistência;
- enfileirar Push, aguardar `GET /push` e validar `POST /result`;
- testar confirmações de alto impacto sem executar ação real sempre que possível;
- registrar modelo, firmware, IP, URL pública, licença e limitações observadas.

## Recomendações para próximas implementações/testes

1. Criar testes de controller para `AuthController`, `SessionController`, `OfficialObjectsController`, `SystemController` e `OperationModesController`.
2. Validar a heurística do secret scan com amostras controladas antes de qualquer uso com credenciais reais.
3. Definir política operacional para RBAC ou declarar formalmente que a PoC é de uso restrito em laboratório.
4. Criar rotina opcional de retenção/purge para `MonitorEvents` e `PushCommands`.
5. Expandir smoke local para shared key obrigatória, callbacks oficiais e ciclo Push completo com stub.
6. Criar runbook de homologação física com equipamento real, incluindo versão de firmware, modo, rede e evidências esperadas.
