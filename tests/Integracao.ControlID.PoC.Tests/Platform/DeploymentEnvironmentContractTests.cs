namespace Integracao.ControlID.PoC.Tests.Platform;

public class DeploymentEnvironmentContractTests
{
    [Fact]
    public void Dockerfile_DefinesNonRootRuntimeHealthCheckAndPersistentPaths()
    {
        var dockerfile = ReadRepoFile("Dockerfile");

        Assert.Contains("ARG DOTNET_SDK_VERSION=10.0.302", dockerfile);
        Assert.Contains("ARG DOTNET_RUNTIME_VERSION=10.0.11", dockerfile);
        Assert.Contains("FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_SDK_VERSION}-noble AS build", dockerfile);
        Assert.Contains("FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_RUNTIME_VERSION}-noble AS runtime", dockerfile);
        Assert.Contains("dotnet restore ./Integracao.ControlID.PoC.csproj --locked-mode", dockerfile);
        Assert.Contains("ASPNETCORE_URLS=http://+:8080", dockerfile);
        Assert.Contains("Data Source=/app/data/integracao_controlid.db", dockerfile);
        Assert.Contains("DataProtection__KeyPath=/app/data/data-protection-keys", dockerfile);
        Assert.Contains("apt-get install --yes --no-install-recommends curl", dockerfile);
        Assert.Contains("rm -rf /var/lib/apt/lists/*", dockerfile);
        Assert.Contains("USER app", dockerfile);
        Assert.Contains("EXPOSE 8080", dockerfile);
        Assert.Contains("HEALTHCHECK", dockerfile);
        Assert.Contains("curl --fail --silent --show-error", dockerfile);
        Assert.Contains("/health/ready", dockerfile);
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
        Assert.Contains("DataProtection__KeyPath", compose);
        Assert.Contains("DataProtection__CertificatePath", compose);
        Assert.Contains("DataProtection__CertificatePasswordFile", compose);
        Assert.Contains("Database__Encryption__RequireProtectedSensitiveColumns: \"true\"", compose);
        Assert.Contains("Database__Encryption__EncryptedVolumeAttested", compose);
        Assert.Contains("Security__RequireHttps: \"true\"", compose);
        Assert.Contains("ControlIDApi__RequireHttpsDeviceUrls: \"true\"", compose);
        Assert.Contains("controlid-logs:/app/Logs", compose);
        Assert.Contains("Database__ApplyMigrationsOnStartup", compose);
        Assert.Contains("Database__ExitAfterMigrations", compose);
        Assert.Contains("ControlIDApi__MaxResponseBodyBytes", compose);
        Assert.Contains("CallbackSecurity__MaxTrackedNonces", compose);
        Assert.Contains("/health/ready", compose);
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
    public void RuntimeSecurity_BlocksUnsafeNonDevelopmentEnvironmentValues()
    {
        var program = ReadRepoFile("Program.cs");
        var runtimeSecurity = ReadRepoFile("Services", "Security", "RuntimeSecurityValidator.cs");

        Assert.Contains("RuntimeSecurityValidator.Validate(app)", program);
        Assert.Contains("CallbackSecurity:SharedKey must be a non-placeholder value with at least 32 characters", runtimeSecurity);
        Assert.Contains("ForwardedHeaders:KnownProxies must list trusted reverse proxy IPs", runtimeSecurity);
        Assert.Contains("ControlIDApi:AllowedDeviceHosts must not contain placeholder values", runtimeSecurity);
        Assert.Contains("DataProtection:KeyPath must point to persistent storage", runtimeSecurity);
        Assert.Contains("DataProtection:CertificatePath must point to a PKCS#12 certificate", runtimeSecurity);
        Assert.Contains("Database:Encryption:RequireProtectedSensitiveColumns must be true", runtimeSecurity);
        Assert.Contains("ControlIDApi:RequireHttpsDeviceUrls must be true", runtimeSecurity);
        Assert.Contains("Security:RequireHttps must be true", runtimeSecurity);
        Assert.Contains("options.ShutdownTimeout", program);
        Assert.Contains("app.UseForwardedHeaders()", program);
    }

    [Fact]
    public void EnvironmentExamplesAndRunbookDocumentRollbackAndRequiredSettings()
    {
        var envExample = ReadRepoFile(".env.example");
        var staging = ReadRepoFile("appsettings.Staging.json");
        var production = ReadRepoFile("appsettings.Production.json");
        var runbook = ReadRepoFile("docs", "operacao", "deployment-runbook.md");

        Assert.Contains("CallbackSecurity__Shared" + "Key=replace-with-at-least-32-random-characters", envExample);
        Assert.Contains("DataProtection__KeyPath=/app/data/data-protection-keys", envExample);
        Assert.Contains("DATA_PROTECTION_CERTIFICATE_FILE=", envExample);
        Assert.Contains("Database__Encryption__EncryptedVolumeAttested=false", envExample);
        Assert.Contains("ControlIDApi__RequireHttpsDeviceUrls=true", envExample);
        Assert.Contains("\"RequireSignedRequests\": true", staging);
        Assert.Contains("\"RequireAllowedDeviceHosts\": true", production);
        Assert.Contains("Procedimento de implantação", runbook);
        Assert.Contains("Reversão técnica", runbook);
        Assert.Contains("ForwardedHeaders__KnownProxies__0", runbook);
        Assert.Contains("Database__ExitAfterMigrations=true", runbook);
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
