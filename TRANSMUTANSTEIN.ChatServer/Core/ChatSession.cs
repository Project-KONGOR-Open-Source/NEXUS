namespace TRANSMUTANSTEIN.ChatServer.Core;

public class ChatSession(TCPServer server) : TCPSession(server)
{
    protected override void OnConnected()
    {
        Console.WriteLine($"Chat Session ID {Id} Was Created");
    }

    protected override void OnDisconnected()
    {
        Console.WriteLine($"Chat Session ID {Id} Has Terminated");
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        // TODO: Minimise Console Logging And Other I/O For Higher Throughput

        if (size < 2)
        {
            // TODO: Log Bug

            throw new ArgumentException("Buffer Size Is Less Than 2 Bytes");
        }

        if (size is 2)
        {
            Console.WriteLine($"Incoming Buffer Size: {buffer[0]}, {buffer[1]}");
        }

        if (size > 2)
        {
            ushort command = BitConverter.ToUInt16([buffer[0], buffer[1]]);

            Console.WriteLine($"Incoming Command: {command:X4}");

            string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);

            Console.WriteLine($"Incoming Buffer Text: {message}");

            Type? commandType = GetCommandType(command);

            if (commandType is null)
            {
                // TODO: Log Missing Command Handler
            }

            else
            {
                ICommandProcessor? commandTypeInstance = GetCommandTypeInstance(commandType);

                if (commandTypeInstance is null)
                {
                    // TODO: Log Bug
                }

                else
                {
                    commandTypeInstance.Process(this, new ChatBuffer(buffer));
                }
            }

            // TODO: Cache Command-To-Type Mapping To Reduce Reflection Overhead

            Type? GetCommandType(ushort command)
            {
                Type[] types = typeof(TRANSMUTANSTEIN).Assembly.GetTypes();

                Type? type = types
                    .SingleOrDefault(type => type.GetCustomAttribute<ChatCommandAttribute>() is not null
                        && (type.GetCustomAttribute<ChatCommandAttribute>()?.Command.Equals(command) ?? false));

                return type;
            }
            
            ICommandProcessor? GetCommandTypeInstance(Type type)
            {
                ConstructorInfo? constructor = type.GetConstructor(Type.EmptyTypes);

                object? instance = constructor?.Invoke(new object[] { });

                return instance as ICommandProcessor;
            }
        }
    }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Chat Session ID {Id} Caught A Socket Error With Code {error}");
    }
}
