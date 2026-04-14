using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Services.ProductSpecific;
using Integracao.ControlID.PoC.ViewModels.ProductSpecific;

namespace Integracao.ControlID.PoC.Tests.Helpers;

public class ProductSpecificPresentationHelperTests
{
    [Fact]
    public void CountConfiguredAccessAudioEvents_CountsOnlyEnabledSlots()
    {
        var model = new ProductSpecificViewModel
        {
            AccessAudioHasNotIdentified = true,
            AccessAudioHasAuthorized = true,
            AccessAudioHasNotAuthorized = false,
            AccessAudioHasUseMask = true
        };

        var count = ProductSpecificPresentationHelper.CountConfiguredAccessAudioEvents(model);

        Assert.Equal(3, count);
    }

    [Fact]
    public void BuildResponsePanel_UsesSectionSpecificCopy()
    {
        var model = new ProductSpecificViewModel
        {
            ActiveSection = ProductSpecificSections.SipAudio,
            ResponseJson = "{\"status\":\"ok\"}",
            ResultStatusType = "success"
        };

        var panel = ProductSpecificPresentationHelper.BuildResponsePanel(model);

        Assert.NotNull(panel);
        Assert.Equal("Resposta do toque personalizado do SIP", panel!.Title);
        Assert.Equal("success", panel.BadgeTone);
        Assert.Equal("{\"status\":\"ok\"}", panel.Content);
    }
}
