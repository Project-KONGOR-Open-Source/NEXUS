namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

/// <summary>
///     Handles status updates from server managers.
///     Server managers periodically send their status to inform the chat server about their current state.
///     Protocol verified against HoN c_servermanager.cpp ProcessStatusUpdate (lines 209-221) and KONGOR ManagerStatusRequest.cs.
/// </summary>
[ChatCommand(ChatProtocol.ServerManagerToChatServer.NET_CHAT_SM_STATUS)]
public class ServerManagerStatus : ISynchronousCommandProcessor<MatchServerManagerChatSession>
{
    public void Process(MatchServerManagerChatSession session, ChatBuffer buffer)
    {

        ServerManagerStatusData statusData = new (buffer);

        Log.Debug(@"Received Status Update From Server Manager ID ""{ServerManagerID}"" - Location: ""{Location}"", Name: ""{Name}"", Version: ""{Version}"", Address: ""{Address}:{Port}"", Shutting Down: {ShuttingDown}",
            statusData.ServerManagerID, statusData.Location, statusData.Name, statusData.Version, statusData.Address, statusData.Port, statusData.ShuttingDown);

        // TODO: Update Server Manager State In Context.MatchServerManagers
        // TODO: If ShuttingDown Is TRUE, Mark Server Manager As Unavailable For New Match Allocations
        // TODO: If ShuttingDown Is TRUE And No Active Matches, Remove Server Manager From Pool After Grace Period
    }
}

/// <summary>
///     Server manager status request data structure.
///     Structure matches HoN c_servermanager.cpp lines 211-218 and KONGOR ManagerStatusRequest.cs lines 31-44.
/// </summary>
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

    public bool ShuttingDown = buffer.ReadInt8() != 0;
}
