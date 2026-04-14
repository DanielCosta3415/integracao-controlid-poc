using System;

namespace Integracao.ControlID.PoC.ViewModels.Users
{
    /// <summary>
    /// ViewModel simplificado para exibição e detalhes de usuário.
    /// </summary>
        public class UserViewModel
        {
            public long Id { get; set; }
        public string Registration { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? BeginTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
