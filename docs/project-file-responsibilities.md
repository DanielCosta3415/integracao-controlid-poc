# Responsabilidades dos arquivos do projeto

> **Inventário vivo** · Público: desenvolvimento e manutenção · Responsável: mantenedores · Última validação: 2026-08-04.

Este documento resume a responsabilidade dos arquivos versionados da PoC `Integracao.ControlID.PoC`.

O objetivo é servir como um mapa rápido de navegação para quem quiser entender a solução, localizar uma funcionalidade ou contribuir com o projeto sem precisar descobrir a estrutura apenas pelo código.

Observações de escopo:

- Arquivos gerados em `bin/`, `obj/`, banco SQLite local, logs e artefatos temporários não fazem parte deste inventário.
- Bibliotecas vendorizadas em `wwwroot/lib/` foram agrupadas por família, porque incluem variações minificadas, sourcemaps e licenças sem regra de negócio da PoC.
- As descrições abaixo são intencionais e resumidas; para detalhes de comportamento, consulte o código e os testes relacionados.

## Raiz da solução

| Arquivo | Responsabilidade |
| --- | --- |
| `.dockerignore` | Remove segredos, banco local, registros e artefatos do contexto de compilação Docker. |
| `.env.example` | Lista variáveis de ambiente seguras para criar `.env` local sem versionar segredos. |
| `Dockerfile` | Define compilação em múltiplos estágios e execução sem usuário raiz para a PoC em contêiner. |
| `appsettings.Staging.json` | Valores padrão seguros, sem segredos, para validação no ambiente `Staging`. |
| `appsettings.Production.json` | Valores padrão seguros, sem segredos, para produção; exige variáveis reais no ambiente. |
| `docker-compose.yml` | Executa a PoC em contêiner com volumes persistentes, verificação de integridade e variáveis obrigatórias. |
| `.editorconfig` | Padroniza convenções básicas de edição, formatação e estilo entre IDEs. |
| `.gitignore` | Define arquivos e pastas que não devem ser versionados, como compilações, registros e artefatos locais. |
| `.semgrep.yml` | Define regras locais e exclusões usadas pela análise estática externa. |
| `ops.example.json` | Exemplo versionado de configuração operacional para incidentes, plantão, cópia externa, RTO/RPO, implantação, privacidade, validação externa, contrato físico e FinOps. |
| `Directory.Build.props` | Centraliza propriedades comuns de compilação para os projetos .NET da solução. |
| `global.json` | Fixa a família do SDK .NET usada para restauração, compilação e testes reproduzíveis. |
| `Integracao.ControlID.PoC.csproj` | Define o projeto ASP.NET Core MVC principal, dependências NuGet e configurações de compilação. |
| `Integracao.ControlID.PoC.sln` | Agrupa o projeto principal, testes e utilitários em uma única solução. |
| `packages.lock.json` | Fixa o grafo NuGet do projeto principal para restauração em modo bloqueado. |
| `Program.cs` | Configura a inicialização da aplicação, injeção de dependências, middlewares, banco local, rotas MVC, Serilog e serviços da PoC. |
| `README.md` | Apresenta a PoC, tecnologias, configuração local, testes, observabilidade e links de referência. |
| `appsettings.json` | Configurações base da aplicação, API Control iD, banco, logs e segurança de callbacks. |
| `appsettings.Development.json` | Sobrescritas de configuração para execução local em ambiente de desenvolvimento. |

## GitHub

| Arquivo | Responsabilidade |
| --- | --- |
| `.github/dependabot.yml` | Agenda a revisão automatizada de dependências NuGet e GitHub Actions. |
| `.config/dotnet-tools.json` | Pina o `dotnet-ef` local na mesma versão do Entity Framework Core. |
| `.github/workflows/ci.yml` | Executa os gates de compilação, testes, documentação, auditoria e contêiner em pull requests e na ramificação principal. |

## Properties

| Arquivo | Responsabilidade |
| --- | --- |
| `Properties/launchSettings.json` | Define perfis locais de execução, URLs, portas e variáveis usadas pelo `dotnet run`/IDE. |

## Controllers

Os controladores coordenam as rotas MVC, recebem a entrada da interface, acionam serviços/repositórios e retornam telas ou respostas auxiliares.

| Arquivo | Responsabilidade |
| --- | --- |
| `Controllers/AccessLogsController.cs` | Fluxos de listagem, detalhe e remoção de logs de acesso persistidos localmente. |
| `Controllers/AccessRulesController.cs` | CRUD e visualização das regras de acesso locais usadas pela PoC. |
| `Controllers/AdvancedOfficialController.cs` | Telas e execuções de cenários oficiais avançados, como captura, exportação e comandos especiais. |
| `Controllers/AuthController.cs` | Fluxos de cadastro/login local, troca de senha e login/logout/status da sessão oficial do equipamento. |
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
| `Controllers/HomeController.cs` | Monta o painel inicial e os indicadores principais da PoC. |
| `Controllers/LogoController.cs` | Gerencia upload, consulta e remoção de logos/imagens locais. |
| `Controllers/MediaController.cs` | Gerencia mídias como fotos e vídeos usados nos fluxos da PoC. |
| `Controllers/MonitorWebhookController.cs` | Recebe, registra e apresenta eventos recebidos por webhook/callback. |
| `Controllers/OfficialApiController.cs` | Exibe catálogo oficial, contratos e invocação genérica dos endpoints documentados. |
| `Controllers/OfficialCallbacksController.cs` | Implementa endpoints de callbacks oficiais e processamento de payloads recebidos. |
| `Controllers/OfficialEventsController.cs` | Centraliza fluxos ligados a eventos oficiais da API Control iD. |
| `Controllers/OfficialObjectsController.cs` | Apresenta operações oficiais sobre objetos, payloads e contratos relacionados. |
| `Controllers/OperationModesController.cs` | Exibe e simula os modos de operação Standalone, Pro e Enterprise. |
| `Controllers/PrivacyController.cs` | Coordena consulta, correção, exportação e eliminação controlada dos dados locais de um titular. |
| `Controllers/ProductSpecificController.cs` | Coordena funcionalidades específicas por produto/modelo da família Control iD. |
| `Controllers/PushCenterController.cs` | Organiza a central de push, filas e comandos pendentes. |
| `Controllers/PushController.cs` | Fluxos de eventos push, consulta e enfileiramento de comandos. |
| `Controllers/QRCodesController.cs` | CRUD e visualização de QR Codes persistidos localmente. |
| `Controllers/RemoteActionsController.cs` | Execução e acompanhamento de ações remotas como autorização e enrolamento. |
| `Controllers/SessionController.cs` | Gerenciamento de sessões locais e status de autenticação/conexão com o equipamento. |
| `Controllers/SystemController.cs` | Operações de sistema, rede, VPN, hash de senha e ações administrativas. |
| `Controllers/UsersController.cs` | CRUD, visualização e payloads de usuários da PoC. |
| `Controllers/WorkspaceController.cs` | Exibe a área de trabalho/o explorador operacional para navegar pelos recursos implementados. |

## Data

