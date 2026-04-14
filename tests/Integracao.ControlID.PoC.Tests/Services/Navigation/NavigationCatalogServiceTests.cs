using Integracao.ControlID.PoC.Services.Navigation;

namespace Integracao.ControlID.PoC.Tests.Services.Navigation;

public class NavigationCatalogServiceTests
{
    private readonly NavigationCatalogService _service = new();

    [Fact]
    public void GetModule_ReturnsExactMatch_WhenActionIsProvided()
    {
        var module = _service.GetModule("System", "Network");

        Assert.NotNull(module);
        Assert.Equal("Rede e SSL", module!.Label);
        Assert.Equal("System", module.Controller);
        Assert.Equal("Network", module.Action);
    }

    [Fact]
    public void GetModule_ReturnsPrimaryFallback_WhenActionIsMissing()
    {
        var module = _service.GetModule("System", null);

        Assert.NotNull(module);
        Assert.Equal("Sistema", module!.Label);
        Assert.Equal("Info", module.Action);
    }

    [Fact]
    public void GetDomainByController_ResolvesCachedDomain()
    {
        var domain = _service.GetDomainByController("OfficialApi");

        Assert.NotNull(domain);
        Assert.Equal("api", domain!.Id);
        Assert.Equal("API", domain.ShortTitle);
    }

    [Fact]
    public void GetDomain_ReturnsPeopleDomainById()
    {
        var domain = _service.GetDomain("people");

        Assert.NotNull(domain);
        Assert.Equal("Pessoas", domain!.ShortTitle);
        Assert.Equal("success", domain.AccentTone);
    }

    [Fact]
    public void GetModule_ReturnsOperationModesModule()
    {
        var module = _service.GetModule("OperationModes", "Index");

        Assert.NotNull(module);
        Assert.Equal("Modos de operação", module!.Label);
        Assert.Equal("operations", module.DomainId);
    }
}
