namespace TRANSMUTANSTEIN.ChatServer.Services;

/// <summary>
///     Asks a match server, over the UDP game socket, whether a player's slot in its match can still be reclaimed and for how long.
///     The question is the reconnect query the game client itself sends, so the match server answers it exactly as it answers a returning player.
///     The query is a command byte pair, the match ID, and the account ID, all little-endian.
///     The match server reads it at the start of the datagram, unless watermarking is enabled, in which case <c>CHostServer::ReadGamePackets</c> expects the watermark area first and <c>CEnhancedWatermark::handleReceivePacket</c> strips it before the query is handled.
///     The request therefore carries two copies of the query, one at the start and one after the watermark area, so that either kind of match server finds it where it looks.
///     The answer is either the bare reply of eight bytes or the watermarked reply of forty-eight bytes, and both end in the reconnect command byte followed by the remaining slot time as a little-endian unsigned integer, which <c>CGameServer::GetReconnectInfo</c> produces.
///     A reply which has neither shape is not a reconnect response and is ignored.
///     UDP offers no delivery guarantee, so each query is attempted a few times before the match server is considered unreachable.
/// </summary>
public class MatchServerReconnectQuery
{
    /// <summary>
    ///     The number of times the query is sent before the match server is considered unreachable.
    /// </summary>
    internal const int RequestAttempts = 3;

    /// <summary>
    ///     How long each attempt waits for an answer before the next one is sent.
    /// </summary>
    internal static readonly TimeSpan AttemptTimeout = TimeSpan.FromSeconds(1);

    private const int QueryLength = 14;
    private const int WatermarkedQueryOffset = 40;
    private const int WatermarkedResponseLength = 48;
    private const int WatermarkedResponseCommandOffset = 43;
    private const int UnwatermarkedResponseLength = 8;
    private const int UnwatermarkedResponseCommandOffset = 3;
    private const byte ReconnectQueryCommand = 0xCC;
    private const byte ReconnectResponseCommand = 0x6F;

    /// <summary>
    ///     Builds the request datagram, which holds the reconnect query both at the start and after the watermark area.
    /// </summary>
    internal static byte[] BuildRequest(int matchID, int accountID)
    {
        byte[] request = new byte[WatermarkedQueryOffset + QueryLength];

        WriteQuery(request.AsSpan(0, QueryLength), matchID, accountID);
        WriteQuery(request.AsSpan(WatermarkedQueryOffset, QueryLength), matchID, accountID);

        return request;
    }

    /// <summary>
    ///     Reads the remaining slot time from a datagram, or returns <see langword="null"/> when the datagram is not a reconnect response.
    /// </summary>
    internal static uint? ParseResponse(ReadOnlySpan<byte> datagram)
    {
        int commandOffset = datagram.Length switch
        {
            WatermarkedResponseLength => WatermarkedResponseCommandOffset,
            UnwatermarkedResponseLength => UnwatermarkedResponseCommandOffset,
            _ => -1
        };

        if (commandOffset < 0 || datagram[commandOffset] is not ReconnectResponseCommand)
            return null;

        return BitConverter.ToUInt32(datagram.Slice(commandOffset + 1, sizeof(uint)));
    }

    /// <summary>
    ///     Queries the match server for the time remaining on the account's slot in the match, in milliseconds.
    ///     Returns <see langword="null"/> when no attempt is answered, and never throws a <see cref="SocketException"/>.
    ///     Unreachability is not logged here, so that the caller can report it once per server.
    /// </summary>
    public async Task<uint?> QuerySlotTimeRemaining(IPEndPoint gameEndPoint, int matchID, int accountID, CancellationToken cancellationToken = default)
    {
        byte[] request = BuildRequest(matchID, accountID);

        using UdpClient client = new (gameEndPoint.AddressFamily);

        try
        {
            // Connecting Filters The Received Datagrams To Those Of The Match Server
            client.Connect(gameEndPoint);
        }
        catch (SocketException)
        {
            return null;
        }

        for (int attempt = 0; attempt < RequestAttempts; attempt++)
        {
            using CancellationTokenSource attemptCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            attemptCancellation.CancelAfter(AttemptTimeout);

            try
            {
                await client.SendAsync(request, attemptCancellation.Token);

                while (true)
                {
                    UdpReceiveResult received = await client.ReceiveAsync(attemptCancellation.Token);

                    uint? slotTimeRemaining = ParseResponse(received.Buffer);

                    if (slotTimeRemaining is not null)
                        return slotTimeRemaining;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested is false)
            {
                // The Attempt Timed Out, So The Next One Is Sent
            }
            catch (SocketException)
            {
                // A Refused Connection Is Reported By The Operating System When The Port Is Closed, So The Attempt Counts As Unanswered
            }
        }

        return null;
    }

    private static void WriteQuery(Span<byte> destination, int matchID, int accountID)
    {
        destination[2] = 0x01;
        destination[3] = ReconnectQueryCommand;

        BitConverter.TryWriteBytes(destination[4..], matchID);
        BitConverter.TryWriteBytes(destination[8..], accountID);
    }
}
