using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.Shared
{
    public class AppPageHeaderViewModel
    {
        public string? Eyebrow { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? BadgeText { get; set; }
        public string BadgeTone { get; set; } = "neutral";
        public IReadOnlyList<PageHeaderActionViewModel> Actions { get; set; } = [];
    }

    public class PageHeaderActionViewModel
    {
        public string Label { get; set; } = string.Empty;
        public string Href { get; set; } = string.Empty;
        public string ButtonClass { get; set; } = "btn btn-outline-secondary";
        public string? AriaLabel { get; set; }
        public bool NewTab { get; set; }
    }
}
