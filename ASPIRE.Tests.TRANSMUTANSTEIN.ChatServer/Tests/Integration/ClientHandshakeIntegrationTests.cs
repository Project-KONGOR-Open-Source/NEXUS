namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Integration;

/// <summary>
///     Drives the client handshake against the real host over a real socket, covering the accept and reject paths, the concurrent-connection takeover, and the disconnect cleanup.
/// </summary>
[NotInParallel(nameof(ChatServerHost))]
public sealed class ClientHandshakeIntegrationTests(ServiceContainerContext containerContext)
{
    private const string RemoteIP = "127.0.0.1";

    [Test]
    public async Task Valid_Handshake_Is_Accepted()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (int accountID, string accountName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);

        string cookie = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedClientSessionCookie(host.Services, cookie, accountName);

        string authenticationHash = SRPAuthenticationHandlers.ComputeChatServerCookieHash(accountID, RemoteIP, cookie);

        using TcpClient client = await host.ConnectClientAsync();

        NetworkStream stream = client.GetStream();

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildClientHandshake(accountID, cookie, RemoteIP, authenticationHash));

        using CancellationTokenSource timeout = new (TimeSpan.FromSeconds(10));

        ushort command = await ChatTestProtocol.ReadCommand(stream, timeout.Token);

        await Assert.That(command).IsEqualTo((ushort) ChatProtocol.ChatServerToClient.NET_CHAT_CL_ACCEPT);
    }

    [Test]
    public async Task Handshake_With_Unknown_Cookie_Is_Rejected()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (int accountID, string _) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);

        // The Cookie Is Never Seeded Into The Cache, So Authentication Must Fail
        string cookie = Guid.CreateVersion7().ToString();

        string authenticationHash = SRPAuthenticationHandlers.ComputeChatServerCookieHash(accountID, RemoteIP, cookie);

        using TcpClient client = await host.ConnectClientAsync();

        NetworkStream stream = client.GetStream();

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildClientHandshake(accountID, cookie, RemoteIP, authenticationHash));

        using CancellationTokenSource timeout = new (TimeSpan.FromSeconds(10));

        ushort command = await ChatTestProtocol.ReadCommand(stream, timeout.Token);

        await Assert.That(command).IsEqualTo((ushort) ChatProtocol.ChatServerToClient.NET_CHAT_CL_REJECT);
    }

    [Test]
    public async Task Second_Login_Disconnects_The_First_Session()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (int accountID, string accountName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);

        string cookie = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedClientSessionCookie(host.Services, cookie, accountName);

        string authenticationHash = SRPAuthenticationHandlers.ComputeChatServerCookieHash(accountID, RemoteIP, cookie);

        using TcpClient first = await host.ConnectClientAsync();

        NetworkStream firstStream = first.GetStream();

        await ChatTestProtocol.WriteFrame(firstStream, ChatTestProtocol.BuildClientHandshake(accountID, cookie, RemoteIP, authenticationHash));

        using (CancellationTokenSource firstTimeout = new (TimeSpan.FromSeconds(10)))
            await Assert.That(await ChatTestProtocol.ReadCommand(firstStream, firstTimeout.Token)).IsEqualTo((ushort) ChatProtocol.ChatServerToClient.NET_CHAT_CL_ACCEPT);

        using TcpClient second = await host.ConnectClientAsync();

        NetworkStream secondStream = second.GetStream();

        await ChatTestProtocol.WriteFrame(secondStream, ChatTestProtocol.BuildClientHandshake(accountID, cookie, RemoteIP, authenticationHash));

        using (CancellationTokenSource secondTimeout = new (TimeSpan.FromSeconds(10)))
            await Assert.That(await ChatTestProtocol.ReadCommand(secondStream, secondTimeout.Token)).IsEqualTo((ushort) ChatProtocol.ChatServerToClient.NET_CHAT_CL_ACCEPT);

        bool firstWasDisconnected = await ChatTestProtocol.WaitForClose(firstStream, TimeSpan.FromSeconds(10));

        await Assert.That(firstWasDisconnected).IsTrue();
    }

    [Test]
    public async Task Disconnect_Removes_The_Session_From_The_Registry()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (int accountID, string accountName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);

        string cookie = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedClientSessionCookie(host.Services, cookie, accountName);

        string authenticationHash = SRPAuthenticationHandlers.ComputeChatServerCookieHash(accountID, RemoteIP, cookie);

        using (TcpClient client = await host.ConnectClientAsync())
        {
            NetworkStream stream = client.GetStream();

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildClientHandshake(accountID, cookie, RemoteIP, authenticationHash));

            using CancellationTokenSource timeout = new (TimeSpan.FromSeconds(10));

            await ChatTestProtocol.ReadCommand(stream, timeout.Token);

            await Assert.That(await ChatTestProtocol.WaitUntil(() => Context.ClientChatSessions.ContainsKey(accountName), TimeSpan.FromSeconds(10))).IsTrue();
        }

        // After The Client Socket Closes, The Disconnect Cleanup Removes The Session From The Registry
        await Assert.That(await ChatTestProtocol.WaitUntil(() => Context.ClientChatSessions.ContainsKey(accountName) is false, TimeSpan.FromSeconds(10))).IsTrue();
    }
}
