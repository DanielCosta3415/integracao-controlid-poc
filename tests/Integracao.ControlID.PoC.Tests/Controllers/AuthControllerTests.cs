using Integracao.ControlID.PoC.Controllers;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Tests.TestSupport;
using Integracao.ControlID.PoC.ViewModels.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;

namespace Integracao.ControlID.PoC.Tests.Controllers;

public class AuthControllerTests
{
    private const string SessionDeviceAddressKey = "ControlID_DeviceAddress";
    private const string SessionSessionStringKey = "ControlID_SessionString";

    [Fact]
    public async Task Login_WithoutConnectedDeviceBlocksBeforeOfficialCall()
    {
        using var database = new SqliteTestDatabase();
        var handler = new RecordingHttpMessageHandler();
        var controller = CreateController(database, handler);

        var result = await controller.Login(new LoginViewModel
        {
            Username = "operator",
            Password = "<senha>"
        });

        Assert.IsType<ViewResult>(result);
        Assert.Empty(handler.Requests);
        Assert.False(controller.HttpContext.Session.TryGetValue(SessionSessionStringKey, out _));
        Assert.Contains(
            controller.ModelState[string.Empty]!.Errors,
            error => error.ErrorMessage.Contains("Nenhum dispositivo conectado", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Login_WhenDeviceResponseDoesNotContainSessionKeepsUserOnLogin()
    {
        using var database = new SqliteTestDatabase();
        var handler = new RecordingHttpMessageHandler();
        handler.EnqueueJson("{}");
        var controller = CreateController(database, handler);
        controller.HttpContext.Session.SetString(SessionDeviceAddressKey, "http://device.local");

        var result = await controller.Login(new LoginViewModel
        {
            Username = "operator",
            Password = "<senha>"
        });

        Assert.IsType<ViewResult>(result);
        var request = Assert.Single(handler.Requests);
        Assert.Equal("POST", request.Method);
        Assert.Contains("/login.fcgi", request.Url, StringComparison.OrdinalIgnoreCase);
        Assert.False(controller.HttpContext.Session.TryGetValue(SessionSessionStringKey, out _));
        Assert.Contains(
            controller.ModelState[string.Empty]!.Errors,
            error => error.ErrorMessage.Contains("sess", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Logout_GetFromCrossSiteNavigationRequiresInterfaceConfirmation()
    {
        using var database = new SqliteTestDatabase();
        var handler = new RecordingHttpMessageHandler();
        var controller = CreateController(database, handler);
        controller.HttpContext.Session.SetString(SessionDeviceAddressKey, "http://device.local");
        controller.HttpContext.Session.SetString(SessionSessionStringKey, "official-session");
        controller.HttpContext.Request.Headers["Sec-Fetch-Site"] = "cross-site";

        var result = await controller.Logout();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AuthController.Status), redirect.ActionName);
        Assert.Empty(handler.Requests);
        Assert.Equal("official-session", controller.HttpContext.Session.GetString(SessionSessionStringKey));
        Assert.Equal("warning", controller.TempData["StatusType"]);
    }

    private static AuthController CreateController(SqliteTestDatabase database, RecordingHttpMessageHandler handler)
    {
        var httpContext = CreateHttpContext();

        return new AuthController(
            OfficialApiTestFactory.Create(httpContext, handler),
            new UserRepository(database.Context, NullLogger<UserRepository>.Instance),
            NullLogger<AuthController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(httpContext, new DictionaryTempDataProvider())
        };
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<ISessionFeature>(new TestSessionFeature());
        return httpContext;
    }
}
