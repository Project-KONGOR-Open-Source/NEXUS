namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.ServerManagement;

/// <summary>
///     Tests for the single-holder hosting enforcement applied to every host account at the server-requester authentication endpoints, exercised here through the OPERATOR account.
///     These tests run sequentially because they share the fixed OPERATOR account name and its hosting lease key.
/// </summary>
[NotInParallel]
public sealed class OperatorHostingAuthenticationTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private const string OperatorAccountName = "OPERATOR";

    private const string OperatorPassword = OpenPasswords.OperatorPassword;

    [Before(HookType.Test)]
    public async Task Before_Each_Test()
    {
        await webApplicationFactory.WithSQLServerContainer().WithRedisContainer().InitialiseAsync();

        await EnsureOperatorAccountExists();

        // Start Each Test From A Clean Slate With No Hosting Lease Held
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        IDatabase distributedCacheStore = scope.ServiceProvider.GetRequiredService<IDatabase>();

        await distributedCacheStore.ReleaseHostLease(OperatorAccountName);
    }

    [Test]
    public async Task Manager_Authentication_Is_Rejected_When_The_Hosting_Lease_Is_Already_Held()
    {
        // Simulate Another Host Already Holding The Lease
        using (IServiceScope scope = webApplicationFactory.Services.CreateScope())
        {
            IDatabase distributedCacheStore = scope.ServiceProvider.GetRequiredService<IDatabase>();

            await Assert.That(await distributedCacheStore.TryClaimHostLease(OperatorAccountName)).IsTrue();
        }

        HttpResponseMessage response = await AuthenticateManager(OperatorAccountName);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Server_Authentication_Is_Rejected_When_No_Manager_Holds_The_Hosting_Lease()
    {
        // The Before Hook Has Released The Lease, So No Manager Holds It
        HttpResponseMessage response = await AuthenticateServer($"{OperatorAccountName}:1");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Server_Authentication_Is_Allowed_When_A_Manager_Holds_The_Hosting_Lease()
    {
        // Simulate A Manager Already Holding The Lease
        using (IServiceScope scope = webApplicationFactory.Services.CreateScope())
        {
            IDatabase distributedCacheStore = scope.ServiceProvider.GetRequiredService<IDatabase>();

            await distributedCacheStore.TryClaimHostLease(OperatorAccountName);
        }

        HttpResponseMessage response = await AuthenticateServer($"{OperatorAccountName}:1");

        await Assert.That(response.StatusCode).IsNotEqualTo(HttpStatusCode.Unauthorized);
    }

    private async Task<HttpResponseMessage> AuthenticateManager(string login)
    {
        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "S2 Games/Heroes Of Newerth/4.10.1.0/wac/x86_64");

        Dictionary<string, string> formData = new ()
        {
            { "login", login },
            { "pass", ComputeServerPasswordHash(OperatorPassword) }
        };

        return await httpClient.PostAsync("server_requester.php?f=replay_auth", new FormUrlEncodedContent(formData));
    }

    private async Task<HttpResponseMessage> AuthenticateServer(string login)
    {
        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "S2 Games/Heroes Of Newerth/4.10.1.0/wac/x86_64");

        Dictionary<string, string> formData = new ()
        {
            { "login", login },
            { "pass", ComputeServerPasswordHash(OperatorPassword) },
            { "port", "11235" },
            { "name", "TEST-SERVER" },
            { "desc", "Test Server" },
            { "location", "EU" },
            { "ip", "127.0.0.1" }
        };

        return await httpClient.PostAsync("server_requester.php?f=new_session", new FormUrlEncodedContent(formData));
    }

    /// <summary>
    ///     Computes the password value a match server or manager sends, which is the lower-case hexadecimal MD5 of the password.
    ///     The master server wraps this with the account salt to compare against the stored hash.
    /// </summary>
    private static string ComputeServerPasswordHash(string password)
        => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(password))).ToLower();

    private async Task EnsureOperatorAccountExists()
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext merrickContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        if (await merrickContext.Accounts.AnyAsync(account => account.Name.Equals(OperatorAccountName)))
            return;

        Role role = await merrickContext.Roles.SingleAsync(candidate => candidate.Name.Equals(UserRoles.User));

        string salt = SRPPasswordHasher.GenerateSRPPasswordSalt();

        User user = new ()
        {
            EmailAddress = "operator-test@kongor.test",
            Role = role,
            SRPPasswordSalt = salt,
            SRPPasswordHash = SRPPasswordHasher.ComputeSRPPasswordHash(OperatorPassword, salt)
        };

        user.PBKDF2PasswordHash = new PasswordHasher<User>().HashPassword(user, OperatorPassword);

        Account account = new ()
        {
            Name = OperatorAccountName,
            User = user,
            Type = AccountType.ServerHost,
            IsMain = true
        };

        user.Accounts.Add(account);

        await merrickContext.Users.AddAsync(user);

        await merrickContext.SaveChangesAsync();
    }
}
