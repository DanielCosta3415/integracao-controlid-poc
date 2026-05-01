using System.Text.Json;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Services.Privacy;
using Integracao.ControlID.PoC.Tests.TestSupport;

namespace Integracao.ControlID.PoC.Tests.Services.Privacy;

public class PrivacySubjectReportServiceTests
{
    [Fact]
    public async Task BuildReportAsync_ReturnsCountsWithoutSensitivePayloads()
    {
        using var database = new SqliteTestDatabase();
        var user = new UserLocal
        {
            Name = "Maria Silva",
            Registration = "REG-001",
            Username = "maria",
            Email = "maria@example.test",
            Phone = "5551999999999",
            PasswordHash = "hash-secret",
            Salt = "salt-secret",
            Role = "Operator",
            CreatedAt = DateTime.UtcNow
        };
        database.Context.Users.Add(user);
        await database.Context.SaveChangesAsync();

        database.Context.Sessions.Add(new SessionLocal
        {
            Username = user.Username,
            DeviceAddress = "http://192.168.10.20:8080",
            SessionString = "session-secret",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        database.Context.Photos.Add(new PhotoLocal { UserId = user.Id, Base64Image = "photo-secret-base64", FileName = "person.jpg", CreatedAt = DateTime.UtcNow });
        database.Context.BiometricTemplates.Add(new BiometricTemplateLocal { UserId = user.Id, Template = "biometric-template-secret", CreatedAt = DateTime.UtcNow });
        database.Context.Cards.Add(new CardLocal { UserId = user.Id, Value = "card-secret", CreatedAt = DateTime.UtcNow });
        database.Context.QRCodes.Add(new QRCodeLocal { UserId = user.Id, Value = "qr-secret", CreatedAt = DateTime.UtcNow });
        database.Context.AccessLogs.Add(new AccessLogLocal { UserId = user.Id, Event = 1, Info = "access-secret", Time = DateTime.UtcNow, CreatedAt = DateTime.UtcNow });
        database.Context.MonitorEvents.Add(new MonitorEventLocal
        {
            EventId = Guid.NewGuid(),
            UserId = user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
            RawJson = "{\"payload\":\"monitor-secret\"}",
            Payload = "{\"payload\":\"monitor-secret\"}",
            EventType = "callback:test",
            Status = "received",
            CreatedAt = DateTime.UtcNow,
            ReceivedAt = DateTime.UtcNow
        });
        database.Context.PushCommands.Add(new PushCommandLocal
        {
            CommandId = Guid.NewGuid(),
            UserId = user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
            RawJson = "{\"payload\":\"push-secret\"}",
            Payload = "{\"payload\":\"push-secret\"}",
            CommandType = "result",
            Status = "completed",
            CreatedAt = DateTime.UtcNow,
            ReceivedAt = DateTime.UtcNow
        });
        await database.Context.SaveChangesAsync();

        var service = new PrivacySubjectReportService(database.Context);
        var report = await service.BuildReportAsync("maria@example.test");
        var serialized = JsonSerializer.Serialize(report);

        Assert.Equal(1, report.DataCategories.Single(item => item.Area == "Usuarios locais").RecordCount);
        Assert.Equal(1, report.DataCategories.Single(item => item.Area == "Fotos faciais locais").RecordCount);
        Assert.Equal(1, report.DataCategories.Single(item => item.Area == "Templates biometricos locais").RecordCount);
        Assert.Equal(1, report.DataCategories.Single(item => item.Area == "Cartoes RFID/tags").RecordCount);
        Assert.Equal(1, report.DataCategories.Single(item => item.Area == "QR Codes").RecordCount);
        Assert.Equal(1, report.DataCategories.Single(item => item.Area == "Logs de acesso locais").RecordCount);
        Assert.Equal(1, report.DataCategories.Single(item => item.Area == "Callbacks e monitoramento").RecordCount);
        Assert.Equal(1, report.DataCategories.Single(item => item.Area == "Push e resultados").RecordCount);

        Assert.DoesNotContain("maria@example.test", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Maria Silva", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash-secret", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt-secret", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("session-secret", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("photo-secret-base64", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("biometric-template-secret", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("card-secret", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("qr-secret", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("monitor-secret", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("push-secret", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BuildReportAsync_UsesNumericIdentifierForLocalOperationalTables()
    {
        using var database = new SqliteTestDatabase();
        database.Context.AccessLogs.Add(new AccessLogLocal
        {
            UserId = 42,
            Event = 1,
            Time = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
        await database.Context.SaveChangesAsync();

        var service = new PrivacySubjectReportService(database.Context);
        var report = await service.BuildReportAsync("42");

        Assert.Equal(1, report.DataCategories.Single(item => item.Area == "Logs de acesso locais").RecordCount);
        Assert.Contains(report.MatchedUserRefs, item => item.StartsWith("ref:", StringComparison.Ordinal));
    }
}
