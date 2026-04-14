namespace Integracao.ControlID.PoC.ViewModels.Users
{
    /// <summary>
    /// ViewModel para confirmação de exclusão de usuário.
    /// </summary>
    public class UserDeleteViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Registration { get; set; } = string.Empty;
    }
}
