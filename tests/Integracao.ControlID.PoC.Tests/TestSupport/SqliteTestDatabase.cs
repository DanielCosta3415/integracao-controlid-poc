using Integracao.ControlID.PoC.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Integracao.ControlID.PoC.Tests.TestSupport;

public sealed class SqliteTestDatabase : IDisposable
{
    public SqliteTestDatabase()
    {
        Connection = new SqliteConnection("Data Source=:memory:");
        Connection.Open();

        var options = new DbContextOptionsBuilder<IntegracaoControlIDContext>()
            .UseSqlite(Connection)
            .Options;

        Context = new IntegracaoControlIDContext(options);
        Context.Database.EnsureCreated();
    }

    public SqliteConnection Connection { get; }

    public IntegracaoControlIDContext Context { get; }

    public void Dispose()
    {
        Context.Dispose();
        Connection.Dispose();
    }
}
