namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

public class ServerManagerStatusRequestData
{
    public ServerManagerStatusRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ServerManagerId = buffer.ReadInt32();
        ServerLogin = buffer.ReadString();
        Location = buffer.ReadString();
        Name = buffer.ReadString();
        Version = buffer.ReadString();
        Address = buffer.ReadString();
        Port = buffer.ReadInt16();
        ShuttingDown = buffer.ReadInt8() is not 0;
    }

    public byte[] CommandBytes { get; init; }

    public int ServerManagerId { get; }

    public string ServerLogin { get; init; }

    public string Location { get; }

    public string Name { get; }

    public string Version { get; }

    public string Address { get; }

    public short Port { get; }

    public bool ShuttingDown { get; }
}