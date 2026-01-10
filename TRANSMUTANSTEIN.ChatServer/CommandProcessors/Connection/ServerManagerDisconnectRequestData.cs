namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

public class ServerManagerDisconnectRequestData
{
    public ServerManagerDisconnectRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }

    public byte[] CommandBytes { get; init; }
}
