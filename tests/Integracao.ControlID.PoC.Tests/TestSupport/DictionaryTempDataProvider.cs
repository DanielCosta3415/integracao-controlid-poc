using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Integracao.ControlID.PoC.Tests.TestSupport;

public sealed class DictionaryTempDataProvider : ITempDataProvider
{
    private readonly Dictionary<string, object?> _data = new(StringComparer.OrdinalIgnoreCase);

    public IDictionary<string, object?> LoadTempData(HttpContext context)
    {
        return _data;
    }

    public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
    {
        _data.Clear();
        foreach (var pair in values)
        {
            _data[pair.Key] = pair.Value;
        }
    }
}
