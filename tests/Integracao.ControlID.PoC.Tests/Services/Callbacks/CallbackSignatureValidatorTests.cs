using System.Net;
using System.Text;
using Integracao.ControlID.PoC.Options;
using Integracao.ControlID.PoC.Services.Callbacks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Tests.Services.Callbacks;

public class CallbackSignatureValidatorTests
{
    [Fact]
    public void Validate_AllowsRequestWhenSignaturesAreDisabled()
    {
        var validator = CreateValidator(new CallbackSecurityOptions());
        var context = CreateContext("/result", "{\"ok\":true}");

        var result = validator.Validate(context.Request, "{\"ok\":true}");

        Assert.True(result.IsAllowed);
    }

    [Fact]
    public void Validate_AcceptsValidHmacSignature()
    {
        var options = CreateRequiredOptions();
        var validator = CreateValidator(options);
        var context = CreateContext("/result", "{\"ok\":true}", "?device_id=1");
        Sign(context.Request, "{\"ok\":true}", validator, options, "nonce-1");

        var result = validator.Validate(context.Request, "{\"ok\":true}");

        Assert.True(result.IsAllowed);
    }

    [Fact]
    public void Validate_RejectsReplayedNonce()
    {
        var options = CreateRequiredOptions();
        var validator = CreateValidator(options);
        var context = CreateContext("/result", "{\"ok\":true}");
        Sign(context.Request, "{\"ok\":true}", validator, options, "nonce-1");

        Assert.True(validator.Validate(context.Request, "{\"ok\":true}").IsAllowed);

        var replay = validator.Validate(context.Request, "{\"ok\":true}");

        Assert.False(replay.IsAllowed);
        Assert.Equal(StatusCodes.Status409Conflict, replay.StatusCode);
    }

    [Fact]
    public void Validate_RejectsReplayedNonceAcrossDifferentCallbackPaths()
    {
        var options = CreateRequiredOptions();
        var validator = CreateValidator(options);
        var first = CreateContext("/result", "{}");
        Sign(first.Request, "{}", validator, options, "shared-nonce");
        Assert.True(validator.Validate(first.Request, "{}").IsAllowed);

        var second = CreateContext("/new_card.fcgi", "{}");
        Sign(second.Request, "{}", validator, options, "shared-nonce");

        var replay = validator.Validate(second.Request, "{}");

        Assert.False(replay.IsAllowed);
        Assert.Equal(StatusCodes.Status409Conflict, replay.StatusCode);
    }

    [Fact]
    public void Validate_FailsSecurelyWhenNonceCapacityIsReached()
    {
        var options = CreateRequiredOptions();
        options.MaxTrackedNonces = 100;
        var validator = CreateValidator(options);

        for (var index = 0; index < options.MaxTrackedNonces; index++)
        {
            var context = CreateContext("/result", "{}");
            Sign(context.Request, "{}", validator, options, $"nonce-{index}");
            Assert.True(validator.Validate(context.Request, "{}").IsAllowed);
        }

        var overflow = CreateContext("/result", "{}");
        Sign(overflow.Request, "{}", validator, options, "nonce-overflow");

        var result = validator.Validate(overflow.Request, "{}");

        Assert.False(result.IsAllowed);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, result.StatusCode);
    }

    [Fact]
    public void Validate_RejectsTamperedBody()
    {
        var options = CreateRequiredOptions();
        var validator = CreateValidator(options);
        var context = CreateContext("/result", "{\"ok\":true}");
        Sign(context.Request, "{\"ok\":true}", validator, options, "nonce-1");

        var result = validator.Validate(context.Request, "{\"ok\":false}");

        Assert.False(result.IsAllowed);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
    }

    [Fact]
    public void Validate_AcceptsSignatureComputedFromExactBinaryBytes()
    {
        var options = CreateRequiredOptions();
        var validator = CreateValidator(options);
        var context = CreateContext("/new_biometric_image.fcgi", string.Empty);
        var body = new byte[] { 0xff, 0xfe, 0x00, 0x80, 0x41 };
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture);
        context.Request.Headers[options.TimestampHeaderName] = timestamp;
        context.Request.Headers[options.NonceHeaderName] = "binary-nonce";
        context.Request.Headers[options.SignatureHeaderName] = validator.ComputeSignature(
            context.Request,
            body,
            timestamp,
            "binary-nonce");

        var result = validator.Validate(context.Request, body);

        Assert.True(result.IsAllowed);
    }

    private static CallbackSecurityOptions CreateRequiredOptions()
    {
        return new CallbackSecurityOptions
        {
            RequireSignedRequests = true,
            SharedKey = "test",
            MaxClockSkewSeconds = 300
        };
    }

    private static CallbackSignatureValidator CreateValidator(CallbackSecurityOptions options)
    {
        return new CallbackSignatureValidator(Microsoft.Extensions.Options.Options.Create(options), NullLogger<CallbackSignatureValidator>.Instance);
    }

    private static DefaultHttpContext CreateContext(string path, string body, string query = "")
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Loopback;
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = path;
        context.Request.QueryString = new QueryString(query);
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        return context;
    }

    private static void Sign(
        HttpRequest request,
        string body,
        CallbackSignatureValidator validator,
        CallbackSecurityOptions options,
        string nonce)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture);
        request.Headers[options.TimestampHeaderName] = timestamp;
        request.Headers[options.NonceHeaderName] = nonce;
        request.Headers[options.SignatureHeaderName] = validator.ComputeSignature(request, body, timestamp, nonce);
    }
}
