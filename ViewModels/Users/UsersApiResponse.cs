using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.Users
{
    public class UsersApiResponse
    {
        public List<UserDto> Users { get; set; } = new();
    }
}
