using System.Text.Json;
using Integracao.ControlID.PoC.Services.ProductSpecific;

namespace Integracao.ControlID.PoC.Tests.Services.ProductSpecific;

public class ProductSpecificJsonReaderTests
{
    private readonly ProductSpecificJsonReader _reader = new();

    [Fact]
    public void GetConfigBool_InterpretsEquipmentFriendlyFlags()
    {
        using var document = JsonDocument.Parse(
            """
            {
              "pjsip": {
                "enabled": "1"
              }
            }
            """);

        var value = _reader.GetConfigBool(document.RootElement, "pjsip", "enabled");

        Assert.True(value);
    }

    [Fact]
    public void GetRootInt_ReturnsFallback_WhenValueIsMissing()
    {
        using var document = JsonDocument.Parse("{}");

        var value = _reader.GetRootInt(document.RootElement, "status", 42);

        Assert.Equal(42, value);
    }
}
