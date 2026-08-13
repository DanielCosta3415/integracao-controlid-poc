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
    public async Task BuildSanitizedContent_CreatesTypedJsonAndRejectsUnknownBodyKind()
    {
        var jsonEndpoint = new OfficialApiEndpointDefinition
        {
            Id = "json-test",
            BodyKind = "json",
            ContentType = "application/json"
        };
        var unknownEndpoint = new OfficialApiEndpointDefinition
        {
            Id = "unknown-test",
            BodyKind = "html",
            ContentType = "text/html"
        };

        using var content = _sanitizer.BuildSanitizedContent(jsonEndpoint, "{\"value\":\"<tag>\"}");

        Assert.NotNull(content);
        Assert.Equal("application/json", content.Headers.ContentType?.MediaType);
        var serialized = await content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var document = System.Text.Json.JsonDocument.Parse(serialized);
        Assert.Equal("<tag>", document.RootElement.GetProperty("value").GetString());
        Assert.DoesNotContain("<tag>", serialized, StringComparison.Ordinal);
        Assert.Throws<InvalidOperationException>(() => _sanitizer.BuildSanitizedContent(unknownEndpoint, "<script>"));
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

    [Fact]
    public void TryNormalizeBaseAddress_RejectsHttpWhenHttpsIsRequired()
    {
        var sanitizer = new ControlIdInputSanitizer(Microsoft.Extensions.Options.Options.Create(new ControlIdEgressOptions
        {
            RequireHttpsDeviceUrls = true
        }));

        var success = sanitizer.TryNormalizeBaseAddress(
            "http://controlid.local",
            "http",
            null,
            out _,
            out var errorMessage);

        Assert.False(success);
        Assert.Contains("HTTPS", errorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalizeAdditionalQuery_RemainsSafeAcrossDeterministicAdversarialInputs()
    {
        var random = new Random(20260804);
        const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 %&=+?#\\\r\n<>\"'";

        for (var iteration = 0; iteration < 500; iteration++)
        {
            var length = random.Next(0, 160);
            var input = new string(Enumerable.Range(0, length)
                .Select(_ => alphabet[random.Next(alphabet.Length)])
                .ToArray());

            try
            {
                var normalized = _sanitizer.NormalizeAdditionalQuery(input);
                Assert.DoesNotContain('\r', normalized);
                Assert.DoesNotContain('\n', normalized);
                Assert.DoesNotContain(' ', normalized);
                Assert.DoesNotContain('#', normalized);
            }
            catch (InvalidOperationException)
            {
                // Rejeição explícita também é um resultado seguro para entradas sem estrutura válida.
            }
        }
    }

    [Theory]
    [InlineData("http://127.0.0.1:80\r\nX-Injected: yes")]
    [InlineData("http://[::1]garbage")]
    [InlineData("file:///etc/passwd")]
    [InlineData("javascript:alert(1)")]
    public void NormalizeDeviceAddress_RejectsMalformedOrUnsafeAddresses(string address)
    {
        Assert.Throws<InvalidOperationException>(() => _sanitizer.NormalizeDeviceAddress(address));
    }
}
