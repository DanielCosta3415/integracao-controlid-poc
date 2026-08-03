using System.Text.RegularExpressions;
using Integracao.ControlID.PoC.Services.Database;

namespace Integracao.ControlID.PoC.Tests.Services.Database;

public sealed class RepositoryFailureContractTests
{
    [Fact]
    public void Repositories_DoNotTranslateInfrastructureExceptionsIntoNotFoundResults()
    {
        var repositoryDirectory = Path.Combine(FindRepositoryRoot(), "Services", "Database");
        var catchReturningFalse = new Regex(
            @"catch\s*\(Exception\s+\w+\)\s*\{[^}]*return\s+false\s*;",
            RegexOptions.CultureInvariant | RegexOptions.Singleline);

        var violations = Directory
            .EnumerateFiles(repositoryDirectory, "*Repository.cs")
            .Where(path => catchReturningFalse.IsMatch(File.ReadAllText(path)))
            .Select(Path.GetFileName)
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void Repositories_DoNotUseBroadPassThroughExceptionHandlers()
    {
        var repositoryDirectory = Path.Combine(FindRepositoryRoot(), "Services", "Database");
        var broadExceptionHandler = new Regex(
            @"catch\s*\(Exception(?:\s+\w+)?\)",
            RegexOptions.CultureInvariant);

        var violations = Directory
            .EnumerateFiles(repositoryDirectory, "*Repository.cs")
            .Where(path => broadExceptionHandler.IsMatch(File.ReadAllText(path)))
            .Select(Path.GetFileName)
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void PushRepository_PreservesPublicUpdateSignature()
    {
        var method = typeof(PushCommandRepository).GetMethod(nameof(PushCommandRepository.UpdatePushCommandAsync));

        Assert.NotNull(method);
        Assert.Equal(typeof(Task<bool>), method.ReturnType);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Integracao.ControlID.PoC.sln")))
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Repository root was not found from the test output directory.");
    }
}
