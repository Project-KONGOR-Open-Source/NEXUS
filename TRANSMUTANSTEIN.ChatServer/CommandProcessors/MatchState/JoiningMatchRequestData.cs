namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

public class JoiningMatchRequestData
{
    public JoiningMatchRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ServerAddress = buffer.ReadString();
    }

    public byte[] CommandBytes { get; init; }

    public string ServerAddress { get; }
}
