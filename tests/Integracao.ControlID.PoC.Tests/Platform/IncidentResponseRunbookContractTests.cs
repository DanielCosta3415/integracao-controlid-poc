namespace Integracao.ControlID.PoC.Tests.Platform;

public class IncidentResponseRunbookContractTests
{
    [Fact]
    public void IncidentResponseRunbook_CoversSeveritiesCriticalScenariosAndPostmortem()
    {
        var runbook = ReadRepoFile("docs", "incident-response-and-dr.md");
        var readme = ReadRepoFile("README.md");
        var alerts = ReadRepoFile("docs", "observability", "alert-rules.json");

        Assert.Contains("SEV1", runbook, StringComparison.Ordinal);
        Assert.Contains("SEV2", runbook, StringComparison.Ordinal);
        Assert.Contains("IR-01 API fora do ar", runbook, StringComparison.Ordinal);
        Assert.Contains("IR-02 Banco SQLite indisponivel", runbook, StringComparison.Ordinal);
        Assert.Contains("IR-07 Integracao Control iD indisponivel", runbook, StringComparison.Ordinal);
        Assert.Contains("IR-08 Webhook/callback falhando", runbook, StringComparison.Ordinal);
        Assert.Contains("IR-10 Deploy ruim", runbook, StringComparison.Ordinal);
        Assert.Contains("IR-13 Vazamento de dados", runbook, StringComparison.Ordinal);
        Assert.Contains("IR-14 Secret comprometido", runbook, StringComparison.Ordinal);
        Assert.Contains("RTO/RPO", runbook, StringComparison.Ordinal);
        Assert.Contains("Template de postmortem", runbook, StringComparison.Ordinal);
        Assert.Contains("ops.local.json", runbook, StringComparison.Ordinal);
        Assert.Contains("docs/equipment-contingency-runbook.md", runbook, StringComparison.Ordinal);
        Assert.Contains("docs/incident-response-and-dr.md", readme, StringComparison.Ordinal);
        Assert.Contains("\"incidentRunbook\": \"docs/incident-response-and-dr.md\"", alerts, StringComparison.Ordinal);
    }

    [Fact]
    public void OperationalReadinessArtifacts_ConvertResidualRisksIntoRequiredReleaseGate()
    {
        var gate = ReadRepoFile("tools", "test-readiness-gates.ps1");
        var readinessCheck = ReadRepoFile("tools", "operational-readiness-check.ps1");
        var backupScript = ReadRepoFile("tools", "backup-sqlite-operational.ps1");
        var example = ReadRepoFile("ops.example.json");
        var equipmentRunbook = ReadRepoFile("docs", "equipment-contingency-runbook.md");

        Assert.Contains("RequireOperationalConfig", gate, StringComparison.Ordinal);
        Assert.Contains("operational-readiness-check.ps1", gate, StringComparison.Ordinal);
        Assert.Contains("$RequireOperationalConfig = $true", gate, StringComparison.Ordinal);
        Assert.Contains("ops.local.json", readinessCheck, StringComparison.Ordinal);
        Assert.Contains("incidentCommander", example, StringComparison.Ordinal);
        Assert.Contains("externalBackupTarget", example, StringComparison.Ordinal);
        Assert.Contains("manualAccessProcedureOwner", example, StringComparison.Ordinal);
        Assert.Contains("MirrorDirectory", backupScript, StringComparison.Ordinal);
        Assert.Contains("RunRestoreSmoke", backupScript, StringComparison.Ordinal);
        Assert.Contains("RetentionConfirmation", backupScript, StringComparison.Ordinal);
        Assert.Contains("Manual fallback", equipmentRunbook, StringComparison.Ordinal);
        Assert.Contains("Contingency validation", equipmentRunbook, StringComparison.Ordinal);
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
