namespace Integracao.ControlID.PoC.Options;

public sealed class SqliteRuntimeOptions
{
    public int BusyTimeoutMilliseconds { get; set; } = 5_000;

    public bool WriteAheadLoggingEnabled { get; set; } = true;

    public string SynchronousMode { get; set; } = "NORMAL";
}
