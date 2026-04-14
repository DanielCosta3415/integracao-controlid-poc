namespace Integracao.ControlID.PoC.ViewModels.Devices
{
    /// <summary>
    /// ViewModel para confirmação de exclusão de dispositivo.
    /// </summary>
    public class DeviceDeleteViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
