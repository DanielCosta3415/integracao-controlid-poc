using System;
using System.Threading.Tasks;
using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Services.Callbacks;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class MonitorWebhookController : Controller
    {
        private readonly CallbackIngressService _callbackIngressService;
        private readonly MonitorEventRepository _monitorEventRepository;

        public MonitorWebhookController(
            CallbackIngressService callbackIngressService,
            MonitorEventRepository monitorEventRepository)
        {
            _callbackIngressService = callbackIngressService;
            _monitorEventRepository = monitorEventRepository;
        }

        // GET: /MonitorWebhook
        public IActionResult Index()
        {
            return RedirectToAction(nameof(OfficialEventsController.Index), "OfficialEvents");
        }

        // GET: /MonitorWebhook/Details/{guid}
        public IActionResult Details(Guid? id)
        {
            return id == null
                ? NotFound()
                : RedirectToAction(nameof(OfficialEventsController.Details), "OfficialEvents", new { id });
        }

        // POST: /MonitorWebhook/Receive
        [HttpPost]
        [Route("MonitorWebhook/Receive")]
        [AllowAnonymous]
        public async Task<IActionResult> Receive(CancellationToken cancellationToken)
        {
            var result = await _callbackIngressService.PersistAsync(HttpContext, "legacy-webhook", cancellationToken);
            if (!result.Accepted)
                return StatusCode(result.StatusCode, result.Message);

            return Ok(new { status = "received", eventId = result.EventId });
        }

        // POST: /MonitorWebhook/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppSecurityRoles.Administrator)]
        public async Task<IActionResult> Clear(string confirmationPhrase)
        {
            if (!HighImpactOperationGuard.IsConfirmed(confirmationPhrase, HighImpactOperationGuard.ConfirmClearMonitorEvents))
            {
                TempData["StatusMessage"] = HighImpactOperationGuard.BuildRequiredMessage(HighImpactOperationGuard.ConfirmClearMonitorEvents);
                return RedirectToAction(nameof(OfficialEventsController.Index), "OfficialEvents");
            }

            var events = await _monitorEventRepository.GetAllMonitorEventsAsync();
            foreach (var item in events)
                await _monitorEventRepository.DeleteMonitorEventAsync(item.EventId);

            TempData["StatusMessage"] = "Monitor events cleared successfully.";
            return RedirectToAction(nameof(OfficialEventsController.Index), "OfficialEvents");
        }
    }
}
