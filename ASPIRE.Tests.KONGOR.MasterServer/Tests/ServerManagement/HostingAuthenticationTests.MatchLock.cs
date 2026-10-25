namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.ServerManagement;

public sealed partial class HostingAuthenticationTests
{
    /// <summary>
    ///     The name of an account of the same user as <see cref="NonHostAccountName"/>, which is not the user's main account.
    /// </summary>
    private const string NonHostSubAccountName = "NON-HOST-SUB";

    /// <summary>
    ///     The name of the main account of a user other than the user of <see cref="NonHostAccountName"/>.
    /// </summary>
    private const string OtherNonHostAccountName = "OTHER-NON-HOST";

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

        await AssertClientConnectionIsRejectedAtOnce(response, NonHostAccountName);

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
        await AssertClientConnectionIsRejectedAtOnce(await ConnectClient(otherSession, cookie), NonHostAccountName);
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
        await AssertClientConnectionIsRejectedAtOnce(await ConnectClient(otherSession, cookie), NonHostAccountName);
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
        await AssertClientConnectionIsRejectedAtOnce(await ConnectClient(otherSession, cookie), NonHostAccountName);
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
        await AssertClientConnectionIsRejectedAtOnce(await ConnectClient(otherSession, cookie), NonHostAccountName);
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

