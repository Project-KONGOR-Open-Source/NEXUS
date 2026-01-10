namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

public partial class ChatSession
{
    /// <summary>
    ///     Gets set after a successful match server manager handshake.
    ///     Contains metadata about the match server manager connected to this chat session.
    /// </summary>
    public MatchServerManagerChatSessionMetadata ManagerMetadata { get; set; } = null!;

    public async Task TerminateMatchServerManager(IDatabase distributedCacheStore)
    {
        await RemoveMatchServerManager(distributedCacheStore);

        // Remove The Match Server Manager Chat Session
        if (Context.MatchServerManagerChatSessions.TryRemove(ManagerMetadata.ServerManagerID, out ChatSession? existingSession))
        {
            Log.Information(@"Match Server Manager ID ""{ServerManagerID}"" Was Removed From The Match Server Manager Pool", ManagerMetadata.ServerManagerID);

            if (existingSession is null)
            {
                Log.Warning(@"Match Server Manager ID ""{ServerManagerID}"" Had A Null Session In The Match Server Manager Pool", ManagerMetadata.ServerManagerID);

                // Disconnect And Dispose The Chat Session
                Disconnect(); Dispose();

                return;
            }

            if (existingSession.ManagerMetadata.SessionCookie != ManagerMetadata.SessionCookie)
            {
                Log.Warning(@"Match Server Manager ID ""{ServerManagerID}"" Had A Mismatched Session Cookie", ManagerMetadata.ServerManagerID);

                // Disconnect And Dispose The Chat Session
                Disconnect(); Dispose();

                return;
            }

            ChatBuffer acknowledgementResponse = new ();

            acknowledgementResponse.WriteCommand(ChatProtocol.ChatServerToServerManager.NET_CHAT_SM_REMOTE_COMMAND);
            acknowledgementResponse.WriteString(ManagerMetadata.SessionCookie);
            acknowledgementResponse.WriteString("quit");

            // Send Disconnect Acknowledgement To Match Server Manager
            Send(acknowledgementResponse);
        }

        else
        {
            Log.Warning(@"Match Server Manager ID ""{ServerManagerID}"" Attempted To Disconnect But Was Not Found In The Match Server Manager Pool", ManagerMetadata.ServerManagerID);
        }

        // Disconnect And Dispose The Chat Session
        Disconnect(); Dispose();

        Log.Information(@"Match Server Manager ID ""{ServerManagerID}"" Has Disconnected Gracefully", ManagerMetadata.ServerManagerID);
    }

    private async Task RemoveMatchServerManager(IDatabase distributedCacheStore)
    {
        // Remove Match Server Manager From The Distributed Cache
        // Match Server Children Are Also Implicitly Removed From The Distributed Cache
        await distributedCacheStore.RemoveMatchServerManagerByID(ManagerMetadata.ServerManagerID);
    }
}
