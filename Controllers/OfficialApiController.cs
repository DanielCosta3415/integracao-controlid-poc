using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.Security;
using Integracao.ControlID.PoC.ViewModels.OfficialApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class OfficialApiController : Controller
    {
        private const string SessionDeviceAddressKey = "ControlID_DeviceAddress";
        private const string SessionSessionStringKey = "ControlID_SessionString";

        private readonly OfficialApiCatalogService _catalogService;
        private readonly OfficialApiContractDocumentationService _documentationService;
        private readonly OfficialApiInvokerService _invokerService;

        public OfficialApiController(
            OfficialApiCatalogService catalogService,
            OfficialApiContractDocumentationService documentationService,
            OfficialApiInvokerService invokerService)
        {
            _catalogService = catalogService;
            _documentationService = documentationService;
            _invokerService = invokerService;
        }

        public IActionResult Index(string? category = null, string? method = null, string? direction = null, string? session = null)
        {
            var allEndpoints = _catalogService.GetAll();
            IEnumerable<OfficialApiEndpointDefinition> filteredEndpoints = allEndpoints;

            if (!string.IsNullOrWhiteSpace(category))
            {
                filteredEndpoints = filteredEndpoints
                    .Where(endpoint => endpoint.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(method))
            {
                filteredEndpoints = filteredEndpoints
                    .Where(endpoint => endpoint.Method.Equals(method, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(direction))
            {
                filteredEndpoints = filteredEndpoints
                    .Where(endpoint => endpoint.Direction.Equals(direction, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(session))
            {
                filteredEndpoints = session switch
                {
                    "required" => filteredEndpoints.Where(endpoint => endpoint.RequiresSession),
                    "optional" => filteredEndpoints.Where(endpoint => !endpoint.RequiresSession),
                    _ => filteredEndpoints
                };
            }

            var endpoints = filteredEndpoints.ToList();

            var model = new OfficialApiIndexViewModel
            {
                SelectedCategory = category ?? string.Empty,
                SelectedMethod = method ?? string.Empty,
                SelectedDirection = direction ?? string.Empty,
                SelectedSessionFilter = session ?? string.Empty,
                Categories = _catalogService.GetCategories(),
                Methods = _catalogService.GetMethods(),
                Directions = _catalogService.GetDirections(),
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
        [Authorize(Roles = AppSecurityRoles.Administrator)]
        public async Task<IActionResult> Invoke(OfficialApiInvokeViewModel model)
        {
            var endpoint = _catalogService.GetById(model.EndpointId);
            if (endpoint == null)
                return NotFound();

            model.Endpoint = endpoint;
            model.Contract = _documentationService.Build(endpoint);

            if (!ModelState.IsValid)
            {
                model.ErrorMessage = "Revise os dados informados antes de invocar o endpoint.";
                return View(model);
            }

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
                Contract = _documentationService.Build(endpoint),
                DeviceAddress = HttpContext.Session.GetString(SessionDeviceAddressKey) ?? string.Empty,
                SessionString = HttpContext.Session.GetString(SessionSessionStringKey) ?? string.Empty,
                RequestBody = endpoint.SamplePayload
            };
        }
    }
}

