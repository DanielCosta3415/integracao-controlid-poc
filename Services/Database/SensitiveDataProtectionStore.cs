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

        while (true)
        {
            var rows = new List<(long RowId, string Value)>();
            await using (var select = connection.CreateCommand())
            {
                select.CommandText = $"""
                    SELECT rowid, "{column.Column}"
                    FROM "{column.Table}"
                    WHERE "{column.Column}" IS NOT NULL
                      AND "{column.Column}" <> ''
                      AND substr("{column.Column}", 1, 6) <> 'dp:v1:'
                    LIMIT {BatchSize};
                    """;

                await using var reader = await select.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                    rows.Add((reader.GetInt64(0), reader.GetString(1)));
            }

            if (rows.Count == 0)
                return protectedCount;

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            foreach (var row in rows)
            {
                await using var update = connection.CreateCommand();
                update.Transaction = transaction;
                update.CommandText = $"UPDATE \"{column.Table}\" SET \"{column.Column}\" = @value WHERE rowid = @rowid;";

                var valueParameter = update.CreateParameter();
                valueParameter.ParameterName = "@value";
                valueParameter.Value = _protector.Protect(row.Value, column.Purpose);
                update.Parameters.Add(valueParameter);

                var rowIdParameter = update.CreateParameter();
                rowIdParameter.ParameterName = "@rowid";
                rowIdParameter.Value = row.RowId;
                update.Parameters.Add(rowIdParameter);

                protectedCount += await update.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
    }
}
