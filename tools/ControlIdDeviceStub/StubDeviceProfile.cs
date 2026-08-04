internal sealed record StubDeviceProfile(string Name, string ProductName, string Firmware, string Serial, string DeviceId)
{
    private static readonly IReadOnlyDictionary<string, StubDeviceProfile> Profiles =
        new Dictionary<string, StubDeviceProfile>(StringComparer.OrdinalIgnoreCase)
        {
            ["idface"] = new("idface", "iDFace Stub", "6.2.0-stub", "1900001", "stub-idface"),
            ["idflex"] = new("idflex", "iDFlex Stub", "6.1.0-stub", "2900001", "stub-idflex"),
            ["idbox"] = new("idbox", "iDBox Stub", "5.9.0-stub", "3900001", "stub-idbox"),
            ["legacy"] = new("legacy", "Control iD Legacy Stub", "4.0.0-stub", "4900001", "stub-legacy")
        };

    public static StubDeviceProfile Default => Profiles["idface"];
    public static IReadOnlyCollection<string> Names => Profiles.Keys.Order(StringComparer.OrdinalIgnoreCase).ToArray();

    public static StubDeviceProfile Resolve(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Default;

        if (Profiles.TryGetValue(name.Trim(), out var profile))
            return profile;

        throw new ArgumentOutOfRangeException(nameof(name), $"Perfil desconhecido: {name}.");
    }

    public object CreateSystemInformation(string idCloudCode)
    {
        return new
        {
            serial = Serial,
            version = Firmware,
            product_name = ProductName,
            iDCloud_code = idCloudCode,
            device_id = DeviceId,
            online = true,
            network = new
            {
                ip = "127.0.0.1",
                gateway = "127.0.0.1",
                netmask = "255.255.255.0",
                mac = "02:00:00:00:00:01"
            }
        };
    }
}
