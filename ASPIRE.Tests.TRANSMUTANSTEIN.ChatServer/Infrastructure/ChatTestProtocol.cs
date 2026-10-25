namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

/// <summary>
///     Helpers for speaking the chat wire protocol from integration tests: length-prefix framing, reading responses, building handshake packets, and waiting on connection state.
/// </summary>
internal static class ChatTestProtocol
{
    /// <summary>
    ///     The game client version which the matchmaking requests carry, which is the version that the client handshake reports.
    /// </summary>
    private const string ClientVersion = "4.10.1.0";

    /// <summary>
    ///     Wraps a buffer in the chat protocol's little-endian 2-byte length prefix and writes it to the stream.
    /// </summary>
    public static async Task WriteFrame(NetworkStream stream, ChatBuffer buffer)
    {
        byte[] body = buffer.Data[..(int) buffer.Size];
        byte[] frame = new byte[2 + body.Length];

        BitConverter.GetBytes((ushort) body.Length).CopyTo(frame, 0);

        body.CopyTo(frame, 2);

        await stream.WriteAsync(frame);
    }

    /// <summary>
    ///     Reads one length-prefixed frame and returns its payload (the command identifier followed by the command body).
    /// </summary>
    public static async Task<byte[]> ReadFramePayload(NetworkStream stream, CancellationToken cancellationToken)
    {
        byte[] header = await ReadExactly(stream, 2, cancellationToken);
        ushort length = BitConverter.ToUInt16(header, 0);

        return await ReadExactly(stream, length, cancellationToken);
    }

    /// <summary>
    ///     Reads one length-prefixed frame and returns the command identifier from the front of its payload.
    /// </summary>
    public static async Task<ushort> ReadCommand(NetworkStream stream, CancellationToken cancellationToken)
        => BitConverter.ToUInt16(await ReadFramePayload(stream, cancellationToken), 0);

    /// <summary>
    ///     Reads frames until a remote command frame carrying the expected command text is seen, returning whether it arrived before the connection closed or the timeout elapsed.
    /// </summary>
    public static async Task<bool> ReadUntilRemoteCommand(NetworkStream stream, string expectedCommand, TimeSpan timeout)
    {
        using CancellationTokenSource cancellation = new (timeout);

        try
        {
            while (true)
            {
                ChatBuffer frame = new (await ReadFramePayload(stream, cancellation.Token));

                ushort command = BitConverter.ToUInt16(frame.ReadCommandBytes(), 0);

                if (command != ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REMOTE_COMMAND)
                    continue;

                frame.ReadString();                        // Session Cookie

                string remoteCommand = frame.ReadString(); // Command Text

                if (remoteCommand.Equals(expectedCommand, StringComparison.Ordinal))
                    return true;
            }
        }

        catch (OperationCanceledException) { return false; }

        catch (Exception exception) when (exception is IOException or SocketException or InvalidOperationException) { return false; }
    }

    /// <summary>
    ///     Reads frames and collects the text of every remote command frame up to the first one starting with "echo ", which the server always sends last, returning whatever was collected if the connection closes or the timeout elapses first.
    /// </summary>
    public static async Task<List<string>> ReadRemoteCommandsUntilEcho(NetworkStream stream, TimeSpan timeout)
    {
        List<string> remoteCommands = [];

        using CancellationTokenSource cancellation = new (timeout);

        try
        {
            while (true)
            {
                ChatBuffer frame = new (await ReadFramePayload(stream, cancellation.Token));

                ushort command = BitConverter.ToUInt16(frame.ReadCommandBytes(), 0);

                if (command != ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_REMOTE_COMMAND)
                    continue;

                frame.ReadString();                        // Session Cookie

                string remoteCommand = frame.ReadString(); // Command Text

                remoteCommands.Add(remoteCommand);

                if (remoteCommand.StartsWith("echo ", StringComparison.Ordinal))
                    return remoteCommands;
            }
        }

        catch (OperationCanceledException) { return remoteCommands; }

        catch (Exception exception) when (exception is IOException or SocketException or InvalidOperationException) { return remoteCommands; }
    }

    /// <summary>
    ///     Reads frames until one carrying the expected command identifier is seen, returning that frame positioned immediately after the command bytes so the caller can read its fields, or <see langword="null"/> if the connection closed or the timeout elapsed first.
    /// </summary>
    public static async Task<ChatBuffer?> ReadUntilCommand(NetworkStream stream, ushort expectedCommand, TimeSpan timeout)
    {
        using CancellationTokenSource cancellation = new (timeout);

        try
        {
            while (true)
            {
                ChatBuffer frame = new (await ReadFramePayload(stream, cancellation.Token));

                ushort command = BitConverter.ToUInt16(frame.ReadCommandBytes(), 0);

                if (command == expectedCommand)
                    return frame;
            }
        }

        catch (OperationCanceledException) { return null; }

        catch (Exception exception) when (exception is IOException or SocketException or InvalidOperationException) { return null; }
    }

