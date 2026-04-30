using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;

namespace Integracao.ControlID.PoC.Tests.Services.Database;

public class DeviceRepositoryTests
{
    [Fact]
    public async Task SearchDevicesAsync_FiltersByCanonicalIpColumn()
    {
        using var database = new SqliteTestDatabase();
        var repository = CreateRepository(database);
        await repository.AddDeviceAsync(CreateDevice("Entrada", "10.0.0.10"));
        await repository.AddDeviceAsync(CreateDevice("Saida", "10.0.0.20"));

        var result = await repository.SearchDevicesAsync(ip: "10.0.0.10");

        var device = Assert.Single(result);
        Assert.Equal("Entrada", device.Name);
    }

    [Fact]
    public async Task AddDeviceAsync_PersistsIpAndIpAddressAliasWithSameValue()
    {
        using var database = new SqliteTestDatabase();
        var repository = CreateRepository(database);

        await repository.AddDeviceAsync(CreateDevice("Entrada", "10.0.0.10"));

        using var command = database.Connection.CreateCommand();
        command.CommandText = "SELECT Ip, IpAddress FROM Devices WHERE Name = 'Entrada';";
        using var reader = command.ExecuteReader();

        Assert.True(reader.Read());
        Assert.Equal("10.0.0.10", reader.GetString(0));
        Assert.Equal("10.0.0.10", reader.GetString(1));
    }

    private static DeviceRepository CreateRepository(SqliteTestDatabase database)
    {
        return new DeviceRepository(database.Context, NullLogger<DeviceRepository>.Instance);
    }

    private static DeviceLocal CreateDevice(string name, string ip)
    {
        return new DeviceLocal
        {
            Name = name,
            Ip = ip,
            SerialNumber = name + "-serial",
            Firmware = "1.0.0",
            Status = "active",
            Description = "fixture"
        };
    }
}
