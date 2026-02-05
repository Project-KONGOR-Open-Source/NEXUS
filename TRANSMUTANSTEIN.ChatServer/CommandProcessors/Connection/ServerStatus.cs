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

        // Update Session Metadata
        // This is CRITICAL for MatchmakingService to find IDLE servers
        if (session.ServerMetadata != null)
        {
            session.ServerMetadata.Status = requestData.Status;
            session.ServerMetadata.MatchID = requestData.MatchID;
            
            // Note: ServerStatusRequestData seems to have Address/Port/Location too, 
            // but we usually trust the DB/Cache source of truth or what we set on Handshake.
            // But if it changes dynamically (e.g. port), we might update it here.
            // For now, trusting Handshake/DB is safer for IP/Port stability.
        }
    }
}