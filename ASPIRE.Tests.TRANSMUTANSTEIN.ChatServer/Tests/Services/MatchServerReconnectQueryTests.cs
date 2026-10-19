namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Services;

/// <summary>
///     Exercises the reconnect query that asks a match server whether a player's slot can still be reclaimed, against a fake game socket on the loopback interface.
/// </summary>
public sealed class MatchServerReconnectQueryTests
{
    private const int MatchID = 4242;
    private const int AccountID = 17;

    [Test]
    public async Task The_Request_Carries_The_Query_Before_And_After_The_Watermark_Area()
    {
        byte[] request = MatchServerReconnectQuery.BuildRequest(matchID: 0x01020304, accountID: 0x0A0B0C0D);
        byte[] query = [0x00, 0x00, 0x01, 0xCC, 0x04, 0x03, 0x02, 0x01, 0x0D, 0x0C, 0x0B, 0x0A, 0x00, 0x00];

        await Assert.That(request.Length).IsEqualTo(54);
        await Assert.That(request[..14]).IsEquivalentTo(query);
        await Assert.That(request[14..40].All(value => value is 0)).IsTrue();
        await Assert.That(request[40..]).IsEquivalentTo(query);
    }

    [Test]
    [Arguments(0u)]
    [Arguments(240000u)]
    [Arguments(uint.MaxValue - 5000u)]
    public async Task A_Watermarked_Response_Is_Read(uint slotTimeRemaining)
    {
        byte[] datagram = new byte[48];
        datagram[43] = 0x6F;
        BitConverter.TryWriteBytes(datagram.AsSpan(44), slotTimeRemaining);

        await Assert.That(MatchServerReconnectQuery.ParseResponse(datagram)).IsEqualTo(slotTimeRemaining);
    }

    [Test]
    public async Task An_Unwatermarked_Response_Is_Read()
    {
        byte[] datagram = new byte[8];
        datagram[3] = 0x6F;
        BitConverter.TryWriteBytes(datagram.AsSpan(4), 98765u);

        await Assert.That(MatchServerReconnectQuery.ParseResponse(datagram)).IsEqualTo(98765u);
    }

    [Test]
    [Arguments(48, 0x66)]
    [Arguments(8, 0x66)]
    [Arguments(47, 0x6F)]
    [Arguments(9, 0x6F)]
    public async Task A_Datagram_Which_Is_Not_A_Reconnect_Response_Is_Ignored(int length, int command)
    {
        byte[] datagram = new byte[length];
        datagram[length is 48 or 47 ? 43 : 3] = (byte) command;

        await Assert.That(MatchServerReconnectQuery.ParseResponse(datagram)).IsNull();
    }

    [Test]
    public async Task The_Query_Returns_The_Time_The_Match_Server_Reports()
    {
        await using FakeMatchServerGameSocket socket = FakeMatchServerGameSocket.Start(123456);

        uint? result = await new MatchServerReconnectQuery().QuerySlotTimeRemaining(new IPEndPoint(IPAddress.Loopback, socket.Port), MatchID, AccountID);

        await Assert.That(result).IsEqualTo(123456u);
        await Assert.That(socket.Requests.Contains((MatchID, AccountID))).IsTrue();
    }

    [Test]
    public async Task The_Query_Retries_A_Lost_Request()
    {
        await using FakeMatchServerGameSocket socket = FakeMatchServerGameSocket.Start(777);
        socket.IgnoredRequestsRemaining = MatchServerReconnectQuery.RequestAttempts - 1;

        uint? result = await new MatchServerReconnectQuery().QuerySlotTimeRemaining(new IPEndPoint(IPAddress.Loopback, socket.Port), MatchID, AccountID);

        await Assert.That(result).IsEqualTo(777u);
    }

    [Test]
    public async Task The_Query_Returns_Null_When_The_Match_Server_Never_Answers()
    {
        await using FakeMatchServerGameSocket socket = FakeMatchServerGameSocket.Start(1);
        socket.Answers = false;

        uint? result = await new MatchServerReconnectQuery().QuerySlotTimeRemaining(new IPEndPoint(IPAddress.Loopback, socket.Port), MatchID, AccountID);

        await Assert.That(result).IsNull();
        await Assert.That(socket.Requests.Count).IsEqualTo(MatchServerReconnectQuery.RequestAttempts);
    }

    [Test]
    public async Task The_Query_Returns_Null_Rather_Than_Throwing_For_A_Closed_Port()
    {
        int port;

        using (UdpClient probe = new (new IPEndPoint(IPAddress.Loopback, 0)))
            port = ((IPEndPoint) probe.Client.LocalEndPoint!).Port;

        uint? result = await new MatchServerReconnectQuery().QuerySlotTimeRemaining(new IPEndPoint(IPAddress.Loopback, port), MatchID, AccountID);

        await Assert.That(result).IsNull();
    }
}
