using Integracao.ControlID.PoC.Services.Callbacks;
using Integracao.ControlID.PoC.Services.Database;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    [ApiController]
    public class OfficialCallbacksController : ControllerBase
    {
        private readonly CallbackIngressService _callbackIngressService;
        private readonly PhotoRepository _photoRepository;
        private readonly ILogger<OfficialCallbacksController> _logger;

        public OfficialCallbacksController(
            CallbackIngressService callbackIngressService,
            PhotoRepository photoRepository,
            ILogger<OfficialCallbacksController> logger)
        {
            _callbackIngressService = callbackIngressService;
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
            var photos = await _photoRepository.SearchPhotosAsync(userId: userId);
            var photo = photos.OrderByDescending(item => item.CreatedAt).FirstOrDefault();

            if (photo == null || string.IsNullOrWhiteSpace(photo.Base64Image))
                return NotFound();

            try
            {
                var bytes = Convert.FromBase64String(photo.Base64Image);
                var format = string.IsNullOrWhiteSpace(photo.Format) ? "jpeg" : photo.Format;
                return File(bytes, $"image/{format}", photo.FileName ?? $"user_{userId}.{format}");
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Imagem local inválida para o usuário {UserId}.", userId);
                return NotFound();
            }
        }

    }
}
