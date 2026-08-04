using System.Data.Common;
using Integracao.ControlID.PoC.Options;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Services.Database;

public sealed class SqliteConnectionPragmaInterceptor : DbConnectionInterceptor
{
    private readonly int _busyTimeoutMilliseconds;
    private readonly string _synchronousMode;

    public SqliteConnectionPragmaInterceptor(IOptions<SqliteRuntimeOptions> options)
    {
        _busyTimeoutMilliseconds = Math.Clamp(options.Value.BusyTimeoutMilliseconds, 100, 60_000);
        _synchronousMode = options.Value.SynchronousMode.Equals("FULL", StringComparison.OrdinalIgnoreCase)
            ? "FULL"
            : "NORMAL";
    }

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        Apply(connection);
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await ApplyAsync(connection, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    private void Apply(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = BuildCommandText();
        command.ExecuteNonQuery();
    }

    private async Task ApplyAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = BuildCommandText();
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private string BuildCommandText()
    {
        return $"PRAGMA busy_timeout={_busyTimeoutMilliseconds}; PRAGMA foreign_keys=ON; PRAGMA synchronous={_synchronousMode};";
    }
}
