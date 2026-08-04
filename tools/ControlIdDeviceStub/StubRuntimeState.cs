using System.Collections.Concurrent;

internal sealed class StubRuntimeState
{
    private readonly ConcurrentDictionary<string, StubRequestStatistics> _requests = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _configurationGate = new();
    private StubState _device = new();
    private StubScenario _scenario = StubScenario.Normal;
    private StubDeviceProfile _profile = StubDeviceProfile.Default;
    private int _datasetSize = 1;

    public StubState Device => Volatile.Read(ref _device);
    public StubScenario Scenario => Volatile.Read(ref _scenario);
    public StubDeviceProfile Profile => Volatile.Read(ref _profile);
    public int DatasetSize => Volatile.Read(ref _datasetSize);

    public void ConfigureScenario(string? name, int? delayMs = null, string? endpoint = null, int? responseBytes = null)
    {
        var scenario = StubScenario.Create(name, delayMs, endpoint, responseBytes);
        Volatile.Write(ref _scenario, scenario);
    }

    public void Reset(int datasetSize, string? profileName)
    {
        StubDatasetFactory.ValidateSize(datasetSize);
        var device = new StubState();
        StubDatasetFactory.Populate(device, datasetSize);

        lock (_configurationGate)
        {
            Volatile.Write(ref _device, device);
            Volatile.Write(ref _datasetSize, datasetSize);
            Volatile.Write(ref _profile, StubDeviceProfile.Resolve(profileName));
            Volatile.Write(ref _scenario, StubScenario.Normal);
            _requests.Clear();
        }
    }

    public void RecordRequest(string path, TimeSpan elapsed)
    {
        _requests.GetOrAdd(path, static _ => new StubRequestStatistics()).Record(elapsed);
    }

    public object CreateStatus()
    {
        var requests = _requests
            .OrderBy(static item => item.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                static item => item.Key,
                static item => item.Value.Snapshot(),
                StringComparer.OrdinalIgnoreCase);

        return new
        {
            simulator = "ControlIdDeviceStub",
            loopback_only = true,
            scenario = Scenario,
            profile = Profile,
            dataset_size = DatasetSize,
            supported_dataset_sizes = StubDatasetFactory.SupportedSizes,
            requests
        };
    }
}

internal sealed class StubRequestStatistics
{
    private long _count;
    private long _totalTicks;
    private long _maxTicks;

    public void Record(TimeSpan elapsed)
    {
        var ticks = Math.Max(0, elapsed.Ticks);
        Interlocked.Increment(ref _count);
        Interlocked.Add(ref _totalTicks, ticks);

        while (true)
        {
            var current = Volatile.Read(ref _maxTicks);
            if (ticks <= current || Interlocked.CompareExchange(ref _maxTicks, ticks, current) == current)
                break;
        }
    }

    public object Snapshot()
    {
        var count = Volatile.Read(ref _count);
        var totalTicks = Volatile.Read(ref _totalTicks);
        var maxTicks = Volatile.Read(ref _maxTicks);
        return new
        {
            count,
            average_ms = count == 0 ? 0 : Math.Round(TimeSpan.FromTicks(totalTicks / count).TotalMilliseconds, 3),
            max_ms = Math.Round(TimeSpan.FromTicks(maxTicks).TotalMilliseconds, 3)
        };
    }
}
