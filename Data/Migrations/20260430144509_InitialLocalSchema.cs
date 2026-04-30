using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Integracao.ControlID.PoC.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialLocalSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS "AccessLogs" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_AccessLogs" PRIMARY KEY AUTOINCREMENT,
                    "Time" TEXT NOT NULL,
                    "Event" INTEGER NOT NULL,
                    "DeviceId" INTEGER NULL,
                    "UserId" INTEGER NULL,
                    "PortalId" INTEGER NULL,
                    "Info" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS "AccessRules" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_AccessRules" PRIMARY KEY AUTOINCREMENT,
                    "Name" TEXT NOT NULL,
                    "Type" INTEGER NOT NULL,
                    "Priority" INTEGER NOT NULL,
                    "BeginTime" TEXT NULL,
                    "EndTime" TEXT NULL,
                    "Status" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS "BiometricTemplates" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_BiometricTemplates" PRIMARY KEY AUTOINCREMENT,
                    "UserId" INTEGER NOT NULL,
                    "Template" TEXT NOT NULL,
                    "Type" INTEGER NOT NULL,
                    "FingerPosition" INTEGER NOT NULL,
                    "FingerType" INTEGER NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NULL,
                    "BeginTime" TEXT NULL,
                    "EndTime" TEXT NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS "Cards" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Cards" PRIMARY KEY AUTOINCREMENT,
                    "UserId" INTEGER NOT NULL,
                    "Value" TEXT NOT NULL,
                    "Type" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NULL,
                    "BeginTime" TEXT NULL,
                    "EndTime" TEXT NULL,
                    "Status" TEXT NOT NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS "ChangeLogs" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_ChangeLogs" PRIMARY KEY AUTOINCREMENT,
                    "OperationType" TEXT NOT NULL,
                    "TableName" TEXT NOT NULL,
                    "TableId" INTEGER NULL,
                    "Timestamp" TEXT NOT NULL,
                    "PerformedBy" TEXT NOT NULL,
                    "Description" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS "Configs" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Configs" PRIMARY KEY AUTOINCREMENT,
                    "Group" TEXT NOT NULL,
                    "Key" TEXT NOT NULL,
                    "Value" TEXT NOT NULL,
                    "Description" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS "Devices" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Devices" PRIMARY KEY AUTOINCREMENT,
                    "Name" TEXT NOT NULL,
                    "Ip" TEXT NOT NULL,
                    "IpAddress" TEXT NOT NULL,
                    "SerialNumber" TEXT NOT NULL,
                    "Firmware" TEXT NOT NULL,
                    "Status" TEXT NOT NULL,
                    "RegisteredAt" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "LastSeenAt" TEXT NULL,
                    "UpdatedAt" TEXT NULL,
                    "Description" TEXT NOT NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS "Groups" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Groups" PRIMARY KEY AUTOINCREMENT,
                    "Name" TEXT NOT NULL,
                    "Description" TEXT NOT NULL,
                    "Status" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS "Logos" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Logos" PRIMARY KEY AUTOINCREMENT,
                    "Base64Image" TEXT NOT NULL,
                    "Timestamp" TEXT NOT NULL,
                    "FileName" TEXT NOT NULL,
                    "Format" TEXT NOT NULL,
                    "Description" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS "Logs" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Logs" PRIMARY KEY AUTOINCREMENT,
                    "Level" TEXT NOT NULL,
                    "Message" TEXT NOT NULL,
                    "Timestamp" TEXT NOT NULL,
                    "StackTrace" TEXT NOT NULL,
                    "User" TEXT NOT NULL,
                    "EventCode" TEXT NOT NULL,
                    "Source" TEXT NOT NULL,
                    "AdditionalData" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS "MonitorEvents" (
                    "EventId" TEXT NOT NULL CONSTRAINT "PK_MonitorEvents" PRIMARY KEY,
                    "ReceivedAt" TEXT NOT NULL,
                    "RawJson" TEXT NOT NULL,
                    "EventType" TEXT NOT NULL,
                    "DeviceId" TEXT NOT NULL,
                    "UserId" TEXT NOT NULL,
                    "Payload" TEXT NOT NULL,
                    "Status" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS "Photos" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Photos" PRIMARY KEY AUTOINCREMENT,
                    "UserId" INTEGER NOT NULL,
                    "Base64Image" TEXT NOT NULL,
                    "Timestamp" TEXT NOT NULL,
                    "FileName" TEXT NOT NULL,
                    "Format" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS "PushCommands" (
                    "CommandId" TEXT NOT NULL CONSTRAINT "PK_PushCommands" PRIMARY KEY,
                    "ReceivedAt" TEXT NOT NULL,
                    "CommandType" TEXT NOT NULL,
                    "RawJson" TEXT NOT NULL,
                    "Status" TEXT NOT NULL,
                    "Payload" TEXT NOT NULL,
                    "DeviceId" TEXT NOT NULL,
                    "UserId" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS "QRCodes" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_QRCodes" PRIMARY KEY AUTOINCREMENT,
                    "UserId" INTEGER NOT NULL,
                    "Value" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NULL,
                    "BeginTime" TEXT NULL,
                    "EndTime" TEXT NULL,
                    "Status" TEXT NOT NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS "Sessions" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Sessions" PRIMARY KEY AUTOINCREMENT,
                    "DeviceAddress" TEXT NOT NULL,
                    "SessionString" TEXT NOT NULL,
                    "DeviceName" TEXT NOT NULL,
                    "DeviceSerial" TEXT NOT NULL,
                    "Username" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "ExpiresAt" TEXT NULL,
                    "IsActive" INTEGER NOT NULL,
                    "UpdatedAt" TEXT NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS "Syncs" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Syncs" PRIMARY KEY AUTOINCREMENT,
                    "SyncType" TEXT NOT NULL,
                    "Status" TEXT NOT NULL,
                    "Message" TEXT NOT NULL,
                    "StartedAt" TEXT NOT NULL,
                    "FinishedAt" TEXT NULL,
                    "ErrorCode" TEXT NOT NULL,
                    "AdditionalData" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS "Users" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
                    "Name" TEXT NOT NULL,
                    "Registration" TEXT NOT NULL,
                    "Username" TEXT NOT NULL,
                    "PasswordHash" TEXT NOT NULL,
                    "Salt" TEXT NOT NULL,
                    "Email" TEXT NOT NULL,
                    "Phone" TEXT NOT NULL,
                    "Status" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NULL
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"Users\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"Syncs\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"Sessions\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"QRCodes\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"PushCommands\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"Photos\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"MonitorEvents\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"Logs\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"Logos\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"Groups\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"Devices\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"Configs\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"ChangeLogs\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"Cards\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"BiometricTemplates\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"AccessRules\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"AccessLogs\";");
        }
    }
}
