using System;
using System.Buffers;
using System.Globalization;
using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.Files;
using Integracao.ControlID.PoC.Services.Security;
using Integracao.ControlID.PoC.ViewModels.Media;
using Integracao.ControlID.PoC.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    [Authorize(Roles = AppSecurityRoles.Administrator)]
    public class MediaController : Controller
    {
        private const long MaxImageUploadBytes = 2L * 1024 * 1024;
        private const long MaxAdVideoBytes = 256L * 1024 * 1024;

        private readonly OfficialControlIdApiService _officialApi;
        private readonly UploadedFileBase64Encoder _fileEncoder;
        private readonly ILogger<MediaController> _logger;

        public MediaController(
            OfficialControlIdApiService officialApi,
            UploadedFileBase64Encoder fileEncoder,
            ILogger<MediaController> logger)
        {
            _officialApi = officialApi;
            _fileEncoder = fileEncoder;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PhotoListViewModel();

            if (!_officialApi.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                model.Photos = await LoadPhotosAsync();
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("Erro ao consultar fotos", ex);
                _logger.LogError(ex, "Erro ao consultar fotos dos usuários.");
            }

            return View(model);
        }

        public async Task<IActionResult> AdMode()
        {
            var model = new AdVideoManageViewModel();

            if (!_officialApi.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            await PopulateAdModeStateAsync(model);
            return View(model);
        }

        public IActionResult Details(long? id)
        {
            if (id == null)
                return NotFound();

            return View(CreatePhotoPlaceholder(id.Value));
        }

        public IActionResult Upload()
        {
            return View(new PhotoUploadViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(PhotoUploadViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!_officialApi.TryGetConnection(out _, out _))
            {
                ModelState.AddModelError(string.Empty, "É necessário conectar-se e autenticar com um equipamento Control iD.");
                return View(model);
            }

            if (model.PhotoFile == null || model.PhotoFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Selecione um arquivo de foto válido.");
                return View(model);
            }

            if (model.PhotoFile.Length > MaxImageUploadBytes)
            {
                ModelState.AddModelError(string.Empty, "Apenas arquivos de imagem até 2MB são permitidos.");
                return View(model);
            }

            try
            {
                var payload = await BuildPhotoMultipartPayloadAsync(model);
                var result = await _officialApi.InvokeAsync("user-set-image", payload);

                if (result.Success)
                {
                    TempData["StatusMessage"] = "Foto enviada com sucesso!";
                    TempData["StatusType"] = "success";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, BuildErrorMessage("Erro ao enviar foto", result));
            }
            catch (IOException ioex)
            {
                ModelState.AddModelError(string.Empty, "Erro de leitura do arquivo selecionado. Tente novamente.");
                _logger.LogError(ioex, "Erro de IO ao enviar foto do usuário {UserRef}.", PrivacyLogHelper.PseudonymizeIdentifier(model.UserId));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, SecurityTextHelper.BuildSafeUserMessage("Erro ao enviar foto", ex));
                _logger.LogError(ex, "Erro ao enviar foto do usuário {UserRef}.", PrivacyLogHelper.PseudonymizeIdentifier(model.UserId));
            }

            return View(model);
        }

        public async Task<IActionResult> Download(long? id)
        {
            if (id == null)
                return NotFound();

            var result = await _officialApi.InvokeToStreamAsync(
                "user-get-image",
                Response.Body,
                (metadata, _) => OfficialApiDownloadResponse.ApplyAsync(
                    Response,
                    metadata,
                    BuildFileName(metadata.ContentType, "user-photo"),
                    "image/jpeg"),
                additionalQuery: $"user_id={id.Value}",
                cancellationToken: HttpContext.RequestAborted);
            if (!result.Success)
                return NotFound();

            return new EmptyResult();
        }

        public IActionResult Delete(long? id)
        {
            if (id == null)
                return NotFound();

            return View(new PhotoDeleteViewModel
            {
                Id = id.Value,
                UserId = id.Value,
                FileName = "user-photo.jpg"
            });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (!_officialApi.TryGetConnection(out _, out _))
            {
                TempData["StatusMessage"] = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                TempData["StatusType"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var result = await _officialApi.InvokeAsync("user-destroy-image", new { user_id = id });

                TempData["StatusMessage"] = result.Success
                    ? "Foto excluída com sucesso!"
                    : BuildErrorMessage("Erro ao excluir foto", result);
                TempData["StatusType"] = result.Success ? "success" : "danger";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("Erro ao excluir foto", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao excluir foto do usuário {UserRef}.", PrivacyLogHelper.PseudonymizeIdentifier(id));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAdVideo(AdVideoManageViewModel model)
        {
            if (!_officialApi.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View("AdMode", model);
            }

            if (!ModelState.IsValid)
            {
                await PopulateAdModeStateAsync(model);
                return View("AdMode", model);
            }

            if (model.VideoFile == null || model.VideoFile.Length == 0)
            {
                model.ResultMessage = "Selecione um arquivo MP4 válido para envio.";
                model.ResultStatusType = "danger";
                await PopulateAdModeStateAsync(model);
                return View("AdMode", model);
            }

            var fileName = model.VideoFile.FileName ?? string.Empty;
            if (!fileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
            {
                model.ResultMessage = "A API oficial de propaganda espera um arquivo MP4.";
                model.ResultStatusType = "danger";
                await PopulateAdModeStateAsync(model);
                return View("AdMode", model);
            }

            try
            {
                if (model.VideoFile.Length > MaxAdVideoBytes)
                    throw new InvalidOperationException("Envie um arquivo MP4 válido de até 256 MB.");

                var chunkSize = model.ChunkSizeKb * 1024;
                var totalChunks = checked((int)((model.VideoFile.Length + chunkSize - 1) / chunkSize));
                var validation = UploadedFileValidation.Mp4("Envie um arquivo MP4 válido de até 256 MB.");
                var buffer = ArrayPool<byte>.Shared.Rent(chunkSize);

                OfficialApiInvocationResult? lastResult = null;
                try
                {
                    await using var stream = model.VideoFile.OpenReadStream();
                    for (var chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
                    {
                        var length = await ReadChunkAsync(
                            stream,
                            buffer.AsMemory(0, chunkSize),
                            HttpContext.RequestAborted);
                        if (length == 0)
                            throw new InvalidDataException("O arquivo MP4 terminou antes do tamanho declarado.");

                        if (chunkIndex == 0)
                            validation.Validate(model.VideoFile, buffer.AsSpan(0, length));

                        lastResult = await _officialApi.InvokeBinaryAsync(
                            "send-video",
                            buffer.AsMemory(0, length),
                            $"current={chunkIndex + 1}&total={totalChunks}",
                            HttpContext.RequestAborted);

                        if (!lastResult.Success)
                            throw new InvalidOperationException(BuildErrorMessage("Erro ao enviar bloco do vídeo", lastResult));
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
                }

                if (model.EnableAfterUpload)
                {
                    var enableResult = await _officialApi.InvokeAsync("set-custom-video", new
                    {
                        custom_video_enabled = "1"
                    });

                    if (!enableResult.Success)
                        throw new InvalidOperationException(BuildErrorMessage("Vídeo enviado, mas falhou ao ativar o modo propaganda", enableResult));

                    lastResult = enableResult;
                }

                model.ResultMessage = model.EnableAfterUpload
                    ? "Vídeo enviado e modo propaganda ativado com sucesso."
                    : "Vídeo enviado com sucesso.";
                model.ResultStatusType = "success";
                model.LastResponseJson = string.IsNullOrWhiteSpace(lastResult?.ResponseBody)
                    ? "Operação concluída sem corpo de resposta."
                    : lastResult.ResponseBody;
            }
            catch (Exception ex)
            {
                model.ResultMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                model.ResultStatusType = "danger";
                _logger.LogError(ex, "Erro ao enviar vídeo de propaganda.");
            }

            await PopulateAdModeStateAsync(model);
            return View("AdMode", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdVideo(bool enabled)
        {
            var model = new AdVideoManageViewModel();

            if (!_officialApi.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View("AdMode", model);
            }

            try
            {
                var result = await _officialApi.InvokeAsync("set-custom-video", new
                {
                    custom_video_enabled = enabled ? "1" : "0"
                });

                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage("Erro ao alterar o modo propaganda", result));

                model.ResultMessage = enabled
                    ? "Modo propaganda habilitado com sucesso."
                    : "Modo propaganda desabilitado com sucesso.";
                model.ResultStatusType = "success";
                model.LastResponseJson = string.IsNullOrWhiteSpace(result.ResponseBody)
                    ? "Operação concluída sem corpo de resposta."
                    : result.ResponseBody;
            }
            catch (Exception ex)
            {
                model.ResultMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                model.ResultStatusType = "danger";
                _logger.LogError(ex, "Erro ao alternar modo propaganda.");
            }

            await PopulateAdModeStateAsync(model);
            return View("AdMode", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAdVideo()
        {
            var model = new AdVideoManageViewModel();

            if (!_officialApi.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View("AdMode", model);
            }

            try
            {
                var result = await _officialApi.InvokeAsync("remove-custom-video");
                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage("Erro ao remover vídeo de propaganda", result));

                model.ResultMessage = "Vídeo de propaganda removido com sucesso.";
                model.ResultStatusType = "success";
                model.LastResponseJson = string.IsNullOrWhiteSpace(result.ResponseBody)
                    ? "Operação concluída sem corpo de resposta."
                    : result.ResponseBody;
            }
            catch (Exception ex)
            {
                model.ResultMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                model.ResultStatusType = "danger";
                _logger.LogError(ex, "Erro ao remover vídeo de propaganda.");
            }

            await PopulateAdModeStateAsync(model);
            return View("AdMode", model);
        }

        private async Task<List<PhotoViewModel>> LoadPhotosAsync()
        {
            var (result, document) = await _officialApi.InvokeJsonAsync("user-list-images");
            if (!result.Success)
                throw new InvalidOperationException(BuildErrorMessage("Erro ao listar fotos dos usuários", result));

            if (document == null)
                return [];

            return ExtractUserIds(document.RootElement)
                .OrderBy(static value => value)
                .Select(CreatePhotoPlaceholder)
                .ToList();
        }

        private async Task PopulateAdModeStateAsync(AdVideoManageViewModel model)
        {
            try
            {
                var payload = "{\"video_player\":{}}";
                var (result, document) = await _officialApi.InvokeJsonAsync("get-configuration", payload);

                if (!result.Success)
                {
                    model.ErrorMessage = BuildErrorMessage("Não foi possível consultar o estado do modo propaganda", result);
                    return;
                }

                model.StatusJson = FormatJson(result.ResponseBody, document);
                model.CustomVideoEnabled = TryExtractCustomVideoEnabled(document?.RootElement);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("Erro ao consultar estado do modo propaganda", ex);
                _logger.LogWarning(ex, "Erro ao consultar configuração do vídeo personalizado.");
            }
        }

        private async Task<string> BuildPhotoMultipartPayloadAsync(PhotoUploadViewModel model)
        {
            var validation = UploadedFileValidation.JpegOrPng("Apenas arquivos JPG ou PNG validos de ate 2 MB sao permitidos.");
            var base64Photo = await _fileEncoder.EncodeValidatedAsync(
                model.PhotoFile,
                "Selecione um arquivo de foto para envio.",
                MaxImageUploadBytes,
                validation);
            var safeFileName = BuildGenericPhotoFileName(model.PhotoFile.FileName);

            var payload = new
            {
                fields = new Dictionary<string, string>
                {
                    ["user_id"] = model.UserId.ToString(CultureInfo.InvariantCulture)
                },
                files = new[]
                {
                    new
                    {
                        name = "image",
                        fileName = safeFileName,
                        contentType = GetUploadImageContentType(safeFileName),
                        base64Content = base64Photo
                    }
                }
            };

            return JsonSerializer.Serialize(payload);
        }

        private static IReadOnlyCollection<long> ExtractUserIds(JsonElement root)
        {
            var userIds = new HashSet<long>();
            CollectUserIds(root, null, userIds);
            return userIds;
        }

        private static void CollectUserIds(JsonElement element, string? context, ISet<long> userIds)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    if (TryGetInt64(element, "user_id", out var userId))
                        userIds.Add(userId);
                    else if (ContextSuggestsUserIds(context) && TryGetInt64(element, "id", out var objectId))
                        userIds.Add(objectId);

                    foreach (var property in element.EnumerateObject())
                        CollectUserIds(property.Value, property.Name, userIds);
                    break;

                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray())
                        CollectUserIds(item, context, userIds);
                    break;

                case JsonValueKind.Number:
                case JsonValueKind.String:
                    if (ContextSuggestsUserIds(context) && TryGetInt64(element, out var rawId))
                        userIds.Add(rawId);
                    break;
            }
        }

        private static bool ContextSuggestsUserIds(string? context)
        {
            if (string.IsNullOrWhiteSpace(context))
                return false;

            return context.Contains("user", StringComparison.OrdinalIgnoreCase) ||
                   context.Contains("image", StringComparison.OrdinalIgnoreCase) ||
                   context.Contains("photo", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryGetInt64(JsonElement element, string propertyName, out long value)
        {
            value = default;
            if (!element.TryGetProperty(propertyName, out var property))
                return false;

            return TryGetInt64(property, out value);
        }

        private static bool TryGetInt64(JsonElement element, out long value)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Number:
                    return element.TryGetInt64(out value);
                case JsonValueKind.String:
                    return long.TryParse(element.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
                default:
                    value = default;
                    return false;
            }
        }

        private static PhotoViewModel CreatePhotoPlaceholder(long userId)
        {
            return new PhotoViewModel
            {
                Id = userId,
                UserId = userId,
                FileName = "user-photo.jpg",
                Format = "jpg",
                ContentType = "image/jpeg",
                HasImage = true
            };
        }

        private static async ValueTask<int> ReadChunkAsync(
            Stream stream,
            Memory<byte> buffer,
            CancellationToken cancellationToken)
        {
            var totalRead = 0;
            while (totalRead < buffer.Length)
            {
                var read = await stream.ReadAsync(buffer[totalRead..], cancellationToken);
                if (read == 0)
                    break;

                totalRead += read;
            }

            return totalRead;
        }

        private static string BuildErrorMessage(string prefix, OfficialApiInvocationResult result)
        {
            return SecurityTextHelper.BuildApiFailureMessage(result, prefix);
        }

        private static string BuildFileName(string? contentType, string prefix)
        {
            return $"{prefix}.{GetFileExtension(contentType, "jpg")}";
        }

        private static string BuildGenericPhotoFileName(string? originalFileName)
        {
            var extension = System.IO.Path.GetExtension(originalFileName);
            return string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase)
                ? "user-photo.png"
                : "user-photo.jpg";
        }

        private static string GetFileExtension(string? contentType, string fallback)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                return fallback;

            if (contentType.Contains("png", StringComparison.OrdinalIgnoreCase))
                return "png";

            if (contentType.Contains("bmp", StringComparison.OrdinalIgnoreCase))
                return "bmp";

            if (contentType.Contains("gif", StringComparison.OrdinalIgnoreCase))
                return "gif";

            return contentType.Contains("jpeg", StringComparison.OrdinalIgnoreCase) ||
                   contentType.Contains("jpg", StringComparison.OrdinalIgnoreCase)
                ? "jpg"
                : fallback;
        }

        private static string GetContentType(string? contentType, string fallback)
        {
            return string.IsNullOrWhiteSpace(contentType) ? fallback : contentType;
        }

        private static string GetUploadImageContentType(string fileName)
        {
            return fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                ? "image/png"
                : "image/jpeg";
        }

        private static bool? TryExtractCustomVideoEnabled(JsonElement? root)
        {
            if (root == null)
                return null;

            if (!root.Value.TryGetProperty("video_player", out var videoPlayer) || videoPlayer.ValueKind != JsonValueKind.Object)
                return null;

            if (!videoPlayer.TryGetProperty("custom_video_enabled", out var enabledProperty))
                return null;

            return enabledProperty.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number => enabledProperty.GetInt32() == 1,
                JsonValueKind.String => enabledProperty.GetString() == "1" ||
                                        enabledProperty.GetString()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true,
                _ => null
            };
        }

        private static string FormatJson(string rawJson, OfficialApiJsonPayload? document)
        {
            if (document == null)
                return rawJson;

            return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
}




