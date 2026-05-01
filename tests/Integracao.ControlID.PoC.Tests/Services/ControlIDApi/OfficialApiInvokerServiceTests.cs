using System.Reflection;
using Integracao.ControlID.PoC.Services.ControlIDApi;

namespace Integracao.ControlID.PoC.Tests.Services.ControlIDApi;

public class OfficialApiInvokerServiceTests
{
    [Fact]
    public void BuildSafeDisplayUrl_Masks_Sensitive_Query_Values()
    {
        var method = typeof(OfficialApiInvokerService).GetMethod(
            "BuildSafeDisplayUrl",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        var result = Assert.IsType<string>(method.Invoke(null, ["http://device.local/login.fcgi?session=abc123&foo=bar&token=secret"]));

        Assert.Contains("session=***", result);
        Assert.Contains("token=***", result);
        Assert.Contains("foo=bar", result);
        Assert.DoesNotContain("abc123", result);
        Assert.DoesNotContain("secret", result);
    }
}
