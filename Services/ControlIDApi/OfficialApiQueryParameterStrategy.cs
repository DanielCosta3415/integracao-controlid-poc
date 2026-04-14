using System.Text.RegularExpressions;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.ViewModels.OfficialApi;

namespace Integracao.ControlID.PoC.Services.ControlIDApi
{
    public sealed class OfficialApiQueryParameterStrategy
    {
        private static readonly Regex QueryPairRegex = new(@"(?<key>[a-zA-Z_][a-zA-Z0-9_\-]*)=(?<value>[^&\s,]+)", RegexOptions.Compiled);
        private readonly OfficialApiDocumentationSeedCatalog _seedCatalog;

        public OfficialApiQueryParameterStrategy(OfficialApiDocumentationSeedCatalog seedCatalog)
        {
            _seedCatalog = seedCatalog;
        }

        public IList<OfficialApiParameterDocViewModel> Build(
            OfficialApiEndpointDefinition endpoint,
            OfficialApiEndpointDocumentationSeed seed)
        {
            var items = new List<OfficialApiParameterDocViewModel>();
            items.AddRange(seed.QueryFields.Select(OfficialApiParameterDocumentationUtilities.ToDoc));

            if (items.Count > 0)
            {
                return items;
            }

            if (string.IsNullOrWhiteSpace(endpoint.Notes))
            {
                return items;
            }

            foreach (Match match in QueryPairRegex.Matches(endpoint.Notes))
            {
                var key = match.Groups["key"].Value;
                var example = match.Groups["value"].Value;

                items.Add(new OfficialApiParameterDocViewModel
                {
                    Path = key,
                    TypeLabel = OfficialApiParameterDocumentationUtilities.InferType(example),
                    RequirementLabel = "Nao especificado na PoC",
                    Description = _seedCatalog.LookupDescription(key),
                    Example = example,
                    SourceLabel = "Inferido das observacoes",
                    Depth = 0
                });
            }

            return OfficialApiParameterDocumentationUtilities.Deduplicate(items);
        }
    }
}
