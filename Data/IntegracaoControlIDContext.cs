using Integracao.ControlID.PoC.Models.Database;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

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

            // Exemplo de configuração adicional (caso precise de índices únicos ou relacionamentos):
            // modelBuilder.Entity<UserLocal>().HasIndex(u => u.Registration).IsUnique();
            // modelBuilder.Entity<DeviceLocal>().HasIndex(d => d.IpAddress);
        }
    }
}
