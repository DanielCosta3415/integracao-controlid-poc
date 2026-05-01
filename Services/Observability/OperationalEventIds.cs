using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Services.Observability;

public static class OperationalEventIds
{
    public static readonly EventId RequestCompleted = new(1000, nameof(RequestCompleted));
    public static readonly EventId RequestFailed = new(1001, nameof(RequestFailed));
    public static readonly EventId UnhandledException = new(1002, nameof(UnhandledException));

    public static readonly EventId LocalAuthSucceeded = new(2000, nameof(LocalAuthSucceeded));
    public static readonly EventId LocalAuthFailed = new(2001, nameof(LocalAuthFailed));
    public static readonly EventId LocalLogoutSucceeded = new(2002, nameof(LocalLogoutSucceeded));
    public static readonly EventId DeviceAuthSucceeded = new(2010, nameof(DeviceAuthSucceeded));
    public static readonly EventId DeviceAuthFailed = new(2011, nameof(DeviceAuthFailed));

    public static readonly EventId OfficialApiInvocationStarted = new(3000, nameof(OfficialApiInvocationStarted));
    public static readonly EventId OfficialApiInvocationCompleted = new(3001, nameof(OfficialApiInvocationCompleted));
    public static readonly EventId OfficialApiInvocationBlocked = new(3002, nameof(OfficialApiInvocationBlocked));
    public static readonly EventId OfficialApiInvocationFailed = new(3003, nameof(OfficialApiInvocationFailed));

    public static readonly EventId CallbackAccepted = new(4000, nameof(CallbackAccepted));
    public static readonly EventId CallbackRejected = new(4001, nameof(CallbackRejected));
    public static readonly EventId CallbackPersistenceFailed = new(4002, nameof(CallbackPersistenceFailed));

    public static readonly EventId PushQueued = new(5000, nameof(PushQueued));
    public static readonly EventId PushDelivered = new(5001, nameof(PushDelivered));
    public static readonly EventId PushResultStored = new(5002, nameof(PushResultStored));
    public static readonly EventId PushQueueCleared = new(5003, nameof(PushQueueCleared));
    public static readonly EventId PushRejected = new(5004, nameof(PushRejected));
}
