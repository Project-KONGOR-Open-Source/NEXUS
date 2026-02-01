namespace KONGOR.MasterServer.Models.RequestResponse.ServerManagement;

public class ServerForCreateListResponse(List<MatchServer> servers, List<MatchServerManager> serverManagers, string? region, string cookie) : ServerListResponse(cookie)
{
       // TODO: Filter Server List By Region(+Add Support For NEWERTH Region)

        /*

            switch (region)
               {
                   case null:
                       // Blank region indicates ALL servers.
                       lock (SharedContext.IdleServersByRegion)
                       {
                           foreach (KeyValuePair<string, HashSet<ConnectedServer>> entry in SharedContext.IdleServersByRegion)
                           {
                               AddServers(entry.Value);
                           }
                       }
                       break;
                   case "US":
                       // US combines USE and USW
                       lock (SharedContext.IdleServersByRegion)
                       {
                           if (SharedContext.IdleServersByRegion.TryGetValue("USE", out HashSet<ConnectedServer>? useServers))
                           {
                               AddServers(useServers);
                           }
                           if (SharedContext.IdleServersByRegion.TryGetValue("USW", out HashSet<ConnectedServer>? uswServers))
                           {
                               AddServers(uswServers);
                           }
                       }
                       break;
                   default:
                       // Only use a specific region.
                       lock (SharedContext.IdleServersByRegion)
                       {
                           if (SharedContext.IdleServersByRegion.TryGetValue(region, out HashSet<ConnectedServer>? regionSpecificServers))
                           {
                               AddServers(regionSpecificServers);
                           }
                       }
                       break;
               }

         */

    /// <summary>
    ///     A dictionary of available match servers on which new matches can be created, keyed by server ID.
    ///     Only includes servers with status <see cref="ServerStatus.SERVER_STATUS_SLEEPING"/> or <see cref="ServerStatus.SERVER_STATUS_IDLE"/>.
    ///     The IP address is resolved from the server manager's connection rather than the server's reported IP,
    ///     which handles NAT/proxy scenarios where servers report their internal IP but are accessed via the manager's public IP.
    /// </summary>
    [PHPProperty("server_list")]
    public Dictionary<int, ServerForCreate> Servers { get; set; } = servers.Any() is false ? []
        : servers.Where(server => server.Status is ServerStatus.SERVER_STATUS_SLEEPING or ServerStatus.SERVER_STATUS_IDLE)
            .ToDictionary(server => server.ID, server => new ServerForCreate(server.ID.ToString(),
                serverManagers.Single(manager => manager.ID == server.MatchServerManagerID).IPAddress, server.Port.ToString(), server.Location));
}

public class ServerForJoinListResponse(List<MatchServer> servers, List<MatchServerManager> serverManagers, string cookie) : ServerListResponse(cookie)
{
    /// <summary>
    ///     A dictionary of match servers with active or loading matches that can be joined, keyed by server ID.
    ///     Only includes servers with status <see cref="ServerStatus.SERVER_STATUS_LOADING"/> or <see cref="ServerStatus.SERVER_STATUS_ACTIVE"/>.
    ///     The IP address is resolved from the server manager's connection rather than the server's reported IP,
    ///     which handles NAT/proxy scenarios where servers report their internal IP but are accessed via the manager's public IP.
    /// </summary>
    [PHPProperty("server_list")]
    public Dictionary<int, ServerForJoin> Servers { get; set; } = servers.Any() is false ? []
        : servers.Where(server => server.Status is ServerStatus.SERVER_STATUS_LOADING or ServerStatus.SERVER_STATUS_ACTIVE)
            .ToDictionary(server => server.ID, server => new ServerForJoin(server.ID.ToString(),
                serverManagers.Single(manager => manager.ID == server.MatchServerManagerID).IPAddress, server.Port.ToString(), server.Location));
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
