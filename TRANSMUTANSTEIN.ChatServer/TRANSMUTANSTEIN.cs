namespace TRANSMUTANSTEIN.ChatServer;

public class TRANSMUTANSTEIN
{
    public static void Main(string[] args)
    {
        IPAddress address = IPAddress.Any;
        int port = 55508; // TODO: Get From Configuration

        Core.ChatServer server = new(address, port);

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
}
