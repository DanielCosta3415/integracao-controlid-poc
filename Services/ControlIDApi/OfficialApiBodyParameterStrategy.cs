using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.ViewModels.OfficialApi;

namespace Integracao.ControlID.PoC.Services.ControlIDApi
{
    public sealed class OfficialApiBodyParameterStrategy
    {
        private readonly OfficialApiDocumentationSeedCatalog _seedCatalog;

        public OfficialApiBodyParameterStrategy(OfficialApiDocumentationSeedCatalog seedCatalog)
        {
            _seedCatalog = seedCatalog;
        }

        public IList<OfficialApiParameterDocViewModel> Build(
            OfficialApiEndpointDefinition endpoint,
            OfficialApiEndpointDocumentationSeed seed)
        {
            var items = new List<OfficialApiParameterDocViewModel>();
            items.AddRange(seed.BodyFields.Select(OfficialApiParameterDocumentationUtilities.ToDoc));

            if (items.Count > 0 || string.IsNullOrWhiteSpace(endpoint.SamplePayload))
            {
                return items;
            }

            if (!endpoint.BodyKind.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                return items;
            }

            try
            {
                using var document = JsonDocument.Parse(endpoint.SamplePayload);
                ExtractJsonFields(document.RootElement, string.Empty, 0, items);
            }
            catch
            {
                return items;
            }

            return OfficialApiParameterDocumentationUtilities.Deduplicate(items);
        }

        private void ExtractJsonFields(
            JsonElement element,
            string prefix,
            int depth,
            IList<OfficialApiParameterDocViewModel> items)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        var currentPath = string.IsNullOrWhiteSpace(prefix) ? property.Name : $"{prefix}.{property.Name}";
                        if (property.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                        {
                            items.Add(new OfficialApiParameterDocViewModel
                            {
                                Path = currentPath,
                                TypeLabel = OfficialApiParameterDocumentationUtilities.InferElementType(property.Value),
                                RequirementLabel = "Nao especificado na PoC",
                                Description = _seedCatalog.LookupDescription(property.Name),
                                Example = property.Value.ValueKind == JsonValueKind.Array ? "[...]" : "{...}",
                                SourceLabel = "Inferido do exemplo",
                                Depth = depth
                            });
                        }
                        else
                        {
                            items.Add(new OfficialApiParameterDocViewModel
                            {
                                Path = currentPath,
                                TypeLabel = OfficialApiParameterDocumentationUtilities.InferElementType(property.Value),
                                RequirementLabel = "Nao especificado na PoC",
                                Description = _seedCatalog.LookupDescription(property.Name),
                                Example = OfficialApiParameterDocumentationUtilities.BuildExample(property.Value),
                                SourceLabel = "Inferido do exemplo",
                                Depth = depth
                            });
                        }

                        ExtractJsonFields(property.Value, currentPath, depth + 1, items);
                    }

                    break;
                case JsonValueKind.Array:
                    var first = element.EnumerateArray().FirstOrDefault();
                    if (first.ValueKind == JsonValueKind.Undefined)
                    {
                        return;
                    }

                    var arrayPath = $"{prefix}[]";
                    if (first.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                    {
                        ExtractJsonFields(first, arrayPath, depth + 1, items);
                    }

                    break;
            }
        }
    }
}
