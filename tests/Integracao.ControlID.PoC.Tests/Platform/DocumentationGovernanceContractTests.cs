namespace Integracao.ControlID.PoC.Tests.Platform;

public class DocumentationGovernanceContractTests
{
    [Fact]
    public void DocumentationIndex_OnboardingArchitectureAndAdrs_AreLinked()
    {
        var readme = ReadRepoFile("README.md");
        var docsIndex = ReadRepoFile("docs", "README.md");
        var onboarding = ReadRepoFile("docs", "developer-onboarding.md");
        var architecture = ReadRepoFile("docs", "architecture-overview.md");
        var agents = ReadRepoFile("AGENTS.md");

        Assert.Contains("docs/developer-onboarding.md", readme, StringComparison.Ordinal);
        Assert.Contains("docs/architecture-overview.md", readme, StringComparison.Ordinal);
        Assert.Contains("docs/residual-risk-closure.md", readme, StringComparison.Ordinal);
        Assert.Contains("docs/adrs/", readme, StringComparison.Ordinal);
        Assert.Contains("Leitura por papel", docsIndex, StringComparison.Ordinal);
        Assert.Contains("Critério estrito de liberação", onboarding, StringComparison.Ordinal);
        Assert.Contains("Fronteiras de confiança", architecture, StringComparison.Ordinal);
        Assert.Contains("docs/adrs/", agents, StringComparison.Ordinal);
    }

    [Fact]
    public void AdrAndChangeArtifacts_CoverCurrentArchitectureDecisions()
    {
        var sqliteAdr = ReadRepoFile("docs", "adrs", "0001-local-sqlite-runtime-state.md");
        var securityAdr = ReadRepoFile("docs", "adrs", "0002-secure-controlid-ingress-and-egress.md");
        var observabilityAdr = ReadRepoFile("docs", "adrs", "0003-in-process-observability-and-readiness-gates.md");
        var releaseAdr = ReadRepoFile("docs", "adrs", "0004-release-governance-with-local-scripts.md");
        var changelog = ReadRepoFile("docs", "changelog-2026-05-01.md");
        var prSummary = ReadRepoFile("docs", "pr-summary-2026-05-01.md");
        var audit = ReadRepoFile("docs", "documentation-audit-2026-05-01.md");

        Assert.Contains("Estado: aceita", sqliteAdr, StringComparison.Ordinal);
        Assert.Contains("SQLite local", sqliteAdr, StringComparison.Ordinal);
        Assert.Contains("Fluxos Control iD de entrada e saída", securityAdr, StringComparison.Ordinal);
        Assert.Contains("Observabilidade no processo", observabilityAdr, StringComparison.Ordinal);
        Assert.Contains("Governança de liberação", releaseAdr, StringComparison.Ordinal);
        Assert.Contains("Como validar", changelog, StringComparison.Ordinal);
        Assert.Contains("Pendências conhecidas", prSummary, StringComparison.Ordinal);
        Assert.Contains("Lacunas restantes", audit, StringComparison.Ordinal);
    }

    [Fact]
    public void DocumentationValidation_CoversInventoryEncodingLinksTraceabilityAndSourceMap()
    {
        var workflow = ReadRepoFile(".github", "workflows", "ci.yml");
        var releaseGate = ReadRepoFile("tools", "test-readiness-gates.ps1");
        var validator = ReadRepoFile("tools", "validate-documentation.ps1");
        var docsIndex = ReadRepoFile("docs", "README.md");

        Assert.Contains(".\\tools\\validate-documentation.ps1", workflow, StringComparison.Ordinal);
        Assert.Contains("documentation-validation", releaseGate, StringComparison.Ordinal);
        Assert.Contains("ExpectedMarkdownCount = 49", validator, StringComparison.Ordinal);
        Assert.Contains("[switch]$CheckExternalUrls", validator, StringComparison.Ordinal);
        Assert.Contains("Missing local Markdown anchor", validator, StringComparison.Ordinal);
        Assert.Contains("Invalid bare URL", validator, StringComparison.Ordinal);
        Assert.Contains("Vendored jquery-validation license hash", validator, StringComparison.Ordinal);
        Assert.Contains("Test file missing from project map", validator, StringComparison.Ordinal);
        Assert.Contains("Source file missing from project map", validator, StringComparison.Ordinal);
        Assert.Contains("Requirement traceability row must occur exactly once", validator, StringComparison.Ordinal);
        Assert.Contains("Mapped source files", validator, StringComparison.Ordinal);
        Assert.Contains("tools/validate-documentation.ps1", docsIndex, StringComparison.Ordinal);
        Assert.Contains("-CheckExternalUrls", docsIndex, StringComparison.Ordinal);
    }

    [Fact]
    public void ResidualRiskClosure_IsDocumentedAndEnforcedByReleaseGates()
    {
        var docsIndex = ReadRepoFile("docs", "README.md");
        var closure = ReadRepoFile("docs", "residual-risk-closure.md");
        var opsExample = ReadRepoFile("ops.example.json");
        var operationalReadiness = ReadRepoFile("tools", "operational-readiness-check.ps1");
        var releaseGate = ReadRepoFile("tools", "test-readiness-gates.ps1");

        Assert.Contains("docs/residual-risk-closure.md", docsIndex, StringComparison.Ordinal);
        Assert.Contains("Critério estrito sem exceções", closure, StringComparison.Ordinal);
        Assert.Contains("deployment", opsExample, StringComparison.Ordinal);
        Assert.Contains("privacy", opsExample, StringComparison.Ordinal);
        Assert.Contains("externalValidation", opsExample, StringComparison.Ordinal);
        Assert.Contains("hardwareContract", opsExample, StringComparison.Ordinal);
        Assert.Contains("deployment.productionApprovalStatus", operationalReadiness, StringComparison.Ordinal);
        Assert.Contains("privacy.legalBasisApprovalStatus", operationalReadiness, StringComparison.Ordinal);
        Assert.Contains("externalValidation.validationStatus", operationalReadiness, StringComparison.Ordinal);
        Assert.Contains("hardwareContract.validationStatus", operationalReadiness, StringComparison.Ordinal);
        Assert.Contains("-RequireHardwareContract", releaseGate, StringComparison.Ordinal);
        Assert.Contains("-RequireExternalScanners", releaseGate, StringComparison.Ordinal);
        Assert.Contains("RequireOperationalConfig", releaseGate, StringComparison.Ordinal);
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
