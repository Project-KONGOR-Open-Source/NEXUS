namespace KINESIS.Server;

public class ServerStatusChecker
{
    public delegate void Callback(bool success);
    public static ServerStatusChecker Instance { get; private set; } = null!;

    private record Request(byte[] PayloadBytes, Callback Callback);

    private Socket? _socket;
    private readonly byte[] _receiveBuffer = new byte[3];
    private readonly Dictionary<ushort, Request> _requests = new();
    private ushort _requestId;

    public ServerStatusChecker()
    {
        Instance = this;
    }

    public void Start()
    {
        new Thread(Operate).Start();
    }

    public void EnqueueRequest(string address, short port, Callback callback)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        long ipAddress = IPAddress.Parse(address).Address;
#pragma warning restore CS0618 // Type or member is obsolete
        byte[] data = new byte[8];

        ulong portShifted = (ulong)port << 32;
        lock (this)
        {
            ++_requestId;

            ulong requestIdShifted = (ulong)_requestId << 48;

            // ipAddress is long but only lower 4 bytes are set for IPv4 so it's safe to reuse upper bits.
            // For IPv6, IPAddress.Address throws an Exception so we would not get here.
            ulong payload = (ulong)ipAddress | portShifted | requestIdShifted;

            byte[] payloadBytes = new byte[8];
            BitConverter.TryWriteBytes(payloadBytes, payload);

            _requests[_requestId] = new Request(payloadBytes, callback);

            if (_socket != null)
            {
                if (_requests.Count > 100)
                {
                    Console.WriteLine("Something is terribly wrong, ServerStatusChecker has a huge backlog of {0} requests.", _requests.Count);
                }

                try
                {
                    _socket.Send(payloadBytes);
                }
                catch
                {
                    // That's okay, we added request to the _requests and failed Socket will
                    // re-queue all requests that haven't been processed yet.
                }
            }
        }
    }

    private void Operate()
    {
        while (true)
        {
            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(IPAddress.Loopback, 999);
            }
            catch (Exception e)
            {
                Console.WriteLine("ServerStatusChecker does not appear to be running: {0}", e.Message);
                continue;
            }

            Console.WriteLine("Connected to ServerStatusChecker.");

            lock (this)
            {
                // Now that we have connected, write all accumulated requests to Socket.
                foreach (Request request in _requests.Values)
                {
                    socket.Send(request.PayloadBytes);
                }

                // And update Socket to allow writes again.
                _socket = socket;
            }

            while (true)
            {
                try
                {
                    // Read response.
                    int bytesReceived = socket.Receive(_receiveBuffer);
                    if (bytesReceived != 3)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    lock (this)
                    {
                        _socket = null;
                    }

                    // Disconnected while trying to send or receive, that's okay.
                    Console.WriteLine("Something went wrong, did not receive a response from ServerStatusChecker: {0}", e.Message);

                    // Close existing connection.
                    socket.Close();

                    // Break from this loop to reconnect.
                    break;
                }

                ushort requestId = BitConverter.ToUInt16(_receiveBuffer, 0);
                lock (this)
                {
                    // Propagate the result.
                    if (_requests.Remove(requestId, out var request))
                    {
                        request.Callback(_receiveBuffer[2] != 0);
                    }
                    else
                    {
                        Console.WriteLine("Couldn't find the request for the id {0}", requestId);
                    }
                }

                // Move to the next Request.
                continue;
            }
        }
    }
}
