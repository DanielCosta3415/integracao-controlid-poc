# Responsabilidades dos arquivos do projeto

Este documento resume a responsabilidade dos arquivos versionados da PoC `Integracao.ControlID.PoC`.

O objetivo é servir como um mapa rápido de navegação para quem quiser entender a solução, localizar uma funcionalidade ou contribuir com o projeto sem precisar descobrir a estrutura apenas pelo código.

Observações de escopo:

- Arquivos gerados em `bin/`, `obj/`, banco SQLite local, logs e artefatos temporários não fazem parte deste inventário.
- Bibliotecas vendorizadas em `wwwroot/lib/` foram agrupadas por família, porque incluem variações minificadas, sourcemaps e licenças sem regra de negócio da PoC.
- As descrições abaixo são intencionais e resumidas; para detalhes de comportamento, consulte o código e os testes relacionados.

## Raiz da solução

| Arquivo | Responsabilidade |
| --- | --- |
| `.dockerignore` | Remove segredos, banco local, logs e artefatos do contexto de build Docker. |
| `.env.example` | Lista variáveis de ambiente seguras para criar `.env` local sem versionar secrets. |
| `Dockerfile` | Define build multi-stage e runtime não root para execução containerizada da PoC. |
| `appsettings.Staging.json` | Defaults seguros sem segredos para validação em staging. |
| `appsettings.Production.json` | Defaults seguros sem segredos para produção; exige variáveis reais no ambiente. |
| `docker-compose.yml` | Executa a PoC em container com volumes persistentes, healthcheck e variáveis obrigatórias. |
| `.editorconfig` | Padroniza convenções básicas de edição, formatação e estilo entre IDEs. |
| `.gitignore` | Define arquivos e pastas que não devem ser versionados, como builds, logs e artefatos locais. |
| `ops.example.json` | Exemplo versionado de configuracao operacional para incidentes, on-call, backup externo, RTO/RPO, deploy, privacidade, validacao externa, contrato fisico e FinOps. |
| `Directory.Build.props` | Centraliza propriedades comuns de build para os projetos .NET da solução. |
| `Integracao.ControlID.PoC.csproj` | Define o projeto ASP.NET Core MVC principal, dependências NuGet e configurações de compilação. |
| `Integracao.ControlID.PoC.sln` | Agrupa o projeto principal, testes e utilitários em uma única solução. |
| `Program.cs` | Configura o bootstrap da aplicação, DI, middlewares, banco local, rotas MVC, Serilog e serviços da PoC. |
| `README.md` | Apresenta a PoC, stack, setup local, testes, observabilidade e links de referência. |
| `appsettings.json` | Configurações base da aplicação, API Control iD, banco, logs e segurança de callbacks. |
| `appsettings.Development.json` | Sobrescritas de configuração para execução local em ambiente de desenvolvimento. |

## Properties

| Arquivo | Responsabilidade |
| --- | --- |
| `Properties/launchSettings.json` | Define perfis locais de execução, URLs, portas e variáveis usadas pelo `dotnet run`/IDE. |

## Controllers

Os controllers coordenam as rotas MVC, recebem a entrada da UI, acionam serviços/repositórios e retornam views ou respostas auxiliares.

| Arquivo | Responsabilidade |
| --- | --- |
| `Controllers/AccessLogsController.cs` | Fluxos de listagem, detalhe e remoção de logs de acesso persistidos localmente. |
| `Controllers/AccessRulesController.cs` | CRUD e visualização das regras de acesso locais usadas pela PoC. |
| `Controllers/AdvancedOfficialController.cs` | Telas e execuções de cenários oficiais avançados, como captura, exportação e comandos especiais. |
| `Controllers/AuthController.cs` | Fluxos de login, registro, logout, status e troca de senha relacionados a autenticação no equipamento. |
| `Controllers/BiometricTemplatesController.cs` | Operações de listagem, detalhe, edição e remoção de templates biométricos. |
| `Controllers/CardsController.cs` | Operações de gerenciamento local de cartões vinculados a usuários/acesso. |
| `Controllers/CatraController.cs` | Fluxos específicos de catraca, eventos e abertura remota. |
| `Controllers/ChangeLogsController.cs` | Consulta e remoção de registros de alteração sincronizados ou armazenados localmente. |
| `Controllers/ConfigController.cs` | Gerenciamento, diagnóstico e visualização de configurações do equipamento e da PoC. |
| `Controllers/DevicesController.cs` | CRUD e visualização de dispositivos cadastrados no contexto local. |
| `Controllers/DocumentedFeaturesController.cs` | Exibe o consolidado de funcionalidades documentadas e implementadas na PoC. |
| `Controllers/ErrorsController.cs` | Lista e detalha erros registrados durante chamadas, integrações ou processamento local. |
| `Controllers/GroupsController.cs` | CRUD e visualização de grupos locais relacionados aos fluxos de acesso. |
| `Controllers/HardwareController.cs` | Aciona e apresenta recursos de hardware, como GPIO, relé, porta e validações biométricas. |
| `Controllers/HomeController.cs` | Monta o dashboard inicial e indicadores principais da PoC. |
| `Controllers/LogoController.cs` | Gerencia upload, consulta e remoção de logos/imagens locais. |
| `Controllers/MediaController.cs` | Gerencia mídias como fotos e vídeos usados nos fluxos da PoC. |
| `Controllers/MonitorWebhookController.cs` | Recebe, registra e apresenta eventos recebidos por webhook/callback. |
| `Controllers/OfficialApiController.cs` | Exibe catálogo oficial, contratos e invocação genérica dos endpoints documentados. |
| `Controllers/OfficialCallbacksController.cs` | Implementa endpoints de callbacks oficiais e processamento de payloads recebidos. |
| `Controllers/OfficialEventsController.cs` | Centraliza fluxos ligados a eventos oficiais da API Control iD. |
| `Controllers/OfficialObjectsController.cs` | Apresenta operações oficiais sobre objetos, payloads e contratos relacionados. |
| `Controllers/OperationModesController.cs` | Exibe e simula os modos de operação Standalone, Pro e Enterprise. |
| `Controllers/ProductSpecificController.cs` | Coordena funcionalidades específicas por produto/modelo da família Control iD. |
| `Controllers/PushCenterController.cs` | Organiza a central de push, filas e comandos pendentes. |
| `Controllers/PushController.cs` | Fluxos de eventos push, consulta e enfileiramento de comandos. |
| `Controllers/QRCodesController.cs` | CRUD e visualização de QR Codes persistidos localmente. |
| `Controllers/RemoteActionsController.cs` | Execução e acompanhamento de ações remotas como autorização e enrolamento. |
| `Controllers/SessionController.cs` | Gerenciamento de sessões locais e status de autenticação/conexão com o equipamento. |
| `Controllers/SystemController.cs` | Operações de sistema, rede, VPN, hash de senha e ações administrativas. |
| `Controllers/UsersController.cs` | CRUD, visualização e payloads de usuários da PoC. |
| `Controllers/WorkspaceController.cs` | Exibe o workspace/explorador operacional para navegar pelos recursos implementados. |

## Data

