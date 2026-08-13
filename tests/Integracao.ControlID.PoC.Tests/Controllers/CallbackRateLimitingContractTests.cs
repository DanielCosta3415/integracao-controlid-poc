using Integracao.ControlID.PoC.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Integracao.ControlID.PoC.Tests.Controllers;

public class CallbackRateLimitingContractTests
{
    [Fact]
    public void OfficialCallbacksController_UsesCallbackIngressRateLimit()
    {
        var attribute = Attribute.GetCustomAttribute(
            typeof(OfficialCallbacksController),
            typeof(EnableRateLimitingAttribute));

        Assert.NotNull(attribute);
    }

    [Theory]
    [InlineData(nameof(PushCenterController.Poll))]
    [InlineData(nameof(PushCenterController.Result))]
    public void PushCenterIngressActions_UseCallbackIngressRateLimit(string actionName)
    {
        var attribute = GetActionRateLimitAttribute(typeof(PushCenterController), actionName);

        Assert.NotNull(attribute);
    }

    [Fact]
    public void LegacyPushReceive_UsesCallbackIngressRateLimit()
    {
        var attribute = GetActionRateLimitAttribute(typeof(PushController), nameof(PushController.Receive));

        Assert.NotNull(attribute);
    }

    [Fact]
    public void MachineIngressActions_ExplicitlyUseHmacInsteadOfBrowserAntiforgery()
    {
        Assert.NotNull(Attribute.GetCustomAttribute(
            typeof(OfficialCallbacksController),
            typeof(IgnoreAntiforgeryTokenAttribute)));
        AssertActionIgnoresAntiforgery(typeof(MonitorWebhookController), nameof(MonitorWebhookController.Receive));
        AssertActionIgnoresAntiforgery(typeof(PushController), nameof(PushController.Receive));
        AssertActionIgnoresAntiforgery(typeof(PushCenterController), nameof(PushCenterController.Result));
    }

    private static EnableRateLimitingAttribute? GetActionRateLimitAttribute(Type controllerType, string actionName)
    {
        var method = controllerType.GetMethods()
            .Single(method => method.Name == actionName);

        return Attribute.GetCustomAttribute(method, typeof(EnableRateLimitingAttribute)) as EnableRateLimitingAttribute;
    }

    private static void AssertActionIgnoresAntiforgery(Type controllerType, string actionName)
    {
        var method = controllerType.GetMethods().Single(candidate => candidate.Name == actionName);
        Assert.NotNull(Attribute.GetCustomAttribute(method, typeof(IgnoreAntiforgeryTokenAttribute)));
    }
}
