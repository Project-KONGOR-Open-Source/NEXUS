namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

public class MatchServerManagerChatSession(TCPServer server, IServiceProvider serviceProvider) : ChatSession(server, serviceProvider)
{
    /// <summary>
    ///     Gets set after a successful match server manager handshake.
    ///     Contains metadata about the match server manager connected to this chat session.
    /// </summary>
    public MatchServerManagerChatSessionMetadata Metadata { get; set; } = null!;

    /// <summary>
    ///     Gets set after a successful match server manager handshake.
    ///     Contains the account information of the match server manager connected to this chat session.
    /// </summary>
    public Account Account { get; set; } = null!;

    public async Task Terminate(IDatabase distributedCacheStore)
    {
        // Remove Match Server Manager From The Distributed Cache
        await distributedCacheStore.RemoveMatchServerManagerByID(Metadata.ServerManagerID);

        // Remove The Match Server Manager Chat Session
        if (Context.MatchServerManagerChatSessions.TryRemove(Metadata.ServerManagerID, out MatchServerManagerChatSession? existingSession))
        {
            Log.Information(@"Match Server Manager ID ""{ServerManagerID}"" Was Removed From The Match Server Manager Pool", Metadata.ServerManagerID);

            if (existingSession is null)
            {
                Log.Warning(@"Match Server Manager ID ""{ServerManagerID}"" Had A Null Session In The Match Server Manager Pool", Metadata.ServerManagerID);

                // Disconnect And Dispose The Chat Session
                Disconnect(); Dispose();

                return;
            }

            if (existingSession.Metadata.SessionCookie != Metadata.SessionCookie)
            {
                Log.Warning(@"Match Server Manager ID ""{ServerManagerID}"" Had A Mismatched Session Cookie", Metadata.ServerManagerID);

                // Disconnect And Dispose The Chat Session
                Disconnect(); Dispose();

                return;
            }

            // TODO: Check For Orphaned Match Servers And Handle Appropriately

            ChatBuffer acknowledgementResponse = new ();

            acknowledgementResponse.WriteCommand(ChatProtocol.ChatServerToServerManager.NET_CHAT_SM_REMOTE_COMMAND);
            acknowledgementResponse.WriteString(Metadata.SessionCookie);
            acknowledgementResponse.WriteString("quit");

            // Send Disconnect Acknowledgement To Match Server Manager
            Send(acknowledgementResponse);
        }

        else
        {
            Log.Warning(@"Match Server Manager ID ""{ServerManagerID}"" Attempted To Disconnect But Was Not Found In The Match Server Manager Pool", Metadata.ServerManagerID);
        }

        // Disconnect And Dispose The Chat Session
        Disconnect(); Dispose();

        Log.Information(@"Match Server Manager ID ""{ServerManagerID}"" Has Disconnected Gracefully", Metadata.ServerManagerID);
    }
}
