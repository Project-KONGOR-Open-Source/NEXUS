namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.ServerManagerToChatServer.NET_CHAT_SM_STATUS)]
public class ServerManagerStatus : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        ServerManagerStatusRequestData requestData = new(buffer);

        Log.Debug(
            @"Received Status Update From Server Manager ID ""{ServerManagerID}"" - Name: ""{Name}"", Address: ""{Address}:{Port}"", Location: ""{Location}"", Version: ""{Version}"", Shutting Down: {ShuttingDown}",
            requestData.ServerManagerID, requestData.Name, requestData.Address, requestData.Port, requestData.Location,
            requestData.Version, requestData.ShuttingDown);

        // TODO: Update Any Relevant Match Server Manager Data

        // TODO: Update Server Manager In Distributed Cache
    }
}