using System;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.ViewModels.Logo;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class LogoController : Controller
    {
        private readonly OfficialControlIdApiService _officialApi;
        private readonly ILogger<LogoController> _logger;

        public LogoController(OfficialControlIdApiService officialApi, ILogger<LogoController> logger)
        {
            _officialApi = officialApi;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new LogoListViewModel();

            if (!_officialApi.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                model.Logos = await LoadLogosAsync();
            }
            catch (Exception ex)
            {
                model.ErrorMessage = $"Erro ao consultar os slots de logo: {ex.Message}";
                _logger.LogError(ex, "Erro ao consultar os slots de logo.");
            }

            return View(model);
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
                return NotFound();

            var logo = await GetLogoBySlotAsync(id.Value);
            if (logo == null || !logo.HasImage)
                return NotFound();

            return View(logo);
        }

        public IActionResult Upload()
        {
            return View(new LogoUploadViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(LogoUploadViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!_officialApi.TryGetConnection(out _, out _))
            {
                ModelState.AddModelError(string.Empty, "É necessário conectar-se e autenticar com um equipamento Control iD.");
                return View(model);
            }

            if (model.LogoFile == null || model.LogoFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Selecione um arquivo de logo válido.");
                return View(model);
            }

            var fileName = model.LogoFile.FileName ?? string.Empty;
            var isPng = model.LogoFile.ContentType.Contains("png", StringComparison.OrdinalIgnoreCase) ||
                        fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase);

            if (!isPng || model.LogoFile.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError(string.Empty, "Envie um arquivo PNG de até 2MB.");
                return View(model);
            }

            try
            {
                await using var stream = new MemoryStream();
                await model.LogoFile.CopyToAsync(stream);

                var result = await _officialApi.InvokeAsync(
                    "logo-change",
                    Convert.ToBase64String(stream.ToArray()),
                    $"id={model.Id}");

                if (result.Success)
                {
                    TempData["StatusMessage"] = $"Logo do slot {model.Id} enviado com sucesso!";
                    TempData["StatusType"] = "success";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, BuildErrorMessage("Erro ao enviar logo", result));
            }
            catch (IOException ioex)
            {
                ModelState.AddModelError(string.Empty, "Erro de leitura do arquivo selecionado. Tente novamente.");
                _logger.LogError(ioex, "Erro de IO ao enviar o logo do slot {SlotId}.", model.Id);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro ao enviar logo: {ex.Message}");
                _logger.LogError(ex, "Erro ao enviar o logo do slot {SlotId}.", model.Id);
            }

            return View(model);
        }

        public async Task<IActionResult> Download(long? id)
        {
            if (id == null)
                return NotFound();

            var result = await _officialApi.InvokeAsync("logo-get", additionalQuery: $"id={id.Value}");
            if (!result.Success || !result.ResponseBodyIsBase64 || string.IsNullOrWhiteSpace(result.ResponseBody))
                return NotFound();

            try
            {
                var imageBytes = Convert.FromBase64String(result.ResponseBody);
                return File(imageBytes, GetContentType(result.ResponseContentType), $"logo_slot_{id.Value}.{GetFileExtension(result.ResponseContentType)}");
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Resposta inválida ao baixar o logo do slot {SlotId}.", id.Value);
                return NotFound();
            }
        }

        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
                return NotFound();

            var logo = await GetLogoBySlotAsync(id.Value);
            if (logo == null || !logo.HasImage)
                return NotFound();

            return View(new LogoDeleteViewModel
            {
                Id = logo.Id,
                FileName = logo.FileName
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
                var result = await _officialApi.InvokeAsync("logo-destroy", additionalQuery: $"id={id}");

                TempData["StatusMessage"] = result.Success
                    ? $"Logo do slot {id} excluído com sucesso!"
                    : BuildErrorMessage("Erro ao excluir logo", result);
                TempData["StatusType"] = result.Success ? "success" : "danger";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = $"Erro ao excluir logo: {ex.Message}";
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao excluir o logo do slot {SlotId}.", id);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<LogoViewModel>> LoadLogosAsync()
        {
            var logos = new List<LogoViewModel>();

            for (var slotId = 1; slotId <= 8; slotId++)
            {
                var logo = await GetLogoBySlotAsync(slotId);
                logos.Add(logo ?? CreateEmptyLogo(slotId));
            }

            return logos;
        }

        private async Task<LogoViewModel?> GetLogoBySlotAsync(long slotId)
        {
            try
            {
                var result = await _officialApi.InvokeAsync("logo-get", additionalQuery: $"id={slotId}");
                if (!result.Success || !result.ResponseBodyIsBase64 || string.IsNullOrWhiteSpace(result.ResponseBody))
                    return null;

                return new LogoViewModel
                {
                    Id = slotId,
                    Base64Image = result.ResponseBody,
                    ContentType = GetContentType(result.ResponseContentType),
                    FileName = $"logo_slot_{slotId}.{GetFileExtension(result.ResponseContentType)}",
                    Format = GetFileExtension(result.ResponseContentType),
                    HasImage = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Não foi possível obter o logo do slot {SlotId}.", slotId);
                return null;
            }
        }

        private static LogoViewModel CreateEmptyLogo(long slotId)
        {
            return new LogoViewModel
            {
                Id = slotId,
                FileName = $"logo_slot_{slotId}.png",
                Format = "png",
                ContentType = "image/png",
                HasImage = false
            };
        }

        private static string BuildErrorMessage(string prefix, OfficialApiInvocationResult result)
        {
            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                return $"{prefix}: {result.ErrorMessage}";

            if (!string.IsNullOrWhiteSpace(result.ResponseBody) && !result.ResponseBodyIsBase64)
                return $"{prefix}: {result.ResponseBody}";

            return $"{prefix} (status: {result.StatusCode}).";
        }

        private static string GetFileExtension(string? contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                return "png";

            if (contentType.Contains("jpeg", StringComparison.OrdinalIgnoreCase) ||
                contentType.Contains("jpg", StringComparison.OrdinalIgnoreCase))
                return "jpg";

            return contentType.Contains("bmp", StringComparison.OrdinalIgnoreCase) ? "bmp" : "png";
        }

        private static string GetContentType(string? contentType)
        {
            return string.IsNullOrWhiteSpace(contentType) ? "image/png" : contentType;
        }
    }
}
