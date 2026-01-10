namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

/// <summary>
///     Manages three separate TCP servers for handling different types of connections.
///     ClientChatServer for game clients.
///     MatchServerChatServer for match servers.
///     MatchServerManagerChatServer for match server managers.
/// </summary>
public class ChatServer(
    IServiceProvider serviceProvider,
    IPAddress address,
    int clientPort,
    int matchServerPort,
    int matchServerManagerPort)
{
    private ClientChatServer ClientServer { get; } = new(serviceProvider, address, clientPort);
    private MatchServerChatServer MatchServer { get; } = new(serviceProvider, address, matchServerPort);

    private MatchServerManagerChatServer MatchServerManagerServer { get; } =
        new(serviceProvider, address, matchServerManagerPort);

    public bool IsStarted => ClientServer.IsStarted && MatchServer.IsStarted && MatchServerManagerServer.IsStarted;

    public bool IsAccepting =>
        ClientServer.IsAccepting && MatchServer.IsAccepting && MatchServerManagerServer.IsAccepting;

    public bool IsDisposed => ClientServer.IsDisposed && MatchServer.IsDisposed && MatchServerManagerServer.IsDisposed;

    public void Start()
    {
        ClientServer.Start();
        MatchServer.Start();
        MatchServerManagerServer.Start();

        Log.Information("The Chat Server Has Started");
    }

    public void Stop()
    {
        ClientServer.Stop();
        MatchServer.Stop();
        MatchServerManagerServer.Stop();

        Log.Information("The Chat Server Has Stopped");
    }

    public void Restart()
    {
        Stop();
        Start();

        Log.Information("The Chat Server Has Restarted");
    }

    public void DisconnectAll()
    {
        ClientServer.DisconnectAll();
        MatchServer.DisconnectAll();
        MatchServerManagerServer.DisconnectAll();

        Log.Information("All Chat Server Sessions Have Been Disconnected");
    }

    public void Dispose()
    {
        ClientServer.Dispose();
        MatchServer.Dispose();
        MatchServerManagerServer.Dispose();

        Log.Information("The Chat Server Has Been Disposed");
    }
}