    /// <summary>
    ///     Reads every frame in order, up to and including the first one carrying the final command, and returns the command and the frame positioned immediately after the command bytes for each, or those read before the connection closed or the timeout elapsed.
    /// </summary>
    public static async Task<List<(ushort Command, ChatBuffer Frame)>> ReadCommandsUntil(NetworkStream stream, ushort finalCommand, TimeSpan timeout)
    {
        List<(ushort Command, ChatBuffer Frame)> frames = [];

        using CancellationTokenSource cancellation = new (timeout);

        try
        {
            while (true)
            {
                ChatBuffer frame = new (await ReadFramePayload(stream, cancellation.Token));

                ushort command = BitConverter.ToUInt16(frame.ReadCommandBytes(), 0);

                frames.Add((command, frame));

                if (command == finalCommand)
                    return frames;
            }
        }

        catch (OperationCanceledException) { return frames; }

        catch (Exception exception) when (exception is IOException or SocketException or InvalidOperationException) { return frames; }
    }

    /// <summary>
    ///     Reads frames until a notice arrives, returning its sender name and message, or <see langword="null"/> if the connection closed or the timeout elapsed first.
    /// </summary>
    public static async Task<(string SenderName, string Message)?> ReadNotice(NetworkStream stream, TimeSpan timeout)
        => await ReadUntilCommand(stream, ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL, timeout) is { } frame ? ParseNotice(frame) : null;

    /// <summary>
    ///     Reads frames until an "AUTO_MATCH_CONNECT" arrives, returning its fields, or <see langword="null"/> if the connection closed or the timeout elapsed first.
    /// </summary>
    public static async Task<(byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)?> ReadAutoMatchConnect(NetworkStream stream, TimeSpan timeout)
        => await ReadUntilCommand(stream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, timeout) is { } frame ? ParseAutoMatchConnect(frame) : null;

    /// <summary>
    ///     Reads frames until a matchmaking failure reply arrives, returning its reason and seconds, or <see langword="null"/> if the connection closed or the timeout elapsed first.
    /// </summary>
    public static async Task<(ChatProtocol.TMMFailedToJoinReason Reason, uint Seconds)?> ReadMatchmakingFailure(NetworkStream stream, TimeSpan timeout)
        => await ReadUntilCommand(stream, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_FAILED_TO_JOIN, timeout) is { } frame ? ParseMatchmakingFailure(frame) : null;

    /// <summary>
    ///     Reads the sender name and message of a notice frame which is positioned immediately after its command bytes.
    /// </summary>
    public static (string SenderName, string Message) ParseNotice(ChatBuffer frame)
        => (frame.ReadString(), frame.ReadString());

    /// <summary>
    ///     Reads the fields of an "AUTO_MATCH_CONNECT" frame which is positioned immediately after its command bytes.
    ///     The random integer is -1 for a reminder.
    /// </summary>
    public static (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger) ParseAutoMatchConnect(ChatBuffer frame)
        => (frame.ReadInt8(), frame.ReadInt32(), frame.ReadString(), frame.ReadInt16(), frame.ReadInt32());

    /// <summary>
    ///     Reads the reason of a matchmaking failure reply which is positioned immediately after its command bytes, and the seconds which follow it.
    ///     The seconds are read only for <see cref="ChatProtocol.TMMFailedToJoinReason.TMMFTJR_BANNED"/>, as the game client reads them, and are zero otherwise.
    /// </summary>
    public static (ChatProtocol.TMMFailedToJoinReason Reason, uint Seconds) ParseMatchmakingFailure(ChatBuffer frame)
    {
        ChatProtocol.TMMFailedToJoinReason reason = (ChatProtocol.TMMFailedToJoinReason) frame.ReadInt8();

        return (reason, reason is ChatProtocol.TMMFailedToJoinReason.TMMFTJR_BANNED ? unchecked((uint) frame.ReadInt32()) : 0);
    }

