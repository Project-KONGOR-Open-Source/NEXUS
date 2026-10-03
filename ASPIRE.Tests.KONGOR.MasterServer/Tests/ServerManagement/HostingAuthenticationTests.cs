namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.ServerManagement;

/// <summary>
///     Tests for the production-only block that prevents the built-in <see cref="OOTB.Accounts.OPERATOR"/> host account, which ships with a publicly-known password, from hosting match servers.
///     The block is applied at both server requester authentication endpoints (the match server manager's "replay_auth" and the match server's "new_session"), and only when the master server runs in the production environment, so that self-hosters running outside production are unaffected.
/// </summary>
public sealed class HostingAuthenticationTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private const string DedicatedHostAccountName = "DEDICATED-HOST";

    private const string HostPassword = "HOST-PASSWORD";

    [Test]
    public async Task Operator_Manager_Authentication_Is_Rejected_In_Production()
    {
        await InitialiseIn("Production");

        HttpResponseMessage response = await AuthenticateManager(OOTB.Accounts.OPERATOR.Name);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Operator_Server_Authentication_Is_Rejected_In_Production()
    {
        await InitialiseIn("Production");

        HttpResponseMessage response = await AuthenticateServer($"{OOTB.Accounts.OPERATOR.Name}:1");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Operator_Authentication_Is_Allowed_Outside_Production()
    {
        await InitialiseIn("Development");

        using (Assert.Multiple())
        {
            await Assert.That((await AuthenticateManager(OOTB.Accounts.OPERATOR.Name)).StatusCode).IsNotEqualTo(HttpStatusCode.Unauthorized);
            await Assert.That((await AuthenticateServer($"{OOTB.Accounts.OPERATOR.Name}:1")).StatusCode).IsNotEqualTo(HttpStatusCode.Unauthorized);
        }
    }

    [Test]
    public async Task Dedicated_Host_Account_Authentication_Is_Allowed_In_Production()
    {
        await InitialiseIn("Production");

        using (Assert.Multiple())
        {
            await Assert.That((await AuthenticateManager(DedicatedHostAccountName)).StatusCode).IsNotEqualTo(HttpStatusCode.Unauthorized);
            await Assert.That((await AuthenticateServer($"{DedicatedHostAccountName}:1")).StatusCode).IsNotEqualTo(HttpStatusCode.Unauthorized);
        }
    }

    private async Task InitialiseIn(string environment)
    {
        await webApplicationFactory.WithEnvironment(environment).WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

        await EnsureHostAccountExists(OOTB.Accounts.OPERATOR.Name, OOTB.Accounts.OPERATOR.EmailAddress);
        await EnsureHostAccountExists(DedicatedHostAccountName, "dedicated-host@kongor.test");
    }

    private async Task<HttpResponseMessage> AuthenticateManager(string login)
    {
        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "S2 Games/Heroes Of Newerth/4.10.1.0/wac/x86_64");

        Dictionary<string, string> formData = new ()
        {
            { "login", login },
            { "pass", ComputeServerPasswordHash(HostPassword) }
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
            { "pass", ComputeServerPasswordHash(HostPassword) },
            { "port", "11235" },
            { "name", "TEST-SERVER" },
            { "desc", "Test Server" },
            { "location", "EU" },
            { "ip", "127.0.0.1" }
        };

        return await httpClient.PostAsync("server_requester.php?f=new_session", new FormUrlEncodedContent(formData));
    }

    /// <summary>
    ///     Computes the password value a match server or match server manager sends, which is the lower-case hexadecimal MD5 of the password.
    ///     The master server wraps this with the account salt to compare against the stored hash.
    /// </summary>
    private static string ComputeServerPasswordHash(string password)
        => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(password))).ToLower();

    private async Task EnsureHostAccountExists(string accountName, string emailAddress)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext merrickContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        if (await merrickContext.Accounts.AnyAsync(account => account.Name.Equals(accountName)))
            return;

        Role role = await merrickContext.Roles.SingleAsync(candidate => candidate.Name.Equals(UserRoles.User));

        string salt = SRPPasswordHasher.GenerateSRPPasswordSalt();

        User user = new ()
        {
            EmailAddress = emailAddress,
            Role = role,
            SRPPasswordSalt = salt,
            SRPPasswordHash = SRPPasswordHasher.ComputeSRPPasswordHash(HostPassword, salt)
        };

        user.PBKDF2PasswordHash = new PasswordHasher<User>().HashPassword(user, HostPassword);

        Account account = new ()
        {
            Name = accountName,
            User = user,
            Type = AccountType.ServerHost,
            IsMain = true
        };

        user.Accounts.Add(account);

        await merrickContext.Users.AddAsync(user);

        await merrickContext.SaveChangesAsync();
    }
}