| Arquivo | Responsabilidade |
| --- | --- |
| `Data/IntegracaoControlIDContext.cs` | DbContext do Entity Framework Core; mapeia entidades locais, índices, relacionamentos e configurações SQLite. |

## Helpers

| Arquivo | Responsabilidade |
| --- | --- |
| `Helpers/ApiResponseHelper.cs` | Utilitários para normalizar respostas e mensagens vindas da API/serviços. |
| `Helpers/CryptoHelper.cs` | Apoio criptográfico, especialmente para hashes e transformações relacionadas a segurança. |
| `Helpers/FileHelper.cs` | Rotinas auxiliares para manipulação de arquivos usados pela PoC. |
| `Helpers/HttpHelper.cs` | Funções de apoio para chamadas HTTP, montagem de requests e leitura de respostas. |
| `Helpers/NavigationPresentationHelper.cs` | Centraliza detalhes de apresentação usados pela navegação da UI. |
| `Helpers/ProductSpecificPresentationHelper.cs` | Apoia a exibição de conteúdos específicos de produto na interface. |
| `Helpers/SecurityTextHelper.cs` | Padroniza textos e mascaramentos ligados a informações sensíveis. |
| `Helpers/SessionHelper.cs` | Auxilia leitura, escrita e interpretação de dados de sessão no contexto web. |

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
| `Middlewares/ApiSessionMiddleware.cs` | Garante contexto mínimo de sessão/API durante o pipeline HTTP. |
| `Middlewares/CorrelationIdMiddleware.cs` | Normaliza, propaga e registra correlation ID seguro em requests/responses. |
| `Middlewares/ExceptionHandlingMiddleware.cs` | Captura exceções não tratadas e padroniza a resposta/registro de erro. |
| `Middlewares/RequestLoggingMiddleware.cs` | Registra informações de requests para observabilidade local. |
| `Middlewares/SecurityHeadersMiddleware.cs` | Aplica cabeçalhos de segurança HTTP nas respostas da aplicação. |

## Models/ControlIDApi

Modelos que representam contratos, payloads e respostas próximas da API Control iD.

| Arquivo | Responsabilidade |
| --- | --- |
| `Models/ControlIDApi/AccessLog.cs` | Representa um registro de acesso retornado ou enviado no contexto da API. |
| `Models/ControlIDApi/AccessRule.cs` | Representa regras de permissão/acesso no formato de integração. |
| `Models/ControlIDApi/BiometricTemplate.cs` | Representa templates biométricos trafegados na API. |
| `Models/ControlIDApi/Card.cs` | Representa cartões de acesso vinculados a usuários. |
| `Models/ControlIDApi/CatraEvent.cs` | Representa eventos específicos de catraca. |
| `Models/ControlIDApi/ChangeLog.cs` | Representa logs de alteração/sincronização do equipamento. |
| `Models/ControlIDApi/ConfigGroup.cs` | Agrupa configurações retornadas ou aplicadas no equipamento. |
| `Models/ControlIDApi/Device.cs` | Representa dados de identificação e configuração de dispositivo. |
| `Models/ControlIDApi/ErrorInfo.cs` | Modela informações de erro retornadas ou registradas pela integração. |
| `Models/ControlIDApi/GpioState.cs` | Representa estado de GPIO/entradas e saídas físicas. |
| `Models/ControlIDApi/Group.cs` | Representa grupos utilizados para organização de acesso. |
| `Models/ControlIDApi/HardwareStatus.cs` | Representa o status geral de componentes físicos do equipamento. |
| `Models/ControlIDApi/HashPasswordResponse.cs` | Representa resposta de geração/validação de hash de senha. |
| `Models/ControlIDApi/Logo.cs` | Representa dados de logo/imagem do equipamento. |
| `Models/ControlIDApi/MonitorEvent.cs` | Representa eventos monitorados em tempo real ou via callback. |
| `Models/ControlIDApi/OfficialApiEndpointDefinition.cs` | Define metadados de endpoints oficiais, parâmetros, método HTTP e documentação visual. |
| `Models/ControlIDApi/OfficialApiInvocationResult.cs` | Representa o resultado de uma invocação genérica da API oficial. |
| `Models/ControlIDApi/Photo.cs` | Representa foto ou imagem associada a usuário/midia. |
| `Models/ControlIDApi/PushCommand.cs` | Representa comandos push enfileirados ou recebidos. |
| `Models/ControlIDApi/QRCode.cs` | Representa QR Codes de acesso. |
| `Models/ControlIDApi/RemoteAction.cs` | Representa uma ação remota solicitada ao equipamento. |
| `Models/ControlIDApi/RemoteActionResult.cs` | Representa o retorno de execução de uma ação remota. |
| `Models/ControlIDApi/SessionInfo.cs` | Representa dados de sessão/autenticação com o equipamento. |
| `Models/ControlIDApi/SystemInfo.cs` | Representa informações de sistema, rede e ambiente do equipamento. |
| `Models/ControlIDApi/User.cs` | Representa usuário no formato usado pelos fluxos de integração. |

## Models/Database

Entidades persistidas no SQLite local para histórico, cache operacional, simulacoes e suporte a UI.

| Arquivo | Responsabilidade |
| --- | --- |
| `Models/Database/AccessLogLocal.cs` | Entidade local para logs de acesso. |
| `Models/Database/AccessRuleLocal.cs` | Entidade local para regras de acesso. |
| `Models/Database/BiometricTemplateLocal.cs` | Entidade local para templates biométricos. |
| `Models/Database/CardLocal.cs` | Entidade local para cartões. |
| `Models/Database/ChangeLogLocal.cs` | Entidade local para logs de alteração. |
| `Models/Database/ConfigLocal.cs` | Entidade local para configurações. |
| `Models/Database/DeviceLocal.cs` | Entidade local para dispositivos. |
| `Models/Database/GroupLocal.cs` | Entidade local para grupos. |
| `Models/Database/LogLocal.cs` | Entidade local genérica para registros de log/auditoria. |
| `Models/Database/LogoLocal.cs` | Entidade local para logos/imagens. |
| `Models/Database/MonitorEventLocal.cs` | Entidade local para eventos monitorados por push/webhook. |
| `Models/Database/PhotoLocal.cs` | Entidade local para fotos e imagens. |
| `Models/Database/PushCommandLocal.cs` | Entidade local para comandos push. |
| `Models/Database/QRCodeLocal.cs` | Entidade local para QR Codes. |
| `Models/Database/SessionLocal.cs` | Entidade local para sessões e estado de autenticação. |
| `Models/Database/SyncLocal.cs` | Entidade local para estado de sincronização. |
| `Models/Database/UserLocal.cs` | Entidade local para usuários. |

## Monitor

| Arquivo | Responsabilidade |
| --- | --- |
| `Monitor/MonitorEventHandler.cs` | Processa eventos monitorados e encaminha para persistência/fila. |
| `Monitor/MonitorEventMapper.cs` | Converte payloads de monitoramento para modelos internos/localmente persistíveis. |
| `Monitor/MonitorEventQueue.cs` | Mantém fila em memória para eventos recebidos e processamento assíncrono. |

## Options

| Arquivo | Responsabilidade |
| --- | --- |
| `Options/CallbackSecurityOptions.cs` | Representa as configurações de segurança aplicadas aos callbacks/webhooks. |

