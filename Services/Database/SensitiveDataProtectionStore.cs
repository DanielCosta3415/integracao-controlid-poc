using System.Data;
using Integracao.ControlID.PoC.Data;
using Microsoft.EntityFrameworkCore;

namespace Integracao.ControlID.PoC.Services.Database;

public sealed class SensitiveDataProtectionStore
{
    private const int BatchSize = 100;
    private readonly IntegracaoControlIDContext _dbContext;
    private readonly SensitiveDataProtector _protector;

    public SensitiveDataProtectionStore(
        IntegracaoControlIDContext dbContext,
        SensitiveDataProtector protector)
    {
        _dbContext = dbContext;
        _protector = protector;
    }

    public async Task<int> CountUnprotectedValuesAsync(CancellationToken cancellationToken = default)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var openedHere = connection.State != ConnectionState.Open;
        if (openedHere)
            await connection.OpenAsync(cancellationToken);

        try
        {
            var total = 0;
            foreach (var column in SensitiveDataColumnCatalog.All)
            {
                await using var command = connection.CreateCommand();
                command.CommandText = $"""
                    SELECT COUNT(*)
                    FROM "{column.Table}"
                    WHERE "{column.Column}" IS NOT NULL
                      AND "{column.Column}" <> ''
                      AND substr("{column.Column}", 1, 6) <> 'dp:v1:';
                    """;
                total += Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
            }

            return total;
        }
        finally
        {
            if (openedHere)
                await connection.CloseAsync();
        }
    }

    public async Task<bool> HasUnprotectedValuesAsync(CancellationToken cancellationToken = default)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var openedHere = connection.State != ConnectionState.Open;
        if (openedHere)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            var checks = SensitiveDataColumnCatalog.All.Select(static column => $"""
                EXISTS (
                    SELECT 1
                    FROM "{column.Table}"
                    WHERE "{column.Column}" IS NOT NULL
                      AND "{column.Column}" <> ''
                      AND substr("{column.Column}", 1, 6) <> 'dp:v1:'
                    LIMIT 1
                )
                """);
            command.CommandText = $"SELECT {string.Join(" OR ", checks)};";
            return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) != 0;
        }
        finally
        {
            if (openedHere)
                await connection.CloseAsync();
        }
    }

    public async Task<int> ProtectLegacyValuesAsync(CancellationToken cancellationToken = default)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var openedHere = connection.State != ConnectionState.Open;
        if (openedHere)
            await connection.OpenAsync(cancellationToken);

        try
        {
            var total = 0;
            foreach (var column in SensitiveDataColumnCatalog.All)
                total += await ProtectColumnAsync(connection, column, cancellationToken);

            return total;
        }
        finally
        {
            if (openedHere)
                await connection.CloseAsync();
        }
    }

    private async Task<int> ProtectColumnAsync(
        System.Data.Common.DbConnection connection,
        SensitiveDataColumn column,
        CancellationToken cancellationToken)
    {
        var protectedCount = 0;
        var lastRowId = 0L;

        while (true)
        {
            var rows = new List<(long RowId, string Value)>();
            await using (var select = connection.CreateCommand())
            {
                select.CommandText = $"""
                    SELECT rowid, "{column.Column}"
                    FROM "{column.Table}"
                    WHERE rowid > @lastRowId
                      AND "{column.Column}" IS NOT NULL
                      AND "{column.Column}" <> ''
                      AND substr("{column.Column}", 1, 6) <> 'dp:v1:'
                    ORDER BY rowid
                    LIMIT {BatchSize};
                    """;

                var lastRowIdParameter = select.CreateParameter();
                lastRowIdParameter.ParameterName = "@lastRowId";
                lastRowIdParameter.Value = lastRowId;
                select.Parameters.Add(lastRowIdParameter);

                await using var reader = await select.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                    rows.Add((reader.GetInt64(0), reader.GetString(1)));
            }

            if (rows.Count == 0)
                return protectedCount;

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            await using var update = connection.CreateCommand();
            update.Transaction = transaction;
            update.CommandText = $"UPDATE \"{column.Table}\" SET \"{column.Column}\" = @value WHERE rowid = @rowid;";

            var valueParameter = update.CreateParameter();
            valueParameter.ParameterName = "@value";
            update.Parameters.Add(valueParameter);

            var rowIdParameter = update.CreateParameter();
            rowIdParameter.ParameterName = "@rowid";
            update.Parameters.Add(rowIdParameter);

            foreach (var row in rows)
            {
                valueParameter.Value = _protector.Protect(row.Value, column.Purpose);
                rowIdParameter.Value = row.RowId;

                protectedCount += await update.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            lastRowId = rows[^1].RowId;
        }
    }
}
