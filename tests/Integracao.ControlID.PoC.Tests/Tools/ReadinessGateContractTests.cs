namespace Integracao.ControlID.PoC.Tests.Tools;

public class ReadinessGateContractTests
{
    [Fact]
    public void TestReadinessGate_AlwaysValidatesObservabilityArtifacts()
    {
        var script = ReadRepoFile("tools", "test-readiness-gates.ps1");

        Assert.Contains("observability-offline", script, StringComparison.Ordinal);
        Assert.Contains("observability-check.ps1\" -OfflineValidateOnly", script, StringComparison.Ordinal);
        Assert.Contains("RunObservabilityOnline", script, StringComparison.Ordinal);
        Assert.Contains("RequireObservabilityMetrics", script, StringComparison.Ordinal);
        Assert.Contains("ReleaseGate", script, StringComparison.Ordinal);
        Assert.Contains("$RequireExternalScanners = $true", script, StringComparison.Ordinal);
    }

    [Fact]
    public void CiWorkflow_ValidatesObservabilityArtifactsBeforeAudit()
    {
        var workflow = ReadRepoFile(".github", "workflows", "ci.yml");

        Assert.Contains("Observability artifact check", workflow, StringComparison.Ordinal);
        Assert.Contains(".\\tools\\observability-check.ps1 -OfflineValidateOnly", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void AcceptanceDocs_ConvertExternalResidualsIntoReadinessGates()
    {
        var acceptance = ReadRepoFile("docs", "product-acceptance-criteria.md");
        var strategy = ReadRepoFile("docs", "testing-strategy.md");

        Assert.Contains("Gates de aceite e valida", acceptance, StringComparison.Ordinal);
        Assert.Contains("-RequireHardwareContract", acceptance, StringComparison.Ordinal);
        Assert.Contains("-RunObservabilityOnline -RequireObservabilityMetrics", acceptance, StringComparison.Ordinal);
        Assert.Contains("-ReleaseGate", acceptance, StringComparison.Ordinal);
        Assert.Contains("Gates de validacao externa", strategy, StringComparison.Ordinal);
    }

    private static string ReadRepoFile(params string[] segments)
    {
        return File.ReadAllText(Path.Combine(FindRepositoryRoot(), Path.Combine(segments)));
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Integracao.ControlID.PoC.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Repository root was not found from the test output directory.");
    }
}
