using System.Text.Json;
using Integracao.ControlID.PoC.ViewModels.OfficialApi;

namespace Integracao.ControlID.PoC.Services.ControlIDApi
{
    internal static class OfficialApiParameterDocumentationUtilities
    {
        public static OfficialApiParameterDocViewModel ToDoc(OfficialApiParameterSeed field)
        {
            return new OfficialApiParameterDocViewModel
            {
                Path = field.Path,
                TypeLabel = field.TypeLabel,
                RequirementLabel = field.RequirementLabel,
                Description = field.Description,
                Example = field.Example,
                SourceLabel = field.Explicit ? "Especificado pela PoC" : "Inferido do exemplo",
                Depth = field.Path.Count(character => character == '.')
            };
        }

        public static IList<OfficialApiParameterDocViewModel> Deduplicate(IEnumerable<OfficialApiParameterDocViewModel> items)
        {
            return items
                .GroupBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static string BuildExample(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString() ?? string.Empty,
                JsonValueKind.Number => value.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => "null",
                _ => value.GetRawText()
            };
        }

        public static string InferElementType(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.Object => "object",
                JsonValueKind.Array => value.EnumerateArray().FirstOrDefault().ValueKind switch
                {
                    JsonValueKind.Object => "array<object>",
                    JsonValueKind.String => "array<string>",
                    JsonValueKind.Number => "array<number>",
                    JsonValueKind.True or JsonValueKind.False => "array<boolean>",
                    _ => "array"
                },
                JsonValueKind.String => "string",
                JsonValueKind.Number => value.GetRawText().Contains('.') ? "decimal" : "integer",
                JsonValueKind.True or JsonValueKind.False => "boolean",
                JsonValueKind.Null => "null",
                _ => "json"
            };
        }

        public static string InferType(string example)
        {
            if (bool.TryParse(example, out _))
            {
                return "boolean";
            }

            if (long.TryParse(example.Replace("..", string.Empty), out _))
            {
                return "integer";
            }

            return "string";
        }
    }
}
