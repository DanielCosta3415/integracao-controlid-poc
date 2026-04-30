using System.Collections.Concurrent;
using Integracao.ControlID.PoC.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Services.ControlIDApi;

public sealed class OfficialApiCircuitBreaker
{
    private readonly ConcurrentDictionary<string, CircuitState> _states = new();
    private readonly ControlIdCircuitBreakerOptions _options;

    public OfficialApiCircuitBreaker(IOptions<ControlIdCircuitBreakerOptions> options)
    {
        _options = options.Value;
    }

    public bool TryAcquire(string endpointId, string deviceTarget, out TimeSpan retryAfter)
    {
        retryAfter = TimeSpan.Zero;

        if (!_options.Enabled)
            return true;

        var state = _states.GetOrAdd(BuildKey(endpointId, deviceTarget), _ => new CircuitState());
        lock (state)
        {
            var now = DateTimeOffset.UtcNow;
            if (state.OpenUntilUtc is null || state.OpenUntilUtc <= now)
            {
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

        var state = _states.GetOrAdd(BuildKey(endpointId, deviceTarget), _ => new CircuitState());
        lock (state)
        {
            state.ConsecutiveFailures = 0;
            state.OpenUntilUtc = null;
        }
    }

    public void RecordFailure(string endpointId, string deviceTarget)
    {
        if (!_options.Enabled)
            return;

        var state = _states.GetOrAdd(BuildKey(endpointId, deviceTarget), _ => new CircuitState());
        lock (state)
        {
            state.ConsecutiveFailures++;
            if (state.ConsecutiveFailures >= FailureThreshold)
            {
                state.OpenUntilUtc = DateTimeOffset.UtcNow.Add(BreakDuration);
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

    private static string BuildKey(string endpointId, string deviceTarget)
    {
        return $"{deviceTarget.Trim().ToUpperInvariant()}::{endpointId.Trim().ToUpperInvariant()}";
    }

    private sealed class CircuitState
    {
        public int ConsecutiveFailures { get; set; }

        public DateTimeOffset? OpenUntilUtc { get; set; }
    }
}
