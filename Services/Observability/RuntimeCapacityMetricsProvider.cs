using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Integracao.ControlID.PoC.Services.Observability;

public static class RuntimeCapacityMetricsProvider
{
    private const string ProcessMemoryMetric = "controlid.runtime.process.memory.bytes";
    private const string ManagedHeapMetric = "controlid.runtime.managed_heap.bytes";
    private const string StorageLocalMetric = "controlid.runtime.storage.local.bytes";
    private const string DiskTotalMetric = "controlid.runtime.disk.total.bytes";
    private const string DiskFreeMetric = "controlid.runtime.disk.free.bytes";
    private const string DiskFreePercentMetric = "controlid.runtime.disk.free.percent";
    private const string StorageScanTruncatedMetric = "controlid.runtime.storage.scan.truncated";

    /// <summary>
    /// Records coarse local capacity gauges without exposing host names, file paths,
    /// connection strings or artifact names in metric labels.
    /// </summary>
    public static void RecordSnapshot(IServiceProvider services)
    {
        var configuration = services.GetService(typeof(IConfiguration)) as IConfiguration;
        var environment = services.GetService(typeof(IHostEnvironment)) as IHostEnvironment;
        var contentRoot = environment?.ContentRootPath ?? AppContext.BaseDirectory;
        var maxFilesPerScope = Math.Clamp(
            configuration?.GetValue<int?>("Observability:CapacityMaxFilesPerScope") ?? 10_000,
            100,
            100_000);

        OperationalMetrics.RecordGauge(ProcessMemoryMetric, Environment.WorkingSet, ("scope", "working_set"));
        OperationalMetrics.RecordGauge(ManagedHeapMetric, GC.GetTotalMemory(forceFullCollection: false), ("scope", "managed_heap"));

        RecordStorageSize("sqlite", ResolveSqliteFileSet(configuration, contentRoot), maxFilesPerScope);
        RecordStorageSize("logs", ResolvePath(contentRoot, configuration?["Logging:File:Path"] ?? "Logs"), maxFilesPerScope);
        RecordStorageSize("artifacts", ResolvePath(contentRoot, "artifacts"), maxFilesPerScope);
        RecordStorageSize("reports", ResolvePath(contentRoot, Path.Combine("docs", "reports")), maxFilesPerScope);

        RecordDiskCapacity("data", ResolveDirectoryForDisk(ResolveSqliteDataSource(configuration, contentRoot)));
        RecordDiskCapacity("logs", ResolvePath(contentRoot, configuration?["Logging:File:Path"] ?? "Logs"));
    }

    private static void RecordStorageSize(string scope, IEnumerable<string> paths, int maxFiles)
    {
        long total = 0;
        var truncated = false;
        foreach (var path in paths)
        {
            var result = GetPathSize(path, maxFiles);
            total += result.Bytes;
            truncated |= result.Truncated;
        }

        OperationalMetrics.RecordGauge(StorageLocalMetric, total, ("scope", scope));
        OperationalMetrics.RecordGauge(StorageScanTruncatedMetric, truncated ? 1 : 0, ("scope", scope));
    }

    private static void RecordStorageSize(string scope, string path, int maxFiles)
    {
        var result = GetPathSize(path, maxFiles);
        OperationalMetrics.RecordGauge(StorageLocalMetric, result.Bytes, ("scope", scope));
        OperationalMetrics.RecordGauge(StorageScanTruncatedMetric, result.Truncated ? 1 : 0, ("scope", scope));
    }

    private static void RecordDiskCapacity(string scope, string path)
    {
        try
        {
            var directory = Directory.Exists(path) ? path : Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(directory))
                return;

            var root = Path.GetPathRoot(Path.GetFullPath(directory));
            if (string.IsNullOrWhiteSpace(root))
                return;

            var drive = new DriveInfo(root);
            if (!drive.IsReady || drive.TotalSize <= 0)
                return;

            OperationalMetrics.RecordGauge(DiskTotalMetric, drive.TotalSize, ("scope", scope));
            OperationalMetrics.RecordGauge(DiskFreeMetric, drive.AvailableFreeSpace, ("scope", scope));
            OperationalMetrics.RecordGauge(
                DiskFreePercentMetric,
                Math.Round((double)drive.AvailableFreeSpace / drive.TotalSize * 100, 3),
                ("scope", scope));
        }
        catch (IOException)
        {
            return;
        }
        catch (UnauthorizedAccessException)
        {
            return;
        }
        catch (ArgumentException)
        {
            return;
        }
    }

    private static IEnumerable<string> ResolveSqliteFileSet(IConfiguration? configuration, string contentRoot)
    {
        var dataSource = ResolveSqliteDataSource(configuration, contentRoot);
        yield return dataSource;
        yield return dataSource + "-wal";
        yield return dataSource + "-shm";
    }

    private static string ResolveSqliteDataSource(IConfiguration? configuration, string contentRoot)
    {
        var connectionString = configuration?.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            return ResolvePath(contentRoot, "integracao_controlid.db");

        try
        {
            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };

            if (builder.TryGetValue("Data Source", out var dataSource) ||
                builder.TryGetValue("DataSource", out dataSource))
            {
                return ResolvePath(contentRoot, Convert.ToString(dataSource) ?? "integracao_controlid.db");
            }
        }
        catch (ArgumentException)
        {
            return ResolvePath(contentRoot, "integracao_controlid.db");
        }

        return ResolvePath(contentRoot, "integracao_controlid.db");
    }

    private static string ResolvePath(string contentRoot, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return contentRoot;

        return Path.IsPathRooted(path)
            ? Path.GetFullPath(path)
            : Path.GetFullPath(Path.Combine(contentRoot, path));
    }

    private static string ResolveDirectoryForDisk(string path)
    {
        if (Directory.Exists(path))
            return path;

        return Path.GetDirectoryName(path) ?? AppContext.BaseDirectory;
    }

    private static PathSizeResult GetPathSize(string path, int maxFiles)
    {
        try
        {
            if (File.Exists(path))
                return new PathSizeResult(new FileInfo(path).Length, false);

            if (!Directory.Exists(path))
                return new PathSizeResult(0, false);

            long bytes = 0;
            var count = 0;
            foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                if (count++ >= maxFiles)
                    return new PathSizeResult(bytes, true);

                bytes += new FileInfo(file).Length;
            }

            return new PathSizeResult(bytes, false);
        }
        catch (IOException)
        {
            return new PathSizeResult(0, false);
        }
        catch (UnauthorizedAccessException)
        {
            return new PathSizeResult(0, false);
        }
    }

    private readonly record struct PathSizeResult(long Bytes, bool Truncated);
}
