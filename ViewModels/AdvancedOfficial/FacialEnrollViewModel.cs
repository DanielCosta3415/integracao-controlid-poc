using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.ViewModels.AdvancedOfficial
{
    public class FacialEnrollViewModel
    {
        [Required(ErrorMessage = "Informe ao menos um ID de usuário.")]
        [Display(Name = "IDs dos usuários")]
        public string UserIdsCsv { get; set; } = "1";

        [Display(Name = "Rejeitar faces já cadastradas")]
        public bool MatchDuplicates { get; set; }

        [Display(Name = "Arquivos para cadastro em lote")]
        public List<IFormFile> BatchFiles { get; set; } = [];

        [Display(Name = "Arquivo para teste facial")]
        public IFormFile? TestFile { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;
        public string ResultMessage { get; set; } = string.Empty;
        public string ResultStatusType { get; set; } = string.Empty;
        public string GetListResponseJson { get; set; } = string.Empty;
        public string SetListResponseJson { get; set; } = string.Empty;
        public string TestResponseJson { get; set; } = string.Empty;
    }
}
