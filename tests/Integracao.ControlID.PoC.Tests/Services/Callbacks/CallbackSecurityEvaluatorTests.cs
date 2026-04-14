using System.Net;
using Integracao.ControlID.PoC.Options;
using Integracao.ControlID.PoC.Services.Callbacks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Tests.Services.Callbacks;

public class CallbackSecurityEvaluatorTests
{
    [Fact]
    public void Evaluate_Allows_Request_When_DefaultPolicyIsUsed()
    {
        var evaluator = CreateEvaluator();
        var context = CreateHttpContext("10.10.10.10");

        var result = evaluator.Evaluate(context);

        Assert.True(result.IsAllowed);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    [Fact]
    public void Evaluate_Rejects_Request_When_SharedKeyIsMissing()
    {
        var evaluator = CreateEvaluator(options =>
        {
            options.RequireSharedKey = true;
            options.SharedKey = "secret-key";
        });

        var context = CreateHttpContext("10.10.10.10");

        var result = evaluator.Evaluate(context);

        Assert.False(result.IsAllowed);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
    }

    [Fact]
    public void Evaluate_Rejects_Request_When_RemoteIpIsNotAllowed()
    {
        var evaluator = CreateEvaluator(options =>
        {
            options.AllowedRemoteIps.Add("192.168.0.10");
            options.AllowLoopback = false;
        });

        var context = CreateHttpContext("192.168.0.11");

        var result = evaluator.Evaluate(context);

        Assert.False(result.IsAllowed);
        Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
    }

    [Fact]
    public void Evaluate_Allows_Loopback_When_AllowLoopbackIsEnabled()
    {
        var evaluator = CreateEvaluator(options =>
        {
            options.AllowedRemoteIps.Add("192.168.0.10");
            options.AllowLoopback = true;
        });

        var context = CreateHttpContext(IPAddress.Loopback.ToString());

        var result = evaluator.Evaluate(context);

        Assert.True(result.IsAllowed);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    [Fact]
    public void Evaluate_Rejects_Request_When_ContentLengthExceedsLimit()
    {
        var evaluator = CreateEvaluator(options => options.MaxBodyBytes = 32);
        var context = CreateHttpContext("10.10.10.10", contentLength: 64);

        var result = evaluator.Evaluate(context);

        Assert.False(result.IsAllowed);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, result.StatusCode);
    }

    private static CallbackSecurityEvaluator CreateEvaluator(Action<CallbackSecurityOptions>? configure = null)
    {
        var options = new CallbackSecurityOptions();
        configure?.Invoke(options);
        return new CallbackSecurityEvaluator(Microsoft.Extensions.Options.Options.Create(options));
    }

    private static DefaultHttpContext CreateHttpContext(string remoteIp, long? contentLength = null)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(remoteIp);
        context.Request.ContentLength = contentLength;
        return context;
    }
}
