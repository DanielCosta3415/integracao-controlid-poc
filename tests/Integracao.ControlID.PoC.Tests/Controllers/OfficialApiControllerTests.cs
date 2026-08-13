using System.Reflection;
using Integracao.ControlID.PoC.Controllers;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.Security;
using Integracao.ControlID.PoC.Tests.TestSupport;
using Integracao.ControlID.PoC.ViewModels.OfficialApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Tests.Controllers;

public sealed class OfficialApiControllerTests
{
    private const string SessionDeviceAddressKey = "ControlID_DeviceAddress";
    private const string SessionSessionStringKey = "ControlID_SessionString";

    [Fact]
    public void InvokeGet_RequiresAdministratorRole()
    {
        var action = typeof(OfficialApiController).GetMethod(
            nameof(OfficialApiController.Invoke),
            BindingFlags.Instance | BindingFlags.Public,
            [typeof(string)]);

        var authorization = Assert.Single(action!.GetCustomAttributes<AuthorizeAttribute>());
        Assert.Equal(AppSecurityRoles.Administrator, authorization.Roles);
    }

    [Fact]
    public void InvokeGet_DoesNotExposeDeviceSessionInViewModel()
    {
        var handler = new RecordingHttpMessageHandler();
        var controller = CreateController(handler, "server-session");

        var result = controller.Invoke("system-information");

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<OfficialApiInvokeViewModel>(view.Model);
        Assert.Equal(string.Empty, model.SessionString);
    }

    [Fact]
    public async Task InvokePost_IgnoresSubmittedSessionAndUsesServerSession()
    {
        var handler = new RecordingHttpMessageHandler();
        handler.EnqueueJson("{}");
        var controller = CreateController(handler, "server-session");

        var result = await controller.Invoke(new OfficialApiInvokeViewModel
        {
            EndpointId = "logout",
            DeviceAddress = "http://device.local",
            SessionString = "submitted-session",
            RequestBody = string.Empty
        });

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<OfficialApiInvokeViewModel>(view.Model);
        var request = Assert.Single(handler.Requests);
        Assert.Contains("session=server-session", request.Url, StringComparison.Ordinal);
        Assert.DoesNotContain("submitted-session", request.Url, StringComparison.Ordinal);
        Assert.Equal(string.Empty, model.SessionString);
    }

    private static OfficialApiController CreateController(
        RecordingHttpMessageHandler handler,
        string sessionString)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<ISessionFeature>(new TestSessionFeature());
        httpContext.Session.SetString(SessionDeviceAddressKey, "http://device.local");
        httpContext.Session.SetString(SessionSessionStringKey, sessionString);

        var seedCatalog = new OfficialApiDocumentationSeedCatalog();
        var documentation = new OfficialApiContractDocumentationService(
            seedCatalog,
            new OfficialApiQueryParameterStrategy(seedCatalog),
            new OfficialApiBodyParameterStrategy(seedCatalog));

        return new OfficialApiController(
            new OfficialApiCatalogService(),
            documentation,
            OfficialApiTestFactory.CreateInvoker(httpContext, handler))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }
}
