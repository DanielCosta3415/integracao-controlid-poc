using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Integracao.ControlID.PoC.E2E;

internal sealed class E2EEnvironment : IAsyncDisposable
{
    private readonly List<Process> _processes = [];
    private readonly string _runtimeDirectory;

    private E2EEnvironment(string root, string runtimeDirectory, Uri appUrl, Uri stubUrl)
    {
        Root = root;
        _runtimeDirectory = runtimeDirectory;
        AppUrl = appUrl;
        StubUrl = stubUrl;
    }

    public string Root { get; }
    public Uri AppUrl { get; }
    public Uri StubUrl { get; }
    public string ScreenshotDirectory => Path.Combine(Root, "artifacts", "e2e", "screenshots");
    public string BaselineDirectory => Path.Combine(Root, "tests", "Integracao.ControlID.PoC.E2E", "Snapshots");

    public static async Task<E2EEnvironment> StartAsync(CancellationToken cancellationToken)
    {
        var root = FindRepositoryRoot();
        var runtimeDirectory = Path.Combine(root, "artifacts", "e2e", "runtime", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(runtimeDirectory);

        var appUrl = new Uri($"http://127.0.0.1:{GetAvailablePort()}");
        var stubUrl = new Uri($"http://127.0.0.1:{GetAvailablePort()}");
        var environment = new E2EEnvironment(root, runtimeDirectory, appUrl, stubUrl);

        environment._processes.Add(environment.StartDotnetProcess(
            Path.Combine(root, "tools", "ControlIdDeviceStub", "bin", "Debug", "net10.0", "ControlIdDeviceStub.dll"),
            "stub",
            new Dictionary<string, string?> { ["CONTROLID_STUB_URL"] = stubUrl.ToString().TrimEnd('/') }));
        await WaitUntilReadyAsync(new Uri(stubUrl, "/__stub/status"), cancellationToken);

        environment._processes.Add(environment.StartDotnetProcess(
            Path.Combine(root, "bin", "Debug", "net10.0", "Integracao.ControlID.PoC.dll"),
            "app",
            new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Development",
                ["ASPNETCORE_URLS"] = appUrl.ToString().TrimEnd('/'),
                ["ConnectionStrings__DefaultConnection"] = $"Data Source={Path.Combine(runtimeDirectory, "e2e.db")}",
                ["DataProtection__KeyPath"] = Path.Combine(runtimeDirectory, "data-protection-keys"),
                ["Database__ApplyMigrationsOnStartup"] = "true",
                ["Session__CookieSecure"] = "SameAsRequest",
                ["CallbackSecurity__RequireSharedKey"] = "false",
                ["CallbackSecurity__RequireSignedRequests"] = "false",
                ["ControlIDApi__RequireAllowedDeviceHosts"] = "false",
                ["Demo__StubUrl"] = stubUrl.ToString().TrimEnd('/'),
                ["OpenApi__Enabled"] = "true"
            }));
        await WaitUntilReadyAsync(new Uri(appUrl, "/health/ready"), cancellationToken);
        return environment;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var process in _processes.AsEnumerable().Reverse())
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                    await process.WaitForExitAsync();
                }
            }
            catch (InvalidOperationException)
            {
            }
            finally
            {
                process.Dispose();
            }
        }

        try
        {
            if (Directory.Exists(_runtimeDirectory))
                Directory.Delete(_runtimeDirectory, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private Process StartDotnetProcess(string assemblyPath, string name, IReadOnlyDictionary<string, string?> environment)
    {
        if (!File.Exists(assemblyPath))
            throw new FileNotFoundException($"Compile a solucao e o simulador antes do E2E: {assemblyPath}", assemblyPath);

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = Root,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        startInfo.ArgumentList.Add(assemblyPath);
        foreach (var item in environment)
            startInfo.Environment[item.Key] = item.Value;

        var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Nao foi possivel iniciar {name}.");
        _ = DrainOutputAsync(process.StandardOutput, Path.Combine(_runtimeDirectory, $"{name}.stdout.log"));
        _ = DrainOutputAsync(process.StandardError, Path.Combine(_runtimeDirectory, $"{name}.stderr.log"));
        return process;
    }

    private static async Task DrainOutputAsync(StreamReader reader, string outputPath)
    {
        await using var writer = new StreamWriter(outputPath, append: true);
        while (await reader.ReadLineAsync() is { } line)
            await writer.WriteLineAsync(line);
    }

    private static async Task WaitUntilReadyAsync(Uri uri, CancellationToken cancellationToken)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTimeOffset.UtcNow.AddSeconds(30);
        Exception? lastError = null;
        while (DateTimeOffset.UtcNow < deadline)
        {
            try
            {
                using var response = await client.GetAsync(uri, cancellationToken);
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch (HttpRequestException ex)
            {
                lastError = ex;
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                lastError = ex;
            }

            await Task.Delay(250, cancellationToken);
        }

        throw new TimeoutException($"O endpoint {uri} nao ficou pronto.", lastError);
    }

    private static int GetAvailablePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Integracao.ControlID.PoC.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Raiz do repositorio nao encontrada.");
    }
}
