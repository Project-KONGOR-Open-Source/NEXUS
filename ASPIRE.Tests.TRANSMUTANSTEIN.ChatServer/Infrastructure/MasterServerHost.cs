namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

/// <summary>
///     Boots the real KONGOR master server in process against the database and the distributed cache of a <see cref="ChatServerHost"/>, so that integration tests drive the master server's requests and the chat server's reactions to them together.
///     The master server reads the distributed cache without a key prefix, as the chat server does, so that both see the same records and the same channels.
///     The master server purges every session cookie when it starts, so a test starts it with <see cref="Start"/> before it seeds any session cookie.
/// </summary>
internal sealed class MasterServerHost(ChatServerHost chatServerHost) : WebApplicationFactory<KONGORAssemblyMarker>
{
    /// <summary>
    ///     Starts a master server which shares the database and the distributed cache of the chat server host.
    /// </summary>
    public static MasterServerHost Start(ChatServerHost chatServerHost)
    {
        MasterServerHost masterServerHost = new (chatServerHost);

        masterServerHost.StartServer();

        return masterServerHost;
    }

    /// <summary>
    ///     Sends the request which a match server sends to authenticate a connecting client.
    /// </summary>
    public async Task<HttpResponseMessage> ConnectClient(string session, string cookie)
    {
        HttpClient httpClient = CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"S2 Games/Heroes Of Newerth/{Distributions.Version}/wac/x86_64");

        Dictionary<string, string> formData = new () { { "session", session }, { "cookie", cookie }, { "ip", "127.0.0.1" }, { "cas", "0" }, { "new", "1" } };

        return await httpClient.PostAsync("server_requester.php?f=c_conn", new FormUrlEncodedContent(formData));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // The Master Server Reads These Via "Environment.GetEnvironmentVariable" Rather Than "IConfiguration", So "builder.UseSetting" Would Not Reach Them; The Chat Server Ones Name The Chat Server Host Which Shares This Master Server's Database And Distributed Cache
        Environment.SetEnvironmentVariable("CHAT_SERVER_HOST", "localhost");
        Environment.SetEnvironmentVariable("CHAT_SERVER_PORT_CLIENT", chatServerHost.ClientPort.ToString());
        Environment.SetEnvironmentVariable("CHAT_SERVER_PORT_MATCH_SERVER", chatServerHost.MatchServerPort.ToString());
        Environment.SetEnvironmentVariable("CHAT_SERVER_PORT_MATCH_SERVER_MANAGER", chatServerHost.MatchServerManagerPort.ToString());
        Environment.SetEnvironmentVariable("APPLICATION_URL", "http://0.0.0.0/");
        Environment.SetEnvironmentVariable("INFRASTRUCTURE_GATEWAY", "localhost");

        builder.UseEnvironment("Development");

        builder.UseSetting("ConnectionStrings:MERRICK", chatServerHost.DatabaseConnectionString);
        builder.UseSetting("ConnectionStrings:DISTRIBUTED-CACHE", chatServerHost.DistributedCacheConnectionString);
    }
}
