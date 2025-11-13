namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

/// <summary>
///     Tests For SRP Authentication Functionality In KONGOR Master Server
/// </summary>
public sealed class SRPAuthenticationTests
{
    [Test]
    [Arguments("srpuser1@kongor.com", "SRPPlayer1", "SecurePassword123!")]
    [Arguments("srpuser2@kongor.net", "SRPPlayer2", "MyP@ssw0rd!")]
    public async Task CreateAccountWithSRPCredentials_WithValidData_CreatesAccountWithValidSRPFields(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account account, string returnedPassword) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);

        await Assert.That(account).IsNotNull();

        using (Assert.Multiple())
        {
            await Assert.That(account.Name).IsEqualTo(accountName);
            await Assert.That(account.User.EmailAddress).IsEqualTo(emailAddress);
            await Assert.That(account.User.SRPPasswordSalt).IsNotEmpty();
            await Assert.That(account.User.SRPPasswordHash).IsNotEmpty();
            await Assert.That(account.User.PBKDF2PasswordHash).IsNotEmpty();
            await Assert.That(account.IsMain).IsTrue();
            await Assert.That(returnedPassword).IsEqualTo(password);
        }

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        Account? dbAccount = await databaseContext.Accounts
            .Include(acc => acc.User)
            .SingleOrDefaultAsync(acc => acc.Name.Equals(accountName));

        await Assert.That(dbAccount).IsNotNull();

        using (Assert.Multiple())
        {
            await Assert.That(dbAccount.User.EmailAddress).IsEqualTo(emailAddress);
            await Assert.That(dbAccount.User.SRPPasswordSalt).IsEqualTo(account.User.SRPPasswordSalt);
            await Assert.That(dbAccount.User.SRPPasswordHash).IsEqualTo(account.User.SRPPasswordHash);
        }
    }

    [Test]
    [Arguments("salt1@kongor.com", "SaltPlayer1", "SecurePassword123!")]
    [Arguments("salt2@kongor.net", "SaltPlayer2", "MyP@ssw0rd!")]
    public async Task CreateAccountWithSRPCredentials_GeneratesUniqueSaltsForDifferentAccounts(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account account1, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);
        (Account account2, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials($"other_{emailAddress}", $"Other{accountName}", password);

        await Assert.That(account1.User.SRPPasswordSalt).IsNotEqualTo(account2.User.SRPPasswordSalt);
    }

    [Test]
    [Arguments("hash1@kongor.com", "HashPlayer1", "SecurePassword123!")]
    [Arguments("hash2@kongor.net", "HashPlayer2", "MyP@ssw0rd!")]
    public async Task CreateAccountWithSRPCredentials_HashesAreDeterministicForSamePasswordAndSalt(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account account, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);

        string salt = account.User.SRPPasswordSalt;
        string expectedHash = SRPRegistrationHandlers.ComputeSRPPasswordHash(password, salt);

        await Assert.That(account.User.SRPPasswordHash).IsEqualTo(expectedHash);
    }

    [Test]
    [Arguments("role1@kongor.com", "RolePlayer1", "SecurePassword123!")]
    [Arguments("role2@kongor.net", "RolePlayer2", "MyP@ssw0rd!")]
    public async Task CreateAccountWithSRPCredentials_AssignsUserRoleCorrectly(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account account, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        Account? dbAccount = await databaseContext.Accounts
            .Include(acc => acc.User)
            .ThenInclude(user => user.Role)
            .SingleOrDefaultAsync(acc => acc.Name.Equals(accountName));

        await Assert.That(dbAccount).IsNotNull();

        await Assert.That(dbAccount.User.Role).IsNotNull();
        await Assert.That(dbAccount.User.Role.Name).IsEqualTo(UserRoles.User);
    }

    [Test]
    [Arguments("pbkdf1@kongor.com", "PBKDF2Player1", "SecurePassword123!")]
    [Arguments("pbkdf2@kongor.net", "PBKDF2Player2", "MyP@ssw0rd!")]
    public async Task CreateAccountWithSRPCredentials_GeneratesBothSRPAndPBKDF2Hashes(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account account, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);

        using (Assert.Multiple())
        {
            await Assert.That(account.User.SRPPasswordHash).IsNotEmpty();
            await Assert.That(account.User.PBKDF2PasswordHash).IsNotEmpty();
            await Assert.That(account.User.SRPPasswordHash).IsNotEqualTo(account.User.PBKDF2PasswordHash);
        }

        PasswordHasher<User> passwordHasher = new ();
        PasswordVerificationResult verificationResult = passwordHasher.VerifyHashedPassword(account.User, account.User.PBKDF2PasswordHash, password);

        await Assert.That(verificationResult).IsNotEqualTo(PasswordVerificationResult.Failed);
    }
}
