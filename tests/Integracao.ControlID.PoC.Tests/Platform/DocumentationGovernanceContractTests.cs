namespace Integracao.ControlID.PoC.Tests.Platform;

public class DocumentationGovernanceContractTests
{
    [Fact]
    public void DocumentationIndex_OnboardingArchitectureAndAdrs_AreLinked()
    {
        var readme = ReadRepoFile("README.md");
        var docsIndex = ReadRepoFile("docs", "README.md");
        var onboarding = ReadRepoFile("docs", "primeiros-passos", "developer-onboarding.md");
        var architecture = ReadRepoFile("docs", "arquitetura", "architecture-overview.md");
        var faq = ReadRepoFile("docs", "primeiros-passos", "faq.md");
        var accountAdministration = ReadRepoFile("docs", "seguranca-privacidade", "local-account-administration.md");
        var agents = ReadRepoFile("AGENTS.md");

        Assert.Contains("docs/primeiros-passos/faq.md", readme, StringComparison.Ordinal);
        Assert.Contains("docs/primeiros-passos/developer-onboarding.md", readme, StringComparison.Ordinal);
        Assert.Contains("docs/arquitetura/project-file-responsibilities.md", readme, StringComparison.Ordinal);
        Assert.Contains("docs/README.md", readme, StringComparison.Ordinal);
        Assert.Contains("Comece pelo seu objetivo", docsIndex, StringComparison.Ordinal);
        Assert.Contains("Fontes canônicas", docsIndex, StringComparison.Ordinal);
        Assert.Contains("Responsabilidade e cadência", docsIndex, StringComparison.Ordinal);
        Assert.Contains("Critério estrito de liberação", onboarding, StringComparison.Ordinal);
        Assert.Contains("Fronteiras de confiança", architecture, StringComparison.Ordinal);
        Assert.Contains("96. Que evidências", faq, StringComparison.Ordinal);
        Assert.Contains("Papéis e permissões atuais", accountAdministration, StringComparison.Ordinal);
        Assert.Contains("docs/adrs/", agents, StringComparison.Ordinal);
    }

