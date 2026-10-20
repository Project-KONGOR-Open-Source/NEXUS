namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.ServerManagement;

public sealed partial class HostingAuthenticationTests
{
    /// <summary>
    ///     The name of an account of the same user as <see cref="NonHostAccountName"/>, which is not the user's main account.
    /// </summary>
    private const string NonHostSubAccountName = "NON-HOST-SUB";

    /// <summary>
    ///     How long a published refusal is awaited.
    /// </summary>
    private static readonly TimeSpan RefusalTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     How long a test listens to be sure that no refusal is published.
    /// </summary>
    private static readonly TimeSpan RefusalQuietPeriod = TimeSpan.FromSeconds(2);

    [Test]
    [NotInParallel(DistributedCacheExtensions.MatchLockRefusalChannel)]
    public async Task Client_Connection_From_Another_Registered_Server_Is_Refused_For_A_Locked_Account()
    {
        await InitialiseIn("Development");

        (_, int lockedServerID) = await RegisterMatchServer(1);
        (string otherSession, _) = await RegisterMatchServer(2);

        MatchLock matchLock = await LockAccount(NonHostAccountName, lockedServerID);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        HttpResponseMessage response = await ConnectClient(otherSession, cookie);

        await AssertClientConnectionIsRefused(response);

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        // A Refusal Leaves Everything As It Was, So That The Player Is Still Sent Back To The Locked Match, And Can Still Join Once The Lock Lifts
        using (Assert.Multiple())
        {
            await Assert.That((await distributedCache.GetMatchLock(matchLock.UserID))?.MatchID).IsEqualTo(matchLock.MatchID);
            await Assert.That(await distributedCache.GetAccountNameForSessionCookie(cookie)).IsEqualTo(NonHostAccountName);
        }
    }

    [Test]
    [NotInParallel(DistributedCacheExtensions.MatchLockRefusalChannel)]
    public async Task Client_Connection_From_The_Locked_Match_Server_Is_Served()
    {
        await InitialiseIn("Development");

        (string lockedSession, int lockedServerID) = await RegisterMatchServer(1);
        (string otherSession, _) = await RegisterMatchServer(2);

        await LockAccount(NonHostAccountName, lockedServerID);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        await AssertClientConnectionReturnsTheAccount(await ConnectClient(lockedSession, cookie), NonHostAccountName);

        // The Same Lock Refuses Another Match Server, So That The Locked Match Server Is Served Because It Is The Locked One
        await AssertClientConnectionIsRefused(await ConnectClient(otherSession, cookie));
    }

    [Test]
    [NotInParallel(DistributedCacheExtensions.MatchLockRefusalChannel)]
    public async Task Client_Connection_From_The_Locked_Match_Server_After_It_Renews_Its_Session_Is_Served()
    {
        await InitialiseIn("Development");

        (string expiredSession, int lockedServerID) = await RegisterMatchServer(1);
        (string otherSession, _) = await RegisterMatchServer(2);

        await LockAccount(NonHostAccountName, lockedServerID);

        // The Locked Match Server Renews Its Session While Its Match Is In Progress
        (string renewedSession, int renewedServerID) = await RegisterMatchServer(1);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        using (Assert.Multiple())
        {
            await Assert.That(renewedSession).IsNotEqualTo(expiredSession);
            await Assert.That(renewedServerID).IsEqualTo(lockedServerID);
        }

        await AssertClientConnectionReturnsTheAccount(await ConnectClient(renewedSession, cookie), NonHostAccountName);

        // The Same Lock Refuses Another Match Server, So That The Locked Match Server Is Served Because It Is The Locked One
        await AssertClientConnectionIsRefused(await ConnectClient(otherSession, cookie));
    }

    [Test]
    [NotInParallel(DistributedCacheExtensions.MatchLockRefusalChannel)]
    public async Task Client_Connection_With_An_Unrecognised_Session_Is_Served_For_A_Locked_Account()
    {
        await InitialiseIn("Development");

        (_, int lockedServerID) = await RegisterMatchServer(1);
        (string otherSession, _) = await RegisterMatchServer(2);

        await LockAccount(NonHostAccountName, lockedServerID);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        // The Locked Match Server May Be The One Whose Session The Master Server Has Lost
        await AssertClientConnectionReturnsTheAccount(await ConnectClient(Guid.CreateVersion7().ToString(), cookie), NonHostAccountName);

        // The Same Lock Refuses Another Match Server, So That The Unrecognised Session Is Served Because It Is Unrecognised
        await AssertClientConnectionIsRefused(await ConnectClient(otherSession, cookie));
    }

