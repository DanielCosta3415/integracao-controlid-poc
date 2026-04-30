using System;
using System.Threading.Tasks;
using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Services.Callbacks;
using Integracao.ControlID.PoC.Services.Push;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Controllers
{
    public class PushController : Controller
    {
        private readonly ILogger<PushController> _logger;
        private readonly CallbackSecurityEvaluator _securityEvaluator;
        private readonly PushCommandWorkflowService _pushWorkflowService;
        private readonly CallbackRequestBodyReader _bodyReader;
        private readonly PushIdempotencyKeyResolver _idempotencyKeyResolver;

        public PushController(
            ILogger<PushController> logger,
            CallbackSecurityEvaluator securityEvaluator,
            PushCommandWorkflowService pushWorkflowService,
            CallbackRequestBodyReader bodyReader,
            PushIdempotencyKeyResolver idempotencyKeyResolver)
        {
            _logger = logger;
            _securityEvaluator = securityEvaluator;
            _pushWorkflowService = pushWorkflowService;
            _bodyReader = bodyReader;
            _idempotencyKeyResolver = idempotencyKeyResolver;
        }

        // GET: /Push
        public IActionResult Index()
        {
            return RedirectToAction(nameof(PushCenterController.Index), "PushCenter");
        }

        // GET: /Push/Details/{guid}
        public IActionResult Details(Guid? id)
        {
            return id == null
                ? NotFound()
                : RedirectToAction(nameof(PushCenterController.Details), "PushCenter", new { id });
        }

        // POST: /Push/Receive
        [HttpPost]
        [Route("Push/Receive")]
        [EnableRateLimiting("CallbackIngress")]
        public async Task<IActionResult> Receive()
        {
            string? body = null;
            try
            {
                var ingressRejection = ValidateIngressRequest();
                if (ingressRejection != null)
                    return ingressRejection;

                var bodyResult = await _bodyReader.ReadAsync(Request);
                if (!bodyResult.IsSuccessful)
                    return StatusCode(bodyResult.StatusCode, bodyResult.Message);

                body = bodyResult.Body;
                var command = await _pushWorkflowService.StoreLegacyEventAsync(
                    body,
                    _idempotencyKeyResolver.Resolve(Request));

                _logger.LogInformation("Evento Push legado recebido em {Time}: {Summary}",
                    command.ReceivedAt, Truncate(body, 500));

                return Ok(new { status = "received", eventId = command.CommandId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao processar evento Push legado. Body (trunc): {Body}", Truncate(body, 500));
                return StatusCode(500, "Erro ao processar evento Push");
            }
        }

        // POST: /Push/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear(string confirmationPhrase)
        {
            if (!HighImpactOperationGuard.IsConfirmed(confirmationPhrase, HighImpactOperationGuard.ConfirmClearPushQueue))
            {
                TempData["StatusMessage"] = HighImpactOperationGuard.BuildRequiredMessage(HighImpactOperationGuard.ConfirmClearPushQueue);
                TempData["StatusType"] = "warning";
                return RedirectToAction(nameof(PushCenterController.Index), "PushCenter");
            }

            await _pushWorkflowService.ClearAsync();
            TempData["StatusMessage"] = "Eventos Push limpos com sucesso.";
            TempData["StatusType"] = "success";
            return RedirectToAction(nameof(PushCenterController.Index), "PushCenter");
        }

        private IActionResult? ValidateIngressRequest()
        {
            var securityResult = _securityEvaluator.Evaluate(HttpContext);
            if (securityResult.IsAllowed)
                return null;

            _logger.LogWarning(
                "Blocked legacy push ingress request for {Path}. Status {StatusCode}. Reason: {Reason}",
                Request.Path,
                securityResult.StatusCode,
                securityResult.Message);

            return StatusCode(securityResult.StatusCode, new { error = securityResult.Message });
        }

        /// <summary>
        /// Trunca texto longo para logs.
        /// </summary>
        private static string Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }
    }
}
