using System.Collections.Concurrent;
using Integracao.ControlID.PoC.Options;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Services.ControlIDApi;

public sealed class OfficialApiConcurrencyLimiter : IDisposable
{
    private readonly ConcurrentDictionary<string, DeviceGate> _gates = new(StringComparer.OrdinalIgnoreCase);
    private readonly DeviceGate _overflowGate;
    private readonly object _gateCreationLock = new();
    private readonly int _maxTrackedDevices;
    private bool _disposed;

    public OfficialApiConcurrencyLimiter(IOptions<ControlIdConcurrencyOptions> options)
    {
        var value = options.Value;
        var concurrency = Math.Clamp(value.MaxConcurrentRequestsPerDevice, 1, 32);
        var queueLimit = Math.Clamp(value.QueueLimitPerDevice, 0, 256);
        _maxTrackedDevices = Math.Clamp(value.MaxTrackedDevices, 1, 1_024);
        GateFactory = _ => new DeviceGate(concurrency, queueLimit);
        _overflowGate = new DeviceGate(concurrency, queueLimit);
    }

    private Func<string, DeviceGate> GateFactory { get; }

    public async ValueTask<IDisposable> AcquireAsync(string deviceTarget, CancellationToken cancellationToken)
    {
        var normalizedTarget = string.IsNullOrWhiteSpace(deviceTarget) ? "unknown-device" : deviceTarget;
        DeviceGate gate;
        lock (_gateCreationLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!_gates.TryGetValue(normalizedTarget, out gate!))
            {
                gate = _gates.Count < _maxTrackedDevices
                    ? _gates.GetOrAdd(normalizedTarget, GateFactory)
                    : _overflowGate;
            }
        }

        return await gate.AcquireAsync(cancellationToken);
    }

    public void Dispose()
    {
        lock (_gateCreationLock)
            _disposed = true;
    }

    private sealed class DeviceGate
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly int _queueLimit;
        private int _waiting;

        public DeviceGate(int concurrency, int queueLimit)
        {
            _semaphore = new SemaphoreSlim(concurrency, concurrency);
            _queueLimit = queueLimit;
        }

        public async ValueTask<IDisposable> AcquireAsync(CancellationToken cancellationToken)
        {
            if (_semaphore.Wait(0))
                return new Lease(_semaphore);

            var waiting = Interlocked.Increment(ref _waiting);
            if (waiting > _queueLimit)
            {
                Interlocked.Decrement(ref _waiting);
                throw new OfficialApiConcurrencyRejectedException();
            }

            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                return new Lease(_semaphore);
            }
            finally
            {
                Interlocked.Decrement(ref _waiting);
            }
        }
    }

    private sealed class Lease(SemaphoreSlim semaphore) : IDisposable
    {
        private SemaphoreSlim? _semaphore = semaphore;

        public void Dispose()
        {
            Interlocked.Exchange(ref _semaphore, null)?.Release();
        }
    }
}

public sealed class OfficialApiConcurrencyRejectedException : Exception
{
    public OfficialApiConcurrencyRejectedException()
        : base("A fila de comunicação com o equipamento atingiu o limite configurado.")
    {
    }
}
