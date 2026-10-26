namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.ServerManagement;

public sealed partial class HostingAuthenticationTests
{
    [Test]
    [NotInParallel(DistributedCacheExtensions.MatchLockRefusalChannel)]
    [Arguments("10")]
    [Arguments("90")]
    public async Task The_Server_List_Of_A_Locked_Account_Is_Empty_And_Relays_A_Refusal(string gameType)
    {
        await InitialiseIn("Development");

        (_, int lockedServerID) = await RegisterMatchServer(1);
        (_, int activeServerID) = await RegisterMatchServer(2);
        (_, int idleServerID) = await RegisterMatchServer(3);

        // One Match Server Is Active And Another Is Idle, So That Neither Kind Of List Would Be Empty Without The Lock
        await SetMatchServerStatus(activeServerID, ChatProtocol.ServerStatus.SERVER_STATUS_ACTIVE);
        await SetMatchServerStatus(idleServerID, ChatProtocol.ServerStatus.SERVER_STATUS_IDLE);

        MatchLock matchLock = await LockAccount(NonHostAccountName, lockedServerID);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        ChannelMessageQueue refusals = await SubscribeToRefusals();

        try
        {
            HttpResponseMessage response = await RequestServerList(cookie, gameType);

            IDictionary<object, object> body = await DeserialisePHPResponse(response);

            MatchLockRefusal? refusal = await ReadRefusal(refusals, RefusalTimeout);

            using (Assert.Multiple())
            {
                await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
                await Assert.That(body["server_list"] is ICollection { Count: 0 }).IsTrue();
                await Assert.That(body.ContainsKey("acc_key")).IsTrue();
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
    [Arguments("10")]
    [Arguments("90")]
    public async Task The_Server_List_Of_An_Unlocked_Account_Lists_The_Servers(string gameType)
    {
        await InitialiseIn("Development");

        await RegisterMatchServer(1);
        (_, int activeServerID) = await RegisterMatchServer(2);
        (_, int idleServerID) = await RegisterMatchServer(3);

        await SetMatchServerStatus(activeServerID, ChatProtocol.ServerStatus.SERVER_STATUS_ACTIVE);
        await SetMatchServerStatus(idleServerID, ChatProtocol.ServerStatus.SERVER_STATUS_IDLE);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        ChannelMessageQueue refusals = await SubscribeToRefusals();

        try
        {
            HttpResponseMessage response = await RequestServerList(cookie, gameType);

            IDictionary<object, object> body = await DeserialisePHPResponse(response);

            MatchLockRefusal? refusal = await ReadRefusal(refusals, RefusalQuietPeriod);

            using (Assert.Multiple())
            {
                await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
                await Assert.That(body["server_list"] is ICollection { Count: > 0 }).IsTrue();
                await Assert.That(refusal).IsNull();
            }
        }

        finally
        {
            await refusals.UnsubscribeAsync();
        }
    }

    [Test]
    [NotInParallel(DistributedCacheExtensions.MatchLockRefusalChannel)]
    public async Task The_Server_List_Of_Another_Account_Of_A_Locked_User_Is_Empty()
    {
        await InitialiseIn("Development");

        await CreateSubAccount(NonHostAccountName, NonHostSubAccountName);

        (_, int lockedServerID) = await RegisterMatchServer(1);
        (_, int idleServerID) = await RegisterMatchServer(2);

        await SetMatchServerStatus(idleServerID, ChatProtocol.ServerStatus.SERVER_STATUS_IDLE);

        MatchLock matchLock = await LockAccount(NonHostAccountName, lockedServerID);

        string cookie = await CreateClientSessionCookie(NonHostSubAccountName);

        ChannelMessageQueue refusals = await SubscribeToRefusals();

        try
        {
            HttpResponseMessage response = await RequestServerList(cookie, "90");

            IDictionary<object, object> body = await DeserialisePHPResponse(response);

            MatchLockRefusal? refusal = await ReadRefusal(refusals, RefusalTimeout);

            using (Assert.Multiple())
            {
                await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
                await Assert.That(body["server_list"] is ICollection { Count: 0 }).IsTrue();
                await Assert.That(refusal?.UserID).IsEqualTo(matchLock.UserID);
                await Assert.That(refusal?.AccountName).IsEqualTo(NonHostSubAccountName);
            }
        }

        finally
        {
            await refusals.UnsubscribeAsync();
        }
    }

    [Test]
    [Arguments(AccountType.Guest)]
    [Arguments(AccountType.Staff)]
    public async Task The_Server_List_Is_Never_Refused_For_Guest_And_Staff_Accounts(AccountType accountType)
    {
        await InitialiseIn("Development");

        string accountName = $"{accountType.ToString().ToUpperInvariant()}-ACCOUNT";

        await EnsureAccountExists(accountName, $"{accountName.ToLowerInvariant()}@kongor.test", accountType);

        (_, int lockedServerID) = await RegisterMatchServer(1);
        (_, int idleServerID) = await RegisterMatchServer(2);

        await SetMatchServerStatus(idleServerID, ChatProtocol.ServerStatus.SERVER_STATUS_IDLE);

        // The Chat Server Never Locks These Accounts, Because Each Type Shares One User, So This Lock Stands In For One Left Behind By Any Other Means
        await LockAccount(accountName, lockedServerID);

        string cookie = await CreateClientSessionCookie(accountName);

        HttpResponseMessage response = await RequestServerList(cookie, "90");

        IDictionary<object, object> body = await DeserialisePHPResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(body["server_list"] is ICollection { Count: > 0 }).IsTrue();
        }
    }

    [Test]
    public async Task The_Server_List_Is_Served_And_The_Lock_Lifted_When_The_Locked_Server_Is_Gone()
    {
        await InitialiseIn("Development");

        (_, int serverID) = await RegisterMatchServer(1);

        await SetMatchServerStatus(serverID, ChatProtocol.ServerStatus.SERVER_STATUS_IDLE);

        // No Other Match Server Is Registered, So The Lock Names A Match Server Which Is Gone
        MatchLock matchLock = await LockAccount(NonHostAccountName, serverID + 1);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        HttpResponseMessage response = await RequestServerList(cookie, "90");

        IDictionary<object, object> body = await DeserialisePHPResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(body["server_list"] is ICollection { Count: > 0 }).IsTrue();
            await Assert.That(await webApplicationFactory.Services.GetRequiredService<IDatabase>().GetMatchLock(matchLock.UserID)).IsNull();
        }
    }

    [Test]
    public async Task The_Server_List_Is_Served_When_The_Match_Lock_Cannot_Be_Read()
    {
        await InitialiseIn("Development");

        (_, int lockedServerID) = await RegisterMatchServer(1);
        (_, int idleServerID) = await RegisterMatchServer(2);

        await SetMatchServerStatus(idleServerID, ChatProtocol.ServerStatus.SERVER_STATUS_IDLE);

        MatchLock matchLock = await LockAccount(NonHostAccountName, lockedServerID);

        // The Stored Lock No Longer Deserialises, As It Would After A Change To The Shape Of The Lock
        await webApplicationFactory.Services.GetRequiredService<IDatabase>().StringSetAsync(DistributedCacheExtensions.ConstructMatchLockKey(matchLock.UserID), "NOT-A-MATCH-LOCK", MatchLock.Lifetime);

        string cookie = await CreateClientSessionCookie(NonHostAccountName);

        HttpResponseMessage response = await RequestServerList(cookie, "90");

        IDictionary<object, object> body = await DeserialisePHPResponse(response);

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(body["server_list"] is ICollection { Count: > 0 }).IsTrue();
        }
    }

