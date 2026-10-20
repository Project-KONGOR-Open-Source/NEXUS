namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

/// <summary>
///     Seeds the database and distributed cache with the minimal data the chat handshakes require.
/// </summary>
internal static class ChatTestData
{
    /// <summary>
    ///     The last match server ID which <see cref="NextSeededServerID"/> issued, which starts high, above the range from which most other tests pick their match server IDs at random.
    /// </summary>
    private static int LastSeededServerID = 2_000_000_000;

    /// <summary>
    ///     Issues a match server ID which no other match server seeded through it during the test run shares, because every chat server host shares the unprefixed distributed cache, where a match server left behind by one test would otherwise be mistaken for another test's.
    /// </summary>
    public static int NextSeededServerID() => Interlocked.Increment(ref LastSeededServerID);

    /// <summary>
    ///     Inserts a user and a single account of the given type, returning the generated account identifier and name.
    ///     The SRP and password fields are placeholders, because the chat handshakes validate the session cookie and authentication hash rather than the password.
    /// </summary>
    public static async Task<(int AccountID, string AccountName)> SeedAccount(IServiceProvider services, AccountType accountType)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Role role = await databaseContext.Roles.SingleAsync(candidate => candidate.Name.Equals(UserRoles.User));

        // Version 7 GUIDs Are Time-Ordered, So Their Leading Characters Are A Shared Millisecond Timestamp That Collides For Accounts Seeded Close Together; The Trailing Characters Are Random And Therefore Unique
        string suffix = Guid.CreateVersion7().ToString("N")[^8..];

        User user = new ()
        {
            EmailAddress = $"{suffix}@test.local",
            Role = role,
            SRPPasswordSalt = new string('0', 64),
            SRPPasswordHash = new string('0', 64),
            PBKDF2PasswordHash = new string('0', 84)
        };

        Account account = new ()
        {
            Name = $"TEST:{suffix}",
            User = user,
            IsMain = true,
            Type = accountType
        };

        user.Accounts.Add(account);

        await databaseContext.Users.AddAsync(user);
        await databaseContext.SaveChangesAsync();

