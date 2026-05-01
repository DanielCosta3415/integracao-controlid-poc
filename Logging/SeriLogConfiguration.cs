using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

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
                    .Enrich.FromLogContext();
            });
        }
    }
}
