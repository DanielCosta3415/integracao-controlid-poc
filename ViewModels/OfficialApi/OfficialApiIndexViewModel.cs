using Integracao.ControlID.PoC.Models.ControlIDApi;

namespace Integracao.ControlID.PoC.ViewModels.OfficialApi
{
    public class OfficialApiIndexViewModel
    {
        public string SelectedCategory { get; set; } = string.Empty;
        public string SelectedMethod { get; set; } = string.Empty;
        public string SelectedDirection { get; set; } = string.Empty;
        public string SelectedSessionFilter { get; set; } = string.Empty;
        public List<string> Categories { get; set; } = new();
        public List<string> Methods { get; set; } = new();
        public List<string> Directions { get; set; } = new();
        public List<OfficialApiEndpointDefinition> Endpoints { get; set; } = new();
    }
}
