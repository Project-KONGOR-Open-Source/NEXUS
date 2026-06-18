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
    ///     Reads one length-prefixed frame and returns the command identifier from the front of its payload.
    /// </summary>
    public static async Task<ushort> ReadCommand(NetworkStream stream, CancellationToken cancellationToken)
    {
        byte[] header = await ReadExactly(stream, 2, cancellationToken);
        ushort length = BitConverter.ToUInt16(header, 0);

        byte[] payload = await ReadExactly(stream, length, cancellationToken);

        return BitConverter.ToUInt16(payload, 0);
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
