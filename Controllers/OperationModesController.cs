using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Services.OperationModes;
using Integracao.ControlID.PoC.ViewModels.OperationModes;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    /// <summary>
    /// Orquestra a experiencia de Standalone, Pro e Enterprise, combinando leitura
    /// de configuracao oficial, aplicacao de perfis, licencas e sinais de callbacks.
    /// </summary>
    public class OperationModesController : Controller
    {
        private static readonly string[] RelevantCallbackPaths =
        {
            "/new_user_identified.fcgi",
            "/new_card.fcgi",
            "/new_biometric_image.fcgi",
            "/device_is_alive.fcgi",
            "/api/notifications/operation_mode"
        };

        private readonly OfficialControlIdApiService _apiService;
        private readonly OperationModesPayloadFactory _payloadFactory;
        private readonly OperationModesProfileResolver _profileResolver;
        private readonly OfficialApiResultPresentationService _resultPresentationService;
        private readonly MonitorEventRepository _monitorEventRepository;
        private readonly ILogger<OperationModesController> _logger;

        public OperationModesController(
            OfficialControlIdApiService apiService,
            OperationModesPayloadFactory payloadFactory,
            OperationModesProfileResolver profileResolver,
            OfficialApiResultPresentationService resultPresentationService,
            MonitorEventRepository monitorEventRepository,
            ILogger<OperationModesController> logger)
        {
            _apiService = apiService;
            _payloadFactory = payloadFactory;
            _profileResolver = profileResolver;
            _resultPresentationService = resultPresentationService;
            _monitorEventRepository = monitorEventRepository;
            _logger = logger;
        }

        /// <summary>
        /// Exibe o hub de modos de operacao com estado atual, prontidao de callbacks e acoes disponiveis.
        /// </summary>
        /// <returns>View principal dos modos de operacao.</returns>
        public async Task<IActionResult> Index()
        {
            var model = new OperationModesViewModel();
            await PrepareViewModelAsync(model);
            return View(model);
        }

        /// <summary>
        /// Aplica o perfil Standalone no equipamento, desligando o online e mantendo identificacao local.
        /// </summary>
        /// <param name="model">Estado atual da tela e mensagens de resultado.</param>
        /// <returns>View atualizada com resposta oficial ou erro sanitizado.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyStandalone(OperationModesViewModel model)
        {
            model.ActiveSection = "standalone";
            if (!EnsureConnected(model))
            {
                await PrepareViewModelAsync(model, populateRemoteState: false);
                return View("Index", model);
            }

            try
            {
                _logger.LogInformation("Applying operation mode Standalone.");
                var (result, document) = await _apiService.InvokeJsonAsync("set-configuration", _payloadFactory.BuildStandaloneSettings());
                _resultPresentationService.EnsureSuccess(result, "Erro ao aplicar o modo Standalone");

                model.ResultMessage = "Modo Standalone aplicado com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = _resultPresentationService.FormatJson(result.ResponseBody, document);
                _logger.LogInformation("Operation mode Standalone applied successfully. Status {StatusCode}.", result.StatusCode);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao aplicar o modo Standalone.");
            }

            await PrepareViewModelAsync(model);
            return View("Index", model);
        }

        /// <summary>
        /// Aplica o perfil Pro, garantindo um server_id e mantendo identificacao local ativa.
        /// </summary>
        /// <param name="model">Dados da tela usados para resolver servidor online e parametros do perfil.</param>
        /// <returns>View atualizada com resposta oficial ou erro sanitizado.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyPro(OperationModesViewModel model)
        {
            model.ActiveSection = "pro";
            if (!EnsureConnected(model))
            {
                await PrepareViewModelAsync(model, populateRemoteState: false);
                return View("Index", model);
            }

            try
            {
                var serverId = await ResolveServerIdAsync(model);
                _logger.LogInformation(
                    "Applying operation mode Pro with server_id {ServerId}. ExtractTemplate {ExtractTemplate}. MaxAttempts {MaxAttempts}.",
                    serverId,
                    model.ExtractTemplate,
                    model.MaxRequestAttempts);

                var (result, document) = await _apiService.InvokeJsonAsync(
                    "set-configuration",
                    _payloadFactory.BuildProSettings(serverId, model.ExtractTemplate, model.MaxRequestAttempts));

                _resultPresentationService.EnsureSuccess(result, "Erro ao aplicar o modo Pro");
                model.ResultMessage = $"Modo Pro aplicado com sucesso usando o server_id {serverId}.";
                model.ResultStatusType = "success";
                model.ResponseJson = _resultPresentationService.FormatJson(result.ResponseBody, document);
                _logger.LogInformation("Operation mode Pro applied successfully. ServerId {ServerId}. Status {StatusCode}.", serverId, result.StatusCode);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao aplicar o modo Pro.");
            }

            await PrepareViewModelAsync(model);
            return View("Index", model);
        }

        /// <summary>
        /// Aplica o perfil Enterprise, garantindo um server_id e desativando identificacao local.
        /// </summary>
        /// <param name="model">Dados da tela usados para resolver servidor online e parametros do perfil.</param>
        /// <returns>View atualizada com resposta oficial ou erro sanitizado.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyEnterprise(OperationModesViewModel model)
        {
            model.ActiveSection = "enterprise";
            if (!EnsureConnected(model))
            {
                await PrepareViewModelAsync(model, populateRemoteState: false);
                return View("Index", model);
            }

            try
            {
                var serverId = await ResolveServerIdAsync(model);
                _logger.LogInformation(
                    "Applying operation mode Enterprise with server_id {ServerId}. ExtractTemplate {ExtractTemplate}. MaxAttempts {MaxAttempts}.",
                    serverId,
                    model.ExtractTemplate,
                    model.MaxRequestAttempts);

                var (result, document) = await _apiService.InvokeJsonAsync(
                    "set-configuration",
                    _payloadFactory.BuildEnterpriseSettings(serverId, model.ExtractTemplate, model.MaxRequestAttempts));

                _resultPresentationService.EnsureSuccess(result, "Erro ao aplicar o modo Enterprise");
                model.ResultMessage = $"Modo Enterprise aplicado com sucesso usando o server_id {serverId}.";
                model.ResultStatusType = "success";
                model.ResponseJson = _resultPresentationService.FormatJson(result.ResponseBody, document);
                _logger.LogInformation("Operation mode Enterprise applied successfully. ServerId {ServerId}. Status {StatusCode}.", serverId, result.StatusCode);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao aplicar o modo Enterprise.");
            }

            await PrepareViewModelAsync(model);
            return View("Index", model);
        }

        /// <summary>
        /// Solicita o upgrade Pro do iDFace sem registrar o valor sensivel da licenca nos logs.
        /// </summary>
        /// <param name="model">Modelo contendo a licenca informada pelo operador.</param>
        /// <returns>View atualizada com status do upgrade.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpgradeProLicense(OperationModesViewModel model)
        {
            model.ActiveSection = "licenses";
            if (!EnsureConnected(model))
            {
                await PrepareViewModelAsync(model, populateRemoteState: false);
                return View("Index", model);
            }

            if (string.IsNullOrWhiteSpace(model.ProLicensePassword))
            {
                model.ErrorMessage = "Informe a senha/licença Pro para solicitar o upgrade do iDFace.";
                await PrepareViewModelAsync(model);
                return View("Index", model);
            }

            try
            {
                _logger.LogInformation("Requesting iDFace Pro upgrade through official endpoint.");
                var result = await _apiService.InvokeAsync("upgrade-idface-pro", new { password = model.ProLicensePassword });
                _resultPresentationService.EnsureSuccess(result, "Erro ao executar o upgrade Pro");

                model.ResultMessage = "Upgrade Pro solicitado com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = _resultPresentationService.FormatResponseBody(result);
                _logger.LogInformation("iDFace Pro upgrade request completed. Status {StatusCode}.", result.StatusCode);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao solicitar o upgrade Pro.");
            }

            await PrepareViewModelAsync(model);
            return View("Index", model);
        }

        /// <summary>
        /// Solicita o upgrade Enterprise em linhas compativeis sem registrar o valor sensivel da licenca.
        /// </summary>
        /// <param name="model">Modelo contendo a licenca informada pelo operador.</param>
        /// <returns>View atualizada com status do upgrade.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpgradeEnterpriseLicense(OperationModesViewModel model)
        {
            model.ActiveSection = "licenses";
            if (!EnsureConnected(model))
            {
                await PrepareViewModelAsync(model, populateRemoteState: false);
                return View("Index", model);
            }

            if (string.IsNullOrWhiteSpace(model.EnterpriseLicensePassword))
            {
                model.ErrorMessage = "Informe a senha/licença Enterprise para solicitar o upgrade da linha compatível.";
                await PrepareViewModelAsync(model);
                return View("Index", model);
            }

            try
            {
                _logger.LogInformation("Requesting Enterprise upgrade through official endpoint.");
                var result = await _apiService.InvokeAsync("upgrade-idflex-enterprise", new { password = model.EnterpriseLicensePassword });
                _resultPresentationService.EnsureSuccess(result, "Erro ao executar o upgrade Enterprise");

                model.ResultMessage = "Upgrade Enterprise solicitado com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = _resultPresentationService.FormatResponseBody(result);
                _logger.LogInformation("Enterprise upgrade request completed. Status {StatusCode}.", result.StatusCode);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao solicitar o upgrade Enterprise.");
            }

            await PrepareViewModelAsync(model);
            return View("Index", model);
        }

        /// <summary>
        /// Valida a sessao oficial atual antes de aplicar perfis ou upgrades sensiveis.
        /// </summary>
        /// <param name="model">Estado da tela usado para exibir o resultado da validacao.</param>
        /// <returns>View atualizada com o status da sessao oficial.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidateSession(OperationModesViewModel model)
        {
            model.ActiveSection = "session";
            if (!EnsureConnected(model))
            {
                await PrepareViewModelAsync(model, populateRemoteState: false);
                return View("Index", model);
            }

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("session-is-valid");
                _resultPresentationService.EnsureSuccess(result, "Erro ao validar a sessão oficial");

                var sessionIsValid = document == null || GetRootBool(document.RootElement, "session_is_valid", true);
                _logger.LogInformation(
                    "Official session validation completed. IsValid {IsValid}. Status {StatusCode}.",
                    sessionIsValid,
                    result.StatusCode);

                model.ResultMessage = sessionIsValid
                    ? "Sessão oficial validada com sucesso."
                    : "A sessão oficial foi respondida como inválida pelo equipamento.";
                model.ResultStatusType = sessionIsValid ? "success" : "warning";
                model.ResponseJson = _resultPresentationService.FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao validar a sessão oficial.");
            }

            await PrepareViewModelAsync(model);
            return View("Index", model);
        }

        /// <summary>
        /// Prepara o estado completo da tela, combinando dados remotos do equipamento e sinais locais de monitoramento.
        /// </summary>
        /// <param name="model">ViewModel que recebera os dados consolidados.</param>
        /// <param name="populateRemoteState">Indica se a PoC deve consultar o equipamento antes de renderizar.</param>
        private async Task PrepareViewModelAsync(OperationModesViewModel model, bool populateRemoteState = true)
        {
            ApplyRuntimeDefaults(model);

            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.IsConnected = false;
                model.SessionValidated = false;
                model.SessionStatusSummary = "Conecte e autentique um equipamento para validar a sessão oficial.";
                model.CurrentModeKey = "unknown";
                model.CurrentModeLabel = "Aguardando equipamento";
                model.CurrentModeDescription = "A PoC precisa de um equipamento conectado para identificar o modo de operação atual.";
                model.CurrentModeTone = "neutral";
                model.CurrentModeEvidence = "Sem equipamento conectado.";
                model.Readiness = BuildReadiness(Array.Empty<MonitorEventLocal>());
                model.RecentSignals = Array.Empty<OperationModeSignalViewModel>();
                PopulateStaticProfiles(model);
                return;
            }

            model.IsConnected = true;

            if (populateRemoteState)
            {
                await PopulateCurrentStateAsync(model);
            }

            var events = await _monitorEventRepository.GetAllMonitorEventsAsync();
            model.Readiness = BuildReadiness(events);
            model.RecentSignals = BuildRecentSignals(events);
            PopulateStaticProfiles(model);
        }

        /// <summary>
        /// Preenche URLs padrao baseadas na requisicao atual para facilitar callbacks e criacao de servidor online.
        /// </summary>
        /// <param name="model">ViewModel que recebera os defaults calculados.</param>
        private void ApplyRuntimeDefaults(OperationModesViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ServerUrl))
            {
                model.ServerUrl = $"{Request.Scheme}://{Request.Host}";
            }

            model.CallbackBaseUrl ??= $"{Request.Scheme}://{Request.Host}";
        }

        /// <summary>
        /// Consulta o equipamento para detectar modo atual, sessao, produto e serial.
        /// </summary>
        /// <param name="model">ViewModel que recebera o estado remoto consolidado.</param>
        private async Task PopulateCurrentStateAsync(OperationModesViewModel model)
        {
            try
            {
                var (_, document) = await _apiService.InvokeJsonAsync("get-configuration", new
                {
                    general = new[] { "online", "local_identification" },
                    online_client = new[] { "server_id", "extract_template", "max_request_attempts" }
                });

                if (document != null)
                {
                    model.OnlineEnabled = GetConfigBool(document.RootElement, "general", "online");
                    model.LocalIdentificationEnabled = GetConfigBool(document.RootElement, "general", "local_identification", true);
                    model.ExtractTemplateEnabled = GetConfigBool(document.RootElement, "online_client", "extract_template");
                    model.ExtractTemplate = model.ExtractTemplateEnabled;
                    model.MaxRequestAttempts = GetConfigInt(document.RootElement, "online_client", "max_request_attempts", model.MaxRequestAttempts);
                    model.CurrentServerId = GetConfigLong(document.RootElement, "online_client", "server_id");
                    model.ExistingDeviceId = model.CurrentServerId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nao foi possivel ler a configuracao atual dos modos de operacao.");
            }

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("session-is-valid");
                model.SessionValidated = result.Success && (document == null || GetRootBool(document.RootElement, "session_is_valid", true));
                model.SessionStatusSummary = model.SessionValidated
                    ? "Sessão oficial válida para aplicar perfis e upgrades."
                    : "A sessão oficial não está válida. Refaça o login antes de alterar o modo.";
            }
            catch (Exception ex)
            {
                model.SessionValidated = false;
                model.SessionStatusSummary = "Não foi possível validar a sessão oficial agora.";
                _logger.LogWarning(ex, "Nao foi possivel validar a sessao oficial no hub de modos.");
            }

            try
            {
                var (_, document) = await _apiService.InvokeJsonAsync("system-information");
                if (document != null)
                {
                    model.DetectedProductModel = GetRootString(document.RootElement, "product_name");
                    model.DetectedSerialNumber = GetRootString(document.RootElement, "serial");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nao foi possivel obter as informacoes do produto para o hub de modos.");
            }

            var snapshot = _profileResolver.Resolve(model.OnlineEnabled, model.LocalIdentificationEnabled);
            _logger.LogDebug(
                "Operation mode snapshot resolved as {Mode}. Online {Online}. LocalIdentification {LocalIdentification}. ServerId {ServerId}.",
                snapshot.Key,
                model.OnlineEnabled,
                model.LocalIdentificationEnabled,
                model.CurrentServerId);

            model.CurrentModeKey = snapshot.Key;
            model.CurrentModeLabel = snapshot.Label;
            model.CurrentModeDescription = snapshot.Description;
            model.CurrentModeTone = snapshot.Key switch
            {
                "standalone" => "warning",
                "pro" => "success",
                "enterprise" => "danger",
                _ => "neutral"
            };
            model.CurrentModeEvidence = $"online={(model.OnlineEnabled ? "1" : "0")} | local_identification={(model.LocalIdentificationEnabled ? "1" : "0")} | server_id={(model.CurrentServerId?.ToString() ?? "-")}";
        }

        /// <summary>
        /// Monta os cards estaticos de Standalone, Pro e Enterprise exibidos na interface.
        /// </summary>
        /// <param name="model">ViewModel que recebera a colecao de perfis.</param>
        private void PopulateStaticProfiles(OperationModesViewModel model)
        {
            model.Profiles = new[]
            {
                new OperationModeProfileCardViewModel
                {
                    Key = "standalone",
                    Label = "Standalone",
                    Summary = "Mantém a operação no próprio equipamento, útil para contingência, piloto local e cenários sem servidor online.",
                    Tone = "warning",
                    IsCurrent = string.Equals(model.CurrentModeKey, "standalone", StringComparison.OrdinalIgnoreCase),
                    Requirements = "Requer apenas sessão oficial ativa. Não depende de server_id.",
                    Checklist = "Define online=0 e preserva identificação local no equipamento.",
                    SubmitAction = nameof(ApplyStandalone),
                    SubmitLabel = "Aplicar Standalone",
                    SubmitAriaLabel = "Aplicar o perfil Standalone neste equipamento"
                },
                new OperationModeProfileCardViewModel
                {
                    Key = "pro",
                    Label = "Pro",
                    Summary = "Liga o modo online mantendo a identificação local ativa, com callbacks para eventos de usuário identificado e sincronização.",
                    Tone = "success",
                    IsCurrent = string.Equals(model.CurrentModeKey, "pro", StringComparison.OrdinalIgnoreCase),
                    Requirements = "Requer server_id e, quando aplicável, upgrade Pro da linha iDFace.",
                    Checklist = "Define online=1, local_identification=1 e usa o servidor online configurado.",
                    SubmitAction = nameof(ApplyPro),
                    SubmitLabel = "Aplicar Pro",
                    SubmitAriaLabel = "Aplicar o perfil Pro neste equipamento"
                },
                new OperationModeProfileCardViewModel
                {
                    Key = "enterprise",
                    Label = "Enterprise",
                    Summary = "Mantém o equipamento online com identificação centralizada no servidor, adequado para topologias corporativas e integrações avançadas.",
                    Tone = "danger",
                    IsCurrent = string.Equals(model.CurrentModeKey, "enterprise", StringComparison.OrdinalIgnoreCase),
                    Requirements = "Requer server_id e, quando aplicável, upgrade Enterprise da linha iDFlex ou iDAccess Nano.",
                    Checklist = "Define online=1, local_identification=0 e mantém a operação orientada ao servidor.",
                    SubmitAction = nameof(ApplyEnterprise),
                    SubmitLabel = "Aplicar Enterprise",
                    SubmitAriaLabel = "Aplicar o perfil Enterprise neste equipamento"
                }
            };
        }

        /// <summary>
        /// Consolida a prontidao dos callbacks usados pelos modos online.
        /// </summary>
        /// <param name="events">Eventos de monitoramento persistidos localmente.</param>
        /// <returns>Itens de prontidao exibidos na tela de modos.</returns>
        private IReadOnlyList<OperationModeReadinessViewModel> BuildReadiness(IEnumerable<MonitorEventLocal> events)
        {
            return new[]
            {
                BuildReadinessItem(events, "Identificação Pro", "Callback oficial para identificar localmente em modo Pro.", "/new_user_identified.fcgi"),
                BuildReadinessItem(events, "Identificação por cartão", "Evento online para cartões processados pelo servidor.", "/new_card.fcgi"),
                BuildReadinessItem(events, "Imagem biométrica", "Recepção de imagem biométrica para fluxos Enterprise e testes faciais.", "/new_biometric_image.fcgi"),
                BuildReadinessItem(events, "Keep-alive do dispositivo", "Callback de contingência para verificar se o equipamento segue vivo no modo online.", "/device_is_alive.fcgi"),
                BuildReadinessItem(events, "Monitor de modo", "Notificação oficial de mudança de modo via monitor compatível.", "/api/notifications/operation_mode")
            };
        }

        /// <summary>
        /// Filtra os eventos recentes relevantes para Standalone, Pro e Enterprise.
        /// </summary>
        /// <param name="events">Eventos de monitoramento persistidos localmente.</param>
        /// <returns>Ultimos sinais operacionais associados aos modos.</returns>
        private IReadOnlyList<OperationModeSignalViewModel> BuildRecentSignals(IEnumerable<MonitorEventLocal> events)
        {
            return events
                .Where(evt => RelevantCallbackPaths.Any(path => EndsWithPath(evt.EventType, path)))
                .OrderByDescending(evt => evt.ReceivedAt)
                .Take(6)
                .Select(evt => new OperationModeSignalViewModel
                {
                    Label = ToFriendlyPathLabel(evt.EventType),
                    Description = string.IsNullOrWhiteSpace(evt.DeviceId)
                        ? "Evento recebido sem identificação explícita do device_id."
                        : $"Evento recebido do device {evt.DeviceId}.",
                    Tone = EndsWithPath(evt.EventType, "/api/notifications/operation_mode") ? "danger" : "success",
                    ReceivedAt = evt.ReceivedAt
                })
                .ToList();
        }

        /// <summary>
        /// Cria um item de prontidao para uma rota de callback especifica.
        /// </summary>
        /// <param name="events">Eventos persistidos usados para localizar a ultima ocorrencia.</param>
        /// <param name="title">Titulo amigavel exibido na UI.</param>
        /// <param name="description">Descricao operacional da rota.</param>
        /// <param name="path">Path oficial esperado para o callback.</param>
        /// <returns>ViewModel de prontidao para a rota informada.</returns>
        private OperationModeReadinessViewModel BuildReadinessItem(
            IEnumerable<MonitorEventLocal> events,
            string title,
            string description,
            string path)
        {
            var lastEvent = events
                .Where(evt => EndsWithPath(evt.EventType, path))
                .OrderByDescending(evt => evt.ReceivedAt)
                .FirstOrDefault();

            return new OperationModeReadinessViewModel
            {
                Title = title,
                Description = description,
                Path = $"{Request.Scheme}://{Request.Host}{path}",
                Tone = lastEvent == null ? "neutral" : "success",
                StatusText = lastEvent == null
                    ? "Aguardando primeiro callback"
                    : $"Último evento em {lastEvent.ReceivedAt:dd/MM/yyyy HH:mm}",
                LastReceivedAt = lastEvent?.ReceivedAt
            };
        }

        /// <summary>
        /// Resolve o server_id usado pelos modos online, reutilizando um device existente ou criando um novo objeto oficial.
        /// </summary>
        /// <param name="model">Dados informados pelo operador para o servidor online.</param>
        /// <returns>Identificador do servidor online que deve ser usado no payload de configuracao.</returns>
        private async Task<long> ResolveServerIdAsync(OperationModesViewModel model)
        {
            if (model.ReuseExistingDevice)
            {
                if (!model.ExistingDeviceId.HasValue || model.ExistingDeviceId.Value <= 0)
                {
                    throw new InvalidOperationException("Informe um ID de device existente para reutilizar o servidor online.");
                }

                _logger.LogInformation("Reusing existing online server device id {ServerId} for operation mode profile.", model.ExistingDeviceId.Value);
                return model.ExistingDeviceId.Value;
            }

            if (string.IsNullOrWhiteSpace(model.ServerName))
            {
                throw new InvalidOperationException("Informe um nome para o servidor online.");
            }

            if (string.IsNullOrWhiteSpace(model.ServerUrl))
            {
                throw new InvalidOperationException("Informe a URL pública da PoC para criar o servidor online.");
            }

            _logger.LogInformation("Creating online server definition for operation mode profile. ServerName {ServerName}.", model.ServerName);
            var (result, document) = await _apiService.InvokeJsonAsync(
                "create-objects",
                _payloadFactory.BuildOnlineServerDefinition(model.ServerName, model.ServerUrl, model.PublicKey));

            _resultPresentationService.EnsureSuccess(result, "Erro ao criar o device servidor para o modo online");
            var serverId = ReadFirstId(document) ?? throw new InvalidOperationException("A API não retornou um server_id válido.");
            _logger.LogInformation("Online server definition created for operation mode profile. ServerId {ServerId}.", serverId);
            return serverId;
        }

        /// <summary>
        /// Garante que a PoC possui equipamento e sessao antes de executar acoes oficiais.
        /// </summary>
        /// <param name="model">ViewModel que recebera mensagem segura quando a conexao estiver ausente.</param>
        /// <returns>True quando ha conexao ativa; caso contrario, false.</returns>
        private bool EnsureConnected(OperationModesViewModel model)
        {
            if (_apiService.TryGetConnection(out _, out _))
            {
                model.IsConnected = true;
                return true;
            }

            model.IsConnected = false;
            model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD antes de aplicar um modo de operação.";
            _logger.LogWarning("Operation mode action blocked because no active Control iD connection was found.");
            return false;
        }

        private static bool EndsWithPath(string? eventType, string path)
        {
            return (eventType ?? string.Empty).EndsWith(path, StringComparison.OrdinalIgnoreCase);
        }

        private static string ToFriendlyPathLabel(string? eventType)
        {
            return eventType switch
            {
                string value when EndsWithPath(value, "/new_user_identified.fcgi") => "Usuário identificado (Pro)",
                string value when EndsWithPath(value, "/new_card.fcgi") => "Cartão online",
                string value when EndsWithPath(value, "/new_biometric_image.fcgi") => "Imagem biométrica",
                string value when EndsWithPath(value, "/device_is_alive.fcgi") => "Keep-alive",
                string value when EndsWithPath(value, "/api/notifications/operation_mode") => "Monitor de modo",
                _ => "Evento operacional"
            };
        }

        /// <summary>
        /// Extrai o primeiro id retornado por endpoints oficiais que respondem no formato `ids`.
        /// </summary>
        /// <param name="document">Documento JSON retornado pela API oficial.</param>
        /// <returns>Primeiro id numerico encontrado ou null quando o contrato nao contem ids validos.</returns>
        private static long? ReadFirstId(JsonDocument? document)
        {
            if (document == null || !document.RootElement.TryGetProperty("ids", out var ids) || ids.ValueKind != JsonValueKind.Array || ids.GetArrayLength() == 0)
            {
                return null;
            }

            var first = ids[0];
            if (first.ValueKind == JsonValueKind.Number && first.TryGetInt64(out var numeric))
            {
                return numeric;
            }

            return first.ValueKind == JsonValueKind.String && long.TryParse(first.GetString(), out var parsed) ? parsed : null;
        }

        /// <summary>
        /// Le um valor textual de configuracao no JSON oficial, preservando fallback seguro.
        /// </summary>
        /// <param name="root">Elemento raiz do JSON retornado por `get-configuration`.</param>
        /// <param name="section">Secao de configuracao, por exemplo `general`.</param>
        /// <param name="field">Campo dentro da secao.</param>
        /// <param name="fallback">Valor usado quando a secao ou campo nao existem.</param>
        /// <returns>Valor textual normalizado ou fallback.</returns>
        private static string GetConfigString(JsonElement root, string section, string field, string fallback = "")
        {
            if (root.TryGetProperty(section, out var sectionElement) &&
                sectionElement.ValueKind == JsonValueKind.Object &&
                sectionElement.TryGetProperty(field, out var fieldElement))
            {
                return fieldElement.ToString() ?? fallback;
            }

            return fallback;
        }

        /// <summary>
        /// Le uma configuracao booleana no formato usado pela API, aceitando `1` e `true`.
        /// </summary>
        /// <param name="root">Elemento raiz do JSON retornado por `get-configuration`.</param>
        /// <param name="section">Secao de configuracao.</param>
        /// <param name="field">Campo booleano dentro da secao.</param>
        /// <param name="fallback">Valor padrao quando a configuracao nao esta presente.</param>
        /// <returns>Booleano interpretado a partir do contrato oficial.</returns>
        private static bool GetConfigBool(JsonElement root, string section, string field, bool fallback = false)
        {
            var value = GetConfigString(root, section, field, fallback ? "1" : "0");
            return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Le uma configuracao inteira no JSON oficial.
        /// </summary>
        /// <param name="root">Elemento raiz do JSON retornado por `get-configuration`.</param>
        /// <param name="section">Secao de configuracao.</param>
        /// <param name="field">Campo numerico dentro da secao.</param>
        /// <param name="fallback">Valor padrao quando a conversao falha.</param>
        /// <returns>Inteiro convertido ou fallback.</returns>
        private static int GetConfigInt(JsonElement root, string section, string field, int fallback)
        {
            var value = GetConfigString(root, section, field, fallback.ToString());
            return int.TryParse(value, out var parsed) ? parsed : fallback;
        }

        /// <summary>
        /// Le uma configuracao long opcional no JSON oficial.
        /// </summary>
        /// <param name="root">Elemento raiz do JSON retornado por `get-configuration`.</param>
        /// <param name="section">Secao de configuracao.</param>
        /// <param name="field">Campo numerico dentro da secao.</param>
        /// <returns>Valor long convertido ou null quando ausente/invalido.</returns>
        private static long? GetConfigLong(JsonElement root, string section, string field)
        {
            var value = GetConfigString(root, section, field, string.Empty);
            return long.TryParse(value, out var parsed) ? parsed : null;
        }

        /// <summary>
        /// Le uma propriedade textual diretamente da raiz de um retorno oficial.
        /// </summary>
        /// <param name="root">Elemento raiz do JSON oficial.</param>
        /// <param name="name">Nome da propriedade.</param>
        /// <returns>Valor textual ou null quando ausente.</returns>
        private static string? GetRootString(JsonElement root, string name)
        {
            return root.TryGetProperty(name, out var value) ? value.ToString() : null;
        }

        /// <summary>
        /// Le uma propriedade booleana diretamente da raiz aceitando booleano, numero ou texto.
        /// </summary>
        /// <param name="root">Elemento raiz do JSON oficial.</param>
        /// <param name="name">Nome da propriedade.</param>
        /// <param name="fallback">Valor padrao quando a propriedade nao existe ou nao e reconhecida.</param>
        /// <returns>Booleano interpretado a partir da propriedade.</returns>
        private static bool GetRootBool(JsonElement root, string name, bool fallback = false)
        {
            if (!root.TryGetProperty(name, out var value))
            {
                return fallback;
            }

            return value.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number => value.TryGetInt32(out var number) && number != 0,
                JsonValueKind.String => value.GetString() is string text && (text == "1" || text.Equals("true", StringComparison.OrdinalIgnoreCase)),
                _ => fallback
            };
        }
    }
}
