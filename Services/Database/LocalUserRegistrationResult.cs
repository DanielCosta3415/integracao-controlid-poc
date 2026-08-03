using Integracao.ControlID.PoC.Models.Database;

namespace Integracao.ControlID.PoC.Services.Database;

public enum LocalUserRegistrationStatus
{
    Created,
    DuplicateUsername,
    DuplicateEmail,
    DuplicateIdentity,
    RegistrationClosed
}

public sealed record LocalUserRegistrationResult(
    LocalUserRegistrationStatus Status,
    UserLocal? User,
    bool IsBootstrapAdministrator)
{
    public static LocalUserRegistrationResult Created(UserLocal user, bool isBootstrapAdministrator) =>
        new(LocalUserRegistrationStatus.Created, user, isBootstrapAdministrator);

    public static LocalUserRegistrationResult DuplicateUsername() =>
        new(LocalUserRegistrationStatus.DuplicateUsername, null, false);

    public static LocalUserRegistrationResult DuplicateEmail() =>
        new(LocalUserRegistrationStatus.DuplicateEmail, null, false);

    public static LocalUserRegistrationResult DuplicateIdentity() =>
        new(LocalUserRegistrationStatus.DuplicateIdentity, null, false);

    public static LocalUserRegistrationResult RegistrationClosed() =>
        new(LocalUserRegistrationStatus.RegistrationClosed, null, false);
}
