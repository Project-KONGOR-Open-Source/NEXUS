namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Services;

/// <summary>
///     Tests for how the chat server ends a chat session when the master server publishes a logout: only the session which used the logout's session cookie ends, because the newer login of the same account must survive the old machine's logout.
/// </summary>
[NotInParallel(nameof(ChatServerHost))]
public sealed class LogoutMonitorTests(ServiceContainerContext containerContext)
{
    private const string RemoteIP = "127.0.0.1";

    /// <summary>
    ///     How long a test listens to be sure that a session which is expected to survive does not close.
    /// </summary>
    private static readonly TimeSpan QuietPeriod = TimeSpan.FromSeconds(2);

    /// <summary>
    ///     How long a session which is expected to close is awaited.
    /// </summary>
    private static readonly TimeSpan CloseTimeout = TimeSpan.FromSeconds(10);

    [Test]
    public async Task A_Logout_With_Another_Session_Cookie_Leaves_The_Chat_Session_Alone()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (_, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        // The Old Machine Logs Out With Its Own Session Cookie After The New Machine Has Taken Over The Account's Chat Session
        await host.Services.GetRequiredService<IDatabase>().PublishAccountLogout(new AccountLogout { AccountName = accountName, SessionCookie = Guid.CreateVersion7().ToString() });

        bool closed = await ChatTestProtocol.WaitForClose(client.GetStream(), QuietPeriod);

        using (Assert.Multiple())
        {
            await Assert.That(closed).IsFalse();
            await Assert.That(Context.ClientChatSessions.ContainsKey(accountName)).IsTrue();
        }
    }

    [Test]
    public async Task A_Logout_Ends_The_Chat_Session_Which_Used_Its_Session_Cookie()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (_, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        await host.Services.GetRequiredService<IDatabase>().PublishAccountLogout(new AccountLogout { AccountName = accountName, SessionCookie = cookie });

        bool closed = await ChatTestProtocol.WaitForClose(client.GetStream(), CloseTimeout);

        bool unregistered = await ChatTestProtocol.WaitUntil(() => Context.ClientChatSessions.ContainsKey(accountName) is false, CloseTimeout);

        using (Assert.Multiple())
        {
            await Assert.That(closed).IsTrue();
            await Assert.That(unregistered).IsTrue();
        }
    }

    [Test]
    public async Task A_Malformed_Logout_Does_Not_Stop_The_Monitor()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (_, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        // A Plain Account Name Is What Older Master Servers Published, And It Is Not An Account Logout
        await host.Services.GetRequiredService<IConnectionMultiplexer>().GetSubscriber().PublishAsync(RedisChannel.Literal(DistributedCacheExtensions.AccountLogoutChannel), accountName);

        await host.Services.GetRequiredService<IDatabase>().PublishAccountLogout(new AccountLogout { AccountName = accountName, SessionCookie = cookie });

        bool closed = await ChatTestProtocol.WaitForClose(client.GetStream(), CloseTimeout);

        await Assert.That(closed).IsTrue();
    }
}
