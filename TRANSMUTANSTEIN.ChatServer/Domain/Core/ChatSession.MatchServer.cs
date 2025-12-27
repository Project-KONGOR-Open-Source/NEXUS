namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

public class MatchServerChatSession(TCPServer server, IServiceProvider serviceProvider) : ChatSession(server, serviceProvider)
{
    /// <summary>
    ///     Gets set after a successful match server handshake.
    ///     Contains metadata about the match server connected to this chat session.
    /// </summary>
    public MatchServerChatSessionMetadata Metadata { get; set; } = null!;

    /// <summary>
    ///     Gets set after a successful match server handshake.
    ///     Contains the account information of the match server connected to this chat session.
    /// </summary>
    public Account Account { get; set; } = null!;

    public async Task Terminate(IDatabase distributedCacheStore)
    {
        await Remove(distributedCacheStore);

        // Remove The Match Server Chat Session
        if (Context.MatchServerChatSessions.TryRemove(Metadata.ServerID, out MatchServerChatSession? existingSession))
        {
            Log.Information(@"Match Server ID ""{ServerID}"" Was Removed From The Match Server Pool", Metadata.ServerID);

            if (existingSession is null)
            {
                Log.Warning(@"Match Server ID ""{ServerID}"" Had A Null Session In The Match Server Pool", Metadata.ServerID);

                // Disconnect And Dispose The Chat Session
                Disconnect(); Dispose();

                return;
            }

            if (existingSession.Metadata.SessionCookie != Metadata.SessionCookie)
            {
                Log.Warning(@"Match Server ID ""{ServerID}"" Had A Mismatched Session Cookie", Metadata.ServerID);

                // Disconnect And Dispose The Chat Session
                Disconnect(); Dispose();

                return;
            }

            ChatBuffer acknowledgementResponse = new ();

            acknowledgementResponse.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REMOTE_COMMAND);
            acknowledgementResponse.WriteString(Metadata.SessionCookie);
            acknowledgementResponse.WriteString("quit");

            // Send Disconnect Acknowledgement To Match Server
            Send(acknowledgementResponse);
        }

        else
        {
            Log.Warning(@"Match Server ID ""{ServerID}"" Attempted To Disconnect But Was Not Found In The Match Server Pool", Metadata.ServerID);
        }

        // Disconnect And Dispose The Chat Session
        Disconnect(); Dispose();

        Log.Information(@"Match Server ID ""{ServerID}"" Has Disconnected Gracefully", Metadata.ServerID);
    }

    private async Task Remove(IDatabase distributedCacheStore)
    {
        // Remove Match Server From The Distributed Cache
        await distributedCacheStore.RemoveMatchServerByID(Metadata.ServerID);
    }
}
