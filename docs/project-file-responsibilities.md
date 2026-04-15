# Responsabilidades dos arquivos do projeto

Este documento resume a responsabilidade dos arquivos versionados da PoC `Integracao.ControlID.PoC`.

O objetivo e servir como um mapa rapido de navegacao para quem quiser entender a solucao, localizar uma funcionalidade ou contribuir com o projeto sem precisar descobrir a estrutura apenas pelo codigo.

Observacoes de escopo:

- Arquivos gerados em `bin/`, `obj/`, banco SQLite local, logs e artefatos temporarios nao fazem parte deste inventario.
- Bibliotecas vendorizadas em `wwwroot/lib/` foram agrupadas por familia, porque incluem variacoes minificadas, sourcemaps e licencas sem regra de negocio da PoC.
- As descricoes abaixo sao intencionais e resumidas; para detalhes de comportamento, consulte o codigo e os testes relacionados.

## Raiz da solucao

| Arquivo | Responsabilidade |
| --- | --- |
| `.editorconfig` | Padroniza convencoes basicas de edicao, formatacao e estilo entre IDEs. |
| `.gitignore` | Define arquivos e pastas que nao devem ser versionados, como builds, logs e artefatos locais. |
| `Directory.Build.props` | Centraliza propriedades comuns de build para os projetos .NET da solucao. |
| `Integracao.ControlID.PoC.csproj` | Define o projeto ASP.NET Core MVC principal, dependencias NuGet e configuracoes de compilacao. |
| `Integracao.ControlID.PoC.sln` | Agrupa o projeto principal, testes e utilitarios em uma unica solucao. |
| `Program.cs` | Configura o bootstrap da aplicacao, DI, middlewares, banco local, rotas MVC, Serilog e servicos da PoC. |
| `README.md` | Apresenta a PoC, stack, setup local, testes, observabilidade e links de referencia. |
| `appsettings.json` | Configuracoes base da aplicacao, API Control iD, banco, logs e seguranca de callbacks. |
| `appsettings.Development.json` | Sobrescritas de configuracao para execucao local em ambiente de desenvolvimento. |

## Properties

| Arquivo | Responsabilidade |
| --- | --- |
| `Properties/launchSettings.json` | Define perfis locais de execucao, URLs, portas e variaveis usadas pelo `dotnet run`/IDE. |

## Controllers

Os controllers coordenam as rotas MVC, recebem a entrada da UI, acionam servicos/repositorios e retornam views ou respostas auxiliares.

| Arquivo | Responsabilidade |
| --- | --- |
| `Controllers/AccessLogsController.cs` | Fluxos de listagem, detalhe e remocao de logs de acesso persistidos localmente. |
| `Controllers/AccessRulesController.cs` | CRUD e visualizacao das regras de acesso locais usadas pela PoC. |
| `Controllers/AdvancedOfficialController.cs` | Telas e execucoes de cenarios oficiais avancados, como captura, exportacao e comandos especiais. |
| `Controllers/AuthController.cs` | Fluxos de login, registro, logout, status e troca de senha relacionados a autenticacao no equipamento. |
| `Controllers/BiometricTemplatesController.cs` | Operacoes de listagem, detalhe, edicao e remocao de templates biometricos. |
| `Controllers/CardsController.cs` | Operacoes de gerenciamento local de cartoes vinculados a usuarios/acesso. |
| `Controllers/CatraController.cs` | Fluxos especificos de catraca, eventos e abertura remota. |
| `Controllers/ChangeLogsController.cs` | Consulta e remocao de registros de alteracao sincronizados ou armazenados localmente. |
| `Controllers/ConfigController.cs` | Gerenciamento, diagnostico e visualizacao de configuracoes do equipamento e da PoC. |
| `Controllers/DevicesController.cs` | CRUD e visualizacao de dispositivos cadastrados no contexto local. |
| `Controllers/DocumentedFeaturesController.cs` | Exibe o consolidado de funcionalidades documentadas e implementadas na PoC. |
| `Controllers/ErrorsController.cs` | Lista e detalha erros registrados durante chamadas, integracoes ou processamento local. |
| `Controllers/GroupsController.cs` | CRUD e visualizacao de grupos locais relacionados aos fluxos de acesso. |
| `Controllers/HardwareController.cs` | Aciona e apresenta recursos de hardware, como GPIO, rele, porta e validacoes biometricas. |
| `Controllers/HomeController.cs` | Monta o dashboard inicial e indicadores principais da PoC. |
| `Controllers/LogoController.cs` | Gerencia upload, consulta e remocao de logos/imagens locais. |
| `Controllers/MediaController.cs` | Gerencia midias como fotos e videos usados nos fluxos da PoC. |
| `Controllers/MonitorWebhookController.cs` | Recebe, registra e apresenta eventos recebidos por webhook/callback. |
| `Controllers/OfficialApiController.cs` | Exibe catalogo oficial, contratos e invocacao generica dos endpoints documentados. |
| `Controllers/OfficialCallbacksController.cs` | Implementa endpoints de callbacks oficiais e processamento de payloads recebidos. |
| `Controllers/OfficialEventsController.cs` | Centraliza fluxos ligados a eventos oficiais da API Control iD. |
| `Controllers/OfficialObjectsController.cs` | Apresenta operacoes oficiais sobre objetos, payloads e contratos relacionados. |
| `Controllers/OperationModesController.cs` | Exibe e simula os modos de operacao Standalone, Pro e Enterprise. |
| `Controllers/ProductSpecificController.cs` | Coordena funcionalidades especificas por produto/modelo da familia Control iD. |
| `Controllers/PushCenterController.cs` | Organiza a central de push, filas e comandos pendentes. |
| `Controllers/PushController.cs` | Fluxos de eventos push, consulta e enfileiramento de comandos. |
| `Controllers/QRCodesController.cs` | CRUD e visualizacao de QR Codes persistidos localmente. |
| `Controllers/RemoteActionsController.cs` | Execucao e acompanhamento de acoes remotas como autorizacao e enrolamento. |
| `Controllers/SessionController.cs` | Gerenciamento de sessoes locais e status de autenticacao/conexao com o equipamento. |
| `Controllers/SystemController.cs` | Operacoes de sistema, rede, VPN, hash de senha e acoes administrativas. |
| `Controllers/UsersController.cs` | CRUD, visualizacao e payloads de usuarios da PoC. |
| `Controllers/WorkspaceController.cs` | Exibe o workspace/explorador operacional para navegar pelos recursos implementados. |

## Data

| Arquivo | Responsabilidade |
| --- | --- |
| `Data/IntegracaoControlIDContext.cs` | DbContext do Entity Framework Core; mapeia entidades locais, indices, relacionamentos e configuracoes SQLite. |

