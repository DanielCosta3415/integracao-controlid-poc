using Integracao.ControlID.PoC.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Integracao.ControlID.PoC.Tests.Data;

public sealed class OperationalIndexMigrationTests
{
    [Fact]
    public void Migrate_CreatesOperationalIndexes()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<IntegracaoControlIDContext>()
            .UseSqlite(connection)
            .Options;

        using var context = new IntegracaoControlIDContext(options);

        context.Database.Migrate();

        var indexNames = ReadIndexNames(connection);

        foreach (var expectedIndex in ExpectedOperationalIndexes)
        {
            Assert.Contains(expectedIndex, indexNames);
        }
    }

    private static HashSet<string> ReadIndexNames(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT name
            FROM sqlite_master
            WHERE type = 'index'
              AND name NOT LIKE 'sqlite_autoindex_%';
            """;

        using var reader = command.ExecuteReader();
        var indexNames = new HashSet<string>(StringComparer.Ordinal);

        while (reader.Read())
        {
            indexNames.Add(reader.GetString(0));
        }

        return indexNames;
    }

    private static readonly string[] ExpectedOperationalIndexes =
    {
        "IX_AccessLogs_DeviceId_Time",
        "IX_AccessLogs_Event_Time",
        "IX_AccessLogs_Time",
        "IX_AccessLogs_UserId_Time",
        "IX_BiometricTemplates_UserId_Type",
        "IX_Cards_UserId_Status",
        "IX_ChangeLogs_OperationType_Timestamp",
        "IX_ChangeLogs_TableName_Timestamp",
        "IX_ChangeLogs_Timestamp",
        "IX_Configs_Group_Key",
        "IX_Devices_Ip",
        "IX_Devices_IpAddress",
        "IX_Devices_SerialNumber",
        "IX_Logs_CreatedAt",
        "IX_Logs_Level_CreatedAt",
        "IX_MonitorEvents_DeviceId_ReceivedAt",
        "IX_MonitorEvents_EventType_ReceivedAt",
        "IX_MonitorEvents_ReceivedAt",
        "IX_MonitorEvents_Status_ReceivedAt",
        "IX_Photos_UserId_CreatedAt",
        "IX_PushCommands_CommandType_ReceivedAt",
        "IX_PushCommands_ReceivedAt",
        "IX_PushCommands_Status_DeviceId_CreatedAt",
        "IX_PushCommands_UserId_ReceivedAt",
        "IX_QRCodes_UserId_Status",
        "IX_Sessions_DeviceAddress_IsActive_CreatedAt",
        "IX_Sessions_IsActive_CreatedAt",
        "IX_Sessions_Username_CreatedAt",
        "IX_Syncs_StartedAt",
        "IX_Syncs_Status_StartedAt",
        "IX_Users_Registration",
        "IX_Users_Username",
    };
}
