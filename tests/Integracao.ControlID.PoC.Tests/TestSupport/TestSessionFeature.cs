using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Integracao.ControlID.PoC.Tests.TestSupport;

public sealed class TestSessionFeature : ISessionFeature
{
    public ISession Session { get; set; } = new TestSession();
}
