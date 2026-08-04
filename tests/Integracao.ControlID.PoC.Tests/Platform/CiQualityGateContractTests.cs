namespace Integracao.ControlID.PoC.Tests.Platform;

public class CiQualityGateContractTests
{
    [Fact]
    public void GithubActionsCi_UsesReproducibleQualityGatesWithoutDeploy()
    {
        var workflow = ReadRepoFile(".github", "workflows", "ci.yml");

        Assert.Contains("name: CI", workflow, StringComparison.Ordinal);
        Assert.Contains("pull_request:", workflow, StringComparison.Ordinal);
        Assert.Contains("contents: read", workflow, StringComparison.Ordinal);
        Assert.Contains("concurrency:", workflow, StringComparison.Ordinal);
        Assert.Contains("actions/checkout@v7", workflow, StringComparison.Ordinal);
        Assert.Contains("actions/setup-dotnet@v6", workflow, StringComparison.Ordinal);
        Assert.Contains("global-json-file: global.json", workflow, StringComparison.Ordinal);
        Assert.Contains("cache-dependency-path:", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet restore .\\Integracao.ControlID.PoC.sln --locked-mode", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet build .\\Integracao.ControlID.PoC.sln --no-restore", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet test .\\Integracao.ControlID.PoC.sln --no-build", workflow, StringComparison.Ordinal);
        Assert.Contains(".\\tools\\smoke-localhost.ps1", workflow, StringComparison.Ordinal);
        Assert.Contains(".\\tools\\contract-controlid-stub.ps1", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet format .\\Integracao.ControlID.PoC.sln --verify-no-changes", workflow, StringComparison.Ordinal);
        Assert.Contains("git diff --check", workflow, StringComparison.Ordinal);
        Assert.Contains(".\\tools\\validate-documentation.ps1", workflow, StringComparison.Ordinal);
        Assert.Contains(".\\tools\\scan-secrets.ps1", workflow, StringComparison.Ordinal);
        Assert.Contains(".\\tools\\observability-check.ps1 -OfflineValidateOnly", workflow, StringComparison.Ordinal);
        Assert.Contains(".\\tools\\operational-readiness-check.ps1", workflow, StringComparison.Ordinal);
        Assert.Contains(".\\tools\\finops-capacity-check.ps1", workflow, StringComparison.Ordinal);
        Assert.Contains(".\\tools\\audit-supply-chain.ps1", workflow, StringComparison.Ordinal);
        Assert.Contains(".\\tools\\external-security-scans.ps1 -InventoryOnly", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet list $target package --vulnerable --include-transitive", workflow, StringComparison.Ordinal);
        Assert.Contains("actions/upload-artifact@v7", workflow, StringComparison.Ordinal);
        Assert.Contains("docker compose config", workflow, StringComparison.Ordinal);
        Assert.Contains("docker build --pull", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("deploy", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("environment:", workflow, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Dependabot_GroupsCompatibleUpdatesAndBlocksAutomaticMajorUpdates()
    {
        var dependabot = ReadRepoFile(".github", "dependabot.yml");

        Assert.Contains("package-ecosystem: nuget", dependabot, StringComparison.Ordinal);
        Assert.Contains("package-ecosystem: github-actions", dependabot, StringComparison.Ordinal);
        Assert.Contains("open-pull-requests-limit: 2", dependabot, StringComparison.Ordinal);
        Assert.Contains("dotnet-compatible-updates:", dependabot, StringComparison.Ordinal);
        Assert.Contains("github-actions-compatible-updates:", dependabot, StringComparison.Ordinal);
        Assert.Equal(2, CountOccurrences(dependabot, "version-update:semver-major"));
    }

    [Fact]
    public void CiCdDocumentation_ExplainsLocalReproductionAndBranchProtection()
    {
        var readme = ReadRepoFile("README.md");
        var docsIndex = ReadRepoFile("docs", "README.md");
        var ciDocs = ReadRepoFile("docs", "ci-cd-quality-gates.md");
        var projectMap = ReadRepoFile("docs", "project-file-responsibilities.md");

        Assert.Contains("docs/ci-cd-quality-gates.md", readme, StringComparison.Ordinal);
        Assert.Contains("docs/ci-cd-quality-gates.md", docsIndex, StringComparison.Ordinal);
        Assert.Contains("GitHub Actions", ciDocs, StringComparison.Ordinal);
        Assert.Contains("Critérios de qualidade obrigatórios", ciDocs, StringComparison.Ordinal);
        Assert.Contains("Reprodução local", ciDocs, StringComparison.Ordinal);
        Assert.Contains("Proteção recomendada da ramificação", ciDocs, StringComparison.Ordinal);
        Assert.Contains("A CI não executa implantação", ciDocs, StringComparison.Ordinal);
        Assert.Contains("docs/ci-cd-quality-gates.md", projectMap, StringComparison.Ordinal);
    }

    [Fact]
    public void ContainerAndVendorAudit_AreResilientAcrossCiRunnerImagesAndLineEndings()
    {
        var dockerfile = ReadRepoFile("Dockerfile");
        var vendorAudit = ReadRepoFile("tools", "audit-vendor-dependencies.ps1");
        var supplyChainDocs = ReadRepoFile("docs", "supply-chain-review.md");

        Assert.Contains("if ! grep -q '^app:' /etc/group; then addgroup -S app; fi", dockerfile, StringComparison.Ordinal);
        Assert.Contains("if ! id -u app >/dev/null 2>&1; then adduser -S -G app app; fi", dockerfile, StringComparison.Ordinal);
        Assert.Contains("USER app", dockerfile, StringComparison.Ordinal);
        Assert.Contains("Get-NormalizedFileSha256", vendorAudit, StringComparison.Ordinal);
        Assert.Contains("Replace(\"`r`n\", \"`n\").Replace(\"`r\", \"`n\")", vendorAudit, StringComparison.Ordinal);
        Assert.Contains("Get-FileHash -LiteralPath $Path", vendorAudit, StringComparison.Ordinal);
        Assert.Contains("[Array]::Sort($files, [StringComparer]::Ordinal)", vendorAudit, StringComparison.Ordinal);
        Assert.Contains("manifest=$($dependency.directorySha256), detected=$($directoryHash.Sha256)", vendorAudit, StringComparison.Ordinal);
        Assert.Contains("normaliza finais de linha de arquivos texto e usa ordenação ordinal de caminhos", supplyChainDocs, StringComparison.Ordinal);
    }

    private static string ReadRepoFile(params string[] segments)
    {
        return File.ReadAllText(Path.Combine(FindRepositoryRoot(), Path.Combine(segments)));
    }

    private static int CountOccurrences(string value, string searchValue)
    {
        var count = 0;
        var index = 0;

        while ((index = value.IndexOf(searchValue, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += searchValue.Length;
        }

        return count;
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