## Helpers

| Arquivo | Responsabilidade |
| --- | --- |
| `Helpers/ApiResponseHelper.cs` | Utilitarios para normalizar respostas e mensagens vindas da API/servicos. |
| `Helpers/CryptoHelper.cs` | Apoio criptografico, especialmente para hashes e transformacoes relacionadas a seguranca. |
| `Helpers/FileHelper.cs` | Rotinas auxiliares para manipulacao de arquivos usados pela PoC. |
| `Helpers/HttpHelper.cs` | Funcoes de apoio para chamadas HTTP, montagem de requests e leitura de respostas. |
| `Helpers/NavigationPresentationHelper.cs` | Centraliza detalhes de apresentacao usados pela navegacao da UI. |
| `Helpers/ProductSpecificPresentationHelper.cs` | Apoia a exibicao de conteudos especificos de produto na interface. |
| `Helpers/SecurityTextHelper.cs` | Padroniza textos e mascaramentos ligados a informacoes sensiveis. |
| `Helpers/SessionHelper.cs` | Auxilia leitura, escrita e interpretacao de dados de sessao no contexto web. |

## Logging

| Arquivo | Responsabilidade |
| --- | --- |
| `Logging/SeriLogConfiguration.cs` | Configura sinks, formato e politicas de logging com Serilog. |
| `Logging/SeriLogEvents.cs` | Centraliza identificadores/eventos de log usados para rastreabilidade. |

## Mappings

| Arquivo | Responsabilidade |
| --- | --- |
| `Mappings/ModelMappings.cs` | Converte modelos da API/DOMINIO para entidades locais ou estruturas equivalentes. |
| `Mappings/ViewModelMappings.cs` | Converte modelos e entidades em ViewModels prontos para as telas Razor. |

## Middlewares

| Arquivo | Responsabilidade |
| --- | --- |
| `Middlewares/ApiSessionMiddleware.cs` | Garante contexto minimo de sessao/API durante o pipeline HTTP. |
| `Middlewares/ExceptionHandlingMiddleware.cs` | Captura excecoes nao tratadas e padroniza a resposta/registro de erro. |
| `Middlewares/RequestLoggingMiddleware.cs` | Registra informacoes de requests para observabilidade local. |
| `Middlewares/SecurityHeadersMiddleware.cs` | Aplica cabecalhos de seguranca HTTP nas respostas da aplicacao. |

## Models/ControlIDApi

Modelos que representam contratos, payloads e respostas proximas da API Control iD.

| Arquivo | Responsabilidade |
| --- | --- |
| `Models/ControlIDApi/AccessLog.cs` | Representa um registro de acesso retornado ou enviado no contexto da API. |
| `Models/ControlIDApi/AccessRule.cs` | Representa regras de permissao/acesso no formato de integracao. |
| `Models/ControlIDApi/BiometricTemplate.cs` | Representa templates biometricos trafegados na API. |
| `Models/ControlIDApi/Card.cs` | Representa cartoes de acesso vinculados a usuarios. |
| `Models/ControlIDApi/CatraEvent.cs` | Representa eventos especificos de catraca. |
| `Models/ControlIDApi/ChangeLog.cs` | Representa logs de alteracao/sincronizacao do equipamento. |
| `Models/ControlIDApi/ConfigGroup.cs` | Agrupa configuracoes retornadas ou aplicadas no equipamento. |
| `Models/ControlIDApi/Device.cs` | Representa dados de identificacao e configuracao de dispositivo. |
| `Models/ControlIDApi/ErrorInfo.cs` | Modela informacoes de erro retornadas ou registradas pela integracao. |
| `Models/ControlIDApi/GpioState.cs` | Representa estado de GPIO/entradas e saidas fisicas. |
| `Models/ControlIDApi/Group.cs` | Representa grupos utilizados para organizacao de acesso. |
| `Models/ControlIDApi/HardwareStatus.cs` | Representa o status geral de componentes fisicos do equipamento. |
| `Models/ControlIDApi/HashPasswordResponse.cs` | Representa resposta de geracao/validacao de hash de senha. |
| `Models/ControlIDApi/Logo.cs` | Representa dados de logo/imagem do equipamento. |
| `Models/ControlIDApi/MonitorEvent.cs` | Representa eventos monitorados em tempo real ou via callback. |
| `Models/ControlIDApi/OfficialApiEndpointDefinition.cs` | Define metadados de endpoints oficiais, parametros, metodo HTTP e documentacao visual. |
| `Models/ControlIDApi/OfficialApiInvocationResult.cs` | Representa o resultado de uma invocacao generica da API oficial. |
| `Models/ControlIDApi/Photo.cs` | Representa foto ou imagem associada a usuario/midia. |
| `Models/ControlIDApi/PushCommand.cs` | Representa comandos push enfileirados ou recebidos. |
| `Models/ControlIDApi/QRCode.cs` | Representa QR Codes de acesso. |
| `Models/ControlIDApi/RemoteAction.cs` | Representa uma acao remota solicitada ao equipamento. |
| `Models/ControlIDApi/RemoteActionResult.cs` | Representa o retorno de execucao de uma acao remota. |
| `Models/ControlIDApi/SessionInfo.cs` | Representa dados de sessao/autenticacao com o equipamento. |
| `Models/ControlIDApi/SystemInfo.cs` | Representa informacoes de sistema, rede e ambiente do equipamento. |
| `Models/ControlIDApi/User.cs` | Representa usuario no formato usado pelos fluxos de integracao. |

## Models/Database

Entidades persistidas no SQLite local para historico, cache operacional, simulacoes e suporte a UI.

| Arquivo | Responsabilidade |
| --- | --- |
| `Models/Database/AccessLogLocal.cs` | Entidade local para logs de acesso. |
| `Models/Database/AccessRuleLocal.cs` | Entidade local para regras de acesso. |
| `Models/Database/BiometricTemplateLocal.cs` | Entidade local para templates biometricos. |
| `Models/Database/CardLocal.cs` | Entidade local para cartoes. |
| `Models/Database/ChangeLogLocal.cs` | Entidade local para logs de alteracao. |
| `Models/Database/ConfigLocal.cs` | Entidade local para configuracoes. |
| `Models/Database/DeviceLocal.cs` | Entidade local para dispositivos. |
| `Models/Database/GroupLocal.cs` | Entidade local para grupos. |
| `Models/Database/LogLocal.cs` | Entidade local generica para registros de log/auditoria. |
| `Models/Database/LogoLocal.cs` | Entidade local para logos/imagens. |
| `Models/Database/MonitorEventLocal.cs` | Entidade local para eventos monitorados por push/webhook. |
| `Models/Database/PhotoLocal.cs` | Entidade local para fotos e imagens. |
| `Models/Database/PushCommandLocal.cs` | Entidade local para comandos push. |
| `Models/Database/QRCodeLocal.cs` | Entidade local para QR Codes. |
| `Models/Database/SessionLocal.cs` | Entidade local para sessoes e estado de autenticacao. |
| `Models/Database/SyncLocal.cs` | Entidade local para estado de sincronizacao. |
| `Models/Database/UserLocal.cs` | Entidade local para usuarios. |

