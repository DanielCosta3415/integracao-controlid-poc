namespace Integracao.ControlID.PoC.Services.Database;

public static class SensitiveDataColumnCatalog
{
    public static readonly SensitiveDataColumn SessionString = new("Sessions", "SessionString");
    public static readonly SensitiveDataColumn BiometricTemplate = new("BiometricTemplates", "Template");
    public static readonly SensitiveDataColumn CardValue = new("Cards", "Value");
    public static readonly SensitiveDataColumn QrCodeValue = new("QRCodes", "Value");
    public static readonly SensitiveDataColumn PhotoImage = new("Photos", "Base64Image");
    public static readonly SensitiveDataColumn ConfigValue = new("Configs", "Value");
    public static readonly SensitiveDataColumn MonitorRawJson = new("MonitorEvents", "RawJson");
    public static readonly SensitiveDataColumn MonitorPayload = new("MonitorEvents", "Payload");
    public static readonly SensitiveDataColumn PushRawJson = new("PushCommands", "RawJson");
    public static readonly SensitiveDataColumn PushPayload = new("PushCommands", "Payload");
    public static readonly SensitiveDataColumn LogMessage = new("Logs", "Message");
    public static readonly SensitiveDataColumn LogStackTrace = new("Logs", "StackTrace");
    public static readonly SensitiveDataColumn LogUser = new("Logs", "User");
    public static readonly SensitiveDataColumn LogAdditionalData = new("Logs", "AdditionalData");

    public static IReadOnlyList<SensitiveDataColumn> All { get; } =
    [
        SessionString,
        BiometricTemplate,
        CardValue,
        QrCodeValue,
        PhotoImage,
        ConfigValue,
        MonitorRawJson,
        MonitorPayload,
        PushRawJson,
        PushPayload,
        LogMessage,
        LogStackTrace,
        LogUser,
        LogAdditionalData
    ];
}

public sealed record SensitiveDataColumn(string Table, string Column)
{
    public string Purpose => $"Integracao.ControlID.PoC.Database.{Table}.{Column}.v1";
}