| Arquivo | Responsabilidade |
| --- | --- |
| `Data/IntegracaoControlIDContext.cs` | DbContext do Entity Framework Core; mapeia entidades locais, índices, relacionamentos e configurações SQLite. |
| `Data/Migrations/20260430144509_InitialLocalSchema.cs` | Cria o esquema local inicial usado pela PoC. |
| `Data/Migrations/20260430144509_InitialLocalSchema.Designer.cs` | Registra os metadados EF da migração inicial. |
| `Data/Migrations/20260430224746_AddOperationalIndexes.cs` | Adiciona índices operacionais para consultas locais frequentes. |
| `Data/Migrations/20260430224746_AddOperationalIndexes.Designer.cs` | Registra os metadados EF da migração de índices. |
| `Data/Migrations/20260430233000_AddLocalUserRoles.cs` | Acrescenta os papéis de autorização dos usuários locais. |
| `Data/Migrations/20260430233000_AddLocalUserRoles.Designer.cs` | Registra os metadados EF da migração de papéis locais. |
| `Data/Migrations/20260803192319_HardenLocalIdentity.cs` | Fortalece unicidade e normalização da identidade local. |
| `Data/Migrations/20260803192319_HardenLocalIdentity.Designer.cs` | Registra os metadados EF da migração de fortalecimento da identidade. |
| `Data/Migrations/IntegracaoControlIDContextModelSnapshot.cs` | Mantém o retrato EF vigente do esquema local para gerar e revisar migrações. |

## Helpers

| Arquivo | Responsabilidade |
| --- | --- |
| `Helpers/ApiResponseHelper.cs` | Utilitários para normalizar respostas e mensagens vindas da API/serviços. |
| `Helpers/CryptoHelper.cs` | Apoio criptográfico, especialmente para hashes e transformações relacionadas a segurança. |
| `Helpers/FileHelper.cs` | Rotinas auxiliares para manipulação de arquivos usados pela PoC. |
| `Helpers/HttpHelper.cs` | Funções de apoio para chamadas HTTP, montagem de requests e leitura de respostas. |
| `Helpers/HighImpactOperationGuard.cs` | Exige confirmação explícita antes de operações administrativas ou destrutivas de alto impacto. |
| `Helpers/NavigationPresentationHelper.cs` | Centraliza detalhes de apresentação usados pela navegação da UI. |
| `Helpers/ProductSpecificPresentationHelper.cs` | Apoia a exibição de conteúdos específicos de produto na interface. |
| `Helpers/PrivacyLogHelper.cs` | Minimiza e mascara identificadores antes de incluí-los em registros técnicos. |
| `Helpers/SecurityTextHelper.cs` | Padroniza textos e mascaramentos ligados a informações sensíveis. |
| `Helpers/SessionHelper.cs` | Auxilia leitura, escrita e interpretação de dados de sessão no contexto web. |

## Registros

| Arquivo | Responsabilidade |
| --- | --- |
| `Logging/SeriLogConfiguration.cs` | Configura destinos, formato e políticas de registro com Serilog. |
| `Logging/SeriLogEvents.cs` | Centraliza identificadores/eventos de log usados para rastreabilidade. |

## Mappings

| Arquivo | Responsabilidade |
| --- | --- |
| `Mappings/ModelMappings.cs` | Converte modelos da API e do domínio para entidades locais ou estruturas equivalentes. |
| `Mappings/ViewModelMappings.cs` | Converte modelos e entidades em ViewModels prontos para as telas Razor. |

## Middlewares

| Arquivo | Responsabilidade |
| --- | --- |
| `Middlewares/ApiSessionMiddleware.cs` | Garante contexto mínimo de sessão/API durante o pipeline HTTP. |
| `Middlewares/CorrelationIdMiddleware.cs` | Normaliza, propaga e registra correlation ID seguro em requests/responses. |
| `Middlewares/ExceptionHandlingMiddleware.cs` | Captura exceções não tratadas e padroniza a resposta/registro de erro. |
| `Middlewares/DynamicResponseCachePolicyMiddleware.cs` | Impede cache de respostas dinâmicas e dados operacionais no navegador. |
| `Middlewares/RequestLoggingMiddleware.cs` | Registra informações de requests para observabilidade local. |
| `Middlewares/SecurityHeadersMiddleware.cs` | Aplica cabeçalhos de segurança HTTP nas respostas da aplicação. |

## Models/ControlIDApi

Modelos que representam contratos, cargas úteis e respostas próximas da API Control iD.

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
| `Models/ControlIDApi/OfficialApiJsonPayload.cs` | Preserva um `JsonElement` independente do ciclo de vida do `JsonDocument` usado no parse. |
| `Models/ControlIDApi/Photo.cs` | Representa foto ou imagem associada a usuário/mídia. |
| `Models/ControlIDApi/PushCommand.cs` | Representa comandos push enfileirados ou recebidos. |
| `Models/ControlIDApi/QRCode.cs` | Representa QR Codes de acesso. |
| `Models/ControlIDApi/RemoteAction.cs` | Representa uma ação remota solicitada ao equipamento. |
| `Models/ControlIDApi/RemoteActionResult.cs` | Representa o retorno de execução de uma ação remota. |
| `Models/ControlIDApi/SessionInfo.cs` | Representa dados de sessão/autenticação com o equipamento. |
| `Models/ControlIDApi/SystemInfo.cs` | Representa informações de sistema, rede e ambiente do equipamento. |
| `Models/ControlIDApi/User.cs` | Representa usuário no formato usado pelos fluxos de integração. |

## Models/Database

Entidades persistidas no SQLite local para histórico, cache operacional, simulações e suporte a UI.

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

## Models/Security

| Arquivo | Responsabilidade |
| --- | --- |
| `Models/Security/LocalIdentityPolicy.cs` | Centraliza normalização, limites e validações da identidade autenticável local. |

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
| `Options/ControlIdCircuitBreakerOptions.cs` | Configura o limiar e a janela do disjuntor das chamadas à API Control iD. |
| `Options/ControlIdEgressOptions.cs` | Configura limites, timeout e políticas seguras para tráfego de saída ao equipamento. |
| `Options/ControlIdConcurrencyOptions.cs` | Configura concorrência, fila e quantidade de equipamentos rastreados. |
| `Options/SqliteRuntimeOptions.cs` | Configura espera ocupada, WAL e sincronização do SQLite. |

## Services/Callbacks

| Arquivo | Responsabilidade |
| --- | --- |
| `Services/Callbacks/CallbackIngressService.cs` | Orquestra o recebimento, validação e persistência de callbacks. |
| `Services/Callbacks/CallbackRequestBodyReader.cs` | Lê o corpo bruto das requisições de callback de forma reutilizável. |
| `Services/Callbacks/CallbackSignatureCanonicalizer.cs` | Canonicaliza e assina method/path/query/timestamp/nonce e bytes exatos do body. |
| `Services/Callbacks/CallbackSecurityEvaluator.cs` | Avalia regras de segurança, chave compartilhada e origem permitida dos callbacks. |
| `Services/Callbacks/CallbackSignatureValidator.cs` | Valida assinatura HMAC, janela temporal e nonce dos callbacks recebidos. |

## Services/ControlIDApi

