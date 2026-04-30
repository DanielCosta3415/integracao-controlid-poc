using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Integracao.ControlID.PoC.Logging
{
    public static class SeriLogConfiguration
    {
        /// <summary>
        /// Configura o Serilog para a aplicação (console, arquivo, enrichers, filtros, etc.).
        /// Deve ser chamado no início do Program.cs.
        /// </summary>
        public static void ConfigureSerilog(IHostBuilder hostBuilder, IConfiguration configuration)
        {
            hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File(
                        path: "Logs/app_log.txt",
                        rollingInterval: RollingInterval.Day,
                        restrictedToMinimumLevel: LogEventLevel.Information,
                        retainedFileCountLimit: 14,
                        fileSizeLimitBytes: 10_000_000,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
                    );
            });
        }
    }
}
