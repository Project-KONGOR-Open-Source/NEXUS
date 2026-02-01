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

    [PHPProperty("server_list")]
    public Dictionary<int, ServerForCreate> Servers { get; set; } = servers.Any() is false ? []
        : servers.Where(server => server.Status is ServerStatus.SERVER_STATUS_SLEEPING or ServerStatus.SERVER_STATUS_IDLE)
            .ToDictionary(server => server.ID, server => new ServerForCreate(server.ID.ToString(),
                MatchServerUtilities.ResolvePublicIPAddress(server, serverManagers.Single(manager => manager.ID == server.MatchServerManagerID)), server.Port.ToString(), server.Location));
}

public class ServerForJoinListResponse(List<MatchServer> servers, List<MatchServerManager> serverManagers, string cookie) : ServerListResponse(cookie)
{
    [PHPProperty("server_list")]
    public Dictionary<int, ServerForJoin> Servers { get; set; } = servers.Any() is false ? []
        : servers.Where(server => server.Status is ServerStatus.SERVER_STATUS_LOADING or ServerStatus.SERVER_STATUS_ACTIVE)
            .ToDictionary(server => server.ID, server => new ServerForJoin(server.ID.ToString(),
                MatchServerUtilities.ResolvePublicIPAddress(server, serverManagers.Single(manager => manager.ID == server.MatchServerManagerID)), server.Port.ToString(), server.Location));
}

file static class MatchServerUtilities
{
    // TODO: Move This To A Shared Utilities Type
    // TODO: Is This Worth It, Or Should Be Just Always Use The Server Manager's IP?

    /// <summary>
    ///     Resolves the public IP address for a match server.
    ///     If the server manager has a public IP, it is used instead of the server's reported IP.
    ///     This handles NAT/proxy scenarios where servers report their internal IP but are accessed via the manager's public IP.
    ///     For local environments (where the manager's IP is loopback), the server's reported IP is used.
    /// </summary>
    public static string ResolvePublicIPAddress(MatchServer server, MatchServerManager serverManager)
        => IsPublicIPAddress(serverManager.IPAddress) ? serverManager.IPAddress : server.IPAddress;

    public static bool IsPublicIPAddress(string ipAddress)
    {
        if (IPAddress.TryParse(ipAddress, out IPAddress? parsedAddress) is false)
            throw new FormatException($@"Unable To Parse IP Address ""{ipAddress}""");

        // Loopback Addresses: 127.x.x.x, ::1
        if (IPAddress.IsLoopback(parsedAddress))
            return false;

        byte[] addressBytes = parsedAddress.MapToIPv4().GetAddressBytes();

        // Private Ranges: 10.x.x.x, 172.16-31.x.x, 192.168.x.x
        bool isPrivate = addressBytes[0] == 10
                     || (addressBytes[0] == 172 && addressBytes[1] > 15 && addressBytes[1] < 32)
                     || (addressBytes[0] == 192 && addressBytes[1] == 168);

        return isPrivate is false;
    }
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
