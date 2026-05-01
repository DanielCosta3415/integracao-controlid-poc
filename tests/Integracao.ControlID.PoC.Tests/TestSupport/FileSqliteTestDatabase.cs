using Integracao.ControlID.PoC.Data;
using Microsoft.EntityFrameworkCore;

namespace Integracao.ControlID.PoC.Tests.TestSupport;

public sealed class FileSqliteTestDatabase : IDisposable
{
    private readonly string _directoryPath;

    public FileSqliteTestDatabase()
    {
        _directoryPath = Path.Combine(Path.GetTempPath(), "controlid-poc-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directoryPath);
        DatabasePath = Path.Combine(_directoryPath, "test.db");

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public string DatabasePath { get; }

    public IntegracaoControlIDContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<IntegracaoControlIDContext>()
            .UseSqlite($"Data Source={DatabasePath}")
            .Options;

        return new IntegracaoControlIDContext(options);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_directoryPath, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
