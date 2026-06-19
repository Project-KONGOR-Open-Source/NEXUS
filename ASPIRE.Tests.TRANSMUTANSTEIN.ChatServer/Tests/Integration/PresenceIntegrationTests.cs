namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Integration;

/// <summary>
///     Drives presence broadcasting against the real host over real sockets, verifying that connecting clients receive an initial snapshot of their already-online friends and that a friend coming online is broadcast to friends who are already connected.
/// </summary>
[NotInParallel(nameof(ChatServerHost))]
public sealed class PresenceIntegrationTests(ServiceContainerContext containerContext)
{
    private const string RemoteIP = "127.0.0.1";

    [Test]
    public async Task A_Friend_Connecting_Broadcasts_A_Status_Update_To_An_Already_Online_Friend()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (int firstID, string firstName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);
        (int secondID, string secondName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);

        await ChatTestData.SeedFriendship(host.Services, (firstID, firstName), (secondID, secondName));

        string firstCookie = Guid.CreateVersion7().ToString();
        string secondCookie = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedClientSessionCookie(host.Services, firstCookie, firstName);
        await ChatTestData.SeedClientSessionCookie(host.Services, secondCookie, secondName);

        // The First Friend Comes Online First, So It Is Already Connected When The Second Friend's Status Update Is Broadcast
        using TcpClient first = await host.ConnectAndAuthenticateClientAsync(firstID, firstName, firstCookie, RemoteIP);
        using TcpClient second = await host.ConnectAndAuthenticateClientAsync(secondID, secondName, secondCookie, RemoteIP);

        ChatBuffer? statusUpdate = await ChatTestProtocol.ReadUntilCommand(first.GetStream(), (ushort) ChatProtocol.Command.CHAT_CMD_UPDATE_STATUS, TimeSpan.FromSeconds(10));

        await Assert.That(statusUpdate).IsNotNull();

        // The First Field Of A Status Update Is The Account Whose Presence Changed
        await Assert.That(statusUpdate!.ReadInt32()).IsEqualTo(secondID);
    }

    [Test]
    public async Task Initial_Status_Reports_An_Already_Online_Friend_To_A_Connecting_Client()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (int firstID, string firstName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);
        (int secondID, string secondName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);

        await ChatTestData.SeedFriendship(host.Services, (firstID, firstName), (secondID, secondName));

        string firstCookie = Guid.CreateVersion7().ToString();
        string secondCookie = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedClientSessionCookie(host.Services, firstCookie, firstName);
        await ChatTestData.SeedClientSessionCookie(host.Services, secondCookie, secondName);

        // The First Friend Is Already Online When The Second Friend Connects And Requests Its Initial Status Snapshot
        using TcpClient first = await host.ConnectAndAuthenticateClientAsync(firstID, firstName, firstCookie, RemoteIP);
        using TcpClient second = await host.ConnectAndAuthenticateClientAsync(secondID, secondName, secondCookie, RemoteIP);

        ChatBuffer? initialStatus = await ChatTestProtocol.ReadUntilCommand(second.GetStream(), (ushort) ChatProtocol.Command.CHAT_CMD_INITIAL_STATUS, TimeSpan.FromSeconds(10));

        await Assert.That(initialStatus).IsNotNull();

        // The Second Client's Only Online Friend Is The First Client, So The Snapshot Lists Exactly That One Peer
        using (Assert.Multiple())
        {
            await Assert.That(initialStatus!.ReadInt32()).IsEqualTo(1);       // Online Peer Count
            await Assert.That(initialStatus.ReadInt32()).IsEqualTo(firstID);  // First Online Peer Account ID
        }
    }
}
