namespace Integracao.ControlID.PoC.ViewModels.OfficialApi
{
    public class OfficialApiContractViewModel
    {
        public string FunctionalSummary { get; set; } = string.Empty;
        public string InteractionSummary { get; set; } = string.Empty;
        public string RequestGuidance { get; set; } = string.Empty;
        public string ResponseGuidance { get; set; } = string.Empty;
        public string QueryGuidance { get; set; } = string.Empty;
        public string QueryTemplate { get; set; } = string.Empty;
        public string SamplePayload { get; set; } = string.Empty;
        public bool HasStructuredQueryParameters => QueryParameters.Count > 0;
        public bool HasStructuredBodyParameters => BodyParameters.Count > 0;
        public IList<OfficialApiParameterDocViewModel> QueryParameters { get; set; } = [];
        public IList<OfficialApiParameterDocViewModel> BodyParameters { get; set; } = [];
        public IList<string> DeveloperTips { get; set; } = [];
    }

    public class OfficialApiParameterDocViewModel
    {
        public string Path { get; set; } = string.Empty;
        public string TypeLabel { get; set; } = string.Empty;
        public string RequirementLabel { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Example { get; set; } = string.Empty;
        public string SourceLabel { get; set; } = string.Empty;
        public int Depth { get; set; }
    }
}
