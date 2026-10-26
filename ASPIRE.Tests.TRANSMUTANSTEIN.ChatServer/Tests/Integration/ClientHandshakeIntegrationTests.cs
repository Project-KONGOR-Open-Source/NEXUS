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
    public async Task Handshake_With_A_Retired_Session_Cookie_Is_Rejected()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (int accountID, string accountName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);

        IDatabase distributedCacheStore = host.Services.GetRequiredService<IDatabase>();

        string retiredCookie = Guid.CreateVersion7().ToString();
        string liveCookie = Guid.CreateVersion7().ToString();

        try
        {
            await distributedCacheStore.SupersedeSessionCookies(accountID, accountName, retiredCookie, []);

            // The Player Logs In Again, Which Retires The Earlier Login
            await distributedCacheStore.SupersedeSessionCookies(accountID, accountName, liveCookie, []);

            string authenticationHash = SRPAuthenticationHandlers.ComputeChatServerCookieHash(accountID, RemoteIP, retiredCookie);

            using TcpClient client = await host.ConnectClientAsync();

            NetworkStream stream = client.GetStream();

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildClientHandshake(accountID, retiredCookie, RemoteIP, authenticationHash));

            using CancellationTokenSource timeout = new (TimeSpan.FromSeconds(10));

            ushort command = await ChatTestProtocol.ReadCommand(stream, timeout.Token);

            await Assert.That(command).IsEqualTo((ushort) ChatProtocol.ChatServerToClient.NET_CHAT_CL_REJECT);
        }

        finally
        {
            // Every Chat Server Host Shares The Unprefixed Distributed Cache While Each Host's Database Restarts Its Identifiers, So A Latest-Login Record Left Behind Would Be Retired By The Login Of A Later Test's Account With The Same ID
            await distributedCacheStore.RemoveLatestSessionCookie(accountID, retiredCookie);
            await distributedCacheStore.RemoveLatestSessionCookie(accountID, liveCookie);
            await distributedCacheStore.RemoveAccountNameForSessionCookie(retiredCookie);
            await distributedCacheStore.RemoveAccountNameForSessionCookie(liveCookie);
        }
    }

    [Test]
    public async Task Handshake_With_Another_Version_Is_Rejected_With_Bad_Version()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (int accountID, string accountName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);

        string cookie = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedClientSessionCookie(host.Services, cookie, accountName);

        string authenticationHash = SRPAuthenticationHandlers.ComputeChatServerCookieHash(accountID, RemoteIP, cookie);

        using TcpClient client = await host.ConnectClientAsync();

        NetworkStream stream = client.GetStream();

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildClientHandshake(accountID, cookie, RemoteIP, authenticationHash, clientVersion: [4, 10, 1, 0]));

        ChatBuffer? reject = await ChatTestProtocol.ReadUntilCommand(stream, (ushort) ChatProtocol.ChatServerToClient.NET_CHAT_CL_REJECT, TimeSpan.FromSeconds(10));

        await Assert.That(reject).IsNotNull();
        await Assert.That((ChatProtocol.ChatRejectReason) reject!.ReadInt8()).IsEqualTo(ChatProtocol.ChatRejectReason.ECR_BAD_VERSION);
        await Assert.That(await ChatTestProtocol.WaitForClose(stream, TimeSpan.FromSeconds(10))).IsTrue();
    }

    [Test]
    public async Task Handshake_With_Another_Version_Leaves_The_Existing_Session_Connected()
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

        await ChatTestProtocol.WriteFrame(secondStream, ChatTestProtocol.BuildClientHandshake(accountID, cookie, RemoteIP, authenticationHash, clientVersion: [4, 10, 1, 0]));

        ChatBuffer? reject = await ChatTestProtocol.ReadUntilCommand(secondStream, (ushort) ChatProtocol.ChatServerToClient.NET_CHAT_CL_REJECT, TimeSpan.FromSeconds(10));

        await Assert.That(reject).IsNotNull();
        await Assert.That((ChatProtocol.ChatRejectReason) reject!.ReadInt8()).IsEqualTo(ChatProtocol.ChatRejectReason.ECR_BAD_VERSION);

        // A Login That Is Turned Away Takes Nothing From The Session Already Connected
        await Assert.That(await ChatTestProtocol.WaitForClose(firstStream, TimeSpan.FromSeconds(3))).IsFalse();
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
    public async Task Second_Login_Tells_The_First_Session_Why_Just_Before_Rejecting_It()
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

        await ChatTestProtocol.WriteFrame(second.GetStream(), ChatTestProtocol.BuildClientHandshake(accountID, cookie, RemoteIP, authenticationHash));

        List<(ushort Command, ChatBuffer Frame)> frames = await ChatTestProtocol.ReadCommandsUntil(firstStream, (ushort) ChatProtocol.ChatServerToClient.NET_CHAT_CL_REJECT, TimeSpan.FromSeconds(10));

        using (Assert.Multiple())
        {
            await Assert.That(frames.Count).IsGreaterThanOrEqualTo(2);
            await Assert.That(frames[^1].Command).IsEqualTo((ushort) ChatProtocol.ChatServerToClient.NET_CHAT_CL_REJECT);
            await Assert.That(frames[^2].Command).IsEqualTo((ushort) ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL);
            await Assert.That(ChatTestProtocol.ParseNotice(frames[^2].Frame)).IsEqualTo((ClientHandshake.LoggedInElsewhereSenderName, ClientHandshake.LoggedInElsewhereMessage(accountName)));
        }
    }

    [Test]
    public async Task A_Login_Elsewhere_Names_The_Account_With_Its_Clan_Tag()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (int accountID, string accountName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);

        string accountNameWithClanTag = await ChatTestData.SeedClanMember(host.Services, accountID);

        string cookie = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedClientSessionCookie(host.Services, cookie, accountName);

        string authenticationHash = SRPAuthenticationHandlers.ComputeChatServerCookieHash(accountID, RemoteIP, cookie);

        using TcpClient first = await host.ConnectClientAsync();

        NetworkStream firstStream = first.GetStream();

        await ChatTestProtocol.WriteFrame(firstStream, ChatTestProtocol.BuildClientHandshake(accountID, cookie, RemoteIP, authenticationHash));

        using (CancellationTokenSource firstTimeout = new (TimeSpan.FromSeconds(10)))
            await Assert.That(await ChatTestProtocol.ReadCommand(firstStream, firstTimeout.Token)).IsEqualTo((ushort) ChatProtocol.ChatServerToClient.NET_CHAT_CL_ACCEPT);

        using TcpClient second = await host.ConnectClientAsync();

        await ChatTestProtocol.WriteFrame(second.GetStream(), ChatTestProtocol.BuildClientHandshake(accountID, cookie, RemoteIP, authenticationHash));

        List<(ushort Command, ChatBuffer Frame)> frames = await ChatTestProtocol.ReadCommandsUntil(firstStream, (ushort) ChatProtocol.ChatServerToClient.NET_CHAT_CL_REJECT, TimeSpan.FromSeconds(10));

        using (Assert.Multiple())
        {
            await Assert.That(frames.Count).IsGreaterThanOrEqualTo(2);
            await Assert.That(frames[^1].Command).IsEqualTo((ushort) ChatProtocol.ChatServerToClient.NET_CHAT_CL_REJECT);
            await Assert.That(frames[^2].Command).IsEqualTo((ushort) ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL);
            await Assert.That(ChatTestProtocol.ParseNotice(frames[^2].Frame)).IsEqualTo((ClientHandshake.LoggedInElsewhereSenderName, ClientHandshake.LoggedInElsewhereMessage(accountNameWithClanTag)));
        }
    }

    [Test]
    public async Task A_Login_On_Another_Account_Of_The_User_Names_That_Account_To_The_First_Session()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (int mainAccountID, string mainAccountName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);

        (int subAccountID, string subAccountName) = await ChatTestData.SeedSubAccount(host.Services, mainAccountID);

        string mainCookie = Guid.CreateVersion7().ToString();
        string subCookie = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedClientSessionCookie(host.Services, mainCookie, mainAccountName);
        await ChatTestData.SeedClientSessionCookie(host.Services, subCookie, subAccountName);

        using TcpClient first = await host.ConnectClientAsync();

        NetworkStream firstStream = first.GetStream();

        await ChatTestProtocol.WriteFrame(firstStream, ChatTestProtocol.BuildClientHandshake(mainAccountID, mainCookie, RemoteIP, SRPAuthenticationHandlers.ComputeChatServerCookieHash(mainAccountID, RemoteIP, mainCookie)));

        using (CancellationTokenSource firstTimeout = new (TimeSpan.FromSeconds(10)))
            await Assert.That(await ChatTestProtocol.ReadCommand(firstStream, firstTimeout.Token)).IsEqualTo((ushort) ChatProtocol.ChatServerToClient.NET_CHAT_CL_ACCEPT);

        using TcpClient second = await host.ConnectClientAsync();

        await ChatTestProtocol.WriteFrame(second.GetStream(), ChatTestProtocol.BuildClientHandshake(subAccountID, subCookie, RemoteIP, SRPAuthenticationHandlers.ComputeChatServerCookieHash(subAccountID, RemoteIP, subCookie)));

        List<(ushort Command, ChatBuffer Frame)> frames = await ChatTestProtocol.ReadCommandsUntil(firstStream, (ushort) ChatProtocol.ChatServerToClient.NET_CHAT_CL_REJECT, TimeSpan.FromSeconds(10));

        using (Assert.Multiple())
        {
            await Assert.That(frames.Count).IsGreaterThanOrEqualTo(2);
            await Assert.That(frames[^1].Command).IsEqualTo((ushort) ChatProtocol.ChatServerToClient.NET_CHAT_CL_REJECT);
            await Assert.That(frames[^2].Command).IsEqualTo((ushort) ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL);
            await Assert.That(ChatTestProtocol.ParseNotice(frames[^2].Frame)).IsEqualTo((ClientHandshake.LoggedInElsewhereSenderName, ClientHandshake.LoggedInElsewhereMessage(subAccountName)));
        }
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
