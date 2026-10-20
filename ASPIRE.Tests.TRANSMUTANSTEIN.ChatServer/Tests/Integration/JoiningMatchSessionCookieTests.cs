namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Integration;

/// <summary>
///     Tests that a client which reports joining a match server refreshes the session cookie which the master server answers to that match server's duplicate session check, because a client which logs in again during its match pushes its new session cookie to the match server.
///     Only the records which the match server's client connections left are refreshed, for the session's own account and for every other account of the same user, though never for another account of a guest or staff account, and their game cookies are never changed.
///     Every test removes the match servers and the game cookies which it seeds, because every chat server host shares the unprefixed distributed cache while each host's database restarts its identifiers, so a game cookie left behind would be read as that of a later test's account.
/// </summary>
[NotInParallel(nameof(ChatServerHost))]
public sealed class JoiningMatchSessionCookieTests(ServiceContainerContext containerContext)
{
    private const string RemoteIP = "127.0.0.1";

    /// <summary>
    ///     The IP address of the match servers which these tests seed, which no other test registers, so that each address resolves to one match server alone.
    /// </summary>
    private const string ServerIPAddress = "127.0.0.3";

    /// <summary>
    ///     How long a refresh, or the report of joining a match server, is awaited.
    /// </summary>
    private static readonly TimeSpan JoinTimeout = TimeSpan.FromSeconds(10);

    [Test]
    public async Task Joining_A_Match_Server_With_A_New_Session_Cookie_Refreshes_The_Recorded_Session_Cookie()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        IDatabase distributedCache = host.Services.GetRequiredService<IDatabase>();

        (_, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        (int serverID, string serverAddress) = await SeedMatchServer(host);

        try
        {
            // The Match Server Served The Client With An Earlier Login, Before The Client Logged In Again In Place
            string gameCookie = await distributedCache.IssueGameCookie(accountID, serverID, Guid.CreateVersion7().ToString());

            using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

            await ChatTestProtocol.WriteFrame(client.GetStream(), ChatTestProtocol.BuildJoiningGame(serverAddress));

            bool refreshed = await ChatTestProtocol.WaitUntil(async () => (await distributedCache.GetStoredGameCookie(accountID))?.SessionCookie == cookie, JoinTimeout);

            GameCookie? record = await distributedCache.GetStoredGameCookie(accountID);

            using (Assert.Multiple())
            {
                await Assert.That(refreshed).IsTrue();
                await Assert.That(record?.Cookie).IsEqualTo(gameCookie);
                await Assert.That(record?.ServerID).IsEqualTo(serverID);
            }
        }

        finally
        {
            await RemoveGameCookies(distributedCache, accountID);
            await distributedCache.RemoveMatchServerByID(serverID);
        }
    }

    [Test]
    public async Task Joining_Another_Match_Server_Leaves_The_Recorded_Session_Cookie_Alone()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        IDatabase distributedCache = host.Services.GetRequiredService<IDatabase>();

        (_, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        (int serverID, _) = await SeedMatchServer(host);
        (int otherServerID, string otherServerAddress) = await SeedMatchServer(host);

        try
        {
            string earlierCookie = Guid.CreateVersion7().ToString();

            string gameCookie = await distributedCache.IssueGameCookie(accountID, serverID, earlierCookie);

            using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

            ClientChatSession session = Context.ClientChatSessions[accountName];

            await ChatTestProtocol.WriteFrame(client.GetStream(), ChatTestProtocol.BuildJoiningGame(otherServerAddress));

            // The Session Names The Match Server Only Once It Has Tried To Refresh The Recorded Session Cookie
            bool joined = await ChatTestProtocol.WaitUntil(() => session.Metadata.MatchServerConnectedTo?.ID == otherServerID, JoinTimeout);

            GameCookie? record = await distributedCache.GetStoredGameCookie(accountID);

            using (Assert.Multiple())
            {
                await Assert.That(joined).IsTrue();
                await Assert.That(record?.SessionCookie).IsEqualTo(earlierCookie);
                await Assert.That(record?.Cookie).IsEqualTo(gameCookie);
                await Assert.That(record?.ServerID).IsEqualTo(serverID);
            }
        }

        finally
        {
            await RemoveGameCookies(distributedCache, accountID);
            await distributedCache.RemoveMatchServerByID(serverID);
            await distributedCache.RemoveMatchServerByID(otherServerID);
        }
    }