    /// <summary>
    ///     Reads every frame which arrives within the period, and returns the messages of the match lock's notices and the random integers of the "AUTO_MATCH_CONNECT" frames among them, so that frames of either kind are counted however they interleave.
    ///     Notices from other senders are left out, because the chat server also sends notices about chat channels.
    /// </summary>
    public static async Task<(List<string> NoticeMessages, List<int> ConnectRandomIntegers)> ReadSendBackFrames(NetworkStream stream, TimeSpan period)
    {
        List<string> noticeMessages = [];
        List<int> connectRandomIntegers = [];

        using CancellationTokenSource cancellation = new (period);

        try
        {
            while (true)
            {
                ChatBuffer frame = new (await ReadFramePayload(stream, cancellation.Token));

                ushort command = BitConverter.ToUInt16(frame.ReadCommandBytes(), 0);

                if (command == ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL && ParseNotice(frame) is { SenderName: MatchLockService.MessageSenderName } notice)
                    noticeMessages.Add(notice.Message);

                else if (command == ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT)
                    connectRandomIntegers.Add(ParseAutoMatchConnect(frame).RandomInteger);
            }
        }

        catch (OperationCanceledException) { }

        return (noticeMessages, connectRandomIntegers);
    }

    /// <summary>
    ///     Reads and discards from the stream until the peer closes the connection, returning whether it closed within the timeout.
    /// </summary>
    public static async Task<bool> WaitForClose(NetworkStream stream, TimeSpan timeout)
    {
        using CancellationTokenSource cancellation = new (timeout);

        byte[] scratch = new byte[256];

        try
        {
            while (true)
            {
                int read = await stream.ReadAsync(scratch, cancellation.Token);

                if (read is 0)
                    return true;
            }
        }

        catch (OperationCanceledException) { return false; }

        catch (Exception exception) when (exception is IOException or SocketException) { return true; }
    }

    /// <summary>
    ///     Polls a condition until it holds or the timeout elapses, returning whether it became true.
    /// </summary>
    public static Task<bool> WaitUntil(Func<bool> condition, TimeSpan timeout)
        => WaitUntil(() => Task.FromResult(condition()), timeout);

    /// <summary>
    ///     Polls an asynchronous condition until it holds or the timeout elapses, returning whether it became true.
    /// </summary>
    public static async Task<bool> WaitUntil(Func<Task<bool>> condition, TimeSpan timeout)
    {
        using CancellationTokenSource cancellation = new (timeout);

        while (cancellation.IsCancellationRequested is false)
        {
            if (await condition())
                return true;

            try { await Task.Delay(TimeSpan.FromMilliseconds(50), cancellation.Token); }

            catch (OperationCanceledException) { break; }
        }

        return await condition();
    }

    public static ChatBuffer BuildClientHandshake(int accountID, string cookie, string remoteIP, string authenticationHash)
    {
        ChatBuffer buffer = new ();

        buffer.WriteCommand(ChatProtocol.ClientToChatServer.NET_CHAT_CL_CONNECT);
        buffer.WriteInt32(accountID);
        buffer.WriteString(cookie);
        buffer.WriteString(remoteIP);
        buffer.WriteString(authenticationHash);
        buffer.WriteInt32((int) ChatProtocol.CHAT_PROTOCOL_EXTERNAL_VERSION);
        buffer.WriteInt8(0);          // Operating System Identifier
        buffer.WriteInt8(0);          // Operating System Version Major
        buffer.WriteInt8(0);          // Operating System Version Minor
        buffer.WriteInt8(0);          // Operating System Version Patch
        buffer.WriteString("0");      // Operating System Build Code
        buffer.WriteString("x86_64"); // Operating System Architecture
        buffer.WriteInt8(4);          // Client Version Major
        buffer.WriteInt8(10);         // Client Version Minor
        buffer.WriteInt8(1);          // Client Version Patch
        buffer.WriteInt8(0);          // Client Version Revision
        buffer.WriteInt8(0);          // Crash Reporting Client State
        buffer.WriteInt8(0);          // Client Chat Mode State (Normal)
        buffer.WriteString("USE");    // Client Region
        buffer.WriteString("en");     // Client Language

        return buffer;
    }

    public static ChatBuffer BuildServerHandshake(int serverID, string cookie)
    {
        ChatBuffer buffer = new ();

        buffer.WriteCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_CONNECT);
        buffer.WriteInt32(serverID);
        buffer.WriteString(cookie);
        buffer.WriteInt32((int) ChatProtocol.CHAT_PROTOCOL_EXTERNAL_VERSION);