        await AssertClientConnectionIsRejectedAtOnce(await ConnectClient(otherSession, cookie), NonHostSubAccountName);
    }

    [Test]
    [NotInParallel(DistributedCacheExtensions.MatchLockRefusalChannel)]
    public async Task Client_Connection_From_The_Locked_Match_Server_For_Another_Account_Of_The_Locked_User_Is_Refused()
    {
        await InitialiseIn("Development");

        await CreateSubAccount(NonHostAccountName, NonHostSubAccountName);

        (string lockedSession, int lockedServerID) = await RegisterMatchServer(1);

        MatchLock matchLock = await LockAccount(NonHostAccountName, lockedServerID);

        string gameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(lockedSession, await CreateClientSessionCookie(NonHostAccountName))))["game_cookie"];

        string subAccountCookie = await CreateClientSessionCookie(NonHostSubAccountName);

        // Another Account Of The User Cannot Take The Slot, So It Could Only Spectate The User's Own Match
        await AssertClientConnectionIsRejectedAtOnce(await ConnectClient(lockedSession, subAccountCookie), NonHostSubAccountName);

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        // The Locked Account Logs In Again And Reconnects, Undisturbed By The Refusal
        string reconnectedGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(lockedSession, await CreateClientSessionCookie(NonHostAccountName))))["game_cookie"];

        using (Assert.Multiple())
        {
            await Assert.That(await distributedCache.GetGameCookie(await GetAccountID(NonHostSubAccountName))).IsNull();
            await Assert.That(reconnectedGameCookie).IsEqualTo(gameCookie);
            await Assert.That((await distributedCache.GetMatchLock(matchLock.UserID))?.MatchID).IsEqualTo(matchLock.MatchID);
        }
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
    public async Task Client_Connection_With_A_Retired_Session_Cookie_Is_Refused_Without_Blacklisting_It()
    {
        await InitialiseIn("Development");

        (string session, _) = await RegisterMatchServer(1);

        string retiredCookie = await CreateClientSessionCookie(NonHostAccountName);

        // The Player Logs In Again, Which Retires The Earlier Login
        await CreateClientSessionCookie(NonHostAccountName);

        HttpResponseMessage response = await ConnectClient(session, retiredCookie);

        string body = await response.Content.ReadAsStringAsync();

        // A Retired Session Cookie Is Refused As Any Unknown Session Cookie Is, Never With "auth_failure", Which Makes The Match Server Drop Its Own Session, Nor With "Invalid cookie.", Which Makes It Blacklist The Session Cookie
        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
            await Assert.That(body).DoesNotContain("account_id");
            await Assert.That(body).DoesNotContain("game_cookie");
            await Assert.That(body).DoesNotContain("auth_failure");
            await Assert.That(body).DoesNotContain("Invalid cookie.");
            await Assert.That(await webApplicationFactory.Services.GetRequiredService<IDatabase>().GetGameCookie(await GetAccountID(NonHostAccountName))).IsNull();
        }
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
    public async Task Connecting_With_An_Unrecognised_Session_First_Then_To_A_Registered_Server_Returns_A_New_Game_Cookie()
    {
        await InitialiseIn("Development");

        (string session, int serverID) = await RegisterMatchServer(1);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        int accountID = await GetAccountID(NonHostAccountName);

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        string unrecognisedSessionGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(Guid.CreateVersion7().ToString(), cookie)))["game_cookie"];

        // The Master Server Cannot Tell Which Match Server Asks With An Unrecognised Session, So The Game Cookie Which It Issues Belongs To No Match Server
        GameCookie? unrecognisedSessionRecord = JsonSerializer.Deserialize<GameCookie>((await distributedCache.StringGetAsync(DistributedCacheExtensions.ConstructAccountGameCookieKey(accountID))).ToString());

        List<int> accountsBeforeRegisteredConnection = await distributedCache.GetAccountsConnectedToMatchServer(serverID, [accountID]);

        string registeredServerGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, cookie)))["game_cookie"];

        using (Assert.Multiple())
        {
            await Assert.That(unrecognisedSessionRecord?.Cookie).IsEqualTo(unrecognisedSessionGameCookie);
            await Assert.That(unrecognisedSessionRecord?.ServerID).IsNull();
            await Assert.That(accountsBeforeRegisteredConnection).DoesNotContain(accountID);
            await Assert.That(registeredServerGameCookie).IsNotEqualTo(unrecognisedSessionGameCookie);
            await Assert.That(await distributedCache.GetGameCookie(accountID)).IsEqualTo(registeredServerGameCookie);
            await Assert.That(await distributedCache.GetAccountsConnectedToMatchServer(serverID, [accountID])).Contains(accountID);
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

        await AssertClientConnectionIsRejectedAtOnce(await ConnectClient(otherSession, cookie), NonHostAccountName);

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        int accountID = await GetAccountID(NonHostAccountName);

        // An Attempt To Join Another Game While Locked Never Replaces The Game Cookie, So That The Locked Match Server Never Removes The Player From Their Match
        using (Assert.Multiple())
        {
            await Assert.That(await distributedCache.GetGameCookie(accountID)).IsEqualTo(lockedGameCookie);
            await Assert.That(await distributedCache.GetAccountsConnectedToMatchServer(otherServerID, [accountID])).DoesNotContain(accountID);
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

        int accountID = await GetAccountID(NonHostAccountName);

        await Assert.That(await distributedCache.GetAccountsConnectedToMatchServer(serverID, [accountID])).Contains(accountID);
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

    [Test]
    public async Task Account_IDs_To_Cookie_Answers_With_The_Session_Cookie_And_The_Game_Cookie_Of_Each_Served_Client()
    {
        await InitialiseIn("Development");

        (string session, int _) = await RegisterMatchServer(1);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        string gameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, cookie)))["game_cookie"];

        int accountID = await GetAccountID(NonHostAccountName);

        IDictionary<object, object> body = await DeserialisePHPResponse(await RequestAccountCookiesInTheRequestBody(session, accountID));

        await Assert.That((string) ((IDictionary<object, object>) body["aids2cookie"])[accountID.ToString()]).IsEqualTo(cookie);
        await Assert.That((string) ((IDictionary<object, object>) body["aids2gcookie"])[accountID.ToString()]).IsEqualTo(gameCookie);
    }

    [Test]
    public async Task Account_IDs_To_Cookie_Lists_Both_Cookies_In_The_Same_Account_Order()
    {
        await InitialiseIn("Development");

        await CreateSubAccount(NonHostAccountName, NonHostSubAccountName);

        (string session, _) = await RegisterMatchServer(1);

        await AssertClientConnectionReturnsTheAccount(await ConnectClient(session, await CreateClientSessionCookie(NonHostAccountName)), NonHostAccountName);
        await AssertClientConnectionReturnsTheAccount(await ConnectClient(session, await CreateClientSessionCookie(NonHostSubAccountName)), NonHostSubAccountName);

        (IDictionary<object, object> sessionCookies, IDictionary<object, object> gameCookies) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(session, await GetAccountID(NonHostSubAccountName), await GetAccountID(NonHostAccountName)));

        // The Match Server Pairs Each Session Cookie With The Game Cookie At The Same Position, Rather Than With The Game Cookie Of The Same Account
        using (Assert.Multiple())
        {
            await Assert.That(sessionCookies.Count).IsEqualTo(2);
            await Assert.That(gameCookies.Keys).IsEquivalentTo(sessionCookies.Keys, TUnit.Assertions.Enums.CollectionOrdering.Matching);
        }
    }

    [Test]
    public async Task Account_IDs_To_Cookie_Keeps_Both_Lists_Aligned_Around_An_Omitted_Account()
    {
        await InitialiseIn("Development");

        await CreateSubAccount(NonHostAccountName, NonHostSubAccountName);

        await EnsureAccountExists(OtherNonHostAccountName, "other-non-host@kongor.test", AccountType.Normal);

        (string session, _) = await RegisterMatchServer(1);

        int firstAccountID = await GetAccountID(NonHostAccountName);
        int omittedAccountID = await GetAccountID(NonHostSubAccountName);
        int lastAccountID = await GetAccountID(OtherNonHostAccountName);

        string firstCookie = await CreateClientSessionCookie(NonHostAccountName);
        string omittedCookie = await CreateClientSessionCookie(NonHostSubAccountName);
        string lastCookie = await CreateClientSessionCookie(OtherNonHostAccountName);

        string firstGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, firstCookie)))["game_cookie"];
        string omittedGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, omittedCookie)))["game_cookie"];
        string lastGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, lastCookie)))["game_cookie"];

        // The Middle Account Lacks Its Game Cookie, So That It Is Left Out While Accounts Are Answered On Either Side Of It
        await webApplicationFactory.Services.GetRequiredService<IDatabase>().KeyDeleteAsync(DistributedCacheExtensions.ConstructAccountGameCookieKey(omittedAccountID));

        (IDictionary<object, object> sessionCookies, IDictionary<object, object> gameCookies) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(session, firstAccountID, omittedAccountID, lastAccountID));

        // The Match Server Pairs Both Lists By Position, So Leaving An Account Out Of Only One List Would Pair Every Later Account With Another Account's Game Cookie
        using (Assert.Multiple())
        {
            await Assert.That(omittedGameCookie).IsNotEmpty();
            await Assert.That(sessionCookies.Keys.Select(key => (string) key)).IsEquivalentTo([firstAccountID.ToString(), lastAccountID.ToString()], TUnit.Assertions.Enums.CollectionOrdering.Matching);
            await Assert.That(gameCookies.Keys.Select(key => (string) key)).IsEquivalentTo([firstAccountID.ToString(), lastAccountID.ToString()], TUnit.Assertions.Enums.CollectionOrdering.Matching);
            await Assert.That(sessionCookies.Values.Select(value => (string) value)).IsEquivalentTo([firstCookie, lastCookie], TUnit.Assertions.Enums.CollectionOrdering.Matching);
            await Assert.That(gameCookies.Values.Select(value => (string) value)).IsEquivalentTo([firstGameCookie, lastGameCookie], TUnit.Assertions.Enums.CollectionOrdering.Matching);
        }
    }

    [Test]
    public async Task Account_IDs_To_Cookie_Omits_Accounts_The_Asking_Server_Never_Served()
    {
        await InitialiseIn("Development");

        (string askingSession, _) = await RegisterMatchServer(1);
        (string servingSession, _) = await RegisterMatchServer(2);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        int accountID = await GetAccountID(NonHostAccountName);

        await AssertClientConnectionReturnsTheAccount(await ConnectClient(servingSession, cookie), NonHostAccountName);

        (IDictionary<object, object> askingSessionCookies, IDictionary<object, object> askingGameCookies) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(askingSession, accountID));

        (IDictionary<object, object> servingSessionCookies, _) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(servingSession, accountID));

        // The Match Server Which Served The Account Is Answered, So That The Other Is Answered Nothing Only Because It Never Served The Account
        using (Assert.Multiple())
        {
            await Assert.That(askingSessionCookies.Count).IsEqualTo(0);
            await Assert.That(askingGameCookies.Count).IsEqualTo(0);
            await Assert.That((string) servingSessionCookies[accountID.ToString()]).IsEqualTo(cookie);
        }
    }

    [Test]
    public async Task Account_IDs_To_Cookie_Still_Answers_After_The_Session_Cookie_Ends()
    {
        await InitialiseIn("Development");

        (string session, _) = await RegisterMatchServer(1);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        int accountID = await GetAccountID(NonHostAccountName);

        string gameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, cookie)))["game_cookie"];

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        // The Session Cookie Ends, As A Logout, A Login Elsewhere, Or Its Expiry Ends It, While Its Client Plays On In The Match
        await distributedCache.KeyDeleteAsync(DistributedCacheExtensions.ConstructAccountSessionCookieKey(cookie));
        await distributedCache.KeyDeleteAsync(DistributedCacheExtensions.ConstructAccountLatestSessionCookieKey(accountID));

        (IDictionary<object, object> sessionCookies, IDictionary<object, object> gameCookies) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(session, accountID));

        // The Client Still Holds The Session Cookie Which It Posted, So Answering With It Never Kicks The Client, While The Game Cookie Is Still Checked
        using (Assert.Multiple())
        {
            await Assert.That((string) sessionCookies[accountID.ToString()]).IsEqualTo(cookie);
            await Assert.That((string) gameCookies[accountID.ToString()]).IsEqualTo(gameCookie);
        }
    }

    [Test]
    public async Task Account_IDs_To_Cookie_Omits_Accounts_Without_A_Game_Cookie()
    {
        await InitialiseIn("Development");

        (string session, _) = await RegisterMatchServer(1);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        int accountID = await GetAccountID(NonHostAccountName);

        await AssertClientConnectionReturnsTheAccount(await ConnectClient(session, cookie), NonHostAccountName);

        // The Game Cookie Has Expired During A Long Match Without A Reconnect
        await webApplicationFactory.Services.GetRequiredService<IDatabase>().KeyDeleteAsync(DistributedCacheExtensions.ConstructAccountGameCookieKey(accountID));

        (IDictionary<object, object> sessionCookies, IDictionary<object, object> gameCookies) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(session, accountID));

        // The Match Server Removes A Listed Player Whose Cookie Is Empty, While It Leaves Alone An Account Which Is Not Listed
        using (Assert.Multiple())
        {
            await Assert.That(sessionCookies.Count).IsEqualTo(0);
            await Assert.That(gameCookies.Count).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Account_IDs_To_Cookie_Omits_A_Game_Cookie_Record_Without_A_Session_Cookie()
    {
        await InitialiseIn("Development");

        (string session, int serverID) = await RegisterMatchServer(1);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        int accountID = await GetAccountID(NonHostAccountName);

        string gameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, cookie)))["game_cookie"];

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        // The Record Is Valid JSON In The Shape Which The Game Cookie Had Before It Carried A Session Cookie, As A Record Stored Before An Upgrade Would Be
        await distributedCache.StringSetAsync(DistributedCacheExtensions.ConstructAccountGameCookieKey(accountID), JsonSerializer.Serialize(new { Cookie = gameCookie, ServerID = serverID }), DistributedCacheExtensions.SessionCookieLifetime);

        (IDictionary<object, object> sessionCookies, IDictionary<object, object> gameCookies) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(session, accountID));

        HttpResponseMessage reconnect = await ConnectClient(session, cookie);

        await AssertClientConnectionReturnsTheAccount(reconnect, NonHostAccountName);

        // The Match Server Leaves Alone An Account Which Is Not Listed, And The Reconnecting Client Carries The Game Cookie Which Replaces The Old Record
        using (Assert.Multiple())
        {
            await Assert.That(sessionCookies.Count).IsEqualTo(0);
            await Assert.That(gameCookies.Count).IsEqualTo(0);
            await Assert.That(await distributedCache.GetGameCookie(accountID)).IsEqualTo((string) (await DeserialisePHPResponse(reconnect))["game_cookie"]);
        }
    }

    [Test]
    public async Task Account_IDs_To_Cookie_After_A_Reconnect_Returns_The_Cookies_The_Reconnect_Used()
    {
        await InitialiseIn("Development");

        (string session, _) = await RegisterMatchServer(1);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        int accountID = await GetAccountID(NonHostAccountName);

        await AssertClientConnectionReturnsTheAccount(await ConnectClient(session, cookie), NonHostAccountName);

        // The Match Server Holds The Cookies Of Each Player's Latest Client Connection, So Answering With Those Never Removes A Reconnecting Player
        string ownServerReconnectGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, cookie)))["game_cookie"];

        (IDictionary<object, object> ownServerReconnectSessionCookies, IDictionary<object, object> ownServerReconnectGameCookies) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(session, accountID));

        // The Match Server May Be The One Whose Session The Master Server Has Lost
        string unrecognisedSessionReconnectGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(Guid.CreateVersion7().ToString(), cookie)))["game_cookie"];

        (IDictionary<object, object> unrecognisedSessionReconnectSessionCookies, IDictionary<object, object> unrecognisedSessionReconnectGameCookies) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(session, accountID));

        using (Assert.Multiple())
        {
            await Assert.That((string) ownServerReconnectSessionCookies[accountID.ToString()]).IsEqualTo(cookie);
            await Assert.That((string) ownServerReconnectGameCookies[accountID.ToString()]).IsEqualTo(ownServerReconnectGameCookie);
            await Assert.That((string) unrecognisedSessionReconnectSessionCookies[accountID.ToString()]).IsEqualTo(cookie);
            await Assert.That((string) unrecognisedSessionReconnectGameCookies[accountID.ToString()]).IsEqualTo(unrecognisedSessionReconnectGameCookie);
        }
    }

    [Test]
    public async Task Account_IDs_To_Cookie_With_A_Renewed_Session_Still_Answers()
    {
        await InitialiseIn("Development");

        (string expiredSession, int serverID) = await RegisterMatchServer(1);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        int accountID = await GetAccountID(NonHostAccountName);

        string gameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(expiredSession, cookie)))["game_cookie"];

        // The Match Server Renews Its Session While Its Match Is In Progress
        (string renewedSession, int renewedServerID) = await RegisterMatchServer(1);

        (IDictionary<object, object> sessionCookies, IDictionary<object, object> gameCookies) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(renewedSession, accountID));

        using (Assert.Multiple())
        {
            await Assert.That(renewedSession).IsNotEqualTo(expiredSession);
            await Assert.That(renewedServerID).IsEqualTo(serverID);
            await Assert.That((string) sessionCookies[accountID.ToString()]).IsEqualTo(cookie);
            await Assert.That((string) gameCookies[accountID.ToString()]).IsEqualTo(gameCookie);
        }
    }

    [Test]
    [NotInParallel(DistributedCacheExtensions.MatchLockRefusalChannel)]
    public async Task Account_IDs_To_Cookie_After_A_Refused_Connection_Elsewhere_Returns_The_Original_Game_Cookie()
    {
        await InitialiseIn("Development");

        (string lockedSession, int lockedServerID) = await RegisterMatchServer(1);
        (string otherSession, _) = await RegisterMatchServer(2);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        int accountID = await GetAccountID(NonHostAccountName);

        string lockedGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(lockedSession, cookie)))["game_cookie"];

        await LockAccount(NonHostAccountName, lockedServerID);

        // The Player Leaves Their Match In Progress And Tries To Join Another Game
        await AssertClientConnectionIsRejectedAtOnce(await ConnectClient(otherSession, cookie), NonHostAccountName);

        (IDictionary<object, object> lockedSessionCookies, IDictionary<object, object> lockedGameCookies) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(lockedSession, accountID));

        (IDictionary<object, object> otherSessionCookies, IDictionary<object, object> otherGameCookies) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(otherSession, accountID));

        // A Refused Attempt Never Makes The Locked Match Server Remove The Player, So That They Can Still Reconnect To Their Match
        using (Assert.Multiple())
        {
            await Assert.That((string) lockedSessionCookies[accountID.ToString()]).IsEqualTo(cookie);
            await Assert.That((string) lockedGameCookies[accountID.ToString()]).IsEqualTo(lockedGameCookie);
            await Assert.That(otherSessionCookies.Count).IsEqualTo(0);
            await Assert.That(otherGameCookies.Count).IsEqualTo(0);
        }
    }

    [Test]
    public async Task After_A_Lift_Joining_Another_Game_Reports_The_New_Game_Cookie_To_The_Original_Server()
    {
        await InitialiseIn("Development");

        (string originalSession, int originalServerID) = await RegisterMatchServer(1);
        (string otherSession, _) = await RegisterMatchServer(2);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        int accountID = await GetAccountID(NonHostAccountName);

        string originalGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(originalSession, cookie)))["game_cookie"];

        MatchLock matchLock = await LockAccount(NonHostAccountName, originalServerID);

        // Every Lift Removes The Lock, Whatever Lifts It
        await Assert.That(await webApplicationFactory.Services.GetRequiredService<IDatabase>().RemoveMatchLock(matchLock.UserID, matchLock.MatchID)).IsTrue();

        string otherGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(otherSession, cookie)))["game_cookie"];

        (IDictionary<object, object> sessionCookies, IDictionary<object, object> gameCookies) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(originalSession, accountID));

        // The Original Match Server Holds The Original Game Cookie, So It Removes The Player From The Match They Left, Which Lets Their Team Share Their Gold And Use Or Sell Their Items
        using (Assert.Multiple())
        {
            await Assert.That(otherGameCookie).IsNotEqualTo(originalGameCookie);
            await Assert.That((string) sessionCookies[accountID.ToString()]).IsEqualTo(cookie);
            await Assert.That((string) gameCookies[accountID.ToString()]).IsEqualTo(otherGameCookie);
        }
    }

    [Test]
    public async Task After_A_Lift_A_New_Login_Then_Joining_Another_Game_Still_Removes_The_Player()
    {
        await InitialiseIn("Development");

        (string originalSession, int originalServerID) = await RegisterMatchServer(1);
        (string otherSession, _) = await RegisterMatchServer(2);

        string firstCookie = await CreateClientSessionCookie(NonHostAccountName);

        int accountID = await GetAccountID(NonHostAccountName);

        string originalGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(originalSession, firstCookie)))["game_cookie"];

        MatchLock matchLock = await LockAccount(NonHostAccountName, originalServerID);

        await Assert.That(await webApplicationFactory.Services.GetRequiredService<IDatabase>().RemoveMatchLock(matchLock.UserID, matchLock.MatchID)).IsTrue();

        // The Player Logs In Again, On The Same Machine Or Another, Which Retires The Login With Which They Joined The Original Match
        string secondCookie = await CreateClientSessionCookie(NonHostAccountName);

        string otherGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(otherSession, secondCookie)))["game_cookie"];

        (_, IDictionary<object, object> gameCookies) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(originalSession, accountID));

        // The New Login Changes Nothing About Leaving: The Original Match Server Is Answered The New Game Cookie, Which Differs From The One Its Player Holds, So It Removes The Player From The Match They Left
        using (Assert.Multiple())
        {
            await Assert.That(otherGameCookie).IsNotEqualTo(originalGameCookie);
            await Assert.That((string) gameCookies[accountID.ToString()]).IsEqualTo(otherGameCookie);
        }
    }

    [Test]
    public async Task Account_IDs_To_Cookie_After_A_Login_Elsewhere_Still_Answers_The_Session_Cookie_Of_The_Client_On_The_Match_Server()
    {
        await InitialiseIn("Development");

        (string session, _) = await RegisterMatchServer(1);

        string firstCookie = await CreateClientSessionCookie(NonHostAccountName);

        int accountID = await GetAccountID(NonHostAccountName);

        string gameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, firstCookie)))["game_cookie"];

        // The Player Logs In On Another Machine, Which Retires The First Machine's Login But Not Its Connection To The Match
        string secondCookie = await CreateClientSessionCookie(NonHostAccountName);

        (IDictionary<object, object> sessionCookies, IDictionary<object, object> gameCookies) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(session, accountID));

        // The Match Server Holds The First Machine's Session Cookie, So Answering With It Never Kicks That Connection As A Duplicate Login, Which Would Count Toward The Engine's Kick Ban
        using (Assert.Multiple())
        {
            await Assert.That(secondCookie).IsNotEqualTo(firstCookie);
            await Assert.That(await webApplicationFactory.Services.GetRequiredService<IDatabase>().GetAccountNameForSessionCookie(firstCookie)).IsNull();
            await Assert.That((string) sessionCookies[accountID.ToString()]).IsEqualTo(firstCookie);
            await Assert.That((string) gameCookies[accountID.ToString()]).IsEqualTo(gameCookie);
        }
    }

    [Test]
    public async Task Account_IDs_To_Cookie_After_A_Reconnect_From_Another_Machine_Answers_Its_Session_Cookie()
    {
        await InitialiseIn("Development");

        (string session, _) = await RegisterMatchServer(1);

        string firstCookie = await CreateClientSessionCookie(NonHostAccountName);

        int accountID = await GetAccountID(NonHostAccountName);

        string gameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, firstCookie)))["game_cookie"];

        // The Player Logs In On Another Machine, And Reconnects From It To Their Match
        string secondCookie = await CreateClientSessionCookie(NonHostAccountName);

        string reconnectGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, secondCookie)))["game_cookie"];

        (IDictionary<object, object> sessionCookies, IDictionary<object, object> gameCookies) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(session, accountID));

        // The Match Server Now Holds The Second Machine's Session Cookie, While The Same Game Cookie Keeps The Player In Their Match
        using (Assert.Multiple())
        {
            await Assert.That(reconnectGameCookie).IsEqualTo(gameCookie);
            await Assert.That((string) sessionCookies[accountID.ToString()]).IsEqualTo(secondCookie);
            await Assert.That((string) gameCookies[accountID.ToString()]).IsEqualTo(gameCookie);
        }
    }

    [Test]
    public async Task Account_IDs_To_Cookie_After_A_Reconnect_Through_An_Unrecognised_Session_Answers_Its_Session_Cookie()
    {
        await InitialiseIn("Development");

        (string session, _) = await RegisterMatchServer(1);

        string firstCookie = await CreateClientSessionCookie(NonHostAccountName);

        int accountID = await GetAccountID(NonHostAccountName);

        string gameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, firstCookie)))["game_cookie"];

        // The Player Logs In On Another Machine, And Reconnects From It Through A Match Server Whose Session The Master Server Has Lost, Which May Be Their Own
        string secondCookie = await CreateClientSessionCookie(NonHostAccountName);

        string reconnectGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(Guid.CreateVersion7().ToString(), secondCookie)))["game_cookie"];

        (IDictionary<object, object> sessionCookies, IDictionary<object, object> gameCookies) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(session, accountID));

        using (Assert.Multiple())
        {
            await Assert.That(reconnectGameCookie).IsEqualTo(gameCookie);
            await Assert.That((string) sessionCookies[accountID.ToString()]).IsEqualTo(secondCookie);
            await Assert.That((string) gameCookies[accountID.ToString()]).IsEqualTo(gameCookie);
        }
    }

    [Test]
    public async Task Refreshing_The_Session_Cookie_For_The_Match_Server_Of_The_Game_Cookie_Is_Answered()
    {
        await InitialiseIn("Development");

        (string session, int serverID) = await RegisterMatchServer(1);

        string firstCookie = await CreateClientSessionCookie(NonHostAccountName);

        int accountID = await GetAccountID(NonHostAccountName);

        string gameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, firstCookie)))["game_cookie"];

        // The Client Logs In Again During Its Match, And Pushes Its New Session Cookie To The Match Server
        string secondCookie = await CreateClientSessionCookie(NonHostAccountName);

        bool refreshed = await webApplicationFactory.Services.GetRequiredService<IDatabase>().RefreshClientSessionCookie(accountID, serverID, secondCookie);

        (IDictionary<object, object> sessionCookies, IDictionary<object, object> gameCookies) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(session, accountID));

        using (Assert.Multiple())
        {
            await Assert.That(refreshed).IsTrue();
            await Assert.That((string) sessionCookies[accountID.ToString()]).IsEqualTo(secondCookie);
            await Assert.That((string) gameCookies[accountID.ToString()]).IsEqualTo(gameCookie);
        }
    }

    [Test]
    public async Task A_Refresh_Never_Records_Its_Session_Cookie_As_Served_While_A_Client_Connection_Does()
    {
        await InitialiseIn("Development");

        (string session, int serverID) = await RegisterMatchServer(1);

        string firstCookie = await CreateClientSessionCookie(NonHostAccountName);

        int accountID = await GetAccountID(NonHostAccountName);

        string gameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, firstCookie)))["game_cookie"];

        string secondCookie = await CreateClientSessionCookie(NonHostAccountName);

        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        await distributedCache.RefreshClientSessionCookie(accountID, serverID, secondCookie);

        GameCookie? refreshedRecord = await distributedCache.GetStoredGameCookie(accountID);

        // A Client Connection With The Session Cookie Which The Refresh Recorded Is The First To Be Served With It, And Reconnects To The Same Match With The Same Game Cookie
        string reconnectedGameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, secondCookie)))["game_cookie"];

        GameCookie? servedRecord = await distributedCache.GetStoredGameCookie(accountID);

        using (Assert.Multiple())
        {
            await Assert.That(refreshedRecord?.SessionCookie).IsEqualTo(secondCookie);
            await Assert.That(refreshedRecord?.ServedSessionCookie).IsEqualTo(firstCookie);
            await Assert.That(servedRecord?.ServedSessionCookie).IsEqualTo(secondCookie);
            await Assert.That(servedRecord?.Cookie).IsEqualTo(gameCookie);
            await Assert.That(reconnectedGameCookie).IsEqualTo(gameCookie);
        }
    }

    [Test]
    public async Task Refreshing_The_Session_Cookie_For_Another_Match_Server_Changes_Nothing()
    {
        await InitialiseIn("Development");

        (string session, _) = await RegisterMatchServer(1);
        (_, int otherServerID) = await RegisterMatchServer(2);

        string firstCookie = await CreateClientSessionCookie(NonHostAccountName);

        int accountID = await GetAccountID(NonHostAccountName);

        string gameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, firstCookie)))["game_cookie"];

        string secondCookie = await CreateClientSessionCookie(NonHostAccountName);

        // The Client With The New Session Cookie Joins Another Match Server, Which Never Served The Account, So The Match Server Which Did Still Holds The First Session Cookie
        bool refreshed = await webApplicationFactory.Services.GetRequiredService<IDatabase>().RefreshClientSessionCookie(accountID, otherServerID, secondCookie);

        (IDictionary<object, object> sessionCookies, IDictionary<object, object> gameCookies) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(session, accountID));

        using (Assert.Multiple())
        {
            await Assert.That(refreshed).IsFalse();
            await Assert.That((string) sessionCookies[accountID.ToString()]).IsEqualTo(firstCookie);
            await Assert.That((string) gameCookies[accountID.ToString()]).IsEqualTo(gameCookie);
        }
    }

    [Test]
    public async Task Refreshing_With_The_Session_Cookie_Already_Recorded_Reports_No_Refresh()
    {
        await InitialiseIn("Development");

        (string session, int serverID) = await RegisterMatchServer(1);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        int accountID = await GetAccountID(NonHostAccountName);

        string gameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, cookie)))["game_cookie"];

        // The Client Reports Joining The Match Server Which Served It With The Same Login, As Every Chat Connection During The Match Does, Which Changes Nothing And So Is Not Logged As A Refresh
        bool refreshed = await webApplicationFactory.Services.GetRequiredService<IDatabase>().RefreshClientSessionCookie(accountID, serverID, cookie);

        (IDictionary<object, object> sessionCookies, IDictionary<object, object> gameCookies) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(session, accountID));

        using (Assert.Multiple())
        {
            await Assert.That(refreshed).IsFalse();
            await Assert.That((string) sessionCookies[accountID.ToString()]).IsEqualTo(cookie);
            await Assert.That((string) gameCookies[accountID.ToString()]).IsEqualTo(gameCookie);
        }
    }

    [Test]
    public async Task Account_IDs_To_Cookie_Ignores_Account_IDs_In_The_Query_String()
    {
        await InitialiseIn("Development");

        (string session, _) = await RegisterMatchServer(1);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        await AssertClientConnectionReturnsTheAccount(await ConnectClient(session, cookie), NonHostAccountName);

        HttpResponseMessage response = await RequestAccountCookies(session, await GetAccountID(NonHostAccountName));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        (IDictionary<object, object> sessionCookies, IDictionary<object, object> gameCookies) = await ReadAccountCookies(response);

        using (Assert.Multiple())
        {
            await Assert.That(sessionCookies.Count).IsEqualTo(0);
            await Assert.That(gameCookies.Count).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Account_IDs_To_Cookie_Ignores_Duplicate_Non_Positive_And_Non_Integer_Account_IDs()
    {
        await InitialiseIn("Development");

        (string session, _) = await RegisterMatchServer(1);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        int accountID = await GetAccountID(NonHostAccountName);

        string gameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, cookie)))["game_cookie"];

        HttpResponseMessage response = await RequestAccountCookiesInTheRequestBody(session, "ACCOUNT", "1.5", string.Empty, "0", "-1", "12345678901", accountID.ToString(), accountID.ToString());

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        (IDictionary<object, object> sessionCookies, IDictionary<object, object> gameCookies) = await ReadAccountCookies(response);

        using (Assert.Multiple())
        {
            await Assert.That(sessionCookies.Count).IsEqualTo(1);
            await Assert.That(gameCookies.Count).IsEqualTo(1);
            await Assert.That((string) sessionCookies[accountID.ToString()]).IsEqualTo(cookie);
            await Assert.That((string) gameCookies[accountID.ToString()]).IsEqualTo(gameCookie);
        }
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
    ///     Asserts that the client connection reply rejects the client at once, which is a reply that names the account and carries nothing else, so that the match server disconnects the client without retrying or admitting it.
    ///     The reply carries no "game_cookie", no "auth_failure", which would make the match server drop its session, or end its match if the match has not started yet, and no "cookie" set to "Invalid cookie.", after which the match server refuses the client's session cookie for as long as it runs.
    /// </summary>
    private async Task AssertClientConnectionIsRejectedAtOnce(HttpResponseMessage response, string accountName)
    {
        IDictionary<object, object> fields = await DeserialisePHPResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
            await Assert.That(fields.Count).IsEqualTo(1);
            await Assert.That(Convert.ToInt32(fields["account_id"])).IsEqualTo(await GetAccountID(accountName));
        }
    }

    /// <summary>
    ///     Reads the session cookies ("aids2cookie") and the game cookies ("aids2gcookie") of a reply to the duplicate session check, each keyed by account ID, in the order of the reply.
    ///     An empty list is deserialised as an empty sequential array rather than as a dictionary, so it is read as an empty dictionary.
    /// </summary>
    private static async Task<(IDictionary<object, object> SessionCookies, IDictionary<object, object> GameCookies)> ReadAccountCookies(HttpResponseMessage response)
    {
        IDictionary<object, object> body = await DeserialisePHPResponse(response);

        return (ReadAccountCookieList(body["aids2cookie"]), ReadAccountCookieList(body["aids2gcookie"]));

        static IDictionary<object, object> ReadAccountCookieList(object list)
            => list is ICollection { Count: 0 } ? new Dictionary<object, object>() : (IDictionary<object, object>) list;
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
