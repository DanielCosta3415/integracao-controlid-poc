using System.Text.Json;
using Integracao.ControlID.PoC.Services.OperationModes;

namespace Integracao.ControlID.PoC.Tests.Services.OperationModes;

public class OperationModesPayloadFactoryTests
{
    private readonly OperationModesPayloadFactory _factory = new();

    [Fact]
    public void BuildStandaloneSettings_DisablesOnlineAndKeepsLocalIdentification()
    {
        var payload = JsonSerializer.Serialize(_factory.BuildStandaloneSettings());

        Assert.Contains("\"online\":\"0\"", payload);
        Assert.Contains("\"local_identification\":\"1\"", payload);
    }

    [Fact]
    public void BuildProSettings_UsesLocalIdentificationAndServerId()
    {
        var payload = JsonSerializer.Serialize(_factory.BuildProSettings(18, extractTemplate: true, maxRequestAttempts: 5));

        Assert.Contains("\"online\":\"1\"", payload);
        Assert.Contains("\"local_identification\":\"1\"", payload);
        Assert.Contains("\"server_id\":\"18\"", payload);
        Assert.Contains("\"extract_template\":\"1\"", payload);
        Assert.Contains("\"max_request_attempts\":\"5\"", payload);
    }

    [Fact]
    public void BuildEnterpriseSettings_DisablesLocalIdentification()
    {
        var payload = JsonSerializer.Serialize(_factory.BuildEnterpriseSettings(31, extractTemplate: false, maxRequestAttempts: 2));

        Assert.Contains("\"online\":\"1\"", payload);
        Assert.Contains("\"local_identification\":\"0\"", payload);
        Assert.Contains("\"server_id\":\"31\"", payload);
        Assert.Contains("\"extract_template\":\"0\"", payload);
    }
}
