using System.Text;
using Integracao.ControlID.PoC.Options;
using Integracao.ControlID.PoC.Services.Callbacks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Tests.Services.Callbacks;

public class CallbackRequestBodyReaderTests
{
    [Fact]
    public async Task ReadAsync_ReturnsUtf8Payload_ForJsonRequests()
    {
        var reader = CreateReader();
        var request = CreateRequest("{\"ping\":true}", "application/json");

        var result = await reader.ReadAsync(request);

        Assert.True(result.IsSuccessful);
        Assert.Equal("{\"ping\":true}", result.Body);
        Assert.Equal(0, request.Body.Position);
    }

    [Fact]
    public async Task ReadAsync_ReturnsBase64Payload_ForBinaryRequests()
    {
        var reader = CreateReader();
        var request = CreateBinaryRequest(new byte[] { 1, 2, 3, 4 }, "application/octet-stream");

        var result = await reader.ReadAsync(request);

        Assert.True(result.IsSuccessful);
        Assert.Equal("AQIDBA==", result.Body);
    }

    [Fact]
    public async Task ReadAsync_Rejects_Request_When_StreamExceedsConfiguredLimit()
    {
        var reader = CreateReader(options => options.MaxBodyBytes = 4);
        var request = CreateRequest("payload-too-large", "text/plain");
        request.ContentLength = null;

        var result = await reader.ReadAsync(request);

        Assert.False(result.IsSuccessful);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, result.StatusCode);
    }

    private static CallbackRequestBodyReader CreateReader(Action<CallbackSecurityOptions>? configure = null)
    {
        var options = new CallbackSecurityOptions();
        configure?.Invoke(options);
        return new CallbackRequestBodyReader(Microsoft.Extensions.Options.Options.Create(options));
    }

    private static HttpRequest CreateRequest(string content, string contentType)
    {
        var context = new DefaultHttpContext();
        var bytes = Encoding.UTF8.GetBytes(content);
        context.Request.Body = new MemoryStream(bytes);
        context.Request.ContentLength = bytes.Length;
        context.Request.ContentType = contentType;
        return context.Request;
    }

    private static HttpRequest CreateBinaryRequest(byte[] bytes, string contentType)
    {
        var context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream(bytes);
        context.Request.ContentLength = bytes.Length;
        context.Request.ContentType = contentType;
        return context.Request;
    }
}
