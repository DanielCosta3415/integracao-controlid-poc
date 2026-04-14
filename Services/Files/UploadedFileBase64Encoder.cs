using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Services.Files;

public sealed class UploadedFileBase64Encoder
{
    private const long DefaultMaxBytes = 25L * 1024 * 1024;

    public async Task<string> EncodeAsync(IFormFile? file, string emptyMessage, long maxBytes = DefaultMaxBytes)
    {
        var bytes = await ReadBytesAsync(file, emptyMessage, maxBytes);
        return Convert.ToBase64String(bytes);
    }

    public async Task<byte[]> ReadBytesAsync(IFormFile? file, string emptyMessage, long maxBytes = DefaultMaxBytes)
    {
        if (file == null || file.Length == 0)
            throw new InvalidOperationException(emptyMessage);

        if (maxBytes > 0 && file.Length > maxBytes)
            throw new InvalidOperationException($"O arquivo excede o limite de {FormatMegabytes(maxBytes)} MB permitido pela PoC.");

        // SECURITY: centralizar o limite de upload reduz o risco de DoS por
        // arquivos excessivos e evita que cada controller implemente validacao
        // parcial ou divergente para o mesmo fluxo.
        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream((int)Math.Min(file.Length, int.MaxValue));
        await stream.CopyToAsync(memory);
        return memory.ToArray();
    }

    private static string FormatMegabytes(long bytes)
    {
        return (bytes / 1024d / 1024d).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
    }
}