        return buffer;
    }

    public static ChatBuffer BuildAnnounceMatch(int correlationID, int matchID)
    {
        ChatBuffer buffer = new ();

        buffer.WriteCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_ANNOUNCE_MATCH);
        buffer.WriteInt32(correlationID);
        buffer.WriteInt32(0);       // Challenge
        buffer.WriteInt32(0);       // Group Count
        buffer.WriteInt32(matchID);

        return buffer;
    }

    public static ChatBuffer BuildMatchEnded(int matchupID)
    {
        ChatBuffer buffer = new ();

        buffer.WriteCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_MATCH_ENDED);
        buffer.WriteInt32(matchupID);
        buffer.WriteInt8((byte) ChatProtocol.MatchEndedReason.MATCH_ENDED_FINISHED);
        buffer.WriteInt8(1); // Winning Team
        buffer.WriteInt8(0); // Player Count

        return buffer;
    }

    public static ChatBuffer BuildAbandonMatch(bool failed)
    {
        ChatBuffer buffer = new ();

        buffer.WriteCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_ABANDON_MATCH);
        buffer.WriteBool(failed);

        return buffer;
    }

    public static ChatBuffer BuildMatchAborted(int matchupID)
    {
        ChatBuffer buffer = new ();

        buffer.WriteCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_MATCH_ABORTED);
        buffer.WriteInt32(matchupID);
        buffer.WriteInt8((byte) ChatProtocol.MatchAbortedReason.MATCH_ABORT_PLAYER_LEFT);

        return buffer;
    }

    public static ChatBuffer BuildWhisper(string targetName, string message)
    {
        ChatBuffer buffer = new ();

        buffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_WHISPER);
        buffer.WriteString(targetName);
        buffer.WriteString(message);

        return buffer;
    }

    public static ChatBuffer BuildJoinChannel(string channelName)
    {
        ChatBuffer buffer = new ();

        buffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_JOIN_CHANNEL);
        buffer.WriteString(channelName);

        return buffer;
    }

    public static ChatBuffer BuildChannelMessage(string message, int channelID)
    {
        ChatBuffer buffer = new ();

        buffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_MSG);
        buffer.WriteString(message);
        buffer.WriteInt32(channelID);

        return buffer;
    }

    public static ChatBuffer BuildJoinedGame(string gameName, int matchID, bool joinMatchChannel)
    {
        ChatBuffer buffer = new ();

        buffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_JOINED_GAME);
        buffer.WriteString(gameName);
        buffer.WriteInt32(matchID);
        buffer.WriteInt8(Convert.ToByte(joinMatchChannel));

        return buffer;
    }

    public static ChatBuffer BuildTrackPlayerAction(ChatProtocol.ActionCampaign action)
    {
        ChatBuffer buffer = new ();

        buffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_TRACK_PLAYER_ACTION);
        buffer.WriteInt8((byte) action);

        return buffer;
    }

    public static ChatBuffer BuildJoiningGame(string serverAddress)
    {
        ChatBuffer buffer = new ();

        buffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_JOINING_GAME);
        buffer.WriteString(serverAddress);

        return buffer;
    }

    public static ChatBuffer BuildLeftGame()
    {
        ChatBuffer buffer = new ();

        buffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_LEFT_GAME);

        return buffer;
    }

    public static ChatBuffer BuildGroupCreate(ChatProtocol.TMMType groupType)
    {
        ChatBuffer buffer = new ();

        buffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_CREATE);
        buffer.WriteString(ClientVersion);                                               // Client Version
        buffer.WriteInt8((byte) groupType);                                              // Group Type
        buffer.WriteInt8((byte) ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL); // Game Type
        buffer.WriteString("caldavar");                                                  // Map Name
        buffer.WriteString("ap");                                                        // Game Modes
        buffer.WriteString("USE");                                                       // Game Regions
        buffer.WriteBool(true);                                                          // Ranked
        buffer.WriteInt8(0);                                                             // Match Fidelity
        buffer.WriteInt8(1);                                                             // Bot Difficulty
        buffer.WriteBool(false);                                                         // Randomise Bots

        return buffer;
    }

    public static ChatBuffer BuildGroupJoin(string inviteIssuerName)
    {
        ChatBuffer buffer = new ();

        buffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN);
        buffer.WriteString(ClientVersion);    // Client Version
        buffer.WriteString(inviteIssuerName); // Invite Issuer Name

        return buffer;
    }

    public static ChatBuffer BuildGroupInvite(string inviteReceiverName)
    {
        ChatBuffer buffer = new ();

        buffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE);
        buffer.WriteString(inviteReceiverName); // Invite Receiver Name

        return buffer;
    }

    public static ChatBuffer BuildGroupJoinQueue()
    {
        ChatBuffer buffer = new ();

        buffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE);

        return buffer;
    }

    public static ChatBuffer BuildGroupPlayerLoadingStatus(byte loadingPercent)
    {
        ChatBuffer buffer = new ();

        buffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_LOADING_STATUS);
        buffer.WriteInt8(loadingPercent);

        return buffer;
    }

    private static async Task<byte[]> ReadExactly(NetworkStream stream, int count, CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[count];
        int offset = 0;

        while (offset < count)
        {
            int read = await stream.ReadAsync(buffer.AsMemory(offset, count - offset), cancellationToken);

            if (read is 0)
                throw new InvalidOperationException("The Connection Closed Before The Expected Bytes Arrived");

            offset += read;
        }

        return buffer;
    }
}
