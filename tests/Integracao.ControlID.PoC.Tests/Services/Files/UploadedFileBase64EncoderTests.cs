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

    [Fact]
    public async Task EncodeValidatedAsync_Accepts_Png_With_Matching_Signature()
    {
        var encoder = new UploadedFileBase64Encoder();
        var file = CreateFormFile(
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00],
            "logo.png",
            "image/png");

        var result = await encoder.EncodeValidatedAsync(
            file,
            "Arquivo obrigatorio.",
            1024,
            UploadedFileValidation.Png("PNG invalido."));

        Assert.Equal(Convert.ToBase64String(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00 }), result);
    }

    [Fact]
    public async Task EncodeValidatedAsync_Rejects_Spoofed_Image_Content()
    {
        var encoder = new UploadedFileBase64Encoder();
        var file = CreateFormFile(Encoding.UTF8.GetBytes("<script>alert(1)</script>"), "logo.png", "image/png");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            encoder.EncodeValidatedAsync(
                file,
                "Arquivo obrigatorio.",
                1024,
                UploadedFileValidation.Png("PNG invalido.")));

        Assert.Equal("PNG invalido.", exception.Message);
    }

    [Fact]
    public async Task EncodeValidatedAsync_Rejects_PublicKey_AsCertificate()
    {
        var encoder = new UploadedFileBase64Encoder();
        var file = CreateFormFile(
            Encoding.ASCII.GetBytes("-----BEGIN PUBLIC KEY-----\nabc\n-----END PUBLIC KEY-----"),
            "cert.pem",
            "text/plain");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            encoder.EncodeValidatedAsync(
                file,
                "Arquivo obrigatorio.",
                1024,
                UploadedFileValidation.PemCertificate("Certificado invalido.")));

        Assert.Equal("Certificado invalido.", exception.Message);
    }

    [Fact]
    public async Task EncodeValidatedAsync_Accepts_Mp4_With_Ftyp_Box()
    {
        var encoder = new UploadedFileBase64Encoder();
        var bytes = new byte[] { 0, 0, 0, 24, 102, 116, 121, 112, 105, 115, 111, 109, 0, 0, 2, 0 };
        var file = CreateFormFile(bytes, "ad.mp4", "video/mp4");

        var result = await encoder.EncodeValidatedAsync(
            file,
            "Arquivo obrigatorio.",
            1024,
            UploadedFileValidation.Mp4("Video invalido."));

        Assert.Equal(Convert.ToBase64String(bytes), result);
    }

    private static IFormFile CreateFormFile(string content)
    {
        return CreateFormFile(Encoding.UTF8.GetBytes(content));
    }

    private static IFormFile CreateFormFile(byte[] bytes)
    {
        return CreateFormFile(bytes, "sample.bin", "application/octet-stream");
    }

    private static IFormFile CreateFormFile(byte[] bytes, string fileName, string contentType)
    {
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
