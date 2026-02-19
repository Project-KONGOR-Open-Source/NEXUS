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

    /// <summary>
    ///     Completes the connection process by sending the accept packet.
    /// </summary>
    public MatchServerChatSession SetOnline()
    {
        Metadata.LastStatusUpdate = DateTimeOffset.UtcNow;

        ChatBuffer accept = new ();

        accept.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_ACCEPT);

        Send(accept);

        return this;
    }

    /// <summary>
    ///     Sends server configuration options to the match server.
    /// </summary>
    /// <param name="remoteCommands">Additional remote commands to execute on the server.</param>
    public MatchServerChatSession SendOptions(IEnumerable<string>? remoteCommands = null)
    {
        ChatBuffer options = new ();

        options.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_OPTIONS);

        // Match ID Required - Whether The Server Must Request A Match ID From The Master Server
        options.WriteBool(true);

        // Match ID Request Attempts - Number Of Times The Server Will Attempt To Request A Match ID
        options.WriteInt32(3);

        // Match ID Request Total Timeout - Milliseconds Before All Match ID Requests Time Out
        options.WriteInt32(15000);

        // Match ID Request Interval - Milliseconds Between Match ID Request Attempts
        options.WriteInt32(500);

        // Auth Request Attempts - Number Of Times The Server Will Attempt To Authenticate A Client
        options.WriteInt32(3);

        // Auth Request Total Timeout - Milliseconds Before All Auth Requests Time Out
        options.WriteInt32(30000);

        // Auth Request Interval - Milliseconds Between Auth Request Attempts
        options.WriteInt32(10000);

        // Submit Stats - Whether The Server Should Submit Statistics
        options.WriteBool(true);

        // Upload Replays - Whether The Server Should Upload Replays
        options.WriteBool(true);

        // Upload To FTP - Whether Replays Should Be Uploaded To FTP
        options.WriteBool(false);

        // Upload To S3 - Whether Replays Should Be Uploaded To S3
        options.WriteBool(true);

        // Max Connection Attempts - Maximum Client Connection Attempts
        options.WriteInt32(10);

        // Max Incoming Packets Per Second
        options.WriteInt32(1000);

        // Max Incoming Bytes Per Second
        options.WriteInt32(65536);

        // Submit Match Stat Disconnects - Whether To Track Disconnect Statistics
        options.WriteBool(true);

        // Heartbeat Interval - Milliseconds Between Status Updates (Minimum 60000)
        options.WriteInt32(60000);

        // Disconnect Reporting - Whether To Report Disconnect Reasons
        options.WriteBool(true);

        // Quests Availability - Whether Quests Are Enabled
        options.WriteInt8(Convert.ToByte(ChatProtocol.QuestsAvailabilityType.EQAT_ENABLED));

        // Quests Ladder Availability - Whether Quest Leaderboards Are Enabled
        options.WriteInt8(Convert.ToByte(ChatProtocol.QuestsAvailabilityType.EQAT_ENABLED));

        // Log Product Usage - Whether To Log Product Usage Statistics
        options.WriteBool(true);

        // Dynamic Concede Times - Whether Concede Times Are Dynamic Based On Match State
        options.WriteBool(true);

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
    ///     Sends a remote command to the match server for execution.
    /// </summary>
    /// <param name="command">The command string to execute.</param>
    public MatchServerChatSession SendRemoteCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return this;

        ChatBuffer remoteCommand = new ();

        remoteCommand.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REMOTE_COMMAND);
        remoteCommand.WriteString(Metadata.SessionCookie);
        remoteCommand.WriteString(command);

        Send(remoteCommand);

        return this;
    }

    /// <summary>
    ///     Sends multiple remote commands to the match server.
    /// </summary>
    /// <param name="commands">The commands to execute.</param>
    public MatchServerChatSession SendRemoteCommands(IEnumerable<string> commands)
    {
        foreach (string command in commands)
            SendRemoteCommand(command);

        return this;
    }

    /// <summary>
    ///     Updates the match server's status from a status update packet.
    /// </summary>
    public MatchServerChatSession UpdateStatus(ChatProtocol.ServerStatus status)
    {
        Metadata.Status = status;
        Metadata.LastStatusUpdate = DateTimeOffset.UtcNow;

        return this;
    }

    /// <summary>
    ///     Marks this server as recently used to prevent immediate reuse.
    /// </summary>
    /// <param name="expirationSeconds">Number of seconds before the server can be reused.</param>
    public MatchServerChatSession SetRecentlyUsed(int expirationSeconds = 30)
    {
        Metadata.RecentlyUsedExpiration = DateTimeOffset.UtcNow.AddSeconds(expirationSeconds);

        return this;
    }

    /// <summary>
    ///     Checks if this server was recently used and should not be selected for new matches.
    /// </summary>
    public bool WasRecentlyUsed()
    {
        if (Metadata.RecentlyUsedExpiration is null)
            return false;

        return DateTimeOffset.UtcNow < Metadata.RecentlyUsedExpiration;
    }

    /// <summary>
    ///     Clears the recently used expiration flag.
    /// </summary>
    public MatchServerChatSession ClearRecentlyUsed()
    {
        Metadata.RecentlyUsedExpiration = null;

        return this;
    }

    /// <summary>
    ///     Sends a request to create a match on this server.
    /// </summary>
    /// <param name="matchPacket">The match creation packet to send.</param>
    public MatchServerChatSession CreateMatch(ChatBuffer matchPacket)
    {
        Send(matchPacket);
        SetRecentlyUsed();

        return this;
    }

    /// <summary>
    ///     Sends a request to end the current match on this server.
    /// </summary>
    /// <param name="losingTeam">The team that lost (0 or 1, or -1 for draw/abort).</param>
    public MatchServerChatSession EndMatch(int losingTeam = -1)
    {
        ChatBuffer endMatch = new ();

        endMatch.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_END_MATCH);
        endMatch.WriteInt32(losingTeam);

        Send(endMatch);

        return this;
    }

    /// <summary>
    ///     Checks if this server is available for a new match.
    /// </summary>
    public bool IsAvailable()
    {
        return Metadata.Status is ChatProtocol.ServerStatus.SERVER_STATUS_IDLE && WasRecentlyUsed() is false;
    }

    /// <summary>
    ///     Gets the full server name including location.
    /// </summary>
    public string GetFullServerName()
    {
        return $"{Metadata.Location ?? "Unknown"} - {Metadata.Name ?? $"Server {Metadata.ServerID}"}";
    }

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

            // Send Quit Command To Match Server
            SendRemoteCommand("quit");
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
