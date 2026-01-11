namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

public class GroupPlayerReadyStatusRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public byte ReadyStatus { get; init; } = buffer.ReadInt8();

    public ChatProtocol.TMMGameType GameType { get; } = (ChatProtocol.TMMGameType) buffer.ReadInt8();
}