## Services/Callbacks

| Arquivo | Responsabilidade |
| --- | --- |
| `Services/Callbacks/CallbackIngressService.cs` | Orquestra o recebimento, validação e persistência de callbacks. |
| `Services/Callbacks/CallbackRequestBodyReader.cs` | Lê o corpo bruto das requisições de callback de forma reutilizável. |
| `Services/Callbacks/CallbackSecurityEvaluator.cs` | Avalia regras de segurança, chave compartilhada e origem permitida dos callbacks. |

## Services/ControlIDApi

| Arquivo | Responsabilidade |
| --- | --- |
| `Services/ControlIDApi/IOfficialControlIdApiService.cs` | Contrato da camada de cliente HTTP para chamadas oficiais a API Control iD. |
| `Services/ControlIDApi/OfficialApiBinaryFileResultFactory.cs` | Monta respostas de arquivo/binário para resultados oficiais que precisam de download. |
| `Services/ControlIDApi/OfficialApiBodyParameterStrategy.cs` | Define estratégia de montagem de parâmetros enviados no corpo da requisição. |
| `Services/ControlIDApi/OfficialApiCatalogService.cs` | Disponibiliza o catálogo navegável de endpoints oficiais implementados/documentados. |
| `Services/ControlIDApi/OfficialApiContractDocumentationService.cs` | Gera a apresentação dos contratos oficiais de entrada/saída. |
| `Services/ControlIDApi/OfficialApiDocumentationSeedCatalog.cs` | Semeia metadados e documentação base dos endpoints oficiais. |
| `Services/ControlIDApi/OfficialApiDocumentationService.cs` | Consolida documentação, exemplos e metadados para exibição na UI. |
| `Services/ControlIDApi/OfficialApiInvokerService.cs` | Executa chamadas genéricas aos endpoints oficiais a partir do catálogo. |
| `Services/ControlIDApi/OfficialApiParameterDocumentationUtilities.cs` | Utilitários para documentar parâmetros, tipos e obrigatoriedade. |
| `Services/ControlIDApi/OfficialApiQueryParameterStrategy.cs` | Define estratégia de montagem de parâmetros via query string. |
| `Services/ControlIDApi/OfficialApiResultPresentationService.cs` | Prepara resultados oficiais para exibição amigável na interface. |
| `Services/ControlIDApi/OfficialControlIdApiService.cs` | Implementa o cliente HTTP oficial, autenticação, envio de payloads e leitura de respostas. |
| `Services/ControlIDApi/README.md` | Documentação específica da camada oficial de API e organização dos serviços. |

## Services/Database

Repositórios que encapsulam acesso ao SQLite local para cada entidade da PoC.

| Arquivo | Responsabilidade |
| --- | --- |
| `Services/Database/AccessLogRepository.cs` | Persistência e consulta de logs de acesso. |
| `Services/Database/AccessRuleRepository.cs` | Persistência e consulta de regras de acesso. |
| `Services/Database/BiometricTemplateRepository.cs` | Persistência e consulta de templates biométricos. |
| `Services/Database/CardRepository.cs` | Persistência e consulta de cartões. |
| `Services/Database/ChangeLogRepository.cs` | Persistência e consulta de logs de alteração. |
| `Services/Database/ConfigRepository.cs` | Persistência e consulta de configurações locais. |
| `Services/Database/DeviceRepository.cs` | Persistência e consulta de dispositivos. |
| `Services/Database/GroupRepository.cs` | Persistência e consulta de grupos. |
| `Services/Database/LogRepository.cs` | Persistência e consulta de logs genéricos/auditoria. |
| `Services/Database/LogoRepository.cs` | Persistência e consulta de logos. |
| `Services/Database/MonitorEventRepository.cs` | Persistência e consulta de eventos monitorados. |
| `Services/Database/PhotoRepository.cs` | Persistência e consulta de fotos. |
| `Services/Database/PushCommandRepository.cs` | Persistência e consulta de comandos push. |
| `Services/Database/QRCodeRepository.cs` | Persistência e consulta de QR Codes. |
| `Services/Database/SessionRepository.cs` | Persistência e consulta de sessões. |
| `Services/Database/SyncRepository.cs` | Persistência e consulta do estado de sincronização. |
| `Services/Database/UserRepository.cs` | Persistência e consulta de usuários. |

## Services complementares

| Arquivo | Responsabilidade |
| --- | --- |
| `Services/DocumentedFeatures/DocumentedFeaturesPayloadFactory.cs` | Monta o payload/resumo de funcionalidades documentadas exibido na UI. |
| `Services/Files/UploadedFileBase64Encoder.cs` | Converte arquivos enviados pela UI para Base64 antes de envio/persistência. |
| `Services/Analytics/ProductAnalyticsEventClassifier.cs` | Classifica rotas allowlist em eventos agregados de produto sem identificadores pessoais. |
| `Services/Navigation/NavigationCatalogService.cs` | Monta o catálogo de navegação das páginas e módulos da PoC. |
| `Services/Navigation/PageShellService.cs` | Fornece metadados de shell, cabeçalho e breadcrumbs das páginas. |
| `Services/Observability/HealthCheckResponseWriter.cs` | Serializa health checks sem expor excecoes, paths locais ou connection string. |
| `Services/Observability/ObservabilityConstants.cs` | Centraliza nomes de header, item de contexto e propriedades de escopo. |
| `Services/Observability/OperationalEventIds.cs` | Define IDs estaveis para eventos operacionais criticos. |
| `Services/Observability/OperationalMetrics.cs` | Publica metricas via `System.Diagnostics.Metrics` para coleta futura. |
| `Services/Observability/PrometheusMetricsWriter.cs` | Renderiza snapshot de metricas locais em formato Prometheus text para `/metrics`. |
| `Services/Observability/RuntimeCapacityMetricsProvider.cs` | Coleta gauges seguros de memoria, storage local e disco para FinOps/capacidade. |
| `Services/Observability/SqliteReadinessHealthCheck.cs` | Verifica readiness do SQLite local usado como estado runtime. |
| `Services/OperationModes/OperationModesPayloadFactory.cs` | Monta payloads demonstrativos dos modos Standalone, Pro e Enterprise. |
| `Services/OperationModes/OperationModesProfileResolver.cs` | Resolve perfis, comportamento esperado e transições dos modos de operação. |
| `Services/ProductSpecific/ProductSpecificCommandService.cs` | Executa comandos específicos por produto/modelo. |
| `Services/ProductSpecific/ProductSpecificConfigurationPayloadFactory.cs` | Monta payloads de configuração específicos por linha de produto. |
| `Services/ProductSpecific/ProductSpecificDownloadResult.cs` | Representa resultado de download em fluxos específicos de produto. |
| `Services/ProductSpecific/ProductSpecificJsonReader.cs` | Lê e interpreta JSONs usados por funcionalidades específicas. |
| `Services/ProductSpecific/ProductSpecificSections.cs` | Define secoes/categorias exibidas no modulo de recursos específicos. |
| `Services/ProductSpecific/ProductSpecificSnapshotService.cs` | Monta snapshots de estado/configuração específicos por produto. |
| `Services/Security/ControlIdInputSanitizer.cs` | Sanitiza entradas para reduzir risco de payloads invalidos ou inseguros. |

