namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.ServerManagerToChatServer.NET_CHAT_SM_STATUS)]
public class ServerManagerStatus : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        ServerManagerStatusRequestData requestData = new (buffer);

        Log.Debug(@"Received Status Update From Server Manager ID ""{ServerManagerID}"" - Name: ""{Name}"", Address: ""{Address}:{Port}"", Location: ""{Location}"", Version: ""{Version}"", Shutting Down: {ShuttingDown}",
            requestData.ServerManagerID, requestData.Name, requestData.Address, requestData.Port, requestData.Location, requestData.Version, requestData.ShuttingDown);

        // TODO: Update Any Relevant Match Server Manager Data

        // TODO: Update Server Manager In Distributed Cache
    }
}

file class ServerManagerStatusRequestData
{
    public byte[] CommandBytes { get; init; }

    public int ServerManagerID { get; init; }

    public string ServerLogin { get; init; }

    public string Location { get; init; }

    public string Name { get; init; }

    public string Version { get; init; }

    public string Address { get; init; }

    public short Port { get; init; }

    public bool ShuttingDown { get; init; }

    public ServerManagerStatusRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ServerManagerID = buffer.ReadInt32();
        ServerLogin = buffer.ReadString();
        Location = buffer.ReadString();
        Name = buffer.ReadString();
        Version = buffer.ReadString();
        Address = buffer.ReadString();
        Port = buffer.ReadInt16();
        ShuttingDown = buffer.ReadInt8() is not 0;
    }
}

