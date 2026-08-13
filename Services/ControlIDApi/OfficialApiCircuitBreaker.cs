using System.Collections.Concurrent;
using Integracao.ControlID.PoC.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Services.ControlIDApi;

public sealed class OfficialApiCircuitBreaker
{
    private readonly ConcurrentDictionary<string, CircuitState> _states = new();
    private readonly object _maintenanceGate = new();
    private readonly ControlIdCircuitBreakerOptions _options;
    private readonly TimeProvider _timeProvider;

    public OfficialApiCircuitBreaker(
        IOptions<ControlIdCircuitBreakerOptions> options,
        TimeProvider? timeProvider = null)
    {
        _options = options.Value;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public bool TryAcquire(string endpointId, string deviceTarget, out TimeSpan retryAfter)
    {
        retryAfter = TimeSpan.Zero;

        if (!_options.Enabled)
            return true;

        var state = GetOrCreateState(BuildKey(endpointId, deviceTarget));
        lock (state)
        {
            var now = _timeProvider.GetUtcNow();
            state.LastTouchedUtc = now;
            if (state.OpenUntilUtc is null)
                return true;

            if (state.OpenUntilUtc <= now)
            {
                state.HalfOpenProbeActive = true;
                state.OpenUntilUtc = now.Add(BreakDuration);
                return true;
            }

            retryAfter = state.OpenUntilUtc.Value - now;
            return false;
        }
    }

    public void RecordSuccess(string endpointId, string deviceTarget)
    {
        if (!_options.Enabled)
            return;

        if (!_states.TryGetValue(BuildKey(endpointId, deviceTarget), out var state))
            return;

        lock (state)
        {
            state.ConsecutiveFailures = 0;
            state.OpenUntilUtc = null;
            state.HalfOpenProbeActive = false;
            state.LastTouchedUtc = _timeProvider.GetUtcNow();
        }
    }

    public void RecordFailure(string endpointId, string deviceTarget)
    {
        if (!_options.Enabled)
            return;

        var state = GetOrCreateState(BuildKey(endpointId, deviceTarget));
        lock (state)
        {
            state.ConsecutiveFailures++;
            state.LastTouchedUtc = _timeProvider.GetUtcNow();
            if (state.HalfOpenProbeActive || state.ConsecutiveFailures >= FailureThreshold)
            {
                state.OpenUntilUtc = state.LastTouchedUtc.Add(BreakDuration);
                state.HalfOpenProbeActive = false;
            }
        }
    }

    public static bool IsTransientStatusCode(int statusCode)
    {
        return statusCode == StatusCodes.Status408RequestTimeout ||
               statusCode == StatusCodes.Status429TooManyRequests ||
               statusCode >= StatusCodes.Status500InternalServerError;
    }

    private int FailureThreshold => Math.Clamp(_options.FailureThreshold, 1, 100);

    private TimeSpan BreakDuration => TimeSpan.FromSeconds(Math.Clamp(_options.BreakDurationSeconds, 1, 3600));

    private int MaxTrackedStates => Math.Clamp(_options.MaxTrackedStates, 16, 4096);

    private TimeSpan StateRetention => TimeSpan.FromSeconds(Math.Max(
        Math.Clamp(_options.StateRetentionSeconds, 60, 86_400),
        BreakDuration.TotalSeconds));

    private CircuitState GetOrCreateState(string key)
    {
        if (_states.TryGetValue(key, out var existing))
            return existing;

        lock (_maintenanceGate)
        {
            if (_states.TryGetValue(key, out existing))
                return existing;

            var now = _timeProvider.GetUtcNow();
            foreach (var candidate in _states)
            {
                if (now - candidate.Value.LastTouchedUtc > StateRetention)
                    _states.TryRemove(candidate.Key, out _);
            }

            if (_states.Count >= MaxTrackedStates)
            {
                var oldest = _states.MinBy(static candidate => candidate.Value.LastTouchedUtc);
                if (!string.IsNullOrEmpty(oldest.Key))
                    _states.TryRemove(oldest.Key, out _);
            }

            return _states.GetOrAdd(key, _ => new CircuitState { LastTouchedUtc = now });
        }
    }

    private static string BuildKey(string endpointId, string deviceTarget)
    {
        return $"{deviceTarget.Trim().ToUpperInvariant()}::{endpointId.Trim().ToUpperInvariant()}";
    }

    private sealed class CircuitState
    {
        public int ConsecutiveFailures { get; set; }

        public DateTimeOffset? OpenUntilUtc { get; set; }

        public bool HalfOpenProbeActive { get; set; }

        public DateTimeOffset LastTouchedUtc { get; set; }
    }
}
