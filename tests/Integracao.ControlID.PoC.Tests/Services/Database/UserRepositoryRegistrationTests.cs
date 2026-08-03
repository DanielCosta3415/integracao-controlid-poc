using Integracao.ControlID.PoC.Data;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Services.Security;
using Integracao.ControlID.PoC.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Integracao.ControlID.PoC.Tests.Services.Database;

public sealed class UserRepositoryRegistrationTests
{
    [Fact]
    public async Task RegisterLocalUserAsync_AllowsExactlyOneAnonymousBootstrapAdministrator()
    {
        using var database = new FileSqliteTestDatabase();

        var firstTask = RegisterAsync(database.DatabasePath, CreateUser("first-admin", "first@example.invalid"), false);
        var secondTask = RegisterAsync(database.DatabasePath, CreateUser("second-user", "second@example.invalid"), false);

        var results = await Task.WhenAll(firstTask, secondTask);

        var created = Assert.Single(results, item => item.Status == LocalUserRegistrationStatus.Created);
        Assert.True(created.IsBootstrapAdministrator);
        Assert.Equal(AppSecurityRoles.Administrator, created.User!.Role);
        Assert.Single(results, item => item.Status == LocalUserRegistrationStatus.RegistrationClosed);

        await using var context = CreateContext(database.DatabasePath);
        Assert.Equal(1, await context.Users.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RegisterLocalUserAsync_RejectsCaseInsensitiveDuplicateIdentity()
    {
        using var database = new FileSqliteTestDatabase();
        var first = await RegisterAsync(database.DatabasePath, CreateUser("Local.Admin", "admin@example.invalid"), false);
        Assert.Equal(LocalUserRegistrationStatus.Created, first.Status);

        var duplicate = await RegisterAsync(
            database.DatabasePath,
            CreateUser("local.admin", "other@example.invalid"),
            true);

        Assert.Equal(LocalUserRegistrationStatus.DuplicateUsername, duplicate.Status);
    }

    [Fact]
    public async Task RegisterLocalUserAsync_CreatesOperatorWhenAdministratorAuthorizesAdditionalUser()
    {
        using var database = new FileSqliteTestDatabase();
        await RegisterAsync(database.DatabasePath, CreateUser("admin", "admin@example.invalid"), false);

        var result = await RegisterAsync(
            database.DatabasePath,
            CreateUser("operator", "operator@example.invalid"),
            true);

        Assert.Equal(LocalUserRegistrationStatus.Created, result.Status);
        Assert.False(result.IsBootstrapAdministrator);
        Assert.Equal(AppSecurityRoles.Operator, result.User!.Role);
    }

    private static async Task<LocalUserRegistrationResult> RegisterAsync(
        string databasePath,
        UserLocal user,
        bool allowAdditionalUsers)
    {
        await using var context = CreateContext(databasePath);
        var repository = new UserRepository(context, NullLogger<UserRepository>.Instance);
        return await repository.RegisterLocalUserAsync(
            user,
            allowAdditionalUsers,
            TestContext.Current.CancellationToken);
    }

    private static IntegracaoControlIDContext CreateContext(string databasePath)
    {
        var options = new DbContextOptionsBuilder<IntegracaoControlIDContext>()
            .UseSqlite($"Data Source={databasePath};Default Timeout=30")
            .Options;
        return new IntegracaoControlIDContext(options);
    }

    private static UserLocal CreateUser(string username, string email)
    {
        return new UserLocal
        {
            Name = "Usuário de teste",
            Username = username,
            Registration = username,
            Email = email,
            PasswordHash = "test-hash",
            Status = "active"
        };
    }
}
