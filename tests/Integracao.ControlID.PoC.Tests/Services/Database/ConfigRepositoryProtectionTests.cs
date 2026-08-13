using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;

namespace Integracao.ControlID.PoC.Tests.Services.Database;

public sealed class ConfigRepositoryProtectionTests
{
    [Fact]
    public async Task SearchConfigsAsync_FiltersLogicalValueWithoutPersistingPlaintext()
    {
        using var database = new SqliteTestDatabase();
        var repository = new ConfigRepository(database.Context, NullLogger<ConfigRepository>.Instance);

        await repository.AddConfigAsync(new ConfigLocal
        {
            Group = "integration",
            Key = "shared-setting",
            Value = "sensitive-logical-value"
        });

        var matches = await repository.SearchConfigsAsync(value: "sensitive-logical-value");

        var match = Assert.Single(matches);
        Assert.Equal("sensitive-logical-value", match.Value);

        await using var command = database.Connection.CreateCommand();
        command.CommandText = "SELECT Value FROM Configs WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", match.Id);
        var storedValue = Assert.IsType<string>(
            await command.ExecuteScalarAsync(TestContext.Current.CancellationToken));
        Assert.StartsWith(SensitiveDataProtector.ProtectedValuePrefix, storedValue, StringComparison.Ordinal);
        Assert.DoesNotContain("sensitive-logical-value", storedValue, StringComparison.Ordinal);
    }
}
