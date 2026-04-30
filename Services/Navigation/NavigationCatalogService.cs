using System;
using System.Collections.Generic;
using System.Linq;
using Integracao.ControlID.PoC.ViewModels.Shared;

namespace Integracao.ControlID.PoC.Services.Navigation
{
    public class NavigationCatalogService
    {
        private readonly IReadOnlyList<NavigationDomainViewModel> _domains;
        private readonly IReadOnlyList<NavigationModuleViewModel> _modules;
        private readonly IReadOnlyDictionary<string, NavigationDomainViewModel> _domainById;
        private readonly IReadOnlyDictionary<string, NavigationDomainViewModel> _domainByController;
        private readonly IReadOnlyDictionary<string, NavigationModuleViewModel> _moduleByRoute;
        private readonly IReadOnlyDictionary<string, NavigationModuleViewModel> _defaultModuleByController;

        public NavigationCatalogService()
        {
            _domains = BuildDomains();
            _modules = _domains.SelectMany(domain => domain.Modules).ToList();
            _domainById = _domains.ToDictionary(domain => domain.Id, StringComparer.OrdinalIgnoreCase);
            _moduleByRoute = _modules
                .GroupBy(module => BuildRouteKey(module.Controller, module.Action), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
            _defaultModuleByController = _modules
                .GroupBy(module => module.Controller, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(module => module.Visibility == "primary" ? 0 : 1)
                        .ThenBy(module => module.Priority)
                        .First(),
                    StringComparer.OrdinalIgnoreCase);
            _domainByController = _defaultModuleByController
                .ToDictionary(
                    pair => pair.Key,
                    pair => _domainById[pair.Value.DomainId],
                    StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyList<NavigationDomainViewModel> GetDomains()
        {
            return _domains;
        }

        public IReadOnlyList<NavigationModuleViewModel> GetAllModules()
        {
            return _modules;
        }

        public NavigationDomainViewModel? GetDomain(string? id)
        {
            return _domainById.TryGetValue(id ?? string.Empty, out var domain) ? domain : null;
        }

        public NavigationDomainViewModel? GetDomainByController(string? controller)
        {
            return _domainByController.TryGetValue(controller ?? string.Empty, out var domain) ? domain : null;
        }

        public NavigationModuleViewModel? GetModule(string? controller, string? action)
        {
            var normalizedController = controller ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedController))
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(action))
            {
                var routeKey = BuildRouteKey(normalizedController, action);
                if (_moduleByRoute.TryGetValue(routeKey, out var exact))
                {
                    return exact;
                }
            }

            return _defaultModuleByController.TryGetValue(normalizedController, out var fallback) ? fallback : null;
        }

        private static string BuildRouteKey(string? controller, string? action)
        {
            return $"{controller ?? string.Empty}:{action ?? string.Empty}";
        }

