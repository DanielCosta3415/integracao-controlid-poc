namespace Integracao.ControlID.PoC.Options;

public sealed class ControlIdConcurrencyOptions
{
    public int MaxConcurrentRequestsPerDevice { get; set; } = 4;

    public int QueueLimitPerDevice { get; set; } = 16;

    public int MaxTrackedDevices { get; set; } = 128;
}
