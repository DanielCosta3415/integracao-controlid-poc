using System.Buffers;
using System.Text;

namespace Integracao.ControlID.PoC.Services.ControlIDApi;

internal static class OfficialApiResponseBodyReader
{
    public static bool IsBinaryContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        return !contentType.Contains("json", StringComparison.OrdinalIgnoreCase) &&
               !contentType.Contains("text", StringComparison.OrdinalIgnoreCase) &&
               !contentType.Contains("xml", StringComparison.OrdinalIgnoreCase);
    }

    public static async Task<byte[]> ReadAsync(
        HttpContent content,
        int maxResponseBodyBytes,
        CancellationToken cancellationToken)
    {
        if (content.Headers.ContentLength > maxResponseBodyBytes)
            throw new InvalidDataException("Response Content-Length exceeds the configured limit.");

        await using var stream = await content.ReadAsStreamAsync(cancellationToken);
        using var output = new MemoryStream();
        var buffer = ArrayPool<byte>.Shared.Rent(81920);

        try
        {
            while (true)
            {
                var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (bytesRead == 0)
                    break;

                if (output.Length + bytesRead > maxResponseBodyBytes)
                    throw new InvalidDataException("Chunked response exceeds the configured limit.");

                await output.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            }

            return output.ToArray();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public static async Task<long> CopyToAsync(
        HttpContent content,
        Stream destination,
        long maxResponseBodyBytes,
        CancellationToken cancellationToken)
    {
        if (content.Headers.ContentLength > maxResponseBodyBytes)
            throw new InvalidDataException("Response Content-Length exceeds the configured streaming limit.");

        await using var stream = await content.ReadAsStreamAsync(cancellationToken);
        var buffer = ArrayPool<byte>.Shared.Rent(81920);
        long totalBytes = 0;

        try
        {
            while (true)
            {
                var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (bytesRead == 0)
                    break;

                totalBytes += bytesRead;
                if (totalBytes > maxResponseBodyBytes)
                    throw new InvalidDataException("Chunked response exceeds the configured streaming limit.");

                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            }

            return totalBytes;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public static string DecodeText(HttpContent content, byte[] responseBytes)
    {
        var charset = content.Headers.ContentType?.CharSet?.Trim('"');
        if (string.IsNullOrWhiteSpace(charset))
            return Encoding.UTF8.GetString(responseBytes);

        try
        {
            return Encoding.GetEncoding(charset).GetString(responseBytes);
        }
        catch (ArgumentException)
        {
            return Encoding.UTF8.GetString(responseBytes);
        }
    }
}
