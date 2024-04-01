using TRANSMUTANSTEIN.ChatServer.Core;

namespace QWERTYUIOP;

public class TRANSMUTANSTEIN
{
    public static void Main(string[] args)
    {
        IPAddress address = IPAddress.Any;
        int port = 55508; // TODO: Get From Configuration

        ChatServer server = new(address, port);

        if (server.Start() is false)
        {
            // TODO: Log Critical Event

            throw new ApplicationException("Chat Server Was Unable To Start");
        }

        Console.WriteLine($"Chat Server Listening On {server.Endpoint}");

        while (server.IsStarted)
        {
            // keep listening
        }
    }

    public class ChatServer(IPAddress address, int port) : TCPServer(address, port)
    {
        protected override TCPSession CreateSession() => new ChatSession(this);

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat Server Caught A Socket Error With Code {error}");
        }
    }

    public class ChatSession(TCPServer server) : TCPSession(server)
    {
        protected override void OnConnected()
        {
            Console.WriteLine($"Chat Session ID {Id} Was Created");

            // Establish A Handshake
            ChatCommandBuffer buffer = new();
            const short accept = 0x1C00;
            buffer.WriteInt16(accept);
            SendAsync(buffer.Buffer);
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"Chat Session ID {Id} Has Terminated");
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            Console.WriteLine($"Incoming: {message}");

            // Multicast Message To All Connected Sessions
            Server.Multicast(message);

            // If The Buffer Starts With "!disconnect", Then Terminate The Current Session
            if (message == "!disconnect") Disconnect();
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat Session ID {Id} Caught A Socket Error With Code {error}");
        }
    }

    public class ChatCommandBuffer
    {
        // Based on logging, ~99% of all messages are less than 112. 112 also has a nice property that
        // it allocates exactly 128 bytes, when including byte[] object header.
        private static readonly int INITIAL_CAPACITY = 112;

        // The rest of the messages are very long (e.g. ChatChannel connects), so when we grow, grow by a lot.
        private static readonly int GROW_SIZE = 896;

        public byte[] Buffer = new byte[INITIAL_CAPACITY];
        public int Size;
        private Span<byte> Span => ((Span<byte>)Buffer).Slice(Size);

        public void WriteInt8(byte value)
        {
            if (Buffer.Length == Size)
            {
                Array.Resize(ref Buffer, Buffer.Length + GROW_SIZE);
            }
            Buffer[Size++] = value;
        }

        public void WriteInt16(short value)
        {
            if (!BitConverter.TryWriteBytes(Span, value))
            {
                Array.Resize(ref Buffer, Buffer.Length + GROW_SIZE);
                BitConverter.TryWriteBytes(Span, value);
            }
            Size += 2;
        }

        public void WriteInt32(int value)
        {
            if (!BitConverter.TryWriteBytes(Span, value))
            {
                Array.Resize(ref Buffer, Buffer.Length + GROW_SIZE);
                BitConverter.TryWriteBytes(Span, value);
            }
            Size += 4;
        }

        public void WriteInt64(long value)
        {
            if (!BitConverter.TryWriteBytes(Span, value))
            {
                Array.Resize(ref Buffer, Buffer.Length + GROW_SIZE);
                BitConverter.TryWriteBytes(Span, value);
            }
            Size += 8;
        }

        public void WriteFloat32(float value)
        {
            if (!BitConverter.TryWriteBytes(Span, value))
            {
                Array.Resize(ref Buffer, Buffer.Length + GROW_SIZE);
                BitConverter.TryWriteBytes(Span, value);
            }
            Size += 4;
        }

        public void WriteString(string value)
        {
            byte[] data = Encoding.UTF8.GetBytes(value);
            int finalWriteOffset = Size + data.Length + 1;

            if (Buffer.Length < finalWriteOffset)
            {
                Array.Resize(ref Buffer, Math.Max(Buffer.Length + GROW_SIZE, finalWriteOffset));
            }

            Array.Copy(data, 0, Buffer, Size, data.Length);
            Buffer[finalWriteOffset - 1] = 0;
            Size = finalWriteOffset;
        }
    }
}
