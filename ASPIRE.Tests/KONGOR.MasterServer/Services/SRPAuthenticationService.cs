using System.Security.Cryptography;

namespace ASPIRE.Tests.KONGOR.MasterServer.Services;

/// <summary>
///     Helper Class For Building And Executing SRP Authentication Flows In Tests
/// </summary>
public sealed class SRPAuthenticationService(WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory)
{
    /// <summary>
    ///     Creates An Account With SRP Credentials For Testing
    /// </summary>
    public async Task<(Account Account, string Password)> CreateAccountWithSRPCredentials(string emailAddress,
        string accountName, string password)
    {
        MerrickContext merrickContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        Role role = await merrickContext.Roles.SingleAsync(role => role.Name.Equals(UserRoles.User));

        string salt = SRPRegistrationHandlers.GenerateSRPPasswordSalt();
        string srpPasswordHash = SRPRegistrationHandlers.ComputeSRPPasswordHash(password, salt);

        User user = new()
        {
            EmailAddress = emailAddress, Role = role, SRPPasswordSalt = salt, SRPPasswordHash = srpPasswordHash
        };

        user.PBKDF2PasswordHash = new PasswordHasher<User>().HashPassword(user, password);

        await merrickContext.Users.AddAsync(user);

        Account account = new() { Name = accountName, User = user, IsMain = true };

        user.Accounts.Add(account);

        await merrickContext.SaveChangesAsync();

        return (account, password);
    }

    /// <summary>
    ///     Creates An Account And Performs Complete SRP Authentication Flow
    /// </summary>
    public async Task<SRPAuthenticationData> AuthenticateWithSRP(string emailAddress, string accountName,
        string password)
    {
        (Account account, string _) = await CreateAccountWithSRPCredentials(emailAddress, accountName, password);

        return await PerformFullAuthentication(account, password);
    }

    /// <summary>
    ///     Performs Complete SRP Authentication Flow Without Creating An Account
    /// </summary>
    public async Task<SRPAuthenticationData> PerformFullAuthentication(Account account, string password)
    {
        MerrickContext merrickContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        Account? refreshedAccount = await merrickContext.Accounts
            .Include(accountRecord => accountRecord.User)
            .SingleOrDefaultAsync(accountRecord => accountRecord.ID == account.ID);

        if (refreshedAccount is null)
        {
            throw new InvalidOperationException($"Account With ID {account.ID} Not Found");
        }

        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
            "S2 Games/Heroes Of Newerth/4.10.1.0/wac/x86_64");

        SrpParameters parameters = SrpParameters.Create<SHA256>
        (SRPAuthenticationSessionDataStageOne.SafePrimeNumber,
            SRPAuthenticationSessionDataStageOne.MultiplicativeGroupGenerator);

        parameters.Generator = parameters.Pad(parameters.Generator);

        SrpClient client = new(parameters);

        SrpEphemeral clientEphemeral = client.GenerateEphemeral();

        Dictionary<string, string> preAuthFormData = new()
        {
            { "login", account.Name },
            { "A", clientEphemeral.Public },
            { "SysInfo", "MAC123|system|info|data|testhash" }
        };

        FormUrlEncodedContent preAuthContent = new(preAuthFormData);

        HttpResponseMessage preAuthResponse =
            await httpClient.PostAsync("client_requester.php?f=pre_auth", preAuthContent);

        string preAuthResponseBody = await preAuthResponse.Content.ReadAsStringAsync();

        if (preAuthResponse.IsSuccessStatusCode is false)
        {
            IDictionary<object, object>? errorDictionary =
                PhpSerialization.Deserialize(preAuthResponseBody) as IDictionary<object, object>;

            string errorMessage = errorDictionary?["auth"] as string ??
                                  $"Pre-Authentication Failed With HTTP {(int) preAuthResponse.StatusCode}";

            return new SRPAuthenticationData { Success = false, ErrorMessage = errorMessage, Account = account };
        }