        private static IReadOnlyList<NavigationDomainViewModel> BuildDomains()
        {
            return
            [
                new NavigationDomainViewModel
                {
                    Id = "operations",
                    Title = "Conexão e operação",
                    ShortTitle = "Operação",
                    Description = "Conecte equipamentos, valide sessão, acompanhe o hardware e execute comandos remotos sem perder o contexto do dispositivo ativo.",
                    Summary = "Entrada operacional da PoC para testes de comunicação, sessão, estado do equipamento e comandos imediatos.",
                    AccentTone = "danger",
                    Icon = "OP",
                    Modules =
                    [
                        Module("operations", "Autenticação", "AU", "Fluxos oficiais de login e logout do equipamento para liberar chamadas autenticadas.", "Auth", "Login", "autenticacao login logout", "console", "primary", "Essencial", 1),
                        Module("operations", "Sessão", "SE", "Validação do contexto atual da sessão e visibilidade do token oficial em uso.", "Session", "Status", "sessao token status", "console", "primary", "Essencial", 2),
                        Module("operations", "Modos de operação", "MO", "Centraliza Standalone, Pro e Enterprise com aplicação de perfil, licenças e callbacks oficiais.", "OperationModes", "Index", "standalone pro enterprise online modos licenca", "console", "primary", "Essencial", 3),
                        Module("operations", "Sistema", "SI", "Informações do equipamento, manutenção administrativa e ações críticas do dispositivo.", "System", "Info", "sistema firmware rede tempo manutencao", "console", "primary", "Essencial", 4),
                        Module("operations", "Hardware", "HW", "GPIO, portas, biometria local e estado físico do equipamento.", "Hardware", "Status", "hardware gpio relay biometria portas", "console", "primary", "Operação", 5),
                        Module("operations", "Ações remotas", "AR", "Autorizações e comandos remotos com feedback imediato da operação.", "RemoteActions", "Index", "acoes remotas open door enroll message", "console", "primary", "Operação", 6),
                        Module("operations", "Catraca", "CT", "Eventos de giro, acionamentos e comandos dedicados para catracas.", "Catra", "Index", "catraca turnstile giro eventos", "console", "primary", "Operação", 7),
                        Module("operations", "Rede e SSL", "RE", "Configurações de rede, SSL e infraestrutura de comunicação do equipamento.", "System", "Network", "rede ssl tcp ip network", "console", "secondary", "Especializado", 20, "Requer equipamento conectado"),
                        Module("operations", "OpenVPN", "VP", "Configuração e estado do túnel OpenVPN suportado pelo equipamento.", "System", "Vpn", "vpn openvpn tunel", "console", "secondary", "Especializado", 21, "Requer equipamento conectado"),
                        Module("operations", "Credenciais do equipamento", "CR", "Troca de login e senha do dispositivo com fluxo oficial da API.", "System", "LoginCredentials", "credenciais login senha equipamento", "console", "secondary", "Especializado", 22, "Requer sessão autenticada"),
                        Module("operations", "Hash de senha", "HS", "Ferramenta auxiliar para gerar hash oficial de senha e aplicar em usuários.", "System", "HashPassword", "hash senha password", "console", "secondary", "Apoio", 23)
                    ]
                },
                new NavigationDomainViewModel
                {
                    Id = "people",
                    Title = "Pessoas e credenciais",
                    ShortTitle = "Pessoas",
                    Description = "Apresente visualmente todo o ciclo de usuários, credenciais, regras e materiais faciais com foco operacional.",
                    Summary = "Cadastros, validade e credenciais de acesso gerenciados pela PoC em uma trilha única.",
                    AccentTone = "success",
                    Icon = "PC",
                    Modules =
                    [
                        Module("people", "Usuários", "US", "Cadastro principal de pessoas, validade, senha e dados sincronizados.", "Users", "Index", "usuarios people access", "workspace", "primary", "Essencial", 1),
                        Module("people", "Templates biométricos", "BT", "Inventário e manutenção de biometrias vinculadas aos usuários.", "BiometricTemplates", "Index", "biometria template fingerprint", "workspace", "primary", "Operação", 2),
                        Module("people", "Cartões RFID", "RF", "Gestão de cartões físicos e sua vigência operacional.", "Cards", "Index", "cartoes rfid cards", "workspace", "primary", "Operação", 3),
                        Module("people", "QR Codes", "QR", "Credenciais QR e TOTP com visão clara de validade e distribuição.", "QRCodes", "Index", "qr qrcode totp", "workspace", "primary", "Operação", 4),
                        Module("people", "Grupos", "GR", "Agrupamento lógico de usuários para simplificar permissões e filtros.", "Groups", "Index", "grupos access groups", "workspace", "primary", "Operação", 5),
                        Module("people", "Regras de acesso", "RA", "Políticas, horários e vínculos de autorização por domínio operacional.", "AccessRules", "Index", "regras access rules timezones", "workspace", "primary", "Operação", 6),
                        Module("people", "Fotos faciais", "FT", "Uploads, previews e sincronização de imagens associadas aos usuários.", "Media", "Index", "media fotos face image", "workspace", "secondary", "Especializado", 20, "Indicado quando houver sincronização de imagens")
                    ]
                },
                new NavigationDomainViewModel
                {
                    Id = "infrastructure",
                    Title = "Dispositivo e configuração",
                    ShortTitle = "Infraestrutura",
                    Description = "Acesse inventário local, configurações oficiais, diagnósticos e telas auxiliares de infraestrutura sem misturar com as operações de negócio.",
                    Summary = "Configuração do dispositivo, inventário local e diagnósticos da integração em um domínio separado.",
                    AccentTone = "warning",
                    Icon = "IF",
                    Modules =
                    [
                        Module("infrastructure", "Dispositivos", "DV", "Inventário local dos equipamentos reconhecidos pela PoC.", "Devices", "Index", "devices inventario hardware local", "workspace", "primary", "Essencial", 1),
                        Module("infrastructure", "Configurações", "CF", "Painel principal de configuração com visão consolidada do estado oficial e local.", "Config", "Index", "config configuracoes json diagnostics", "workspace", "primary", "Essencial", 2),
                        Module("infrastructure", "Diagnósticos", "DG", "Ferramentas de diagnóstico da configuração para QA e troubleshooting.", "Config", "Diagnostics", "diagnosticos config checks", "console", "secondary", "Apoio", 20),
                        Module("infrastructure", "Configuração oficial", "OF", "Editor e leitura de payloads oficiais de configuração do equipamento.", "Config", "Official", "configuracao oficial payload", "explorer", "secondary", "Especializado", 21),
                        Module("infrastructure", "Logo do equipamento", "LG", "Gerenciamento dos slots de logo e branding embarcado no dispositivo.", "Logo", "Index", "logo branding equipment", "workspace", "secondary", "Especializado", 22)
                    ]
                },
                new NavigationDomainViewModel
                {
                    Id = "api",
                    Title = "API e exploração técnica",
                    ShortTitle = "API",
                    Description = "Separe claramente a navegação operacional da exploração técnica da API, endpoints, objetos oficiais e recursos específicos de produto.",
                    Summary = "Exploradores e construtores técnicos para cobrir integralmente a superfície da Access API.",
                    AccentTone = "neutral",
                    Icon = "AP",
                    Modules =
                    [
                        Module("api", "Catálogo oficial", "CA", "Inventário navegável dos endpoints documentados com acesso direto aos invocáveis.", "OfficialApi", "Index", "api catalogo endpoints official", "explorer", "primary", "Essencial", 1, isTechnical: true),
                        Module("api", "Objetos oficiais", "OB", "Workspace técnico de CRUD oficial com exemplos para todos os objetos documentados.", "OfficialObjects", "Index", "objects load create modify destroy", "explorer", "primary", "Essencial", 2, isTechnical: true),
                        Module("api", "Recursos avançados", "AV", "Exportação, câmera, intertravamento e chamadas menos frequentes da API.", "AdvancedOfficial", "Index", "advanced interlock export camera", "explorer", "primary", "Especializado", 3, isTechnical: true),
                        Module("api", "Recursos de produto", "PR", "Particularidades por equipamento, SIP, áudio, upgrade e capacidades específicas.", "ProductSpecific", "Index", "product specific sip audio upgrade", "explorer", "secondary", "Especializado", 20, "Indicado para validações por linha de equipamento", true),
                        Module("api", "Tópicos documentados", "TD", "Mapa funcional da documentação viva transformado em fluxos operacionais e técnicos.", "DocumentedFeatures", "Index", "documentacao docs topics", "explorer", "secondary", "Apoio", 21, "Aprofundamento técnico e temas menos frequentes", true),
                        Module("api", "Mapa funcional", "MF", "Índice global dos módulos e domínios da PoC com navegação orientada por experiência.", "Workspace", "Index", "mapa funcional explorer modulos", "dashboard", "secondary", "Apoio", 20, isTechnical: true)
                    ]
                },
                new NavigationDomainViewModel
                {
                    Id = "observability",
                    Title = "Eventos, push e auditoria",
                    ShortTitle = "Observabilidade",
                    Description = "Concentre eventos, filas, logs e compatibilidade de monitor em uma área dedicada de observabilidade e auditoria.",
                    Summary = "Eventos oficiais, push, auditoria e trilhas locais de operação em um só lugar.",
                    AccentTone = "info",
                    Icon = "EV",
                    Modules =
                    [
                        Module("observability", "Eventos oficiais", "EV", "Timeline persistida de callbacks, monitor e eventos online do equipamento.", "OfficialEvents", "Index", "eventos monitor callbacks", "timeline", "primary", "Essencial", 1),
                        Module("observability", "Central push", "PU", "Fila persistida do push oficial e retorno operacional por POST /result.", "PushCenter", "Index", "push queue result", "timeline", "primary", "Essencial", 2),
                        Module("observability", "Logs de acesso", "LA", "Auditoria dos eventos de acesso sincronizados e persistidos localmente.", "AccessLogs", "Index", "logs access audit", "timeline", "primary", "Operação", 3),
                        Module("observability", "Logs de alteração", "LC", "Mudanças administrativas e trilha de alteração do equipamento.", "ChangeLogs", "Index", "change logs auditoria", "timeline", "primary", "Operação", 4),
                        Module("observability", "Monitor compatível", "MC", "Visualização de compatibilidade para o fluxo legado de monitor push e webhook.", "Monitor", "Push", "monitor compat push webhook legado", "timeline", "secondary", "Compatibilidade", 20),
                        Module("observability", "Ocorrências internas", "ER", "Telas de erro e diagnóstico auxiliar para suporte interno da PoC.", "Errors", "Index", "errors falhas internas", "timeline", "secondary", "Apoio", 21, "Uso interno e troubleshooting")
                    ]
                }
            ];
        }

        private static NavigationModuleViewModel Module(
            string domainId,
            string label,
            string shortLabel,
            string description,
            string controller,
            string action,
            string tags,
            string experienceType,
            string visibility,
            string complexity,
            int priority,
            string? prerequisite = null,
            bool isTechnical = false)
        {
            return new NavigationModuleViewModel
            {
                Key = $"{controller}:{action}",
                DomainId = domainId,
                Label = label,
                ShortLabel = shortLabel,
                Description = description,
                Controller = controller,
                Action = action,
                Tags = tags,
                ExperienceType = experienceType,
                Visibility = visibility,
                Complexity = complexity,
                StatusText = visibility == "primary" ? "Recomendado" : "Especializado",
                StatusTone = visibility == "primary" ? "success" : "neutral",
                Prerequisite = prerequisite,
                Priority = priority,
                IsTechnical = isTechnical
            };
        }
    }
}
