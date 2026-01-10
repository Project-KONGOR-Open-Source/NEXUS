namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

public class GroupPlayerReadyStatusRequestData
{
    public GroupPlayerReadyStatusRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ReadyStatus = buffer.ReadInt8();
        GameType = (ChatProtocol.TMMGameType) buffer.ReadInt8();
    }

    public byte[] CommandBytes { get; init; }

    public byte ReadyStatus { get; init; }

    public ChatProtocol.TMMGameType GameType { get; }
}