## ViewModels

As ViewModels carregam dados já preparados para as telas Razor, reduzindo regra de apresentação dentro das views.

| Arquivo | Responsabilidade |
| --- | --- |
| `ViewModels/AccessLogs/AccessLogDeleteViewModel.cs` | Dados da confirmação de exclusão de um log de acesso. |
| `ViewModels/AccessLogs/AccessLogFilterViewModel.cs` | Filtros aplicados na consulta de logs de acesso. |
| `ViewModels/AccessLogs/AccessLogListViewModel.cs` | Dados da tela de listagem de logs de acesso. |
| `ViewModels/AccessLogs/AccessLogViewModel.cs` | Dados de detalhe de um log de acesso. |
| `ViewModels/AccessRules/AccessRuleDeleteViewModel.cs` | Dados da confirmação de exclusão de regra de acesso. |
| `ViewModels/AccessRules/AccessRuleEditViewModel.cs` | Campos usados na criação/edição de regra de acesso. |
| `ViewModels/AccessRules/AccessRuleListViewModel.cs` | Dados da listagem de regras de acesso. |
| `ViewModels/AccessRules/AccessRuleViewModel.cs` | Dados de detalhe de uma regra de acesso. |
| `ViewModels/AdvancedOfficial/CameraCaptureViewModel.cs` | Dados do fluxo oficial de captura de camera. |
| `ViewModels/AdvancedOfficial/ExportObjectsViewModel.cs` | Dados do fluxo oficial de exportação de objetos. |
| `ViewModels/AdvancedOfficial/FacialEnrollViewModel.cs` | Dados do fluxo oficial de enroll facial. |
| `ViewModels/AdvancedOfficial/NetworkInterlockViewModel.cs` | Dados do fluxo de intertravamento/rede. |
| `ViewModels/AdvancedOfficial/RemoteLedControlViewModel.cs` | Dados do fluxo de controle remoto de LED. |
| `ViewModels/Auth/AuthStatusViewModel.cs` | Dados de status da autenticação atual. |
| `ViewModels/Auth/ChangePasswordViewModel.cs` | Campos de troca de senha. |
| `ViewModels/Auth/LoginViewModel.cs` | Campos de login e conexão. |
| `ViewModels/Auth/LogoutViewModel.cs` | Dados exibidos no encerramento de sessão. |
| `ViewModels/Auth/RegisterViewModel.cs` | Campos de registro/cadastro. |
| `ViewModels/BiometricTemplates/BiometricTemplateDeleteViewModel.cs` | Dados da confirmação de exclusão de template biométrico. |
| `ViewModels/BiometricTemplates/BiometricTemplateEditViewModel.cs` | Campos usados na criação/edição de template biométrico. |
| `ViewModels/BiometricTemplates/BiometricTemplateListViewModel.cs` | Dados da listagem de templates biométricos. |
| `ViewModels/BiometricTemplates/BiometricTemplateViewModel.cs` | Dados de detalhe de template biométrico. |
| `ViewModels/Cards/CardDeleteViewModel.cs` | Dados da confirmação de exclusão de cartão. |
| `ViewModels/Cards/CardEditViewModel.cs` | Campos usados na criação/edição de cartão. |
| `ViewModels/Cards/CardListViewModel.cs` | Dados da listagem de cartões. |
| `ViewModels/Cards/CardViewModel.cs` | Dados de detalhe de cartão. |
| `ViewModels/Catra/CatraEventListViewModel.cs` | Dados da listagem de eventos de catraca. |
| `ViewModels/Catra/CatraEventViewModel.cs` | Dados de detalhe de evento de catraca. |
| `ViewModels/Catra/CatraOpenViewModel.cs` | Dados do comando de abertura de catraca. |
| `ViewModels/ChangeLogs/ChangeLogDeleteViewModel.cs` | Dados da confirmação de exclusão de log de alteração. |
| `ViewModels/ChangeLogs/ChangeLogListViewModel.cs` | Dados da listagem de logs de alteração. |
| `ViewModels/ChangeLogs/ChangeLogViewModel.cs` | Dados de detalhe de log de alteração. |
| `ViewModels/Config/ConfigDeleteViewModel.cs` | Dados da confirmação de exclusão de configuração. |
| `ViewModels/Config/ConfigDiagnosticsViewModel.cs` | Dados da tela de diagnóstico de configurações. |
| `ViewModels/Config/ConfigEditViewModel.cs` | Campos usados na criação/edição de configuração. |
| `ViewModels/Config/ConfigListViewModel.cs` | Dados da listagem de configurações. |
| `ViewModels/Config/ConfigOfficialViewModel.cs` | Dados da tela de configurações oficiais da API. |
| `ViewModels/Config/ConfigViewModel.cs` | Dados de detalhe de configuração. |
| `ViewModels/Devices/DeviceDeleteViewModel.cs` | Dados da confirmação de exclusão de dispositivo. |
| `ViewModels/Devices/DeviceEditViewModel.cs` | Campos usados na criação/edição de dispositivo. |
| `ViewModels/Devices/DeviceListViewModel.cs` | Dados da listagem de dispositivos. |
| `ViewModels/Devices/DeviceViewModel.cs` | Dados de detalhe de dispositivo. |
| `ViewModels/DocumentedFeatures/DocumentedFeaturesViewModel.cs` | Dados consolidados das funcionalidades documentadas/implementadas. |
| `ViewModels/Errors/ErrorListViewModel.cs` | Dados da listagem de erros. |
| `ViewModels/Errors/ErrorViewModel.cs` | Dados de detalhe de erro. |
| `ViewModels/Groups/GroupDeleteViewModel.cs` | Dados da confirmação de exclusão de grupo. |
| `ViewModels/Groups/GroupEditViewModel.cs` | Campos usados na criação/edição de grupo. |
| `ViewModels/Groups/GroupListViewModel.cs` | Dados da listagem de grupos. |
| `ViewModels/Groups/GroupViewModel.cs` | Dados de detalhe de grupo. |
| `ViewModels/Hardware/BiometryValidationViewModel.cs` | Dados do fluxo de validação biométrica. |
| `ViewModels/Hardware/DoorStateViewModel.cs` | Dados de estado de porta. |
| `ViewModels/Hardware/GpioStateViewModel.cs` | Dados de estado GPIO. |
| `ViewModels/Hardware/HardwareStatusViewModel.cs` | Dados de status geral de hardware. |
| `ViewModels/Hardware/RelayActionViewModel.cs` | Dados de acionamento de relé. |
| `ViewModels/Home/HomeDashboardViewModel.cs` | Dados do dashboard inicial. |
| `ViewModels/Logo/LogoDeleteViewModel.cs` | Dados da confirmação de exclusão de logo. |
| `ViewModels/Logo/LogoListViewModel.cs` | Dados da listagem de logos. |
| `ViewModels/Logo/LogoUploadViewModel.cs` | Campos do upload de logo. |
| `ViewModels/Logo/LogoViewModel.cs` | Dados de detalhe de logo. |
| `ViewModels/Media/AdVideoManageViewModel.cs` | Dados do gerenciamento de vídeo de propaganda. |
| `ViewModels/Media/PhotoDeleteViewModel.cs` | Dados da confirmação de exclusão de foto. |
| `ViewModels/Media/PhotoListViewModel.cs` | Dados da listagem de fotos. |
| `ViewModels/Media/PhotoUploadViewModel.cs` | Campos do upload de foto. |
| `ViewModels/Media/PhotoViewModel.cs` | Dados de detalhe de foto. |
| `ViewModels/Monitor/MonitorPushListViewModel.cs` | Dados da listagem de eventos push monitorados. |
| `ViewModels/Monitor/MonitorWebhookListViewModel.cs` | Dados da listagem de eventos webhook monitorados. |
| `ViewModels/Monitor/PushEventViewModel.cs` | Dados de detalhe de evento push. |
| `ViewModels/Monitor/WebhookEventViewModel.cs` | Dados de detalhe de evento webhook. |
| `ViewModels/OfficialApi/OfficialApiContractViewModel.cs` | Dados de contrato de endpoint oficial. |
| `ViewModels/OfficialApi/OfficialApiIndexViewModel.cs` | Dados do catálogo oficial de endpoints. |
| `ViewModels/OfficialApi/OfficialApiInvokeViewModel.cs` | Dados do formulário e resultado de invocação oficial. |
| `ViewModels/OfficialObjects/OfficialObjectsViewModel.cs` | Dados da tela de exploracao de objetos oficiais. |
| `ViewModels/OperationModes/OperationModesViewModel.cs` | Dados da tela de modos Standalone, Pro e Enterprise. |
| `ViewModels/ProductSpecific/ProductSpecificViewModel.cs` | Dados da tela de recursos específicos por produto. |
| `ViewModels/Push/PushEventListViewModel.cs` | Dados da listagem de eventos push. |
| `ViewModels/Push/PushEventViewModel.cs` | Dados de detalhe de evento push. |
| `ViewModels/Push/PushQueueCommandViewModel.cs` | Campos para enfileirar comando push. |
| `ViewModels/QRCodes/QRCodeDeleteViewModel.cs` | Dados da confirmação de exclusão de QR Code. |
| `ViewModels/QRCodes/QRCodeEditViewModel.cs` | Campos usados na criação/edição de QR Code. |
| `ViewModels/QRCodes/QRCodeListViewModel.cs` | Dados da listagem de QR Codes. |
| `ViewModels/QRCodes/QRCodeViewModel.cs` | Dados de detalhe de QR Code. |
| `ViewModels/RemoteActions/RemoteActionExecuteViewModel.cs` | Dados do fluxo de execução de ação remota. |
| `ViewModels/RemoteActions/RemoteActionListViewModel.cs` | Dados da listagem de ações remotas. |
| `ViewModels/RemoteActions/RemoteActionViewModel.cs` | Dados de detalhe de ação remota. |
| `ViewModels/RemoteActions/RemoteAuthorizationViewModel.cs` | Dados do fluxo de autorização remota. |
| `ViewModels/RemoteActions/RemoteEnrollViewModel.cs` | Dados do fluxo de enroll remoto. |
| `ViewModels/Session/SessionCreateViewModel.cs` | Campos de criação de sessão. |
| `ViewModels/Session/SessionDeactivateViewModel.cs` | Dados de desativacao/encerramento de sessão. |
| `ViewModels/Session/SessionEditViewModel.cs` | Campos de edição de sessão. |
| `ViewModels/Session/SessionListViewModel.cs` | Dados da listagem de sessões. |
| `ViewModels/Session/SessionStatusViewModel.cs` | Dados de status da sessão. |
| `ViewModels/Session/SessionViewModel.cs` | Dados de detalhe de sessão. |
| `ViewModels/Shared/AppPageHeaderViewModel.cs` | Dados do cabeçalho padrao das páginas. |
| `ViewModels/Shared/NavigationViewModels.cs` | Modelos compartilhados de navegação, menus e itens do shell. |
| `ViewModels/Shared/RawResponsePanelViewModel.cs` | Dados do painel reutilizável de resposta bruta. |
| `ViewModels/System/HashPasswordViewModel.cs` | Campos e resultado de hash de senha. |
| `ViewModels/System/SystemActionResultViewModel.cs` | Dados do resultado de uma ação de sistema. |
| `ViewModels/System/SystemInfoViewModel.cs` | Dados de informações gerais do sistema. |
| `ViewModels/System/SystemLoginCredentialsViewModel.cs` | Campos de credenciais de login do sistema/equipamento. |
| `ViewModels/System/SystemNetworkViewModel.cs` | Dados de configuração/consulta de rede. |
| `ViewModels/System/SystemVpnViewModel.cs` | Dados de configuração/consulta de VPN. |
| `ViewModels/Users/HashPasswordResponse.cs` | Estrutura de resposta de hash de senha usada nos fluxos de usuário. |
| `ViewModels/Users/UserDeleteViewModel.cs` | Dados da confirmação de exclusão de usuário. |
| `ViewModels/Users/UserDto.cs` | DTO auxiliar para transferencia de dados de usuário. |
| `ViewModels/Users/UserEditViewModel.cs` | Campos usados na criação/edição de usuário. |
| `ViewModels/Users/UserListViewModel.cs` | Dados da listagem de usuários. |
| `ViewModels/Users/UsersApiResponse.cs` | Estrutura de resposta agregada da API para usuários. |
| `ViewModels/Users/UserViewModel.cs` | Dados de detalhe de usuário. |
| `ViewModels/Workspace/WorkspaceExplorerViewModel.cs` | Dados do explorador/workspace operacional. |