| Arquivo | Responsabilidade |
| --- | --- |
| `Services/ControlIDApi/IOfficialControlIdApiService.cs` | Contrato da camada de cliente HTTP para chamadas oficiais a API Control iD. |
| `Services/ControlIDApi/OfficialApiBinaryFileResultFactory.cs` | Monta respostas de arquivo/binário para resultados oficiais que precisam de download. |
| `Services/ControlIDApi/OfficialApiBodyParameterStrategy.cs` | Define estratégia de montagem de parâmetros enviados no corpo da requisição. |
| `Services/ControlIDApi/OfficialApiCatalogService.cs` | Disponibiliza o catálogo navegável de endpoints oficiais implementados/documentados. |
| `Services/ControlIDApi/OfficialApiCircuitBreaker.cs` | Interrompe temporariamente chamadas externas após falhas consecutivas para limitar cascatas. |
| `Services/ControlIDApi/OfficialApiContractDocumentationService.cs` | Gera a apresentação dos contratos oficiais de entrada/saída. |
| `Services/ControlIDApi/OfficialApiDocumentationSeedCatalog.cs` | Semeia metadados e documentação base dos endpoints oficiais. |
| `Services/ControlIDApi/OfficialApiDocumentationService.cs` | Consolida documentação, exemplos e metadados para exibição na UI. |
| `Services/ControlIDApi/OfficialApiInvokerService.cs` | Executa chamadas genéricas aos endpoints oficiais a partir do catálogo. |
| `Services/ControlIDApi/OfficialObjectPaging.cs` | Aplica limite, offset, lookahead e estado de navegação às listagens oficiais abertas por GET. |
| `Services/ControlIDApi/OfficialApiResponseBodyReader.cs` | Lê respostas externas com limite, cancelamento, charset e classificação binária. |
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
| `Services/Database/LocalUserRegistrationResult.cs` | Resultado tipado e estados do registro atômico de usuário local. |
| `Services/Database/LocalDataQueryLimits.cs` | Centraliza os limites padrão e máximos das consultas e listagens locais. |

## Services complementares

| Arquivo | Responsabilidade |
| --- | --- |
| `Services/DocumentedFeatures/DocumentedFeaturesPayloadFactory.cs` | Monta o payload/resumo de funcionalidades documentadas exibido na UI. |
| `Services/Files/UploadedFileBase64Encoder.cs` | Converte arquivos enviados pela UI para Base64 antes de envio/persistência. |
| `Services/Analytics/ProductAnalyticsEventClassifier.cs` | Classifica rotas allowlist em eventos agregados de produto sem identificadores pessoais. |
| `Services/Navigation/NavigationCatalogService.cs` | Monta o catálogo de navegação das páginas e módulos da PoC. |
| `Services/Navigation/PageShellService.cs` | Fornece metadados de shell, cabeçalho e breadcrumbs das páginas. |
| `Services/Observability/HealthCheckResponseWriter.cs` | Serializa health checks sem expor exceções, paths locais ou connection string. |
| `Services/Observability/ObservabilityConstants.cs` | Centraliza nomes de header, item de contexto e propriedades de escopo. |
| `Services/Observability/OperationalEventIds.cs` | Define IDs estáveis para eventos operacionais críticos. |
| `Services/Observability/OperationalMetrics.cs` | Publica métricas via `System.Diagnostics.Metrics` para coleta futura. |
| `Services/Observability/PrometheusMetricsWriter.cs` | Renderiza snapshot de métricas locais em formato Prometheus text para `/metrics`. |
| `Services/Observability/RuntimeCapacityMetricsProvider.cs` | Coleta gauges seguros de memória, storage local e disco para FinOps/capacidade. |
| `Services/Observability/RuntimeCapacityMetricsBackgroundService.cs` | Atualiza o snapshot de capacidade em segundo plano para evitar varredura de disco por requisição de métricas. |
| `Services/Observability/SqliteReadinessHealthCheck.cs` | Verifica readiness do SQLite local usado como estado runtime. |
| `Services/OperationModes/OperationModesPayloadFactory.cs` | Monta payloads demonstrativos dos modos Standalone, Pro e Enterprise. |
| `Services/OperationModes/OperationModesProfileResolver.cs` | Resolve perfis, comportamento esperado e transições dos modos de operação. |
| `Services/Performance/ServerTimingHeaderWriter.cs` | Publica medições agregadas e seguras no cabeçalho `Server-Timing`. |
| `Services/Performance/StaticAssetCachePolicy.cs` | Define a política de cache e invalidação dos ativos estáticos versionados. |
| `Services/Privacy/PrivacySubjectReportService.cs` | Monta relatório minimizado de dados locais para atender solicitações de titulares. |
| `Services/ProductSpecific/ProductSpecificCommandService.cs` | Executa comandos específicos por produto ou modelo. |
| `Services/ProductSpecific/ProductSpecificConfigurationPayloadFactory.cs` | Monta payloads de configuração específicos por linha de produto. |
| `Services/ProductSpecific/ProductSpecificDownloadResult.cs` | Representa resultado de download em fluxos específicos de produto. |
| `Services/ProductSpecific/ProductSpecificJsonReader.cs` | Lê e interpreta JSONs usados por funcionalidades específicas. |
| `Services/ProductSpecific/ProductSpecificSections.cs` | Define seções/categorias exibidas no módulo de recursos específicos. |
| `Services/ProductSpecific/ProductSpecificSnapshotService.cs` | Monta snapshots de estado/configuração específicos por produto. |
| `Services/Push/PushCommandStatuses.cs` | Centraliza os estados canônicos do ciclo de vida de comandos push. |
| `Services/Push/PushCommandWorkflowService.cs` | Aplica transições válidas e idempotentes ao fluxo persistido de comandos push. |
| `Services/Push/PushIdempotencyKeyResolver.cs` | Resolve e valida chaves de idempotência para evitar duplicidade de comandos. |
| `Services/Security/AppSecurityRoles.cs` | Define os papéis de autorização usados nas políticas e telas protegidas. |
| `Services/Security/ControlIdInputSanitizer.cs` | Sanitiza entradas para reduzir risco de payloads inválidos ou inseguros. |

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
| `ViewModels/AdvancedOfficial/CameraCaptureViewModel.cs` | Dados do fluxo oficial de captura de câmera. |
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
| `ViewModels/Home/HomeDashboardViewModel.cs` | Dados do painel inicial. |
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
| `ViewModels/OfficialObjects/OfficialObjectsViewModel.cs` | Dados da tela de exploração de objetos oficiais. |
| `ViewModels/OperationModes/OperationModesViewModel.cs` | Dados da tela de modos Standalone, Pro e Enterprise. |
| `ViewModels/Privacy/PrivacySubjectRequestViewModel.cs` | Campos e validações das solicitações de direitos do titular sobre dados locais. |
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
| `ViewModels/Session/SessionDeactivateViewModel.cs` | Dados de desativação/encerramento de sessão. |
| `ViewModels/Session/SessionEditViewModel.cs` | Campos de edição de sessão. |
| `ViewModels/Session/SessionListViewModel.cs` | Dados da listagem de sessões. |
| `ViewModels/Session/SessionStatusViewModel.cs` | Dados de status da sessão. |
| `ViewModels/Session/SessionViewModel.cs` | Dados de detalhe de sessão. |
| `ViewModels/Shared/AppPageHeaderViewModel.cs` | Dados do cabeçalho padrão das páginas. |
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
| `ViewModels/Users/UserDto.cs` | DTO auxiliar para transferência de dados de usuário. |
| `ViewModels/Users/UserEditViewModel.cs` | Campos usados na criação/edição de usuário. |
| `ViewModels/Users/UserListViewModel.cs` | Dados da listagem de usuários. |
| `ViewModels/Users/UsersApiResponse.cs` | Estrutura de resposta agregada da API para usuários. |
| `ViewModels/Users/UserViewModel.cs` | Dados de detalhe de usuário. |
| `ViewModels/Workspace/WorkspaceExplorerViewModel.cs` | Dados do explorador/área de trabalho operacional. |