    [Test]
    [NotInParallel(DistributedCacheExtensions.MatchLockRefusalChannel)]
    public async Task Client_Connection_With_A_Copy_On_Write_Master_Session_Is_Served_For_A_Locked_Account()
    {
        await InitialiseIn("Development");

        string copyOnWriteMasterSession = await AuthenticateCopyOnWriteMaster();

        (_, int lockedServerID) = await RegisterMatchServer(1);
        (string otherSession, _) = await RegisterMatchServer(2);

        await LockAccount(NonHostAccountName, lockedServerID);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        // The Master Server Cannot Tell Which Match Server Asks With The Copy-On-Write Master's Session, So It May Be The Locked One
        await AssertClientConnectionReturnsTheAccount(await ConnectClient(copyOnWriteMasterSession, cookie), NonHostAccountName);

        // The Same Lock Refuses Another Match Server, So That The Copy-On-Write Master's Session Is Served Because The Match Server Which Asks Is Unknown
        await AssertClientConnectionIsRefused(await ConnectClient(otherSession, cookie));
    }

    [Test]
    [NotInParallel(DistributedCacheExtensions.MatchLockRefusalChannel)]
    public async Task Client_Connection_For_Another_Account_Of_A_Locked_User_Is_Refused()
    {
        await InitialiseIn("Development");

        await CreateSubAccount(NonHostAccountName, NonHostSubAccountName);

        (_, int lockedServerID) = await RegisterMatchServer(1);
        (string otherSession, _) = await RegisterMatchServer(2);

        await LockAccount(NonHostAccountName, lockedServerID);

        string cookie = await CreateClientSessionCookie(NonHostSubAccountName);

        await AssertClientConnectionIsRefused(await ConnectClient(otherSession, cookie));
    }

    [Test]
    public async Task Client_Connection_Is_Served_And_The_Lock_Lifted_When_The_Locked_Server_Is_Gone()
    {
        await InitialiseIn("Development");

        (string session, int serverID) = await RegisterMatchServer(1);

        // No Other Match Server Is Registered, So The Lock Names A Match Server Which Is Gone
        MatchLock matchLock = await LockAccount(NonHostAccountName, serverID + 1);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        await AssertClientConnectionReturnsTheAccount(await ConnectClient(session, cookie), NonHostAccountName);

        await Assert.That(await webApplicationFactory.Services.GetRequiredService<IDatabase>().GetMatchLock(matchLock.UserID)).IsNull();
    }

    [Test]
    public async Task Client_Connection_Is_Served_When_The_Match_Lock_Cannot_Be_Read()
    {
        await InitialiseIn("Development");

        (_, int lockedServerID) = await RegisterMatchServer(1);
        (string otherSession, _) = await RegisterMatchServer(2);

        MatchLock matchLock = await LockAccount(NonHostAccountName, lockedServerID);

        // The Stored Lock No Longer Deserialises, As It Would After A Change To The Shape Of The Lock
        await webApplicationFactory.Services.GetRequiredService<IDatabase>().StringSetAsync(DistributedCacheExtensions.ConstructMatchLockKey(matchLock.UserID), "NOT-A-MATCH-LOCK", MatchLock.Lifetime);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        await AssertClientConnectionReturnsTheAccount(await ConnectClient(otherSession, cookie), NonHostAccountName);
    }

    [Test]
    [Arguments(AccountType.Guest)]
    [Arguments(AccountType.Staff)]
    public async Task Client_Connection_Is_Never_Refused_For_Guest_And_Staff_Accounts(AccountType accountType)
    {
        await InitialiseIn("Development");

        string accountName = $"{accountType.ToString().ToUpperInvariant()}-ACCOUNT";

        await EnsureAccountExists(accountName, $"{accountName.ToLowerInvariant()}@kongor.test", accountType);

        (_, int lockedServerID) = await RegisterMatchServer(1);
        (string otherSession, _) = await RegisterMatchServer(2);

        // The Chat Server Never Locks These Accounts, Because Each Type Shares One User, So This Lock Stands In For One Left Behind By Any Other Means
        await LockAccount(accountName, lockedServerID);

        string cookie = await CreateClientSessionCookie(accountName);

        await AssertClientConnectionReturnsTheAccount(await ConnectClient(otherSession, cookie), accountName);
    }