## Views

As views Razor compõem a interface web da PoC. Em geral, cada pasta espelha um controller e cada arquivo `.cshtml` representa uma tela ou parcial reutilizável.

| Arquivo | Responsabilidade |
| --- | --- |
| `Views/_ViewImports.cshtml` | Importa namespaces e tag helpers disponiveis para todas as views. |
| `Views/_ViewStart.cshtml` | Define o layout padrao usado pelas views. |
| `Views/AccessLogs/Delete.cshtml` | Tela de confirmação de exclusão de log de acesso. |
| `Views/AccessLogs/Details.cshtml` | Tela de detalhe de log de acesso. |
| `Views/AccessLogs/Index.cshtml` | Tela de listagem/filtro de logs de acesso. |
| `Views/AccessRules/Create.cshtml` | Tela de criação de regra de acesso. |
| `Views/AccessRules/Delete.cshtml` | Tela de confirmação de exclusão de regra de acesso. |
| `Views/AccessRules/Details.cshtml` | Tela de detalhe de regra de acesso. |
| `Views/AccessRules/Edit.cshtml` | Tela de edição de regra de acesso. |
| `Views/AccessRules/Index.cshtml` | Tela de listagem de regras de acesso. |
| `Views/AdvancedOfficial/CameraCapture.cshtml` | Tela do fluxo oficial de captura de camera. |
| `Views/AdvancedOfficial/ExportObjects.cshtml` | Tela do fluxo oficial de exportação de objetos. |
| `Views/AdvancedOfficial/FacialEnroll.cshtml` | Tela do fluxo oficial de enroll facial. |
| `Views/AdvancedOfficial/Index.cshtml` | Tela inicial dos recursos oficiais avançados. |
| `Views/AdvancedOfficial/NetworkInterlock.cshtml` | Tela do fluxo de intertravamento/rede. |
| `Views/AdvancedOfficial/RemoteLedControl.cshtml` | Tela do fluxo de controle remoto de LED. |
| `Views/Auth/ChangePassword.cshtml` | Tela de troca de senha. |
| `Views/Auth/Login.cshtml` | Tela de login/conexão. |
| `Views/Auth/Logout.cshtml` | Tela de encerramento de sessão. |
| `Views/Auth/Register.cshtml` | Tela de registro/cadastro. |
| `Views/Auth/Status.cshtml` | Tela de status da autenticação. |
| `Views/BiometricTemplates/Create.cshtml` | Tela de criação de template biométrico. |
| `Views/BiometricTemplates/Delete.cshtml` | Tela de confirmação de exclusão de template biométrico. |
| `Views/BiometricTemplates/Details.cshtml` | Tela de detalhe de template biométrico. |
| `Views/BiometricTemplates/Edit.cshtml` | Tela de edição de template biométrico. |
| `Views/BiometricTemplates/Index.cshtml` | Tela de listagem de templates biométricos. |
| `Views/Cards/Create.cshtml` | Tela de criação de cartão. |
| `Views/Cards/Delete.cshtml` | Tela de confirmação de exclusão de cartão. |
| `Views/Cards/Details.cshtml` | Tela de detalhe de cartão. |
| `Views/Cards/Edit.cshtml` | Tela de edição de cartão. |
| `Views/Cards/Index.cshtml` | Tela de listagem de cartões. |
| `Views/Catra/Delete.cshtml` | Tela de confirmação de exclusão de evento/registro de catraca. |
| `Views/Catra/Details.cshtml` | Tela de detalhe de evento de catraca. |
| `Views/Catra/Index.cshtml` | Tela de listagem/operação de catraca. |
| `Views/ChangeLogs/Delete.cshtml` | Tela de confirmação de exclusão de log de alteração. |
| `Views/ChangeLogs/Details.cshtml` | Tela de detalhe de log de alteração. |
| `Views/ChangeLogs/Index.cshtml` | Tela de listagem de logs de alteração. |
| `Views/Config/Create.cshtml` | Tela de criação de configuração. |
| `Views/Config/Delete.cshtml` | Tela de confirmação de exclusão de configuração. |
| `Views/Config/Details.cshtml` | Tela de detalhe de configuração. |
| `Views/Config/Diagnostics.cshtml` | Tela de diagnóstico de configurações. |
| `Views/Config/Edit.cshtml` | Tela de edição de configuração. |
| `Views/Config/Index.cshtml` | Tela de listagem de configurações. |
| `Views/Config/Official.cshtml` | Tela de configurações oficiais da API. |
| `Views/Devices/Create.cshtml` | Tela de criação de dispositivo. |
| `Views/Devices/Delete.cshtml` | Tela de confirmação de exclusão de dispositivo. |
| `Views/Devices/Details.cshtml` | Tela de detalhe de dispositivo. |
| `Views/Devices/Edit.cshtml` | Tela de edição de dispositivo. |
| `Views/Devices/Index.cshtml` | Tela de listagem de dispositivos. |
| `Views/DocumentedFeatures/Index.cshtml` | Tela consolidada de funcionalidades documentadas/implementadas. |
| `Views/Errors/Details.cshtml` | Tela de detalhe de erro. |
| `Views/Errors/Index.cshtml` | Tela de listagem de erros. |
| `Views/Groups/Create.cshtml` | Tela de criação de grupo. |
| `Views/Groups/Delete.cshtml` | Tela de confirmação de exclusão de grupo. |
| `Views/Groups/Details.cshtml` | Tela de detalhe de grupo. |
| `Views/Groups/Edit.cshtml` | Tela de edição de grupo. |
| `Views/Groups/Index.cshtml` | Tela de listagem de grupos. |
| `Views/Hardware/DoorState.cshtml` | Tela de estado de porta. |
| `Views/Hardware/Gpio.cshtml` | Tela de consulta/ação GPIO. |
| `Views/Hardware/RelayAction.cshtml` | Tela de acionamento de relé. |
| `Views/Hardware/Status.cshtml` | Tela de status de hardware. |
| `Views/Hardware/ValidateBiometry.cshtml` | Tela de validação biométrica. |
| `Views/Home/About.cshtml` | Tela institucional/sobre a PoC. |
| `Views/Home/Contact.cshtml` | Tela de contato/referências. |
| `Views/Home/Index.cshtml` | Dashboard inicial da PoC. |
| `Views/Logo/Delete.cshtml` | Tela de confirmação de exclusão de logo. |
| `Views/Logo/Details.cshtml` | Tela de detalhe de logo. |
| `Views/Logo/Index.cshtml` | Tela de listagem de logos. |
| `Views/Logo/Upload.cshtml` | Tela de upload de logo. |
| `Views/Media/AdMode.cshtml` | Tela de gerenciamento de vídeo/modo propaganda. |
| `Views/Media/Delete.cshtml` | Tela de confirmação de exclusão de midia. |
| `Views/Media/Details.cshtml` | Tela de detalhe de midia/foto. |
| `Views/Media/Index.cshtml` | Tela de listagem de mídias/fotos. |
| `Views/Media/Upload.cshtml` | Tela de upload de midia/foto. |
| `Views/Monitor/Push.cshtml` | Tela de monitoramento de eventos push. |
| `Views/Monitor/PushDetails.cshtml` | Tela de detalhe de evento push monitorado. |
| `Views/Monitor/Webhook.cshtml` | Tela de monitoramento de webhooks/callbacks. |
| `Views/Monitor/WebhookDetails.cshtml` | Tela de detalhe de webhook/callback recebido. |
| `Views/OfficialApi/Index.cshtml` | Tela do catálogo oficial de endpoints. |
| `Views/OfficialApi/Invoke.cshtml` | Tela de invocação dinamica de endpoint oficial. |
| `Views/OfficialEvents/Details.cshtml` | Tela de detalhe de evento oficial. |
| `Views/OfficialEvents/Index.cshtml` | Tela de listagem de eventos oficiais. |
| `Views/OfficialObjects/Index.cshtml` | Tela de exploracao de objetos oficiais. |
| `Views/OperationModes/Index.cshtml` | Tela dos modos Standalone, Pro e Enterprise. |
| `Views/ProductSpecific/Index.cshtml` | Tela de recursos específicos por produto. |
| `Views/PushCenter/Details.cshtml` | Tela de detalhe de item/comando da central de push. |
| `Views/PushCenter/Index.cshtml` | Tela centralizada de eventos e comandos push. |
| `Views/QRCodes/Create.cshtml` | Tela de criação de QR Code. |
| `Views/QRCodes/Delete.cshtml` | Tela de confirmação de exclusão de QR Code. |
| `Views/QRCodes/Details.cshtml` | Tela de detalhe de QR Code. |
| `Views/QRCodes/Edit.cshtml` | Tela de edição de QR Code. |
| `Views/QRCodes/Index.cshtml` | Tela de listagem de QR Codes. |
| `Views/RemoteActions/Authorization.cshtml` | Tela de autorização remota. |
| `Views/RemoteActions/Details.cshtml` | Tela de detalhe de ação remota. |
| `Views/RemoteActions/Enroll.cshtml` | Tela de enroll remoto. |
| `Views/RemoteActions/Execute.cshtml` | Tela de execução de ação remota. |
| `Views/RemoteActions/Index.cshtml` | Tela de listagem de ações remotas. |
| `Views/Session/Delete.cshtml` | Tela de encerramento/exclusão de sessão. |
| `Views/Session/Details.cshtml` | Tela de detalhe de sessão. |
| `Views/Session/Index.cshtml` | Tela de listagem de sessões. |
| `Views/Session/Status.cshtml` | Tela de status de sessão. |
| `Views/Shared/_AccessDenied.cshtml` | Parcial de acesso negado. |
| `Views/Shared/_AppPageHeader.cshtml` | Parcial de cabeçalho padrao das páginas. |
| `Views/Shared/_ConnectionPanel.cshtml` | Parcial do painel de conexão/status do equipamento. |
| `Views/Shared/_EndpointContractPanel.cshtml` | Parcial de exibição de contrato de endpoint. |
| `Views/Shared/_Layout.cshtml` | Layout principal da aplicação. |
| `Views/Shared/_Layout.cshtml.css` | Estilos escopados do layout principal. |
| `Views/Shared/_NavBar.cshtml` | Parcial da barra de navegação principal. |
| `Views/Shared/_NavBar.cshtml.css` | Estilos escopados da barra de navegação. |
| `Views/Shared/_NotFound.cshtml` | Parcial de recurso não encontrado. |
| `Views/Shared/_RawResponsePanel.cshtml` | Parcial de exibição de resposta bruta JSON/texto. |
| `Views/Shared/_ServerError.cshtml` | Parcial de erro interno. |
| `Views/Shared/_StatusMessage.cshtml` | Parcial de mensagens de status/sucesso/erro. |
| `Views/Shared/_TopNavigation.cshtml` | Parcial de navegação superior. |
| `Views/Shared/_ValidationScriptsPartial.cshtml` | Parcial com scripts de validação client-side. |
| `Views/Shared/Error.cshtml` | Tela genérica de erro MVC. |
| `Views/System/ActionResult.cshtml` | Tela de resultado de ação administrativa/sistema. |
| `Views/System/HashPassword.cshtml` | Tela de geração/validação de hash de senha. |
| `Views/System/Info.cshtml` | Tela de informações de sistema. |
| `Views/System/LoginCredentials.cshtml` | Tela de credenciais de login do sistema/equipamento. |
| `Views/System/Network.cshtml` | Tela de configuração/consulta de rede. |
| `Views/System/Vpn.cshtml` | Tela de configuração/consulta de VPN. |
| `Views/Users/Create.cshtml` | Tela de criação de usuário. |
| `Views/Users/Delete.cshtml` | Tela de confirmação de exclusão de usuário. |
| `Views/Users/Details.cshtml` | Tela de detalhe de usuário. |
| `Views/Users/Edit.cshtml` | Tela de edição de usuário. |
| `Views/Users/Index.cshtml` | Tela de listagem de usuários. |
| `Views/Workspace/Domain.cshtml` | Tela de dominio/área específica do workspace. |
| `Views/Workspace/Index.cshtml` | Tela principal do workspace/explorador operacional. |

