using System;
using System.Collections.Generic;
using Integracao.ControlID.PoC.ViewModels.Shared;

namespace Integracao.ControlID.PoC.ViewModels.Home
{
    public class HomeDashboardViewModel
    {
        public string? DeviceAddress { get; set; }
        public string? DeviceName { get; set; }
        public string? DeviceSerial { get; set; }
        public string? DeviceFirmware { get; set; }
        public bool IsSessionActive { get; set; }
        public string? StatusMessage { get; set; }
        public string StatusType { get; set; } = "info";
        public int OfficialEndpointCount { get; set; }
        public int InvokableEndpointCount { get; set; }
        public int CallbackEndpointCount { get; set; }
        public int RecentEventCount { get; set; }
        public int PendingPushCount { get; set; }
        public ConnectionPanelViewModel ConnectionPanel { get; set; } = new();
        public IReadOnlyList<NavigationModuleViewModel> FeaturedModules { get; set; } = Array.Empty<NavigationModuleViewModel>();
        public IReadOnlyList<NavigationDomainViewModel> Domains { get; set; } = Array.Empty<NavigationDomainViewModel>();
        public IReadOnlyList<HomeQuickFlowViewModel> QuickFlows { get; set; } = Array.Empty<HomeQuickFlowViewModel>();
        public IReadOnlyList<DashboardActivityItemViewModel> RecentActivities { get; set; } = Array.Empty<DashboardActivityItemViewModel>();

        public bool IsDeviceConnected => !string.IsNullOrWhiteSpace(DeviceAddress);
    }

    public class HomeQuickFlowViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Href { get; set; } = string.Empty;
        public string ButtonLabel { get; set; } = string.Empty;
    }

    public class DashboardActivityItemViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Meta { get; set; } = string.Empty;
        public string Href { get; set; } = string.Empty;
        public string Tone { get; set; } = "neutral";
        public DateTime OccurredAt { get; set; }
    }
}
