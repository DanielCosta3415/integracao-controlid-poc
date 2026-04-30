using Integracao.ControlID.PoC.Helpers;

namespace Integracao.ControlID.PoC.Tests.Helpers;

public class HighImpactOperationGuardTests
{
    [Theory]
    [InlineData("ALTERAR REDE", "ALTERAR REDE")]
    [InlineData(" alterar rede ", "ALTERAR REDE")]
    [InlineData("reset fabrica", "RESET FABRICA")]
    public void IsConfirmed_Accepts_ExpectedPhrase_IgnoringCaseAndWhitespace(string provided, string expected)
    {
        var result = HighImpactOperationGuard.IsConfirmed(provided, expected);

        Assert.True(result);
    }

    [Theory]
    [InlineData("", "ALTERAR REDE")]
    [InlineData(null, "ALTERAR REDE")]
    [InlineData("ALTERAR", "ALTERAR REDE")]
    public void IsConfirmed_Rejects_MissingOrPartialPhrase(string? provided, string expected)
    {
        var result = HighImpactOperationGuard.IsConfirmed(provided, expected);

        Assert.False(result);
    }

    [Fact]
    public void BuildDestroyObjectsConfirmation_Includes_ObjectName()
    {
        var result = HighImpactOperationGuard.BuildDestroyObjectsConfirmation("users");

        Assert.Equal("DESTROY users", result);
    }
}
