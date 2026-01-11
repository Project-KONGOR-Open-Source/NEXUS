namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

public class ServerDisconnectRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();
}