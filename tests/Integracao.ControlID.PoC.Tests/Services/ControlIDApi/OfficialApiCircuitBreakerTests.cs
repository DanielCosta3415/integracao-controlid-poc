using Integracao.ControlID.PoC.Options;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Tests.Services.ControlIDApi;

public class OfficialApiCircuitBreakerTests
{
    [Fact]
    public void TryAcquire_OpensCircuitAfterConfiguredFailures()
    {
        var breaker = CreateBreaker(new ControlIdCircuitBreakerOptions
        {
            Enabled = true,
            FailureThreshold = 2,
            BreakDurationSeconds = 60
        });

        Assert.True(breaker.TryAcquire("login", "http://device", out _));

        breaker.RecordFailure("login", "http://device");
        Assert.True(breaker.TryAcquire("login", "http://device", out _));

        breaker.RecordFailure("login", "http://device");

        Assert.False(breaker.TryAcquire("login", "http://device", out var retryAfter));
        Assert.True(retryAfter.TotalSeconds > 0);
    }

    [Fact]
    public void RecordSuccess_ClosesOpenCircuit()
    {
        var breaker = CreateBreaker(new ControlIdCircuitBreakerOptions
        {
            Enabled = true,
            FailureThreshold = 1,
            BreakDurationSeconds = 60
        });

        breaker.RecordFailure("login", "http://device");
        Assert.False(breaker.TryAcquire("login", "http://device", out _));

        breaker.RecordSuccess("login", "http://device");

        Assert.True(breaker.TryAcquire("login", "http://device", out _));
    }

    [Theory]
    [InlineData(StatusCodes.Status408RequestTimeout, true)]
    [InlineData(StatusCodes.Status429TooManyRequests, true)]
    [InlineData(StatusCodes.Status500InternalServerError, true)]
    [InlineData(StatusCodes.Status400BadRequest, false)]
    [InlineData(StatusCodes.Status401Unauthorized, false)]
    public void IsTransientStatusCode_ClassifiesOnlyTransientFailures(int statusCode, bool expected)
    {
        Assert.Equal(expected, OfficialApiCircuitBreaker.IsTransientStatusCode(statusCode));
    }

    private static OfficialApiCircuitBreaker CreateBreaker(ControlIdCircuitBreakerOptions options)
    {
        return new OfficialApiCircuitBreaker(Microsoft.Extensions.Options.Options.Create(options));
    }
}