## Monitor

| Arquivo | Responsabilidade |
| --- | --- |
| `Monitor/MonitorEventHandler.cs` | Processa eventos monitorados e encaminha para persistencia/fila. |
| `Monitor/MonitorEventMapper.cs` | Converte payloads de monitoramento para modelos internos/localmente persistiveis. |
| `Monitor/MonitorEventQueue.cs` | Mantem fila em memoria para eventos recebidos e processamento assincrono. |

## Options

| Arquivo | Responsabilidade |
| --- | --- |
| `Options/CallbackSecurityOptions.cs` | Representa as configuracoes de seguranca aplicadas aos callbacks/webhooks. |

## Services/Callbacks

| Arquivo | Responsabilidade |
| --- | --- |
| `Services/Callbacks/CallbackIngressService.cs` | Orquestra o recebimento, validacao e persistencia de callbacks. |
| `Services/Callbacks/CallbackRequestBodyReader.cs` | Le o corpo bruto das requisicoes de callback de forma reutilizavel. |
| `Services/Callbacks/CallbackSecurityEvaluator.cs` | Avalia regras de seguranca, chave compartilhada e origem permitida dos callbacks. |

## Services/ControlIDApi

| Arquivo | Responsabilidade |
| --- | --- |
| `Services/ControlIDApi/IOfficialControlIdApiService.cs` | Contrato da camada de cliente HTTP para chamadas oficiais a API Control iD. |
| `Services/ControlIDApi/OfficialApiBinaryFileResultFactory.cs` | Monta respostas de arquivo/binario para resultados oficiais que precisam de download. |
| `Services/ControlIDApi/OfficialApiBodyParameterStrategy.cs` | Define estrategia de montagem de parametros enviados no corpo da requisicao. |
| `Services/ControlIDApi/OfficialApiCatalogService.cs` | Disponibiliza o catalogo navegavel de endpoints oficiais implementados/documentados. |
| `Services/ControlIDApi/OfficialApiContractDocumentationService.cs` | Gera a apresentacao dos contratos oficiais de entrada/saida. |
| `Services/ControlIDApi/OfficialApiDocumentationSeedCatalog.cs` | Semeia metadados e documentacao base dos endpoints oficiais. |
| `Services/ControlIDApi/OfficialApiDocumentationService.cs` | Consolida documentacao, exemplos e metadados para exibicao na UI. |
| `Services/ControlIDApi/OfficialApiInvokerService.cs` | Executa chamadas genericas aos endpoints oficiais a partir do catalogo. |
| `Services/ControlIDApi/OfficialApiParameterDocumentationUtilities.cs` | Utilitarios para documentar parametros, tipos e obrigatoriedade. |
| `Services/ControlIDApi/OfficialApiQueryParameterStrategy.cs` | Define estrategia de montagem de parametros via query string. |
| `Services/ControlIDApi/OfficialApiResultPresentationService.cs` | Prepara resultados oficiais para exibicao amigavel na interface. |
| `Services/ControlIDApi/OfficialControlIdApiService.cs` | Implementa o cliente HTTP oficial, autenticacao, envio de payloads e leitura de respostas. |
| `Services/ControlIDApi/README.md` | Documentacao especifica da camada oficial de API e organizacao dos servicos. |

## Services/Database

Repositorios que encapsulam acesso ao SQLite local para cada entidade da PoC.

| Arquivo | Responsabilidade |
| --- | --- |
| `Services/Database/AccessLogRepository.cs` | Persistencia e consulta de logs de acesso. |
| `Services/Database/AccessRuleRepository.cs` | Persistencia e consulta de regras de acesso. |
| `Services/Database/BiometricTemplateRepository.cs` | Persistencia e consulta de templates biometricos. |
| `Services/Database/CardRepository.cs` | Persistencia e consulta de cartoes. |
| `Services/Database/ChangeLogRepository.cs` | Persistencia e consulta de logs de alteracao. |
| `Services/Database/ConfigRepository.cs` | Persistencia e consulta de configuracoes locais. |
| `Services/Database/DeviceRepository.cs` | Persistencia e consulta de dispositivos. |
| `Services/Database/GroupRepository.cs` | Persistencia e consulta de grupos. |
| `Services/Database/LogRepository.cs` | Persistencia e consulta de logs genericos/auditoria. |
| `Services/Database/LogoRepository.cs` | Persistencia e consulta de logos. |
| `Services/Database/MonitorEventRepository.cs` | Persistencia e consulta de eventos monitorados. |
| `Services/Database/PhotoRepository.cs` | Persistencia e consulta de fotos. |
| `Services/Database/PushCommandRepository.cs` | Persistencia e consulta de comandos push. |
| `Services/Database/QRCodeRepository.cs` | Persistencia e consulta de QR Codes. |
| `Services/Database/SessionRepository.cs` | Persistencia e consulta de sessoes. |
| `Services/Database/SyncRepository.cs` | Persistencia e consulta do estado de sincronizacao. |
| `Services/Database/UserRepository.cs` | Persistencia e consulta de usuarios. |

## Services complementares

| Arquivo | Responsabilidade |
| --- | --- |
| `Services/DocumentedFeatures/DocumentedFeaturesPayloadFactory.cs` | Monta o payload/resumo de funcionalidades documentadas exibido na UI. |
| `Services/Files/UploadedFileBase64Encoder.cs` | Converte arquivos enviados pela UI para Base64 antes de envio/persistencia. |
| `Services/Navigation/NavigationCatalogService.cs` | Monta o catalogo de navegacao das paginas e modulos da PoC. |
| `Services/Navigation/PageShellService.cs` | Fornece metadados de shell, cabecalho e breadcrumbs das paginas. |
| `Services/OperationModes/OperationModesPayloadFactory.cs` | Monta payloads demonstrativos dos modos Standalone, Pro e Enterprise. |
| `Services/OperationModes/OperationModesProfileResolver.cs` | Resolve perfis, comportamento esperado e transicoes dos modos de operacao. |
| `Services/ProductSpecific/ProductSpecificCommandService.cs` | Executa comandos especificos por produto/modelo. |
| `Services/ProductSpecific/ProductSpecificConfigurationPayloadFactory.cs` | Monta payloads de configuracao especificos por linha de produto. |
| `Services/ProductSpecific/ProductSpecificDownloadResult.cs` | Representa resultado de download em fluxos especificos de produto. |
| `Services/ProductSpecific/ProductSpecificJsonReader.cs` | Le e interpreta JSONs usados por funcionalidades especificas. |
| `Services/ProductSpecific/ProductSpecificSections.cs` | Define secoes/categorias exibidas no modulo de recursos especificos. |
| `Services/ProductSpecific/ProductSpecificSnapshotService.cs` | Monta snapshots de estado/configuracao especificos por produto. |
| `Services/Security/ControlIdInputSanitizer.cs` | Sanitiza entradas para reduzir risco de payloads invalidos ou inseguros. |

