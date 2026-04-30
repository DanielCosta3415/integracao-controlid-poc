namespace Integracao.ControlID.PoC.Options;

public sealed class ControlIdCircuitBreakerOptions
{
    public bool Enabled { get; set; } = true;

    public int FailureThreshold { get; set; } = 5;

    public int BreakDurationSeconds { get; set; } = 30;
}
