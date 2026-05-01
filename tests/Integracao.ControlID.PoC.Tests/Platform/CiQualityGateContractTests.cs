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
        Assert.Contains("global-json-file: global.json", workflow, StringComparison.Ordinal);
        Assert.Contains("cache-dependency-path:", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet restore .\\Integracao.ControlID.PoC.sln --locked-mode", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet build .\\Integracao.ControlID.PoC.sln --no-restore", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet test .\\Integracao.ControlID.PoC.sln --no-build", workflow, StringComparison.Ordinal);
        Assert.Contains(".\\tools\\smoke-localhost.ps1", workflow, StringComparison.Ordinal);
        Assert.Contains(".\\tools\\contract-controlid-stub.ps1", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet format .\\Integracao.ControlID.PoC.sln --verify-no-changes", workflow, StringComparison.Ordinal);
        Assert.Contains("git diff --check", workflow, StringComparison.Ordinal);
        Assert.Contains(".\\tools\\scan-secrets.ps1", workflow, StringComparison.Ordinal);
        Assert.Contains(".\\tools\\observability-check.ps1 -OfflineValidateOnly", workflow, StringComparison.Ordinal);
        Assert.Contains(".\\tools\\operational-readiness-check.ps1", workflow, StringComparison.Ordinal);
        Assert.Contains(".\\tools\\finops-capacity-check.ps1", workflow, StringComparison.Ordinal);
        Assert.Contains(".\\tools\\audit-supply-chain.ps1", workflow, StringComparison.Ordinal);
        Assert.Contains(".\\tools\\external-security-scans.ps1 -InventoryOnly", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet list $target package --vulnerable --include-transitive", workflow, StringComparison.Ordinal);
        Assert.Contains("actions/upload-artifact@v4", workflow, StringComparison.Ordinal);
        Assert.Contains("docker compose config", workflow, StringComparison.Ordinal);
        Assert.Contains("docker build --pull", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("deploy", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("environment:", workflow, StringComparison.OrdinalIgnoreCase);
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
        Assert.Contains("Quality gates obrigatorios", ciDocs, StringComparison.Ordinal);
        Assert.Contains("Reproducao local", ciDocs, StringComparison.Ordinal);
        Assert.Contains("Branch protection recomendada", ciDocs, StringComparison.Ordinal);
        Assert.Contains("A CI nao executa deploy", ciDocs, StringComparison.Ordinal);
        Assert.Contains("docs/ci-cd-quality-gates.md", projectMap, StringComparison.Ordinal);
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
