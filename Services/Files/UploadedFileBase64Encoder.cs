using Microsoft.AspNetCore.Http;
using System.Text;

namespace Integracao.ControlID.PoC.Services.Files;

public sealed class UploadedFileBase64Encoder
{
    private const long DefaultMaxBytes = 25L * 1024 * 1024;

    public async Task<string> EncodeAsync(IFormFile? file, string emptyMessage, long maxBytes = DefaultMaxBytes)
    {
        var bytes = await ReadBytesAsync(file, emptyMessage, maxBytes);
        return Convert.ToBase64String(bytes);
    }

    public async Task<string> EncodeValidatedAsync(
        IFormFile? file,
        string emptyMessage,
        long maxBytes,
        UploadedFileValidation validation)
    {
        var bytes = await ReadValidatedBytesAsync(file, emptyMessage, maxBytes, validation);
        return Convert.ToBase64String(bytes);
    }

    public async Task<byte[]> ReadValidatedBytesAsync(
        IFormFile? file,
        string emptyMessage,
        long maxBytes,
        UploadedFileValidation validation)
    {
        var bytes = await ReadBytesAsync(file, emptyMessage, maxBytes);
        validation.Validate(file!, bytes);
        return bytes;
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
        using var memory = new MemoryStream((int)Math.Min(file.Length, maxBytes > 0 ? maxBytes : int.MaxValue));
        var buffer = new byte[81920];
        long totalBytes = 0;
        int read;
        while ((read = await stream.ReadAsync(buffer)) > 0)
        {
            totalBytes += read;
            if (maxBytes > 0 && totalBytes > maxBytes)
                throw new InvalidOperationException($"O arquivo excede o limite de {FormatMegabytes(maxBytes)} MB permitido pela PoC.");

            await memory.WriteAsync(buffer.AsMemory(0, read));
        }

        return memory.ToArray();
    }

    private static string FormatMegabytes(long bytes)
    {
        return (bytes / 1024d / 1024d).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
    }
}

public sealed class UploadedFileValidation
{
    private static readonly string[] OctetStreamFallbacks = ["application/octet-stream", "binary/octet-stream"];

    private readonly HashSet<string> _allowedExtensions;
    private readonly HashSet<string> _allowedContentTypes;
    private readonly Func<byte[], string, bool>? _contentValidator;

    private UploadedFileValidation(
        IEnumerable<string> allowedExtensions,
        IEnumerable<string> allowedContentTypes,
        string invalidMessage,
        Func<byte[], string, bool>? contentValidator)
    {
        _allowedExtensions = allowedExtensions
            .Select(extension => extension.StartsWith('.') ? extension.ToLowerInvariant() : $".{extension.ToLowerInvariant()}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _allowedContentTypes = allowedContentTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        InvalidMessage = invalidMessage;
        _contentValidator = contentValidator;
    }

    public string InvalidMessage { get; }

    public static UploadedFileValidation Png(string invalidMessage)
    {
        return new UploadedFileValidation(
            [".png"],
            ["image/png"],
            invalidMessage,
            static (bytes, _) => HasPrefix(bytes, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]));
    }

    public static UploadedFileValidation JpegOrPng(string invalidMessage)
    {
        return new UploadedFileValidation(
            [".jpg", ".jpeg", ".png"],
            ["image/jpeg", "image/png"],
            invalidMessage,
            static (bytes, _) =>
                HasPrefix(bytes, [0xFF, 0xD8, 0xFF]) ||
                HasPrefix(bytes, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]));
    }

    public static UploadedFileValidation Mp4(string invalidMessage)
    {
        return new UploadedFileValidation(
            [".mp4"],
            ["video/mp4"],
            invalidMessage,
            static (bytes, _) => bytes.Length >= 12 &&
                                 bytes[4] == (byte)'f' &&
                                 bytes[5] == (byte)'t' &&
                                 bytes[6] == (byte)'y' &&
                                 bytes[7] == (byte)'p');
    }

    public static UploadedFileValidation PemCertificate(string invalidMessage)
    {
        return new UploadedFileValidation(
            [".pem", ".crt", ".cer"],
            ["application/x-pem-file", "application/pem-certificate-chain", "text/plain"],
            invalidMessage,
            static (bytes, _) =>
            {
                if (!LooksLikeText(bytes))
                    return false;

                var text = Encoding.ASCII.GetString(bytes);
                return text.Contains("-----BEGIN CERTIFICATE-----", StringComparison.Ordinal) &&
                       text.Contains("-----END CERTIFICATE-----", StringComparison.Ordinal);
            });
    }

    public static UploadedFileValidation OpenVpn(string invalidMessage)
    {
        return new UploadedFileValidation(
            [".conf", ".ovpn", ".zip"],
            ["application/zip", "application/x-zip-compressed", "text/plain"],
            invalidMessage,
            static (bytes, extension) =>
                extension.Equals(".zip", StringComparison.OrdinalIgnoreCase)
                    ? HasPrefix(bytes, [0x50, 0x4B, 0x03, 0x04]) || HasPrefix(bytes, [0x50, 0x4B, 0x05, 0x06])
                    : LooksLikeText(bytes));
    }

    public static UploadedFileValidation Wav(string invalidMessage)
    {
        return new UploadedFileValidation(
            [".wav"],
            ["audio/wav", "audio/x-wav", "audio/wave"],
            invalidMessage,
            static (bytes, _) => bytes.Length >= 12 &&
                                 bytes[0] == (byte)'R' &&
                                 bytes[1] == (byte)'I' &&
                                 bytes[2] == (byte)'F' &&
                                 bytes[3] == (byte)'F' &&
                                 bytes[8] == (byte)'W' &&
                                 bytes[9] == (byte)'A' &&
                                 bytes[10] == (byte)'V' &&
                                 bytes[11] == (byte)'E');
    }

    public void Validate(IFormFile file, byte[] bytes)
    {
        var fileName = Path.GetFileName(file.FileName ?? string.Empty);
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || !_allowedExtensions.Contains(extension))
            throw new InvalidOperationException(InvalidMessage);

        var contentType = NormalizeContentType(file.ContentType);
        if (!string.IsNullOrWhiteSpace(contentType) &&
            !_allowedContentTypes.Contains(contentType) &&
            !OctetStreamFallbacks.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(InvalidMessage);
        }

        if (_contentValidator != null && !_contentValidator(bytes, extension))
            throw new InvalidOperationException(InvalidMessage);
    }

    public string BuildSafeFileName(IFormFile file, string fallbackFileName)
    {
        var fileName = Path.GetFileName(file.FileName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(fileName) ||
            fileName.Length > 120 ||
            fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            return fallbackFileName;
        }

        var extension = Path.GetExtension(fileName);
        return string.IsNullOrWhiteSpace(extension) || !_allowedExtensions.Contains(extension)
            ? fallbackFileName
            : fileName;
    }

    private static string NormalizeContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return string.Empty;

        var separator = contentType.IndexOf(';');
        return (separator >= 0 ? contentType[..separator] : contentType).Trim();
    }

    private static bool HasPrefix(byte[] bytes, byte[] prefix)
    {
        if (bytes.Length < prefix.Length)
            return false;

        for (var index = 0; index < prefix.Length; index++)
        {
            if (bytes[index] != prefix[index])
                return false;
        }

        return true;
    }

    private static bool LooksLikeText(byte[] bytes)
    {
        return bytes.Length > 0 && !bytes.Any(static value => value == 0);
    }
}
