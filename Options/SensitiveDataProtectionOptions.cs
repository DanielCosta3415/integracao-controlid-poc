namespace Integracao.ControlID.PoC.Options;

public sealed class SensitiveDataProtectionOptions
{
    public bool RequireProtectedSensitiveColumns { get; set; }
    public bool ProtectLegacyDataOnStartup { get; set; }
    public bool RequireEncryptedVolume { get; set; }
    public bool EncryptedVolumeAttested { get; set; }
}