## ViewModels

As ViewModels carregam dados ja preparados para as telas Razor, reduzindo regra de apresentacao dentro das views.

| Arquivo | Responsabilidade |
| --- | --- |
| `ViewModels/AccessLogs/AccessLogDeleteViewModel.cs` | Dados da confirmacao de exclusao de um log de acesso. |
| `ViewModels/AccessLogs/AccessLogFilterViewModel.cs` | Filtros aplicados na consulta de logs de acesso. |
| `ViewModels/AccessLogs/AccessLogListViewModel.cs` | Dados da tela de listagem de logs de acesso. |
| `ViewModels/AccessLogs/AccessLogViewModel.cs` | Dados de detalhe de um log de acesso. |
| `ViewModels/AccessRules/AccessRuleDeleteViewModel.cs` | Dados da confirmacao de exclusao de regra de acesso. |
| `ViewModels/AccessRules/AccessRuleEditViewModel.cs` | Campos usados na criacao/edicao de regra de acesso. |
| `ViewModels/AccessRules/AccessRuleListViewModel.cs` | Dados da listagem de regras de acesso. |
| `ViewModels/AccessRules/AccessRuleViewModel.cs` | Dados de detalhe de uma regra de acesso. |
| `ViewModels/AdvancedOfficial/CameraCaptureViewModel.cs` | Dados do fluxo oficial de captura de camera. |
| `ViewModels/AdvancedOfficial/ExportObjectsViewModel.cs` | Dados do fluxo oficial de exportacao de objetos. |
| `ViewModels/AdvancedOfficial/FacialEnrollViewModel.cs` | Dados do fluxo oficial de enroll facial. |
| `ViewModels/AdvancedOfficial/NetworkInterlockViewModel.cs` | Dados do fluxo de intertravamento/rede. |
| `ViewModels/AdvancedOfficial/RemoteLedControlViewModel.cs` | Dados do fluxo de controle remoto de LED. |
| `ViewModels/Auth/AuthStatusViewModel.cs` | Dados de status da autenticacao atual. |
| `ViewModels/Auth/ChangePasswordViewModel.cs` | Campos de troca de senha. |
| `ViewModels/Auth/LoginViewModel.cs` | Campos de login e conexao. |
| `ViewModels/Auth/LogoutViewModel.cs` | Dados exibidos no encerramento de sessao. |
| `ViewModels/Auth/RegisterViewModel.cs` | Campos de registro/cadastro. |
| `ViewModels/BiometricTemplates/BiometricTemplateDeleteViewModel.cs` | Dados da confirmacao de exclusao de template biometrico. |
| `ViewModels/BiometricTemplates/BiometricTemplateEditViewModel.cs` | Campos usados na criacao/edicao de template biometrico. |
| `ViewModels/BiometricTemplates/BiometricTemplateListViewModel.cs` | Dados da listagem de templates biometricos. |
| `ViewModels/BiometricTemplates/BiometricTemplateViewModel.cs` | Dados de detalhe de template biometrico. |
| `ViewModels/Cards/CardDeleteViewModel.cs` | Dados da confirmacao de exclusao de cartao. |
| `ViewModels/Cards/CardEditViewModel.cs` | Campos usados na criacao/edicao de cartao. |
| `ViewModels/Cards/CardListViewModel.cs` | Dados da listagem de cartoes. |
| `ViewModels/Cards/CardViewModel.cs` | Dados de detalhe de cartao. |
| `ViewModels/Catra/CatraEventListViewModel.cs` | Dados da listagem de eventos de catraca. |
| `ViewModels/Catra/CatraEventViewModel.cs` | Dados de detalhe de evento de catraca. |
| `ViewModels/Catra/CatraOpenViewModel.cs` | Dados do comando de abertura de catraca. |
| `ViewModels/ChangeLogs/ChangeLogDeleteViewModel.cs` | Dados da confirmacao de exclusao de log de alteracao. |
| `ViewModels/ChangeLogs/ChangeLogListViewModel.cs` | Dados da listagem de logs de alteracao. |
| `ViewModels/ChangeLogs/ChangeLogViewModel.cs` | Dados de detalhe de log de alteracao. |
| `ViewModels/Config/ConfigDeleteViewModel.cs` | Dados da confirmacao de exclusao de configuracao. |
| `ViewModels/Config/ConfigDiagnosticsViewModel.cs` | Dados da tela de diagnostico de configuracoes. |
| `ViewModels/Config/ConfigEditViewModel.cs` | Campos usados na criacao/edicao de configuracao. |
| `ViewModels/Config/ConfigListViewModel.cs` | Dados da listagem de configuracoes. |
| `ViewModels/Config/ConfigOfficialViewModel.cs` | Dados da tela de configuracoes oficiais da API. |
| `ViewModels/Config/ConfigViewModel.cs` | Dados de detalhe de configuracao. |
| `ViewModels/Devices/DeviceDeleteViewModel.cs` | Dados da confirmacao de exclusao de dispositivo. |
| `ViewModels/Devices/DeviceEditViewModel.cs` | Campos usados na criacao/edicao de dispositivo. |
| `ViewModels/Devices/DeviceListViewModel.cs` | Dados da listagem de dispositivos. |
| `ViewModels/Devices/DeviceViewModel.cs` | Dados de detalhe de dispositivo. |
| `ViewModels/DocumentedFeatures/DocumentedFeaturesViewModel.cs` | Dados consolidados das funcionalidades documentadas/implementadas. |
| `ViewModels/Errors/ErrorListViewModel.cs` | Dados da listagem de erros. |
| `ViewModels/Errors/ErrorViewModel.cs` | Dados de detalhe de erro. |
| `ViewModels/Groups/GroupDeleteViewModel.cs` | Dados da confirmacao de exclusao de grupo. |
| `ViewModels/Groups/GroupEditViewModel.cs` | Campos usados na criacao/edicao de grupo. |
| `ViewModels/Groups/GroupListViewModel.cs` | Dados da listagem de grupos. |
| `ViewModels/Groups/GroupViewModel.cs` | Dados de detalhe de grupo. |
| `ViewModels/Hardware/BiometryValidationViewModel.cs` | Dados do fluxo de validacao biometrica. |
| `ViewModels/Hardware/DoorStateViewModel.cs` | Dados de estado de porta. |
| `ViewModels/Hardware/GpioStateViewModel.cs` | Dados de estado GPIO. |
| `ViewModels/Hardware/HardwareStatusViewModel.cs` | Dados de status geral de hardware. |
| `ViewModels/Hardware/RelayActionViewModel.cs` | Dados de acionamento de rele. |
| `ViewModels/Home/HomeDashboardViewModel.cs` | Dados do dashboard inicial. |
| `ViewModels/Logo/LogoDeleteViewModel.cs` | Dados da confirmacao de exclusao de logo. |
| `ViewModels/Logo/LogoListViewModel.cs` | Dados da listagem de logos. |
| `ViewModels/Logo/LogoUploadViewModel.cs` | Campos do upload de logo. |
| `ViewModels/Logo/LogoViewModel.cs` | Dados de detalhe de logo. |
| `ViewModels/Media/AdVideoManageViewModel.cs` | Dados do gerenciamento de video de propaganda. |
| `ViewModels/Media/PhotoDeleteViewModel.cs` | Dados da confirmacao de exclusao de foto. |
| `ViewModels/Media/PhotoListViewModel.cs` | Dados da listagem de fotos. |
| `ViewModels/Media/PhotoUploadViewModel.cs` | Campos do upload de foto. |
| `ViewModels/Media/PhotoViewModel.cs` | Dados de detalhe de foto. |
| `ViewModels/Monitor/MonitorPushListViewModel.cs` | Dados da listagem de eventos push monitorados. |
| `ViewModels/Monitor/MonitorWebhookListViewModel.cs` | Dados da listagem de eventos webhook monitorados. |
| `ViewModels/Monitor/PushEventViewModel.cs` | Dados de detalhe de evento push. |
| `ViewModels/Monitor/WebhookEventViewModel.cs` | Dados de detalhe de evento webhook. |
| `ViewModels/OfficialApi/OfficialApiContractViewModel.cs` | Dados de contrato de endpoint oficial. |
| `ViewModels/OfficialApi/OfficialApiIndexViewModel.cs` | Dados do catalogo oficial de endpoints. |
| `ViewModels/OfficialApi/OfficialApiInvokeViewModel.cs` | Dados do formulario e resultado de invocacao oficial. |
| `ViewModels/OfficialObjects/OfficialObjectsViewModel.cs` | Dados da tela de exploracao de objetos oficiais. |
| `ViewModels/OperationModes/OperationModesViewModel.cs` | Dados da tela de modos Standalone, Pro e Enterprise. |
| `ViewModels/ProductSpecific/ProductSpecificViewModel.cs` | Dados da tela de recursos especificos por produto. |
| `ViewModels/Push/PushEventListViewModel.cs` | Dados da listagem de eventos push. |
| `ViewModels/Push/PushEventViewModel.cs` | Dados de detalhe de evento push. |
| `ViewModels/Push/PushQueueCommandViewModel.cs` | Campos para enfileirar comando push. |
| `ViewModels/QRCodes/QRCodeDeleteViewModel.cs` | Dados da confirmacao de exclusao de QR Code. |
| `ViewModels/QRCodes/QRCodeEditViewModel.cs` | Campos usados na criacao/edicao de QR Code. |
| `ViewModels/QRCodes/QRCodeListViewModel.cs` | Dados da listagem de QR Codes. |
| `ViewModels/QRCodes/QRCodeViewModel.cs` | Dados de detalhe de QR Code. |
| `ViewModels/RemoteActions/RemoteActionExecuteViewModel.cs` | Dados do fluxo de execucao de acao remota. |
| `ViewModels/RemoteActions/RemoteActionListViewModel.cs` | Dados da listagem de acoes remotas. |
| `ViewModels/RemoteActions/RemoteActionViewModel.cs` | Dados de detalhe de acao remota. |
| `ViewModels/RemoteActions/RemoteAuthorizationViewModel.cs` | Dados do fluxo de autorizacao remota. |
| `ViewModels/RemoteActions/RemoteEnrollViewModel.cs` | Dados do fluxo de enroll remoto. |
| `ViewModels/Session/SessionCreateViewModel.cs` | Campos de criacao de sessao. |
| `ViewModels/Session/SessionDeactivateViewModel.cs` | Dados de desativacao/encerramento de sessao. |
| `ViewModels/Session/SessionEditViewModel.cs` | Campos de edicao de sessao. |
| `ViewModels/Session/SessionListViewModel.cs` | Dados da listagem de sessoes. |
| `ViewModels/Session/SessionStatusViewModel.cs` | Dados de status da sessao. |
| `ViewModels/Session/SessionViewModel.cs` | Dados de detalhe de sessao. |
| `ViewModels/Shared/AppPageHeaderViewModel.cs` | Dados do cabecalho padrao das paginas. |
| `ViewModels/Shared/NavigationViewModels.cs` | Modelos compartilhados de navegacao, menus e itens do shell. |
| `ViewModels/Shared/RawResponsePanelViewModel.cs` | Dados do painel reutilizavel de resposta bruta. |
| `ViewModels/System/HashPasswordViewModel.cs` | Campos e resultado de hash de senha. |
| `ViewModels/System/SystemActionResultViewModel.cs` | Dados do resultado de uma acao de sistema. |
| `ViewModels/System/SystemInfoViewModel.cs` | Dados de informacoes gerais do sistema. |
| `ViewModels/System/SystemLoginCredentialsViewModel.cs` | Campos de credenciais de login do sistema/equipamento. |
| `ViewModels/System/SystemNetworkViewModel.cs` | Dados de configuracao/consulta de rede. |
| `ViewModels/System/SystemVpnViewModel.cs` | Dados de configuracao/consulta de VPN. |
| `ViewModels/Users/HashPasswordResponse.cs` | Estrutura de resposta de hash de senha usada nos fluxos de usuario. |
| `ViewModels/Users/UserDeleteViewModel.cs` | Dados da confirmacao de exclusao de usuario. |
| `ViewModels/Users/UserDto.cs` | DTO auxiliar para transferencia de dados de usuario. |
| `ViewModels/Users/UserEditViewModel.cs` | Campos usados na criacao/edicao de usuario. |
| `ViewModels/Users/UserListViewModel.cs` | Dados da listagem de usuarios. |
| `ViewModels/Users/UsersApiResponse.cs` | Estrutura de resposta agregada da API para usuarios. |
| `ViewModels/Users/UserViewModel.cs` | Dados de detalhe de usuario. |
| `ViewModels/Workspace/WorkspaceExplorerViewModel.cs` | Dados do explorador/workspace operacional. |

