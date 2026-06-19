namespace KONGOR.MasterServer.Models.RequestResponse.ServerManagement;

public class ServerForCreateListResponse(List<MatchServer> servers, string? region, string cookie) : ServerListResponse(cookie)
{
    [PHPProperty("server_list")]
    public Dictionary<int, ServerForCreate> Servers { get; set; } = servers.Any() is false ? []
        : servers.Where(server => server.Status is ServerStatus.SERVER_STATUS_IDLE)
            .Where(server => MatchesRegion(server.Location, region))
            .ToDictionary(server => server.ID, server => new ServerForCreate(server.ID.ToString(), server.IPAddress, server.Port.ToString(), server.Location));

    /// <summary>
    ///     Checks whether a server's location satisfies the requested region, comparing at the aggregate level (see <see cref="GameRegions"/>).
    ///     A blank region or the wildcard matches all servers, and a server whose location normalises to the wildcard (which includes unknown locations) appears in every list.
    /// </summary>
    private static bool MatchesRegion(string location, string? region)
    {
        if (string.IsNullOrWhiteSpace(region) || region.Equals(GameRegions.Wildcard, StringComparison.OrdinalIgnoreCase))
            return true;

        string serverAggregate = GameRegions.NormaliseServerLocation(location);

        if (serverAggregate.Equals(GameRegions.Wildcard, StringComparison.OrdinalIgnoreCase))
            return true;

        return serverAggregate.Equals(GameRegions.GetAggregate(region), StringComparison.OrdinalIgnoreCase);
    }
}

public class ServerForJoinListResponse(List<MatchServer> servers, string cookie) : ServerListResponse(cookie)
{
    [PHPProperty("server_list")]
    public Dictionary<int, ServerForJoin> Servers { get; set; } = servers.Any() is false ? []
        : servers.Where(server => server.Status is ServerStatus.SERVER_STATUS_LOADING or ServerStatus.SERVER_STATUS_ACTIVE)
            .ToDictionary(server => server.ID, server => new ServerForJoin(server.ID.ToString(), server.IPAddress, server.Port.ToString(), server.Location));
}

public abstract class ServerListResponse
{
    protected ServerListResponse(string cookie)
    {
        string key = Guid.CreateVersion7().ToString();

        AccountKey = key;
        AccountKeyHash = SRPAuthenticationHandlers.ComputeMatchServerChatAuthenticationHash(key, cookie);
    }

    [PHPProperty("acc_key")]
    public string AccountKey { get; set; }

    [PHPProperty("acc_key_hash")]
    public string AccountKeyHash { get; set; }

    [PHPProperty("vested_threshold")]
    public int VestedThreshold => 5;

    [PHPProperty(0)]
    public bool Zero => true;
}

public class ServerForCreate(string id, string ip, string port, string location) : ServerForResponse(id, ip, port, location)
{
    [PHPProperty("c_state")]
    public string Category { get; set; } = "1";
}

public class ServerForJoin(string id, string ip, string port, string location) : ServerForResponse(id, ip, port, location)
{
    [PHPProperty("class")]
    public string Category { get; set; } = "1";
}

public abstract class ServerForResponse(string id, string ip, string port, string location)
{
    [PHPProperty("server_id")]
    public string ID { get; set; } = id;

    [PHPProperty("ip")]
    public string IPAddress { get; set; } = ip;

    [PHPProperty("port")]
    public string Port { get; set; } = port;

    [PHPProperty("location")]
    public string Location { get; set; } = location;
}
