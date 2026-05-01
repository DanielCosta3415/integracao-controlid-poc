namespace Integracao.ControlID.PoC.Services.Security
{
    public static class AppSecurityRoles
    {
        public const string Administrator = "Administrator";
        public const string Operator = "Operator";

        public static string Normalize(string? role)
        {
            return string.Equals(role, Administrator, StringComparison.OrdinalIgnoreCase)
                ? Administrator
                : Operator;
        }
    }
}
