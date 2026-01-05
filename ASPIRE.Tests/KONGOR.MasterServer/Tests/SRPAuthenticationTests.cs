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

        SRPAuthenticationService srpAuthenticationService = new(webApplicationFactory);

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
    [Arguments("salt1@kongor.com", "SaltPlayer1", "SecurePassword123!")]
    [Arguments("salt2@kongor.net", "SaltPlayer2", "MyP@ssw0rd!")]
    public async Task CreateAccountWithSRPCredentials_GeneratesUniqueSaltsForDifferentAccounts(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new(webApplicationFactory);

        (Account account1, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);
        (Account account2, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials($"other_{emailAddress}", $"Other{accountName}", password);

        await Assert.That<string?>(account1.User.SRPPasswordSalt).IsNotEqualTo(account2.User.SRPPasswordSalt);
    }

    [Test]
    [Arguments("hash1@kongor.com", "HashPlayer1", "SecurePassword123!")]
    [Arguments("hash2@kongor.net", "HashPlayer2", "MyP@ssw0rd!")]
    public async Task CreateAccountWithSRPCredentials_HashesAreDeterministicForSamePasswordAndSalt(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new(webApplicationFactory);

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

        SRPAuthenticationService srpAuthenticationService = new(webApplicationFactory);

        _ = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

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
    [Arguments("pbkdf1@kongor.com", "PBKDF2Player1", "SecurePassword123!")]
    [Arguments("pbkdf2@kongor.net", "PBKDF2Player2", "MyP@ssw0rd!")]
    public async Task CreateAccountWithSRPCredentials_GeneratesBothSRPAndPBKDF2Hashes(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new(webApplicationFactory);

        (Account account, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);

        using (Assert.Multiple())
        {
            await Assert.That(account.User.SRPPasswordHash).IsNotEmpty();
            await Assert.That(account.User.PBKDF2PasswordHash).IsNotEmpty();
            await Assert.That<string?>(account.User.SRPPasswordHash).IsNotEqualTo(account.User.PBKDF2PasswordHash);
        }

        PasswordHasher<User> passwordHasher = new();

        PasswordVerificationResult verificationResult = passwordHasher.VerifyHashedPassword(account.User, account.User.PBKDF2PasswordHash, password);

        await Assert.That(verificationResult).IsNotEqualTo(PasswordVerificationResult.Failed);
    }

    [Test]
    [Arguments("srpauth1@kongor.com", "SRPAuthPlayer1", "SecurePassword123!")]
    [Arguments("srpauth2@kongor.net", "SRPAuthPlayer2", "MyP@ssw0rd!")]
    public async Task AuthenticateWithSRP_WithValidCredentials_ReturnsSuccessfulAuthentication(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new(webApplicationFactory);

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
    [Arguments("srpflow1@kongor.com", "SRPFlowPlayer1", "SecurePassword123!")]
    [Arguments("srpflow2@kongor.net", "SRPFlowPlayer2", "MyP@ssw0rd!")]
    public async Task AuthenticateWithSRP_CompletesFullAuthenticationFlow_ReturnsAccountData(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new(webApplicationFactory);

        SRPAuthenticationData result = await srpAuthenticationService.AuthenticateWithSRP(emailAddress, accountName, password);

        await Assert.That(result.Success).IsTrue();

        using (Assert.Multiple())
        {
            await Assert.That(result.Name).IsEqualTo(accountName);
            await Assert.That(result.Email).IsEqualTo(emailAddress);
            await Assert.That(result.Cookie).IsNotEmpty();
            await Assert.That(result.ServerProof).IsNotEmpty();
        }

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        Account? databaseAccount = await databaseContext.Accounts
            .Include(accountRecord => accountRecord.User)
            .SingleOrDefaultAsync(accountRecord => accountRecord.Name.Equals(accountName));

        await Assert.That(databaseAccount).IsNotNull();

        using (Assert.Multiple())
        {
            await Assert.That(databaseAccount.TimestampLastActive).IsGreaterThan(DateTimeOffset.MinValue);
        }
    }

    [Test]
    [Arguments("preauth1@kongor.com", "PreAuthPlayer1", "SecurePassword123!")]
    [Arguments("preauth2@kongor.net", "PreAuthPlayer2", "MyP@ssw0rd!")]
    public async Task PerformPreAuthentication_WithValidAccount_ReturnsSessionDataAndServerEphemeral(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new(webApplicationFactory);

        (Account account, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);

        SrpParameters parameters = SrpParameters.Create<System.Security.Cryptography.SHA256>
            (SRPAuthenticationSessionDataStageOne.SafePrimeNumber, SRPAuthenticationSessionDataStageOne.MultiplicativeGroupGenerator);

        SrpClient client = new(parameters);
        SrpEphemeral clientEphemeral = client.GenerateEphemeral();

        (bool success, string? sessionSalt, string? passwordSalt, string? serverPublicEphemeral, string? _)
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
    public async Task PerformPreAuthentication_WithNonExistentAccount_ReturnsNotFound(string accountName)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new(webApplicationFactory);

        SrpParameters parameters = SrpParameters.Create<System.Security.Cryptography.SHA256>
            (SRPAuthenticationSessionDataStageOne.SafePrimeNumber, SRPAuthenticationSessionDataStageOne.MultiplicativeGroupGenerator);

        SrpClient client = new(parameters);
        SrpEphemeral clientEphemeral = client.GenerateEphemeral();

        (bool success, string? _, string? _, string? _, string? errorMessage)
            = await srpAuthenticationService.PerformPreAuthentication(accountName, clientEphemeral.Public);

        using (Assert.Multiple())
        {
            await Assert.That(success).IsFalse();
            await Assert.That(errorMessage).IsNotEmpty();
        }
    }

    [Test]
    [Arguments("crypto1@kongor.com", "CryptoPlayer1", "SecurePassword123!")]
    [Arguments("crypto2@kongor.net", "CryptoPlayer2", "MyP@ssw0rd!")]
    public async Task AuthenticateWithSRP_CryptographicOperations_ProducesValidServerProof(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new(webApplicationFactory);

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
    [Arguments("unique1@kongor.com", "UniquePlayer1", "SecurePassword123!")]
    [Arguments("unique2@kongor.net", "UniquePlayer2", "MyP@ssw0rd!")]
    public async Task AuthenticateWithSRP_MultipleAuthentications_GeneratesUniqueCookies(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new(webApplicationFactory);

        SRPAuthenticationData result1 = await srpAuthenticationService.AuthenticateWithSRP(emailAddress, accountName, password);
        SRPAuthenticationData result2 = await srpAuthenticationService.AuthenticateWithSRP($"other_{emailAddress}", $"Other{accountName}", password);

        using (Assert.Multiple())
        {
            await Assert.That(result1.Success).IsTrue();
            await Assert.That(result2.Success).IsTrue();
            await Assert.That<string?>(result1.Cookie).IsNotEqualTo(result2.Cookie);
        }
    }

    [Test]
    [Arguments("disabled@kongor.com", "DisabledPlayer", "SecurePassword123!")]
    public async Task AuthenticateWithSRP_WithDisabledAccount_ReturnsUnauthorised(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new(webApplicationFactory);

        (Account account, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        account.Type = AccountType.Disabled;

        await databaseContext.SaveChangesAsync();

        SRPAuthenticationData result = await srpAuthenticationService.PerformFullAuthentication(account, password);

        using (Assert.Multiple())
        {
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.ErrorMessage).IsNotEmpty();
        }
    }

    [Test]
    [Arguments("wrongpass1@kongor.com", "WrongPassPlayer1", "CorrectPassword123!", "WrongPassword456!")]
    [Arguments("wrongpass2@kongor.net", "WrongPassPlayer2", "MyP@ssw0rd!", "NotMyP@ssw0rd!")]
    public async Task AuthenticateWithSRP_WithIncorrectPassword_ReturnsAuthenticationFailure(string emailAddress, string accountName, string correctPassword, string wrongPassword)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new(webApplicationFactory);

        (Account account, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, correctPassword);

        SRPAuthenticationData result = await srpAuthenticationService.PerformFullAuthentication(account, wrongPassword);

        using (Assert.Multiple())
        {
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.ErrorMessage).IsNotEmpty();
        }
    }

    [Test]
    [Arguments("sameaccount@kongor.com", "SameAccountPlayer", "SecurePassword123!")]
    public async Task AuthenticateWithSRP_SameAccountMultipleLogins_GeneratesUniqueCookies(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new(webApplicationFactory);

        (Account account, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);

        SRPAuthenticationData result1 = await srpAuthenticationService.PerformFullAuthentication(account, password);
        SRPAuthenticationData result2 = await srpAuthenticationService.PerformFullAuthentication(account, password);

        using (Assert.Multiple())
        {
            await Assert.That(result1.Success).IsTrue();
            await Assert.That(result2.Success).IsTrue();
            await Assert.That<string?>(result1.Cookie).IsNotEqualTo(result2.Cookie);
        }
    }

    [Test]
    [Arguments("casepass@kongor.com", "CasePassPlayer", "SecurePassword123!")]
    public async Task AuthenticateWithSRP_WithDifferentPasswordCase_ReturnsAuthenticationFailure(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new(webApplicationFactory);

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
    [Arguments("tamperedproof@kongor.com", "TamperedProofPlayer", "SecurePassword123!")]
    public async Task AuthenticateWithSRP_WithTamperedClientProof_ReturnsAuthenticationFailure(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new(webApplicationFactory);

        _ = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);

        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "S2 Games/Heroes Of Newerth/4.10.1.0/wac/x86_64");

        SrpParameters parameters = SrpParameters.Create<System.Security.Cryptography.SHA256>
            (SRPAuthenticationSessionDataStageOne.SafePrimeNumber, SRPAuthenticationSessionDataStageOne.MultiplicativeGroupGenerator);

        parameters.Generator = parameters.Pad(parameters.Generator);

        SrpClient client = new(parameters);
        SrpEphemeral clientEphemeral = client.GenerateEphemeral();

        (bool preAuthSuccess, string? sessionSalt, string? passwordSalt, string? serverPublicEphemeral, string? _)
            = await srpAuthenticationService.PerformPreAuthentication(accountName, clientEphemeral.Public);

        await Assert.That(preAuthSuccess).IsTrue();
        await Assert.That(sessionSalt).IsNotNull();
        await Assert.That(passwordSalt).IsNotNull();
        await Assert.That(serverPublicEphemeral).IsNotNull();

        string passwordHash = SRPRegistrationHandlers.ComputeSRPPasswordHash(password, passwordSalt);
        string privateClientKey = client.DerivePrivateKey(sessionSalt, accountName, passwordHash);

        SrpSession clientSession = client.DeriveSession(clientEphemeral.Secret, serverPublicEphemeral, sessionSalt, accountName, privateClientKey);

        string tamperedProof = clientSession.Proof.Length > 10
            ? clientSession.Proof[..^1] + "X"
            : "TamperedProof123";

        Dictionary<string, string> authFormData = new()
        {
            { "login", accountName },
            { "proof", tamperedProof },
            { "OSType", "windows" },
            { "MajorVersion", "4" },
            { "MinorVersion", "10" },
            { "MicroVersion", "1" },
            { "SysInfo", "testhash|testhash|testhash|testhash|testhash" }
        };

        FormUrlEncodedContent authContent = new(authFormData);
        HttpResponseMessage authResponse = await httpClient.PostAsync("client_requester.php?f=srpAuth", authContent);

        await Assert.That(authResponse.IsSuccessStatusCode).IsFalse();
    }

    [Test]
    [Arguments("serververify@kongor.com", "ServerVerifyPlayer", "SecurePassword123!")]
    public async Task AuthenticateWithSRP_ServerProof_IsVerifiableByClient(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new(webApplicationFactory);

        _ = await srpAuthenticationService.CreateAccountWithSRPCredentials(emailAddress, accountName, password);

        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "S2 Games/Heroes Of Newerth/4.10.1.0/wac/x86_64");

        SrpParameters parameters = SrpParameters.Create<System.Security.Cryptography.SHA256>
            (SRPAuthenticationSessionDataStageOne.SafePrimeNumber, SRPAuthenticationSessionDataStageOne.MultiplicativeGroupGenerator);

        parameters.Generator = parameters.Pad(parameters.Generator);

        SrpClient client = new(parameters);
        SrpEphemeral clientEphemeral = client.GenerateEphemeral();

        (bool preAuthSuccess, string? sessionSalt, string? passwordSalt, string? serverPublicEphemeral, string? _)
            = await srpAuthenticationService.PerformPreAuthentication(accountName, clientEphemeral.Public);

        await Assert.That(preAuthSuccess).IsTrue();
        await Assert.That(sessionSalt).IsNotNull();
        await Assert.That(passwordSalt).IsNotNull();
        await Assert.That(serverPublicEphemeral).IsNotNull();

        string passwordHash = SRPRegistrationHandlers.ComputeSRPPasswordHash(password, passwordSalt);
        string privateClientKey = client.DerivePrivateKey(sessionSalt, accountName, passwordHash);

        SrpSession clientSession = client.DeriveSession(clientEphemeral.Secret, serverPublicEphemeral, sessionSalt, accountName, privateClientKey);

        Dictionary<string, string> authFormData = new()
        {
            { "login", accountName },
            { "proof", clientSession.Proof },
            { "OSType", "windows" },
            { "MajorVersion", "4" },
            { "MinorVersion", "10" },
            { "MicroVersion", "1" },
            { "SysInfo", "testhash|testhash|testhash|testhash|testhash" }
        };

        FormUrlEncodedContent authContent = new(authFormData);
        HttpResponseMessage authResponse = await httpClient.PostAsync("client_requester.php?f=srpAuth", authContent);

        string authResponseBody = await authResponse.Content.ReadAsStringAsync();

        await Assert.That(authResponse.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object>? authData = PhpSerialization.Deserialize(authResponseBody) as IDictionary<object, object>;

        string serverProof = authData?["proof"] as string ?? throw new NullReferenceException("Server Proof Is NULL");

        await Assert.That(() => client.VerifySession(clientEphemeral.Public, clientSession, serverProof)).ThrowsNothing();
    }
}
