using Integracao.ControlID.PoC.Controllers;
using Integracao.ControlID.PoC.Tests.TestSupport;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;

namespace Integracao.ControlID.PoC.Tests.Controllers;

public class SessionControllerTests
{
    private const string SessionDeviceAddressKey = "ControlID_DeviceAddress";
    private const string SessionSessionStringKey = "ControlID_SessionString";

    [Fact]
    public async Task Validate_WithoutActiveSessionDoesNotCallOfficialEndpoint()
    {
        var handler = new RecordingHttpMessageHandler();
        var controller = CreateController(handler);

        var result = await controller.Validate();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(SessionController.Status), redirect.ActionName);
        Assert.Empty(handler.Requests);
        Assert.Equal("warning", controller.TempData["StatusType"]);
    }

    [Theory]
    [InlineData("{\"session_is_valid\":true}", "success")]
    [InlineData("{\"session_is_valid\":0}", "danger")]
    public async Task Validate_MapsOfficialSessionStateToFunctionalStatus(string responseJson, string expectedStatusType)
    {
        var handler = new RecordingHttpMessageHandler();
        handler.EnqueueJson(responseJson);
        var controller = CreateController(handler);
        controller.HttpContext.Session.SetString(SessionDeviceAddressKey, "http://device.local");
        controller.HttpContext.Session.SetString(SessionSessionStringKey, "official-session");

        var result = await controller.Validate();

        Assert.IsType<RedirectToActionResult>(result);
        var request = Assert.Single(handler.Requests);
        Assert.Contains("/session_is_valid.fcgi", request.Url, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("session=official-session", request.Url, StringComparison.Ordinal);
        Assert.Equal(expectedStatusType, controller.TempData["StatusType"]);
    }

    [Fact]
    public void Clear_RemovesDeviceAndSessionFromLocalState()
    {
        var handler = new RecordingHttpMessageHandler();
        var controller = CreateController(handler);
        controller.HttpContext.Session.SetString(SessionDeviceAddressKey, "http://device.local");
        controller.HttpContext.Session.SetString(SessionSessionStringKey, "official-session");

        var result = controller.Clear();

        Assert.IsType<RedirectToActionResult>(result);
        Assert.False(controller.HttpContext.Session.TryGetValue(SessionDeviceAddressKey, out _));
        Assert.False(controller.HttpContext.Session.TryGetValue(SessionSessionStringKey, out _));
        Assert.Empty(handler.Requests);
    }

    private static SessionController CreateController(RecordingHttpMessageHandler handler)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<ISessionFeature>(new TestSessionFeature());

        return new SessionController(
            OfficialApiTestFactory.Create(httpContext, handler),
            NullLogger<SessionController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(httpContext, new DictionaryTempDataProvider())
        };
    }
}
