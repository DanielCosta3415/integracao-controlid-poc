namespace Integracao.ControlID.PoC.ViewModels.Media
{
    /// <summary>
    /// ViewModel para confirmação de exclusão de foto.
    /// </summary>
    public class PhotoDeleteViewModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string FileName { get; set; } = string.Empty;
    }
}
