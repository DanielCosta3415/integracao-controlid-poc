using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.Shared
{
    public class NavigationDomainViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ShortTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string AccentTone { get; set; } = "danger";
        public string Icon { get; set; } = "PO";
        public IReadOnlyList<NavigationModuleViewModel> Modules { get; set; } = Array.Empty<NavigationModuleViewModel>();
    }

    public class NavigationModuleViewModel
    {
        public string Key { get; set; } = string.Empty;
        public string DomainId { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string ShortLabel { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = "Index";
        public string Tags { get; set; } = string.Empty;
        public string ExperienceType { get; set; } = "workspace";
        public string Visibility { get; set; } = "primary";
        public string Complexity { get; set; } = "Operação";
        public string StatusText { get; set; } = "Disponível";
        public string StatusTone { get; set; } = "success";
        public string? Prerequisite { get; set; }
        public string? DocumentationUrl { get; set; }
        public int Priority { get; set; } = 100;
        public bool IsTechnical { get; set; }
    }

    public class ConnectionPanelViewModel
    {
        public string Scheme { get; set; } = "http";
        [StringLength(2048, ErrorMessage = "Informe um host ou URL com até 2048 caracteres.")]
        public string Host { get; set; } = string.Empty;
        [Range(1, 65535, ErrorMessage = "Informe uma porta válida entre 1 e 65535.")]
        public int? Port { get; set; }
        public string BaseAddress { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = "/";
        public bool IsDeviceConnected { get; set; }
        public bool IsSessionActive { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceSerial { get; set; } = string.Empty;
        public string DeviceFirmware { get; set; } = string.Empty;
    }

    public class PageShellContextViewModel
    {
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string SectionLabel { get; set; } = string.Empty;
        public NavigationDomainViewModel? CurrentDomain { get; set; }
        public NavigationModuleViewModel? CurrentModule { get; set; }
        public ConnectionPanelViewModel ConnectionPanel { get; set; } = new();
    }
}