## wwwroot

| Arquivo/Pasta | Responsabilidade |
| --- | --- |
| `wwwroot/css/site.css` | Estilos globais da PoC, componentes visuais, dashboard, tabelas, formularios e ajustes responsivos. |
| `wwwroot/js/site.js` | JavaScript global da UI, comportamentos de interacao e utilidades client-side. |
| `wwwroot/favicon.ico` | Icone exibido pelo navegador para a aplicação. |
| `wwwroot/lib/bootstrap/*` | Arquivos CSS/JS do Bootstrap, incluindo versoes minificadas, sourcemaps, utilitários e licenças. |
| `wwwroot/lib/jquery/*` | Arquivos da biblioteca jQuery usados pela camada client-side. |
| `wwwroot/lib/jquery-validation/*` | Biblioteca de validação jQuery usada nos formularios. |
| `wwwroot/lib/jquery-validation-unobtrusive/*` | Adaptadores unobtrusive validation usados com ASP.NET Core MVC/Razor. |

## docs

| Arquivo/Pasta | Responsabilidade |
| --- | --- |
| `docs/README.md` | Indice central da documentacao tecnica por papel e por tema. |
| `docs/adrs/` | Registros de decisoes arquiteturais aceitas e suas consequencias. |
| `docs/architecture-overview.md` | Visao de camadas, fluxos criticos, dependencias, trust boundaries e contratos a preservar. |
| `docs/changelog-2026-04-14.md` | Registro resumido de evolucoes relevantes realizadas na PoC. |
| `docs/changelog-2026-04-15.md` | Registro resumido das atualizacoes de documentação, comentarios inline e observabilidade. |
| `docs/changelog-2026-05-01.md` | Registro da rodada de documentacao, onboarding e ADRs. |
| `docs/developer-onboarding.md` | Guia de setup, execucao, desenvolvimento, diagnostico e entrega segura para novos contribuidores. |
| `docs/deployment-runbook.md` | Mapeia ambientes, container, variaveis obrigatorias, deploy, rollback e riscos de infraestrutura. |
| `docs/documentation-audit-2026-05-01.md` | Auditoria de documentacao, achados, consistencia e lacunas restantes. |
| `docs/equipment-contingency-runbook.md` | Define contingencia operacional do equipamento Control iD, fallback manual e validacao de bancada. |
| `docs/external-validation-runbook.md` | Padroniza SAST, OSV, DAST, acessibilidade e contrato com stub/equipamento. |
| `docs/finops-capacity.md` | Define inventario de custos, capacidade, limites, governanca FinOps e sustentabilidade operacional. |
| `docs/incident-response-and-dr.md` | Define matriz SEV, runbooks de incidentes, continuidade, backup/restore operacional, DR, comunicacao e postmortem. |
| `docs/monitor-implementation.md` | Documenta a implementação da funcionalidade Monitor, callbacks oficiais, segurança e persistência local. |
| `docs/observability-runbook.md` | Define health, metricas, alertas, dashboards e resposta a incidentes operacionais. |
| `docs/observability/alert-rules.json` | Regras versionadas de alerta para o monitor local e ferramentas externas. |
| `docs/observability/dashboard.json` | Especificacao versionada de dashboards independente de fornecedor. |
| `docs/operation-modes-implementation.md` | Documenta a implementação dos modos Standalone, Pro e Enterprise, incluindo payloads e transições. |
| `docs/product-analytics.md` | Define objetivos, KPIs, funis, eventos, dashboards e restricoes de analytics privacy-aware. |
| `docs/pr-summary-2026-05-01.md` | Resumo de PR/release notes da rodada documental. |
| `docs/project-file-responsibilities.md` | Este inventário de responsabilidades por pasta e arquivo. |
| `docs/push-implementation.md` | Documenta a implementação da funcionalidade Push, fila persistida, polling e retorno de resultados. |
| `docs/residual-risk-closure.md` | Mapeia riscos residuais externos para campos obrigatorios, gates e evidencias de release. |
| `docs/reports/controlid-api-audit-2026-04-13.md` | Auditoria tecnica da cobertura da API Control iD. |
| `docs/reports/design-system-accessibility-audit-2026-04-14.md` | Auditoria de design system e acessibilidade da UI. |
| `docs/reports/heuristic-ui-audit-2026-04-14.md` | Avaliacao heuristica inicial da interface. |
| `docs/reports/localhost-smoke-test-2026-04-13.md` | Relatório de smoke test local da rodada de 13/04/2026. |
| `docs/reports/localhost-smoke-test-2026-04-14.md` | Relatório de smoke test local da rodada de 14/04/2026. |
| `docs/reports/operation-modes-e2e-runbook-2026-04-14.md` | Roteiro E2E para validação dos modos de operação. |
| `docs/reports/operation-modes-homologation-matrix-2026-04-14.md` | Matriz de homologação dos modos Standalone, Pro e Enterprise. |
| `docs/reports/visual-inventory-2026-04-14.md` | Inventário visual das telas e estados avaliados. |

