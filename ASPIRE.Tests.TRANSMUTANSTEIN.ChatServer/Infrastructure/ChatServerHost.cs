namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

/// <summary>
///     Boots the real TRANSMUTANSTEIN chat server host on ephemeral ports against the shared SQL Server and Redis containers, so that integration tests drive both the HTTP surface and the TCP chat endpoints through the production wiring.
///     The host reads its ports and gateway from environment variables and its connection strings from configuration, so those are supplied here in the same shapes the production host expects.
/// </summary>
internal sealed class ChatServerHost : IAsyncDisposable
{
    private WebApplication Application { get; }

    /// <summary>
    ///     The base address of the host's HTTP surface, used to reach the health endpoints.
    /// </summary>
    public Uri HTTPBaseAddress { get; }

    /// <summary>
    ///     The port serving game client connections.
    /// </summary>
    public int ClientPort { get; }

    /// <summary>
    ///     The port serving match server connections.
    /// </summary>
    public int MatchServerPort { get; }

    /// <summary>
    ///     The port serving match server manager connections.
    /// </summary>
    public int MatchServerManagerPort { get; }

    private ChatServerHost(WebApplication application, Uri httpBaseAddress, int clientPort, int matchServerPort, int matchServerManagerPort)
    {
        Application = application;
        HTTPBaseAddress = httpBaseAddress;
        ClientPort = clientPort;
        MatchServerPort = matchServerPort;
        MatchServerManagerPort = matchServerManagerPort;
    }

    public static async Task<ChatServerHost> StartAsync(ServiceContainerContext containerContext)
    {
        await containerContext.SQLServer.StartAsync();
        await containerContext.Redis.StartAsync();

        int httpPort = GetFreeTCPPort();
        int clientPort = GetFreeTCPPort();
        int matchServerPort = GetFreeTCPPort();
        int matchServerManagerPort = GetFreeTCPPort();

        string databaseName = $"realhost_{Guid.CreateVersion7():N}";
        string databaseConnectionString = containerContext.SQLServer.GetConnectionString(databaseName);
        string cacheConnectionString = containerContext.Redis.ConnectionString;

        // The Host Reads These Specific Settings Via Environment.GetEnvironmentVariable Rather Than IConfiguration, So They Must Be Real Process Environment Variables
        Environment.SetEnvironmentVariable("CHAT_SERVER_PORT_CLIENT", clientPort.ToString());
        Environment.SetEnvironmentVariable("CHAT_SERVER_PORT_MATCH_SERVER", matchServerPort.ToString());
        Environment.SetEnvironmentVariable("CHAT_SERVER_PORT_MATCH_SERVER_MANAGER", matchServerManagerPort.ToString());
        Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://127.0.0.1:{httpPort}");
        Environment.SetEnvironmentVariable("INFRASTRUCTURE_GATEWAY", "localhost");

        // The Connection Strings Are Read Through IConfiguration, So They Are Supplied As Command-Line Arguments
        string[] arguments =
        [
            "--environment", "Development",
            $"--ConnectionStrings:MERRICK={databaseConnectionString}",
            $"--ConnectionStrings:DISTRIBUTED-CACHE={cacheConnectionString}"
        ];

        WebApplication application = ChatServerApplication.CreateApplication(arguments);

        // Ensure The Target Database Exists And Is Migrated Before The Host Begins Serving
        using (IServiceScope scope = application.Services.CreateScope())
        {
            MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

            await databaseContext.Database.MigrateAsync();
        }

        await application.StartAsync();

        return new ChatServerHost(application, new Uri($"http://127.0.0.1:{httpPort}"), clientPort, matchServerPort, matchServerManagerPort);
    }

    public HttpClient CreateHTTPClient() => new () { BaseAddress = HTTPBaseAddress };

    /// <summary>
    ///     The host's root service provider, used by tests to seed the database and distributed cache.
    /// </summary>
    public IServiceProvider Services => Application.Services;

    public async Task<TcpClient> ConnectClientAsync()
    {
        TcpClient client = new ();

        await client.ConnectAsync(IPAddress.Loopback, ClientPort);

        return client;
    }

    public async Task<TcpClient> ConnectMatchServerAsync()
    {
        TcpClient client = new ();

        await client.ConnectAsync(IPAddress.Loopback, MatchServerPort);

        return client;
    }

    public async ValueTask DisposeAsync()
    {
        await Application.StopAsync();
        await Application.DisposeAsync();
    }

    private static int GetFreeTCPPort()
    {
        TcpListener listener = new (IPAddress.Loopback, 0);

        listener.Start();

        int port = ((IPEndPoint) listener.LocalEndpoint).Port;

        listener.Stop();

        return port;
    }
}
