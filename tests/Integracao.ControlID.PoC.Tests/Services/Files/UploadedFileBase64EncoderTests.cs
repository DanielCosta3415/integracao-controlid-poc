using System.Text;
using Integracao.ControlID.PoC.Services.Files;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Tests.Services.Files;

public class UploadedFileBase64EncoderTests
{
    [Fact]
    public async Task EncodeAsync_Returns_Base64_For_Valid_File()
    {
        var encoder = new UploadedFileBase64Encoder();
        var file = CreateFormFile("hello");

        var result = await encoder.EncodeAsync(file, "Arquivo obrigatorio.");

        Assert.Equal(Convert.ToBase64String(Encoding.UTF8.GetBytes("hello")), result);
    }

    [Fact]
    public async Task ReadBytesAsync_Rejects_File_Above_The_Configured_Limit()
    {
        var encoder = new UploadedFileBase64Encoder();
        var file = CreateFormFile(new string('a', 8));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => encoder.ReadBytesAsync(file, "Arquivo obrigatorio.", 4));

        Assert.Contains("limite", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EncodeAsync_Rejects_Empty_File()
    {
        var encoder = new UploadedFileBase64Encoder();
        var file = CreateFormFile(string.Empty);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => encoder.EncodeAsync(file, "Arquivo obrigatorio."));

        Assert.Equal("Arquivo obrigatorio.", exception.Message);
    }

    private static IFormFile CreateFormFile(string content)
    {
        return CreateFormFile(Encoding.UTF8.GetBytes(content));
    }

    private static IFormFile CreateFormFile(byte[] bytes)
    {
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", "sample.bin")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/octet-stream"
        };
    }
}
