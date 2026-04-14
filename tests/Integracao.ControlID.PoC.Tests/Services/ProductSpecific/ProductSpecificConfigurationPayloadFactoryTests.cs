using System.Text.Json;
using Integracao.ControlID.PoC.Services.ProductSpecific;
using Integracao.ControlID.PoC.ViewModels.ProductSpecific;

namespace Integracao.ControlID.PoC.Tests.Services.ProductSpecific;

public class ProductSpecificConfigurationPayloadFactoryTests
{
    [Fact]
    public void BuildSignalsSettings_FormatsBooleanFlags_AsEquipmentFriendlyStrings()
    {
        var factory = new ProductSpecificConfigurationPayloadFactory();
        var model = new ProductSpecificViewModel
        {
            SignalRelayEnabled = true,
            SignalRelayAutoClose = false,
            SignalRelayTimeout = 15,
            SignalSecBoxOutMode = "door",
            SignalRelayOutMode = "relay",
            SignalGpioExt1Mode = "input",
            SignalGpioExt1Idle = "0",
            SignalGpioExt2Mode = "output",
            SignalGpioExt2Idle = "1",
            SignalGpioExt3Mode = "input",
            SignalGpioExt3Idle = "0"
        };

        var payload = factory.BuildSignalsSettings(model);
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(payload));
        var general = document.RootElement.GetProperty("general");

        Assert.Equal("1", general.GetProperty("relay1_enabled").GetString());
        Assert.Equal("0", general.GetProperty("relay1_auto_close").GetString());
        Assert.Equal("15", general.GetProperty("relay1_timeout").GetString());
    }

    [Fact]
    public void BuildQrCodeSettings_UsesSelectedModule_AsRootSection()
    {
        var factory = new ProductSpecificConfigurationPayloadFactory();
        var model = new ProductSpecificViewModel
        {
            QrModule = "barras",
            QrCodeLegacyModeEnabled = "1",
            QrTotpEnabled = true,
            QrTotpWindowSize = 60,
            QrTotpWindowNum = 3,
            QrTotpSingleUse = false,
            QrTotpTzOffset = 180
        };

        var payload = factory.BuildQrCodeSettings(model);
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(payload));

        Assert.True(document.RootElement.TryGetProperty("barras", out var barras));
        Assert.Equal("1", barras.GetProperty("qrcode_legacy_mode_enabled").GetString());
        Assert.Equal("1", barras.GetProperty("totp_enabled").GetString());
    }
}
