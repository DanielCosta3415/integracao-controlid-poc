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
            CreateConcurrencyLimiter(),
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
            string.Empty,
            TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);
        Assert.True(request.Headers.TryGetValue(ObservabilityConstants.CorrelationIdHeaderName, out var correlationId));
        Assert.Equal("operator-flow-1", correlationId);
    }

    [Fact]
    public async Task InvokeAsync_RejectsResponseLargerThanConfiguredLimit()
    {
        var handler = new RecordingHttpMessageHandler();
        handler.EnqueueResponse(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(new byte[64 * 1024 + 1])
        });
        var httpContext = new DefaultHttpContext();
        var invoker = new OfficialApiInvokerService(
            new StaticHttpClientFactory(handler),
            NullLogger<OfficialApiInvokerService>.Instance,
            new ControlIdInputSanitizer(),
            new OfficialApiCircuitBreaker(Microsoft.Extensions.Options.Options.Create(new ControlIdCircuitBreakerOptions
            {
                Enabled = false
            })),
            CreateConcurrencyLimiter(),
            new HttpContextAccessor { HttpContext = httpContext },
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ControlIDApi:ConnectionTimeoutSeconds"] = "5",
                    ["ControlIDApi:MaxResponseBodyBytes"] = (64 * 1024).ToString(System.Globalization.CultureInfo.InvariantCulture)
                })
                .Build());

        var result = await invoker.InvokeAsync(
            new OfficialApiEndpointDefinition
            {
                Id = "oversized-response",
                Method = "GET",
                Path = "/large.fcgi"
            },
            "http://device.local",
            string.Empty,
            string.Empty,
            string.Empty,
            TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Equal(StatusCodes.Status502BadGateway, result.StatusCode);
        Assert.Contains("excedeu o limite", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvokeBinaryAsync_SendsExactBytesAndKeepsBinaryResponseOutOfBase64()
    {
        var handler = new RecordingHttpMessageHandler();
        handler.EnqueueResponse(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([9, 8, 7])
            {
                Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream") }
            }
        });
        var invoker = new OfficialApiInvokerService(
            new StaticHttpClientFactory(handler),
            NullLogger<OfficialApiInvokerService>.Instance,
            new ControlIdInputSanitizer(),
            new OfficialApiCircuitBreaker(Microsoft.Extensions.Options.Options.Create(new ControlIdCircuitBreakerOptions { Enabled = false })),
            CreateConcurrencyLimiter(),
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ControlIDApi:ConnectionTimeoutSeconds"] = "5"
            }).Build());

        var result = await invoker.InvokeBinaryAsync(
            new OfficialApiEndpointDefinition
            {
                Id = "send-video",
                Method = "POST",
                Path = "/send_video.fcgi",
                BodyKind = "binary",
                RequiresSession = true
            },
            "http://device.local",
            "session-1",
            "current=1&total=1",
            new byte[] { 1, 2, 3, 4 },
            TestContext.Current.CancellationToken);

        Assert.Equal(new byte[] { 1, 2, 3, 4 }, Assert.Single(handler.Requests).BodyBytes);
        Assert.Equal(new byte[] { 9, 8, 7 }, result.ResponseBytes);
        Assert.Empty(result.ResponseBody);
    }

    [Fact]
    public async Task InvokeToStreamAsync_CopiesBinaryResponseWithoutRetainingItInMemoryResult()
    {
        var responseBytes = Enumerable.Range(0, 256 * 1024)
            .Select(static index => (byte)(index % 251))
            .ToArray();
        var handler = new RecordingHttpMessageHandler();
        handler.EnqueueResponse(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(responseBytes)
            {
                Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream") }
            }
        });
        var invoker = new OfficialApiInvokerService(
            new StaticHttpClientFactory(handler),
            NullLogger<OfficialApiInvokerService>.Instance,
            new ControlIdInputSanitizer(),
            new OfficialApiCircuitBreaker(Microsoft.Extensions.Options.Options.Create(new ControlIdCircuitBreakerOptions { Enabled = false })),
            CreateConcurrencyLimiter(),
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ControlIDApi:ConnectionTimeoutSeconds"] = "5",
                ["ControlIDApi:MaxStreamingResponseBytes"] = (512 * 1024).ToString(System.Globalization.CultureInfo.InvariantCulture)
            }).Build());
        await using var destination = new MemoryStream();
        OfficialApiStreamMetadata? metadata = null;

        var result = await invoker.InvokeToStreamAsync(
            new OfficialApiEndpointDefinition
            {
                Id = "binary-download",
                Method = "GET",
                Path = "/binary.fcgi"
            },
            "http://device.local",
            string.Empty,
            string.Empty,
            string.Empty,
            destination,
            (value, _) =>
            {
                metadata = value;
                return ValueTask.CompletedTask;
            },
            TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.Equal(responseBytes.LongLength, result.ResponseBodyLength);
        Assert.Null(result.ResponseBytes);
        Assert.NotNull(metadata);
        Assert.Equal("application/octet-stream", metadata.ContentType);
        Assert.Equal(responseBytes, destination.ToArray());
    }

    [Fact]
    public async Task InvokeToStreamAsync_RejectsKnownOversizedResponseBeforeApplyingHeaders()
    {
        var handler = new RecordingHttpMessageHandler();
        handler.EnqueueResponse(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(new byte[64 * 1024 + 1])
        });
        var invoker = new OfficialApiInvokerService(
            new StaticHttpClientFactory(handler),
            NullLogger<OfficialApiInvokerService>.Instance,
            new ControlIdInputSanitizer(),
            new OfficialApiCircuitBreaker(Microsoft.Extensions.Options.Options.Create(new ControlIdCircuitBreakerOptions { Enabled = false })),
            CreateConcurrencyLimiter(),
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ControlIDApi:ConnectionTimeoutSeconds"] = "5",
                ["ControlIDApi:MaxStreamingResponseBytes"] = (64 * 1024).ToString(System.Globalization.CultureInfo.InvariantCulture)
            }).Build());
        await using var destination = new MemoryStream();
        var headersApplied = false;

        var result = await invoker.InvokeToStreamAsync(
            new OfficialApiEndpointDefinition
            {
                Id = "oversized-download",
                Method = "GET",
                Path = "/large.fcgi"
            },
            "http://device.local",
            string.Empty,
            string.Empty,
            string.Empty,
            destination,
            (_, _) =>
            {
                headersApplied = true;
                return ValueTask.CompletedTask;
            },
            TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Equal(StatusCodes.Status502BadGateway, result.StatusCode);
        Assert.False(headersApplied);
        Assert.Equal(0, destination.Length);
    }

    private static OfficialApiConcurrencyLimiter CreateConcurrencyLimiter()
    {
        return new OfficialApiConcurrencyLimiter(Microsoft.Extensions.Options.Options.Create(new ControlIdConcurrencyOptions()));
    }
}
