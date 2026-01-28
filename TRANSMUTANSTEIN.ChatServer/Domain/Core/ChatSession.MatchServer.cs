namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

public partial class ChatSession
{
    /// <summary>
    ///     Gets set after a successful match server handshake.
    ///     Contains metadata about the match server connected to this chat session.
    /// </summary>
    public MatchServerChatSessionMetadata ServerMetadata { get; set; } = null!;

    public async Task TerminateMatchServer(IDatabase distributedCacheStore)
    {
        await RemoveMatchServer(distributedCacheStore);

        // Remove The Match Server Chat Session
        if (_chatContext.MatchServerChatSessions.TryRemove(ServerMetadata.ServerID, out ChatSession? existingSession))
        {
            Log.Information(@"Match Server ID ""{ServerID}"" Was Removed From The Match Server Pool",
                ServerMetadata.ServerID);

            if (existingSession.ServerMetadata.SessionCookie != ServerMetadata.SessionCookie)
            {
                Log.Warning(@"Match Server ID ""{ServerID}"" Had A Mismatched Session Cookie", ServerMetadata.ServerID);

                // Disconnect And Dispose The Chat Session
                Disconnect();
                Dispose();

                return;
            }

            ChatBuffer acknowledgementResponse = new();

            acknowledgementResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REMOTE_COMMAND);
            acknowledgementResponse.WriteString(ServerMetadata.SessionCookie);
            acknowledgementResponse.WriteString("quit");

            // Send Disconnect Acknowledgement To Match Server
            Send(acknowledgementResponse);
        }

        else
        {
            Log.Warning(
                @"Match Server ID ""{ServerID}"" Attempted To Disconnect But Was Not Found In The Match Server Pool",
                ServerMetadata.ServerID);
        }

        // Disconnect And Dispose The Chat Session
        Disconnect();
        Dispose();

        Log.Information(@"Match Server ID ""{ServerID}"" Has Disconnected Gracefully", ServerMetadata.ServerID);
    }

    private async Task RemoveMatchServer(IDatabase distributedCacheStore)
    {
        // Remove Match Server From The Distributed Cache
        await distributedCacheStore.RemoveMatchServerByID(ServerMetadata.ServerID);
    }
}