using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Options;
using Integracao.ControlID.PoC.Services.Security;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Tests.Services.Security;

public class ControlIdInputSanitizerTests
{
    private readonly ControlIdInputSanitizer _sanitizer = new();

    [Fact]
    public void TryNormalizeBaseAddress_RejectsEmbeddedCredentials()
    {
        var success = _sanitizer.TryNormalizeBaseAddress(
            "admin:admin@192.168.0.10",
            "http",
            80,
            out var normalizedAddress,
            out var errorMessage);

        Assert.False(success);
        Assert.Equal(string.Empty, normalizedAddress);
        Assert.Contains("credenciais", errorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalizeAdditionalQuery_ReencodesSegmentsSafely()
    {
        var normalized = _sanitizer.NormalizeAdditionalQuery(" user_id = 15 & event = acesso liberado ");

        Assert.Equal("user_id=15&event=acesso%20liberado", normalized);
    }

    [Fact]
    public void BuildSanitizedContent_RejectsInvalidBinaryPayload()
    {
        var endpoint = new OfficialApiEndpointDefinition
        {
            Id = "binary-test",
            BodyKind = "binary",
            ContentType = "application/octet-stream"
        };

        var exception = Assert.Throws<InvalidOperationException>(() => _sanitizer.BuildSanitizedContent(endpoint, "%%%"));
        Assert.Contains("base64", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryNormalizeBaseAddress_RejectsHostOutsideConfiguredAllowlist()
    {
        var sanitizer = new ControlIdInputSanitizer(Microsoft.Extensions.Options.Options.Create(new ControlIdEgressOptions
        {
            RequireAllowedDeviceHosts = true,
            AllowedDeviceHosts = ["controlid.local"]
        }));

        var success = sanitizer.TryNormalizeBaseAddress(
            "192.168.0.10",
            "http",
            null,
            out _,
            out var errorMessage);

        Assert.False(success);
        Assert.Contains("allowlist", errorMessage, StringComparison.OrdinalIgnoreCase);
    }
}
