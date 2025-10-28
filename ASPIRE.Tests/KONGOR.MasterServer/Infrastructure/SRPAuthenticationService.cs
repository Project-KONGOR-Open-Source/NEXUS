namespace ASPIRE.Tests.KONGOR.MasterServer.Infrastructure;

/// <summary>
///     Helper Class For Building And Executing SRP Authentication Flows In Tests
/// </summary>
public sealed class SRPAuthenticationService(MerrickContext merrickContext, IMemoryCache cache)
{
    private readonly MerrickContext _merrickContext = merrickContext;
    private readonly IMemoryCache _cache = cache;

    /// <summary>
    ///     Creates An Account With SRP Credentials For Testing
    /// </summary>
    public async Task<(Account Account, string Password)> CreateAccountWithSRPCredentials(string emailAddress, string accountName, string password)
    {
        Role? role = await _merrickContext.Roles.SingleOrDefaultAsync(role => role.Name.Equals(MERRICK.DatabaseContext.Constants.UserRoles.User));

        if (role is null)
        {
            role = new Role { Name = MERRICK.DatabaseContext.Constants.UserRoles.User };
            await _merrickContext.Roles.AddAsync(role);
            await _merrickContext.SaveChangesAsync();
        }

        string salt = SRPRegistrationHandlers.GenerateSRPPasswordSalt();
        string srpPasswordHash = SRPRegistrationHandlers.ComputeSRPPasswordHash(password, salt);

        MERRICK.DatabaseContext.Entities.Core.User user = new ()
        {
            EmailAddress = emailAddress,
            Role = role,
            SRPPasswordSalt = salt,
            SRPPasswordHash = srpPasswordHash,
            PBKDF2PasswordHash = new PasswordHasher<MERRICK.DatabaseContext.Entities.Core.User>().HashPassword(null!, password)
        };

        await _merrickContext.Users.AddAsync(user);

        Account account = new ()
        {
            Name = accountName,
            User = user,
            IsMain = true
        };

        user.Accounts.Add(account);

        await _merrickContext.SaveChangesAsync();

        return (account, password);
    }

}
