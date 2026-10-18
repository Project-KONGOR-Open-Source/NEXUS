namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.ServerManagement;

/// <summary>
///     Tests for the production-only block that prevents the built-in <see cref="OOTB.Accounts.OPERATOR"/> host account, which ships with a publicly-known password, from hosting match servers.
///     The block is applied at both server requester authentication endpoints (the match server manager's "replay_auth" and the match server's "new_session"), and only when the master server runs in the production environment, so that self-hosters running outside production are unaffected.
///     Also tests how the match server's "new_session" endpoint treats the server instance of the login, including the copy-on-write master (HON's "cowmaster") which Linux match server managers fork their match servers from, and which authenticates with an empty server instance.
/// </summary>
public sealed class HostingAuthenticationTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private const string DedicatedHostAccountName = "DEDICATED-HOST";

    private const string NonHostAccountName = "NON-HOST";

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

    [Test]
    public async Task Server_Authentication_With_An_Instance_Registers_A_Match_Server()
    {
        await InitialiseIn("Development");

        await AuthenticateManager(DedicatedHostAccountName);

        HttpResponseMessage response = await AuthenticateServer($"{DedicatedHostAccountName}:1");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        IDictionary<object, object> body = await DeserialisePHPResponse(response);

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        MatchServer matchServer = (await distributedCache.GetMatchServers()).Single();

        MatchServerManager matchServerManager = (await distributedCache.GetMatchServerManagersByAccountName(DedicatedHostAccountName)).Single();

        using (Assert.Multiple())
        {
            await Assert.That(matchServer.HostAccountName).IsEqualTo(DedicatedHostAccountName);
            await Assert.That(matchServer.Instance).IsEqualTo(1);
            await Assert.That(matchServer.Cookie).IsEqualTo((string) body["session"]);
            await Assert.That(matchServerManager.MatchServerIDs).Contains(matchServer.ID);
        }
    }

    [Test]
    [Arguments("abc")]
    [Arguments("1.5")]
    [Arguments(":1")]
    [Arguments("-1")]
    [Arguments("+1")]
    [Arguments(" 1")]
    public async Task Server_Authentication_With_A_Malformed_Instance_Is_Rejected(string serverInstance)
    {
        await InitialiseIn("Development");

        HttpResponseMessage response = await AuthenticateServer($"{DedicatedHostAccountName}:{serverInstance}");

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
            await Assert.That(await distributedCache.GetMatchServers()).IsEmpty();
        }
    }

    [Test]
    public async Task Server_Authentication_With_Instance_0_Is_Rejected()
    {
        await InitialiseIn("Development");

        await AuthenticateManager(DedicatedHostAccountName);

        HttpResponseMessage response = await AuthenticateServer($"{DedicatedHostAccountName}:0");

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
            await Assert.That(await distributedCache.GetMatchServers()).IsEmpty();
        }
    }

    [Test]
    [Arguments("abc")]
    [Arguments("-11235")]
    [Arguments("+11235")]
    [Arguments(" 11235")]
    [Arguments("0")]
    [Arguments("65536")]
    public async Task Server_Authentication_With_A_Malformed_Port_Is_Rejected(string port)
    {
        await InitialiseIn("Development");

        HttpResponseMessage response = await AuthenticateServer($"{DedicatedHostAccountName}:1", port: port);

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
            await Assert.That(await distributedCache.GetMatchServers()).IsEmpty();
        }
    }

    [Test]
    public async Task Copy_On_Write_Master_Authentication_Returns_A_Session_Without_Registering_A_Match_Server()
    {
        await InitialiseIn("Development");

        await AuthenticateManager(DedicatedHostAccountName);

        HttpResponseMessage response = await AuthenticateServer($"{DedicatedHostAccountName}:");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        IDictionary<object, object> body = await DeserialisePHPResponse(response);

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        MatchServerManager matchServerManager = (await distributedCache.GetMatchServerManagersByAccountName(DedicatedHostAccountName)).Single();

        using (Assert.Multiple())
        {
            await Assert.That(body).ContainsKey("session");
            await Assert.That(body).ContainsKey("server_id");
            await Assert.That(body).ContainsKey("chat_address");
            await Assert.That(body).ContainsKey("chat_port");
            await Assert.That(body).ContainsKey("leaverthreshold");

            await Assert.That((string) body["session"]).IsNotEmpty();
            await Assert.That(await distributedCache.GetMatchServers()).IsEmpty();
            await Assert.That(matchServerManager.MatchServerIDs).IsEmpty();
        }
    }

    [Test]
    public async Task Copy_On_Write_Master_Authentication_Without_A_Registered_Manager_Is_Rejected()
    {
        await InitialiseIn("Development");

        HttpResponseMessage response = await AuthenticateServer($"{DedicatedHostAccountName}:");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Copy_On_Write_Master_Authentication_With_An_Incorrect_Password_Is_Rejected()
    {
        await InitialiseIn("Development");

        HttpResponseMessage response = await AuthenticateServer($"{DedicatedHostAccountName}:", "INCORRECT-PASSWORD");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Operator_Copy_On_Write_Master_Authentication_Is_Rejected_In_Production()
    {
        await InitialiseIn("Production");

        HttpResponseMessage response = await AuthenticateServer($"{OOTB.Accounts.OPERATOR.Name}:");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Copy_On_Write_Master_Heartbeat_Is_Accepted_Without_Registering_A_Match_Server()
    {
        await InitialiseIn("Development");

        await AuthenticateManager(DedicatedHostAccountName);

        HttpResponseMessage authenticationResponse = await AuthenticateServer($"{DedicatedHostAccountName}:");

        await Assert.That(authenticationResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        string session = (string) (await DeserialisePHPResponse(authenticationResponse))["session"];

        HttpResponseMessage heartbeatResponse = await SendHeartbeat(session);

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        using (Assert.Multiple())
        {
            await Assert.That(heartbeatResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(await distributedCache.GetMatchServers()).IsEmpty();
        }
    }

    [Test]
    public async Task Non_Host_Account_Copy_On_Write_Master_Authentication_Is_Rejected()
    {
        await InitialiseIn("Development");

        HttpResponseMessage response = await AuthenticateServer($"{NonHostAccountName}:");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Copy_On_Write_Master_Session_Is_Rejected_By_Match_Server_Endpoints()
    {
        await InitialiseIn("Development");

        await AuthenticateManager(DedicatedHostAccountName);

        HttpResponseMessage authenticationResponse = await AuthenticateServer($"{DedicatedHostAccountName}:");

        await Assert.That(authenticationResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        string session = (string) (await DeserialisePHPResponse(authenticationResponse))["session"];

        HttpResponseMessage acceptKeyResponse = await SendServerRequest("accept_key", new () { { "session", session }, { "acc_key", "ACCOUNT-KEY" } });

        HttpResponseMessage matchStartResponse = await SendServerRequest("start_game", new () { { "session", session } });

        using (Assert.Multiple())
        {
            await Assert.That(acceptKeyResponse.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
            await Assert.That(matchStartResponse.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        }
    }

    [Test]
    public async Task A_Second_Copy_On_Write_Master_Authentication_Replaces_The_First_Session()
    {
        await InitialiseIn("Development");

        await AuthenticateManager(DedicatedHostAccountName);

        string firstSession = (string) (await DeserialisePHPResponse(await AuthenticateServer($"{DedicatedHostAccountName}:")))["session"];

        string secondSession = (string) (await DeserialisePHPResponse(await AuthenticateServer($"{DedicatedHostAccountName}:")))["session"];

        HttpResponseMessage firstHeartbeatResponse = await SendHeartbeat(firstSession);

        HttpResponseMessage secondHeartbeatResponse = await SendHeartbeat(secondSession);

        using (Assert.Multiple())
        {
            await Assert.That(secondSession).IsNotEqualTo(firstSession);
            await Assert.That(firstHeartbeatResponse.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
            await Assert.That(secondHeartbeatResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        }
    }

    [Test]
    public async Task Removing_The_Manager_Removes_Its_Copy_On_Write_Master_Session()
    {
        await InitialiseIn("Development");

        await AuthenticateManager(DedicatedHostAccountName);

        HttpResponseMessage authenticationResponse = await AuthenticateServer($"{DedicatedHostAccountName}:");

        await Assert.That(authenticationResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        string session = (string) (await DeserialisePHPResponse(authenticationResponse))["session"];

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        MatchServerManager matchServerManager = (await distributedCache.GetMatchServerManagersByAccountName(DedicatedHostAccountName)).Single();

        await distributedCache.RemoveMatchServerManagerByID(matchServerManager.ID);

        HttpResponseMessage heartbeatResponse = await SendHeartbeat(session);

        using (Assert.Multiple())
        {
            await Assert.That(await distributedCache.IsCopyOnWriteMasterSessionCookie(session)).IsFalse();
            await Assert.That(heartbeatResponse.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        }
    }

    [Test]
    public async Task Heartbeat_With_An_Unrecognised_Session_Is_Rejected()
    {
        await InitialiseIn("Development");

        HttpResponseMessage response = await SendHeartbeat(Guid.CreateVersion7().ToString());

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    private async Task InitialiseIn(string environment)
    {
        await webApplicationFactory.WithEnvironment(environment).WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

        await EnsureAccountExists(OOTB.Accounts.OPERATOR.Name, OOTB.Accounts.OPERATOR.EmailAddress, AccountType.ServerHost);
        await EnsureAccountExists(DedicatedHostAccountName, "dedicated-host@kongor.test", AccountType.ServerHost);
        await EnsureAccountExists(NonHostAccountName, "non-host@kongor.test", AccountType.Normal);
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

    private async Task<HttpResponseMessage> AuthenticateServer(string login, string password = HostPassword, string port = "11235")
    {
        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "S2 Games/Heroes Of Newerth/4.10.1.0/wac/x86_64");

        Dictionary<string, string> formData = new ()
        {
            { "login", login },
            { "pass", ComputeServerPasswordHash(password) },
            { "port", port },
            { "name", "TEST-SERVER" },
            { "desc", "Test Server" },
            { "location", "EU" },
            { "ip", "127.0.0.1" }
        };

        return await httpClient.PostAsync("server_requester.php?f=new_session", new FormUrlEncodedContent(formData));
    }

    /// <summary>
    ///     Sends the periodic heartbeat which a match server, or a copy-on-write master, sends while it is idle with no world loaded.
    /// </summary>
    private async Task<HttpResponseMessage> SendHeartbeat(string session)
    {
        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "S2 Games/Heroes Of Newerth/4.10.1.0/wac/x86_64");

        Dictionary<string, string> formData = new ()
        {
            { "session", session },
            { "num_conn", "0" },
            { "cgt", "00:00:00" },
            { "map", string.Empty },
            { "private", "0" },
            { "vip", "0" },
            { "c_state", "1" },
            { "prev_c_state", "-1" }
        };

        return await httpClient.PostAsync("server_requester.php?f=set_online", new FormUrlEncodedContent(formData));
    }

    private async Task<HttpResponseMessage> SendServerRequest(string function, Dictionary<string, string> formData)
    {
        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "S2 Games/Heroes Of Newerth/4.10.1.0/wac/x86_64");

        return await httpClient.PostAsync($"server_requester.php?f={function}", new FormUrlEncodedContent(formData));
    }

    private static async Task<IDictionary<object, object>> DeserialisePHPResponse(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();

        return PhpSerialization.Deserialize(body) as IDictionary<object, object>
            ?? throw new InvalidOperationException($@"Response Body Was Not A PHP Array: ""{body}""");
    }

    /// <summary>
    ///     Computes the password value a match server or match server manager sends, which is the lower-case hexadecimal MD5 of the password.
    ///     The master server wraps this with the account salt to compare against the stored hash.
    /// </summary>
    private static string ComputeServerPasswordHash(string password)
        => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(password))).ToLower();

    private async Task EnsureAccountExists(string accountName, string emailAddress, AccountType accountType)
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
            Type = accountType,
            IsMain = true
        };

        user.Accounts.Add(account);

        await merrickContext.Users.AddAsync(user);

        await merrickContext.SaveChangesAsync();
    }
}
