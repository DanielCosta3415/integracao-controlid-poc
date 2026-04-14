using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Models.Database;

namespace Integracao.ControlID.PoC.Mappings
{
    public static class ModelMappings
    {
        // User (API → Local)
        public static UserLocal ToLocal(this User user)
        {
            return new UserLocal
            {
                Id = user.Id,
                Name = user.Name ?? string.Empty,
                Registration = user.Registration ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Status = user.Status ?? string.Empty,
                CreatedAt = user.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = user.UpdatedAt
            };
        }

        // User (Local → API)
        public static User ToApi(this UserLocal user)
        {
            return new User
            {
                Id = user.Id,
                Name = user.Name,
                Registration = user.Registration,
                Email = user.Email,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        // Device (API → Local)
        public static DeviceLocal ToLocal(this Device device)
        {
            return new DeviceLocal
            {
                Id = device.Id,
                Name = device.Name ?? string.Empty,
                IpAddress = device.IpAddress ?? string.Empty,
                Status = device.Status ?? string.Empty,
                SerialNumber = device.SerialNumber ?? string.Empty,
                RegisteredAt = device.RegisteredAt ?? DateTime.UtcNow,
                LastSeenAt = device.LastSeenAt
            };
        }

        // Device (Local → API)
        public static Device ToApi(this DeviceLocal device)
        {
            return new Device
            {
                Id = device.Id,
                Name = device.Name,
                IpAddress = device.IpAddress,
                Status = device.Status,
                SerialNumber = device.SerialNumber,
                RegisteredAt = device.RegisteredAt,
                LastSeenAt = device.LastSeenAt
            };
        }

        // AccessLog (API → Local)
        public static AccessLogLocal ToLocal(this AccessLog log)
        {
            return new AccessLogLocal
            {
                Id = log.Id,
                UserId = log.UserId,
                DeviceId = log.DeviceId,
                Event = log.Event,
                PortalId = log.PortalId,
                Info = log.Info,
                Time = log.Time ?? DateTime.UtcNow,
                CreatedAt = log.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = log.UpdatedAt
            };
        }

        // AccessLog (Local → API)
        public static AccessLog ToApi(this AccessLogLocal log)
        {
            return new AccessLog
            {
                Id = log.Id,
                UserId = log.UserId,
                DeviceId = log.DeviceId,
                Event = log.Event,
                PortalId = log.PortalId,
                Info = log.Info,
                Time = log.Time,
                CreatedAt = log.CreatedAt,
                UpdatedAt = log.UpdatedAt
            };
        }

        // Adicione métodos de mapping para outros modelos conforme necessidade:
        // - Group <-> GroupLocal
        // - Card <-> CardLocal
        // - BiometricTemplate <-> BiometricTemplateLocal
        // - ChangeLog <-> ChangeLogLocal
        // - ConfigGroup <-> ConfigLocal
        // - Photo <-> PhotoLocal
        // - Logo <-> LogoLocal
        // - MonitorEvent <-> MonitorEventLocal
        // - PushCommand <-> PushCommandLocal
        // - AccessRule <-> AccessRuleLocal
        // - Sync <-> SyncLocal

        // Exemplo para Group:
        public static GroupLocal ToLocal(this Group group)
        {
            return new GroupLocal
            {
                Id = group.Id,
                Name = group.Name,
                Status = group.Status,
                CreatedAt = group.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = group.UpdatedAt
            };
        }

        public static Group ToApi(this GroupLocal group)
        {
            return new Group
            {
                Id = group.Id,
                Name = group.Name,
                Status = group.Status,
                CreatedAt = group.CreatedAt,
                UpdatedAt = group.UpdatedAt
            };
        }
    }
}