    [Test]
    public async Task Joining_A_Match_Server_As_Another_Account_Of_The_User_Refreshes_The_Account_Served_There()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        IDatabase distributedCache = host.Services.GetRequiredService<IDatabase>();

        (_, int mainAccountID, _, _) = await ChatTestData.SeedPlayer(host);

        (int subAccountID, string subAccountName) = await ChatTestData.SeedSubAccount(host.Services, mainAccountID);

        string subAccountCookie = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedClientSessionCookie(host.Services, subAccountCookie, subAccountName);

        (int serverID, string serverAddress) = await SeedMatchServer(host);

        try
        {
            // The Match Server Served The Main Account, Before The Client Logged In Again In Place With Another Account Of The Same User
            string gameCookie = await distributedCache.IssueGameCookie(mainAccountID, serverID, Guid.CreateVersion7().ToString());

            using TcpClient client = await host.ConnectAndAuthenticateClientAsync(subAccountID, subAccountName, subAccountCookie, RemoteIP);

            await ChatTestProtocol.WriteFrame(client.GetStream(), ChatTestProtocol.BuildJoiningGame(serverAddress));

            bool refreshed = await ChatTestProtocol.WaitUntil(async () => (await distributedCache.GetStoredGameCookie(mainAccountID))?.SessionCookie == subAccountCookie, JoinTimeout);

            GameCookie? mainAccountRecord = await distributedCache.GetStoredGameCookie(mainAccountID);
            GameCookie? subAccountRecord = await distributedCache.GetStoredGameCookie(subAccountID);

            // The Main Account Keeps Its Own Game Cookie, And The Account Which The Match Server Never Served Is Given No Record
            using (Assert.Multiple())
            {
                await Assert.That(refreshed).IsTrue();
                await Assert.That(mainAccountRecord?.Cookie).IsEqualTo(gameCookie);
                await Assert.That(mainAccountRecord?.ServerID).IsEqualTo(serverID);
                await Assert.That(subAccountRecord).IsNull();
            }
        }

