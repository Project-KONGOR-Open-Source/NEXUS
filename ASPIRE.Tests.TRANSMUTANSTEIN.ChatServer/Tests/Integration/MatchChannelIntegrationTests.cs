namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Integration;

/// <summary>
///     Drives the joined-game notification against the real host over real sockets, verifying that players are placed into the match channel while spectators and mentors, for whom the game client asks not to be placed into it, are kept out of it.
/// </summary>
[NotInParallel(nameof(ChatServerHost))]
public sealed class MatchChannelIntegrationTests(ServiceContainerContext containerContext)
{
    private const string RemoteIP = "127.0.0.1";

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Joining_A_Match_Places_The_Client_Into_The_Match_Channel_Only_When_The_Client_Asks_To_Be(bool joinMatchChannel)
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (int accountID, string accountName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);

        string cookie = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedClientSessionCookie(host.Services, cookie, accountName);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        // A Match ID Unique To This Test, Because Channels Live In The Process-Wide Registry For The Lifetime Of The Host
        int matchID = Random.Shared.Next(1_000_000, int.MaxValue);

        await ChatTestProtocol.WriteFrame(client.GetStream(), ChatTestProtocol.BuildJoinedGame("Match Channel Test", matchID, joinMatchChannel));

        // The Client's Requests Are Processed In Order, So The Response To A Subsequent Join Confirms That The Joined-Game Notification Has Been Processed
        await ChatTestProtocol.WriteFrame(client.GetStream(), ChatTestProtocol.BuildJoinChannel($"Sync_{Guid.CreateVersion7():N}"[..20]));

        await Assert.That(await ChatTestProtocol.ReadUntilCommand(client.GetStream(), (ushort) ChatProtocol.Command.CHAT_CMD_CHANGED_CHANNEL, TimeSpan.FromSeconds(10))).IsNotNull();

        bool isMatchChannelMember = Context.ChatChannels.TryGetValue($"Match {matchID}", out ChatChannel? matchChannel) && matchChannel.Members.ContainsKey(accountName);

        await Assert.That(isMatchChannelMember).IsEqualTo(joinMatchChannel);
    }
}
