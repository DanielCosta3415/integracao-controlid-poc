using Integracao.ControlID.PoC.Helpers;

namespace Integracao.ControlID.PoC.Tests.Helpers;

public class CryptoHelperTests
{
    [Fact]
    public void VerifyPassword_AcceptsPbkdf2Hash()
    {
        var hash = CryptoHelper.HashPassword("senha-segura");

        Assert.True(CryptoHelper.IsPbkdf2Hash(hash));
        Assert.True(CryptoHelper.VerifyPassword("senha-segura", hash));
        Assert.False(CryptoHelper.VerifyPassword("senha-incorreta", hash));
    }

    [Fact]
    public void VerifyPassword_AcceptsLegacySha256Hash()
    {
        var salt = CryptoHelper.GenerateSalt();
        var legacyHash = CryptoHelper.ComputeSha256Hash("senha-legada", salt);

        Assert.True(CryptoHelper.VerifyPassword("senha-legada", legacyHash, salt));
    }
}
