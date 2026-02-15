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

    /// <summary>
    ///     Completes the connection process by sending the accept packet.
    /// </summary>
    public MatchServerManagerChatSession SetOnline()
    {
        Metadata.LastStatusUpdate = DateTimeOffset.UtcNow;

        ChatBuffer accept = new ();

        accept.WriteCommand(ChatProtocol.ChatServerToServerManager.NET_CHAT_SM_ACCEPT);

        Send(accept);

        return this;
    }

    /// <summary>
    ///     Sends server manager configuration options.
    /// </summary>
    /// <param name="remoteCommands">Additional remote commands to execute.</param>
    public MatchServerManagerChatSession SendOptions(IEnumerable<string>? remoteCommands = null)
    {
        ChatBuffer options = new ();

        options.WriteCommand(ChatProtocol.ChatServerToServerManager.NET_CHAT_SM_OPTIONS);

        // Submit Stats Enabled
        options.WriteInt8(Convert.ToByte(true));

        // Upload Replays Enabled
        options.WriteInt8(Convert.ToByte(true));

        // Upload To FTP On Demand Enabled
        options.WriteInt8(Convert.ToByte(false));

        // Upload To HTTP On Demand Enabled
        options.WriteInt8(Convert.ToByte(true));

        // Resubmit Stats Enabled
        options.WriteInt8(Convert.ToByte(true));

        // Stats Resubmit Match ID Cut-Off (Minimum Match ID For Resubmission)
        // TODO: Set To Highest Match ID Stored In The Database
        options.WriteInt32(1);

        Send(options);

        // Send Remote Commands If Provided
        if (remoteCommands is not null)
        {
            foreach (string command in remoteCommands)
                SendRemoteCommand(command);
        }

        return this;
    }

    /// <summary>
    ///     Sends a remote command to the server manager for execution.
    /// </summary>
    /// <param name="command">The command string to execute.</param>
    public MatchServerManagerChatSession SendRemoteCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return this;

        ChatBuffer remoteCommand = new ();

        remoteCommand.WriteCommand(ChatProtocol.ChatServerToServerManager.NET_CHAT_SM_REMOTE_COMMAND);
        remoteCommand.WriteString(Metadata.SessionCookie);
        remoteCommand.WriteString(command);

        Send(remoteCommand);

        return this;
    }

    /// <summary>
    ///     Sends multiple remote commands to the server manager.
    /// </summary>
    /// <param name="commands">The commands to execute.</param>
    public MatchServerManagerChatSession SendRemoteCommands(IEnumerable<string> commands)
    {
        foreach (string command in commands)
            SendRemoteCommand(command);

        return this;
    }

    /// <summary>
    ///     Registers a child match server as belonging to this manager.
    /// </summary>
    /// <param name="serverID">The match server ID.</param>
    public MatchServerManagerChatSession AddChildServer(int serverID)
    {
        Metadata.ChildServerIDs.Add(serverID);

        return this;
    }

    /// <summary>
    ///     Removes a child match server from this manager's tracking.
    /// </summary>
    /// <param name="serverID">The match server ID.</param>
    public MatchServerManagerChatSession RemoveChildServer(int serverID)
    {
        Metadata.ChildServerIDs.Remove(serverID);

        return this;
    }

    /// <summary>
    ///     Gets all child match server sessions managed by this server manager.
    /// </summary>
    /// <returns>Collection of child match server sessions.</returns>
    public IEnumerable<MatchServerChatSession> GetChildServerSessions()
    {
        foreach (int serverID in Metadata.ChildServerIDs)
        {
            if (Context.MatchServerChatSessions.TryGetValue(serverID, out MatchServerChatSession? session))
                yield return session;
        }
    }

    /// <summary>
    ///     Sends a remote command to all child match servers.
    /// </summary>
    /// <param name="command">The command string to execute on all children.</param>
    public MatchServerManagerChatSession BroadcastToChildServers(string command)
    {
        foreach (MatchServerChatSession childSession in GetChildServerSessions())
            childSession.SendRemoteCommand(command);

        return this;
    }

    /// <summary>
    ///     Sends a packet to all child match servers.
    /// </summary>
    /// <param name="buffer">The packet to send.</param>
    public MatchServerManagerChatSession BroadcastPacketToChildServers(ChatBuffer buffer)
    {
        foreach (MatchServerChatSession childSession in GetChildServerSessions())
            childSession.Send(buffer);

        return this;
    }

    /// <summary>
    ///     Requests an upload operation from the server manager.
    /// </summary>
    /// <param name="filePath">The file path to upload.</param>
    /// <param name="uploadType">The type of upload (replay, log, etc.).</param>
    public MatchServerManagerChatSession RequestUpload(string filePath, string uploadType)
    {
        ChatBuffer uploadRequest = new ();

        uploadRequest.WriteCommand(ChatProtocol.ChatServerToServerManager.NET_CHAT_SM_UPLOAD_REQUEST);
        uploadRequest.WriteString(uploadType);
        uploadRequest.WriteString(filePath);

        Send(uploadRequest);

        return this;
    }

    public async Task Terminate(IDatabase distributedCacheStore)
    {
        await Remove(distributedCacheStore);

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

            // Send Quit Command To Server Manager
            SendRemoteCommand("quit");
        }

        else
        {
            Log.Warning(@"Match Server Manager ID ""{ServerManagerID}"" Attempted To Disconnect But Was Not Found In The Match Server Manager Pool", Metadata.ServerManagerID);
        }

        // Disconnect And Dispose The Chat Session
        Disconnect(); Dispose();

        Log.Information(@"Match Server Manager ID ""{ServerManagerID}"" Has Disconnected Gracefully", Metadata.ServerManagerID);
    }

    private async Task Remove(IDatabase distributedCacheStore)
    {
        // Remove Match Server Manager From The Distributed Cache
        // Match Server Children Are Also Implicitly Removed From The Distributed Cache
        await distributedCacheStore.RemoveMatchServerManagerByID(Metadata.ServerManagerID);
    }
}
