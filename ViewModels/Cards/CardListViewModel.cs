using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.Cards
{
    /// <summary>
    /// ViewModel para exibir a lista de cartões.
    /// </summary>
    public class CardListViewModel
    {
        public List<CardViewModel> Cards { get; set; } = new List<CardViewModel>();
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
