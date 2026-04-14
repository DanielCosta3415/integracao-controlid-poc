using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.ViewModels.OfficialApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class OfficialApiController : Controller
    {
        private const string SessionDeviceAddressKey = "ControlID_DeviceAddress";
        private const string SessionSessionStringKey = "ControlID_SessionString";

        private readonly OfficialApiCatalogService _catalogService;
        private readonly OfficialApiInvokerService _invokerService;

        public OfficialApiController(OfficialApiCatalogService catalogService, OfficialApiInvokerService invokerService)
        {
            _catalogService = catalogService;
            _invokerService = invokerService;
        }

        public IActionResult Index(string? category = null, string? method = null, string? direction = null, string? session = null)
        {
            var endpoints = _catalogService.GetAll();

            if (!string.IsNullOrWhiteSpace(category))
            {
                endpoints = endpoints
                    .Where(endpoint => endpoint.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(method))
            {
                endpoints = endpoints
                    .Where(endpoint => endpoint.Method.Equals(method, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(direction))
            {
                endpoints = endpoints
                    .Where(endpoint => endpoint.Direction.Equals(direction, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(session))
            {
                endpoints = session switch
                {
                    "required" => endpoints.Where(endpoint => endpoint.RequiresSession).ToList(),
                    "optional" => endpoints.Where(endpoint => !endpoint.RequiresSession).ToList(),
                    _ => endpoints
                };
            }

            var model = new OfficialApiIndexViewModel
            {
                SelectedCategory = category ?? string.Empty,
                SelectedMethod = method ?? string.Empty,
                SelectedDirection = direction ?? string.Empty,
                SelectedSessionFilter = session ?? string.Empty,
                Categories = _catalogService.GetCategories(),
                Methods = _catalogService.GetAll().Select(endpoint => endpoint.Method).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value).ToList(),
                Directions = _catalogService.GetAll().Select(endpoint => endpoint.Direction).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value).ToList(),
                Endpoints = endpoints
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Invoke(string id)
        {
            var endpoint = _catalogService.GetById(id);
            if (endpoint == null)
                return NotFound();

            return View(BuildInvokeViewModel(endpoint));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Invoke(OfficialApiInvokeViewModel model)
        {
            var endpoint = _catalogService.GetById(model.EndpointId);
            if (endpoint == null)
                return NotFound();

            model.Endpoint = endpoint;

            if (!endpoint.Invokable)
            {
                model.ErrorMessage = "Este endpoint é um callback do equipamento para o servidor. Consulte a implementação local desta PoC.";
                return View(model);
            }

            model.Result = await _invokerService.InvokeAsync(
                endpoint,
                model.DeviceAddress,
                model.SessionString,
                model.AdditionalQuery,
                model.RequestBody);

            if (!string.IsNullOrWhiteSpace(model.Result.ErrorMessage))
                model.ErrorMessage = model.Result.ErrorMessage;

            return View(model);
        }

        private OfficialApiInvokeViewModel BuildInvokeViewModel(OfficialApiEndpointDefinition endpoint)
        {
            return new OfficialApiInvokeViewModel
            {
                EndpointId = endpoint.Id,
                Endpoint = endpoint,
                DeviceAddress = HttpContext.Session.GetString(SessionDeviceAddressKey) ?? string.Empty,
                SessionString = HttpContext.Session.GetString(SessionSessionStringKey) ?? string.Empty,
                RequestBody = endpoint.SamplePayload
            };
        }
    }
}

