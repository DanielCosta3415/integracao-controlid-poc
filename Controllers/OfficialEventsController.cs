using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.ViewModels.Monitor;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class OfficialEventsController : Controller
    {
        private readonly MonitorEventRepository _monitorEventRepository;

        public OfficialEventsController(MonitorEventRepository monitorEventRepository)
        {
            _monitorEventRepository = monitorEventRepository;
        }

        public async Task<IActionResult> Index()
        {
            return View(new MonitorWebhookListViewModel
            {
                Events = (await _monitorEventRepository.GetAllMonitorEventsAsync()).Select(ToViewModel).ToList(),
                ErrorMessage = TempData["StatusMessage"] as string ?? string.Empty
            });
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var monitorEvent = await _monitorEventRepository.GetMonitorEventByIdAsync(id.Value);
            return monitorEvent == null ? NotFound() : View(ToViewModel(monitorEvent));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            var events = await _monitorEventRepository.GetAllMonitorEventsAsync();
            foreach (var item in events)
                await _monitorEventRepository.DeleteMonitorEventAsync(item.EventId);

            TempData["StatusMessage"] = "Eventos oficiais limpos com sucesso.";
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
