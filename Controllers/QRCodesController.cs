using System;
using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.ViewModels.QRCodes;
using Integracao.ControlID.PoC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class QRCodesController : Controller
    {
        private readonly OfficialControlIdApiService _apiService;
        private readonly ILogger<QRCodesController> _logger;

        public QRCodesController(OfficialControlIdApiService apiService, ILogger<QRCodesController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: /QRCodes
        public async Task<IActionResult> Index()
        {
            var model = new QRCodeListViewModel();
            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                model.QRCodes = await LoadQRCodesAsync();
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao consultar QRCodes pela API oficial.");
            }

            return View(model);
        }

        // GET: /QRCodes/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
                return NotFound();

            if (!_apiService.TryGetConnection(out _, out _))
                return NotFound();

            try
            {
                var qrCode = await LoadSingleQRCodeAsync(id.Value);
                if (qrCode != null)
                    return View(qrCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar detalhes do QRCode {QRCodeId}.", id);
            }

            return NotFound();
        }

        // GET: /QRCodes/Create
        public IActionResult Create()
        {
            return View(new QRCodeEditViewModel());
        }

        // POST: /QRCodes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QRCodeEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!_apiService.TryGetConnection(out _, out _))
            {
                ModelState.AddModelError(string.Empty, "É necessário conectar-se e autenticar com um equipamento Control iD.");
                return View(model);
            }

            try
            {
                var result = await _apiService.InvokeAsync("create-objects", new
                {
                    @object = "qrcodes",
                    values = new[]
                    {
                        new
                        {
                            user_id = model.UserId,
                            value = model.Value
                        }
                    }
                });

                EnsureSuccess(result, "Erro ao criar QRCode");

                TempData["StatusMessage"] = "QRCode criado com sucesso.";
                TempData["StatusType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex));
                _logger.LogError(ex, "Erro ao criar QRCode.");
            }

            return View(model);
        }

        // GET: /QRCodes/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
                return NotFound();

            if (!_apiService.TryGetConnection(out _, out _))
                return NotFound();

            try
            {
                var qrCode = await LoadSingleQRCodeAsync(id.Value);
                if (qrCode != null)
                {
                    var editModel = new QRCodeEditViewModel
                    {
                        Id = qrCode.Id,
                        UserId = qrCode.UserId,
                        Value = qrCode.Value
                    };
                    return View(editModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar QRCode {QRCodeId} para edição.", id);
            }

            return NotFound();
        }

        // POST: /QRCodes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, QRCodeEditViewModel model)
        {
            if (model.Id != id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            if (!_apiService.TryGetConnection(out _, out _))
            {
                ModelState.AddModelError(string.Empty, "É necessário conectar-se e autenticar com um equipamento Control iD.");
                return View(model);
            }

            try
            {
                var result = await _apiService.InvokeAsync("modify-objects", new
                {
                    @object = "qrcodes",
                    values = new
                    {
                        user_id = model.UserId,
                        value = model.Value
                    },
                    where = new { qrcodes = new { id } }
                });

                EnsureSuccess(result, "Erro ao atualizar QRCode");

                TempData["StatusMessage"] = "QRCode atualizado com sucesso.";
                TempData["StatusType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex));
                _logger.LogError(ex, "Erro ao atualizar QRCode {QRCodeId}.", id);
            }

            return View(model);
        }

        // GET: /QRCodes/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
                return NotFound();

            if (!_apiService.TryGetConnection(out _, out _))
                return NotFound();

            try
            {
                var qrCode = await LoadSingleQRCodeAsync(id.Value);
                if (qrCode != null)
                {
                    return View(new QRCodeDeleteViewModel
                    {
                        Id = qrCode.Id,
                        UserId = qrCode.UserId,
                        Value = qrCode.Value
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar QRCode {QRCodeId} para exclusão.", id);
            }

            return NotFound();
        }

        // POST: /QRCodes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            if (!_apiService.TryGetConnection(out _, out _))
            {
                TempData["StatusMessage"] = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                TempData["StatusType"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var result = await _apiService.InvokeAsync("destroy-objects", new
                {
                    @object = "qrcodes",
                    where = new { qrcodes = new { id } }
                });

                EnsureSuccess(result, "Erro ao excluir QRCode");

                TempData["StatusMessage"] = "QRCode excluído com sucesso.";
                TempData["StatusType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao excluir QRCode {QRCodeId}.", id);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<QRCodeViewModel>> LoadQRCodesAsync(long? id = null)
        {
            object payload = id.HasValue
                ? new
                {
                    @object = "qrcodes",
                    where = new
                    {
                        qrcodes = new
                        {
                            id = id.Value
                        }
                    }
                }
                : new
                {
                    @object = "qrcodes"
                };

            var (result, document) = await _apiService.InvokeJsonAsync("load-objects", payload);
            EnsureSuccess(result, "Erro ao consultar QRCodes");

            if (document == null || !document.RootElement.TryGetProperty("qrcodes", out var qrcodesElement))
                return [];

            var qrcodes = JsonSerializer.Deserialize<List<QRCodeRecord>>(qrcodesElement.GetRawText(), JsonOptions()) ?? [];

            return qrcodes.Select(qrCode => new QRCodeViewModel
            {
                Id = qrCode.Id,
                UserId = qrCode.UserId,
                Value = qrCode.Value
            }).ToList();
        }

        private async Task<QRCodeViewModel?> LoadSingleQRCodeAsync(long id)
        {
            var qrcodes = await LoadQRCodesAsync(id);
            return qrcodes.FirstOrDefault();
        }

        private static void EnsureSuccess(OfficialApiInvocationResult result, string prefix)
        {
            if (result.Success)
                return;

            throw new InvalidOperationException(BuildErrorMessage(result, prefix));
        }

        private static string BuildErrorMessage(OfficialApiInvocationResult result, string prefix)
        {
            return SecurityTextHelper.BuildApiFailureMessage(result, prefix);
        }

        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private sealed class QRCodeRecord
        {
            public long Id { get; set; }
            public long UserId { get; set; }
            public string Value { get; set; } = string.Empty;
        }
    }
}




