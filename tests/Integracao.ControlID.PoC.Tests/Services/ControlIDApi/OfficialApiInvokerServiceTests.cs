using System.Reflection;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Options;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.Observability;
using Integracao.ControlID.PoC.Services.Security;
using Integracao.ControlID.PoC.Tests.TestSupport;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

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

    [Fact]
    public async Task InvokeAsync_PropagatesCorrelationIdHeaderToOfficialApiCall()
    {
        var handler = new RecordingHttpMessageHandler();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[ObservabilityConstants.CorrelationIdHeaderName] = "operator-flow-1";
        httpContext.Items[ObservabilityConstants.CorrelationIdItemName] = "operator-flow-1";

        var invoker = new OfficialApiInvokerService(
            new StaticHttpClientFactory(handler),
            NullLogger<OfficialApiInvokerService>.Instance,
            new ControlIdInputSanitizer(),
            new OfficialApiCircuitBreaker(Microsoft.Extensions.Options.Options.Create(new ControlIdCircuitBreakerOptions
            {
                Enabled = false
            })),
            new HttpContextAccessor { HttpContext = httpContext },
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ControlIDApi:ConnectionTimeoutSeconds"] = "5"
                })
                .Build());

        await invoker.InvokeAsync(
            new OfficialApiEndpointDefinition
            {
                Id = "health-probe",
                Method = "GET",
                Path = "/system_information.fcgi"
            },
            "http://device.local",
            string.Empty,
            string.Empty,
            string.Empty);

        var request = Assert.Single(handler.Requests);
        Assert.True(request.Headers.TryGetValue(ObservabilityConstants.CorrelationIdHeaderName, out var correlationId));
        Assert.Equal("operator-flow-1", correlationId);
    }
}
