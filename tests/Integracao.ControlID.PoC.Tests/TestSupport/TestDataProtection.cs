using Integracao.ControlID.PoC.Services.Database;
using Microsoft.AspNetCore.DataProtection;

namespace Integracao.ControlID.PoC.Tests.TestSupport;

public static class TestDataProtection
{
    private static readonly SensitiveDataProtector SharedProtector =
        new(new EphemeralDataProtectionProvider());

    public static SensitiveDataProtector CreateSensitiveDataProtector()
    {
        return SharedProtector;
    }
}
