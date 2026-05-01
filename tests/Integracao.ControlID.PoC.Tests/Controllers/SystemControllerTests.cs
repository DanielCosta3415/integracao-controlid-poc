using Integracao.ControlID.PoC.Controllers;
using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Services.Files;
using Integracao.ControlID.PoC.Tests.TestSupport;
using Integracao.ControlID.PoC.ViewModels.System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;

namespace Integracao.ControlID.PoC.Tests.Controllers;

public class SystemControllerTests
{
    private const string SessionDeviceAddressKey = "ControlID_DeviceAddress";
    private const string SessionSessionStringKey = "ControlID_SessionString";

    [Theory]
    [InlineData("Reset", HighImpactOperationGuard.ConfirmReboot)]
    [InlineData("FactoryReset", HighImpactOperationGuard.ConfirmFactoryReset)]
    [InlineData("RebootRecovery", HighImpactOperationGuard.ConfirmRebootRecovery)]
    [InlineData("DeleteAdmins", HighImpactOperationGuard.ConfirmDeleteAdmins)]
    public async Task HighImpactActions_WithInvalidConfirmationDoNotInvokeOfficialEndpoint(
        string actionName,
        string expectedConfirmation)
    {
        var handler = new RecordingHttpMessageHandler();
        var controller = CreateController(handler, connected: true);

        var result = actionName switch
        {
            "Reset" => await controller.Reset("wrong"),
            "FactoryReset" => await controller.FactoryReset(keepNetworkInfo: true, confirmationPhrase: "wrong"),
            "RebootRecovery" => await controller.RebootRecovery("wrong"),
            "DeleteAdmins" => await controller.DeleteAdmins("wrong"),
            _ => throw new InvalidOperationException("Unsupported action.")
        };

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(SystemController.Info), redirect.ActionName);
        Assert.Equal("warning", controller.TempData["StatusType"]);
        Assert.Contains(expectedConfirmation, controller.TempData["StatusMessage"]?.ToString());
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task Network_WithInvalidConfirmationDoesNotInvokeOfficialEndpoint()
    {
        var handler = new RecordingHttpMessageHandler();
        var controller = CreateController(handler, connected: true);

        var result = await controller.Network(new SystemNetworkViewModel
        {
            IpAddress = "192.168.0.20",
            Netmask = "255.255.255.0",
            Gateway = "192.168.0.1",
            ConfirmationPhrase = "wrong"
        });

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SystemNetworkViewModel>(view.Model);
        Assert.Equal("warning", model.ResultStatusType);
        Assert.Contains(HighImpactOperationGuard.ConfirmNetworkChange, model.ResultMessage);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task FactoryReset_PreservesKeepNetworkInfoValueInOfficialPayload()
    {
        var handler = new RecordingHttpMessageHandler();
        handler.EnqueueJson("{}");
        var controller = CreateController(handler, connected: true);

        var result = await controller.FactoryReset(
            keepNetworkInfo: false,
            confirmationPhrase: HighImpactOperationGuard.ConfirmFactoryReset);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        var request = Assert.Single(handler.Requests);
        Assert.Equal(nameof(SystemController.Info), redirect.ActionName);
        Assert.Contains("/reset_to_factory_default.fcgi", request.Url, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"keep_network_info\":false", request.Body, StringComparison.Ordinal);
        Assert.Equal("warning", controller.TempData["StatusType"]);
    }

    private static SystemController CreateController(RecordingHttpMessageHandler handler, bool connected)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<ISessionFeature>(new TestSessionFeature());

        if (connected)
        {
            httpContext.Session.SetString(SessionDeviceAddressKey, "http://device.local");
            httpContext.Session.SetString(SessionSessionStringKey, "official-session");
        }

        return new SystemController(
            OfficialApiTestFactory.Create(httpContext, handler),
            new UploadedFileBase64Encoder(),
            NullLogger<SystemController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(httpContext, new DictionaryTempDataProvider())
        };
    }
}
