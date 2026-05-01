using System.Net;
using Integracao.ControlID.PoC.Helpers;

namespace Integracao.ControlID.PoC.Tests.Helpers;

public class PrivacyLogHelperTests
{
    [Fact]
    public void PseudonymizeUser_IsStableAndDoesNotExposeOriginalIdentifier()
    {
        const string user = "maria.silva@example.test";

        var first = PrivacyLogHelper.PseudonymizeUser(user);
        var second = PrivacyLogHelper.PseudonymizeUser(user);

        Assert.Equal(first, second);
        Assert.StartsWith("ref:", first);
        Assert.DoesNotContain("maria", first, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("example.test", first, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PseudonymizeIp_DoesNotExposeRawAddress()
    {
        var value = PrivacyLogHelper.PseudonymizeIp(IPAddress.Parse("192.168.10.20"));

        Assert.StartsWith("ip:", value);
        Assert.DoesNotContain("192.168.10.20", value, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PseudonymizeEndpoint_DoesNotExposeHostPathOrQuery()
    {
        var value = PrivacyLogHelper.PseudonymizeEndpoint("http://192.168.10.20:8080/login.fcgi?session=secret");

        Assert.StartsWith("endpoint:http:", value);
        Assert.DoesNotContain("192.168.10.20", value, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("login.fcgi", value, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", value, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PseudonymizeIdentifier_DoesNotExposeNumericIdentifier()
    {
        var value = PrivacyLogHelper.PseudonymizeIdentifier(123456);

        Assert.StartsWith("ref:", value);
        Assert.DoesNotContain("123456", value, StringComparison.OrdinalIgnoreCase);
    }
}
