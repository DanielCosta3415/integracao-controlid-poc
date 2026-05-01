namespace Integracao.ControlID.PoC.Tests.TestSupport;

public sealed class StaticHttpClientFactory : IHttpClientFactory
{
    private readonly HttpMessageHandler _handler;

    public StaticHttpClientFactory(HttpMessageHandler handler)
    {
        _handler = handler;
    }

    public HttpClient CreateClient(string name)
    {
        return new HttpClient(_handler, disposeHandler: false);
    }
}
