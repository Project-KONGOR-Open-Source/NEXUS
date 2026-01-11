namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

public class ServerManagerDisconnectRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();
}