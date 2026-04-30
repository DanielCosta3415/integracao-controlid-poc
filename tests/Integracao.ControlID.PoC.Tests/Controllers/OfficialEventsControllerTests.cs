using Integracao.ControlID.PoC.Controllers;
using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Tests.TestSupport;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;

namespace Integracao.ControlID.PoC.Tests.Controllers;

public class OfficialEventsControllerTests
{
    [Fact]
    public async Task Clear_KeepsEventsWhenConfirmationIsInvalid()
    {
        using var database = new SqliteTestDatabase();
        var repository = CreateRepository(database);
        await repository.AddMonitorEventAsync(new MonitorEventLocal
        {
            EventId = Guid.NewGuid(),
            EventType = "access",
            RawJson = "{\"event\":\"access\"}",
            Payload = "{\"event\":\"access\"}",
            Status = "received"
        });
        var controller = CreateController(repository);

        var result = await controller.Clear("wrong");

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Single(await repository.GetAllMonitorEventsAsync());
    }

    [Fact]
    public async Task Clear_DeletesEventsWhenConfirmationIsValid()
    {
        using var database = new SqliteTestDatabase();
        var repository = CreateRepository(database);
        await repository.AddMonitorEventAsync(new MonitorEventLocal
        {
            EventId = Guid.NewGuid(),
            EventType = "access",
            RawJson = "{\"event\":\"access\"}",
            Payload = "{\"event\":\"access\"}",
            Status = "received"
        });
        var controller = CreateController(repository);

        var result = await controller.Clear(HighImpactOperationGuard.ConfirmClearMonitorEvents);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Empty(await repository.GetAllMonitorEventsAsync());
    }

    private static OfficialEventsController CreateController(MonitorEventRepository repository)
    {
        var httpContext = new DefaultHttpContext();

        return new OfficialEventsController(repository)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(httpContext, new DictionaryTempDataProvider())
        };
    }

    private static MonitorEventRepository CreateRepository(SqliteTestDatabase database)
    {
        return new MonitorEventRepository(database.Context, NullLogger<MonitorEventRepository>.Instance);
    }
}
