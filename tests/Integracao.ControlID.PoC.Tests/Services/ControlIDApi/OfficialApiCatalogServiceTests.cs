using Integracao.ControlID.PoC.Services.ControlIDApi;

namespace Integracao.ControlID.PoC.Tests.Services.ControlIDApi;

public sealed class OfficialApiCatalogServiceTests
{
    [Fact]
    public void Catalog_ExposesUniqueEndpointsWithoutMojibake()
    {
        var service = new OfficialApiCatalogService();
        var endpoints = service.GetAll();

        Assert.Equal(96, endpoints.Count);
        Assert.Equal(endpoints.Count, endpoints.Select(endpoint => endpoint.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.Contains(endpoints, endpoint => endpoint.Category == "Sess\u00E3o");

        foreach (var endpoint in endpoints)
        {
            var searchableText = string.Join(
                "|",
                endpoint.Category,
                endpoint.Title,
                endpoint.Summary,
                endpoint.Notes,
                endpoint.SamplePayload);

            Assert.DoesNotContain("\u00C3\u00A3", searchableText, StringComparison.Ordinal);
            Assert.DoesNotContain("\u00C3\u00A7", searchableText, StringComparison.Ordinal);
            Assert.DoesNotContain("\u00C3\u00A9", searchableText, StringComparison.Ordinal);
            Assert.DoesNotContain('\uFFFD', searchableText);
        }
    }
}
