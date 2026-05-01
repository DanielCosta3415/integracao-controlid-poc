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
        Assert.Contains("RunContainerBuild", script, StringComparison.Ordinal);
        Assert.Contains("RunExternalScanners", script, StringComparison.Ordinal);
        Assert.Contains("RequireFinOpsCapacity", script, StringComparison.Ordinal);
        Assert.Contains("finops-capacity", script, StringComparison.Ordinal);
        Assert.Contains("finops-capacity-check.ps1", script, StringComparison.Ordinal);
        Assert.Contains("simulated-device-contract", script, StringComparison.Ordinal);
        Assert.Contains("contract-controlid-stub.ps1", script, StringComparison.Ordinal);
        Assert.Contains("external-security-scans.ps1", script, StringComparison.Ordinal);
        Assert.Contains("docker build --pull", script, StringComparison.Ordinal);
        Assert.Contains("ReleaseGate", script, StringComparison.Ordinal);
        Assert.Contains("$RequireExternalScanners = $true", script, StringComparison.Ordinal);
        Assert.Contains("$RequireFinOpsCapacity = $true", script, StringComparison.Ordinal);
    }

    [Fact]
    public void ExternalValidationArtifacts_DefineScannerContracts()
    {
        var scannerScript = ReadRepoFile("tools", "external-security-scans.ps1");
        var stubContractScript = ReadRepoFile("tools", "contract-controlid-stub.ps1");
        var semgrepConfig = ReadRepoFile(".semgrep.yml");
        var runbook = ReadRepoFile("docs", "external-validation-runbook.md");

        Assert.Contains("semgrep", scannerScript, StringComparison.Ordinal);
        Assert.Contains("osv-scanner", scannerScript, StringComparison.Ordinal);
        Assert.Contains("zap-baseline.py", scannerScript, StringComparison.Ordinal);
        Assert.Contains("axe", scannerScript, StringComparison.Ordinal);
        Assert.Contains("EXTERNAL_SCAN_BASE_URL", scannerScript, StringComparison.Ordinal);
        Assert.Contains("contract-controlid-device.ps1", stubContractScript, StringComparison.Ordinal);
        Assert.Contains("stub-admin", stubContractScript, StringComparison.Ordinal);
        Assert.Contains("controlid-sensitive-logging-keywords", semgrepConfig, StringComparison.Ordinal);
        Assert.Contains("-RequireExternalScanners", runbook, StringComparison.Ordinal);
        Assert.Contains("-RequireHardwareContract", runbook, StringComparison.Ordinal);
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
