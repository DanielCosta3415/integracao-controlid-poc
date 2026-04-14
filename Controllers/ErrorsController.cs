using System.Diagnostics;
using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.ViewModels.Errors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Controllers
{
    public class ErrorsController : Controller
    {
        private readonly ILogger<ErrorsController> _logger;

        public ErrorsController(ILogger<ErrorsController> logger)
        {
            _logger = logger;
        }

        [Route("Errors/General")]
        public IActionResult General()
        {
            var errorViewModel = CreateErrorViewModelFromException();
            if (!string.IsNullOrEmpty(errorViewModel.Message))
            {
                _logger.LogError("Erro não tratado em {Path} (RequestId: {RequestId})", errorViewModel.Path, errorViewModel.RequestId);
            }

            Response.StatusCode = 500;
            return View("Error", errorViewModel);
        }

        [Route("Errors/NotFound")]
        public IActionResult NotFoundPage()
        {
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                Message = "A página requisitada não foi encontrada (404).",
                Path = HttpContext.Request.Path
            };

            _logger.LogWarning("404 NotFound em {Path} (RequestId: {RequestId})", errorViewModel.Path, errorViewModel.RequestId);

            Response.StatusCode = 404;
            return View("NotFound", errorViewModel);
        }

        [Route("Errors/ServerError")]
        public IActionResult ServerError()
        {
            var errorViewModel = CreateErrorViewModelFromException();
            if (!string.IsNullOrEmpty(errorViewModel.Message))
            {
                _logger.LogError("Erro de servidor em {Path} (RequestId: {RequestId})", errorViewModel.Path, errorViewModel.RequestId);
            }

            Response.StatusCode = 500;
            return View("ServerError", errorViewModel);
        }

        [Route("Errors/AccessDenied")]
        public IActionResult AccessDenied()
        {
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                Message = "Acesso negado (403).",
                Path = HttpContext.Request.Path
            };

            _logger.LogWarning("403 AccessDenied em {Path} (RequestId: {RequestId})", errorViewModel.Path, errorViewModel.RequestId);

            Response.StatusCode = 403;
            return View("AccessDenied", errorViewModel);
        }

        private ErrorViewModel CreateErrorViewModelFromException()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            if (exceptionFeature != null)
            {
                errorViewModel.Path = exceptionFeature.Path;
                // SECURITY: a interface expõe apenas uma mensagem segura e o request id.
                // Stack trace e detalhes internos permanecem restritos aos logs do servidor.
                errorViewModel.Message = SecurityTextHelper.BuildSafeUserMessage("Erro interno na aplicação", exceptionFeature.Error);
                errorViewModel.StackTrace = string.Empty;
            }

            return errorViewModel;
        }
    }
}
