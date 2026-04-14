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
    public void BuildApiFailureMessage_TruncatesAndNormalizesResponseBody()
    {
        var result = new OfficialApiInvocationResult
        {
            StatusCode = 500,
            ResponseBody = "Falha interna\r\n" + new string('x', 300)
        };

        var message = SecurityTextHelper.BuildApiFailureMessage(result, "Erro ao consultar endpoint");

        Assert.StartsWith("Erro ao consultar endpoint: Falha interna", message);
        Assert.DoesNotContain("\r", message, StringComparison.Ordinal);
        Assert.EndsWith("...", message);
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
