using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.OfficialObjects
{
    public class OfficialObjectsViewModel
    {
        public string ActiveSection { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string ResultMessage { get; set; } = string.Empty;
        public string ResultStatusType { get; set; } = string.Empty;
        public string ResponseJson { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Objeto oficial")]
        public string SelectedObjectName { get; set; } = "areas";

        [Display(Name = "Filtro opcional de leitura (JSON)")]
        public string LoadWhereJson { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Payload de criacao (JSON)")]
        public string CreateValuesJson { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Payload de create-or-modify (JSON)")]
        public string UpsertValuesJson { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Filtro de alteracao (JSON)")]
        public string ModifyWhereJson { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Valores para alteracao (JSON)")]
        public string ModifyValuesJson { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Filtro de exclusao (JSON)")]
        public string DestroyWhereJson { get; set; } = string.Empty;

        [Display(Name = "Confirmacao de remocao")]
        public string DestroyConfirmationPhrase { get; set; } = string.Empty;

        public IReadOnlyList<OfficialObjectDefinition> Definitions { get; set; } = Array.Empty<OfficialObjectDefinition>();
    }

    public record OfficialObjectDefinition(
        string Name,
        string Summary,
        string KeyHints,
        string CreateSampleJson,
        string LoadWhereSampleJson,
        string ModifyWhereSampleJson,
        string ModifyValuesSampleJson,
        string DedicatedScreen);
}