    [Test]
    [NotInParallel(DistributedCacheExtensions.MatchLockRefusalChannel)]
    public async Task A_Refused_Client_Connection_Publishes_The_User_ID_And_Account_Name()
    {
        await InitialiseIn("Development");

        (_, int lockedServerID) = await RegisterMatchServer(1);
        (string otherSession, _) = await RegisterMatchServer(2);

        MatchLock matchLock = await LockAccount(NonHostAccountName, lockedServerID);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        ChannelMessageQueue refusals = await SubscribeToRefusals();

        try
        {
            HttpResponseMessage response = await ConnectClient(otherSession, cookie);

            MatchLockRefusal? refusal = await ReadRefusal(refusals, RefusalTimeout);

            using (Assert.Multiple())
            {
                await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
                await Assert.That(refusal?.UserID).IsEqualTo(matchLock.UserID);
                await Assert.That(refusal?.AccountName).IsEqualTo(NonHostAccountName);
            }
        }

        finally
        {
            await refusals.UnsubscribeAsync();
        }
    }

    [Test]
    [NotInParallel(DistributedCacheExtensions.MatchLockRefusalChannel)]
    public async Task A_Served_Client_Connection_Publishes_Nothing()
    {
        await InitialiseIn("Development");

        (string lockedSession, int lockedServerID) = await RegisterMatchServer(1);

        await LockAccount(NonHostAccountName, lockedServerID);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        ChannelMessageQueue refusals = await SubscribeToRefusals();

        try
        {
            HttpResponseMessage response = await ConnectClient(lockedSession, cookie);

            MatchLockRefusal? refusal = await ReadRefusal(refusals, RefusalQuietPeriod);

            await AssertClientConnectionReturnsTheAccount(response, NonHostAccountName);

            await Assert.That(refusal).IsNull();
        }

        finally
        {
            await refusals.UnsubscribeAsync();
        }
    }