## Views

As views Razor compõem a interface web da PoC. Em geral, cada pasta espelha um controller e cada arquivo `.cshtml` representa uma tela ou parcial reutilizável.

| Arquivo | Responsabilidade |
| --- | --- |
| `Views/_ViewImports.cshtml` | Importa namespaces e tag helpers disponíveis para todas as views. |
| `Views/_ViewStart.cshtml` | Define o layout padrão usado pelas views. |
| `Views/AccessLogs/Delete.cshtml` | Tela de confirmação de exclusão de log de acesso. |
| `Views/AccessLogs/Details.cshtml` | Tela de detalhe de log de acesso. |
| `Views/AccessLogs/Index.cshtml` | Tela de listagem/filtro de logs de acesso. |
| `Views/AccessRules/Create.cshtml` | Tela de criação de regra de acesso. |
| `Views/AccessRules/Delete.cshtml` | Tela de confirmação de exclusão de regra de acesso. |
| `Views/AccessRules/Details.cshtml` | Tela de detalhe de regra de acesso. |
| `Views/AccessRules/Edit.cshtml` | Tela de edição de regra de acesso. |
| `Views/AccessRules/Index.cshtml` | Tela de listagem de regras de acesso. |
| `Views/AdvancedOfficial/CameraCapture.cshtml` | Tela do fluxo oficial de captura de câmera. |
| `Views/AdvancedOfficial/ExportObjects.cshtml` | Tela do fluxo oficial de exportação de objetos. |
| `Views/AdvancedOfficial/FacialEnroll.cshtml` | Tela do fluxo oficial de enroll facial. |
| `Views/AdvancedOfficial/Index.cshtml` | Tela inicial dos recursos oficiais avançados. |
| `Views/AdvancedOfficial/NetworkInterlock.cshtml` | Tela do fluxo de intertravamento/rede. |
| `Views/AdvancedOfficial/RemoteLedControl.cshtml` | Tela do fluxo de controle remoto de LED. |
| `Views/Auth/ChangePassword.cshtml` | Tela de troca de senha. |
| `Views/Auth/AccessDenied.cshtml` | Tela acessível exibida quando o usuário autenticado não possui o papel exigido. |
| `Views/Auth/LocalLogin.cshtml` | Tela de autenticação da conta local usada para proteger a PoC. |
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
| `Views/Media/Delete.cshtml` | Tela de confirmação de exclusão de mídia. |
| `Views/Media/Details.cshtml` | Tela de detalhe de mídia/foto. |
| `Views/Media/Index.cshtml` | Tela de listagem de mídias/fotos. |
| `Views/Media/Upload.cshtml` | Tela de upload de mídia/foto. |
| `Views/Monitor/Push.cshtml` | Tela de monitoramento de eventos push. |
| `Views/Monitor/PushDetails.cshtml` | Tela de detalhe de evento push monitorado. |
| `Views/Monitor/Webhook.cshtml` | Tela de monitoramento de webhooks/callbacks. |
| `Views/Monitor/WebhookDetails.cshtml` | Tela de detalhe de webhook/callback recebido. |
| `Views/OfficialApi/Index.cshtml` | Tela do catálogo oficial de endpoints. |
| `Views/OfficialApi/Invoke.cshtml` | Tela de invocação dinâmica de endpoint oficial. |
| `Views/OfficialEvents/Details.cshtml` | Tela de detalhe de evento oficial. |
| `Views/OfficialEvents/Index.cshtml` | Tela de listagem de eventos oficiais. |
| `Views/OfficialObjects/Index.cshtml` | Tela de exploração de objetos oficiais. |
| `Views/OperationModes/Index.cshtml` | Tela dos modos Standalone, Pro e Enterprise. |
| `Views/ProductSpecific/Index.cshtml` | Tela de recursos específicos por produto. |
| `Views/Privacy/Index.cshtml` | Tela administrativa para solicitações de acesso, correção, exportação e eliminação de dados locais. |
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
| `Views/Shared/_AppPageHeader.cshtml` | Parcial de cabeçalho padrão das páginas. |
| `Views/Shared/_ConnectionPanel.cshtml` | Parcial do painel de conexão/status do equipamento. |
| `Views/Shared/_EndpointContractPanel.cshtml` | Parcial de exibição de contrato de endpoint. |
| `Views/Shared/_Layout.cshtml` | Layout principal da aplicação. |
| `Views/Shared/_Layout.cshtml.css` | Estilos escopados do layout principal. |
| `Views/Shared/_NavBar.cshtml` | Parcial da barra de navegação principal. |
| `Views/Shared/_NavBar.cshtml.css` | Estilos escopados da barra de navegação. |
| `Views/Shared/_NotFound.cshtml` | Parcial de recurso não encontrado. |
| `Views/Shared/_OfficialObjectPagination.cshtml` | Exibe navegação anterior/próxima preservando os parâmetros da consulta oficial. |
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
| `Views/Workspace/Domain.cshtml` | Tela de domínio/área específica da área de trabalho. |
| `Views/Workspace/Index.cshtml` | Tela principal da área de trabalho/do explorador operacional. |

## wwwroot

| Arquivo/Pasta | Responsabilidade |
| --- | --- |
| `wwwroot/css/site.css` | Estilos globais da PoC, componentes visuais, painel, tabelas e formulários. |
| `wwwroot/css/site-content.css` | Componentes e superfícies das páginas de conteúdo. |
| `wwwroot/css/site-shell.css` | Estrutura visual do shell e navegação desktop. |
| `wwwroot/css/site-shell-responsive.css` | Sobrescritas isoladas da navegação e do shell responsivo. |
| `wwwroot/js/site.js` | JavaScript global da interface, comportamentos de interação e utilidades executadas no cliente. |
| `wwwroot/js/site-forms.js` | Estados pendentes, progresso e cancelamento de uploads. |
| `wwwroot/favicon.ico` | Ícone exibido pelo navegador para a aplicação. |
| `wwwroot/img/docs/local-login.png` | Captura sanitizada da tela inicial de autenticação local. |
| `wwwroot/img/docs/authenticated-home.png` | Captura sanitizada do painel autenticado sem equipamento conectado. |
| `wwwroot/img/docs/official-api.png` | Captura sanitizada do catálogo oficial da API. |
| `wwwroot/lib/bootstrap/*` | Arquivos CSS/JS do Bootstrap, incluindo versões minificadas, mapas de código-fonte, utilitários e licenças. |
| `wwwroot/lib/jquery/*` | Arquivos da biblioteca jQuery usados pela camada executada no cliente. |
| `wwwroot/lib/jquery-validation/*` | Biblioteca de validação jQuery usada nos formulários. |
| `wwwroot/lib/jquery-validation-unobtrusive/*` | Adaptadores unobtrusive validation usados com ASP.NET Core MVC/Razor. |

