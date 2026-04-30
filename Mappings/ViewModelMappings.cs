using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.ViewModels.Users;
using Integracao.ControlID.PoC.ViewModels.Groups;
// Adicione outros namespaces de ViewModels conforme necessário

namespace Integracao.ControlID.PoC.Mappings
{
    public static class ViewModelMappings
    {
        // User → UserViewModel
        public static UserViewModel ToViewModel(this User user)
        {
            return new UserViewModel
            {
                Id = user.Id,
                Name = user.Name ?? string.Empty,
                Registration = user.Registration ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Status = user.Status ?? string.Empty,
                BeginTime = user.BeginTime.HasValue ? DateTimeOffset.FromUnixTimeSeconds(user.BeginTime.Value).DateTime : null,
                EndTime = user.EndTime.HasValue ? DateTimeOffset.FromUnixTimeSeconds(user.EndTime.Value).DateTime : null,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        // UserLocal → UserViewModel
        public static UserViewModel ToViewModel(this UserLocal user)
        {
            return new UserViewModel
            {
                Id = user.Id,
                Name = user.Name ?? string.Empty,
                Registration = user.Registration ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Status = user.Status ?? string.Empty,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        // Group → GroupViewModel
        public static GroupViewModel ToViewModel(this Group group)
        {
            return new GroupViewModel
            {
                Id = group.Id,
                Name = group.Name
            };
        }

        // GroupLocal → GroupViewModel
        public static GroupViewModel ToViewModel(this GroupLocal group)
        {
            return new GroupViewModel
            {
                Id = group.Id,
                Name = group.Name
            };
        }

        // AccessLog → AccessLogViewModel
        public static Integracao.ControlID.PoC.ViewModels.AccessLogs.AccessLogViewModel ToViewModel(this AccessLog log)
        {
            return new Integracao.ControlID.PoC.ViewModels.AccessLogs.AccessLogViewModel
            {
                Id = log.Id,
                UserId = log.UserId,
                DeviceId = log.DeviceId,
                Event = log.Event,
                PortalId = log.PortalId,
                Info = log.Info,
                Time = log.Time,
                CreatedAt = log.CreatedAt
            };
        }

        // AccessLogLocal → AccessLogViewModel
        public static Integracao.ControlID.PoC.ViewModels.AccessLogs.AccessLogViewModel ToViewModel(this AccessLogLocal log)
        {
            return new Integracao.ControlID.PoC.ViewModels.AccessLogs.AccessLogViewModel
            {
                Id = log.Id,
                UserId = log.UserId,
                DeviceId = log.DeviceId,
                Event = log.Event,
                PortalId = log.PortalId,
                Info = log.Info,
                Time = log.Time,
                CreatedAt = log.CreatedAt
            };
        }

        // Adicione métodos de mapping para outros modelos/conjuntos de ViewModels conforme necessário.
        // Exemplo:
        // public static CardViewModel ToViewModel(this Card card) { ... }
        // public static DeviceViewModel ToViewModel(this Device device) { ... }
    }
}
