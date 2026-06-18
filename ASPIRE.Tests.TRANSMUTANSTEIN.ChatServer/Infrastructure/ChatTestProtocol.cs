namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

/// <summary>
///     Helpers for speaking the chat wire protocol from integration tests: length-prefix framing, reading responses, building handshake packets, and waiting on connection state.
/// </summary>
internal static class ChatTestProtocol
{
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
    public static async Task<bool> WaitUntil(Func<bool> condition, TimeSpan timeout)
    {
        using CancellationTokenSource cancellation = new (timeout);

        while (cancellation.IsCancellationRequested is false)
        {
            if (condition())
                return true;

            try { await Task.Delay(TimeSpan.FromMilliseconds(50), cancellation.Token); }

            catch (OperationCanceledException) { break; }
        }

        return condition();
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