## Views

As views Razor compoem a interface web da PoC. Em geral, cada pasta espelha um controller e cada arquivo `.cshtml` representa uma tela ou parcial reutilizavel.

| Arquivo | Responsabilidade |
| --- | --- |
| `Views/_ViewImports.cshtml` | Importa namespaces e tag helpers disponiveis para todas as views. |
| `Views/_ViewStart.cshtml` | Define o layout padrao usado pelas views. |
| `Views/AccessLogs/Delete.cshtml` | Tela de confirmacao de exclusao de log de acesso. |
| `Views/AccessLogs/Details.cshtml` | Tela de detalhe de log de acesso. |
| `Views/AccessLogs/Index.cshtml` | Tela de listagem/filtro de logs de acesso. |
| `Views/AccessRules/Create.cshtml` | Tela de criacao de regra de acesso. |
| `Views/AccessRules/Delete.cshtml` | Tela de confirmacao de exclusao de regra de acesso. |
| `Views/AccessRules/Details.cshtml` | Tela de detalhe de regra de acesso. |
| `Views/AccessRules/Edit.cshtml` | Tela de edicao de regra de acesso. |
| `Views/AccessRules/Index.cshtml` | Tela de listagem de regras de acesso. |
| `Views/AdvancedOfficial/CameraCapture.cshtml` | Tela do fluxo oficial de captura de camera. |
| `Views/AdvancedOfficial/ExportObjects.cshtml` | Tela do fluxo oficial de exportacao de objetos. |
| `Views/AdvancedOfficial/FacialEnroll.cshtml` | Tela do fluxo oficial de enroll facial. |
| `Views/AdvancedOfficial/Index.cshtml` | Tela inicial dos recursos oficiais avancados. |
| `Views/AdvancedOfficial/NetworkInterlock.cshtml` | Tela do fluxo de intertravamento/rede. |
| `Views/AdvancedOfficial/RemoteLedControl.cshtml` | Tela do fluxo de controle remoto de LED. |
| `Views/Auth/ChangePassword.cshtml` | Tela de troca de senha. |
| `Views/Auth/Login.cshtml` | Tela de login/conexao. |
| `Views/Auth/Logout.cshtml` | Tela de encerramento de sessao. |
| `Views/Auth/Register.cshtml` | Tela de registro/cadastro. |
| `Views/Auth/Status.cshtml` | Tela de status da autenticacao. |
| `Views/BiometricTemplates/Create.cshtml` | Tela de criacao de template biometrico. |
| `Views/BiometricTemplates/Delete.cshtml` | Tela de confirmacao de exclusao de template biometrico. |
| `Views/BiometricTemplates/Details.cshtml` | Tela de detalhe de template biometrico. |
| `Views/BiometricTemplates/Edit.cshtml` | Tela de edicao de template biometrico. |
| `Views/BiometricTemplates/Index.cshtml` | Tela de listagem de templates biometricos. |
| `Views/Cards/Create.cshtml` | Tela de criacao de cartao. |
| `Views/Cards/Delete.cshtml` | Tela de confirmacao de exclusao de cartao. |
| `Views/Cards/Details.cshtml` | Tela de detalhe de cartao. |
| `Views/Cards/Edit.cshtml` | Tela de edicao de cartao. |
| `Views/Cards/Index.cshtml` | Tela de listagem de cartoes. |
| `Views/Catra/Delete.cshtml` | Tela de confirmacao de exclusao de evento/registro de catraca. |
| `Views/Catra/Details.cshtml` | Tela de detalhe de evento de catraca. |
| `Views/Catra/Index.cshtml` | Tela de listagem/operacao de catraca. |
| `Views/ChangeLogs/Delete.cshtml` | Tela de confirmacao de exclusao de log de alteracao. |
| `Views/ChangeLogs/Details.cshtml` | Tela de detalhe de log de alteracao. |
| `Views/ChangeLogs/Index.cshtml` | Tela de listagem de logs de alteracao. |
| `Views/Config/Create.cshtml` | Tela de criacao de configuracao. |
| `Views/Config/Delete.cshtml` | Tela de confirmacao de exclusao de configuracao. |
| `Views/Config/Details.cshtml` | Tela de detalhe de configuracao. |
| `Views/Config/Diagnostics.cshtml` | Tela de diagnostico de configuracoes. |
| `Views/Config/Edit.cshtml` | Tela de edicao de configuracao. |
| `Views/Config/Index.cshtml` | Tela de listagem de configuracoes. |
| `Views/Config/Official.cshtml` | Tela de configuracoes oficiais da API. |
| `Views/Devices/Create.cshtml` | Tela de criacao de dispositivo. |
| `Views/Devices/Delete.cshtml` | Tela de confirmacao de exclusao de dispositivo. |
| `Views/Devices/Details.cshtml` | Tela de detalhe de dispositivo. |
| `Views/Devices/Edit.cshtml` | Tela de edicao de dispositivo. |
| `Views/Devices/Index.cshtml` | Tela de listagem de dispositivos. |
| `Views/DocumentedFeatures/Index.cshtml` | Tela consolidada de funcionalidades documentadas/implementadas. |
| `Views/Errors/Details.cshtml` | Tela de detalhe de erro. |
| `Views/Errors/Index.cshtml` | Tela de listagem de erros. |
| `Views/Groups/Create.cshtml` | Tela de criacao de grupo. |
| `Views/Groups/Delete.cshtml` | Tela de confirmacao de exclusao de grupo. |
| `Views/Groups/Details.cshtml` | Tela de detalhe de grupo. |
| `Views/Groups/Edit.cshtml` | Tela de edicao de grupo. |
| `Views/Groups/Index.cshtml` | Tela de listagem de grupos. |
| `Views/Hardware/DoorState.cshtml` | Tela de estado de porta. |
| `Views/Hardware/Gpio.cshtml` | Tela de consulta/acao GPIO. |
| `Views/Hardware/RelayAction.cshtml` | Tela de acionamento de rele. |
| `Views/Hardware/Status.cshtml` | Tela de status de hardware. |
| `Views/Hardware/ValidateBiometry.cshtml` | Tela de validacao biometrica. |
| `Views/Home/About.cshtml` | Tela institucional/sobre a PoC. |
| `Views/Home/Contact.cshtml` | Tela de contato/referencias. |
| `Views/Home/Index.cshtml` | Dashboard inicial da PoC. |
| `Views/Logo/Delete.cshtml` | Tela de confirmacao de exclusao de logo. |
| `Views/Logo/Details.cshtml` | Tela de detalhe de logo. |
| `Views/Logo/Index.cshtml` | Tela de listagem de logos. |
| `Views/Logo/Upload.cshtml` | Tela de upload de logo. |
| `Views/Media/AdMode.cshtml` | Tela de gerenciamento de video/modo propaganda. |
| `Views/Media/Delete.cshtml` | Tela de confirmacao de exclusao de midia. |
| `Views/Media/Details.cshtml` | Tela de detalhe de midia/foto. |
| `Views/Media/Index.cshtml` | Tela de listagem de midias/fotos. |
| `Views/Media/Upload.cshtml` | Tela de upload de midia/foto. |
| `Views/Monitor/Push.cshtml` | Tela de monitoramento de eventos push. |
| `Views/Monitor/PushDetails.cshtml` | Tela de detalhe de evento push monitorado. |
| `Views/Monitor/Webhook.cshtml` | Tela de monitoramento de webhooks/callbacks. |
| `Views/Monitor/WebhookDetails.cshtml` | Tela de detalhe de webhook/callback recebido. |
| `Views/OfficialApi/Index.cshtml` | Tela do catalogo oficial de endpoints. |
| `Views/OfficialApi/Invoke.cshtml` | Tela de invocacao dinamica de endpoint oficial. |
| `Views/OfficialEvents/Details.cshtml` | Tela de detalhe de evento oficial. |
| `Views/OfficialEvents/Index.cshtml` | Tela de listagem de eventos oficiais. |
| `Views/OfficialObjects/Index.cshtml` | Tela de exploracao de objetos oficiais. |
| `Views/OperationModes/Index.cshtml` | Tela dos modos Standalone, Pro e Enterprise. |
| `Views/ProductSpecific/Index.cshtml` | Tela de recursos especificos por produto. |
| `Views/PushCenter/Details.cshtml` | Tela de detalhe de item/comando da central de push. |
| `Views/PushCenter/Index.cshtml` | Tela centralizada de eventos e comandos push. |
| `Views/QRCodes/Create.cshtml` | Tela de criacao de QR Code. |
| `Views/QRCodes/Delete.cshtml` | Tela de confirmacao de exclusao de QR Code. |
| `Views/QRCodes/Details.cshtml` | Tela de detalhe de QR Code. |
| `Views/QRCodes/Edit.cshtml` | Tela de edicao de QR Code. |
| `Views/QRCodes/Index.cshtml` | Tela de listagem de QR Codes. |
| `Views/RemoteActions/Authorization.cshtml` | Tela de autorizacao remota. |
| `Views/RemoteActions/Details.cshtml` | Tela de detalhe de acao remota. |
| `Views/RemoteActions/Enroll.cshtml` | Tela de enroll remoto. |
| `Views/RemoteActions/Execute.cshtml` | Tela de execucao de acao remota. |
| `Views/RemoteActions/Index.cshtml` | Tela de listagem de acoes remotas. |
| `Views/Session/Delete.cshtml` | Tela de encerramento/exclusao de sessao. |
| `Views/Session/Details.cshtml` | Tela de detalhe de sessao. |
| `Views/Session/Index.cshtml` | Tela de listagem de sessoes. |
| `Views/Session/Status.cshtml` | Tela de status de sessao. |
| `Views/Shared/_AccessDenied.cshtml` | Parcial de acesso negado. |
| `Views/Shared/_AppPageHeader.cshtml` | Parcial de cabecalho padrao das paginas. |
| `Views/Shared/_ConnectionPanel.cshtml` | Parcial do painel de conexao/status do equipamento. |
| `Views/Shared/_EndpointContractPanel.cshtml` | Parcial de exibicao de contrato de endpoint. |
| `Views/Shared/_Layout.cshtml` | Layout principal da aplicacao. |
| `Views/Shared/_Layout.cshtml.css` | Estilos escopados do layout principal. |
| `Views/Shared/_NavBar.cshtml` | Parcial da barra de navegacao principal. |
| `Views/Shared/_NavBar.cshtml.css` | Estilos escopados da barra de navegacao. |
| `Views/Shared/_NotFound.cshtml` | Parcial de recurso nao encontrado. |
| `Views/Shared/_RawResponsePanel.cshtml` | Parcial de exibicao de resposta bruta JSON/texto. |
| `Views/Shared/_ServerError.cshtml` | Parcial de erro interno. |
| `Views/Shared/_StatusMessage.cshtml` | Parcial de mensagens de status/sucesso/erro. |
| `Views/Shared/_TopNavigation.cshtml` | Parcial de navegacao superior. |
| `Views/Shared/_ValidationScriptsPartial.cshtml` | Parcial com scripts de validacao client-side. |
| `Views/Shared/Error.cshtml` | Tela generica de erro MVC. |
| `Views/System/ActionResult.cshtml` | Tela de resultado de acao administrativa/sistema. |
| `Views/System/HashPassword.cshtml` | Tela de geracao/validacao de hash de senha. |
| `Views/System/Info.cshtml` | Tela de informacoes de sistema. |
| `Views/System/LoginCredentials.cshtml` | Tela de credenciais de login do sistema/equipamento. |
| `Views/System/Network.cshtml` | Tela de configuracao/consulta de rede. |
| `Views/System/Vpn.cshtml` | Tela de configuracao/consulta de VPN. |
| `Views/Users/Create.cshtml` | Tela de criacao de usuario. |
| `Views/Users/Delete.cshtml` | Tela de confirmacao de exclusao de usuario. |
| `Views/Users/Details.cshtml` | Tela de detalhe de usuario. |
| `Views/Users/Edit.cshtml` | Tela de edicao de usuario. |
| `Views/Users/Index.cshtml` | Tela de listagem de usuarios. |
| `Views/Workspace/Domain.cshtml` | Tela de dominio/area especifica do workspace. |
| `Views/Workspace/Index.cshtml` | Tela principal do workspace/explorador operacional. |

