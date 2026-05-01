using Integracao.ControlID.PoC.Services.Analytics;

namespace Integracao.ControlID.PoC.Tests.Services.Analytics;

public class ProductAnalyticsEventClassifierTests
{
    [Theory]
    [InlineData("GET", "/Auth/LocalLogin", "activation", "local_login_viewed", "view")]
    [InlineData("POST", "/Auth/LocalLogin", "activation", "local_login_submitted", "submit")]
    [InlineData("POST", "/Auth/Login", "device_session", "device_login_submitted", "submit")]
    [InlineData("POST", "/OfficialApi/Invoke/login?session=secret", "official_api", "official_endpoint_invoked", "submit")]
    [InlineData("POST", "/Auth/ChangePassword", "security", "credential_change_requested", "submit")]
    [InlineData("GET", "/PushCenter/Details/6b263f5f-6778-40dc-a4fe-99589fe21ec7?user_id=123", "push", "push_flow_used", "view")]
    [InlineData("GET", "/Privacy/Index?email=person@example.test", "privacy_governance", "privacy_report_used", "view")]
    public void TryClassify_MapsCriticalRoutesWithoutUsingIdentifiers(
        string method,
        string path,
        string expectedFlow,
        string expectedEvent,
        string expectedAction)
    {
        var classified = ProductAnalyticsEventClassifier.TryClassify(method, path, out var productEvent);

        Assert.True(classified);
        Assert.Equal(expectedFlow, productEvent.Flow);
        Assert.Equal(expectedEvent, productEvent.Name);
        Assert.Equal(expectedAction, productEvent.Action);
        Assert.DoesNotContain("123", productEvent.Name, StringComparison.Ordinal);
        Assert.DoesNotContain("secret", productEvent.Name, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("example", productEvent.Name, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryClassify_IgnoresUnknownRoutesToAvoidExcessiveTracking()
    {
        var classified = ProductAnalyticsEventClassifier.TryClassify(
            "GET",
            "/unknown/experimental?email=person@example.test",
            out _);

        Assert.False(classified);
    }
}
