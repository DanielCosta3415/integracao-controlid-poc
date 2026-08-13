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
        Assert.Contains("actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1", workflow, StringComparison.Ordinal);
        Assert.Contains("actions/setup-dotnet@a98b56852c35b8e3190ac28c8c2271da59106c68", workflow, StringComparison.Ordinal);
        Assert.Contains("global-json-file: global.json", workflow, StringComparison.Ordinal);
        Assert.Contains("cache-dependency-path:", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet restore .\\Integracao.ControlID.PoC.sln --locked-mode", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet tool restore", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet build .\\Integracao.ControlID.PoC.sln --no-restore", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet test .\\tests\\Integracao.ControlID.PoC.Tests\\Integracao.ControlID.PoC.Tests.csproj --no-build", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet test .\\tests\\Integracao.ControlID.PoC.E2E\\Integracao.ControlID.PoC.E2E.csproj --no-build", workflow, StringComparison.Ordinal);
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
        Assert.Contains("actions/upload-artifact@043fb46d1a93c77aae656e7c1c64a875d1fc6a0a", workflow, StringComparison.Ordinal);
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
        var ciDocs = ReadRepoFile("docs", "qualidade", "ci-cd-quality-gates.md");
        var qualityIndex = ReadRepoFile("docs", "qualidade", "README.md");
        var projectMap = ReadRepoFile("docs", "arquitetura", "project-file-responsibilities.md");

        Assert.Contains("docs/qualidade/README.md", readme, StringComparison.Ordinal);
        Assert.Contains("qualidade/ci-cd-quality-gates.md", docsIndex, StringComparison.Ordinal);
        Assert.Contains("GitHub Actions", ciDocs, StringComparison.Ordinal);
        Assert.Contains("Critérios de qualidade obrigatórios", ciDocs, StringComparison.Ordinal);
        Assert.Contains("Reprodução local", ciDocs, StringComparison.Ordinal);
        Assert.Contains("Proteção aplicada à ramificação", ciDocs, StringComparison.Ordinal);
        Assert.Contains("audit-github-security.ps1", ciDocs, StringComparison.Ordinal);
        Assert.Contains("A CI não executa implantação", ciDocs, StringComparison.Ordinal);
        Assert.Contains("ci-cd-quality-gates.md", qualityIndex, StringComparison.Ordinal);
        Assert.Contains("generate-source-inventory.ps1", projectMap, StringComparison.Ordinal);
    }

    [Fact]
    public void ContainerAndVendorAudit_AreResilientAcrossCiRunnerImagesAndLineEndings()
    {
        var dockerfile = ReadRepoFile("Dockerfile");
        var vendorAudit = ReadRepoFile("tools", "audit-vendor-dependencies.ps1");
        var supplyChainDocs = ReadRepoFile("docs", "seguranca-privacidade", "supply-chain-review.md");

        Assert.Contains("mkdir -p /app/data /app/Logs", dockerfile, StringComparison.Ordinal);
        Assert.Contains("chown -R app:app /app", dockerfile, StringComparison.Ordinal);
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