        IDictionary<object, object>? preAuthData =
            PhpSerialization.Deserialize(preAuthResponseBody) as IDictionary<object, object>
            ?? throw new NullReferenceException("Pre-Authentication Response Deserialised To NULL");

        string sessionSalt = preAuthData["salt"] as string ?? throw new NullReferenceException("Session Salt Is NULL");
        string passwordSalt =
            preAuthData["salt2"] as string ?? throw new NullReferenceException("Password Salt Is NULL");
        string serverPublicEphemeral = preAuthData["B"] as string ??
                                       throw new NullReferenceException("Server Public Ephemeral Is NULL");

        string passwordHash = SRPRegistrationHandlers.ComputeSRPPasswordHash(password, passwordSalt);

        string privateClientKey = client.DerivePrivateKey(sessionSalt, account.Name, passwordHash);

        SrpSession clientSession = client.DeriveSession(clientEphemeral.Secret, serverPublicEphemeral, sessionSalt,
            account.Name, privateClientKey);

        Dictionary<string, string> authFormData = new()
        {
            { "login", account.Name },
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

        if (authResponse.IsSuccessStatusCode is false)
        {
            IDictionary<object, object>? errorDictionary =
                PhpSerialization.Deserialize(authResponseBody) as IDictionary<object, object>;

            string errorMessage = errorDictionary?["auth"] as string ??
                                  $"Authentication Failed With HTTP {(int) authResponse.StatusCode}";

            return new SRPAuthenticationData { Success = false, ErrorMessage = errorMessage, Account = account };
        }

        IDictionary<object, object>? stageTwoData =
            PhpSerialization.Deserialize(authResponseBody) as IDictionary<object, object>
            ?? throw new NullReferenceException("Authentication Response Deserialised To NULL");

        string serverProof =
            stageTwoData["proof"] as string ?? throw new NullReferenceException("Server Proof Is NULL");
        string cookie = stageTwoData["cookie"] as string ?? throw new NullReferenceException("Cookie Is NULL");
        string name = stageTwoData["nickname"] as string ?? throw new NullReferenceException("Name Is NULL");
        string email = stageTwoData["email"] as string ?? throw new NullReferenceException("Email Is NULL");

        return new SRPAuthenticationData
        {
            Success = true,
            Account = account,
            ServerProof = serverProof,
            Cookie = cookie,
            Name = name,
            Email = email
        };
    }

    /// <summary>
    ///     Performs Pre-Authentication Stage Only
    /// </summary>
    public async
        Task<(bool Success, string? SessionSalt, string? PasswordSalt, string? ServerPublicEphemeral, string?
            ErrorMessage)>
        PerformPreAuthentication(string accountName, string clientPublicEphemeral)
    {
        HttpClient httpClient = webApplicationFactory.CreateClient();

        Dictionary<string, string> formData = new()
        {
            { "login", accountName },
            { "A", clientPublicEphemeral },
            { "SysInfo", "test|system|information|data|hash" }
        };

        FormUrlEncodedContent content = new(formData);

        HttpResponseMessage response = await httpClient.PostAsync("client_requester.php?f=pre_auth", content);

        string responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode is false)
        {
            IDictionary<object, object>? errorDictionary =
                PhpSerialization.Deserialize(responseBody) as IDictionary<object, object>;

            string errorMessage = errorDictionary?["auth"] as string ?? "Unknown Error";

            return (false, null, null, null, errorMessage);
        }

        IDictionary<object, object>? preAuthData =
            PhpSerialization.Deserialize(responseBody) as IDictionary<object, object>
            ?? throw new NullReferenceException("Pre-Authentication Response Deserialised To NULL");

        string sessionSalt = preAuthData["salt"] as string ?? throw new NullReferenceException("Session Salt Is NULL");
        string passwordSalt =
            preAuthData["salt2"] as string ?? throw new NullReferenceException("Password Salt Is NULL");
        string serverPublicEphemeral = preAuthData["B"] as string ??
                                       throw new NullReferenceException("Server Public Ephemeral Is NULL");

        return (true, sessionSalt, passwordSalt, serverPublicEphemeral, null);
    }
}