## tests

| Arquivo/Pasta | Responsabilidade |
| --- | --- |
| `tests/Integracao.ControlID.PoC.Tests/Integracao.ControlID.PoC.Tests.csproj` | Projeto de testes unitários xUnit da PoC. |
| `tests/Integracao.ControlID.PoC.Tests/Helpers/ApiResponseHelperTests.cs` | Testa normalização e interpretação de respostas da API. |
| `tests/Integracao.ControlID.PoC.Tests/Helpers/CryptoHelperTests.cs` | Testa comportamentos criptográficos e hash. |
| `tests/Integracao.ControlID.PoC.Tests/Helpers/FileHelperTests.cs` | Testa utilitários de arquivo. |
| `tests/Integracao.ControlID.PoC.Tests/Helpers/HttpHelperTests.cs` | Testa utilitários HTTP. |
| `tests/Integracao.ControlID.PoC.Tests/Helpers/ProductSpecificPresentationHelperTests.cs` | Testa utilitários de apresentação de recursos específicos por produto. |
| `tests/Integracao.ControlID.PoC.Tests/Helpers/SecurityTextHelperTests.cs` | Testa mascaramento e textos de segurança. |
| `tests/Integracao.ControlID.PoC.Tests/Helpers/SessionHelperTests.cs` | Testa manipulação auxiliar de sessão. |
| `tests/Integracao.ControlID.PoC.Tests/Mappings/ViewModelMappingsTests.cs` | Testa conversões para ViewModels. |
| `tests/Integracao.ControlID.PoC.Tests/Middlewares/SecurityHeadersMiddlewareTests.cs` | Testa aplicação de cabeçalhos de segurança. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Callbacks/CallbackIngressServiceTests.cs` | Testa orquestração de callbacks recebidos. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Callbacks/CallbackRequestBodyReaderTests.cs` | Testa leitura reutilizável do corpo bruto de callbacks. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Callbacks/CallbackSecurityEvaluatorTests.cs` | Testa validação de segurança dos callbacks. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialApiBinaryFileResultFactoryTests.cs` | Testa geração de resultados binários/download para chamadas oficiais. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialApiBodyParameterStrategyTests.cs` | Testa montagem de parâmetros no body. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialApiCatalogServiceTests.cs` | Testa catálogo de endpoints oficiais. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialApiContractDocumentationServiceTests.cs` | Testa documentação visual de contratos. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialApiInvokerServiceTests.cs` | Testa invocação genérica de endpoints oficiais. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialApiParameterDocumentationUtilitiesTests.cs` | Testa utilitários de documentação de parâmetros. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialApiQueryParameterStrategyTests.cs` | Testa montagem de parâmetros via query string. |
| `tests/Integracao.ControlID.PoC.Tests/Services/DocumentedFeatures/DocumentedFeaturesPayloadFactoryTests.cs` | Testa payload de funcionalidades documentadas. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Files/UploadedFileBase64EncoderTests.cs` | Testa conversão de uploads para Base64. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Analytics/ProductAnalyticsEventClassifierTests.cs` | Testa classificacao privacy-aware de eventos de produto por rota. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Navigation/NavigationCatalogServiceTests.cs` | Testa catálogo de navegação. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Navigation/PageShellServiceTests.cs` | Testa metadados de shell/cabeçalho das páginas. |
| `tests/Integracao.ControlID.PoC.Tests/Services/OperationModes/OperationModesPayloadFactoryTests.cs` | Testa payloads dos modos de operação. |
| `tests/Integracao.ControlID.PoC.Tests/Services/OperationModes/OperationModesProfileResolverTests.cs` | Testa resolução de perfis dos modos de operação. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ProductSpecific/ProductSpecificCommandServiceTests.cs` | Testa comandos específicos por produto. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ProductSpecific/ProductSpecificConfigurationPayloadFactoryTests.cs` | Testa payloads de configuração específicos por produto. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ProductSpecific/ProductSpecificJsonReaderTests.cs` | Testa leitura e interpretação de JSONs específicos. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ProductSpecific/ProductSpecificSnapshotServiceTests.cs` | Testa montagem de snapshots específicos por produto. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Security/ControlIdInputSanitizerTests.cs` | Testa sanitização de entradas. |

## tools

| Arquivo/Pasta | Responsabilidade |
| --- | --- |
| `tools/observability-check.ps1` | Valida artefatos de observabilidade, health checks, metricas e contrato fisico opcional. |
| `tools/operational-readiness-check.ps1` | Valida runbooks operacionais, `ops.example.json` e, em release, `ops.local.json` sem placeholders. |
| `tools/backup-sqlite-operational.ps1` | Orquestra backup SQLite protegido, espelhamento opcional, restore-smoke e retencao confirmada. |
| `tools/contract-controlid-stub.ps1` | Sobe o stub local e valida contrato Control iD sem equipamento fisico ou credenciais reais. |
| `tools/external-security-scans.ps1` | Orquestra inventario e execucao de Semgrep, OSV Scanner, ZAP baseline e axe quando disponiveis. |
| `tools/finops-capacity-check.ps1` | Valida runbook, alertas, governanca e tamanhos locais de SQLite, logs, artifacts e reports sem apagar dados. |
| `tools/test-readiness-gates.ps1` | Orquestra build, testes, format, secret scan, observabilidade offline, FinOps/capacidade, cobertura, smoke, auditoria, contrato fisico, scanners externos e modo estrito `-ReleaseGate`. |
| `tools/smoke-localhost.ps1` | Script PowerShell que executa smoke test local, sobe stub e percorre fluxos criticos da PoC. |
| `tools/ControlIdDeviceStub/ControlIdDeviceStub.csproj` | Projeto .NET do stub local que simula respostas de um equipamento Control iD. |
| `tools/ControlIdDeviceStub/Program.cs` | Implementa os endpoints simulados usados pelos smoke tests locais. |
