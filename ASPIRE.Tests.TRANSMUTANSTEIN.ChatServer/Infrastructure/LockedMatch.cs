namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

/// <summary>
///     A match lock seeded by <see cref="ChatTestData.SeedLockedMatch"/>, with its match server's chat connection when the match server has one.
///     Disposing it removes the lock and its match server, and closes the connection, because every chat server host shares the unprefixed distributed cache while each host's database restarts its identifiers, so a lock left behind would bind a user of a later test.
///     A match server left behind would be reaped by a later host's stale host reaper, which lifts the locks on it.
/// </summary>
internal sealed class LockedMatch(ChatServerHost host, MatchLock matchLock, TcpClient? matchServerConnection) : IAsyncDisposable
{
    public MatchLock Lock { get; } = matchLock;

    public TcpClient? MatchServerConnection { get; } = matchServerConnection;

    public async ValueTask DisposeAsync()
    {
        await using (AsyncServiceScope scope = host.Services.CreateAsyncScope())
        {
            IDatabase distributedCacheStore = scope.ServiceProvider.GetRequiredService<IDatabase>();

            await distributedCacheStore.RemoveMatchLock(Lock.UserID, Lock.MatchID);

            // The Match Server Is Removed As The Chat Server Removes A Match Server Which Has Gone, Which Also Lifts Any Other Lock On It And Clears Its Index
            await distributedCacheStore.RemoveMatchServerByID(Lock.ServerID);
        }

        MatchServerConnection?.Dispose();
    }
}
