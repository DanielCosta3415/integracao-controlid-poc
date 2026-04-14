using Integracao.ControlID.PoC.Services.OperationModes;

namespace Integracao.ControlID.PoC.Tests.Services.OperationModes;

public class OperationModesProfileResolverTests
{
    private readonly OperationModesProfileResolver _resolver = new();

    [Theory]
    [InlineData(false, true, "standalone")]
    [InlineData(true, true, "pro")]
    [InlineData(true, false, "enterprise")]
    public void Resolve_ReturnsExpectedMode(bool onlineEnabled, bool localIdentificationEnabled, string expectedKey)
    {
        var snapshot = _resolver.Resolve(onlineEnabled, localIdentificationEnabled);

        Assert.Equal(expectedKey, snapshot.Key);
        Assert.False(string.IsNullOrWhiteSpace(snapshot.Label));
        Assert.False(string.IsNullOrWhiteSpace(snapshot.Description));
    }
}
