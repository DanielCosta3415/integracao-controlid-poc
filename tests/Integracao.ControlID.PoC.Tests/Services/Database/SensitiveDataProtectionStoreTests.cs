using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;

namespace Integracao.ControlID.PoC.Tests.Services.Database;

public sealed class SensitiveDataProtectionStoreTests
{
    [Fact]
    public async Task SaveAndLoad_ProtectsSessionTokenAtRestAndRestoresApplicationValue()
    {
        using var database = new SqliteTestDatabase();
        database.Context.Sessions.Add(new SessionLocal
        {
            DeviceAddress = "https://device.local",
            SessionString = "sensitive-session",
            DeviceName = "device",
            DeviceSerial = "serial",
            Username = "operator",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        await database.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        await using var command = database.Connection.CreateCommand();
        command.CommandText = "SELECT SessionString FROM Sessions LIMIT 1;";
        var storedValue = Assert.IsType<string>(
            await command.ExecuteScalarAsync(TestContext.Current.CancellationToken));

        Assert.StartsWith(SensitiveDataProtector.ProtectedValuePrefix, storedValue, StringComparison.Ordinal);
        Assert.DoesNotContain("sensitive-session", storedValue, StringComparison.Ordinal);

        database.Context.ChangeTracker.Clear();
        var session = await database.Context.Sessions.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal("sensitive-session", session.SessionString);
    }

    [Fact]
    public async Task ProtectLegacyValues_ConvertsPlaintextWithoutChangingLogicalValue()
    {
        using var database = new SqliteTestDatabase();
        await database.Context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO Sessions
                (DeviceAddress, SessionString, DeviceName, DeviceSerial, Username, CreatedAt, IsActive)
            VALUES
                ('https://device.local', 'legacy-session', 'device', 'serial', 'operator', CURRENT_TIMESTAMP, 1);
            """,
            TestContext.Current.CancellationToken);
        var store = new SensitiveDataProtectionStore(database.Context, database.SensitiveDataProtector);

        Assert.Equal(1, await store.CountUnprotectedValuesAsync(TestContext.Current.CancellationToken));

        var protectedCount = await store.ProtectLegacyValuesAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, protectedCount);
        Assert.Equal(0, await store.CountUnprotectedValuesAsync(TestContext.Current.CancellationToken));
        database.Context.ChangeTracker.Clear();
        var session = await database.Context.Sessions.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal("legacy-session", session.SessionString);
    }

    [Fact]
    public async Task ProtectLegacyValues_ProcessesMoreThanTwoBatchesWithoutSkippingRows()
    {
        using var database = new SqliteTestDatabase();
        const int rowCount = 205;

        using (var transaction = database.Connection.BeginTransaction())
        await using (var command = database.Connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO Sessions
                    (DeviceAddress, SessionString, DeviceName, DeviceSerial, Username, CreatedAt, IsActive)
                VALUES
                    ('https://device.local', @session, 'device', 'serial', 'operator', CURRENT_TIMESTAMP, 1);
                """;
            var sessionParameter = command.CreateParameter();
            sessionParameter.ParameterName = "@session";
            command.Parameters.Add(sessionParameter);

            for (var index = 0; index < rowCount; index++)
            {
                sessionParameter.Value = $"legacy-session-{index}";
                await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
            }

            await transaction.CommitAsync(TestContext.Current.CancellationToken);
        }

        var store = new SensitiveDataProtectionStore(database.Context, database.SensitiveDataProtector);

        Assert.True(await store.HasUnprotectedValuesAsync(TestContext.Current.CancellationToken));
        Assert.Equal(rowCount, await store.ProtectLegacyValuesAsync(TestContext.Current.CancellationToken));
        Assert.False(await store.HasUnprotectedValuesAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0, await store.CountUnprotectedValuesAsync(TestContext.Current.CancellationToken));
    }
}
