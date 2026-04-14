using System.Diagnostics;
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

        // GET: /Errors/General
        [Route("Errors/General")]
        public IActionResult General()
        {
            var errorViewModel = CreateErrorViewModelFromException();
            if (!string.IsNullOrEmpty(errorViewModel.Message))
            {
                _logger.LogError("{Message}\nStackTrace: {StackTrace}\nPath: {Path}\nRequestId: {RequestId}",
                    errorViewModel.Message,
                    errorViewModel.StackTrace,
                    errorViewModel.Path,
                    errorViewModel.RequestId);
            }
            Response.StatusCode = 500;
            return View("Error", errorViewModel);
        }

        // GET: /Errors/NotFound
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

        // GET: /Errors/ServerError
        [Route("Errors/ServerError")]
        public IActionResult ServerError()
        {
            var errorViewModel = CreateErrorViewModelFromException();
            if (!string.IsNullOrEmpty(errorViewModel.Message))
            {
                _logger.LogError("{Message}\nStackTrace: {StackTrace}\nPath: {Path}\nRequestId: {RequestId}",
                    errorViewModel.Message,
                    errorViewModel.StackTrace,
                    errorViewModel.Path,
                    errorViewModel.RequestId);
            }
            Response.StatusCode = 500;
            return View("ServerError", errorViewModel);
        }

        // GET: /Errors/AccessDenied
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

        /// <summary>
        /// Helper para criar ErrorViewModel a partir do contexto da exceção.
        /// </summary>
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
                errorViewModel.Message = exceptionFeature.Error.Message;

                // Em produção, por padrão, você não mostra StackTrace:
#if DEBUG
                errorViewModel.StackTrace = exceptionFeature.Error.StackTrace ?? string.Empty;
#else
                errorViewModel.StackTrace = string.Empty;
#endif
            }
            return errorViewModel;
        }
    }
}
