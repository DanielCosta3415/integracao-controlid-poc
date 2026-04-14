using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.RemoteActions
{
    /// <summary>
    /// ViewModel para execução/envio de uma ação remota.
    /// </summary>
    public class RemoteActionExecuteViewModel
    {
        [Required(ErrorMessage = "Ação obrigatória.")]
        [Display(Name = "Ação")]
        public string Action { get; set; } = string.Empty;

        [Display(Name = "Porta (se aplicável)")]
        public int? Door { get; set; }

        [Display(Name = "Mensagem (se aplicável)")]
        public string Message { get; set; } = string.Empty;

        // Adicione outros campos para parâmetros específicos conforme sua API
    }
}
