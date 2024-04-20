namespace TRANSMUTANSTEIN.ChatServer.Core;

public class ChatSession(TCPServer server, IServiceProvider serviceProvider) : TCPSession(server)
{
    private IServiceProvider ServiceProvider { get; set; } = serviceProvider;

    private static Dictionary<ushort, Type> CommandToTypeMap { get; set; } = [];

    protected override void OnConnected()
    {
        Console.WriteLine($"Chat Session ID {ID} Was Created");
    }

    protected override void OnDisconnected()
    {
        Console.WriteLine($"Chat Session ID {ID} Has Terminated");
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
                if (GetCommandTypeInstance(commandType) is { } commandTypeInstance)
                    commandTypeInstance.Process(this, new ChatBuffer(buffer));

                else
                {
                    Console.WriteLine($"[BUG] Unknown Command: {command:X4}");
                    // TODO: Log Bug
                }
            }

            Type? GetCommandType(ushort command)
            {
                if (CommandToTypeMap.TryGetValue(command, out Type? type))
                    return type;

                Type[] types = typeof(TRANSMUTANSTEIN).Assembly.GetTypes();

                type = types
                    .SingleOrDefault(type => type.GetCustomAttribute<ChatCommandAttribute>() is not null
                        && (type.GetCustomAttribute<ChatCommandAttribute>()?.Command.Equals(command) ?? false));

                if (type is not null)
                    CommandToTypeMap.Add(command, type);

                else
                {
                    // TODO: Log Error (Unmapped Command)
                }

                return type;
            }
            
            ICommandProcessor? GetCommandTypeInstance(Type type)
            {
                object instance = ActivatorUtilities.CreateInstance(ServiceProvider, type);

                return instance as ICommandProcessor;
            }
        }
    }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Chat Session ID {ID} Caught A Socket Error With Code {error}");
    }
}
