using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.Media
{
    /// <summary>
    /// ViewModel para exibir a lista de fotos.
    /// </summary>
    public class PhotoListViewModel
    {
        public List<PhotoViewModel> Photos { get; set; } = [];
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
