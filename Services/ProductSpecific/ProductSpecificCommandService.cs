using System.Text.Json;
using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.Files;
using Integracao.ControlID.PoC.ViewModels.ProductSpecific;

namespace Integracao.ControlID.PoC.Services.ProductSpecific;

public sealed class ProductSpecificCommandService
{
    private readonly IOfficialControlIdApiService _apiService;
    private readonly ProductSpecificConfigurationPayloadFactory _payloadFactory;
    private readonly ProductSpecificSnapshotService _snapshotService;
    private readonly ProductSpecificJsonReader _reader;
    private readonly OfficialApiResultPresentationService _resultPresentationService;
    private readonly UploadedFileBase64Encoder _fileEncoder;
    private readonly ILogger<ProductSpecificCommandService> _logger;

    public ProductSpecificCommandService(
        IOfficialControlIdApiService apiService,
        ProductSpecificConfigurationPayloadFactory payloadFactory,
        ProductSpecificSnapshotService snapshotService,
        ProductSpecificJsonReader reader,
        OfficialApiResultPresentationService resultPresentationService,
        UploadedFileBase64Encoder fileEncoder,
        ILogger<ProductSpecificCommandService> logger)
    {
        _apiService = apiService;
        _payloadFactory = payloadFactory;
        _snapshotService = snapshotService;
        _reader = reader;
        _resultPresentationService = resultPresentationService;
        _fileEncoder = fileEncoder;
        _logger = logger;
    }

    public Task UpgradeIdFaceAsync(ProductSpecificViewModel model) =>
        ExecuteAsync(
            model,
            ProductSpecificSections.Upgrades,
            "Erro ao executar upgrade Pro do iDFace",
            "Upgrade Pro do iDFace solicitado com sucesso.",
            "Erro ao executar upgrade Pro do iDFace.",
            () => _apiService.InvokeAsync("upgrade-idface-pro", new { password = model.IdFaceProPassword }),
            result =>
            {
                model.ResponseJson = _resultPresentationService.FormatResponseBody(result);
                return Task.CompletedTask;
            });

    public Task UpgradeEnterpriseAsync(ProductSpecificViewModel model) =>
        ExecuteAsync(
            model,
            ProductSpecificSections.Upgrades,
            "Erro ao executar upgrade Enterprise",
            "Upgrade Enterprise solicitado com sucesso.",
            "Erro ao executar upgrade Enterprise.",
            () => _apiService.InvokeAsync("upgrade-idflex-enterprise", new { password = model.IdFlexEnterprisePassword }),
            result =>
            {
                model.ResponseJson = _resultPresentationService.FormatResponseBody(result);
                return Task.CompletedTask;
            });

    public Task ApplyFacialSettingsAsync(ProductSpecificViewModel model) =>
        ExecuteConfigurationAsync(model, ProductSpecificSections.Facial, "Erro ao aplicar configurações faciais", "Configurações faciais atualizadas com sucesso.", "Erro ao aplicar configurações faciais.", _payloadFactory.BuildFacialSettings(model));

    public Task ApplyQrCodeSettingsAsync(ProductSpecificViewModel model) =>
        ExecuteConfigurationAsync(model, ProductSpecificSections.Qr, "Erro ao aplicar configurações de QR Code", "Configurações de QR Code/TOTP atualizadas com sucesso.", "Erro ao aplicar configurações de QR Code.", _payloadFactory.BuildQrCodeSettings(model));

    public Task ApplyPowerSettingsAsync(ProductSpecificViewModel model) =>
        ExecuteConfigurationAsync(model, ProductSpecificSections.Power, "Erro ao aplicar configurações de energia", "Configurações de energia e screenshot atualizadas com sucesso.", "Erro ao aplicar configurações de energia.", _payloadFactory.BuildPowerSettings(model));

    public Task ApplyStreamingSettingsAsync(ProductSpecificViewModel model) =>
        ExecuteConfigurationAsync(model, ProductSpecificSections.Streaming, "Erro ao aplicar configurações de streaming", "Configurações de streaming atualizadas com sucesso.", "Erro ao aplicar configurações de streaming.", _payloadFactory.BuildStreamingSettings(model));

    public Task ApplySipSettingsAsync(ProductSpecificViewModel model) =>
        ExecuteConfigurationAsync(model, ProductSpecificSections.Sip, "Erro ao aplicar configurações SIP", "Configurações SIP atualizadas com sucesso.", "Erro ao aplicar configurações SIP.", _payloadFactory.BuildSipSettings(model), () => _snapshotService.PopulateSipAsync(model));

    public Task ApplyAccessAudioSettingsAsync(ProductSpecificViewModel model) =>
        ExecuteConfigurationAsync(model, ProductSpecificSections.AccessAudio, "Erro ao aplicar configurações de áudio de acesso", "Configurações de áudio de acesso atualizadas com sucesso.", "Erro ao aplicar configurações de áudio de acesso.", _payloadFactory.BuildAccessAudioSettings(model));

