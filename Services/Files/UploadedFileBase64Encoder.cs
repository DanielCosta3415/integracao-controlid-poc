using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Services.Files;

public sealed class UploadedFileBase64Encoder
{
    public async Task<string> EncodeAsync(IFormFile? file, string emptyMessage)
    {
        if (file == null || file.Length == 0)
            throw new InvalidOperationException(emptyMessage);

        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);
        return Convert.ToBase64String(memory.ToArray());
    }
}
