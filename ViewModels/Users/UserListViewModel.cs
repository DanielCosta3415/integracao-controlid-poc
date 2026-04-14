using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.Users
{
    /// <summary>
    /// ViewModel para exibir a lista de usuários.
    /// </summary>
    public class UserListViewModel
    {
        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