    public Task ApplySignalsAsync(ProductSpecificViewModel model) =>
        ExecuteConfigurationAsync(model, ProductSpecificSections.Signals, "Erro ao aplicar sinais customizados", "Sinais customizados atualizados com sucesso.", "Erro ao aplicar sinais customizados.", _payloadFactory.BuildSignalsSettings(model));

    public Task RefreshLedsAsync(ProductSpecificViewModel model) =>
        ExecuteAsync(
            model,
            ProductSpecificSections.Signals,
            "Erro ao recarregar configuracao de LEDs",
            "Configuracao de LEDs reaplicada com sucesso.",
            "Erro ao recarregar configuracao de LEDs.",
            () => _apiService.InvokeAsync("reread-leds"),
            result =>
            {
                model.ResponseJson = _resultPresentationService.FormatResponseBody(result);
                return Task.CompletedTask;
            });

    public Task RefreshSipStatusAsync(ProductSpecificViewModel model) =>
        ExecuteJsonAsync(
            model,
            ProductSpecificSections.Sip,
            "Erro ao consultar status do SIP",
            "Status do SIP atualizado.",
            "Erro ao consultar status do SIP.",
            () => _apiService.InvokeJsonAsync("get-sip-status"),
            document =>
            {
                if (document == null)
                    return Task.CompletedTask;

                model.SipStatusCode = _reader.GetRootInt(document.RootElement, "status", model.SipStatusCode);
                model.SipInCall = _reader.GetRootBool(document.RootElement, "in_call");
                return Task.CompletedTask;
            });

    public Task MakeSipCallAsync(ProductSpecificViewModel model) =>
        ExecuteJsonAsync(
            model,
            ProductSpecificSections.Sip,
            "Erro ao iniciar chamada SIP",
            $"Chamada SIP iniciada para {model.SipCallTarget}.",
            "Erro ao iniciar chamada SIP.",
            () => _apiService.InvokeJsonAsync("make-sip-call", new { target = model.SipCallTarget }),
            _ => Task.CompletedTask,
            () => _snapshotService.PopulateSipAsync(model));

    public Task FinalizeSipCallAsync(ProductSpecificViewModel model) =>
        ExecuteJsonAsync(
            model,
            ProductSpecificSections.Sip,
            "Erro ao finalizar chamada SIP",
            "Chamada SIP finalizada com sucesso.",
            "Erro ao finalizar chamada SIP.",
            () => _apiService.InvokeJsonAsync("finalize-sip-call"),
            _ => Task.CompletedTask,
            () => _snapshotService.PopulateSipAsync(model));

    public Task CheckSipAudioAsync(ProductSpecificViewModel model) =>
        ExecuteJsonAsync(
            model,
            ProductSpecificSections.SipAudio,
            "Erro ao consultar áudio customizado do SIP",
            string.Empty,
            "Erro ao consultar áudio customizado do SIP.",
            () => _apiService.InvokeJsonAsync("has-pjsip-audio-message"),
            document =>
            {
                model.SipAudioExists = document != null && _reader.GetRootBool(document.RootElement, "file_exists");
                model.ResultMessage = model.SipAudioExists
                    ? "O dispositivo possui um toque customizado."
                    : "Não há toque customizado cadastrado.";
                return Task.CompletedTask;
            });

    public async Task UploadSipAudioAsync(ProductSpecificViewModel model)
    {
        model.ActiveSection = ProductSpecificSections.SipAudio;

        try
        {
            var base64 = await _fileEncoder.EncodeAsync(model.SipAudioFile, "Selecione um arquivo WAV para o toque SIP.");
            var result = await _apiService.InvokeAsync("set-pjsip-audio-message", base64, $"current={model.SipAudioCurrent}&total={model.SipAudioTotal}");
            _resultPresentationService.EnsureSuccess(result, "Erro ao enviar áudio customizado do SIP");
            await _snapshotService.PopulateSipAudioAsync(model);
            model.ResultMessage = "Áudio customizado do SIP enviado com sucesso.";
            model.ResultStatusType = "success";
            model.ResponseJson = _resultPresentationService.FormatResponseBody(result);
        }
        catch (Exception ex)
        {
            model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
            _logger.LogError(ex, "Erro ao enviar áudio customizado do SIP.");
        }
    }

    public async Task<ProductSpecificDownloadResult?> DownloadSipAudioAsync(ProductSpecificViewModel model)
    {
        model.ActiveSection = ProductSpecificSections.SipAudio;

        try
        {
            var result = await _apiService.InvokeAsync("get-pjsip-audio-message");
            _resultPresentationService.EnsureSuccess(result, "Erro ao baixar áudio customizado do SIP");
            return new ProductSpecificDownloadResult(result, "sip-ring.wav", "audio/wav");
        }
        catch (Exception ex)
        {
            model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
            _logger.LogError(ex, "Erro ao baixar áudio customizado do SIP.");
            return null;
        }
    }

