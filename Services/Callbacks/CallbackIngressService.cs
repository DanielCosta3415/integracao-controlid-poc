using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Services.Observability;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Services.Callbacks
{
    public class CallbackIngressService
    {
        private readonly CallbackSecurityEvaluator _securityEvaluator;
        private readonly CallbackSignatureValidator _signatureValidator;
        private readonly CallbackRequestBodyReader _bodyReader;
        private readonly MonitorEventRepository _monitorEventRepository;
        private readonly ILogger<CallbackIngressService> _logger;

        public CallbackIngressService(
            CallbackSecurityEvaluator securityEvaluator,
            CallbackSignatureValidator signatureValidator,
            CallbackRequestBodyReader bodyReader,
            MonitorEventRepository monitorEventRepository,
            ILogger<CallbackIngressService> logger)
        {
            _securityEvaluator = securityEvaluator;
            _signatureValidator = signatureValidator;
            _bodyReader = bodyReader;
            _monitorEventRepository = monitorEventRepository;
            _logger = logger;
        }

        /// <summary>
        /// Valida, le e persiste um callback recebido pelo equipamento para o monitor local da PoC.
        /// </summary>
        /// <param name="httpContext">Contexto HTTP bruto da requisicao recebida.</param>
        /// <param name="eventFamily">Familia funcional usada para agrupar callbacks relacionados.</param>
        /// <param name="cancellationToken">Token opcional para cancelamento da leitura/persistencia.</param>
        /// <returns>Resultado padronizado com status HTTP sugerido e id do evento persistido, quando houver.</returns>
        public async Task<CallbackIngressResult> PersistAsync(
            HttpContext httpContext,
            string eventFamily,
            CancellationToken cancellationToken = default)
        {
            var securityResult = _securityEvaluator.Evaluate(httpContext);
            if (!securityResult.IsAllowed)
            {
                OperationalMetrics.RecordCallbackIngress(
                    eventFamily,
                    httpContext.Request.Path.Value ?? string.Empty,
                    "security_rejected",
                    securityResult.StatusCode);

                _logger.LogWarning(
                    OperationalEventIds.CallbackRejected,
                    "Blocked callback request for {Path}. Status {StatusCode}. Reason: {Reason}",
                    httpContext.Request.Path,
                    securityResult.StatusCode,
                    securityResult.Message);

                return CallbackIngressResult.Rejected(securityResult.StatusCode, securityResult.Message);
            }

            var bodyResult = await _bodyReader.ReadAsync(httpContext.Request, cancellationToken);
            if (!bodyResult.IsSuccessful)
            {
                OperationalMetrics.RecordCallbackIngress(
                    eventFamily,
                    httpContext.Request.Path.Value ?? string.Empty,
                    "body_rejected",
                    bodyResult.StatusCode);

                _logger.LogWarning(
                    OperationalEventIds.CallbackRejected,
                    "Rejected callback request body for {Path}. Status {StatusCode}. Reason: {Reason}",
                    httpContext.Request.Path,
                    bodyResult.StatusCode,
                    bodyResult.Message);

                return CallbackIngressResult.Rejected(bodyResult.StatusCode, bodyResult.Message);
            }

            var signatureResult = _signatureValidator.Validate(httpContext.Request, bodyResult.Body);
            if (!signatureResult.IsAllowed)
            {
                OperationalMetrics.RecordCallbackIngress(
                    eventFamily,
                    httpContext.Request.Path.Value ?? string.Empty,
                    "signature_rejected",
                    signatureResult.StatusCode);

                _logger.LogWarning(
                    OperationalEventIds.CallbackRejected,
                    "Rejected callback signature for {Path}. Status {StatusCode}. Reason: {Reason}",
                    httpContext.Request.Path,
                    signatureResult.StatusCode,
                    signatureResult.Message);

                return CallbackIngressResult.Rejected(signatureResult.StatusCode, signatureResult.Message);
            }

            var path = httpContext.Request.Path.Value ?? string.Empty;
            var monitorEvent = new MonitorEventLocal
            {
                EventId = Guid.NewGuid(),
                ReceivedAt = DateTime.UtcNow,
                RawJson = bodyResult.Body,
                EventType = $"{eventFamily}:{path}",
                DeviceId = httpContext.Request.Query["device_id"].ToString(),
                UserId = httpContext.Request.Query["user_id"].ToString(),
                Payload = bodyResult.Body,
                Status = "received",
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _monitorEventRepository.AddMonitorEventAsync(monitorEvent);
            }
            catch (Exception ex)
            {
                OperationalMetrics.RecordCallbackIngress(
                    eventFamily,
                    path,
                    "persistence_failed",
                    StatusCodes.Status500InternalServerError);

                _logger.LogError(
                    OperationalEventIds.CallbackPersistenceFailed,
                    ex,
                    "Failed to persist callback event for {Path}. EventFamily {EventFamily}. RequestId {RequestId}.",
                    path,
                    eventFamily,
                    httpContext.TraceIdentifier);

                return CallbackIngressResult.Rejected(
                    StatusCodes.Status500InternalServerError,
                    "Nao foi possivel persistir o callback recebido.");
            }

            OperationalMetrics.RecordCallbackIngress(
                eventFamily,
                path,
                "accepted",
                StatusCodes.Status200OK);

            _logger.LogInformation(
                OperationalEventIds.CallbackAccepted,
                "Accepted callback request for {Path} as event {EventId}. EventFamily {EventFamily}. Device {DeviceRef}.",
                path,
                monitorEvent.EventId,
                eventFamily,
                PrivacyLogHelper.PseudonymizeIdentifier(monitorEvent.DeviceId));

            return CallbackIngressResult.Success(monitorEvent.EventId);
        }
    }

    public sealed class CallbackIngressResult
    {
        private CallbackIngressResult(bool accepted, Guid? eventId, int statusCode, string message)
        {
            Accepted = accepted;
            EventId = eventId;
            StatusCode = statusCode;
            Message = message;
        }

        public bool Accepted { get; }
        public Guid? EventId { get; }
        public int StatusCode { get; }
        public string Message { get; }

        public static CallbackIngressResult Success(Guid eventId)
        {
            return new CallbackIngressResult(true, eventId, StatusCodes.Status200OK, string.Empty);
        }

        public static CallbackIngressResult Rejected(int statusCode, string message)
        {
            return new CallbackIngressResult(false, null, statusCode, message);
        }
    }
}
