namespace Integracao.ControlID.PoC.Services.Database;

public static class LocalDataQueryLimits
{
    public const int DefaultListLimit = 500;
    public const int MaxListLimit = 2000;
    public const int MinRetentionDays = 1;
    public const int MaxRetentionDays = 3650;

    public static int NormalizeLimit(int? requestedLimit)
    {
        if (!requestedLimit.HasValue || requestedLimit.Value <= 0)
            return DefaultListLimit;

        return Math.Min(requestedLimit.Value, MaxListLimit);
    }

    public static int NormalizeRetentionDays(int retentionDays)
    {
        return Math.Clamp(retentionDays, MinRetentionDays, MaxRetentionDays);
    }
}