    [Fact]
    public void AdrAndChangeArtifacts_CoverCurrentArchitectureDecisions()
    {
        var sqliteAdr = ReadRepoFile("docs", "adrs", "0001-local-sqlite-runtime-state.md");
        var securityAdr = ReadRepoFile("docs", "adrs", "0002-secure-controlid-ingress-and-egress.md");
        var observabilityAdr = ReadRepoFile("docs", "adrs", "0003-in-process-observability-and-readiness-gates.md");
        var releaseAdr = ReadRepoFile("docs", "adrs", "0004-release-governance-with-local-scripts.md");
        var runtimeAdr = ReadRepoFile("docs", "adrs", "0005-dotnet-10-lts-runtime.md");
        var changelog = ReadRepoFile("docs", "historico", "changelogs", "changelog-2026-05-01.md");
        var prSummary = ReadRepoFile("docs", "historico", "auditorias", "pr-summary-2026-05-01.md");
        var audit = ReadRepoFile("docs", "historico", "auditorias", "documentation-audit-2026-05-01.md");

        Assert.Contains("Estado: aceita", sqliteAdr, StringComparison.Ordinal);
        Assert.Contains("SQLite local", sqliteAdr, StringComparison.Ordinal);
        Assert.Contains("Fluxos Control iD de entrada e saída", securityAdr, StringComparison.Ordinal);
        Assert.Contains("Observabilidade no processo", observabilityAdr, StringComparison.Ordinal);
        Assert.Contains("Governança de liberação", releaseAdr, StringComparison.Ordinal);
        Assert.Contains("Adoção coordenada do .NET 10 LTS", runtimeAdr, StringComparison.Ordinal);
        Assert.Contains("Como validar", changelog, StringComparison.Ordinal);
        Assert.Contains("Pendências conhecidas", prSummary, StringComparison.Ordinal);
        Assert.Contains("Lacunas restantes", audit, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeBaseline_IsCoordinatedAcrossProjectsPackagesAndContainer()
    {
        var globalJson = ReadRepoFile("global.json");
        var applicationProject = ReadRepoFile("Integracao.ControlID.PoC.csproj");
        var testProject = ReadRepoFile("tests", "Integracao.ControlID.PoC.Tests", "Integracao.ControlID.PoC.Tests.csproj");
        var stubProject = ReadRepoFile("tools", "ControlIdDeviceStub", "ControlIdDeviceStub.csproj");
        var proxyProject = ReadRepoFile("tools", "ControlIdCallbackSigningProxy", "ControlIdCallbackSigningProxy.csproj");
        var dockerfile = ReadRepoFile("Dockerfile");
        var toolManifest = ReadRepoFile(".config", "dotnet-tools.json");
        var sbomGenerator = ReadRepoFile("tools", "generate-sbom.ps1");

        Assert.Contains("\"version\": \"10.0.302\"", globalJson, StringComparison.Ordinal);
        Assert.Contains("\"rollForward\": \"latestPatch\"", globalJson, StringComparison.Ordinal);
        Assert.All(
            new[] { applicationProject, testProject, stubProject, proxyProject },
            project => Assert.Contains("<TargetFramework>net10.0</TargetFramework>", project, StringComparison.Ordinal));
        Assert.Contains("Microsoft.EntityFrameworkCore\" Version=\"10.0.10\"", applicationProject, StringComparison.Ordinal);
        Assert.Contains("Microsoft.EntityFrameworkCore.Sqlite\" Version=\"10.0.10\"", applicationProject, StringComparison.Ordinal);
        Assert.Contains("Microsoft.EntityFrameworkCore.Tools\" Version=\"10.0.10\"", applicationProject, StringComparison.Ordinal);
        Assert.Contains("Microsoft.AspNetCore.Mvc.Testing\" Version=\"10.0.10\"", testProject, StringComparison.Ordinal);
        Assert.Contains("ARG DOTNET_VERSION=10.0", dockerfile, StringComparison.Ordinal);
        Assert.Contains("\"dotnet-ef\"", toolManifest, StringComparison.Ordinal);
        Assert.Contains("\"version\": \"10.0.10\"", toolManifest, StringComparison.Ordinal);
        Assert.Contains("ToolManifestPath", sbomGenerator, StringComparison.Ordinal);
        Assert.Contains("Type = \"DotnetTool\"", sbomGenerator, StringComparison.Ordinal);
    }

    [Fact]
    public void DocumentationValidation_CoversInventoryEncodingLinksTraceabilityAndNavigation()
    {
        var workflow = ReadRepoFile(".github", "workflows", "ci.yml");
        var releaseGate = ReadRepoFile("tools", "test-readiness-gates.ps1");
        var validator = ReadRepoFile("tools", "validate-documentation.ps1");
        var docsIndex = ReadRepoFile("docs", "README.md");

        Assert.Contains(".\\tools\\validate-documentation.ps1", workflow, StringComparison.Ordinal);
        Assert.Contains("documentation-validation", releaseGate, StringComparison.Ordinal);
        Assert.Contains("ExpectedMarkdownCount = 81", validator, StringComparison.Ordinal);
        Assert.Contains("[switch]$CheckExternalUrls", validator, StringComparison.Ordinal);
        Assert.Contains("Missing local Markdown anchor", validator, StringComparison.Ordinal);
        Assert.Contains("Documentation file missing from domain index", validator, StringComparison.Ordinal);
        Assert.Contains("Orphan Markdown document without incoming link", validator, StringComparison.Ordinal);
        Assert.Contains("Existing Markdown reference must be a clickable link", validator, StringComparison.Ordinal);
        Assert.Contains("Invalid bare URL", validator, StringComparison.Ordinal);
        Assert.Contains("Vendored jquery-validation license hash", validator, StringComparison.Ordinal);
        Assert.Contains("generate-source-inventory.ps1", validator, StringComparison.Ordinal);
        Assert.Contains("Requirement traceability row must occur exactly once", validator, StringComparison.Ordinal);
        Assert.Contains("Indexed documents with incoming links", validator, StringComparison.Ordinal);
        Assert.Contains("Source inventory generator: validated", validator, StringComparison.Ordinal);
        Assert.Contains(".\\tools\\validate-documentation.ps1", docsIndex, StringComparison.Ordinal);
        Assert.Contains("-CheckExternalUrls", docsIndex, StringComparison.Ordinal);
    }

    [Fact]
    public void ResidualRiskClosure_IsDocumentedAndEnforcedByReleaseGates()
    {
        var docsIndex = ReadRepoFile("docs", "README.md");
        var closure = ReadRepoFile("docs", "operacao", "residual-risk-closure.md");
        var opsExample = ReadRepoFile("ops.example.json");
        var operationalReadiness = ReadRepoFile("tools", "operational-readiness-check.ps1");
        var releaseGate = ReadRepoFile("tools", "test-readiness-gates.ps1");

        Assert.Contains("operacao/residual-risk-closure.md", docsIndex, StringComparison.Ordinal);
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
