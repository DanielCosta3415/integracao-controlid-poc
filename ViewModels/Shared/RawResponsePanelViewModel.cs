namespace Integracao.ControlID.PoC.ViewModels.Shared
{
    public class RawResponsePanelViewModel
    {
        public string Title { get; set; } = "Resposta técnica";
        public string? Description { get; set; }
        public string Content { get; set; } = string.Empty;
        public string PanelId { get; set; } = string.Empty;
        public bool StartExpanded { get; set; }
        public string? BadgeText { get; set; }
        public string BadgeTone { get; set; } = "neutral";
    }
}