    [Test]
    [NotInParallel(DistributedCacheExtensions.MatchLockRefusalChannel)]
    public async Task The_Server_List_Is_Served_When_The_Account_Of_The_Session_Cookie_Cannot_Be_Found()
    {
        await InitialiseIn("Development");

        (_, int idleServerID) = await RegisterMatchServer(1);

        await SetMatchServerStatus(idleServerID, ChatProtocol.ServerStatus.SERVER_STATUS_IDLE);

        string cookie = Guid.CreateVersion7().ToString();

        // The Session Cookie Passes The Validation Of Every Client Request, But Names An Account Which Does Not Exist, So That No Match Lock Can Be Looked Up For It
        await webApplicationFactory.Services.GetRequiredService<IDatabase>().SetAccountNameForSessionCookie(cookie, "MISSING-ACCOUNT");

        ChannelMessageQueue refusals = await SubscribeToRefusals();

        try
        {
            HttpResponseMessage response = await RequestServerList(cookie, "90");

            IDictionary<object, object> body = await DeserialisePHPResponse(response);

            MatchLockRefusal? refusal = await ReadRefusal(refusals, RefusalQuietPeriod);

            using (Assert.Multiple())
            {
                await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
                await Assert.That(body["server_list"] is ICollection { Count: > 0 }).IsTrue();
                await Assert.That(refusal).IsNull();
            }
        }

        finally
        {
            await refusals.UnsubscribeAsync();
        }
    }

    /// <summary>
    ///     Sends the request which the game client sends, with its login cookie, for the list of match servers of a game type ("CServerList::RequestMasterServerList").
    /// </summary>
    private async Task<HttpResponseMessage> RequestServerList(string cookie, string gameType)
    {
        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"S2 Games/Heroes Of Newerth/{Distributions.Version}/wac/x86_64");

        Dictionary<string, string> formData = new ()
        {
            { "cookie", cookie },
            { "gametype", gameType }
        };

        return await httpClient.PostAsync("client_requester.php?f=server_list", new FormUrlEncodedContent(formData));
    }

    /// <summary>
    ///     Rewrites the status of a registered match server, as its heartbeat does, which decides the kind of server list that lists it.
    /// </summary>
    private async Task SetMatchServerStatus(int serverID, ChatProtocol.ServerStatus status)
    {
        IDatabase distributedCache = webApplicationFactory.Services.GetRequiredService<IDatabase>();

        MatchServer matchServer = await distributedCache.GetMatchServerByID(serverID)
            ?? throw new InvalidOperationException($@"Match Server ID ""{serverID}"" Is Not Registered");

        matchServer.Status = status;

        await distributedCache.SetMatchServer(matchServer.HostAccountName, matchServer);
    }
}
