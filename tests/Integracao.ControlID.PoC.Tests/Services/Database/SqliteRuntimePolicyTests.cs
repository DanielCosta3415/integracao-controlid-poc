using Integracao.ControlID.PoC.Data;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Options;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Tests.Services.Database;

public sealed class SqliteRuntimePolicyTests
{
    [Fact]
    public async Task ApplyAsync_EnablesWalForFileDatabase()
    {
        var databasePath = BuildTemporaryDatabasePath();
        try
        {
            await using var context = CreateContext(databasePath);
            await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
            var policy = new SqliteRuntimePolicy(Microsoft.Extensions.Options.Options.Create(new SqliteRuntimeOptions()));

            var journalMode = await policy.ApplyAsync(context, TestContext.Current.CancellationToken);

            Assert.Equal("wal", journalMode, ignoreCase: true);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task ConcurrentWriters_CompleteWithoutLockedDatabaseErrors()
    {
        var databasePath = BuildTemporaryDatabasePath();
        try
        {
            await using (var setup = CreateContext(databasePath))
            {
                await setup.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
                var policy = new SqliteRuntimePolicy(Microsoft.Extensions.Options.Options.Create(new SqliteRuntimeOptions()));
                await policy.ApplyAsync(setup, TestContext.Current.CancellationToken);
            }

            var writes = Enumerable.Range(1, 20).Select(async index =>
            {
                await using var context = CreateContext(databasePath);
                context.Configs.Add(new ConfigLocal
                {
                    Group = "concurrency",
                    Key = $"key-{index}",
                    Value = index.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    CreatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            });

            await Task.WhenAll(writes);

            await using var verification = CreateContext(databasePath);
            Assert.Equal(20, await verification.Configs.CountAsync(TestContext.Current.CancellationToken));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    private static IntegracaoControlIDContext CreateContext(string databasePath)
    {
        var options = new DbContextOptionsBuilder<IntegracaoControlIDContext>()
            .UseSqlite($"Data Source={databasePath};Default Timeout=5;Foreign Keys=True;Pooling=False")
            .Options;
        return new IntegracaoControlIDContext(options, TestDataProtection.CreateSensitiveDataProtector());
    }

    private static string BuildTemporaryDatabasePath()
    {
        return Path.Combine(Path.GetTempPath(), $"controlid-sqlite-{Guid.NewGuid():N}.db");
    }

    private static void DeleteDatabaseFiles(string databasePath)
    {
        foreach (var suffix in new[] { string.Empty, "-wal", "-shm" })
        {
            var path = databasePath + suffix;
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
