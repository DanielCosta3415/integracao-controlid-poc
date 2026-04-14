namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    public class OfficialApiEndpointDefinition
    {
        public string Id { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Direction { get; init; } = "device-request";
        public string Method { get; init; } = "POST";
        public string Path { get; init; } = string.Empty;
        public string BodyKind { get; init; } = "none";
        public string ContentType { get; init; } = "application/json";
        public bool RequiresSession { get; init; }
        public bool Invokable { get; init; } = true;
        public string Summary { get; init; } = string.Empty;
        public string DocumentationUrl { get; init; } = string.Empty;
        public string SamplePayload { get; init; } = string.Empty;
        public string Notes { get; init; } = string.Empty;
    }
}
