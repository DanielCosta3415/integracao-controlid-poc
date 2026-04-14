namespace Integracao.ControlID.PoC.ViewModels.Users
{
    public class UserDto
    {
        public long Id { get; set; }
        public string Registration { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int UserTypeId { get; set; }
        public long? BeginTime { get; set; }
        public long? EndTime { get; set; }
        // Outros campos se necessário
    }
}