## docs

| Arquivo/Pasta | Responsabilidade |
| --- | --- |
| `docs/README.md` | Índice central da documentação técnica por papel, tema e percurso. |
| `docs/adrs/0001-local-sqlite-runtime-state.md` | Registra a decisão de usar SQLite como estado local da PoC. |
| `docs/adrs/0002-secure-controlid-ingress-and-egress.md` | Registra os limites seguros das comunicações com o equipamento. |
| `docs/adrs/0003-in-process-observability-and-readiness-gates.md` | Registra a estratégia de observabilidade e prontidão no processo. |
| `docs/adrs/0004-release-governance-with-local-scripts.md` | Registra a governança de liberação por scripts locais e CI. |
| `docs/adrs/0005-dotnet-10-lts-runtime.md` | Registra a migração coordenada de SDK, runtime, pacotes e contêiner para .NET 10 LTS. |
| `docs/api-error-catalog.md` | Cataloga erros por camada, status, conduta e evidência segura. |
| `docs/architecture-overview.md` | Descreve camadas, fluxos críticos, fronteiras de confiança e contratos. |
| `docs/brand.md` | Define identidade, tokens, componentes e acessibilidade visual. |
| `docs/changelog-2026-04-14.md` | Registra evoluções técnicas da rodada de 14/04/2026. |
| `docs/changelog-2026-04-15.md` | Registra documentação, comentários e observabilidade da rodada de 15/04/2026. |
| `docs/changelog-2026-05-01.md` | Registra documentação, integração técnica e ADRs da rodada de 01/05/2026. |
| `docs/changelog-2026-08-03.md` | Registra o fechamento dos 14 riscos da solução completa. |
| `docs/changelog-2026-08-04.md` | Registra as otimizações dos 11 gargalos e a validação funcional/visual subsequente. |
| `docs/ci-cd-quality-gates.md` | Documenta GitHub Actions, gates, artefatos e reprodução local. |
| `docs/database-and-runtime-state.md` | Explica o estado SQLite de runtime e comandos de inspeção seguros. |
| `docs/data-model-and-recovery.md` | Mapeia entidades, índices, migrações, backup e restauração. |
| `docs/data-synchronization-ownership.md` | Define fontes de verdade, sincronização, conflitos e reconciliação. |
| `docs/deployment-runbook.md` | Mapeia ambientes, contêiner, implantação, reversão e riscos. |
| `docs/developer-onboarding.md` | Guia de configuração, execução, diagnóstico e entrega segura. |
| `docs/device-compatibility-matrix.md` | Separa cobertura da PoC de homologação por produto, firmware e licença. |
| `docs/documentation-audit-2026-05-01.md` | Preserva a auditoria documental histórica e suas lacunas. |
| `docs/equipment-contingency-runbook.md` | Define contingência do equipamento, alternativa manual e teste de bancada. |
| `docs/external-validation-runbook.md` | Padroniza SAST, OSV, DAST, acessibilidade e contrato físico/simulado. |
| `docs/faq.md` | Responde às 96 perguntas frequentes de primeiro contato e integração. |
| `docs/finops-capacity.md` | Define custos, capacidade, limites e governança FinOps. |
| `docs/incident-response-and-dr.md` | Define severidade, incidentes, continuidade, DR e pós-incidente. |
| `docs/integration-contracts.md` | Inventaria APIs, callbacks, sessões, persistências e contratos. |
| `docs/local-account-administration.md` | Explica contas locais, papéis, sessões e recuperação de acesso. |
| `docs/monitor-implementation.md` | Documenta Monitor, callbacks, segurança e persistência de eventos. |
| `docs/network-topologies.md` | Mapeia direções de rede, portas, DNS/NAT, TLS e proxies. |
| `docs/observability-runbook.md` | Define saúde, métricas, alertas, painéis e resposta operacional. |
| `docs/observability/alert-rules.json` | Regras versionadas de alerta independentes de fornecedor. |
| `docs/observability/dashboard.json` | Especificação versionada dos painéis operacionais. |
| `docs/official-api-version-governance.md` | Define fontes, cadência e revalidação da Access API/firmware. |
| `docs/operation-modes-implementation.md` | Documenta Standalone, Pro, Enterprise, cargas e transições. |
| `docs/persona-guides.md` | Oferece percursos para avaliação, integração, segurança, QA e operação. |
| `docs/privacy-and-data-retention.md` | Inventaria dados pessoais, tratamentos, retenção e lacunas LGPD. |
| `docs/privacy-governance-runbook.md` | Define RACI, DSAR, RIPD, DPA e incidente de privacidade. |
| `docs/product-acceptance-criteria.md` | Mapeia requisitos, fluxos, aceite, rastreabilidade, DoR e DoD. |
| `docs/product-analytics.md` | Define objetivos, KPIs, eventos e restrições de analytics. |
| `docs/project-file-responsibilities.md` | Mantém este inventário de responsabilidades por arquivo e pasta. |
| `docs/pr-summary-2026-05-01.md` | Preserva o resumo de PR/notas de liberação da rodada documental. |
| `docs/push-implementation.md` | Documenta fila Push, polling, resultados, estados e segurança. |
| `docs/reports/controlid-api-audit-2026-04-13.md` | Preserva a auditoria histórica da cobertura da Access API. |
| `docs/reports/design-system-accessibility-audit-2026-04-14.md` | Preserva a auditoria histórica de design e acessibilidade. |
| `docs/reports/heuristic-ui-audit-2026-04-14.md` | Preserva a avaliação heurística histórica da interface. |
| `docs/reports/localhost-smoke-test-2026-04-13.md` | Preserva o teste integrado local de 13/04/2026. |
| `docs/reports/localhost-smoke-test-2026-04-14.md` | Preserva o teste integrado local de 14/04/2026. |
| `docs/reports/operation-modes-e2e-runbook-2026-04-14.md` | Preserva o roteiro E2E histórico dos modos de operação. |
| `docs/reports/operation-modes-homologation-matrix-2026-04-14.md` | Preserva a matriz histórica de homologação dos modos. |
| `docs/reports/visual-inventory-2026-04-14.md` | Preserva o inventário visual histórico das telas. |
| `docs/residual-risk-closure.md` | Mapeia riscos externos para gates, responsáveis e evidências. |
| `docs/security-hardening.md` | Documenta autenticação, RBAC, HMAC, headers, allowlists e segredos. |
| `docs/supply-chain-review.md` | Revisa NuGet, lockfiles, SBOM, vendors e licenças. |
| `docs/testing-strategy.md` | Define estratégia, cobertura, contratos, smoke e gates. |
| `docs/troubleshooting-controlid.md` | Organiza diagnóstico por sintoma e escalonamento seguro. |

## tests

Este inventário é conferido por `tools/validate-documentation.ps1`. Qualquer
arquivo de teste novo deve ser incluído aqui com sua responsabilidade; referências
a arquivos removidos fazem o gate documental falhar.

