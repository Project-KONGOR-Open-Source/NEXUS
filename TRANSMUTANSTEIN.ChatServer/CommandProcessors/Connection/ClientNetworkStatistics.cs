namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

public class ClientNetworkStatistics
{
    public int AccountID { get; init; }

    public string Address { get; init; } = string.Empty;

    public short PingMinimum { get; init; }

    public short PingAverage { get; init; }

    public short PingMaximum { get; init; }

    public long ReliablePacketsSent { get; init; }

    public long ReliablePacketsAcknowledged { get; init; }

    public long ReliablePacketsPeerSent { get; init; }

    public long ReliablePacketsPeerAcknowledged { get; init; }

    public long UnreliablePacketsSent { get; init; }

    public long UnreliablePacketsPeerReceived { get; init; }

    public long UnreliablePacketsPeerSent { get; init; }

    public long UnreliablePacketsReceived { get; init; }
}