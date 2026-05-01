using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Services.Callbacks;
using Integracao.ControlID.PoC.Services.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Integracao.ControlID.PoC.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [EnableRateLimiting("CallbackIngress")]
    public class OfficialCallbacksController : ControllerBase
    {
        private readonly CallbackIngressService _callbackIngressService;
        private readonly CallbackSecurityEvaluator _securityEvaluator;
        private readonly CallbackSignatureValidator _signatureValidator;
        private readonly PhotoRepository _photoRepository;
        private readonly ILogger<OfficialCallbacksController> _logger;

        public OfficialCallbacksController(
            CallbackIngressService callbackIngressService,
            CallbackSecurityEvaluator securityEvaluator,
            CallbackSignatureValidator signatureValidator,
            PhotoRepository photoRepository,
            ILogger<OfficialCallbacksController> logger)
        {
            _callbackIngressService = callbackIngressService;
            _securityEvaluator = securityEvaluator;
            _signatureValidator = signatureValidator;
            _photoRepository = photoRepository;
            _logger = logger;
        }

        [HttpPost("/new_biometric_image.fcgi")]
        [HttpPost("/new_biometric_template.fcgi")]
        [HttpPost("/new_card.fcgi")]
        [HttpPost("/new_qrcode.fcgi")]
        [HttpPost("/new_uhf_tag.fcgi")]
        [HttpPost("/new_user_id_and_password.fcgi")]
        [HttpPost("/new_user_identified.fcgi")]
        public async Task<IActionResult> ReceiveIdentificationEvent(CancellationToken cancellationToken)
        {
            var result = await _callbackIngressService.PersistAsync(HttpContext, "identification", cancellationToken);
            if (!result.Accepted)
                return StatusCode(result.StatusCode, result.Message);

            return Ok(new { result = new { @event = 14 } });
        }

        [HttpPost("/new_rex_log.fcgi")]
        [HttpPost("/device_is_alive.fcgi")]
        [HttpPost("/card_create.fcgi")]
        [HttpPost("/fingerprint_create.fcgi")]
        [HttpPost("/template_create.fcgi")]
        [HttpPost("/face_create.fcgi")]
        [HttpPost("/pin_create.fcgi")]
        [HttpPost("/password_create.fcgi")]
        public async Task<IActionResult> ReceiveAcknowledgedEvent(CancellationToken cancellationToken)
        {
            var result = await _callbackIngressService.PersistAsync(HttpContext, "callback", cancellationToken);
            if (!result.Accepted)
                return StatusCode(result.StatusCode, result.Message);

            return Ok();
        }

        [HttpPost("/api/notifications/{topic}")]
        public async Task<IActionResult> ReceiveMonitorNotification(string topic, CancellationToken cancellationToken)
        {
            var result = await _callbackIngressService.PersistAsync(HttpContext, $"monitor:{topic}", cancellationToken);
            if (!result.Accepted)
                return StatusCode(result.StatusCode, result.Message);

            return Ok();
        }

        [HttpGet("/user_get_image.fcgi")]
        public async Task<IActionResult> GetUserImage([FromQuery(Name = "user_id")] long userId)
        {
            var ingressRejection = ValidateIngressRequest();
            if (ingressRejection != null)
                return ingressRejection;

            var signatureRejection = ValidateSignature(string.Empty);
            if (signatureRejection != null)
                return signatureRejection;

            var photos = await _photoRepository.SearchPhotosAsync(userId: userId);
            var photo = photos.OrderByDescending(item => item.CreatedAt).FirstOrDefault();

            if (photo == null || string.IsNullOrWhiteSpace(photo.Base64Image))
                return NotFound();

            try
            {
                var bytes = Convert.FromBase64String(photo.Base64Image);
                var format = NormalizeImageFormat(photo.Format);
                return File(bytes, $"image/{format}", BuildPhotoFileName(format));
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Imagem local inválida para o usuário {UserRef}.", PrivacyLogHelper.PseudonymizeIdentifier(userId));
                return NotFound();
            }
        }

        private IActionResult? ValidateIngressRequest()
        {
            var securityResult = _securityEvaluator.Evaluate(HttpContext);
            if (securityResult.IsAllowed)
                return null;

            _logger.LogWarning(
                "Blocked callback file request for {Path}. Status {StatusCode}. Reason: {Reason}",
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
                "Blocked callback file signature for {Path}. Status {StatusCode}. Reason: {Reason}",
                Request.Path,
                signatureResult.StatusCode,
                signatureResult.Message);

            return StatusCode(signatureResult.StatusCode, new { error = signatureResult.Message });
        }

        private static string NormalizeImageFormat(string? format)
        {
            var normalizedFormat = string.IsNullOrWhiteSpace(format)
                ? "jpeg"
                : format.Trim().TrimStart('.').ToLowerInvariant();

            return normalizedFormat switch
            {
                "png" => "png",
                "jpg" => "jpeg",
                "jpeg" => "jpeg",
                "bmp" => "bmp",
                "gif" => "gif",
                _ => "jpeg"
            };
        }

        private static string BuildPhotoFileName(string format)
        {
            var normalizedFormat = NormalizeImageFormat(format);
            return $"user-photo.{normalizedFormat}";
        }

    }
}
