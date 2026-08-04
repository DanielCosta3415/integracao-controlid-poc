using Integracao.ControlID.PoC.Data;
using Integracao.ControlID.PoC.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Services.Database;

public sealed class SqliteRuntimePolicy
{
    private readonly bool _writeAheadLoggingEnabled;

    public SqliteRuntimePolicy(IOptions<SqliteRuntimeOptions> options)
    {
        _writeAheadLoggingEnabled = options.Value.WriteAheadLoggingEnabled;
    }

    public async Task<string> ApplyAsync(
        IntegracaoControlIDContext dbContext,
        CancellationToken cancellationToken = default)
    {
        if (!_writeAheadLoggingEnabled)
            return "disabled";

        var connection = dbContext.Database.GetDbConnection();
        var mustClose = connection.State != System.Data.ConnectionState.Open;
        if (mustClose)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA journal_mode=WAL;";
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToString(result, System.Globalization.CultureInfo.InvariantCulture) ?? "unknown";
        }
        finally
        {
            if (mustClose)
                await connection.CloseAsync();
        }
    }
}