        finally
        {
            await RemoveGameCookies(distributedCache, mainAccountID, subAccountID);
            await distributedCache.RemoveMatchServerByID(serverID);
        }
    }

    [Test]
    public async Task A_Stale_Record_Of_The_Session_Account_Does_Not_Stop_The_Account_In_The_Match_Being_Refreshed()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        IDatabase distributedCache = host.Services.GetRequiredService<IDatabase>();

        (_, int sessionAccountID, string sessionAccountName, string sessionAccountCookie) = await ChatTestData.SeedPlayer(host);

        (int matchAccountID, _) = await ChatTestData.SeedSubAccount(host.Services, sessionAccountID);

        (int serverID, string serverAddress) = await SeedMatchServer(host);

        try
        {
            // The Session's Account Played An Earlier Match On The Same Match Server, Where Another Account Of The Same User Now Plays The Match In Progress
            await distributedCache.IssueGameCookie(sessionAccountID, serverID, Guid.CreateVersion7().ToString());

            string gameCookie = await distributedCache.IssueGameCookie(matchAccountID, serverID, Guid.CreateVersion7().ToString());

            // The Client In The Match Logged In Again In Place With The Session's Account
            using TcpClient client = await host.ConnectAndAuthenticateClientAsync(sessionAccountID, sessionAccountName, sessionAccountCookie, RemoteIP);

            ClientChatSession session = Context.ClientChatSessions[sessionAccountName];

            await ChatTestProtocol.WriteFrame(client.GetStream(), ChatTestProtocol.BuildJoiningGame(serverAddress));

            // The Session Names The Match Server Only Once It Has Tried To Refresh The Recorded Session Cookies
            bool joined = await ChatTestProtocol.WaitUntil(() => session.Metadata.MatchServerConnectedTo?.ID == serverID, JoinTimeout);

            GameCookie? matchAccountRecord = await distributedCache.GetStoredGameCookie(matchAccountID);

            using (Assert.Multiple())
            {
                await Assert.That(joined).IsTrue();
                await Assert.That(matchAccountRecord?.SessionCookie).IsEqualTo(sessionAccountCookie);
                await Assert.That(matchAccountRecord?.Cookie).IsEqualTo(gameCookie);
            }
        }

        finally
        {
            await RemoveGameCookies(distributedCache, sessionAccountID, matchAccountID);
            await distributedCache.RemoveMatchServerByID(serverID);
        }
    }

    [Test]
    [Arguments(AccountType.Guest)]
    [Arguments(AccountType.Staff)]
    public async Task Joining_A_Match_Server_As_A_Guest_Or_Staff_Account_Never_Refreshes_Another_Account(AccountType accountType)
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        IDatabase distributedCache = host.Services.GetRequiredService<IDatabase>();

        (_, int mainAccountID, _, _) = await ChatTestData.SeedPlayer(host, accountType);

        (int subAccountID, string subAccountName) = await ChatTestData.SeedSubAccount(host.Services, mainAccountID, accountType);

        string subAccountCookie = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedClientSessionCookie(host.Services, subAccountCookie, subAccountName);

        (int serverID, string serverAddress) = await SeedMatchServer(host);

        try
        {
            string earlierCookie = Guid.CreateVersion7().ToString();

            string gameCookie = await distributedCache.IssueGameCookie(mainAccountID, serverID, earlierCookie);

            using TcpClient client = await host.ConnectAndAuthenticateClientAsync(subAccountID, subAccountName, subAccountCookie, RemoteIP);

            ClientChatSession session = Context.ClientChatSessions[subAccountName];

            await ChatTestProtocol.WriteFrame(client.GetStream(), ChatTestProtocol.BuildJoiningGame(serverAddress));

            // The Session Names The Match Server Only Once It Has Tried To Refresh The Recorded Session Cookie
            bool joined = await ChatTestProtocol.WaitUntil(() => session.Metadata.MatchServerConnectedTo?.ID == serverID, JoinTimeout);

            GameCookie? record = await distributedCache.GetStoredGameCookie(mainAccountID);

            // Every Guest Account Shares One User, As Does Every Staff Account, So Another Account Of The Same User Belongs To Another Player
            using (Assert.Multiple())
            {
                await Assert.That(joined).IsTrue();
                await Assert.That(record?.SessionCookie).IsEqualTo(earlierCookie);
                await Assert.That(record?.Cookie).IsEqualTo(gameCookie);
            }
        }

        finally
        {
            await RemoveGameCookies(distributedCache, mainAccountID, subAccountID);
            await distributedCache.RemoveMatchServerByID(serverID);
        }
    }

    /// <summary>
    ///     Seeds a match server without a chat session, at a port which no other match server seeded during the test run shares, and returns its server ID with the address at which a client reports joining it.
    /// </summary>
    private static async Task<(int ServerID, string ServerAddress)> SeedMatchServer(ChatServerHost host)
    {
        (int hostAccountID, string hostAccountName) = await ChatTestData.SeedAccount(host.Services, AccountType.ServerHost);

        int serverID = ChatTestData.NextSeededServerID();

        // Seeded Server IDs Are Consecutive, So Each One Maps To Its Own Port
        int serverPort = 20_000 + serverID % 10_000;

        await ChatTestData.SeedMatchServer(host.Services, serverID, hostAccountID, hostAccountName, Guid.CreateVersion7().ToString(), serverPort, ServerIPAddress);

        return (serverID, $"{ServerIPAddress}:{serverPort}");
    }

    /// <summary>
    ///     Deletes the game cookies of the given accounts.
    /// </summary>
    private static async Task RemoveGameCookies(IDatabase distributedCache, params int[] accountIDs)
    {
        foreach (int accountID in accountIDs)
            await distributedCache.KeyDeleteAsync(DistributedCacheExtensions.ConstructAccountGameCookieKey(accountID));
    }
}
