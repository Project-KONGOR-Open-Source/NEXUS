namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

/// <summary>
///     Seeds the database and distributed cache with the minimal data the chat handshakes require.
/// </summary>
internal static class ChatTestData
{
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
    /// </summary>
    public static async Task SeedMatchServer(IServiceProvider services, int serverID, int hostAccountID, string hostAccountName, string cookie)
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
            IPAddress = "127.0.0.1",
            Port = 11235,
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
}
