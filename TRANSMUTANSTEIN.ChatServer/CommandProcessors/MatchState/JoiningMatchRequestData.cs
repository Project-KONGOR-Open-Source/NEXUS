namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

public class JoiningMatchRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public string ServerAddress { get; } = buffer.ReadString();
}