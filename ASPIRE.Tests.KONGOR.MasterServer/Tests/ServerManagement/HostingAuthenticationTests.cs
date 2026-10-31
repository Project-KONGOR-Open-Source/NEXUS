namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.ServerManagement;

/// <summary>
///     Tests for the production-only block that prevents the built-in <see cref="OOTB.Accounts.OPERATOR"/> host account, which ships with a publicly-known password, from hosting match servers.
///     The block is applied at both server requester authentication endpoints (the match server manager's "replay_auth" and the match server's "new_session"), and only when the master server runs in the production environment, so that self-hosters running outside production are unaffected.
///     Also tests how the match server's "new_session" endpoint treats the server instance of the login, including the copy-on-write master (HON's "cowmaster") which Linux match server managers fork their match servers from, and which authenticates with an empty server instance.
///     The copy-on-write master is a non-joinable match server: it is registered apart from the match servers, whether or not its manager is registered, its heartbeat is accepted, and it outlives the removal of its manager.
///     Also tests how the match server endpoints answer a session which is not registered: with the "auth_failure" reply, which makes the match server authenticate again, where acting upon it is safe, with a plain rejection where acting upon it would crash the match server, and as usual where a player reconnecting to a match in progress depends on the reply.
///     Also tests how the client connection request treats a player locked to a match in progress: another registered match server is refused, and the refusal is published, while the locked match server, an unrecognised session, and a copy-on-write master are always served, so that reconnecting to the match never breaks.
///     Also tests that the server list refuses a locked account, with an empty list and the refusal published.
///     Also tests the game cookie which the client connection request issues per account and match server: reconnecting keeps it, whether through the same match server, after that match server renews its session, or through an unrecognised session, while connecting to another match server replaces it, and a refused client connection leaves it unchanged.
///     Also tests that the client connection request refuses the session cookie of a login which a later login has retired, as it refuses any unknown session cookie.
///     Also tests the match server's duplicate session check ("aids2cookie"), which reads the account IDs from the request body, as the match server posts them, and answers, for each account which the asking match server has served, with the session cookie which the account's client on that match server holds and the account's game cookie, so that the match server removes a player who has joined another game, while a player who has logged in elsewhere, a reconnecting player, or one refused elsewhere, is never removed.
///     The tests which cause or observe a refusal run apart from each other, because the refusal channel is shared by every test's master server, unlike the keys of the distributed cache.
/// </summary>
public sealed partial class HostingAuthenticationTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
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
    ///     A match server in a lobby, with its world loaded and its match not started, ends that match and disconnects its players when it reads "auth_failure", as it does for the original master server.
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

    [Test]
    public async Task Manager_Authentication_At_Another_Version_Is_Refused_With_Both_Versions()
    {
        await InitialiseIn("Development");

        HttpResponseMessage response = await AuthenticateManager(DedicatedHostAccountName, userAgent: "S2 Games | Heroes Of Newerth | 5.26.01.01 | WAS");

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
            await Assert.That((string) (await DeserialisePHPResponse(response))["error"]).IsEqualTo($"The Heroes Of Newerth Match Server Version 5.26.01.01 Does Not Match The Expected Version {Distributions.Version}");
        }
    }

    [Test]
    public async Task Server_Authentication_At_Another_Version_Is_Refused_With_Both_Versions()
    {
        await InitialiseIn("Development");

        await AuthenticateManager(DedicatedHostAccountName);

        HttpResponseMessage response = await AuthenticateServer($"{DedicatedHostAccountName}:1", userAgent: "S2 Games | Heroes Of Newerth | 5.26.01.01 | WAS");

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
            await Assert.That((string) (await DeserialisePHPResponse(response))["error"]).IsEqualTo($"The Heroes Of Newerth Match Server Version 5.26.01.01 Does Not Match The Expected Version {Distributions.Version}");
        }
    }

    [Test]
    public async Task Server_Authentication_With_An_Unexpected_User_Agent_Is_Refused_Without_Versions()
    {
        await InitialiseIn("Development");

        await AuthenticateManager(DedicatedHostAccountName);

        HttpResponseMessage response = await AuthenticateServer($"{DedicatedHostAccountName}:1", userAgent: "curl/8.0");

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
            await Assert.That((string) (await DeserialisePHPResponse(response))["error"]).IsEqualTo("Unexpected User Agent");
        }
    }

    [Test]
    public async Task Manager_Authentication_With_An_Unexpected_User_Agent_Is_Refused_Without_Versions()
    {
        await InitialiseIn("Development");

        HttpResponseMessage response = await AuthenticateManager(DedicatedHostAccountName, userAgent: "curl/8.0");

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
            await Assert.That((string) (await DeserialisePHPResponse(response))["error"]).IsEqualTo("Unexpected User Agent");
        }
    }

    [Test]
    public async Task Manager_Authentication_With_Two_User_Agents_Is_Refused_As_An_Unexpected_User_Agent()
    {
        await InitialiseIn("Development");

        string userAgent = $"S2 Games | Heroes Of Newerth | {Distributions.Version} | WAC";

        byte[] form = await new FormUrlEncodedContent(new Dictionary<string, string> { { "login", DedicatedHostAccountName }, { "pass", ComputeServerPasswordHash(HostPassword) } }).ReadAsByteArrayAsync();

        // An HTTP Client Sends Every User-Agent Value In One Header, So The Request Is Built On The Test Server Directly, With The Header Repeated As A Hand-Made Request Can Repeat It
        HttpContext context = await webApplicationFactory.Server.SendAsync(httpContext =>
        {
            httpContext.Request.Method = HttpMethods.Post;
            httpContext.Request.Path = "/server_requester.php";
            httpContext.Request.QueryString = new QueryString("?f=replay_auth");
            httpContext.Request.Headers.UserAgent = new StringValues([ userAgent, userAgent ]);
            httpContext.Request.ContentType = "application/x-www-form-urlencoded";
            httpContext.Request.Body = new MemoryStream(form);
        });

        string body = await new StreamReader(context.Response.Body).ReadToEndAsync();

        using (Assert.Multiple())
        {
            await Assert.That(context.Response.StatusCode).IsEqualTo(StatusCodes.Status400BadRequest);
            await Assert.That(body).Contains("Unexpected User Agent");
        }
    }

    [Test]
    public async Task Server_Authentication_With_An_Incorrect_Password_At_Another_Version_Names_No_Version()
    {
        await InitialiseIn("Development");

        await AuthenticateManager(DedicatedHostAccountName);

        HttpResponseMessage response = await AuthenticateServer($"{DedicatedHostAccountName}:1", password: "INCORRECT-PASSWORD", userAgent: "S2 Games | Heroes Of Newerth | 5.26.01.01 | WAS");

        string body = await response.Content.ReadAsStringAsync();

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
            await Assert.That(body).Contains("Incorrect Password");
            await Assert.That(body).DoesNotContain(Distributions.Version);
        }
    }

    [Test]
    public async Task Start_Game_With_Another_Version_Is_Refused()
    {
        await InitialiseIn("Development");

        string session = await AuthenticateMatchServer();

        HttpResponseMessage response = await SendServerRequest("start_game", MatchStartForm(session, "5.26.10.30"));

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
            await Assert.That(await response.Content.ReadAsStringAsync()).Contains("5.26.10.30");
        }
    }

    [Test]
    public async Task Start_Game_With_The_Distributions_Version_Passes_The_Version_Check()
    {
        await InitialiseIn("Development");

        string session = await AuthenticateMatchServer();

        HttpResponseMessage response = await SendServerRequest("start_game", MatchStartForm(session, Distributions.Version));

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That((await DeserialisePHPResponse(response)).ContainsKey("match_id")).IsTrue();
        }
    }

    private async Task InitialiseIn(string environment)
    {
        await webApplicationFactory.WithEnvironment(environment).WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

        await EnsureAccountExists(OOTB.Accounts.OPERATOR.Name, OOTB.Accounts.OPERATOR.EmailAddress, AccountType.ServerHost);
        await EnsureAccountExists(DedicatedHostAccountName, "dedicated-host@kongor.test", AccountType.ServerHost);
        await EnsureAccountExists(NonHostAccountName, "non-host@kongor.test", AccountType.Normal);
    }

    private async Task<HttpResponseMessage> AuthenticateManager(string login, string? userAgent = null)
    {
        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent ?? $"S2 Games | Heroes Of Newerth | {Distributions.Version} | WAC");

        Dictionary<string, string> formData = new ()
        {
            { "login", login },
            { "pass", ComputeServerPasswordHash(HostPassword) }
        };

        return await httpClient.PostAsync("server_requester.php?f=replay_auth", new FormUrlEncodedContent(formData));
    }

    private async Task<HttpResponseMessage> AuthenticateServer(string login, string password = HostPassword, string port = "11235", string? userAgent = null)
    {
        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent ?? $"S2 Games | Heroes Of Newerth | {Distributions.Version} | WAC");

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
    ///     Logs the account in with a new session cookie, as the client's authentication does, which retires the account's earlier login, and returns the session cookie.
    ///     Unlike the client's authentication, it retires none of the logins of the user's other accounts, so that the tests which log two accounts of one user in keep both logins.
    /// </summary>
    private async Task<string> CreateClientSessionCookie(string accountName)
    {
        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        string cookie = Guid.CreateVersion7().ToString();

        await distributedCache.SupersedeSessionCookies(await GetAccountID(accountName), accountName, cookie, []);

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
    ///     Sends the request which a match server sends to map account IDs to their session cookies, but with the account IDs in the query string, where the match server never posts them, and without a session when the session is <see langword="null"/>.
    ///     Each account ID is sent both under an indexed "aids[N]" key and under the literal "aids[]" key which the match server posts in the request body, so that reading either from the query string is detected.
    /// </summary>
    private async Task<HttpResponseMessage> RequestAccountCookies(string? session, params int[] accountIDs)
    {
        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"S2 Games | Heroes Of Newerth | {Distributions.Version} | WAC");

        Dictionary<string, string> formData = [];

        if (session is not null)
            formData.Add("session", session);

        string accountIDsQuery = string.Join('&', accountIDs.Select((accountID, index) => $"aids[{index}]={accountID}&aids[]={accountID}"));

        return await httpClient.PostAsync($"server_requester.php?f=aids2cookie&{accountIDsQuery}", new FormUrlEncodedContent(formData));
    }

    /// <summary>
    ///     Sends the request which a match server sends to map account IDs to their session cookies exactly as the match server ("CGameServer::GetAccountAuth") sends it, with each account ID posted as "aids[]" in the request body.
    /// </summary>
    private async Task<HttpResponseMessage> RequestAccountCookiesInTheRequestBody(string session, params int[] accountIDs)
        => await RequestAccountCookiesInTheRequestBody(session, accountIDs.Select(accountID => accountID.ToString()).ToArray());

    /// <summary>
    ///     Sends the request which a match server sends to map account IDs to their session cookies, with each value posted as "aids[]" in the request body exactly as given, whether or not it is a valid account ID.
    /// </summary>
    private async Task<HttpResponseMessage> RequestAccountCookiesInTheRequestBody(string session, params string[] accountIDValues)
    {
        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"S2 Games | Heroes Of Newerth | {Distributions.Version} | WAC");

        List<KeyValuePair<string, string>> formData = [new ("session", session), .. accountIDValues.Select(accountIDValue => new KeyValuePair<string, string>("aids[]", accountIDValue))];

        return await httpClient.PostAsync("server_requester.php?f=aids2cookie", new FormUrlEncodedContent(formData));
    }

    /// <summary>
    ///     Sends the periodic heartbeat which a match server, or a copy-on-write master, sends, by default while it is idle with no world loaded.
    /// </summary>
    private async Task<HttpResponseMessage> SendHeartbeat(string session, string connectionState = "1")
    {
        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"S2 Games | Heroes Of Newerth | {Distributions.Version} | WAC");

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

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"S2 Games | Heroes Of Newerth | {Distributions.Version} | WAC");

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

    /// <summary>
    ///     The form a match server posts to start a match, as "start_game" reads it.
    /// </summary>
    private static Dictionary<string, string> MatchStartForm(string session, string version) => new ()
    {
        { "session", session },
        { "map", "caldavar" },
        { "version", version },
        { "mname", "TEST-MATCH" },
        { "mstr", $"{DedicatedHostAccountName}:" },
        { "casual", "0" },
        { "arrangedmatchtype", "0" },
        { "match_mode", "1" }
    };

    private async Task<HttpResponseMessage> SendServerRequest(string function, Dictionary<string, string> formData)
    {
        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"S2 Games | Heroes Of Newerth | {Distributions.Version} | WAC");

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
