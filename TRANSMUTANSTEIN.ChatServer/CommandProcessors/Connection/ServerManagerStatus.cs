namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

/// <summary>
///     Handles status updates from match server managers.
///     Match server managers periodically send their status to inform the chat server about their current state.
/// </summary>
[ChatCommand(ChatProtocol.ServerManagerToChatServer.NET_CHAT_SM_STATUS)]
public class ServerManagerStatus : ISynchronousCommandProcessor<MatchServerManagerChatSession>
{
    public void Process(MatchServerManagerChatSession session, ChatBuffer buffer)
    {
        ServerManagerStatusData statusData = new (buffer);

        Log.Debug(@"Received Status Update From Server Manager ID ""{ServerManagerID}"" - Name: ""{Name}"", Address: ""{Address}:{Port}"", Location: ""{Location}"", Version: ""{Version}"", Shutting Down: {ShuttingDown}",
            statusData.ServerManagerID, statusData.Name, statusData.Address, statusData.Port, statusData.Location, statusData.Version, statusData.ShuttingDown);

        // TODO: Update Any Relevant Match Server Manager Data

        // TODO: Update Server Manager In Distributed Cache
    }
}

file class ServerManagerStatusData
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

    public ServerManagerStatusData(ChatBuffer buffer)
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
