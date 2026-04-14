using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.ViewModels.OfficialApi;

namespace Integracao.ControlID.PoC.Services.ControlIDApi
{
    public sealed class OfficialApiContractDocumentationService
    {
        private readonly OfficialApiDocumentationSeedCatalog _seedCatalog;
        private readonly OfficialApiQueryParameterStrategy _queryParameterStrategy;
        private readonly OfficialApiBodyParameterStrategy _bodyParameterStrategy;

        public OfficialApiContractDocumentationService(
            OfficialApiDocumentationSeedCatalog seedCatalog,
            OfficialApiQueryParameterStrategy queryParameterStrategy,
            OfficialApiBodyParameterStrategy bodyParameterStrategy)
        {
            _seedCatalog = seedCatalog;
            _queryParameterStrategy = queryParameterStrategy;
            _bodyParameterStrategy = bodyParameterStrategy;
        }

        public OfficialApiContractViewModel Build(OfficialApiEndpointDefinition endpoint)
        {
            var seed = _seedCatalog.GetSeed(endpoint.Id);
            var queryParameters = _queryParameterStrategy.Build(endpoint, seed);
            var bodyParameters = _bodyParameterStrategy.Build(endpoint, seed);

            return new OfficialApiContractViewModel
            {
                FunctionalSummary = !string.IsNullOrWhiteSpace(endpoint.FunctionalDescription)
                    ? endpoint.FunctionalDescription
                    : endpoint.Summary,
                InteractionSummary = BuildInteractionSummary(endpoint),
                RequestGuidance = BuildRequestGuidance(endpoint, seed),
                ResponseGuidance = BuildResponseGuidance(endpoint, seed),
                QueryGuidance = BuildQueryGuidance(seed, queryParameters),
                QueryTemplate = !string.IsNullOrWhiteSpace(endpoint.QueryTemplate)
                    ? endpoint.QueryTemplate
                    : BuildQueryTemplate(queryParameters),
                SamplePayload = endpoint.SamplePayload,
                QueryParameters = queryParameters,
                BodyParameters = bodyParameters,
                DeveloperTips = BuildDeveloperTips(endpoint, queryParameters, bodyParameters)
            };
        }

        private static string BuildInteractionSummary(OfficialApiEndpointDefinition endpoint)
        {
            return endpoint.Direction.Equals("server-callback", StringComparison.OrdinalIgnoreCase)
                ? "Fluxo de entrada: o equipamento chama a PoC. Essa rota existe para receber dados do dispositivo, nao para ser disparada manualmente."
                : "Fluxo de saida: a PoC envia a requisicao ao equipamento, usando o metodo, corpo e contexto de sessao indicados abaixo.";
        }

        private static string BuildRequestGuidance(OfficialApiEndpointDefinition endpoint, OfficialApiEndpointDocumentationSeed seed)
        {
            if (!string.IsNullOrWhiteSpace(seed.RequestGuidance))
            {
                return seed.RequestGuidance;
            }

            return endpoint.BodyKind.ToLowerInvariant() switch
            {
                "none" => "Esta chamada nao exige corpo. Preencha apenas sessao, endereco e, se necessario, query adicional.",
                "json" => "Envie o corpo em JSON UTF-8. Use o exemplo como ponto de partida e ajuste somente os campos necessarios.",
                "binary" => "Envie o corpo como base64 puro do arquivo ou binario exigido pelo endpoint.",
                "multipart" => "Monte o envio multipart com campos textuais e arquivos conforme a observacao tecnica abaixo.",
                "form" => "Esse fluxo trabalha com envio de formulario ou callback e normalmente nao deve ser disparado manualmente nesta tela.",
                _ => "Confira o contrato abaixo e use a documentacao oficial para detalhes especificos do corpo."
            };
        }

        private static string BuildResponseGuidance(OfficialApiEndpointDefinition endpoint, OfficialApiEndpointDocumentationSeed seed)
        {
            if (!string.IsNullOrWhiteSpace(seed.ResponseGuidance))
            {
                return seed.ResponseGuidance;
            }

            if (endpoint.Direction.Equals("server-callback", StringComparison.OrdinalIgnoreCase))
            {
                return "Como se trata de callback, o dado real chega do equipamento para a PoC quando o evento correspondente acontece.";
            }

            return endpoint.BodyKind.Equals("binary", StringComparison.OrdinalIgnoreCase)
                ? "O retorno pode vir em binario ou base64 ou em confirmacao textual, dependendo do endpoint e do equipamento."
                : "O retorno costuma ser JSON ou confirmacao textual processada pela PoC e exibida no painel tecnico abaixo.";
        }

        private static string BuildQueryGuidance(
            OfficialApiEndpointDocumentationSeed seed,
            IList<OfficialApiParameterDocViewModel> queryParameters)
        {
            if (!string.IsNullOrWhiteSpace(seed.QueryGuidance))
            {
                return seed.QueryGuidance;
            }

            return queryParameters.Count == 0
                ? "Esta PoC nao encontrou query adicional estruturada para esta chamada."
                : "Preencha a query adicional apenas quando o endpoint exigir parametros fora do corpo principal.";
        }

        private static string BuildQueryTemplate(IList<OfficialApiParameterDocViewModel> queryParameters)
        {
            if (queryParameters.Count == 0)
            {
                return string.Empty;
            }

            return string.Join("&", queryParameters.Select(parameter => $"{parameter.Path.Split('.').Last()}={parameter.Example}"));
        }

        private static IList<string> BuildDeveloperTips(
            OfficialApiEndpointDefinition endpoint,
            IList<OfficialApiParameterDocViewModel> queryParameters,
            IList<OfficialApiParameterDocViewModel> bodyParameters)
        {
            var tips = new List<string>();

            if (!string.IsNullOrWhiteSpace(endpoint.Notes))
            {
                tips.Add(endpoint.Notes);
            }

            if (!string.IsNullOrWhiteSpace(endpoint.DeveloperGuidance))
            {
                tips.Add(endpoint.DeveloperGuidance);
            }

            if (endpoint.RequiresSession)
            {
                tips.Add("Garanta uma sessao valida antes da chamada. Sem ela, o equipamento pode recusar a operacao.");
            }

            if (endpoint.BodyKind.Equals("json", StringComparison.OrdinalIgnoreCase)
                && bodyParameters.Count == 0
                && !string.IsNullOrWhiteSpace(endpoint.SamplePayload))
            {
                tips.Add("Os campos abaixo foram inferidos do payload de exemplo. Quando a obrigatoriedade nao estiver marcada como explicita, confirme na documentacao oficial.");
            }

            if (queryParameters.Any(parameter => parameter.SourceLabel != "Especificado pela PoC"))
            {
                tips.Add("Alguns parametros de query foram inferidos das observacoes do catalogo. Use-os como guia rapido, nao como contrato absoluto.");
            }

            return tips.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
