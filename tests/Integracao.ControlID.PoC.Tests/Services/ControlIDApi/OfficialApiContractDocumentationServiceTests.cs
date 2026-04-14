using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;

namespace Integracao.ControlID.PoC.Tests.Services.ControlIDApi;

public class OfficialApiContractDocumentationServiceTests
{
    [Fact]
    public void Build_UsesExplicitSeededQueryParameters_WhenEndpointHasStructuredSeed()
    {
        var catalog = new OfficialApiDocumentationSeedCatalog();
        var service = CreateService(catalog);
        var endpoint = new OfficialApiEndpointDefinition
        {
            Id = "user-get-image",
            Summary = "Baixa foto do usuario."
        };

        var contract = service.Build(endpoint);

        var parameter = Assert.Single(contract.QueryParameters);
        Assert.Equal("user_id", parameter.Path);
        Assert.Equal("Obrigatorio", parameter.RequirementLabel);
        Assert.Equal("123", parameter.Example);
    }

    [Fact]
    public void Build_InfersJsonBodyFields_WhenOnlySamplePayloadIsAvailable()
    {
        var catalog = new OfficialApiDocumentationSeedCatalog();
        var service = CreateService(catalog);
        var endpoint = new OfficialApiEndpointDefinition
        {
            Id = "custom-endpoint",
            Summary = "Endpoint interno de teste.",
            BodyKind = "json",
            SamplePayload = "{\"user\":{\"id\":1,\"name\":\"Ada\"}}"
        };

        var contract = service.Build(endpoint);

        Assert.Contains(contract.BodyParameters, parameter => parameter.Path == "user");
        Assert.Contains(contract.BodyParameters, parameter => parameter.Path == "user.id" && parameter.TypeLabel == "integer");
        Assert.Contains(contract.BodyParameters, parameter => parameter.Path == "user.name" && parameter.TypeLabel == "string");
    }

    private static OfficialApiContractDocumentationService CreateService(OfficialApiDocumentationSeedCatalog catalog)
    {
        return new OfficialApiContractDocumentationService(
            catalog,
            new OfficialApiQueryParameterStrategy(catalog),
            new OfficialApiBodyParameterStrategy(catalog));
    }
}
