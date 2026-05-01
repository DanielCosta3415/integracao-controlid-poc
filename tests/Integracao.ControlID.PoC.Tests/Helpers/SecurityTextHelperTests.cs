using System.Net.Http;
using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Models.ControlIDApi;

namespace Integracao.ControlID.PoC.Tests.Helpers;

public class SecurityTextHelperTests
{
    [Fact]
    public void BuildSafeUserMessage_HidesUnexpectedInfrastructureDetails()
    {
        var message = SecurityTextHelper.BuildSafeUserMessage("Erro ao autenticar", new HttpRequestException("socket failure"));

        Assert.Equal("Erro ao autenticar: falha de comunicação com o equipamento.", message);
    }

    [Fact]
    public void BuildApiFailureMessage_OmitsRawResponseBody()
    {
        var result = new OfficialApiInvocationResult
        {
            StatusCode = 500,
            ResponseBody = "Falha interna com dado sensivel"
        };

        var message = SecurityTextHelper.BuildApiFailureMessage(result, "Erro ao consultar endpoint");

        Assert.Equal("Erro ao consultar endpoint (status HTTP 500; corpo de resposta omitido por segurança).", message);
        Assert.DoesNotContain("dado sensivel", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MaskSensitiveValue_PreservesOnlyTheEdges()
    {
        var masked = SecurityTextHelper.MaskSensitiveValue("0123456789abcdef", prefix: 3, suffix: 2);

        Assert.Equal("012...ef", masked);
    }

    [Fact]
    public void NormalizeForDisplay_RepairsCommonEncodingArtifacts()
    {
        var message = SecurityTextHelper.NormalizeForDisplay("Ã‰ necessÃ¡rio conectar-se e autenticar com um equipamento Control iD.");

        Assert.Equal("É necessário conectar-se e autenticar com um equipamento Control iD.", message);
    }
}
