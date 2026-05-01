using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Services.Security;
using Integracao.ControlID.PoC.ViewModels.Monitor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class OfficialEventsController : Controller
    {
        private const int EventListLimit = LocalDataQueryLimits.DefaultListLimit;
        private readonly MonitorEventRepository _monitorEventRepository;

        public OfficialEventsController(MonitorEventRepository monitorEventRepository)
        {
            _monitorEventRepository = monitorEventRepository;
        }

        public async Task<IActionResult> Index()
        {
            return View(new MonitorWebhookListViewModel
            {
                Events = (await _monitorEventRepository.GetRecentMonitorEventsAsync(EventListLimit)).Select(ToViewModel).ToList(),
                TotalCount = await _monitorEventRepository.CountMonitorEventsAsync(),
                DisplayLimit = EventListLimit,
                ErrorMessage = TempData["StatusMessage"] as string ?? string.Empty
            });
        }

        [Authorize(Roles = AppSecurityRoles.Administrator)]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var monitorEvent = await _monitorEventRepository.GetMonitorEventByIdAsync(id.Value);
            return monitorEvent == null ? NotFound() : View(ToViewModel(monitorEvent));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppSecurityRoles.Administrator)]
        public async Task<IActionResult> Clear(string confirmationPhrase)
        {
            if (!HighImpactOperationGuard.IsConfirmed(confirmationPhrase, HighImpactOperationGuard.ConfirmClearMonitorEvents))
            {
                TempData["StatusMessage"] = HighImpactOperationGuard.BuildRequiredMessage(HighImpactOperationGuard.ConfirmClearMonitorEvents);
                return RedirectToAction(nameof(Index));
            }

            var removedCount = await _monitorEventRepository.DeleteAllMonitorEventsAsync();

            TempData["StatusMessage"] = $"Eventos oficiais limpos com sucesso. Registros removidos: {removedCount}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppSecurityRoles.Administrator)]
        public async Task<IActionResult> Purge(int retentionDays, string confirmationPhrase)
        {
            if (!HighImpactOperationGuard.IsConfirmed(confirmationPhrase, HighImpactOperationGuard.ConfirmPurgeMonitorEvents))
            {
                TempData["StatusMessage"] = HighImpactOperationGuard.BuildRequiredMessage(HighImpactOperationGuard.ConfirmPurgeMonitorEvents);
                return RedirectToAction(nameof(Index));
            }

            var normalizedRetentionDays = LocalDataQueryLimits.NormalizeRetentionDays(retentionDays);
            var cutoffUtc = DateTime.UtcNow.AddDays(-normalizedRetentionDays);
            var removedCount = await _monitorEventRepository.DeleteMonitorEventsOlderThanAsync(cutoffUtc);

            TempData["StatusMessage"] = $"Expurgo concluido. Retencao: {normalizedRetentionDays} dias. Registros removidos: {removedCount}.";
            return RedirectToAction(nameof(Index));
        }

        private static WebhookEventViewModel ToViewModel(MonitorEventLocal monitorEvent)
        {
            return new WebhookEventViewModel
            {
                EventId = monitorEvent.EventId,
                ReceivedAt = monitorEvent.ReceivedAt,
                RawJson = monitorEvent.RawJson,
                EventType = monitorEvent.EventType,
                DeviceId = monitorEvent.DeviceId,
                UserId = monitorEvent.UserId,
                Payload = monitorEvent.Payload
            };
        }
    }
}
