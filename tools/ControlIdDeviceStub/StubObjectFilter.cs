using System.Text.Json.Nodes;

internal static class StubObjectFilter
{
    public static bool Matches(JsonObject? item, JsonObject? where)
    {
        if (item == null)
            return false;

        if (where == null || where.Count == 0)
            return true;

        return where.All(property => MatchesCondition(item[property.Key], property.Value));
    }

    private static bool MatchesCondition(JsonNode? existing, JsonNode? requested)
    {
        if (requested is not JsonObject operators)
        {
            return string.Equals(
                existing?.ToJsonString() ?? string.Empty,
                requested?.ToJsonString() ?? string.Empty,
                StringComparison.Ordinal);
        }

        foreach (var operation in operators)
        {
            var existingNumber = TryGetInt64(existing);
            var requestedNumber = TryGetInt64(operation.Value);
            if (!existingNumber.HasValue || !requestedNumber.HasValue)
                return false;

            var matches = operation.Key switch
            {
                ">" => existingNumber > requestedNumber,
                ">=" => existingNumber >= requestedNumber,
                "<" => existingNumber < requestedNumber,
                "<=" => existingNumber <= requestedNumber,
                "!=" or "<>" => existingNumber != requestedNumber,
                "=" or "==" => existingNumber == requestedNumber,
                _ => false
            };

            if (!matches)
                return false;
        }

        return true;
    }

    private static long? TryGetInt64(JsonNode? node)
    {
        return node switch
        {
            JsonValue value when value.TryGetValue<long>(out var int64Value) => int64Value,
            JsonValue value when value.TryGetValue<int>(out var int32Value) => int32Value,
            JsonValue value when value.TryGetValue<string>(out var text) && long.TryParse(text, out var parsed) => parsed,
            _ => null
        };
    }
}