| Arquivo | Responsabilidade |
| --- | --- |
| `tests/Integracao.ControlID.PoC.Tests/Integracao.ControlID.PoC.Tests.csproj` | Projeto xUnit, dependências e configuração da suíte da PoC. |
| `tests/Integracao.ControlID.PoC.Tests/packages.lock.json` | Fixa o grafo NuGet da suíte para restauração reprodutível. |
| `tests/Integracao.ControlID.PoC.Tests/Controllers/AuthControllerTests.cs` | Valida comportamento HTTP, autorização e respostas cobertos por `AuthControllerTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Controllers/CallbackRateLimitingContractTests.cs` | Valida comportamento HTTP, autorização e respostas cobertos por `CallbackRateLimitingContractTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Controllers/HomeControllerPerformanceTests.cs` | Valida comportamento HTTP, autorização e respostas cobertos por `HomeControllerPerformanceTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Controllers/OfficialEventsControllerTests.cs` | Valida comportamento HTTP, autorização e respostas cobertos por `OfficialEventsControllerTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Controllers/OfficialObjectsControllerTests.cs` | Valida comportamento HTTP, autorização e respostas cobertos por `OfficialObjectsControllerTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Controllers/PushCenterControllerTests.cs` | Valida comportamento HTTP, autorização e respostas cobertos por `PushCenterControllerTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Controllers/PushControllerTests.cs` | Valida comportamento HTTP, autorização e respostas cobertos por `PushControllerTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Controllers/SessionControllerTests.cs` | Valida comportamento HTTP, autorização e respostas cobertos por `SessionControllerTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Controllers/SystemControllerTests.cs` | Valida comportamento HTTP, autorização e respostas cobertos por `SystemControllerTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Data/OperationalIndexMigrationTests.cs` | Valida migrações, índices e compatibilidade de dados cobertos por `OperationalIndexMigrationTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Frontend/AccessibilityAndResponsiveContractTests.cs` | Valida contrato renderizado, responsividade e acessibilidade cobertos por `AccessibilityAndResponsiveContractTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Frontend/RenderedApplicationContractTests.cs` | Valida contrato renderizado, responsividade e acessibilidade cobertos por `RenderedApplicationContractTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Helpers/CryptoHelperTests.cs` | Valida regras auxiliares e casos de borda cobertos por `CryptoHelperTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Helpers/HighImpactOperationGuardTests.cs` | Valida regras auxiliares e casos de borda cobertos por `HighImpactOperationGuardTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Helpers/PrivacyLogHelperTests.cs` | Valida regras auxiliares e casos de borda cobertos por `PrivacyLogHelperTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Helpers/ProductSpecificPresentationHelperTests.cs` | Valida regras auxiliares e casos de borda cobertos por `ProductSpecificPresentationHelperTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Helpers/SecurityTextHelperTests.cs` | Valida regras auxiliares e casos de borda cobertos por `SecurityTextHelperTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Middlewares/CorrelationIdMiddlewareTests.cs` | Valida o pipeline HTTP e os controles transversais cobertos por `CorrelationIdMiddlewareTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Middlewares/DynamicResponseCachePolicyMiddlewareTests.cs` | Valida o pipeline HTTP e os controles transversais cobertos por `DynamicResponseCachePolicyMiddlewareTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Middlewares/ExceptionHandlingMiddlewareTests.cs` | Valida o pipeline HTTP e os controles transversais cobertos por `ExceptionHandlingMiddlewareTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Middlewares/SecurityHeadersMiddlewareTests.cs` | Valida o pipeline HTTP e os controles transversais cobertos por `SecurityHeadersMiddlewareTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Platform/CiQualityGateContractTests.cs` | Valida governança, infraestrutura e contratos documentais cobertos por `CiQualityGateContractTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Platform/DeploymentEnvironmentContractTests.cs` | Valida governança, infraestrutura e contratos documentais cobertos por `DeploymentEnvironmentContractTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Platform/DocumentationGovernanceContractTests.cs` | Valida governança, infraestrutura e contratos documentais cobertos por `DocumentationGovernanceContractTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Platform/FinOpsCapacityContractTests.cs` | Valida governança, infraestrutura e contratos documentais cobertos por `FinOpsCapacityContractTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Platform/IncidentResponseRunbookContractTests.cs` | Valida governança, infraestrutura e contratos documentais cobertos por `IncidentResponseRunbookContractTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Analytics/ProductAnalyticsEventClassifierTests.cs` | Valida serviços da área `Analytics` e os casos de borda cobertos por `ProductAnalyticsEventClassifierTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Callbacks/CallbackIngressServiceTests.cs` | Valida serviços da área `Callbacks` e os casos de borda cobertos por `CallbackIngressServiceTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Callbacks/CallbackRequestBodyReaderTests.cs` | Valida serviços da área `Callbacks` e os casos de borda cobertos por `CallbackRequestBodyReaderTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Callbacks/CallbackSecurityEvaluatorTests.cs` | Valida serviços da área `Callbacks` e os casos de borda cobertos por `CallbackSecurityEvaluatorTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Callbacks/CallbackSignatureValidatorTests.cs` | Valida serviços da área `Callbacks` e os casos de borda cobertos por `CallbackSignatureValidatorTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialApiBinaryFileResultFactoryTests.cs` | Valida serviços da área `ControlIDApi` e os casos de borda cobertos por `OfficialApiBinaryFileResultFactoryTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialApiCatalogServiceTests.cs` | Valida serviços da área `ControlIDApi` e os casos de borda cobertos por `OfficialApiCatalogServiceTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialApiCircuitBreakerTests.cs` | Valida serviços da área `ControlIDApi` e os casos de borda cobertos por `OfficialApiCircuitBreakerTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialApiContractDocumentationServiceTests.cs` | Valida serviços da área `ControlIDApi` e os casos de borda cobertos por `OfficialApiContractDocumentationServiceTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialApiInvokerServiceTests.cs` | Valida serviços da área `ControlIDApi` e os casos de borda cobertos por `OfficialApiInvokerServiceTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialApiConcurrencyLimiterTests.cs` | Valida isolamento, fila e paralelismo por equipamento. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/OfficialObjectPagingTests.cs` | Valida limite, offset, lookahead e metadados da paginação oficial. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Database/DeviceRepositoryTests.cs` | Valida serviços da área `Database` e os casos de borda cobertos por `DeviceRepositoryTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Database/MonitorEventRepositoryTests.cs` | Valida serviços da área `Database` e os casos de borda cobertos por `MonitorEventRepositoryTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Database/PushCommandRepositoryTests.cs` | Valida serviços da área `Database` e os casos de borda cobertos por `PushCommandRepositoryTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Database/RepositoryFailureContractTests.cs` | Valida serviços da área `Database` e os casos de borda cobertos por `RepositoryFailureContractTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Database/SqliteRuntimePolicyTests.cs` | Valida WAL e escritores SQLite concorrentes. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Database/UserRepositoryRegistrationTests.cs` | Valida serviços da área `Database` e os casos de borda cobertos por `UserRepositoryRegistrationTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Files/UploadedFileBase64EncoderTests.cs` | Valida serviços da área `Files` e os casos de borda cobertos por `UploadedFileBase64EncoderTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Navigation/NavigationCatalogServiceTests.cs` | Valida serviços da área `Navigation` e os casos de borda cobertos por `NavigationCatalogServiceTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Observability/ObservabilityEndpointContractTests.cs` | Valida serviços da área `Observability` e os casos de borda cobertos por `ObservabilityEndpointContractTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Observability/OperationalMetricsTests.cs` | Valida serviços da área `Observability` e os casos de borda cobertos por `OperationalMetricsTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Observability/SqliteReadinessHealthCheckTests.cs` | Valida serviços da área `Observability` e os casos de borda cobertos por `SqliteReadinessHealthCheckTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/OperationModes/OperationModesPayloadFactoryTests.cs` | Valida serviços da área `OperationModes` e os casos de borda cobertos por `OperationModesPayloadFactoryTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/OperationModes/OperationModesProfileResolverTests.cs` | Valida serviços da área `OperationModes` e os casos de borda cobertos por `OperationModesProfileResolverTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Performance/ServerTimingHeaderWriterTests.cs` | Valida serviços da área `Performance` e os casos de borda cobertos por `ServerTimingHeaderWriterTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Performance/StaticAssetCachePolicyTests.cs` | Valida serviços da área `Performance` e os casos de borda cobertos por `StaticAssetCachePolicyTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Privacy/PrivacySubjectReportServiceTests.cs` | Valida serviços da área `Privacy` e os casos de borda cobertos por `PrivacySubjectReportServiceTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ProductSpecific/ProductSpecificConfigurationPayloadFactoryTests.cs` | Valida serviços da área `ProductSpecific` e os casos de borda cobertos por `ProductSpecificConfigurationPayloadFactoryTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ProductSpecific/ProductSpecificJsonReaderTests.cs` | Valida serviços da área `ProductSpecific` e os casos de borda cobertos por `ProductSpecificJsonReaderTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/ProductSpecific/ProductSpecificSnapshotServiceTests.cs` | Valida a consolidação e o paralelismo seguro das leituras de configuração e estado por produto. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Push/PushCommandWorkflowServiceTests.cs` | Valida serviços da área `Push` e os casos de borda cobertos por `PushCommandWorkflowServiceTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Push/PushIdempotencyKeyResolverTests.cs` | Valida serviços da área `Push` e os casos de borda cobertos por `PushIdempotencyKeyResolverTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Services/Security/ControlIdInputSanitizerTests.cs` | Valida serviços da área `Security` e os casos de borda cobertos por `ControlIdInputSanitizerTests`. |
| `tests/Integracao.ControlID.PoC.Tests/TestSupport/DictionaryTempDataProvider.cs` | Fornece infraestrutura determinística de teste por meio de `DictionaryTempDataProvider`. |
| `tests/Integracao.ControlID.PoC.Tests/TestSupport/FileSqliteTestDatabase.cs` | Fornece infraestrutura determinística de teste por meio de `FileSqliteTestDatabase`. |
| `tests/Integracao.ControlID.PoC.Tests/TestSupport/OfficialApiTestFactory.cs` | Fornece infraestrutura determinística de teste por meio de `OfficialApiTestFactory`. |
| `tests/Integracao.ControlID.PoC.Tests/TestSupport/RecordingHttpMessageHandler.cs` | Fornece infraestrutura determinística de teste por meio de `RecordingHttpMessageHandler`. |
| `tests/Integracao.ControlID.PoC.Tests/TestSupport/SqliteTestDatabase.cs` | Fornece infraestrutura determinística de teste por meio de `SqliteTestDatabase`. |
| `tests/Integracao.ControlID.PoC.Tests/TestSupport/StaticHttpClientFactory.cs` | Fornece infraestrutura determinística de teste por meio de `StaticHttpClientFactory`. |
| `tests/Integracao.ControlID.PoC.Tests/TestSupport/StaticUrlHelper.cs` | Fornece infraestrutura determinística de teste por meio de `StaticUrlHelper`. |
| `tests/Integracao.ControlID.PoC.Tests/TestSupport/TestSession.cs` | Fornece infraestrutura determinística de teste por meio de `TestSession`. |
| `tests/Integracao.ControlID.PoC.Tests/TestSupport/TestSessionFeature.cs` | Fornece infraestrutura determinística de teste por meio de `TestSessionFeature`. |
| `tests/Integracao.ControlID.PoC.Tests/Tools/ReadinessGateContractTests.cs` | Valida os contratos dos scripts e gates cobertos por `ReadinessGateContractTests`. |
| `tests/Integracao.ControlID.PoC.Tests/Tools/ControlIdDeviceStubScenarioTests.cs` | Valida catálogo, reset, concorrência e sessão expirada do simulador. |

