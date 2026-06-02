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

    /// <summary>
    ///     Tracks whether the cleanup has already run for this session.
    ///     Required because the graceful (<see cref="Terminate"/>), dropped (<see cref="OnDisconnected"/>), and superseded (<see cref="Supersede"/>) paths can all be reached for the same session, and only one of them should take effect.
    /// </summary>
    private int CleanupCompleted;

    /// <summary>
    ///     Removes this session from the in-memory pool, but only if the pool entry still maps to this exact session.
    ///     Returns whether this session was the current pool holder, so the caller tears down the shared host state (the distributed cache entry and the hosting lease) only when this session genuinely owned it.
    ///     A session that has been superseded by a reconnecting one returns <see langword="false"/> and leaves the shared host state for its replacement.
    /// </summary>
    private bool RemoveFromPoolIfCurrentHolder()
    {
        // Use Interlocked To Guarantee The Body Runs At Most Once, Even Under Concurrent Disconnect Paths
        if (Interlocked.Exchange(ref CleanupCompleted, 1) is 1)
            return false;

        // If The Metadata Is NULL, The Handshake Never Completed And The Session Was Never Added To The Pool, So There Is Nothing To Clean Up
        if (Metadata is null)
            return false;

        // Remove The Pool Entry Only If It Still Maps To This Exact Session
        if (Context.MatchServerManagerChatSessions.TryRemove(new KeyValuePair<int, MatchServerManagerChatSession>(Metadata.ServerManagerID, this)))
        {
            Log.Information(@"Match Server Manager ID ""{MatchServerManagerID}"" Was Removed From The Match Server Manager Pool", Metadata.ServerManagerID);

            return true;
        }

        // The Pool Entry No Longer Maps To This Session, So It Was Superseded By A Reconnecting Session Which Now Owns The Shared Host State
        Log.Debug(@"Match Server Manager ID ""{MatchServerManagerID}"" Was Already Superseded And Will Not Tear Down Shared Host State", Metadata.ServerManagerID);

        return false;
    }

    /// <summary>
    ///     Tears down this session because the host is reconnecting and a replacement session is taking over its pool slot.
    ///     The socket is closed, but the shared host state (the distributed cache entry and the hosting lease) is deliberately preserved for the replacement session, because the reconnecting host reuses its session cookie and the cache entry is what validates it.
    /// </summary>
    public void Supersede()
    {
        // Mark The Cleanup As Done So The Resulting OnDisconnected Leaves The Shared Host State For The Replacement Session
        Interlocked.Exchange(ref CleanupCompleted, 1);

        // Disconnect And Dispose The Chat Session
        Disconnect(); Dispose();
    }

    public async Task Terminate(IDatabase distributedCacheStore)
    {
        // A Graceful Disconnect Is An Intentional Shutdown, So The Shared Host State Is Torn Down Immediately Rather Than Being Left To The Reconnect Or Staleness Paths
        if (RemoveFromPoolIfCurrentHolder())
        {
            // Send The Quit Command While The Socket Is Still Open
            if (IsConnected)
                SendRemoteCommand("quit");

            // Await The Distributed Cache Removal Here So That Callers Which Immediately Register A Replacement Manager Do Not Race The Removal
            await Remove(distributedCacheStore);

            Log.Information(@"Match Server Manager ID ""{MatchServerManagerID}"" Has Disconnected Gracefully", Metadata.ServerManagerID);
        }

        // Disconnect And Dispose The Chat Session
        Disconnect(); Dispose();
    }

    /// <summary>
    ///     Invoked by the TCP transport after the underlying socket has been closed (graceful disconnect, network drop, crash, or keep-alive timeout).
    ///     Removes the in-memory session on every path, and tears down the distributed cache entry and releases the hosting lease only when this session was still the current holder (i.e. it was not superseded by a reconnect).
    /// </summary>
    protected override void OnDisconnected()
    {
        if (RemoveFromPoolIfCurrentHolder())
        {
            // The Disconnect Path Is Synchronous And Must Not Block, So The Distributed Cache Removal Is Fire-And-Forget
            _ = Task.Run(async () =>
            {
                try
                {
                    // IDatabase Is Registered As A Singleton And Can Be Resolved Directly From The Root Service Provider Without A Scope
                    await Remove(ServiceProvider.GetRequiredService<IDatabase>());
                }

                catch (Exception exception)
                {
                    Log.Error(exception, @"Failed To Remove Match Server Manager ID ""{MatchServerManagerID}"" From The Distributed Cache During Cleanup", Metadata.ServerManagerID);
                }
            });
        }

        base.OnDisconnected();
    }

    private async Task Remove(IDatabase distributedCacheStore)
    {
        // Remove Match Server Manager From The Distributed Cache
        // Match Server Children Are Also Implicitly Removed From The Distributed Cache
        await distributedCacheStore.RemoveMatchServerManagerByID(Metadata.ServerManagerID);

        // Release The Single-Holder Hosting Lease
        // The Match Server Manager Session Genuinely Ending (Graceful Shutdown Or Crash, Not A Reconnect) Is The Authoritative "Host Gone" Signal
        if (Account is not null)
            await distributedCacheStore.ReleaseHostLease(Account.Name);
    }
}
