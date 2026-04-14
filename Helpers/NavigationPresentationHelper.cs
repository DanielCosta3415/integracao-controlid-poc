using Integracao.ControlID.PoC.ViewModels.Shared;

namespace Integracao.ControlID.PoC.Helpers
{
    public static class NavigationPresentationHelper
    {
        public static string GetExperienceLabel(string? experienceType)
        {
            return (experienceType ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "workspace" => "Fluxo guiado",
                "explorer" => "Exploração técnica",
                "dashboard" => "Painel",
                "console" => "Console",
                "timeline" => "Timeline",
                _ => SecurityTextHelper.NormalizeForDisplay(experienceType, "Operação")
            };
        }

        public static string GetActionLabel(NavigationModuleViewModel module, bool isSecondary = false)
        {
            if (module.IsTechnical)
            {
                return isSecondary ? "Explorar recurso" : "Abrir recurso";
            }

            return isSecondary ? "Abrir tela" : "Abrir módulo";
        }

        public static string GetModuleHint(NavigationModuleViewModel module)
        {
            if (!string.IsNullOrWhiteSpace(module.Prerequisite))
            {
                return SecurityTextHelper.NormalizeForDisplay(module.Prerequisite, string.Empty);
            }

            return module.Visibility == "primary"
                ? "Entrada recomendada neste domínio."
                : "Disponível para apoio operacional e diagnóstico.";
        }
    }
}