## tools

| Arquivo/Pasta | Responsabilidade |
| --- | --- |
| `tools/audit-supply-chain.ps1` | Orquestra auditoria NuGet, componentes vendorizados, integridade e inventário da cadeia de suprimentos. |
| `tools/audit-vendor-dependencies.ps1` | Confere versões, hashes, licenças e arquivos declarados das bibliotecas de frontend vendorizadas. |
| `tools/backup-sqlite.ps1` | Produz cópia local não destrutiva e consistente do banco SQLite. |
| `tools/observability-check.ps1` | Valida artefatos de observabilidade, verificações de saúde, métricas e contrato físico opcional. |
| `tools/operational-readiness-check.ps1` | Valida guias operacionais, `ops.example.json` e, em liberação, `ops.local.json` sem valores de exemplo. |
| `tools/backup-sqlite-operational.ps1` | Orquestra cópia SQLite protegida, espelhamento opcional, teste de restauração e retenção confirmada. |
| `tools/contract-controlid-device.ps1` | Executa contrato opt-in contra equipamento físico usando credenciais fornecidas apenas pelo ambiente. |
| `tools/contract-controlid-stub.ps1` | Inicia o simulador local e valida o contrato Control iD sem equipamento físico ou credenciais reais. |
| `tools/external-security-scans.ps1` | Orquestra inventário e execução de Semgrep, OSV Scanner, ZAP baseline e axe quando disponíveis. |
| `tools/finops-capacity-check.ps1` | Valida guia operacional, alertas, governança e tamanhos locais de SQLite, registros, artefatos e relatórios sem apagar dados. |
| `tools/generate-sbom.ps1` | Gera SBOM CycloneDX a partir dos grafos NuGet restaurados. |
| `tools/harden-local-state.ps1` | Verifica e restringe, quando solicitado, permissões do estado local sensível. |
| `tools/restore-smoke-sqlite.ps1` | Restaura uma cópia em destino temporário e valida sua integridade sem substituir o banco ativo. |
| `tools/scan-secrets.ps1` | Procura padrões de credenciais e dados sensíveis em arquivos versionados e no diff. |
| `tools/test-readiness-gates.ps1` | Orquestra compilação, testes, formatação, análise de segredos, observabilidade off-line, FinOps/capacidade, cobertura, teste integrado, auditoria, contrato físico, analisadores externos e modo estrito `-ReleaseGate`. |
| `tools/validate-documentation.ps1` | Valida inventário, UTF-8, metadados, links, âncoras, rastreabilidade e integridade da licença vendorizada. |
| `tools/smoke-localhost.ps1` | Script PowerShell que executa teste integrado local, inicia o simulador e percorre fluxos críticos da PoC. |
| `tools/ControlIdDeviceStub/ControlIdDeviceStub.csproj` | Projeto .NET do simulador local que reproduz respostas de um equipamento Control iD. |
| `tools/ControlIdDeviceStub/Program.cs` | Implementa os endpoints simulados usados pelos smoke tests locais. |
| `tools/performance-baseline.ps1` | Mede percentis, vazão, CPU e memória contra massas sintéticas. |
| `tools/maintainability-check.ps1` | Bloqueia crescimento além dos orçamentos de arquivo versionados. |
| `tools/ControlIdDeviceStub/packages.lock.json` | Fixa o grafo NuGet do simulador local. |
| `tools/ControlIdCallbackSigningProxy/ControlIdCallbackSigningProxy.csproj` | Define o proxy mínimo de assinatura HMAC para callbacks de equipamentos sem suporte nativo. |
| `tools/ControlIdCallbackSigningProxy/Program.cs` | Valida origem e tamanho, remove cabeçalhos sensíveis, assina e encaminha callbacks ao destino permitido. |
| `tools/ControlIdCallbackSigningProxy/appsettings.json` | Fornece configuração segura sem segredo para o proxy assinador. |
| `tools/ControlIdCallbackSigningProxy/packages.lock.json` | Fixa o grafo NuGet do proxy assinador. |

