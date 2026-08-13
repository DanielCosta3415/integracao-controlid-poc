namespace Integracao.ControlID.PoC.Services.Observability;

public sealed class SensitiveDataProtectionVerificationState
{
    private long _verifiedAtUtcTicks;

    public bool IsFresh(TimeSpan maximumAge)
    {
        var verifiedAtUtcTicks = Interlocked.Read(ref _verifiedAtUtcTicks);
        if (verifiedAtUtcTicks == 0)
            return false;

        var elapsed = DateTimeOffset.UtcNow - new DateTimeOffset(verifiedAtUtcTicks, TimeSpan.Zero);
        return elapsed >= TimeSpan.Zero && elapsed <= maximumAge;
    }

    public void MarkVerified()
    {
        Interlocked.Exchange(ref _verifiedAtUtcTicks, DateTimeOffset.UtcNow.UtcTicks);
    }

    public void Invalidate()
    {
        Interlocked.Exchange(ref _verifiedAtUtcTicks, 0);
    }
}
