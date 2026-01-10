namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

public class ServerDisconnectRequestData
{
    public ServerDisconnectRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }

    public byte[] CommandBytes { get; init; }
}
