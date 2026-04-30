using System;
using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.BiometricTemplates
{
    /// <summary>
    /// ViewModel para criação ou edição de template biométrico.
    /// </summary>
    public class BiometricTemplateEditViewModel
    {
        public long? Id { get; set; }

        [Required(ErrorMessage = "Usuário obrigatório.")]
        [Display(Name = "Usuário")]
        public long UserId { get; set; }

        [Required(ErrorMessage = "Template biométrico obrigatório.")]
        [Display(Name = "Template (base64/string)")]
        public string Template { get; set; } = string.Empty;

        [Display(Name = "Tipo (0 = digital, 1 = facial, etc)")]
        public int Type { get; set; }

        [Display(Name = "Posição do Dedo")]
        public int FingerPosition { get; set; }

        [Display(Name = "Tipo do Dedo")]
        public int FingerType { get; set; }

        [Display(Name = "Início da Validade")]
        public DateTime? BeginTime { get; set; }

        [Display(Name = "Fim da Validade")]
        public DateTime? EndTime { get; set; }
    }
}
