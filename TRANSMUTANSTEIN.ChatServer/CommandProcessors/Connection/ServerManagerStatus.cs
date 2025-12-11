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

public class ServerManagerStatusData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public int ServerManagerID = buffer.ReadInt32();

    public string ServerLogin = buffer.ReadString();

    public string Location = buffer.ReadString();

    public string Name = buffer.ReadString();

    public string Version = buffer.ReadString();

    public string Address = buffer.ReadString();

    public short Port = buffer.ReadInt16();

    public bool ShuttingDown = buffer.ReadInt8() is not 0;
}
