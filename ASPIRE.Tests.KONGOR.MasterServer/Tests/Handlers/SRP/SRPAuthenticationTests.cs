namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Handlers.SRP;

/// <summary>
///     Tests for SRP authentication in the KONGOR master server.
/// </summary>
public sealed class SRPAuthenticationTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

    [Test]
    [Arguments("srpuser1@kongor.com", "SRP1", "SecurePassword123!")]
    [Arguments("srpuser2@kongor.net", "SRP2", "MyP@ssw0rd!")]
    public async Task Create_Account_With_SRP_Credentials_With_Valid_Data_Creates_Account_With_Valid_SRP_Fields(string emailAddress, string accountName, string password)
    {
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

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account? databaseAccount = await databaseContext.Accounts
            .Include(accountRecord => accountRecord.User)
            .SingleOrDefaultAsync(accountRecord => accountRecord.Name.Equals(accountName));

        await Assert.That(databaseAccount).IsNotNull();

        using (Assert.Multiple())
        {
            await Assert.That(databaseAccount.User.EmailAddress).IsEqualTo(emailAddress);
            await Assert.That(databaseAccount.User.SRPPasswordSalt).IsEqualTo(account.User.SRPPasswordSalt);
            await Assert.That(databaseAccount.User.SRPPasswordHash).IsEqualTo(account.User.SRPPasswordHash);
        }
    }

    [Test]
    [Arguments("salt1@kongor.com", "Salt1", "SecurePassword123!")]
    [Arguments("salt2@kongor.net", "Salt2", "MyP@ssw0rd!")]
    public async Task Create_Account_With_SRP_Credentials_Generates_Unique_Salts_For_Different_Accounts(string emailAddress, string accountName, string password)
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account firstAccount, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);
        (Account secondAccount, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials($"other_{emailAddress}", $"Other{accountName}", password);

        await Assert.That(firstAccount.User.SRPPasswordSalt).IsNotEqualTo(secondAccount.User.SRPPasswordSalt);
    }

    [Test]
    [Arguments("hash1@kongor.com", "Hash1", "SecurePassword123!")]
    [Arguments("hash2@kongor.net", "Hash2", "MyP@ssw0rd!")]
    public async Task Create_Account_With_SRP_Credentials_Hashes_Are_Deterministic_For_Same_Password_And_Salt(string emailAddress, string accountName, string password)
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account account, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);

        string salt = account.User.SRPPasswordSalt;
        string expectedHash = SRPPasswordHasher.ComputeSRPPasswordHash(password, salt);

        await Assert.That(account.User.SRPPasswordHash).IsEqualTo(expectedHash);
    }

    [Test]
    [Arguments("role1@kongor.com", "Role1", "SecurePassword123!")]
    [Arguments("role2@kongor.net", "Role2", "MyP@ssw0rd!")]
    public async Task Create_Account_With_SRP_Credentials_Assigns_User_Role_Correctly(string emailAddress, string accountName, string password)
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account account, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account? databaseAccount = await databaseContext.Accounts
            .Include(accountRecord => accountRecord.User)
            .ThenInclude(user => user.Role)
            .SingleOrDefaultAsync(accountRecord => accountRecord.Name.Equals(accountName));

        await Assert.That(databaseAccount).IsNotNull();

        using (Assert.Multiple())
        {
            await Assert.That(databaseAccount.User.Role).IsNotNull();
            await Assert.That(databaseAccount.User.Role.Name).IsEqualTo(UserRoles.User);
        }
    }

    [Test]
    [Arguments("pbkdf1@kongor.com", "PBKDF2_1", "SecurePassword123!")]
    [Arguments("pbkdf2@kongor.net", "PBKDF2_2", "MyP@ssw0rd!")]
    public async Task Create_Account_With_SRP_Credentials_Generates_Both_SRP_And_PBKDF2_Hashes(string emailAddress, string accountName, string password)
    {
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

    [Test]
    [Arguments("srpauth1@kongor.com", "SRPAuth1", "SecurePassword123!")]
    [Arguments("srpauth2@kongor.net", "SRPAuth2", "MyP@ssw0rd!")]
    public async Task Authenticate_With_SRP_With_Valid_Credentials_Returns_Successful_Authentication(string emailAddress, string accountName, string password)
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        SRPAuthenticationData result = await srpAuthenticationService.AuthenticateWithSRP(emailAddress, accountName, password);

        await Assert.That(result.Success).IsTrue();

        using (Assert.Multiple())
        {
            await Assert.That(result.ServerProof).IsNotNull();
            await Assert.That(result.ServerProof).IsNotEmpty();
            await Assert.That(result.Name).IsEqualTo(accountName);
            await Assert.That(result.Email).IsEqualTo(emailAddress);
            await Assert.That(result.Cookie).IsNotEmpty();
        }
    }

    [Test]
    [Arguments("srpflow1@kongor.com", "Flow1", "SecurePassword123!")]
    [Arguments("srpflow2@kongor.net", "Flow2", "MyP@ssw0rd!")]
    public async Task Authenticate_With_SRP_Completes_Full_Authentication_Flow_Returns_Account_Data(string emailAddress, string accountName, string password)
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        SRPAuthenticationData result = await srpAuthenticationService.AuthenticateWithSRP(emailAddress, accountName, password);

        await Assert.That(result.Success).IsTrue();

        using (Assert.Multiple())
        {
            await Assert.That(result.Name).IsEqualTo(accountName);
            await Assert.That(result.Email).IsEqualTo(emailAddress);
            await Assert.That(result.Cookie).IsNotEmpty();
            await Assert.That(result.ServerProof).IsNotEmpty();
        }

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account? databaseAccount = await databaseContext.Accounts
            .Include(accountRecord => accountRecord.User)
            .SingleOrDefaultAsync(accountRecord => accountRecord.Name.Equals(accountName));

        await Assert.That(databaseAccount).IsNotNull();

        await Assert.That(databaseAccount.TimestampLastActive).IsGreaterThan(DateTimeOffset.MinValue);
    }

    [Test]
    [Arguments("preauth1@kongor.com", "PreAuth1", "SecurePassword123!")]
    [Arguments("preauth2@kongor.net", "PreAuth2", "MyP@ssw0rd!")]
    public async Task Perform_Pre_Authentication_With_Valid_Account_Returns_Session_Data_And_Server_Ephemeral(string emailAddress, string accountName, string password)
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account account, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);

        SrpParameters parameters = SrpParameters.Create<System.Security.Cryptography.SHA256>
            (SRPAuthenticationSessionDataStageOne.SafePrimeNumber, SRPAuthenticationSessionDataStageOne.MultiplicativeGroupGenerator);

        SrpClient client = new (parameters);
        SrpEphemeral clientEphemeral = client.GenerateEphemeral();

        (bool success, string? sessionSalt, string? passwordSalt, string? serverPublicEphemeral, string? errorMessage)
            = await srpAuthenticationService.PerformPreAuthentication(accountName, clientEphemeral.Public);

        using (Assert.Multiple())
        {
            await Assert.That(success).IsTrue();
            await Assert.That(sessionSalt).IsNotEmpty();
            await Assert.That(passwordSalt).IsEqualTo(account.User.SRPPasswordSalt);
            await Assert.That(serverPublicEphemeral).IsNotEmpty();
        }
    }

    [Test]
    [Arguments("NonExistentAccount")]
    [Arguments("InvalidUser123")]
    public async Task Perform_Pre_Authentication_With_Non_Existent_Account_Returns_Not_Found(string accountName)
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        SrpParameters parameters = SrpParameters.Create<System.Security.Cryptography.SHA256>
            (SRPAuthenticationSessionDataStageOne.SafePrimeNumber, SRPAuthenticationSessionDataStageOne.MultiplicativeGroupGenerator);

        SrpClient client = new (parameters);
        SrpEphemeral clientEphemeral = client.GenerateEphemeral();

        (bool success, string? sessionSalt, string? passwordSalt, string? serverPublicEphemeral, string? errorMessage)
            = await srpAuthenticationService.PerformPreAuthentication(accountName, clientEphemeral.Public);

        using (Assert.Multiple())
        {
            await Assert.That(success).IsFalse();
            await Assert.That(errorMessage).IsNotEmpty();
        }
    }

    [Test]
    [Arguments("crypto1@kongor.com", "Crypto1", "SecurePassword123!")]
    [Arguments("crypto2@kongor.net", "Crypto2", "MyP@ssw0rd!")]
    public async Task Authenticate_With_SRP_Cryptographic_Operations_Produces_Valid_Server_Proof(string emailAddress, string accountName, string password)
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        SRPAuthenticationData result = await srpAuthenticationService.AuthenticateWithSRP(emailAddress, accountName, password);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.ServerProof).IsNotNull();

        string serverProof = result.ServerProof ?? throw new InvalidOperationException("Server Proof Is NULL");

        using (Assert.Multiple())
        {
            await Assert.That(serverProof).IsNotEmpty();
            await Assert.That(serverProof.Length).IsGreaterThan(0);
            await Assert.That(serverProof).DoesNotContain(" ");
        }
    }

    [Test]
    [Arguments("unique1@kongor.com", "Unique1", "SecurePassword123!")]
    [Arguments("unique2@kongor.net", "Unique2", "MyP@ssw0rd!")]
    public async Task Authenticate_With_SRP_Multiple_Authentications_Generates_Unique_Cookies(string emailAddress, string accountName, string password)
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        SRPAuthenticationData firstResult = await srpAuthenticationService.AuthenticateWithSRP(emailAddress, accountName, password);
        SRPAuthenticationData secondResult = await srpAuthenticationService.AuthenticateWithSRP($"other_{emailAddress}", $"Other{accountName}", password);

        using (Assert.Multiple())
        {
            await Assert.That(firstResult.Success).IsTrue();
            await Assert.That(secondResult.Success).IsTrue();
            await Assert.That(firstResult.Cookie).IsNotEqualTo(secondResult.Cookie);
        }
    }

    [Test]
    [Arguments("disabled@kongor.com", "Disabled", "SecurePassword123!")]
    public async Task Authenticate_With_SRP_With_Disabled_Account_Returns_Unauthorised(string emailAddress, string accountName, string password)
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account account, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);

        using (IServiceScope scope = webApplicationFactory.Services.CreateScope())
        {
            MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

            Account tracked = await databaseContext.Accounts.SingleAsync(candidate => candidate.ID == account.ID);

            tracked.Type = AccountType.Disabled;

            await databaseContext.SaveChangesAsync();
        }

        SRPAuthenticationData result = await srpAuthenticationService.PerformFullAuthentication(account, password);

        using (Assert.Multiple())
        {
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.ErrorMessage).IsNotEmpty();
        }
    }

    [Test]
    [Arguments("wrongpass1@kongor.com", "WrongPass1", "CorrectPassword123!", "WrongPassword456!")]
    [Arguments("wrongpass2@kongor.net", "WrongPass2", "MyP@ssw0rd!", "NotMyP@ssw0rd!")]
    public async Task Authenticate_With_SRP_With_Incorrect_Password_Returns_Authentication_Failure(string emailAddress, string accountName, string correctPassword, string wrongPassword)
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account account, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, correctPassword);

        SRPAuthenticationData result = await srpAuthenticationService.PerformFullAuthentication(account, wrongPassword);

        using (Assert.Multiple())
        {
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.ErrorMessage).IsNotEmpty();
        }
    }

    [Test]
    [Arguments("sameaccount@kongor.com", "SameAcct", "SecurePassword123!")]
    public async Task Authenticate_With_SRP_Same_Account_Multiple_Logins_Generates_Unique_Cookies(string emailAddress, string accountName, string password)
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account account, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);

        SRPAuthenticationData firstResult = await srpAuthenticationService.PerformFullAuthentication(account, password);
        SRPAuthenticationData secondResult = await srpAuthenticationService.PerformFullAuthentication(account, password);

        using (Assert.Multiple())
        {
            await Assert.That(firstResult.Success).IsTrue();
            await Assert.That(secondResult.Success).IsTrue();
            await Assert.That(firstResult.Cookie).IsNotEqualTo(secondResult.Cookie);
        }
    }

    [Test]
    public async Task Each_Login_Retires_The_Earlier_Session_Cookie_Of_The_Account()
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account account, string password) = await srpAuthenticationService.CreateAccountWithSRPCredentials("latest.cookie@kongor.com", "LatestCookie", "SecurePassword123!");

        SRPAuthenticationData firstResult = await srpAuthenticationService.PerformFullAuthentication(account, password);
        SRPAuthenticationData secondResult = await srpAuthenticationService.PerformFullAuthentication(account, password);

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        string? latestSessionCookie = await distributedCache.GetLatestSessionCookie(account.ID);

        string firstCookie = firstResult.Cookie ?? throw new InvalidOperationException("First Session Cookie Is NULL");
        string secondCookie = secondResult.Cookie ?? throw new InvalidOperationException("Second Session Cookie Is NULL");

        using (Assert.Multiple())
        {
            await Assert.That(firstResult.Success).IsTrue();
            await Assert.That(secondResult.Success).IsTrue();
            await Assert.That(latestSessionCookie).IsEqualTo(secondResult.Cookie);
            await Assert.That(latestSessionCookie).IsNotEqualTo(firstResult.Cookie);
            await Assert.That(await distributedCache.GetAccountNameForSessionCookie(firstCookie)).IsNull();
            await Assert.That(await distributedCache.GetAccountNameForSessionCookie(secondCookie)).IsEqualTo(account.Name);
        }
    }

    [Test]
    public async Task A_Login_Retires_The_Session_Cookies_Of_The_Other_Accounts_Of_The_User()
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account mainAccount, string password) = await srpAuthenticationService.CreateAccountWithSRPCredentials("retire.others@kongor.com", "RetireMain", "SecurePassword123!");

        Account subAccount = await srpAuthenticationService.CreateSubAccount(mainAccount, "RetireSub");

        SRPAuthenticationData mainResult = await srpAuthenticationService.PerformFullAuthentication(mainAccount, password);
        SRPAuthenticationData subResult = await srpAuthenticationService.PerformFullAuthentication(subAccount, password);

        string mainCookie = mainResult.Cookie ?? throw new InvalidOperationException("Main Account Session Cookie Is NULL");
        string subCookie = subResult.Cookie ?? throw new InvalidOperationException("Sub-Account Session Cookie Is NULL");

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        using (Assert.Multiple())
        {
            await Assert.That(await distributedCache.GetAccountNameForSessionCookie(mainCookie)).IsNull();
            await Assert.That(await distributedCache.GetLatestSessionCookie(mainAccount.ID)).IsNull();
            await Assert.That(await distributedCache.GetAccountNameForSessionCookie(subCookie)).IsEqualTo(subAccount.Name);
        }
    }

    [Test]
    [Arguments(AccountType.Guest)]
    [Arguments(AccountType.Staff)]
    public async Task A_Guest_Or_Staff_Login_Leaves_The_Other_Accounts_Of_The_User_Logged_In(AccountType accountType)
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account mainAccount, string password) = await srpAuthenticationService.CreateAccountWithSRPCredentials("shared.user@kongor.com", "SharedMain", "SecurePassword123!");

        Account subAccount = await srpAuthenticationService.CreateSubAccount(mainAccount, "SharedSub");

        using (IServiceScope scope = webApplicationFactory.Services.CreateScope())
        {
            MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

            List<Account> tracked = await databaseContext.Accounts.Where(candidate => candidate.ID == mainAccount.ID || candidate.ID == subAccount.ID).ToListAsync();

            foreach (Account trackedAccount in tracked)
                trackedAccount.Type = accountType;

            await databaseContext.SaveChangesAsync();
        }

        SRPAuthenticationData mainResult = await srpAuthenticationService.PerformFullAuthentication(mainAccount, password);
        SRPAuthenticationData subResult = await srpAuthenticationService.PerformFullAuthentication(subAccount, password);

        string mainCookie = mainResult.Cookie ?? throw new InvalidOperationException("Main Account Session Cookie Is NULL");

        // Each Group Of Guest Or Staff Accounts Shares One User, So That The Login Of One Of Them Never Retires Another's
        using (Assert.Multiple())
        {
            await Assert.That(subResult.Success).IsTrue();
            await Assert.That(await webApplicationFactory.Services.GetRequiredService<IDatabase>().GetAccountNameForSessionCookie(mainCookie)).IsEqualTo(mainAccount.Name);
        }
    }

    [Test]
    public async Task A_Retired_Session_Cookie_Is_Refused_By_The_Client_Requester()
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account account, string password) = await srpAuthenticationService.CreateAccountWithSRPCredentials("retired.cookie@kongor.com", "RetiredCookie", "SecurePassword123!");

        SRPAuthenticationData firstResult = await srpAuthenticationService.PerformFullAuthentication(account, password);
        SRPAuthenticationData secondResult = await srpAuthenticationService.PerformFullAuthentication(account, password);

        string firstCookie = firstResult.Cookie ?? throw new InvalidOperationException("First Session Cookie Is NULL");
        string secondCookie = secondResult.Cookie ?? throw new InvalidOperationException("Second Session Cookie Is NULL");

        HttpClient httpClient = webApplicationFactory.CreateClient();

        HttpResponseMessage retiredResponse = await httpClient.PostAsync("client_requester.php?f=get_products", new FormUrlEncodedContent(new Dictionary<string, string> { { "cookie", firstCookie } }));
        HttpResponseMessage liveResponse = await httpClient.PostAsync("client_requester.php?f=get_products", new FormUrlEncodedContent(new Dictionary<string, string> { { "cookie", secondCookie } }));

        using (Assert.Multiple())
        {
            await Assert.That(retiredResponse.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
            await Assert.That(liveResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        }
    }

    [Test]
    [Arguments("casepass@kongor.com", "CasePass", "SecurePassword123!")]
    public async Task Authenticate_With_SRP_With_Different_Password_Case_Returns_Authentication_Failure(string emailAddress, string accountName, string password)
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account account, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);

        string differentCasePassword = password.ToLowerInvariant();

        SRPAuthenticationData result = await srpAuthenticationService.PerformFullAuthentication(account, differentCasePassword);

        using (Assert.Multiple())
        {
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.ErrorMessage).IsNotEmpty();
        }
    }

    [Test]
    [Arguments("olderversion@kongor.com", "OlderVersion", "SecurePassword123!")]
    public async Task Authenticate_With_SRP_With_Another_Version_In_The_User_Agent_Returns_Unexpected_User_Agent(string emailAddress, string accountName, string password)
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        SRPAuthenticationData result = await srpAuthenticationService.AuthenticateWithSRP(emailAddress, accountName, password, userAgent: "S2 Games/Heroes Of Newerth/4.10.1.0/wac/x86_64");

        using (Assert.Multiple())
        {
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.ErrorMessage).IsEqualTo("Unexpected User Agent");
        }
    }

    [Test]
    [Arguments("enginespelling@kongor.com", "EngineSpelling", "SecurePassword123!")]
    public async Task Authenticate_With_SRP_With_The_Engines_Own_Spelling_Is_Accepted(string emailAddress, string accountName, string password)
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        // The Engine Spells The Name With A Lower-Case "of"
        SRPAuthenticationData result = await srpAuthenticationService.AuthenticateWithSRP(emailAddress, accountName, password, userAgent: $"S2 Games/Heroes of Newerth/{Distributions.Version}/wac/x86_64");

        await Assert.That(result.Success).IsTrue();
    }

    [Test]
    [Arguments("tamperedproof@kongor.com", "TamperProof", "SecurePassword123!")]
    public async Task Authenticate_With_SRP_With_Tampered_Client_Proof_Returns_Authentication_Failure(string emailAddress, string accountName, string password)
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account account, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);

        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"S2 Games/Heroes Of Newerth/{Distributions.Version}/wac/x86_64");

        SrpParameters parameters = SrpParameters.Create<System.Security.Cryptography.SHA256>
            (SRPAuthenticationSessionDataStageOne.SafePrimeNumber, SRPAuthenticationSessionDataStageOne.MultiplicativeGroupGenerator);

        parameters.Generator = parameters.Pad(parameters.Generator);

        SrpClient client = new (parameters);
        SrpEphemeral clientEphemeral = client.GenerateEphemeral();

        (bool preAuthSuccess, string? sessionSalt, string? passwordSalt, string? serverPublicEphemeral, string? preAuthError)
            = await srpAuthenticationService.PerformPreAuthentication(accountName, clientEphemeral.Public);

        await Assert.That(preAuthSuccess).IsTrue();
        await Assert.That(sessionSalt).IsNotNull();
        await Assert.That(passwordSalt).IsNotNull();
        await Assert.That(serverPublicEphemeral).IsNotNull();

        string passwordHash = SRPPasswordHasher.ComputeSRPPasswordHash(password, passwordSalt);
        string privateClientKey = client.DerivePrivateKey(sessionSalt, accountName, passwordHash);

        SrpSession clientSession = client.DeriveSession(clientEphemeral.Secret, serverPublicEphemeral, sessionSalt, accountName, privateClientKey);

        string tamperedProof = clientSession.Proof.Length > 10
            ? clientSession.Proof[..^1] + "X"
            : "TamperedProof123";

        Dictionary<string, string> authFormData = new ()
        {
            { "login", accountName },
            { "proof", tamperedProof },
            { "OSType", "windows" },
            { "MajorVersion", "4" },
            { "MinorVersion", "10" },
            { "MicroVersion", "1" },
            { "SysInfo", "testhash|testhash|testhash|testhash|testhash" }
        };

        FormUrlEncodedContent authContent = new (authFormData);
        HttpResponseMessage authResponse = await httpClient.PostAsync("client_requester.php?f=srpAuth", authContent);

        await Assert.That(authResponse.IsSuccessStatusCode).IsFalse();
    }

    [Test]
    [Arguments("serververify@kongor.com", "ServerVerify", "SecurePassword123!")]
    public async Task Authenticate_With_SRP_Server_Proof_Is_Verifiable_By_Client(string emailAddress, string accountName, string password)
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account account, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);

        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"S2 Games/Heroes Of Newerth/{Distributions.Version}/wac/x86_64");

        SrpParameters parameters = SrpParameters.Create<System.Security.Cryptography.SHA256>
            (SRPAuthenticationSessionDataStageOne.SafePrimeNumber, SRPAuthenticationSessionDataStageOne.MultiplicativeGroupGenerator);

        parameters.Generator = parameters.Pad(parameters.Generator);

        SrpClient client = new (parameters);
        SrpEphemeral clientEphemeral = client.GenerateEphemeral();

        (bool preAuthSuccess, string? sessionSalt, string? passwordSalt, string? serverPublicEphemeral, string? preAuthError)
            = await srpAuthenticationService.PerformPreAuthentication(accountName, clientEphemeral.Public);

        await Assert.That(preAuthSuccess).IsTrue();
        await Assert.That(sessionSalt).IsNotNull();
        await Assert.That(passwordSalt).IsNotNull();
        await Assert.That(serverPublicEphemeral).IsNotNull();

        string passwordHash = SRPPasswordHasher.ComputeSRPPasswordHash(password, passwordSalt);
        string privateClientKey = client.DerivePrivateKey(sessionSalt, accountName, passwordHash);

        SrpSession clientSession = client.DeriveSession(clientEphemeral.Secret, serverPublicEphemeral, sessionSalt, accountName, privateClientKey);

        Dictionary<string, string> authFormData = new ()
        {
            { "login", accountName },
            { "proof", clientSession.Proof },
            { "OSType", "windows" },
            { "MajorVersion", "4" },
            { "MinorVersion", "10" },
            { "MicroVersion", "1" },
            { "SysInfo", "testhash|testhash|testhash|testhash|testhash" }
        };

        FormUrlEncodedContent authContent = new (authFormData);
        HttpResponseMessage authResponse = await httpClient.PostAsync("client_requester.php?f=srpAuth", authContent);

        string authResponseBody = await authResponse.Content.ReadAsStringAsync();

        await Assert.That(authResponse.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object>? authData = PhpSerialization.Deserialize(authResponseBody) as IDictionary<object, object>;

        string serverProof = authData?["proof"] as string ?? throw new NullReferenceException("Server Proof Is NULL");

        await Assert.That(() => client.VerifySession(clientEphemeral.Public, clientSession, serverProof)).ThrowsNothing();
    }

    [Test]
    public async Task Authenticate_With_SRP_Lists_The_Default_Chat_Channels_First_And_Discards_Saved_Reserved_Chat_Channels()
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account account, string password) = await srpAuthenticationService.CreateAccountWithSRPCredentials("channels@kongor.com", "Channels", "SecurePassword123!");

        using (IServiceScope scope = webApplicationFactory.Services.CreateScope())
        {
            MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

            Account databaseAccount = await databaseContext.Accounts.SingleAsync(accountRecord => accountRecord.ID == account.ID);

            databaseAccount.Clan = new Clan { Name = "Current Clan", Tag = "CRNT" };
            databaseAccount.ClanTier = ClanTier.Leader;

            // The Saved Clan Channel Belongs To A Clan Which The Account Has Since Left, And The Numbered General Channel Is Only Reachable Through Load Balancing
            databaseAccount.AutoConnectChatChannels = ["Saved Channel", "Clan Former Clan", "KONGOR 2"];

            await databaseContext.SaveChangesAsync();
        }

        SRPAuthenticationData result = await srpAuthenticationService.PerformFullAuthentication(account, password);

        using (Assert.Multiple())
        {
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.ChatChannels).IsEquivalentTo(["KONGOR", "Clan Current Clan", "Saved Channel"], TUnit.Assertions.Enums.CollectionOrdering.Matching);
        }
    }

    [Test]
    public async Task Authenticate_With_SRP_Lists_The_Hosts_Chat_Channel_For_An_Account_Whose_User_Owns_A_Host_Account()
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account account, string password) = await srpAuthenticationService.CreateAccountWithSRPCredentials("hosting.user@kongor.com", "HostingUser", "SecurePassword123!");

        using (IServiceScope scope = webApplicationFactory.Services.CreateScope())
        {
            MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

            User user = await databaseContext.Users.SingleAsync(userRecord => userRecord.Accounts.Any(accountRecord => accountRecord.ID == account.ID));

            await databaseContext.Accounts.AddAsync(new Account { Name = "HostingUserHost", User = user, IsMain = false, Type = AccountType.ServerHost });

            await databaseContext.SaveChangesAsync();
        }

        SRPAuthenticationData result = await srpAuthenticationService.PerformFullAuthentication(account, password);

        using (Assert.Multiple())
        {
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.ChatChannels).IsEquivalentTo(["KONGOR", "HOSTS"], TUnit.Assertions.Enums.CollectionOrdering.Matching);
        }
    }
}
