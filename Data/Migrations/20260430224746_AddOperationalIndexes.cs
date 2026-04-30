using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Integracao.ControlID.PoC.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var statement in CreateIndexStatements)
            {
                migrationBuilder.Sql(statement);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var indexName in IndexNames)
            {
                migrationBuilder.Sql($"DROP INDEX IF EXISTS {indexName};");
            }
        }

        private static readonly string[] CreateIndexStatements =
        {
            "CREATE INDEX IF NOT EXISTS IX_Users_Registration ON Users (Registration);",
            "CREATE INDEX IF NOT EXISTS IX_Users_Username ON Users (Username);",
            "CREATE INDEX IF NOT EXISTS IX_Syncs_StartedAt ON Syncs (StartedAt);",
            "CREATE INDEX IF NOT EXISTS IX_Syncs_Status_StartedAt ON Syncs (Status, StartedAt);",
            "CREATE INDEX IF NOT EXISTS IX_Sessions_DeviceAddress_IsActive_CreatedAt ON Sessions (DeviceAddress, IsActive, CreatedAt);",
            "CREATE INDEX IF NOT EXISTS IX_Sessions_IsActive_CreatedAt ON Sessions (IsActive, CreatedAt);",
            "CREATE INDEX IF NOT EXISTS IX_Sessions_Username_CreatedAt ON Sessions (Username, CreatedAt);",
            "CREATE INDEX IF NOT EXISTS IX_QRCodes_UserId_Status ON QRCodes (UserId, Status);",
            "CREATE INDEX IF NOT EXISTS IX_PushCommands_CommandType_ReceivedAt ON PushCommands (CommandType, ReceivedAt);",
            "CREATE INDEX IF NOT EXISTS IX_PushCommands_ReceivedAt ON PushCommands (ReceivedAt);",
            "CREATE INDEX IF NOT EXISTS IX_PushCommands_Status_DeviceId_CreatedAt ON PushCommands (Status, DeviceId, CreatedAt);",
            "CREATE INDEX IF NOT EXISTS IX_PushCommands_UserId_ReceivedAt ON PushCommands (UserId, ReceivedAt);",
            "CREATE INDEX IF NOT EXISTS IX_Photos_UserId_CreatedAt ON Photos (UserId, CreatedAt);",
            "CREATE INDEX IF NOT EXISTS IX_MonitorEvents_DeviceId_ReceivedAt ON MonitorEvents (DeviceId, ReceivedAt);",
            "CREATE INDEX IF NOT EXISTS IX_MonitorEvents_EventType_ReceivedAt ON MonitorEvents (EventType, ReceivedAt);",
            "CREATE INDEX IF NOT EXISTS IX_MonitorEvents_ReceivedAt ON MonitorEvents (ReceivedAt);",
            "CREATE INDEX IF NOT EXISTS IX_MonitorEvents_Status_ReceivedAt ON MonitorEvents (Status, ReceivedAt);",
            "CREATE INDEX IF NOT EXISTS IX_Logs_CreatedAt ON Logs (CreatedAt);",
            "CREATE INDEX IF NOT EXISTS IX_Logs_Level_CreatedAt ON Logs (Level, CreatedAt);",
            "CREATE INDEX IF NOT EXISTS IX_Devices_Ip ON Devices (Ip);",
            "CREATE INDEX IF NOT EXISTS IX_Devices_IpAddress ON Devices (IpAddress);",
            "CREATE INDEX IF NOT EXISTS IX_Devices_SerialNumber ON Devices (SerialNumber);",
            "CREATE INDEX IF NOT EXISTS IX_Configs_Group_Key ON Configs (\"Group\", \"Key\");",
            "CREATE INDEX IF NOT EXISTS IX_ChangeLogs_OperationType_Timestamp ON ChangeLogs (OperationType, Timestamp);",
            "CREATE INDEX IF NOT EXISTS IX_ChangeLogs_TableName_Timestamp ON ChangeLogs (TableName, Timestamp);",
            "CREATE INDEX IF NOT EXISTS IX_ChangeLogs_Timestamp ON ChangeLogs (Timestamp);",
            "CREATE INDEX IF NOT EXISTS IX_Cards_UserId_Status ON Cards (UserId, Status);",
            "CREATE INDEX IF NOT EXISTS IX_BiometricTemplates_UserId_Type ON BiometricTemplates (UserId, Type);",
            "CREATE INDEX IF NOT EXISTS IX_AccessLogs_DeviceId_Time ON AccessLogs (DeviceId, Time);",
            "CREATE INDEX IF NOT EXISTS IX_AccessLogs_Event_Time ON AccessLogs (Event, Time);",
            "CREATE INDEX IF NOT EXISTS IX_AccessLogs_Time ON AccessLogs (Time);",
            "CREATE INDEX IF NOT EXISTS IX_AccessLogs_UserId_Time ON AccessLogs (UserId, Time);",
        };

        private static readonly string[] IndexNames =
        {
            "IX_Users_Registration",
            "IX_Users_Username",
            "IX_Syncs_StartedAt",
            "IX_Syncs_Status_StartedAt",
            "IX_Sessions_DeviceAddress_IsActive_CreatedAt",
            "IX_Sessions_IsActive_CreatedAt",
            "IX_Sessions_Username_CreatedAt",
            "IX_QRCodes_UserId_Status",
            "IX_PushCommands_CommandType_ReceivedAt",
            "IX_PushCommands_ReceivedAt",
            "IX_PushCommands_Status_DeviceId_CreatedAt",
            "IX_PushCommands_UserId_ReceivedAt",
            "IX_Photos_UserId_CreatedAt",
            "IX_MonitorEvents_DeviceId_ReceivedAt",
            "IX_MonitorEvents_EventType_ReceivedAt",
            "IX_MonitorEvents_ReceivedAt",
            "IX_MonitorEvents_Status_ReceivedAt",
            "IX_Logs_CreatedAt",
            "IX_Logs_Level_CreatedAt",
            "IX_Devices_Ip",
            "IX_Devices_IpAddress",
            "IX_Devices_SerialNumber",
            "IX_Configs_Group_Key",
            "IX_ChangeLogs_OperationType_Timestamp",
            "IX_ChangeLogs_TableName_Timestamp",
            "IX_ChangeLogs_Timestamp",
            "IX_Cards_UserId_Status",
            "IX_BiometricTemplates_UserId_Type",
            "IX_AccessLogs_DeviceId_Time",
            "IX_AccessLogs_Event_Time",
            "IX_AccessLogs_Time",
            "IX_AccessLogs_UserId_Time",
        };
    }
}
