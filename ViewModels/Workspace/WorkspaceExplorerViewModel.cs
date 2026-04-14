using System;
using System.Collections.Generic;
using Integracao.ControlID.PoC.ViewModels.Shared;

namespace Integracao.ControlID.PoC.ViewModels.Workspace
{
    public class WorkspaceExplorerViewModel
    {
        public string SelectedDomainId { get; set; } = string.Empty;
        public string SearchTerm { get; set; } = string.Empty;
        public IReadOnlyList<NavigationDomainViewModel> Domains { get; set; } = Array.Empty<NavigationDomainViewModel>();
        public IReadOnlyList<NavigationModuleViewModel> Modules { get; set; } = Array.Empty<NavigationModuleViewModel>();
    }

    public class DomainLandingViewModel
    {
        public NavigationDomainViewModel Domain { get; set; } = new();
        public IReadOnlyList<NavigationModuleViewModel> PrimaryModules { get; set; } = Array.Empty<NavigationModuleViewModel>();
        public IReadOnlyList<NavigationModuleViewModel> SecondaryModules { get; set; } = Array.Empty<NavigationModuleViewModel>();
    }
}