    [Test]
    public async Task Reconnecting_To_The_Same_Match_Server_Returns_The_Same_Game_Cookie()
    {
        await InitialiseIn("Development");

        (string session, _) = await RegisterMatchServer(1);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        string firstGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, cookie)))["game_cookie"];
        string secondGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, cookie)))["game_cookie"];

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        using (Assert.Multiple())
        {
            await Assert.That(secondGameCookie).IsEqualTo(firstGameCookie);
            await Assert.That(await distributedCache.GetGameCookie(await GetAccountID(NonHostAccountName))).IsEqualTo(firstGameCookie);
        }
    }

    [Test]
    public async Task Reconnecting_After_The_Match_Server_Renews_Its_Session_Returns_The_Same_Game_Cookie()
    {
        await InitialiseIn("Development");

        (string expiredSession, int serverID) = await RegisterMatchServer(1);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        string firstGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(expiredSession, cookie)))["game_cookie"];

        // The Match Server Renews Its Session While Its Match Is In Progress
        (string renewedSession, int renewedServerID) = await RegisterMatchServer(1);

        string secondGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(renewedSession, cookie)))["game_cookie"];

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        using (Assert.Multiple())
        {
            await Assert.That(renewedSession).IsNotEqualTo(expiredSession);
            await Assert.That(renewedServerID).IsEqualTo(serverID);
            await Assert.That(secondGameCookie).IsEqualTo(firstGameCookie);
            await Assert.That(await distributedCache.GetGameCookie(await GetAccountID(NonHostAccountName))).IsEqualTo(firstGameCookie);
        }
    }

    [Test]
    public async Task Reconnecting_With_An_Unrecognised_Session_Returns_The_Current_Game_Cookie()
    {
        await InitialiseIn("Development");

        (string session, _) = await RegisterMatchServer(1);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        string firstGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, cookie)))["game_cookie"];

        // The Match Server May Be The One Whose Session The Master Server Has Lost
        string secondGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(Guid.CreateVersion7().ToString(), cookie)))["game_cookie"];

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        using (Assert.Multiple())
        {
            await Assert.That(secondGameCookie).IsEqualTo(firstGameCookie);
            await Assert.That(await distributedCache.GetGameCookie(await GetAccountID(NonHostAccountName))).IsEqualTo(firstGameCookie);
        }
    }

    [Test]
    public async Task Connecting_To_Another_Match_Server_Returns_A_New_Game_Cookie()
    {
        await InitialiseIn("Development");

        (string firstSession, _) = await RegisterMatchServer(1);
        (string secondSession, _) = await RegisterMatchServer(2);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        string firstGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(firstSession, cookie)))["game_cookie"];
        string secondGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(secondSession, cookie)))["game_cookie"];

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        using (Assert.Multiple())
        {
            await Assert.That(secondGameCookie).IsNotEqualTo(firstGameCookie);
            await Assert.That(await distributedCache.GetGameCookie(await GetAccountID(NonHostAccountName))).IsEqualTo(secondGameCookie);
        }
    }

    [Test]
    public async Task Different_Accounts_Receive_Different_Game_Cookies()
    {
        await InitialiseIn("Development");

        await CreateSubAccount(NonHostAccountName, NonHostSubAccountName);

        (string session, _) = await RegisterMatchServer(1);

        string mainAccountCookie = await CreateClientSessionCookie(NonHostAccountName);
        string subAccountCookie = await CreateClientSessionCookie(NonHostSubAccountName);

        string mainAccountGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, mainAccountCookie)))["game_cookie"];
        string subAccountGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, subAccountCookie)))["game_cookie"];

        await Assert.That(subAccountGameCookie).IsNotEqualTo(mainAccountGameCookie);
    }

    [Test]
    [NotInParallel(DistributedCacheExtensions.MatchLockRefusalChannel)]
    public async Task A_Refused_Client_Connection_Leaves_The_Game_Cookie_Unchanged()
    {
        await InitialiseIn("Development");

        (string lockedSession, int lockedServerID) = await RegisterMatchServer(1);
        (string otherSession, int otherServerID) = await RegisterMatchServer(2);

        await LockAccount(NonHostAccountName, lockedServerID);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        string lockedGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(lockedSession, cookie)))["game_cookie"];

        await AssertClientConnectionIsRefused(await ConnectClient(otherSession, cookie));

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        int accountID = await GetAccountID(NonHostAccountName);

        // An Attempt To Join Another Game While Locked Never Replaces The Game Cookie, So That The Locked Match Server Never Removes The Player From Their Match
        using (Assert.Multiple())
        {
            await Assert.That(await distributedCache.GetGameCookie(accountID)).IsEqualTo(lockedGameCookie);
            await Assert.That(await distributedCache.GetAccountsConnectedToMatchServer(otherServerID)).DoesNotContain(accountID);
            await Assert.That((string) (await DeserialisePHPResponse(await ConnectClient(lockedSession, cookie)))["game_cookie"]).IsEqualTo(lockedGameCookie);
        }
    }

    [Test]
    public async Task Connecting_Elsewhere_After_The_Lock_Lifts_Returns_A_New_Game_Cookie()
    {
        await InitialiseIn("Development");

        (string lockedSession, int lockedServerID) = await RegisterMatchServer(1);
        (string otherSession, _) = await RegisterMatchServer(2);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        string lockedGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(lockedSession, cookie)))["game_cookie"];

        MatchLock matchLock = await LockAccount(NonHostAccountName, lockedServerID);

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        // Every Lift Removes The Lock, Whatever Lifts It
        await Assert.That(await distributedCache.RemoveMatchLock(matchLock.UserID, matchLock.MatchID)).IsTrue();

        string otherGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(otherSession, cookie)))["game_cookie"];

        using (Assert.Multiple())
        {
            await Assert.That(otherGameCookie).IsNotEqualTo(lockedGameCookie);
            await Assert.That(await distributedCache.GetGameCookie(await GetAccountID(NonHostAccountName))).IsEqualTo(otherGameCookie);
        }
    }

    [Test]
    public async Task A_Served_Client_Connection_Records_The_Account_On_The_Match_Server()
    {
        await InitialiseIn("Development");

        (string session, int serverID) = await RegisterMatchServer(1);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        await AssertClientConnectionReturnsTheAccount(await ConnectClient(session, cookie), NonHostAccountName);

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        await Assert.That(await distributedCache.GetAccountsConnectedToMatchServer(serverID)).Contains(await GetAccountID(NonHostAccountName));
    }

    [Test]
    public async Task Client_Connection_Replaces_A_Game_Cookie_Which_Cannot_Be_Read()
    {
        await InitialiseIn("Development");

        (string session, _) = await RegisterMatchServer(1);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        int accountID = await GetAccountID(NonHostAccountName);

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        // The Stored Game Cookie No Longer Deserialises, As It Would After A Change To The Shape Of The Game Cookie
        await distributedCache.StringSetAsync(DistributedCacheExtensions.ConstructAccountGameCookieKey(accountID), "NOT-A-GAME-COOKIE", DistributedCacheExtensions.SessionCookieLifetime);

        HttpResponseMessage response = await ConnectClient(session, cookie);

        await AssertClientConnectionReturnsTheAccount(response, NonHostAccountName);

        // The Reconnecting Client Carries The Replacement, So That It Still Agrees With The Stored Game Cookie
        await Assert.That(await distributedCache.GetGameCookie(accountID)).IsEqualTo((string) (await DeserialisePHPResponse(response))["game_cookie"]);
    }

    /// <summary>
    ///     Registers the dedicated host's match server manager and its match server with the given instance, and returns the match server's session cookie and server ID.
    ///     Registering the same instance again renews its session and keeps its server ID, as a match server does when it authenticates again.
    /// </summary>
    private async Task<(string Session, int ServerID)> RegisterMatchServer(int instance)
    {
        await AuthenticateManager(DedicatedHostAccountName);

        IDictionary<object, object> body = await DeserialisePHPResponse(await AuthenticateServer($"{DedicatedHostAccountName}:{instance}"));

        return ((string) body["session"], Convert.ToInt32(body["server_id"]));
    }

    /// <summary>
    ///     Locks the user of the account to a match in progress on the match server with the given server ID, with the account holding the user's slot, as the chat server does when a competitive matchmade match is announced, and returns the lock.
    /// </summary>
    private async Task<MatchLock> LockAccount(string accountName, int serverID)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext merrickContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account account = await merrickContext.Accounts.Include(account => account.User).SingleAsync(account => account.Name.Equals(accountName));

        MatchLock matchLock = new ()
        {
            UserID = account.User.ID,
            AccountID = account.ID,
            AccountName = account.Name,
            MatchID = Random.Shared.Next(1, int.MaxValue),
            CorrelationID = Random.Shared.Next(1, int.MaxValue),
            ArrangedMatchType = MatchType.AM_MATCHMAKING,
            ServerID = serverID,
            ServerAddress = "127.0.0.1",
            ServerPort = 11235,
            TimestampStarted = DateTimeOffset.UtcNow
        };

        await webApplicationFactory.Services.GetRequiredService<IDatabase>().SetMatchLock(matchLock);

        return matchLock;
    }

    /// <summary>
    ///     Adds an account which is not the main account to the user of the main account.
    /// </summary>
    private async Task CreateSubAccount(string mainAccountName, string subAccountName)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext merrickContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        User user = await merrickContext.Users.SingleAsync(user => user.Accounts.Any(account => account.Name.Equals(mainAccountName)));

        Account subAccount = new ()
        {
            Name = subAccountName,
            User = user,
            Type = AccountType.Normal,
            IsMain = false
        };

        await merrickContext.Accounts.AddAsync(subAccount);

        await merrickContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Asserts that the client connection reply refuses the client, without authenticating the account, and without "auth_failure", which would make the match server drop its session, or end its match if the match has not started yet.
    ///     The reply also never sets the "cookie" to "Invalid cookie.", after which the match server refuses the client's session cookie for as long as it runs, so that the player could not join it even once the lock lifts.
    /// </summary>
    private static async Task AssertClientConnectionIsRefused(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
            await Assert.That(body).DoesNotContain("account_id");
            await Assert.That(body).DoesNotContain("game_cookie");
            await Assert.That(body).DoesNotContain("auth_failure");
            await Assert.That(body).DoesNotContain("Invalid cookie.");
        }
    }

    /// <summary>
    ///     Subscribes to the channel on which the master server publishes its refusals of client connections.
    /// </summary>
    private async Task<ChannelMessageQueue> SubscribeToRefusals()
        => await webApplicationFactory.Services.GetRequiredService<IConnectionMultiplexer>().GetSubscriber().SubscribeAsync(RedisChannel.Literal(DistributedCacheExtensions.MatchLockRefusalChannel));

    /// <summary>
    ///     Reads the next refusal which is published within the timeout, or returns <see langword="null"/> when none is.
    /// </summary>
    private static async Task<MatchLockRefusal?> ReadRefusal(ChannelMessageQueue refusals, TimeSpan timeout)
    {
        using CancellationTokenSource cancellation = new (timeout);

        try
        {
            ChannelMessage message = await refusals.ReadAsync(cancellation.Token);

            return JsonSerializer.Deserialize<MatchLockRefusal>(message.Message.ToString());
        }

        catch (OperationCanceledException)
        {
            return null;
        }
    }
}
