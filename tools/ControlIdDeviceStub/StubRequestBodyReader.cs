using System.Buffers;
using System.Text.Json.Nodes;

internal static class StubRequestBodyReader
{
    private const int MaxRequestBodyBytes = 32 * 1024 * 1024;

    public static async Task<JsonNode?> ReadJsonAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        if (request.ContentLength is 0)
            return null;

        if (request.ContentLength > MaxRequestBodyBytes)
            throw new BadHttpRequestException("O corpo excede o limite do simulador.", StatusCodes.Status413PayloadTooLarge);

        await using var output = new MemoryStream();
        var buffer = ArrayPool<byte>.Shared.Rent(81920);
        try
        {
            while (true)
            {
                var read = await request.Body.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (read == 0)
                    break;

                if (output.Length + read > MaxRequestBodyBytes)
                    throw new BadHttpRequestException("O corpo excede o limite do simulador.", StatusCodes.Status413PayloadTooLarge);

                await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            }

            if (output.Length == 0)
                return null;

            try
            {
                return JsonNode.Parse(output.GetBuffer().AsSpan(0, checked((int)output.Length)));
            }
            catch (System.Text.Json.JsonException)
            {
                return null;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }
    }
}
