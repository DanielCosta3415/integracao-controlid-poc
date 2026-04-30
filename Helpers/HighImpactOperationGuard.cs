using System;

namespace Integracao.ControlID.PoC.Helpers
{
    public static class HighImpactOperationGuard
    {
        public const string ConfirmNetworkChange = "ALTERAR REDE";
        public const string ConfirmReboot = "REINICIAR";
        public const string ConfirmRebootRecovery = "MODO UPDATE";
        public const string ConfirmDeleteAdmins = "REMOVER ADMINS";
        public const string ConfirmFactoryReset = "RESET FABRICA";
        public const string ConfirmClearPushQueue = "LIMPAR PUSH";
        public const string ConfirmClearMonitorEvents = "LIMPAR EVENTOS";

        public static string BuildDestroyObjectsConfirmation(string objectName)
        {
            return $"DESTROY {objectName}".Trim();
        }

        public static bool IsConfirmed(string? providedPhrase, string expectedPhrase)
        {
            return string.Equals(
                providedPhrase?.Trim(),
                expectedPhrase.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        public static string BuildRequiredMessage(string expectedPhrase)
        {
            return $"Digite exatamente '{expectedPhrase}' para confirmar esta operacao de alto impacto.";
        }
    }
}
