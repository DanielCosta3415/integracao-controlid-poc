using System.Text;

namespace Integracao.ControlID.PoC.Models.Security;

public static class LocalIdentityPolicy
{
    public const int NameMaxLength = 160;
    public const int UsernameMinLength = 3;
    public const int UsernameMaxLength = 128;
    public const int EmailMaxLength = 254;
    public const int PhoneMaxLength = 32;
    public const int PasswordMinLength = 12;
    public const int PasswordMaxLength = 128;
    public const string UsernameAllowedPattern = @"^[A-Za-z0-9._@-]+$";

    public static string NormalizeIdentifier(string value)
    {
        return value.Trim().Normalize(NormalizationForm.FormKC).ToUpperInvariant();
    }
}
