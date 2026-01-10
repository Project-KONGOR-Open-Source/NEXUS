namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_STATUS)]
public class ServerStatus : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        ServerStatusRequestData requestData = new(buffer);

        Log.Debug(
            @"Received Status Update From Server ID ""{ServerID}"" - Name: ""{Name}"", Address: ""{Address}:{Port}"", Location: ""{Location}"", Status: {Status}",
            requestData.ServerID, requestData.Name, requestData.Address, requestData.Port, requestData.Location,
            requestData.Status);

        // TODO: Update Any Relevant Match Server Data

        // TODO: Update Server In Distributed Cache

        // TODO: If Status Is IDLE, Mark Server As Available For Match Allocation
        // TODO: If Status Is ACTIVE, Update Match Information And Player Availability States
        // TODO: If Status Is CRASHED Or KILLED, Remove Server From Pool And Handle Match Cleanup
    }
}