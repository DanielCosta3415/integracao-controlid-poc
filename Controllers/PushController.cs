using System;
using System.Text;
using System.Threading.Tasks;
using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Services.Callbacks;
using Integracao.ControlID.PoC.Services.Push;
using Integracao.ControlID.PoC.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Controllers
{
    public class PushController : Controller
    {
        private readonly ILogger<PushController> _logger;
        private readonly CallbackSecurityEvaluator _securityEvaluator;
        private readonly CallbackSignatureValidator _signatureValidator;
        private readonly PushCommandWorkflowService _pushWorkflowService;
        private readonly CallbackRequestBodyReader _bodyReader;
        private readonly PushIdempotencyKeyResolver _idempotencyKeyResolver;

        public PushController(
            ILogger<PushController> logger,
            CallbackSecurityEvaluator securityEvaluator,
            CallbackSignatureValidator signatureValidator,
            PushCommandWorkflowService pushWorkflowService,
            CallbackRequestBodyReader bodyReader,
            PushIdempotencyKeyResolver idempotencyKeyResolver)
        {
            _logger = logger;
            _securityEvaluator = securityEvaluator;
            _signatureValidator = signatureValidator;
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
        [AllowAnonymous]
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
                var signatureRejection = ValidateSignature(body);
                if (signatureRejection != null)
                    return signatureRejection;

                var command = await _pushWorkflowService.StoreLegacyEventAsync(
                    body,
                    _idempotencyKeyResolver.Resolve(Request));

                _logger.LogInformation(
                    "Evento Push legado recebido em {Time}. Command {CommandId}. BodyBytes {BodyBytes}.",
                    command.ReceivedAt,
                    command.CommandId,
                    Encoding.UTF8.GetByteCount(body));

                return Ok(new { status = "received", eventId = command.CommandId });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Falha ao processar evento Push legado. BodyBytes {BodyBytes}.",
                    string.IsNullOrEmpty(body) ? 0 : Encoding.UTF8.GetByteCount(body));
                return StatusCode(500, "Erro ao processar evento Push");
            }
        }

        // POST: /Push/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppSecurityRoles.Administrator)]
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

        private IActionResult? ValidateSignature(string body)
        {
            var signatureResult = _signatureValidator.Validate(Request, body);
            if (signatureResult.IsAllowed)
                return null;

            _logger.LogWarning(
                "Blocked legacy push signature for {Path}. Status {StatusCode}. Reason: {Reason}",
                Request.Path,
                signatureResult.StatusCode,
                signatureResult.Message);

            return StatusCode(signatureResult.StatusCode, new { error = signatureResult.Message });
        }
    }
}
