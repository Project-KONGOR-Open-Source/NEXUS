namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

/// <summary>
///     Stands in for the game socket of a match server, answering reconnect queries the way a watermarking match server does.
///     It reads the query from the copy that follows the watermark area, and answers in the watermarked layout.
/// </summary>
internal sealed class FakeMatchServerGameSocket : IAsyncDisposable
{
    /// <summary>
    ///     A reply for a slot which is still connected to the match, as the match server gives while the slot account plays on from another machine.
    ///     It lies midway between <see cref="MatchLockService.ConnectedSlotThreshold"/> and the largest possible reply, so that it reads as connected wherever the threshold lies.
    /// </summary>
    public const uint ConnectedSlotTimeRemaining = MatchLockService.ConnectedSlotThreshold + (uint.MaxValue - MatchLockService.ConnectedSlotThreshold) / 2;

    private const int QueryOffset = 40;

    private readonly UdpClient Socket;
    private readonly CancellationTokenSource Cancellation = new ();
    private readonly Task Listener;

    private FakeMatchServerGameSocket(uint slotTimeRemaining)
    {
        SlotTimeRemaining = slotTimeRemaining;
        Socket = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        Port = ((IPEndPoint) Socket.Client.LocalEndPoint!).Port;
        Listener = Listen();
    }

    public int Port { get; }

    public uint SlotTimeRemaining { get; set; }

    public bool Answers { get; set; } = true;

    public int IgnoredRequestsRemaining { get; set; }

    public ConcurrentQueue<(int MatchID, int AccountID)> Requests { get; } = new ();

    public static FakeMatchServerGameSocket Start(uint slotTimeRemaining) => new (slotTimeRemaining);

    private async Task Listen()
    {
        try
        {
            while (Cancellation.IsCancellationRequested is false)
            {
                UdpReceiveResult received = await Socket.ReceiveAsync(Cancellation.Token);
                byte[] request = received.Buffer;

                if (request.Length < QueryOffset + 12)
                    continue;

                int matchID = BitConverter.ToInt32(request, QueryOffset + 4);
                int accountID = BitConverter.ToInt32(request, QueryOffset + 8);

                Requests.Enqueue((matchID, accountID));

                if (Answers is false)
                    continue;

                if (IgnoredRequestsRemaining > 0)
                {
                    IgnoredRequestsRemaining--;

                    continue;
                }

                byte[] response = new byte[48];
                response[43] = 0x6F;
                BitConverter.TryWriteBytes(response.AsSpan(44), SlotTimeRemaining);

                await Socket.SendAsync(response, received.RemoteEndPoint, Cancellation.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (SocketException)
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Cancellation.CancelAsync();
        Socket.Dispose();

        await Listener;

        Cancellation.Dispose();
    }
}
