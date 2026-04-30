using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.Devices
{
    /// <summary>
    /// ViewModel para exibir a lista de dispositivos.
    /// </summary>
    public class DeviceListViewModel
    {
        public List<DeviceViewModel> Devices { get; set; } = new List<DeviceViewModel>();
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
