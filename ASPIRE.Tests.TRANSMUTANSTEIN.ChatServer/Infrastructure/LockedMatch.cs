namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

/// <summary>
///     A match lock seeded by <see cref="ChatTestData.SeedLockedMatch"/>, with its match server's chat connection when the match server has one.
///     Disposing it removes the lock and closes the connection, because every chat server host shares the unprefixed distributed cache while each host's database restarts its identifiers, so a lock left behind would bind a user of a later test.
/// </summary>
internal sealed record LockedMatch(ChatServerHost Host, MatchLock Lock, TcpClient? MatchServerConnection) : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        await using (AsyncServiceScope scope = Host.Services.CreateAsyncScope())
        {
            IDatabase distributedCacheStore = scope.ServiceProvider.GetRequiredService<IDatabase>();

            await distributedCacheStore.RemoveMatchLock(Lock.UserID, Lock.MatchID);
        }

        MatchServerConnection?.Dispose();
    }
}