    public Task CheckAccessAudioAsync(ProductSpecificViewModel model) =>
        ExecuteJsonAsync(
            model,
            ProductSpecificSections.AccessAudio,
            "Erro ao consultar áudios de acesso",
            "Presença dos áudios de acesso atualizada.",
            "Erro ao consultar áudios de acesso.",
            () => _apiService.InvokeJsonAsync("has-audio-access-messages"),
            document =>
            {
                if (document == null)
                    return Task.CompletedTask;

                model.AccessAudioHasNotIdentified = _reader.GetRootBool(document.RootElement, "not_identified");
                model.AccessAudioHasAuthorized = _reader.GetRootBool(document.RootElement, "authorized");
                model.AccessAudioHasNotAuthorized = _reader.GetRootBool(document.RootElement, "not_authorized");
                model.AccessAudioHasUseMask = _reader.GetRootBool(document.RootElement, "use_mask");
                return Task.CompletedTask;
            });

    public async Task UploadAccessAudioAsync(ProductSpecificViewModel model)
    {
        model.ActiveSection = ProductSpecificSections.AccessAudio;

        try
        {
            var base64 = await _fileEncoder.EncodeAsync(model.AccessAudioFile, "Selecione um arquivo WAV para o evento de acesso.");
            var result = await _apiService.InvokeAsync(
                "set-audio-access-message",
                base64,
                $"event={model.AccessAudioEvent}&current={model.AccessAudioCurrent}&total={model.AccessAudioTotal}");

            _resultPresentationService.EnsureSuccess(result, "Erro ao enviar áudio de acesso");
            await _snapshotService.PopulateAccessAudioAsync(model);
            model.ResultMessage = $"Áudio do evento '{model.AccessAudioEvent}' enviado com sucesso.";
            model.ResultStatusType = "success";
            model.ResponseJson = _resultPresentationService.FormatResponseBody(result);
        }
        catch (Exception ex)
        {
            model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
            _logger.LogError(ex, "Erro ao enviar áudio de acesso.");
        }
    }

    public async Task<ProductSpecificDownloadResult?> DownloadAccessAudioAsync(ProductSpecificViewModel model)
    {
        model.ActiveSection = ProductSpecificSections.AccessAudio;

        try
        {
            var result = await _apiService.InvokeAsync("get-audio-access-message", new { @event = model.AccessAudioEvent });
            _resultPresentationService.EnsureSuccess(result, "Erro ao baixar áudio de acesso");
            return new ProductSpecificDownloadResult(result, $"access-{model.AccessAudioEvent}.wav", "audio/wav");
        }
        catch (Exception ex)
        {
            model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
            _logger.LogError(ex, "Erro ao baixar áudio de acesso.");
            return null;
        }
    }

    private Task ExecuteConfigurationAsync(
        ProductSpecificViewModel model,
        string section,
        string failureMessage,
        string successMessage,
        string logMessage,
        object payload,
        Func<Task>? postSuccessAsync = null)
    {
        return ExecuteJsonAsync(
            model,
            section,
            failureMessage,
            successMessage,
            logMessage,
            () => _apiService.InvokeJsonAsync("set-configuration", payload),
            _ => Task.CompletedTask,
            postSuccessAsync);
    }

    private async Task ExecuteAsync(
        ProductSpecificViewModel model,
        string section,
        string failureMessage,
        string successMessage,
        string logMessage,
        Func<Task<OfficialApiInvocationResult>> action,
        Func<OfficialApiInvocationResult, Task> onSuccessAsync)
    {
        model.ActiveSection = section;

        try
        {
            var result = await action();
            _resultPresentationService.EnsureSuccess(result, failureMessage);
            model.ResultMessage = successMessage;
            model.ResultStatusType = "success";
            await onSuccessAsync(result);
        }
        catch (Exception ex)
        {
            model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
            _logger.LogError(ex, logMessage);
        }
    }

    private async Task ExecuteJsonAsync(
        ProductSpecificViewModel model,
        string section,
        string failureMessage,
        string successMessage,
        string logMessage,
        Func<Task<(OfficialApiInvocationResult Result, JsonDocument? Document)>> action,
        Func<JsonDocument?, Task> applyDocumentAsync,
        Func<Task>? postSuccessAsync = null)
    {
        model.ActiveSection = section;

        try
        {
            var (result, document) = await action();
            _resultPresentationService.EnsureSuccess(result, failureMessage);
            await applyDocumentAsync(document);
            if (postSuccessAsync != null)
                await postSuccessAsync();
            if (!string.IsNullOrWhiteSpace(successMessage))
                model.ResultMessage = successMessage;
            model.ResultStatusType = "success";
            model.ResponseJson = _resultPresentationService.FormatJson(result.ResponseBody, document);
        }
        catch (Exception ex)
        {
            model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
            _logger.LogError(ex, logMessage);
        }
    }
}