        return (account.ID, account.Name);
    }

    /// <summary>
    ///     Inserts an account of the given type, which is not the main account, into the user of the given main account, returning the generated account identifier and name.
    /// </summary>
    public static async Task<(int AccountID, string AccountName)> SeedSubAccount(IServiceProvider services, int mainAccountID, AccountType accountType = AccountType.Normal)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        User user = await databaseContext.Users.SingleAsync(candidate => candidate.Accounts.Any(account => account.ID == mainAccountID));

        // The Random Trailing Characters Of A Version 7 GUID Keep The Name Unique, As In "SeedAccount"
        string suffix = Guid.CreateVersion7().ToString("N")[^8..];

        Account account = new ()
        {
            Name = $"TEST:{suffix}",
            User = user,
            IsMain = false,
            Type = accountType
        };

        await databaseContext.Accounts.AddAsync(account);
        await databaseContext.SaveChangesAsync();

        return (account.ID, account.Name);
    }

    /// <summary>
    ///     Seeds an account of the given type, with a fresh session cookie for its login, and returns the identifier of its user with the account's identifier, name, and session cookie.
    /// </summary>
    public static async Task<(int UserID, int AccountID, string AccountName, string Cookie)> SeedPlayer(ChatServerHost host, AccountType accountType = AccountType.Normal)
    {
        (int accountID, string accountName) = await SeedAccount(host.Services, accountType);

        string cookie = Guid.CreateVersion7().ToString();

        await SeedClientSessionCookie(host.Services, cookie, accountName);

        return (await GetUserID(host.Services, accountID), accountID, accountName, cookie);
    }

    /// <summary>
    ///     Reads the identifier of the user who owns the given account.
    /// </summary>
    public static async Task<int> GetUserID(IServiceProvider services, int accountID)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        return await databaseContext.Accounts.Where(account => account.ID == accountID).Select(account => account.User.ID).SingleAsync();
    }

    /// <summary>
    ///     Seeds the client session cookie to account name mapping that the client handshake validates.
    /// </summary>
    public static async Task SeedClientSessionCookie(IServiceProvider services, string cookie, string accountName)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();

        IDatabase distributedCacheStore = scope.ServiceProvider.GetRequiredService<IDatabase>();

        await distributedCacheStore.SetAccountNameForSessionCookie(cookie, accountName);
    }

    /// <summary>
    ///     Seeds a match server in the distributed cache so that a match server handshake with the matching cookie and identifier succeeds.
    ///     The address and port are those at which the match server registers, which tests of the reconnect query point at a fake game socket.
    /// </summary>
    public static async Task SeedMatchServer(IServiceProvider services, int serverID, int hostAccountID, string hostAccountName, string cookie, int port = 11235, string ipAddress = "127.0.0.1")
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();

        IDatabase distributedCacheStore = scope.ServiceProvider.GetRequiredService<IDatabase>();

        MatchServer matchServer = new ()
        {
            ID = serverID,
            HostAccountID = hostAccountID,
            HostAccountName = hostAccountName,
            Name = $"server-{serverID}",
            MatchServerManagerID = null,
            Instance = 1,
            IPAddress = ipAddress,
            Port = port,
            Location = "USE",
            Description = "Integration Test Server",
            Cookie = cookie
        };

        await distributedCacheStore.SetMatchServer(hostAccountName, matchServer);
    }

    /// <summary>
    ///     Records a mutual friendship between two seeded accounts so that each appears in the other's friend list when the client handshake loads the account.
    ///     The friendship is stored as an entry in each account's owned <see cref="Account.FriendedPeers"/> collection, mirroring how the client adds a friend.
    /// </summary>
    public static async Task SeedFriendship(IServiceProvider services, (int ID, string Name) firstAccount, (int ID, string Name) secondAccount)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account first = await databaseContext.Accounts.SingleAsync(account => account.ID == firstAccount.ID);
        Account second = await databaseContext.Accounts.SingleAsync(account => account.ID == secondAccount.ID);

        first.FriendedPeers.Add(new FriendedPeer { ID = secondAccount.ID, Name = secondAccount.Name, ClanTag = null, FriendGroup = "Friends" });
        second.FriendedPeers.Add(new FriendedPeer { ID = firstAccount.ID, Name = firstAccount.Name, ClanTag = null, FriendGroup = "Friends" });

        await databaseContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Locks a user to a match in progress on a seeded match server, whose game socket is the given fake and whose registered address is the given one.
    ///     With a chat session, the match server completes its handshake and reports the locked match in the given game phase, as a live match server does.
    ///     Without one, the match server stays registered with no chat session, as it is while the stale host reaper waits out its grace period.
    /// </summary>
    public static async Task<LockedMatch> SeedLockedMatch(ChatServerHost host, int userID, int accountID, string accountName, FakeMatchServerGameSocket gameSocket,
        GamePhase gamePhase = GamePhase.GAME_PHASE_ACTIVE, string registeredAddress = "127.0.0.1", bool withChatSession = true)
    {
        (int hostAccountID, string hostAccountName) = await SeedAccount(host.Services, AccountType.ServerHost);

        int serverID = NextSeededServerID();
        int matchID = Random.Shared.Next(100_000, 999_999);
        string cookie = Guid.CreateVersion7().ToString();

        await SeedMatchServer(host.Services, serverID, hostAccountID, hostAccountName, cookie, gameSocket.Port, registeredAddress);

        TcpClient? matchServerConnection = null;

        if (withChatSession)
        {
            matchServerConnection = await host.ConnectAndAuthenticateMatchServerAsync(serverID, cookie);

            MatchServerChatSessionMetadata metadata = Context.MatchServerChatSessions[serverID].Metadata;

            metadata.Status = ChatProtocol.ServerStatus.SERVER_STATUS_ACTIVE;
            metadata.GamePhase = (int) gamePhase;
            metadata.MatchID = matchID;
        }

        MatchLock matchLock = new ()
        {
            UserID = userID,
            AccountID = accountID,
            AccountName = accountName,
            MatchID = matchID,
            CorrelationID = Random.Shared.Next(),
            ArrangedMatchType = MatchType.AM_MATCHMAKING,
            ServerID = serverID,
            ServerAddress = registeredAddress,
            ServerPort = (ushort) gameSocket.Port,
            TimestampStarted = DateTimeOffset.UtcNow
        };

        await using AsyncServiceScope scope = host.Services.CreateAsyncScope();

        IDatabase distributedCacheStore = scope.ServiceProvider.GetRequiredService<IDatabase>();

        await distributedCacheStore.SetMatchLock(matchLock);

        return new LockedMatch(host, matchLock, matchServerConnection);
    }

    /// <summary>
    ///     Polls the distributed cache until the user's match lock is gone or the timeout elapses, returning whether it disappeared in time.
    /// </summary>
    public static Task<bool> WaitUntilLockIsGone(ChatServerHost host, int userID, TimeSpan timeout)
        => ChatTestProtocol.WaitUntil(async () => await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID) is null, timeout);
}