## wwwroot

| Arquivo/Pasta | Responsabilidade |
| --- | --- |
| `wwwroot/css/site.css` | Estilos globais da PoC, componentes visuais, dashboard, tabelas, formularios e ajustes responsivos. |
| `wwwroot/js/site.js` | JavaScript global da UI, comportamentos de interacao e utilidades client-side. |
| `wwwroot/favicon.ico` | Icone exibido pelo navegador para a aplicacao. |
| `wwwroot/lib/bootstrap/*` | Arquivos CSS/JS do Bootstrap, incluindo versoes minificadas, sourcemaps, utilitarios e licencas. |
| `wwwroot/lib/jquery/*` | Arquivos da biblioteca jQuery usados pela camada client-side. |
| `wwwroot/lib/jquery-validation/*` | Biblioteca de validacao jQuery usada nos formularios. |
| `wwwroot/lib/jquery-validation-unobtrusive/*` | Adaptadores unobtrusive validation usados com ASP.NET Core MVC/Razor. |

## docs

| Arquivo/Pasta | Responsabilidade |
| --- | --- |
| `docs/changelog-2026-04-14.md` | Registro resumido de evolucoes relevantes realizadas na PoC. |
| `docs/monitor-implementation.md` | Documenta a implementacao da funcionalidade Monitor, callbacks oficiais, seguranca e persistencia local. |
| `docs/operation-modes-implementation.md` | Documenta a implementacao dos modos Standalone, Pro e Enterprise, incluindo payloads e transicoes. |
| `docs/project-file-responsibilities.md` | Este inventario de responsabilidades por pasta e arquivo. |
| `docs/push-implementation.md` | Documenta a implementacao da funcionalidade Push, fila persistida, polling e retorno de resultados. |
| `docs/reports/controlid-api-audit-2026-04-13.md` | Auditoria tecnica da cobertura da API Control iD. |
| `docs/reports/design-system-accessibility-audit-2026-04-14.md` | Auditoria de design system e acessibilidade da UI. |
| `docs/reports/heuristic-ui-audit-2026-04-14.md` | Avaliacao heuristica inicial da interface. |
| `docs/reports/localhost-smoke-test-2026-04-13.md` | Relatorio de smoke test local da rodada de 13/04/2026. |
| `docs/reports/localhost-smoke-test-2026-04-14.md` | Relatorio de smoke test local da rodada de 14/04/2026. |
| `docs/reports/operation-modes-e2e-runbook-2026-04-14.md` | Roteiro E2E para validacao dos modos de operacao. |
| `docs/reports/operation-modes-homologation-matrix-2026-04-14.md` | Matriz de homologacao dos modos Standalone, Pro e Enterprise. |
| `docs/reports/visual-inventory-2026-04-14.md` | Inventario visual das telas e estados avaliados. |

