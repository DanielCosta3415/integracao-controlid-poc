using System.Globalization;
using System.Text.Json.Nodes;

internal static class StubDatasetFactory
{
    public static IReadOnlyList<int> SupportedSizes { get; } = [1, 100, 1_000, 10_000, 100_000];

    public static void ValidateSize(int size)
    {
        if (!SupportedSizes.Contains(size))
            throw new ArgumentOutOfRangeException(nameof(size), $"Volume nao suportado: {size}.");
    }

    public static void Populate(StubState state, int size)
    {
        ValidateSize(size);
        if (size <= 1)
            return;

        const int batchSize = 1_000;
        for (var start = 2; start <= size; start += batchSize)
        {
            var end = Math.Min(size, start + batchSize - 1);
            var users = new JsonArray();
            var cards = new JsonArray();
            var qrCodes = new JsonArray();

            for (var id = start; id <= end; id++)
            {
                users.Add(new JsonObject
                {
                    ["id"] = id,
                    ["registration"] = id.ToString("D8", CultureInfo.InvariantCulture),
                    ["name"] = $"Usuario ficticio {id:D8}",
                    ["email"] = $"usuario{id:D8}@example.invalid",
                    ["status"] = "active",
                    ["user_type_id"] = 1
                });
                cards.Add(new JsonObject
                {
                    ["id"] = id,
                    ["user_id"] = id,
                    ["value"] = $"CARD{id:D12}",
                    ["type"] = 1,
                    ["status"] = "active"
                });
                qrCodes.Add(new JsonObject
                {
                    ["id"] = id,
                    ["user_id"] = id,
                    ["value"] = $"QR-{id:D12}",
                    ["begin_time"] = 1704067200,
                    ["end_time"] = 1893456000
                });
            }

            state.CreateObjects(new JsonObject { ["object"] = "users", ["values"] = users });
            state.CreateObjects(new JsonObject { ["object"] = "cards", ["values"] = cards });
            state.CreateObjects(new JsonObject { ["object"] = "qrcodes", ["values"] = qrCodes });
        }
    }

    public static object CreateImageInventory(int size, long timestamp)
    {
        var userIds = Enumerable.Range(1, size).Select(static id => (long)id).ToArray();
        var imageInfo = userIds.Select(id => new { user_id = id, timestamp }).ToArray();
        return new { user_ids = userIds, image_info = imageInfo };
    }
}
