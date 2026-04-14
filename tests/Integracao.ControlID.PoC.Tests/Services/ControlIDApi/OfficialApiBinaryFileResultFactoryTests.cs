using System.Text;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;

namespace Integracao.ControlID.PoC.Tests.Services.ControlIDApi;

public class OfficialApiBinaryFileResultFactoryTests
{
    [Fact]
    public void Create_DecodesBase64Payload_WhenResponseIsBinary()
    {
        var factory = new OfficialApiBinaryFileResultFactory();
        var result = new OfficialApiInvocationResult
        {
            ResponseBodyIsBase64 = true,
            ResponseBody = "AQID",
            ResponseContentType = "audio/wav"
        };

        var fileResult = factory.Create(result, "tone.wav", "application/octet-stream");

        Assert.Equal("tone.wav", fileResult.FileDownloadName);
        Assert.Equal("audio/wav", fileResult.ContentType);
        Assert.Equal(new byte[] { 1, 2, 3 }, fileResult.FileContents);
    }

    [Fact]
    public void Create_UsesUtf8Fallback_WhenResponseIsPlainText()
    {
        var factory = new OfficialApiBinaryFileResultFactory();
        var result = new OfficialApiInvocationResult
        {
            ResponseBody = "ok"
        };

        var fileResult = factory.Create(result, "report.txt", "text/plain");

        Assert.Equal("report.txt", fileResult.FileDownloadName);
        Assert.Equal("text/plain", fileResult.ContentType);
        Assert.Equal(Encoding.UTF8.GetBytes("ok"), fileResult.FileContents);
    }
}
