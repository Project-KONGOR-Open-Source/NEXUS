namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Results;

public class MatchIdResultRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public ChatProtocol.MatchIDResult Result { get; } = (ChatProtocol.MatchIDResult) buffer.ReadInt8();

    public int RequestTimeMilliseconds { get; } = buffer.ReadInt32();
}