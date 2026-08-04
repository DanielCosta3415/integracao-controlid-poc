using System.Text.Json;

namespace Integracao.ControlID.PoC.Models.ControlIDApi;

public sealed class OfficialApiJsonPayload
{
    public OfficialApiJsonPayload(JsonElement rootElement)
    {
        RootElement = rootElement;
    }

    public JsonElement RootElement { get; }
}
