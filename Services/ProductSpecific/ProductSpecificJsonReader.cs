using System.Text.Json;

namespace Integracao.ControlID.PoC.Services.ProductSpecific;

public sealed class ProductSpecificJsonReader
{
    public string GetConfigString(JsonElement root, string section, string field, string fallback = "")
    {
        if (root.TryGetProperty(section, out var sectionElement) &&
            sectionElement.ValueKind == JsonValueKind.Object &&
            sectionElement.TryGetProperty(field, out var fieldElement))
        {
            return fieldElement.ToString() ?? fallback;
        }

        return fallback;
    }

    public bool GetConfigBool(JsonElement root, string section, string field, bool fallback = false)
    {
        var value = GetConfigString(root, section, field, fallback ? "1" : "0");
        return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    public int GetConfigInt(JsonElement root, string section, string field, int fallback)
    {
        var value = GetConfigString(root, section, field, fallback.ToString());
        return int.TryParse(value, out var parsed) ? parsed : fallback;
    }

    public bool GetRootBool(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var value))
            return false;

        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => value.TryGetInt32(out var number) && number != 0,
            JsonValueKind.String => value.GetString() is string text && (text == "1" || text.Equals("true", StringComparison.OrdinalIgnoreCase)),
            _ => false
        };
    }

    public int GetRootInt(JsonElement root, string name, int fallback)
    {
        if (!root.TryGetProperty(name, out var value))
            return fallback;

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var numeric))
            return numeric;

        return value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var parsed) ? parsed : fallback;
    }
}
