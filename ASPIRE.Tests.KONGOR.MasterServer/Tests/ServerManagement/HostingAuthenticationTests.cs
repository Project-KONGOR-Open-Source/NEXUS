namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.ServerManagement;

/// <summary>
///     Tests for the production-only block that prevents the built-in <see cref="OOTB.Accounts.OPERATOR"/> host account, which ships with a publicly-known password, from hosting match servers.
///     The block is applied at both server requester authentication endpoints (the match server manager's "replay_auth" and the match server's "new_session"), and only when the master server runs in the production environment, so that self-hosters running outside production are unaffected.
///     Also tests how the match server's "new_session" endpoint treats the server instance of the login, including the copy-on-write master (HON's "cowmaster") which Linux match server managers fork their match servers from, and which authenticates with an empty server instance.
///     The copy-on-write master is a non-joinable match server: it is registered apart from the match servers, whether or not its manager is registered, its heartbeat is accepted, and it outlives the removal of its manager.
///     Also tests how the match server endpoints answer a session which is not registered: with the "auth_failure" reply, which makes the match server authenticate again, where acting upon it is safe, with a plain rejection where acting upon it would crash the match server, and as usual where a player reconnecting to a match in progress depends on the reply.
/// </summary>
public sealed class HostingAuthenticationTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private const string DedicatedHostAccountName = "DEDICATED-HOST";

    private const string NonHostAccountName = "NON-HOST";

    private const string HostPassword = "HOST-PASSWORD";

    private const string AuthenticationFailureResponse = @"a:1:{i:0;s:12:""auth_failure"";}";

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
    public async Task Re_Authenticating_A_Match_Server_Lists_It_Once_In_Its_Manager()
    {
        await InitialiseIn("Development");

        await AuthenticateManager(DedicatedHostAccountName);

        await AuthenticateServer($"{DedicatedHostAccountName}:1");

        HttpResponseMessage response = await AuthenticateServer($"{DedicatedHostAccountName}:1");

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        MatchServer matchServer = (await distributedCache.GetMatchServers()).Single();

        MatchServerManager matchServerManager = (await distributedCache.GetMatchServerManagersByAccountName(DedicatedHostAccountName)).Single();

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(matchServerManager.MatchServerIDs.Count(matchServerID => matchServerID == matchServer.ID)).IsEqualTo(1);
        }
    }

    [Test]
    [Arguments("abc")]
    [Arguments("1.5")]
    [Arguments(":1")]
    [Arguments("-1")]
    [Arguments("+1")]
    [Arguments(" 1")]
    [Arguments("01")]
    [Arguments("001")]
    [Arguments("12345678901")]
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
    [Arguments("011235")]
    [Arguments("")]
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
    [Arguments("2147483647", 2147483647)]
    public async Task Server_Authentication_With_The_Largest_Instance_Registers_A_Match_Server(string serverInstance, int expectedInstance)
    {
        await InitialiseIn("Development");

        await AuthenticateManager(DedicatedHostAccountName);

        HttpResponseMessage response = await AuthenticateServer($"{DedicatedHostAccountName}:{serverInstance}");

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That((await distributedCache.GetMatchServers()).Single().Instance).IsEqualTo(expectedInstance);
        }
    }

    [Test]
    public async Task Server_Authentication_With_The_Largest_Port_Registers_A_Match_Server()
    {
        await InitialiseIn("Development");

        await AuthenticateManager(DedicatedHostAccountName);

        HttpResponseMessage response = await AuthenticateServer($"{DedicatedHostAccountName}:1", port: "65535");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Copy_On_Write_Master_Authentication_With_A_Registered_Manager_Registers_It_With_Its_Manager_Apart_From_The_Match_Servers()
    {
        await InitialiseIn("Development");

        await AuthenticateManager(DedicatedHostAccountName);

        HttpResponseMessage response = await AuthenticateServer($"{DedicatedHostAccountName}:");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        IDictionary<object, object> body = await DeserialisePHPResponse(response);

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        MatchServerManager matchServerManager = (await distributedCache.GetMatchServerManagersByAccountName(DedicatedHostAccountName)).Single();

        CopyOnWriteMaster? copyOnWriteMaster = await distributedCache.GetCopyOnWriteMasterBySessionCookie((string) body["session"]);

        using (Assert.Multiple())
        {
            await Assert.That(body).ContainsKey("session");
            await Assert.That(body).ContainsKey("server_id");
            await Assert.That(body).ContainsKey("chat_address");
            await Assert.That(body).ContainsKey("chat_port");
            await Assert.That(body).ContainsKey("leaverthreshold");

            await Assert.That((string) body["session"]).IsNotEmpty();
            await Assert.That(copyOnWriteMaster).IsNotNull();
            await Assert.That(copyOnWriteMaster?.ID).IsEqualTo(Convert.ToInt32(body["server_id"]));
            await Assert.That(copyOnWriteMaster?.HostAccountName).IsEqualTo(DedicatedHostAccountName);
            await Assert.That(copyOnWriteMaster?.MatchServerManagerID).IsEqualTo(matchServerManager.ID);
            await Assert.That(await distributedCache.GetMatchServers()).IsEmpty();
            await Assert.That(matchServerManager.MatchServerIDs).IsEmpty();
        }
    }

    /// <summary>
    ///     The master server may lose a manager's registration while the manager keeps running, and a manager whose chat connection stays up never authenticates again, so its copy-on-write master is accepted without it, with a warning in case the empty server instance comes from a misconfigured standalone match server instead.
    /// </summary>
    [Test]
    public async Task Copy_On_Write_Master_Authentication_Without_A_Registered_Manager_Registers_It_Without_A_Manager()
    {
        await InitialiseIn("Development");

        HttpResponseMessage response = await AuthenticateServer($"{DedicatedHostAccountName}:");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        IDictionary<object, object> body = await DeserialisePHPResponse(response);

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        CopyOnWriteMaster? copyOnWriteMaster = await distributedCache.GetCopyOnWriteMasterBySessionCookie((string) body["session"]);

        using (Assert.Multiple())
        {
            await Assert.That(copyOnWriteMaster).IsNotNull();
            await Assert.That(copyOnWriteMaster?.HostAccountName).IsEqualTo(DedicatedHostAccountName);
            await Assert.That(copyOnWriteMaster?.MatchServerManagerID).IsNull();
            await Assert.That(await distributedCache.GetMatchServers()).IsEmpty();
        }
    }

    [Test]
    public async Task Copy_On_Write_Master_Authentication_With_An_Incorrect_Password_Is_Rejected()
    {
        await InitialiseIn("Development");

        HttpResponseMessage response = await AuthenticateServer($"{DedicatedHostAccountName}:", "INCORRECT-PASSWORD");

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
            await Assert.That(await distributedCache.GetCopyOnWriteMasters()).IsEmpty();
        }
    }

    [Test]
    public async Task Operator_Copy_On_Write_Master_Authentication_Is_Rejected_In_Production()
    {
        await InitialiseIn("Production");

        HttpResponseMessage response = await AuthenticateServer($"{OOTB.Accounts.OPERATOR.Name}:");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Copy_On_Write_Master_Heartbeat_Is_Accepted_And_Refreshes_Its_Record()
    {
        await InitialiseIn("Development");

        string session = await AuthenticateCopyOnWriteMaster();

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        DateTimeOffset registeredHeartbeat = (await distributedCache.GetCopyOnWriteMasters()).Single().TimestampLastHeartbeat;

        HttpResponseMessage heartbeatResponse = await SendHeartbeat(session, "0");

        CopyOnWriteMaster copyOnWriteMaster = (await distributedCache.GetCopyOnWriteMasters()).Single();

        using (Assert.Multiple())
        {
            await Assert.That(heartbeatResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(await heartbeatResponse.Content.ReadAsStringAsync()).IsEmpty();
            await Assert.That(copyOnWriteMaster.Cookie).IsEqualTo(session);
            await Assert.That(copyOnWriteMaster.Status).IsEqualTo(ChatProtocol.ServerStatus.SERVER_STATUS_SLEEPING);
            await Assert.That(copyOnWriteMaster.TimestampLastHeartbeat).IsGreaterThan(registeredHeartbeat);
            await Assert.That(await distributedCache.GetMatchServers()).IsEmpty();
        }
    }

    [Test]
    public async Task Copy_On_Write_Master_Is_Never_Found_By_Match_Server_Lists_Look_Ups_Or_Matchmaking_Reads()
    {
        await InitialiseIn("Development");

        string session = await AuthenticateCopyOnWriteMaster();

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        CopyOnWriteMaster copyOnWriteMaster = (await distributedCache.GetCopyOnWriteMasters()).Single();

        MatchServerManager matchServerManager = (await distributedCache.GetMatchServerManagersByAccountName(DedicatedHostAccountName)).Single();

        // The Server List And Matchmaking Read Every Match Server, And The Other Match Server Endpoints Look One Up By Session, ID, Host Account, Or Address
        using (Assert.Multiple())
        {
            await Assert.That(await distributedCache.GetMatchServers()).IsEmpty();
            await Assert.That(await distributedCache.GetMatchServersByAccountName(DedicatedHostAccountName)).IsEmpty();
            await Assert.That(await distributedCache.GetMatchServerByID(copyOnWriteMaster.ID)).IsNull();
            await Assert.That(await distributedCache.GetMatchServerBySessionCookie(session)).IsNull();
            await Assert.That(await distributedCache.GetMatchServerByIPAddressAndPort(copyOnWriteMaster.IPAddress, copyOnWriteMaster.Port)).IsNull();
            await Assert.That(matchServerManager.MatchServerIDs).IsEmpty();
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
    [Arguments("server_requester.php?f=aids2cookie")]
    [Arguments("server_requester.php?f=accept_key")]
    [Arguments("server_requester.php?f=start_game")]
    [Arguments("stats_requester.php")]
    [Arguments("master/questserver/getplayerquests")]
    [Arguments("master/questserver/getsysteminfo")]
    public async Task Copy_On_Write_Master_Session_Authorises_No_Match_Server_Endpoint(string endpoint)
    {
        await InitialiseIn("Development");

        string session = await AuthenticateCopyOnWriteMaster();

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        HttpResponseMessage response = await SendMatchServerEndpointRequest(endpoint, session, await GetAccountID(NonHostAccountName));

        string body = await response.Content.ReadAsStringAsync();

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
            await Assert.That(body).DoesNotContain("aids2cookie");
            await Assert.That(body).DoesNotContain("aids2gcookie");
            await Assert.That(body).DoesNotContain("quest_status");
            await Assert.That(body).DoesNotContain(cookie);
            await Assert.That(body).DoesNotContain("auth_failure");
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
            await Assert.That(firstHeartbeatResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(await firstHeartbeatResponse.Content.ReadAsStringAsync()).IsEqualTo(AuthenticationFailureResponse);
            await Assert.That(secondHeartbeatResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(await secondHeartbeatResponse.Content.ReadAsStringAsync()).IsEmpty();
        }
    }

    [Test]
    public async Task Removing_The_Manager_Leaves_Its_Copy_On_Write_Master()
    {
        await InitialiseIn("Development");

        string session = await AuthenticateCopyOnWriteMaster();

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        MatchServerManager matchServerManager = (await distributedCache.GetMatchServerManagersByAccountName(DedicatedHostAccountName)).Single();

        await distributedCache.RemoveMatchServerManagerByID(matchServerManager.ID);

        HttpResponseMessage heartbeatResponse = await SendHeartbeat(session);

        using (Assert.Multiple())
        {
            await Assert.That(await distributedCache.GetMatchServerManagersByAccountName(DedicatedHostAccountName)).IsEmpty();
            await Assert.That(await distributedCache.GetCopyOnWriteMasterBySessionCookie(session)).IsNotNull();
            await Assert.That(heartbeatResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(await heartbeatResponse.Content.ReadAsStringAsync()).IsEmpty();
        }
    }

    [Test]
    public async Task Heartbeat_With_An_Unrecognised_Session_Is_Answered_With_An_Authentication_Failure_While_A_Copy_On_Write_Master_Is_Registered()
    {
        await InitialiseIn("Development");

        await AuthenticateCopyOnWriteMaster();

        HttpResponseMessage response = await SendHeartbeat(Guid.CreateVersion7().ToString());

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(await response.Content.ReadAsStringAsync()).IsEqualTo(AuthenticationFailureResponse);
        }
    }

    [Test]
    [Arguments("0")]
    [Arguments("1")]
    [Arguments("3")]
    public async Task Heartbeat_With_An_Unrecognised_Session_Is_Answered_With_An_Authentication_Failure(string connectionState)
    {
        await InitialiseIn("Development");

        HttpResponseMessage response = await SendHeartbeat(Guid.CreateVersion7().ToString(), connectionState);

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(await response.Content.ReadAsStringAsync()).IsEqualTo(AuthenticationFailureResponse);
        }
    }

    /// <summary>
    ///     A match server in a lobby, with its world loaded and its match not started, ends that match and disconnects its players when it reads "auth_failure", as it does for the retail master server.
    ///     This is the intended reply, because "start_game" refuses the unregistered session, so that match could never start anyway.
    /// </summary>
    [Test]
    public async Task Heartbeat_With_An_Unrecognised_Session_From_A_Lobby_Is_Answered_With_An_Authentication_Failure_Which_Ends_The_Unstarted_Match()
    {
        await InitialiseIn("Development");

        HttpResponseMessage response = await SendHeartbeat(Guid.CreateVersion7().ToString(), "2");

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(await response.Content.ReadAsStringAsync()).IsEqualTo(AuthenticationFailureResponse);
        }
    }

    [Test]
    public async Task Heartbeat_From_A_Registered_Match_Server_Is_Accepted()
    {
        await InitialiseIn("Development");

        string session = await AuthenticateMatchServer();

        HttpResponseMessage response = await SendHeartbeat(session);

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(await response.Content.ReadAsStringAsync()).IsEmpty();
        }
    }

    [Test]
    public async Task Client_Connection_With_An_Unrecognised_Session_Returns_The_Account()
    {
        await InitialiseIn("Development");

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        HttpResponseMessage response = await ConnectClient(Guid.CreateVersion7().ToString(), cookie);

        await AssertClientConnectionReturnsTheAccount(response, NonHostAccountName);
    }

    [Test]
    public async Task Client_Connection_With_An_Empty_Session_Returns_The_Account()
    {
        await InitialiseIn("Development");

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        HttpResponseMessage response = await ConnectClient(string.Empty, cookie);

        await AssertClientConnectionReturnsTheAccount(response, NonHostAccountName);
    }

    [Test]
    public async Task Client_Connection_With_A_Copy_On_Write_Master_Session_Returns_The_Account()
    {
        await InitialiseIn("Development");

        string session = await AuthenticateCopyOnWriteMaster();

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        HttpResponseMessage response = await ConnectClient(session, cookie);

        await AssertClientConnectionReturnsTheAccount(response, NonHostAccountName);
    }

    [Test]
    public async Task Client_Connection_With_A_Registered_Match_Server_Session_Returns_The_Account()
    {
        await InitialiseIn("Development");

        string session = await AuthenticateMatchServer();

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        HttpResponseMessage response = await ConnectClient(session, cookie);

        await AssertClientConnectionReturnsTheAccount(response, NonHostAccountName);
    }

    [Test]
    public async Task Account_IDs_To_Cookie_Without_A_Session_Is_Rejected()
    {
        await InitialiseIn("Development");

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        HttpResponseMessage response = await RequestAccountCookies(null, await GetAccountID(NonHostAccountName));

        string body = await response.Content.ReadAsStringAsync();

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
            await Assert.That(body).DoesNotContain("aids2cookie");
            await Assert.That(body).DoesNotContain("aids2gcookie");
            await Assert.That(body).DoesNotContain(cookie);
            await Assert.That(body).DoesNotContain("auth_failure");
        }
    }

    [Test]
    public async Task Account_IDs_To_Cookie_With_An_Unrecognised_Session_Is_Rejected()
    {
        await InitialiseIn("Development");

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        HttpResponseMessage response = await RequestAccountCookies(Guid.CreateVersion7().ToString(), await GetAccountID(NonHostAccountName));

        string body = await response.Content.ReadAsStringAsync();

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
            await Assert.That(body).DoesNotContain("aids2cookie");
            await Assert.That(body).DoesNotContain("aids2gcookie");
            await Assert.That(body).DoesNotContain(cookie);
            await Assert.That(body).DoesNotContain("auth_failure");
        }
    }

    [Test]
    public async Task Account_IDs_To_Cookie_With_A_Copy_On_Write_Master_Session_Is_Rejected()
    {
        await InitialiseIn("Development");

        string session = await AuthenticateCopyOnWriteMaster();

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        HttpResponseMessage response = await RequestAccountCookies(session, await GetAccountID(NonHostAccountName));

        string body = await response.Content.ReadAsStringAsync();

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
            await Assert.That(body).DoesNotContain("aids2cookie");
            await Assert.That(body).DoesNotContain("aids2gcookie");
            await Assert.That(body).DoesNotContain(cookie);
            await Assert.That(body).DoesNotContain("auth_failure");
        }
    }

    /// <summary>
    ///     The master server finds live session cookies by scanning the whole distributed cache keyspace, which the per-test key prefix hides from it, so this test also stores each session cookie key without the prefix.
    ///     The test runs in isolation, so that no other test's master server purges those session cookie keys while it starts up.
    /// </summary>
    [Test]
    [NotInParallel]
    public async Task Account_IDs_To_Cookie_With_A_Registered_Match_Server_Session_Returns_The_Cookies_Of_The_Requested_Accounts()
    {
        await InitialiseIn("Development");

        string session = await AuthenticateMatchServer();

        Dictionary<int, string> cookies = [];

        foreach (string accountName in new List<string> { NonHostAccountName, DedicatedHostAccountName })
            cookies.Add(await GetAccountID(accountName), await CreateClientSessionCookie(accountName));

        IDatabase unprefixedDistributedCache = webApplicationFactory.Services.GetRequiredService<IConnectionMultiplexer>().GetDatabase();

        List<RedisKey> unprefixedKeys = [.. cookies.Values.Select(cookie => new RedisKey($@"ACCOUNT-SESSION-COOKIE:[""{cookie}""]"))];

        try
        {
            foreach (RedisKey unprefixedKey in unprefixedKeys)
                await unprefixedDistributedCache.StringSetAsync(unprefixedKey, string.Empty, TimeSpan.FromMinutes(1));

            HttpResponseMessage response = await RequestAccountCookies(session, [.. cookies.Keys]);

            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

            IDictionary<object, object> body = await DeserialisePHPResponse(response);

            IDictionary<object, object> clientCookies = (IDictionary<object, object>) body["aids2cookie"];

            IDictionary<object, object> gameCookies = (IDictionary<object, object>) body["aids2gcookie"];

            // A Real Match Server Never Receives This Reply, Because It Posts The Account IDs In The Request Body Rather Than In The Query String
            // If It Did, It Would Kick Every Listed Player, Because It Compares Each Player's "aids2gcookie" Value With The "game_cookie" Which "c_conn" Returned, And This Holds The Client Session Cookie Instead, So It Would Terminate Each Player And Disconnect Them As A Duplicate Connection
            using (Assert.Multiple())
            {
                foreach ((int accountID, string cookie) in cookies)
                {
                    await Assert.That((string) clientCookies[accountID.ToString()]).IsEqualTo(cookie);
                    await Assert.That((string) gameCookies[accountID.ToString()]).IsEqualTo(cookie);
                }
            }
        }

        finally
        {
            await unprefixedDistributedCache.KeyDeleteAsync([.. unprefixedKeys]);
        }
    }

    /// <summary>
    ///     The match server ("CGameServer::GetAccountAuth") posts the account IDs as "aids[]" in the request body, where the master server does not read them, so both lists are empty, and the match server, which only checks the players listed, kicks none of them.
    ///     The master server finds live session cookies by scanning the whole distributed cache keyspace, which the per-test key prefix hides from it, so this test also stores each session cookie key without the prefix, so that the lists are empty only because of where the account IDs are posted.
    ///     The test runs in isolation, so that no other test's master server purges those session cookie keys while it starts up.
    /// </summary>
    [Test]
    [NotInParallel]
    public async Task Account_IDs_To_Cookie_Requested_As_The_Match_Server_Requests_It_Returns_Empty_Lists_Which_Kick_No_Player()
    {
        await InitialiseIn("Development");

        string session = await AuthenticateMatchServer();

        Dictionary<int, string> cookies = [];

        foreach (string accountName in new List<string> { NonHostAccountName, DedicatedHostAccountName })
            cookies.Add(await GetAccountID(accountName), await CreateClientSessionCookie(accountName));

        IDatabase unprefixedDistributedCache = webApplicationFactory.Services.GetRequiredService<IConnectionMultiplexer>().GetDatabase();

        List<RedisKey> unprefixedKeys = [.. cookies.Values.Select(cookie => new RedisKey($@"ACCOUNT-SESSION-COOKIE:[""{cookie}""]"))];

        try
        {
            foreach (RedisKey unprefixedKey in unprefixedKeys)
                await unprefixedDistributedCache.StringSetAsync(unprefixedKey, string.Empty, TimeSpan.FromMinutes(1));

            HttpResponseMessage response = await RequestAccountCookiesInTheRequestBody(session, [.. cookies.Keys]);

            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

            IDictionary<object, object> body = await DeserialisePHPResponse(response);

            using (Assert.Multiple())
            {
                await Assert.That(((ICollection) body["aids2cookie"]).Count).IsEqualTo(0);
                await Assert.That(((ICollection) body["aids2gcookie"]).Count).IsEqualTo(0);
            }
        }

        finally
        {
            await unprefixedDistributedCache.KeyDeleteAsync([.. unprefixedKeys]);
        }
    }

    [Test]
    public async Task Server_Upgrades_Request_Never_Answers_With_An_Authentication_Failure()
    {
        await InitialiseIn("Development");

        string matchServerSession = await AuthenticateMatchServer();

        string copyOnWriteMasterSession = await AuthenticateCopyOnWriteMaster();

        string clientCookie = await CreateClientSessionCookie(NonHostAccountName);

        List<string> cookies = [Guid.CreateVersion7().ToString(), string.Empty, matchServerSession, copyOnWriteMasterSession, clientCookie];

        using (Assert.Multiple())
        {
            foreach (string cookie in cookies)
            {
                HttpResponseMessage response = await SendServerRequest("get_upgrades", new () { { "cookie", cookie } });

                await Assert.That(await response.Content.ReadAsStringAsync()).DoesNotContain("auth_failure");
            }
        }
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
    ///     Registers a match server manager and its first match server, and returns the match server's session cookie.
    /// </summary>
    private async Task<string> AuthenticateMatchServer()
    {
        await AuthenticateManager(DedicatedHostAccountName);

        return (string) (await DeserialisePHPResponse(await AuthenticateServer($"{DedicatedHostAccountName}:1")))["session"];
    }

    /// <summary>
    ///     Registers a match server manager and its copy-on-write master, and returns the copy-on-write master's session cookie.
    /// </summary>
    private async Task<string> AuthenticateCopyOnWriteMaster()
    {
        await AuthenticateManager(DedicatedHostAccountName);

        return (string) (await DeserialisePHPResponse(await AuthenticateServer($"{DedicatedHostAccountName}:")))["session"];
    }

    /// <summary>
    ///     Stores a session cookie for the account, as the client's authentication does, and returns the session cookie.
    /// </summary>
    private async Task<string> CreateClientSessionCookie(string accountName)
    {
        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        string cookie = Guid.CreateVersion7().ToString();

        await distributedCache.SetAccountNameForSessionCookie(cookie, accountName);

        return cookie;
    }

    private async Task<int> GetAccountID(string accountName)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext merrickContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        return await merrickContext.Accounts.Where(account => account.Name.Equals(accountName)).Select(account => account.ID).SingleAsync();
    }

    /// <summary>
    ///     Sends the request which a match server sends to authenticate a connecting client.
    /// </summary>
    private async Task<HttpResponseMessage> ConnectClient(string session, string cookie)
        => await SendServerRequest("c_conn", new () { { "session", session }, { "cookie", cookie }, { "ip", "127.0.0.1" }, { "cas", "0" }, { "new", "1" } });

    /// <summary>
    ///     Asserts that the client connection reply authenticates the account, with the "game_cookie" without which the match server disconnects the client, and without "auth_failure", which would make the match server drop its session, or end its match if the match has not started yet.
    /// </summary>
    private async Task AssertClientConnectionReturnsTheAccount(HttpResponseMessage response, string accountName)
    {
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        string body = await response.Content.ReadAsStringAsync();

        IDictionary<object, object> fields = await DeserialisePHPResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(Convert.ToInt32(fields["account_id"])).IsEqualTo(await GetAccountID(accountName));
            await Assert.That(fields).ContainsKey("game_cookie");
            await Assert.That((string) fields["game_cookie"]).IsNotEmpty();
            await Assert.That(body).DoesNotContain("auth_failure");
        }
    }

    /// <summary>
    ///     Sends the request which a match server sends to map account IDs to their session cookies, but with the account IDs in the query string, where the master server reads them, and without a session when the session is <see langword="null"/>.
    /// </summary>
    private async Task<HttpResponseMessage> RequestAccountCookies(string? session, params int[] accountIDs)
    {
        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "S2 Games/Heroes Of Newerth/4.10.1.0/wac/x86_64");

        Dictionary<string, string> formData = [];

        if (session is not null)
            formData.Add("session", session);

        string accountIDsQuery = string.Join('&', accountIDs.Select((accountID, index) => $"aids[{index}]={accountID}"));

        return await httpClient.PostAsync($"server_requester.php?f=aids2cookie&{accountIDsQuery}", new FormUrlEncodedContent(formData));
    }

    /// <summary>
    ///     Sends the request which a match server sends to map account IDs to their session cookies exactly as the match server ("CGameServer::GetAccountAuth") sends it, with each account ID posted as "aids[]" in the request body.
    /// </summary>
    private async Task<HttpResponseMessage> RequestAccountCookiesInTheRequestBody(string session, params int[] accountIDs)
    {
        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "S2 Games/Heroes Of Newerth/4.10.1.0/wac/x86_64");

        List<KeyValuePair<string, string>> formData = [new ("session", session), .. accountIDs.Select(accountID => new KeyValuePair<string, string>("aids[]", accountID.ToString()))];

        return await httpClient.PostAsync("server_requester.php?f=aids2cookie", new FormUrlEncodedContent(formData));
    }

    /// <summary>
    ///     Sends the periodic heartbeat which a match server, or a copy-on-write master, sends, by default while it is idle with no world loaded.
    /// </summary>
    private async Task<HttpResponseMessage> SendHeartbeat(string session, string connectionState = "1")
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
            { "c_state", connectionState },
            { "prev_c_state", "-1" }
        };

        return await httpClient.PostAsync("server_requester.php?f=set_online", new FormUrlEncodedContent(formData));
    }

    /// <summary>
    ///     Sends a request to an endpoint which only a registered match server may use, carrying the session cookie under the form parameter which that endpoint reads it from.
    ///     The statistics submission carries the smallest request which the model binder accepts, so that the session cookie is the only reason for the refusal.
    /// </summary>
    private async Task<HttpResponseMessage> SendMatchServerEndpointRequest(string endpoint, string session, int accountID)
    {
        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "S2 Games/Heroes Of Newerth/4.10.1.0/wac/x86_64");

        Dictionary<string, string> formData = [];

        if (endpoint.StartsWith("master/questserver/", StringComparison.Ordinal))
        {
            formData.Add("cookie", session);
            formData.Add("account_ids[]", $"{accountID}");
        }

        else
        {
            formData.Add("session", session);
            formData.Add("acc_key", "ACCOUNT-KEY");
        }

        if (endpoint.StartsWith("stats_requester.php", StringComparison.Ordinal))
        {
            formData.Add("f", "submit_stats");

            foreach (string field in new [] { "server_id", "match_id", "time_played", "file_size", "c_state", "avgpsr", "avgpsr_team1", "avgpsr_team2", "teamscoregoal", "playerscoregoal", "numrounds",
                "awd_mann", "awd_mqk", "awd_lgks", "awd_msd", "awd_mkill", "awd_masst", "awd_ledth", "awd_mbdmg", "awd_mwk", "awd_mhdd", "awd_hcs" })
                formData.Add($"match_stats[{field}]", "1");

            foreach (string field in new [] { "map", "map_version", "file_name", "version", "gamemode", "release_stage", "submission_debug" })
                formData.Add($"match_stats[{field}]", "x");

            formData.Add("team_stats[1][score]", "0");
            formData.Add("player_stats[1][0][nickname]", "x");
        }

        return await httpClient.PostAsync(endpoint, new FormUrlEncodedContent(formData));
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
