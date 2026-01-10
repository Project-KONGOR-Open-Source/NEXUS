namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Results;

public class MatchIDResultRequestData
{
    public MatchIDResultRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        Result = (ChatProtocol.MatchIDResult) buffer.ReadInt8();
        RequestTimeMilliseconds = buffer.ReadInt32();
    }

    public byte[] CommandBytes { get; init; }

    public ChatProtocol.MatchIDResult Result { get; }

    public int RequestTimeMilliseconds { get; }
}
