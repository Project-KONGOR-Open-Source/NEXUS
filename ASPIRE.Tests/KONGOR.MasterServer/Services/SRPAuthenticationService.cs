namespace ASPIRE.Tests.KONGOR.MasterServer.Services;

/// <summary>
///     Helper Class For Building And Executing SRP Authentication Flows In Tests
/// </summary>
public sealed class SRPAuthenticationService(WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory)
{
    /// <summary>
    ///     Creates An Account With SRP Credentials For Testing
    /// </summary>
    public async Task<(Account Account, string Password)> CreateAccountWithSRPCredentials(string emailAddress, string accountName, string password)
    {
        MerrickContext merrickContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        // Role Is Already Seeded By EnsureCreated() In The Service Provider
        Role role = await merrickContext.Roles.SingleAsync(role => role.Name.Equals(UserRoles.User));

        string salt = SRPRegistrationHandlers.GenerateSRPPasswordSalt();
        string srpPasswordHash = SRPRegistrationHandlers.ComputeSRPPasswordHash(password, salt);

        User user = new ()
        {
            EmailAddress = emailAddress,
            Role = role,
            SRPPasswordSalt = salt,
            SRPPasswordHash = srpPasswordHash
        };

        user.PBKDF2PasswordHash = new PasswordHasher<User>().HashPassword(user, password);

        await merrickContext.Users.AddAsync(user);

        Account account = new ()
        {
            Name = accountName,
            User = user,
            IsMain = true
        };

        user.Accounts.Add(account);

        await merrickContext.SaveChangesAsync();

        return (account, password);
    }
}
