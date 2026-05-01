namespace Integracao.ControlID.PoC.Tests.Platform;

public class DeploymentEnvironmentContractTests
{
    [Fact]
    public void Dockerfile_DefinesNonRootRuntimeHealthCheckAndPersistentPaths()
    {
        var dockerfile = ReadRepoFile("Dockerfile");

        Assert.Contains("FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-alpine AS build", dockerfile);
        Assert.Contains("dotnet restore ./Integracao.ControlID.PoC.csproj --locked-mode", dockerfile);
        Assert.Contains("ASPNETCORE_URLS=http://+:8080", dockerfile);
        Assert.Contains("Data Source=/app/data/integracao_controlid.db", dockerfile);
        Assert.Contains("USER app", dockerfile);
        Assert.Contains("EXPOSE 8080", dockerfile);
        Assert.Contains("HEALTHCHECK", dockerfile);
        Assert.Contains("/health/live", dockerfile);
    }

    [Fact]
    public void Compose_RequiresProductionSafetyEnvironmentAndVolumes()
    {
        var compose = ReadRepoFile("docker-compose.yml");

        Assert.Contains("AllowedHosts: \"${AllowedHosts:?set AllowedHosts in .env}\"", compose);
        Assert.Contains("CallbackSecurity__SharedKey: \"${CallbackSecurity__SharedKey:?set CallbackSecurity__SharedKey in .env}\"", compose);
        Assert.Contains("ControlIDApi__AllowedDeviceHosts__0: \"${ControlIDApi__AllowedDeviceHosts__0:?set ControlIDApi__AllowedDeviceHosts__0 in .env}\"", compose);
        Assert.Contains("CallbackSecurity__RequireSignedRequests: \"true\"", compose);
        Assert.Contains("OpenApi__Enabled: \"false\"", compose);
        Assert.Contains("Serilog__WriteTo__1__Args__retainedFileCountLimit", compose);
        Assert.Contains("Serilog__WriteTo__1__Args__fileSizeLimitBytes", compose);
        Assert.Contains("controlid-data:/app/data", compose);
        Assert.Contains("controlid-logs:/app/Logs", compose);
        Assert.Contains("/health/live", compose);
    }

    [Fact]
    public void Dockerignore_ExcludesSecretsRuntimeDataAndBuildArtifacts()
    {
        var dockerignore = ReadRepoFile(".dockerignore");

        Assert.Contains(".env", dockerignore);
        Assert.Contains("integracao_controlid.db", dockerignore);
        Assert.Contains("Logs", dockerignore);
        Assert.Contains("artifacts", dockerignore);
        Assert.Contains("bin", dockerignore);
        Assert.Contains("obj", dockerignore);
    }

    [Fact]
    public void Program_BlocksUnsafeNonDevelopmentEnvironmentValues()
    {
        var program = ReadRepoFile("Program.cs");

        Assert.Contains("CallbackSecurity:SharedKey must be a non-placeholder value with at least 32 characters", program);
        Assert.Contains("ForwardedHeaders:KnownProxies must list trusted reverse proxy IPs", program);
        Assert.Contains("ControlIDApi:AllowedDeviceHosts must not contain placeholder values", program);
        Assert.Contains("options.ShutdownTimeout", program);
        Assert.Contains("app.UseForwardedHeaders()", program);
    }

    [Fact]
    public void EnvironmentExamplesAndRunbookDocumentRollbackAndRequiredSettings()
    {
        var envExample = ReadRepoFile(".env.example");
        var staging = ReadRepoFile("appsettings.Staging.json");
        var production = ReadRepoFile("appsettings.Production.json");
        var runbook = ReadRepoFile("docs", "deployment-runbook.md");

        Assert.Contains("CallbackSecurity__Shared" + "Key=replace-with-at-least-32-random-characters", envExample);
        Assert.Contains("\"RequireSignedRequests\": true", staging);
        Assert.Contains("\"RequireAllowedDeviceHosts\": true", production);
        Assert.Contains("Procedimento de deploy", runbook);
        Assert.Contains("Rollback tecnico", runbook);
        Assert.Contains("ForwardedHeaders__KnownProxies__0", runbook);
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
