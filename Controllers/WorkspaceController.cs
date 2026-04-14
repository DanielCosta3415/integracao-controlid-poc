using System;
using System.Linq;
using Integracao.ControlID.PoC.Services.Navigation;
using Integracao.ControlID.PoC.ViewModels.Workspace;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class WorkspaceController : Controller
    {
        private readonly NavigationCatalogService _navigationCatalogService;

        public WorkspaceController(NavigationCatalogService navigationCatalogService)
        {
            _navigationCatalogService = navigationCatalogService;
        }

        public IActionResult Index(string? domainId = null, string? q = null)
        {
            var domains = _navigationCatalogService.GetDomains();
            var modules = _navigationCatalogService.GetAllModules();

            if (!string.IsNullOrWhiteSpace(domainId))
            {
                modules = modules
                    .Where(module => module.DomainId.Equals(domainId, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var query = q.Trim();
                modules = modules
                    .Where(module =>
                        module.Label.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        module.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        module.Tags.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var model = new WorkspaceExplorerViewModel
            {
                SelectedDomainId = domainId ?? string.Empty,
                SearchTerm = q ?? string.Empty,
                Domains = domains,
                Modules = modules.OrderBy(module => module.Visibility == "primary" ? 0 : 1).ThenBy(module => module.Priority).ToList()
            };

            ViewData["Title"] = "Mapa funcional";
            ViewData["Subtitle"] = "Índice global para encontrar todas as experiências, módulos e trilhas implementadas na PoC.";
            return View(model);
        }

        public IActionResult Domain(string id)
        {
            var domain = _navigationCatalogService.GetDomain(id);
            if (domain == null)
                return NotFound();

            var model = new DomainLandingViewModel
            {
                Domain = domain,
                PrimaryModules = domain.Modules.Where(module => module.Visibility == "primary").OrderBy(module => module.Priority).ToList(),
                SecondaryModules = domain.Modules.Where(module => module.Visibility != "primary").OrderBy(module => module.Priority).ToList()
            };

            ViewData["Title"] = domain.Title;
            ViewData["Subtitle"] = domain.Description;
            return View(model);
        }
    }
}