## Extensões de validação sem equipamento

Os arquivos desta seção sustentam a validação determinística, a concorrência
controlada e a inspeção visual introduzidas em 2026-08-04. Eles permanecem
listados individualmente para que mudanças futuras de responsabilidade sejam
detectadas pelo inventário.

| Arquivo | Responsabilidade |
| --- | --- |
| `Controllers/DevelopmentController.cs` | Disponibiliza, somente em desenvolvimento e em loopback, a administração autenticada do simulador local. |
| `Models/ControlIDApi/OfficialApiStreamMetadata.cs` | Representa metadados seguros de respostas binárias transmitidas diretamente ao cliente. |
| `Options/ControlIdConcurrencyOptions.cs` | Define limites configuráveis de paralelismo e fila por equipamento. |
| `Options/SqliteRuntimeOptions.cs` | Define espera ocupada, modo de diário e sincronização aplicados ao SQLite local. |
| `Services/ControlIDApi/IControlIdSystemClient.cs` | Expõe o cliente tipado para informações e configuração de rede do equipamento. |
| `Services/ControlIDApi/OfficialApiConcurrencyLimiter.cs` | Isola e limita requisições concorrentes por destino Control iD, com fila limitada e rejeição segura. |
| `Services/ControlIDApi/OfficialApiDownloadResponse.cs` | Aplica cabeçalhos permitidos e transmite downloads oficiais sem materialização integral em memória. |
| `Services/Database/SqliteConnectionPragmaInterceptor.cs` | Aplica pragmas de integridade, sincronização e espera em cada conexão SQLite. |
| `Services/Database/SqliteRuntimePolicy.cs` | Valida e aplica a política de diário WAL na inicialização do banco local. |
| `ViewModels/Development/SimulatorViewModel.cs` | Modela cenário, perfil, massa e estado apresentados na central do simulador. |
| `Views/Development/Simulator.cshtml` | Permite consultar e alterar, de modo seguro, o cenário determinístico usado no desenvolvimento. |
| `tests/Integracao.ControlID.PoC.E2E/Integracao.ControlID.PoC.E2E.csproj` | Define a suíte de navegador Playwright, axe e xUnit com dependências bloqueadas. |
| `tests/Integracao.ControlID.PoC.E2E/CriticalJourneysTests.cs` | Percorre jornadas autenticadas, teclado, acessibilidade, responsividade e regressão visual. |
| `tests/Integracao.ControlID.PoC.E2E/E2EEnvironment.cs` | Inicia aplicação, banco isolado e simulador em portas livres para cada execução de navegador. |
| `tests/Integracao.ControlID.PoC.E2E/VisualRegression.cs` | Compara capturas de tela por pixels com tolerância versionada. |
| `tests/Integracao.ControlID.PoC.E2E/packages.lock.json` | Fixa o grafo NuGet da suíte de navegador para restauração reprodutível. |
| `tests/Integracao.ControlID.PoC.E2E/xunit.runner.json` | Desabilita paralelismo incompatível com o ambiente isolado compartilhado da suíte E2E. |
| `tools/ControlIdDeviceStub/Properties/AssemblyInfo.cs` | Libera componentes internos do simulador apenas para o projeto de testes. |
| `tools/ControlIdDeviceStub/StubDatasetFactory.cs` | Gera massas sintéticas determinísticas, sem dados pessoais reais. |
| `tools/ControlIdDeviceStub/StubDeviceProfile.cs` | Modela perfis representativos de produto e capacidade do equipamento simulado. |
| `tools/ControlIdDeviceStub/StubEndpointRouter.cs` | Resolve endpoints oficiais simulados e separa o roteamento da composição da aplicação. |
| `tools/ControlIdDeviceStub/StubManagementEndpoints.cs` | Expõe catálogo, estado, seleção e restauração do simulador somente em loopback. |
| `tools/ControlIdDeviceStub/StubRequestBodyReader.cs` | Lê corpos de requisição com limite explícito para preservar memória. |
| `tools/ControlIdDeviceStub/StubRuntimeState.cs` | Mantém cenário, perfil, massa e atraso atuais de forma concorrente e determinística. |
| `tools/ControlIdDeviceStub/StubScenario.cs` | Implementa falhas, atrasos e respostas anômalas selecionáveis. |
| `tools/ControlIdDeviceStub/StubState.cs` | Mantém objetos, usuários e demais fixtures determinísticas do equipamento simulado. |
| `tools/ControlIdDeviceStub/contracts/stub-reset.schema.json` | Documenta o contrato JSON aceito para restauração de perfil e massa do stub. |
| `tools/ControlIdDeviceStub/contracts/stub-scenario.schema.json` | Documenta o contrato JSON aceito para seleção de cenários e atrasos. |
| `tools/ControlIdDeviceStub/fixtures/dataset-100.json` | Registra uma fixture mínima representativa para validação documental de massa sintética. |
| `tools/ControlIdDeviceStub/fixtures/scenario-timeout.json` | Registra uma fixture de falha temporal sem dependência de equipamento físico. |
| `tools/coverage.runsettings` | Configura a coleta Cobertura e exclui artefatos gerados das métricas. |
| `tools/maintainability-baseline.json` | Versiona limites globais e exceções justificadas para arquivos legados extensos. |
| `tools/validate-coverage.ps1` | Valida pisos objetivos de cobertura de linhas e desvios no relatório Cobertura. |

## Política de geração e revisão

O inventário combina descoberta automática e descrição humana. O validador
documental compara arquivos de código e testes versionados com os caminhos desta
tabela; a descrição continua revisada por mantenedor porque responsabilidade não
pode ser inferida apenas do nome. Arquivos gerados em `bin/`, `obj/`, `Logs/` e
`artifacts/` não pertencem ao inventário.

Ao criar, mover ou remover arquivo relevante, atualize esta tabela na mesma
mudança. Uma entrada ausente ou obsoleta deve falhar a validação documental; não
reduza o escopo do validador para acomodar divergência.
