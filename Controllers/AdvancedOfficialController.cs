using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.Files;
using Integracao.ControlID.PoC.Services.Security;
using Integracao.ControlID.PoC.ViewModels.AdvancedOfficial;
using Integracao.ControlID.PoC.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    [Authorize(Roles = AppSecurityRoles.Administrator)]
    public class AdvancedOfficialController : Controller
    {
        private const long MaxFacialImageBytes = 5L * 1024 * 1024;

        private readonly OfficialControlIdApiService _apiService;
        private readonly UploadedFileBase64Encoder _fileEncoder;
        private readonly ILogger<AdvancedOfficialController> _logger;

        public AdvancedOfficialController(
            OfficialControlIdApiService apiService,
            UploadedFileBase64Encoder fileEncoder,
            ILogger<AdvancedOfficialController> logger)
        {
            _apiService = apiService;
            _fileEncoder = fileEncoder;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ExportObjects()
        {
            return View(new ExportObjectsViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportObjects(ExportObjectsViewModel model)
        {
            if (!EnsureConnected(model))
                return View(model);

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await _apiService.InvokeAsync("export-objects", new { @object = model.ObjectName });
                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result, "Erro ao exportar objetos"));

                if (result.ResponseBodyIsBase64 && !string.IsNullOrWhiteSpace(result.ResponseBody))
                {
                    var bytes = Convert.FromBase64String(result.ResponseBody);
                    return File(bytes, GetContentType(result.ResponseContentType, "application/octet-stream"), $"{model.ObjectName}-export.bin");
                }

                model.ResultMessage = "Exportação concluída.";
                model.ResultStatusType = "success";
                model.ResponseJson = result.ResponseBody;
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao exportar objetos do tipo {ObjectName}.", model.ObjectName);
            }

            return View(model);
        }

        public IActionResult NetworkInterlock()
        {
            return View(new NetworkInterlockViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NetworkInterlock(NetworkInterlockViewModel model)
        {
            if (!EnsureConnected(model))
                return View(model);

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("set-network-interlock", new
                {
                    interlock_enabled = model.InterlockEnabled ? 1 : 0,
                    api_bypass_enabled = model.ApiBypassEnabled ? 1 : 0,
                    rex_bypass_enabled = model.RexBypassEnabled ? 1 : 0
                });

                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result, "Erro ao atualizar intertravamento em rede"));

                model.ResultMessage = "Intertravamento em rede atualizado com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao atualizar intertravamento em rede.");
            }

            return View(model);
        }

        public IActionResult CameraCapture()
        {
            return View(new CameraCaptureViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CameraCapture(CameraCaptureViewModel model)
        {
            if (!EnsureConnected(model))
                return View(model);

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await _apiService.InvokeAsync("save-screenshot", new
                {
                    frame_type = model.FrameType,
                    camera = model.Camera
                });

                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result, "Erro ao capturar imagem da câmera"));

                model.ResultMessage = "Imagem capturada com sucesso.";
                model.ResultStatusType = "success";

                if (result.ResponseBodyIsBase64)
                {
                    model.Base64Image = result.ResponseBody;
                    model.ImageContentType = GetSafeImageContentType(result.ResponseContentType);
                    model.ResponseJson = $"Imagem retornada em {model.ImageContentType}.";
                }
                else
                {
                    model.ResponseJson = result.ResponseBody;
                }
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao capturar imagem da câmera.");
            }

            return View(model);
        }

        public IActionResult FacialEnroll()
        {
            return View(new FacialEnrollViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetImageList(FacialEnrollViewModel model)
        {
            if (!EnsureConnected(model))
                return View("FacialEnroll", model);

            if (!ModelState.IsValid)
                return View("FacialEnroll", model);

            try
            {
                var userIds = ParseUserIds(model.UserIdsCsv);
                var (result, document) = await _apiService.InvokeJsonAsync("user-get-image-list", new { user_ids = userIds });
                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result, "Erro ao obter lista de fotos"));

                model.ResultMessage = "Lista de fotos consultada com sucesso.";
                model.ResultStatusType = "success";
                model.GetListResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao consultar lista de fotos faciais.");
            }

            return View("FacialEnroll", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetImageList(FacialEnrollViewModel model)
        {
            if (!EnsureConnected(model))
                return View("FacialEnroll", model);

            if (!ModelState.IsValid)
                return View("FacialEnroll", model);

            try
            {
                var userIds = ParseUserIds(model.UserIdsCsv);
                if (model.BatchFiles.Count == 0)
                    throw new InvalidOperationException("Selecione ao menos um arquivo para o cadastro em lote.");

                if (userIds.Count != model.BatchFiles.Count)
                    throw new InvalidOperationException("A quantidade de IDs deve ser igual à quantidade de arquivos enviados no lote.");

                var userImages = new List<object>();
                for (var index = 0; index < model.BatchFiles.Count; index++)
                {
                    var file = model.BatchFiles[index];

                    userImages.Add(new
                    {
                        user_id = userIds[index],
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        image = await _fileEncoder.EncodeValidatedAsync(
                            file,
                            "Selecione arquivos JPG ou PNG validos para o cadastro em lote.",
                            MaxFacialImageBytes,
                            UploadedFileValidation.JpegOrPng("Envie apenas arquivos JPG ou PNG validos para este fluxo."))
                    });
                }

                var (result, document) = await _apiService.InvokeJsonAsync("user-set-image-list", new
                {
                    match = model.MatchDuplicates,
                    user_images = userImages
                });

                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result, "Erro ao cadastrar lote de fotos"));

                model.ResultMessage = "Lote de fotos enviado com sucesso.";
                model.ResultStatusType = "success";
                model.SetListResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao cadastrar lote de fotos faciais.");
            }

            return View("FacialEnroll", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestImage(FacialEnrollViewModel model)
        {
            if (!EnsureConnected(model))
                return View("FacialEnroll", model);

            if (model.TestFile == null || model.TestFile.Length == 0)
            {
                model.ErrorMessage = "Selecione um arquivo JPG ou PNG para teste.";
                return View("FacialEnroll", model);
            }

            try
            {
                var base64Image = await _fileEncoder.EncodeValidatedAsync(
                    model.TestFile,
                    "Selecione um arquivo JPG ou PNG para teste.",
                    MaxFacialImageBytes,
                    UploadedFileValidation.JpegOrPng("Envie apenas arquivos JPG ou PNG validos para este fluxo."));

                var (result, document) = await _apiService.InvokeJsonAsync("user-test-image", base64Image);
                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result, "Erro ao testar foto facial"));

                model.ResultMessage = "Teste facial executado com sucesso.";
                model.ResultStatusType = "success";
                model.TestResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao testar imagem facial.");
            }

            return View("FacialEnroll", model);
        }

        public IActionResult RemoteLedControl()
        {
            return View(new RemoteLedControlViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoteLedControl(RemoteLedControlViewModel model)
        {
            if (!EnsureConnected(model))
                return View(model);

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("remote-led-control", new
                {
                    sec_box = new
                    {
                        color = model.Color,
                        @event = model.Event
                    }
                });

                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result, "Erro ao controlar LED remotamente"));

                model.ResultMessage = "Comando de LED enviado com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao controlar LED remotamente.");
            }

            return View(model);
        }

        private bool EnsureConnected(object model)
        {
            if (_apiService.TryGetConnection(out _, out _))
                return true;

            var message = "É necessário conectar-se e autenticar com um equipamento Control iD.";
            switch (model)
            {
                case ExportObjectsViewModel exportModel:
                    exportModel.ErrorMessage = message;
                    break;
                case NetworkInterlockViewModel interlockModel:
                    interlockModel.ErrorMessage = message;
                    break;
                case CameraCaptureViewModel cameraModel:
                    cameraModel.ErrorMessage = message;
                    break;
                case FacialEnrollViewModel facialModel:
                    facialModel.ErrorMessage = message;
                    break;
                case RemoteLedControlViewModel ledModel:
                    ledModel.ErrorMessage = message;
                    break;
            }

            return false;
        }

        private static List<long> ParseUserIds(string userIdsCsv)
        {
            var values = userIdsCsv
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(value => long.TryParse(value, out var parsed) ? parsed : 0)
                .Where(value => value > 0)
                .Distinct()
                .ToList();

            if (values.Count == 0)
                throw new InvalidOperationException("Informe ao menos um ID de usuário válido.");

            return values;
        }

        private static string BuildErrorMessage(OfficialApiInvocationResult result, string prefix)
        {
            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                return SecurityTextHelper.BuildApiFailureMessage(result, prefix);

            if (!string.IsNullOrWhiteSpace(result.ResponseBody) && !result.ResponseBodyIsBase64)
                return SecurityTextHelper.BuildApiFailureMessage(result, prefix);

            return SecurityTextHelper.BuildApiFailureMessage(result, prefix);
        }

        private static string FormatJson(string rawJson, JsonDocument? document)
        {
            if (document == null)
                return rawJson;

            return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        private static string GetContentType(string contentType, string fallback)
        {
            return string.IsNullOrWhiteSpace(contentType) ? fallback : contentType;
        }

        private static string GetSafeImageContentType(string contentType)
        {
            if (contentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase) ||
                contentType.Equals("image/png", StringComparison.OrdinalIgnoreCase) ||
                contentType.Equals("image/bmp", StringComparison.OrdinalIgnoreCase) ||
                contentType.Equals("image/gif", StringComparison.OrdinalIgnoreCase))
            {
                return contentType.ToLowerInvariant();
            }

            return "image/png";
        }

    }
}




