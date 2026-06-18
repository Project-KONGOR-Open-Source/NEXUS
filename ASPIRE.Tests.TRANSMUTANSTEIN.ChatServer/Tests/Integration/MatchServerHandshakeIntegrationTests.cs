namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Integration;

/// <summary>
///     Drives the match server handshake against the real host over a real socket, covering the accept path, the reconnect supersede path, and the graceful termination that must flush the "quit" remote command before closing.
/// </summary>
[NotInParallel("ChatServerHost")]
public sealed class MatchServerHandshakeIntegrationTests(ServiceContainerContext containerContext)
{
    [Test]
    public async Task Valid_Server_Handshake_Is_Accepted()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (int hostAccountID, string hostAccountName) = await ChatTestData.SeedAccount(host.Services, AccountType.ServerHost);

        int serverID = Random.Shared.Next(100_000, 999_999);
        string cookie = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedMatchServer(host.Services, serverID, hostAccountID, hostAccountName, cookie);

        using TcpClient client = await host.ConnectMatchServerAsync();

        NetworkStream stream = client.GetStream();

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildServerHandshake(serverID, cookie));

        using CancellationTokenSource timeout = new (TimeSpan.FromSeconds(10));

        ushort command = await ChatTestProtocol.ReadCommand(stream, timeout.Token);

        await Assert.That(command).IsEqualTo((ushort) ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_ACCEPT);
    }

    [Test]
    public async Task Reconnect_Supersedes_The_Existing_Session_And_Preserves_The_Cache_Entry()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (int hostAccountID, string hostAccountName) = await ChatTestData.SeedAccount(host.Services, AccountType.ServerHost);

        int serverID = Random.Shared.Next(100_000, 999_999);
        string cookie = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedMatchServer(host.Services, serverID, hostAccountID, hostAccountName, cookie);

        using TcpClient first = await host.ConnectMatchServerAsync();

        NetworkStream firstStream = first.GetStream();

        await ChatTestProtocol.WriteFrame(firstStream, ChatTestProtocol.BuildServerHandshake(serverID, cookie));

        using (CancellationTokenSource firstTimeout = new (TimeSpan.FromSeconds(10)))
            await Assert.That(await ChatTestProtocol.ReadCommand(firstStream, firstTimeout.Token)).IsEqualTo((ushort) ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_ACCEPT);

        using TcpClient second = await host.ConnectMatchServerAsync();

        NetworkStream secondStream = second.GetStream();

        await ChatTestProtocol.WriteFrame(secondStream, ChatTestProtocol.BuildServerHandshake(serverID, cookie));

        using (CancellationTokenSource secondTimeout = new (TimeSpan.FromSeconds(10)))
            await Assert.That(await ChatTestProtocol.ReadCommand(secondStream, secondTimeout.Token)).IsEqualTo((ushort) ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_ACCEPT);

        bool firstWasSuperseded = await ChatTestProtocol.WaitForClose(firstStream, TimeSpan.FromSeconds(10));

        bool cacheEntryPreserved;

        await using (AsyncServiceScope scope = host.Services.CreateAsyncScope())
        {
            IDatabase distributedCacheStore = scope.ServiceProvider.GetRequiredService<IDatabase>();

            cacheEntryPreserved = await distributedCacheStore.GetMatchServerBySessionCookie(cookie) is not null;
        }

        using (Assert.Multiple())
        {
            await Assert.That(firstWasSuperseded).IsTrue();
            await Assert.That(Context.MatchServerChatSessions.ContainsKey(serverID)).IsTrue();
            await Assert.That(cacheEntryPreserved).IsTrue();
        }
    }

    [Test]
    public async Task Graceful_Terminate_Delivers_The_Quit_Remote_Command()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (int hostAccountID, string hostAccountName) = await ChatTestData.SeedAccount(host.Services, AccountType.ServerHost);

        int serverID = Random.Shared.Next(100_000, 999_999);
        string cookie = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedMatchServer(host.Services, serverID, hostAccountID, hostAccountName, cookie);

        using TcpClient client = await host.ConnectMatchServerAsync();

        NetworkStream stream = client.GetStream();

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildServerHandshake(serverID, cookie));

        // Wait For The Handshake To Register The Session In The Pool, Then Terminate It Gracefully
        await ChatTestProtocol.WaitUntil(() => Context.MatchServerChatSessions.ContainsKey(serverID), TimeSpan.FromSeconds(10));

        MatchServerChatSession session = Context.MatchServerChatSessions[serverID];

        await using (AsyncServiceScope scope = host.Services.CreateAsyncScope())
        {
            IDatabase distributedCacheStore = scope.ServiceProvider.GetRequiredService<IDatabase>();

            await session.Terminate(distributedCacheStore);
        }

        // The Graceful Teardown Must Flush The "quit" Remote Command To The Match Server Before The Socket Is Closed
        bool quitDelivered = await ChatTestProtocol.ReadUntilRemoteCommand(stream, "quit", TimeSpan.FromSeconds(10));

        await Assert.That(quitDelivered).IsTrue();
    }
}
