namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Results;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_STAT_SUBMISSION_RESULT)]
public class StatSubmissionResult : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        // 0x0514
        // Just consume the buffer safely for now to prevent "Missing Type Mapping" errors.
        // Based on logs, payload seems to contain result code and match ID.
        
        try 
        {
            byte[] commandBytes = buffer.ReadCommandBytes();

            // Attempt to read generic data if available
            if (buffer.HasRemainingData())
            {
               // We don't strictly need to do anything with this result yet, 
               // as the Master Server handles the actual submission logic.
               // This packet is just the Game Server reporting back to Chat Server (or vice versa in some flows).
            }
        }
        catch (Exception ex)
        {
             // Log but don't crash
             Log.Warning(ex, "Error parsing StatSubmissionResult packet");
        }
    }
}

