using Integracao.ControlID.PoC.Options;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Tests.TestSupport;

public static class OfficialApiTestFactory
{
    public static OfficialControlIdApiService Create(
        HttpContext httpContext,
        RecordingHttpMessageHandler handler)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ControlIDApi:ConnectionTimeoutSeconds"] = "5"
            })
            .Build();

        var invoker = new OfficialApiInvokerService(
            new StaticHttpClientFactory(handler),
            NullLogger<OfficialApiInvokerService>.Instance,
            new ControlIdInputSanitizer(),
            new OfficialApiCircuitBreaker(Microsoft.Extensions.Options.Options.Create(new ControlIdCircuitBreakerOptions
            {
                Enabled = false
            })),
            configuration);

        return new OfficialControlIdApiService(
            new HttpContextAccessor { HttpContext = httpContext },
            new OfficialApiCatalogService(),
            invoker,
            NullLogger<OfficialControlIdApiService>.Instance);
    }
}
