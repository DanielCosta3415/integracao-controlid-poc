using Integracao.ControlID.PoC.Controllers;
using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Tests.TestSupport;
using Integracao.ControlID.PoC.ViewModels.OfficialObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;

namespace Integracao.ControlID.PoC.Tests.Controllers;

public class OfficialObjectsControllerTests
{
    private const string SessionDeviceAddressKey = "ControlID_DeviceAddress";
    private const string SessionSessionStringKey = "ControlID_SessionString";

    [Fact]
    public async Task Create_WithInvalidJsonDoesNotInvokeOfficialEndpoint()
    {
        var handler = new RecordingHttpMessageHandler();
        var controller = CreateController(handler, connected: true);

        var result = await controller.Create(new OfficialObjectsViewModel
        {
            SelectedObjectName = "users",
            CreateValuesJson = "{invalid-json"
        });

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<OfficialObjectsViewModel>(view.Model);
        Assert.Equal("Index", view.ViewName);
        Assert.Equal("create", model.ActiveSection);
        Assert.NotEmpty(model.ErrorMessage);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task Destroy_WithInvalidConfirmationDoesNotInvokeOfficialEndpoint()
    {
        var handler = new RecordingHttpMessageHandler();
        var controller = CreateController(handler, connected: true);

        var result = await controller.Destroy(new OfficialObjectsViewModel
        {
            SelectedObjectName = "users",
            DestroyWhereJson = "{\"users\":{\"id\":101}}",
            DestroyConfirmationPhrase = "wrong"
        });

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<OfficialObjectsViewModel>(view.Model);
        Assert.Equal("Index", view.ViewName);
        Assert.Contains(HighImpactOperationGuard.BuildDestroyObjectsConfirmation("users"), model.ErrorMessage);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task Load_WithValidFilterInvokesLoadObjectsWithExpectedPayload()
    {
        var handler = new RecordingHttpMessageHandler();
        handler.EnqueueJson("{\"users\":[]}");
        var controller = CreateController(handler, connected: true);

        var result = await controller.Load(new OfficialObjectsViewModel
        {
            SelectedObjectName = "users",
            LoadWhereJson = "{\"users\":{\"id\":101}}"
        });

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<OfficialObjectsViewModel>(view.Model);
        var request = Assert.Single(handler.Requests);
        Assert.Equal("Index", view.ViewName);
        Assert.Equal("success", model.ResultStatusType);
        Assert.Contains("/load_objects.fcgi", request.Url, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"object\":\"users\"", request.Body, StringComparison.Ordinal);
        Assert.Contains("\"where\":{\"users\":{\"id\":101}}", request.Body, StringComparison.Ordinal);
    }

    private static OfficialObjectsController CreateController(RecordingHttpMessageHandler handler, bool connected)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<ISessionFeature>(new TestSessionFeature());

        if (connected)
        {
            httpContext.Session.SetString(SessionDeviceAddressKey, "http://device.local");
            httpContext.Session.SetString(SessionSessionStringKey, "official-session");
        }

        return new OfficialObjectsController(
            OfficialApiTestFactory.Create(httpContext, handler),
            NullLogger<OfficialObjectsController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(httpContext, new DictionaryTempDataProvider())
        };
    }
}
