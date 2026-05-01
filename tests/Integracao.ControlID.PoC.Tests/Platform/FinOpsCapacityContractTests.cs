namespace Integracao.ControlID.PoC.Tests.Platform;

public class FinOpsCapacityContractTests
{
    [Fact]
    public void FinOpsRunbook_CoversCostCapacityGovernanceAndTradeoffs()
    {
        var runbook = ReadRepoFile("docs", "finops-capacity.md");
        var alerts = ReadRepoFile("docs", "observability", "alert-rules.json");
        var dashboard = ReadRepoFile("docs", "observability", "dashboard.json");
        var opsExample = ReadRepoFile("ops.example.json");

        Assert.Contains("Inventario de custos", runbook, StringComparison.Ordinal);
        Assert.Contains("Riscos de custo", runbook, StringComparison.Ordinal);
        Assert.Contains("Riscos de capacidade", runbook, StringComparison.Ordinal);
        Assert.Contains("Governanca FinOps", runbook, StringComparison.Ordinal);
        Assert.Contains("Alertas e limites sugeridos", runbook, StringComparison.Ordinal);
        Assert.Contains("Trade-offs", runbook, StringComparison.Ordinal);
        Assert.Contains("Riscos residuais", runbook, StringComparison.Ordinal);
        Assert.Contains("\"FIN-001\"", alerts, StringComparison.Ordinal);
        Assert.Contains("\"FIN-002\"", alerts, StringComparison.Ordinal);
        Assert.Contains("\"FIN-003\"", alerts, StringComparison.Ordinal);
        Assert.Contains("\"finops-capacity\"", dashboard, StringComparison.Ordinal);
        Assert.Contains("\"costOwner\"", opsExample, StringComparison.Ordinal);
        Assert.Contains("\"monthlyBudget\"", opsExample, StringComparison.Ordinal);
        Assert.Contains("\"billingDashboard\"", opsExample, StringComparison.Ordinal);
        Assert.Contains("\"actualSpendReviewSource\"", opsExample, StringComparison.Ordinal);
        Assert.Contains("\"billingAlertOwner\"", opsExample, StringComparison.Ordinal);
    }

    [Fact]
    public void FinOpsCapacityCheck_IsNonDestructiveAndPartOfReadiness()
    {
        var finopsCheck = ReadRepoFile("tools", "finops-capacity-check.ps1");
        var readinessGate = ReadRepoFile("tools", "test-readiness-gates.ps1");
        var workflow = ReadRepoFile(".github", "workflows", "ci.yml");
        var compose = ReadRepoFile("docker-compose.yml");

        Assert.Contains("finops-capacity-check.ps1", readinessGate, StringComparison.Ordinal);
        Assert.Contains("RequireFinOpsCapacity", readinessGate, StringComparison.Ordinal);
        Assert.Contains("$RequireFinOpsCapacity = $true", readinessGate, StringComparison.Ordinal);
        Assert.Contains("-FailOnWarnings", readinessGate, StringComparison.Ordinal);
        Assert.Contains("FinOps capacity artifact check", workflow, StringComparison.Ordinal);
        Assert.Contains("Serilog__WriteTo__1__Args__retainedFileCountLimit", compose, StringComparison.Ordinal);
        Assert.Contains("Serilog__WriteTo__1__Args__fileSizeLimitBytes", compose, StringComparison.Ordinal);
        Assert.Contains("Nao foi removido nenhum dado", finopsCheck, StringComparison.Ordinal);
        Assert.DoesNotContain("Remove-Item", finopsCheck, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("docker compose down", finopsCheck, StringComparison.OrdinalIgnoreCase);
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
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Repository root was not found from the test output directory.");
    }
}
