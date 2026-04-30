using Integracao.ControlID.PoC.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Integracao.ControlID.PoC.Data
{
    public class IntegracaoControlIDContext : DbContext
    {
        public IntegracaoControlIDContext(DbContextOptions<IntegracaoControlIDContext> options)
            : base(options)
        {
        }

        public DbSet<UserLocal> Users { get; set; } = null!;
        public DbSet<DeviceLocal> Devices { get; set; } = null!;
        public DbSet<SessionLocal> Sessions { get; set; } = null!;
        public DbSet<BiometricTemplateLocal> BiometricTemplates { get; set; } = null!;
        public DbSet<CardLocal> Cards { get; set; } = null!;
        public DbSet<QRCodeLocal> QRCodes { get; set; } = null!;
        public DbSet<GroupLocal> Groups { get; set; } = null!;
        public DbSet<AccessLogLocal> AccessLogs { get; set; } = null!;
        public DbSet<ChangeLogLocal> ChangeLogs { get; set; } = null!;
        public DbSet<AccessRuleLocal> AccessRules { get; set; } = null!;
        public DbSet<ConfigLocal> Configs { get; set; } = null!;
        public DbSet<PhotoLocal> Photos { get; set; } = null!;
        public DbSet<LogoLocal> Logos { get; set; } = null!;
        public DbSet<MonitorEventLocal> MonitorEvents { get; set; } = null!;
        public DbSet<PushCommandLocal> PushCommands { get; set; } = null!;
        public DbSet<LogLocal> Logs { get; set; } = null!;
        public DbSet<SyncLocal> Syncs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MonitorEventLocal>()
                .HasKey(item => item.EventId);

            modelBuilder.Entity<MonitorEventLocal>()
                .Property(item => item.EventId)
                .ValueGeneratedNever();

            modelBuilder.Entity<PushCommandLocal>()
                .HasKey(item => item.CommandId);

            modelBuilder.Entity<PushCommandLocal>()
                .Property(item => item.CommandId)
                .ValueGeneratedNever();

            ConfigureOperationalIndexes(modelBuilder);
        }

        private static void ConfigureOperationalIndexes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccessLogLocal>()
                .HasIndex(item => item.Time)
                .HasDatabaseName("IX_AccessLogs_Time");

            modelBuilder.Entity<AccessLogLocal>()
                .HasIndex(item => new { item.UserId, item.Time })
                .HasDatabaseName("IX_AccessLogs_UserId_Time");

            modelBuilder.Entity<AccessLogLocal>()
                .HasIndex(item => new { item.DeviceId, item.Time })
                .HasDatabaseName("IX_AccessLogs_DeviceId_Time");

            modelBuilder.Entity<AccessLogLocal>()
                .HasIndex(item => new { item.Event, item.Time })
                .HasDatabaseName("IX_AccessLogs_Event_Time");

            modelBuilder.Entity<BiometricTemplateLocal>()
                .HasIndex(item => new { item.UserId, item.Type })
                .HasDatabaseName("IX_BiometricTemplates_UserId_Type");

            modelBuilder.Entity<CardLocal>()
                .HasIndex(item => new { item.UserId, item.Status })
                .HasDatabaseName("IX_Cards_UserId_Status");

            modelBuilder.Entity<ChangeLogLocal>()
                .HasIndex(item => item.Timestamp)
                .HasDatabaseName("IX_ChangeLogs_Timestamp");

            modelBuilder.Entity<ChangeLogLocal>()
                .HasIndex(item => new { item.TableName, item.Timestamp })
                .HasDatabaseName("IX_ChangeLogs_TableName_Timestamp");

            modelBuilder.Entity<ChangeLogLocal>()
                .HasIndex(item => new { item.OperationType, item.Timestamp })
                .HasDatabaseName("IX_ChangeLogs_OperationType_Timestamp");

            modelBuilder.Entity<ConfigLocal>()
                .HasIndex(item => new { item.Group, item.Key })
                .HasDatabaseName("IX_Configs_Group_Key");

            modelBuilder.Entity<DeviceLocal>()
                .HasIndex(item => item.Ip)
                .HasDatabaseName("IX_Devices_Ip");

            modelBuilder.Entity<DeviceLocal>()
                .HasIndex(item => item.IpAddress)
                .HasDatabaseName("IX_Devices_IpAddress");

            modelBuilder.Entity<DeviceLocal>()
                .HasIndex(item => item.SerialNumber)
                .HasDatabaseName("IX_Devices_SerialNumber");

            modelBuilder.Entity<LogLocal>()
                .HasIndex(item => item.CreatedAt)
                .HasDatabaseName("IX_Logs_CreatedAt");

            modelBuilder.Entity<LogLocal>()
                .HasIndex(item => new { item.Level, item.CreatedAt })
                .HasDatabaseName("IX_Logs_Level_CreatedAt");

            modelBuilder.Entity<MonitorEventLocal>()
                .HasIndex(item => item.ReceivedAt)
                .HasDatabaseName("IX_MonitorEvents_ReceivedAt");

            modelBuilder.Entity<MonitorEventLocal>()
                .HasIndex(item => new { item.EventType, item.ReceivedAt })
                .HasDatabaseName("IX_MonitorEvents_EventType_ReceivedAt");

            modelBuilder.Entity<MonitorEventLocal>()
                .HasIndex(item => new { item.Status, item.ReceivedAt })
                .HasDatabaseName("IX_MonitorEvents_Status_ReceivedAt");

            modelBuilder.Entity<MonitorEventLocal>()
                .HasIndex(item => new { item.DeviceId, item.ReceivedAt })
                .HasDatabaseName("IX_MonitorEvents_DeviceId_ReceivedAt");

            modelBuilder.Entity<PhotoLocal>()
                .HasIndex(item => new { item.UserId, item.CreatedAt })
                .HasDatabaseName("IX_Photos_UserId_CreatedAt");

            modelBuilder.Entity<PushCommandLocal>()
                .HasIndex(item => new { item.Status, item.DeviceId, item.CreatedAt })
                .HasDatabaseName("IX_PushCommands_Status_DeviceId_CreatedAt");

            modelBuilder.Entity<PushCommandLocal>()
                .HasIndex(item => item.ReceivedAt)
                .HasDatabaseName("IX_PushCommands_ReceivedAt");

            modelBuilder.Entity<PushCommandLocal>()
                .HasIndex(item => new { item.CommandType, item.ReceivedAt })
                .HasDatabaseName("IX_PushCommands_CommandType_ReceivedAt");

            modelBuilder.Entity<PushCommandLocal>()
                .HasIndex(item => new { item.UserId, item.ReceivedAt })
                .HasDatabaseName("IX_PushCommands_UserId_ReceivedAt");

            modelBuilder.Entity<QRCodeLocal>()
                .HasIndex(item => new { item.UserId, item.Status })
                .HasDatabaseName("IX_QRCodes_UserId_Status");

            modelBuilder.Entity<SessionLocal>()
                .HasIndex(item => new { item.IsActive, item.CreatedAt })
                .HasDatabaseName("IX_Sessions_IsActive_CreatedAt");

            modelBuilder.Entity<SessionLocal>()
                .HasIndex(item => new { item.DeviceAddress, item.IsActive, item.CreatedAt })
                .HasDatabaseName("IX_Sessions_DeviceAddress_IsActive_CreatedAt");

            modelBuilder.Entity<SessionLocal>()
                .HasIndex(item => new { item.Username, item.CreatedAt })
                .HasDatabaseName("IX_Sessions_Username_CreatedAt");

            modelBuilder.Entity<SyncLocal>()
                .HasIndex(item => item.StartedAt)
                .HasDatabaseName("IX_Syncs_StartedAt");

            modelBuilder.Entity<SyncLocal>()
                .HasIndex(item => new { item.Status, item.StartedAt })
                .HasDatabaseName("IX_Syncs_Status_StartedAt");

            modelBuilder.Entity<UserLocal>()
                .HasIndex(item => item.Registration)
                .HasDatabaseName("IX_Users_Registration");

            modelBuilder.Entity<UserLocal>()
                .HasIndex(item => item.Username)
                .HasDatabaseName("IX_Users_Username");
        }
    }
}
