using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.Catra
{
    /// <summary>
    /// ViewModel para exibir a lista de eventos de catraca.
    /// </summary>
    public class CatraEventListViewModel
    {
        public List<CatraEventViewModel> CatraEvents { get; set; } = new List<CatraEventViewModel>();
        public CatraOpenViewModel OpenCommand { get; set; } = new CatraOpenViewModel();
        public long? LeftTurns { get; set; }
        public long? RightTurns { get; set; }
        public long? EntranceTurns { get; set; }
        public long? ExitTurns { get; set; }
        public long? TotalTurns { get; set; }
        public string CatraInfoRawJson { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