## tests

| Arquivo/Pasta | Responsabilidade |
| --- | --- |
| `tests/Integracao.ControlID.PoC.Tests/Integracao.ControlID.PoC.Tests.csproj` | Projeto de testes unitarios xUnit da PoC. |
| `tests/Integracao.ControlID.PoC.Tests/Helpers/ApiResponseHelperTests.cs` | Testa normalizacao e interpretacao de respostas da API. |
| `tests/Integracao.ControlID.PoC.Tests/Helpers/CryptoHelperTests.cs` | Testa comportamentos criptograficos e hash. |
| `tests/Integracao.ControlID.PoC.Tests/Helpers/FileHelperTests.cs` | Testa utilitarios de arquivo. |
| `tests/Integracao.ControlID.PoC.Tests/Helpers/HttpHelperTests.cs` | Testa utilitarios HTTP. |
| `tests/Integracao.ControlID.PoC.Tests/Helpers/ProductSpecificPresentationHelperTests.cs` | Testa utilitarios de apresentacao de recursos especificos por produto. |
| `tests/Integracao.ControlID.PoC.Tests/Helpers/SecurityTextHelperTests.cs` | Testa mascaramento e textos de seguranca. |
| `tests/Integracao.ControlID.PoC.Tests/Helpers/SessionHelperTests.cs` | Testa manipulacao auxiliar de sessao. |
| `tests/Integracao.ControlID.PoC.Tests/Mappings/ViewModelMappingsTests.cs` | Testa conversoes para ViewModels. |
| `tests/Integracao.ControlID.PoC.Tests/Middlewares/SecurityHeadersMiddlewareTests.cs` | Testa aplicacao de cabecalhos de seguranca. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Callbacks/CallbackIngressServiceTests.cs` | Testa orquestracao de callbacks recebidos. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Callbacks/CallbackRequestBodyReaderTests.cs` | Testa leitura reutilizavel do corpo bruto de callbacks. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Callbacks/CallbackSecurityEvaluatorTests.cs` | Testa validacao de seguranca dos callbacks. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialApiBinaryFileResultFactoryTests.cs` | Testa geracao de resultados binarios/download para chamadas oficiais. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialApiBodyParameterStrategyTests.cs` | Testa montagem de parametros no body. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialApiCatalogServiceTests.cs` | Testa catalogo de endpoints oficiais. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialApiContractDocumentationServiceTests.cs` | Testa documentacao visual de contratos. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialApiInvokerServiceTests.cs` | Testa invocacao generica de endpoints oficiais. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialApiParameterDocumentationUtilitiesTests.cs` | Testa utilitarios de documentacao de parametros. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialApiQueryParameterStrategyTests.cs` | Testa montagem de parametros via query string. |
| `tests/Integracao.ControlID.PoC.Tests/Services/DocumentedFeatures/DocumentedFeaturesPayloadFactoryTests.cs` | Testa payload de funcionalidades documentadas. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Files/UploadedFileBase64EncoderTests.cs` | Testa conversao de uploads para Base64. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Navigation/NavigationCatalogServiceTests.cs` | Testa catalogo de navegacao. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Navigation/PageShellServiceTests.cs` | Testa metadados de shell/cabecalho das paginas. |
| `tests/Integracao.ControlID.PoC.Tests/Services/OperationModes/OperationModesPayloadFactoryTests.cs` | Testa payloads dos modos de operacao. |
| `tests/Integracao.ControlID.PoC.Tests/Services/OperationModes/OperationModesProfileResolverTests.cs` | Testa resolucao de perfis dos modos de operacao. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ProductSpecific/ProductSpecificCommandServiceTests.cs` | Testa comandos especificos por produto. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ProductSpecific/ProductSpecificConfigurationPayloadFactoryTests.cs` | Testa payloads de configuracao especificos por produto. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ProductSpecific/ProductSpecificJsonReaderTests.cs` | Testa leitura e interpretacao de JSONs especificos. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ProductSpecific/ProductSpecificSnapshotServiceTests.cs` | Testa montagem de snapshots especificos por produto. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Security/ControlIdInputSanitizerTests.cs` | Testa sanitizacao de entradas. |

## tools

| Arquivo/Pasta | Responsabilidade |
| --- | --- |
| `tools/smoke-localhost.ps1` | Script PowerShell que executa smoke test local, sobe stub e percorre fluxos criticos da PoC. |
| `tools/ControlIdDeviceStub/ControlIdDeviceStub.csproj` | Projeto .NET do stub local que simula respostas de um equipamento Control iD. |
| `tools/ControlIdDeviceStub/Program.cs` | Implementa os endpoints simulados usados pelos smoke tests locais. |
