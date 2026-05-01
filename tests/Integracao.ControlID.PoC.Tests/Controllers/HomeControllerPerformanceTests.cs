using Integracao.ControlID.PoC.Controllers;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Services.Navigation;
using Integracao.ControlID.PoC.Services.Push;
using Integracao.ControlID.PoC.Services.Security;
using Integracao.ControlID.PoC.Tests.TestSupport;
using Integracao.ControlID.PoC.ViewModels.Home;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;

namespace Integracao.ControlID.PoC.Tests.Controllers;

public class HomeControllerPerformanceTests
{
    [Fact]
    public async Task Index_UsesBoundedReadModelsAndPublishesDashboardServerTiming()
    {
        using var database = new SqliteTestDatabase();
        SeedDashboardData(database, eventCount: 525, commandCount: 525, pendingCount: 12);
        database.Context.ChangeTracker.Clear();
        var controller = CreateController(database);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<HomeDashboardViewModel>(view.Model);
        Assert.Equal(525, model.RecentEventCount);
        Assert.Equal(12, model.PendingPushCount);
        Assert.True(model.RecentActivities.Count <= 6);
        Assert.Contains("dashboard-local-metrics;dur=", controller.Response.Headers["Server-Timing"].ToString());
        Assert.Empty(database.Context.ChangeTracker.Entries<MonitorEventLocal>());
        Assert.Empty(database.Context.ChangeTracker.Entries<PushCommandLocal>());
    }

    private static HomeController CreateController(SqliteTestDatabase database)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<ISessionFeature>(new TestSessionFeature { Session = new TestSession() });
        httpContext.Request.Path = "/";

        var navigationCatalog = new NavigationCatalogService();
        var controller = new HomeController(
            NullLogger<HomeController>.Instance,
            officialApi: null!,
            new OfficialApiCatalogService(),
            new MonitorEventRepository(database.Context, NullLogger<MonitorEventRepository>.Instance),
            new PushCommandRepository(database.Context, NullLogger<PushCommandRepository>.Instance),
            navigationCatalog,
            new PageShellService(navigationCatalog, new ControlIdInputSanitizer()))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(httpContext, new DictionaryTempDataProvider())
        };
        controller.Url = new StaticUrlHelper(controller.ControllerContext);

        return controller;
    }

    private static void SeedDashboardData(
        SqliteTestDatabase database,
        int eventCount,
        int commandCount,
        int pendingCount)
    {
        var now = DateTime.UtcNow;

        for (var index = 0; index < eventCount; index++)
        {
            database.Context.MonitorEvents.Add(new MonitorEventLocal
            {
                EventId = Guid.NewGuid(),
                EventType = "access",
                RawJson = "{\"event\":\"access\"}",
                Payload = "{\"event\":\"access\"}",
                Status = "received",
                ReceivedAt = now.AddSeconds(-index),
                CreatedAt = now.AddSeconds(-index)
            });
        }

        for (var index = 0; index < commandCount; index++)
        {
            database.Context.PushCommands.Add(new PushCommandLocal
            {
                CommandId = Guid.NewGuid(),
                CommandType = "custom",
                DeviceId = "device-1",
                UserId = string.Empty,
                Payload = "{\"type\":\"custom\"}",
                RawJson = "{\"type\":\"custom\"}",
                Status = index < pendingCount ? PushCommandStatuses.Pending : PushCommandStatuses.Completed,
                ReceivedAt = now.AddSeconds(-index),
                CreatedAt = now.AddSeconds(-index)
            });
        }

        database.Context.SaveChanges();
    }
}
