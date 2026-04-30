using Integracao.ControlID.PoC.Services.Push;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Tests.Services.Push;

public class PushIdempotencyKeyResolverTests
{
    [Fact]
    public void Resolve_ReturnsSameGuidForSameHeaderKey()
    {
        var resolver = new PushIdempotencyKeyResolver();
        var first = new DefaultHttpContext();
        var second = new DefaultHttpContext();
        first.Request.Headers["Idempotency-Key"] = "same-operation";
        second.Request.Headers["Idempotency-Key"] = "same-operation";

        Assert.Equal(resolver.Resolve(first.Request), resolver.Resolve(second.Request));
    }

    [Fact]
    public void Resolve_PrefersExplicitGuidFromQuery()
    {
        var expected = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString($"?idempotency_key={expected}");
        context.Request.Headers["Idempotency-Key"] = "ignored-header";

        Assert.Equal(expected, new PushIdempotencyKeyResolver().Resolve(context.Request));
    }

    [Fact]
    public void Resolve_IgnoresOversizedKeys()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["Idempotency-Key"] = new string('x', 129);

        Assert.Null(new PushIdempotencyKeyResolver().Resolve(context.Request));
    }
}
