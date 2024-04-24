namespace KONGOR.MasterServer.Models.RequestResponse.ServerManagement;

public class ServerForCreateListResponse : ServerListResponse
{
    public ServerForCreateListResponse(/* List<Server> servers, */ string? region)
    {
        // TODO: Retrieve Servers From Distributed Cache

        //Servers = new Dictionary<int, ServerForCreate>();

        //if (servers.Any() is false) return;

        //foreach (Server server in servers.Where(entry => entry.ServerStatus.Equals(ChatServerProtocol.ServerStatus.Idle)))
        //    Servers.Add(server.ServerId, new ServerForCreate(server.ServerId.ToString(), server.Address, server.Port.ToString(), server.Location));

        // TODO: Filter Server List By Region (+ Add Support For NEWERTH Region)

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
    }

    [PhpProperty("server_list")]
    public Dictionary<int, ServerForCreate> Servers { get; set; } = [];
}

public class ServerForJoinListResponse : ServerListResponse
{
    public ServerForJoinListResponse(/* List<Server> servers */)
    {
        // TODO: Retrieve Servers From Distributed Cache

        //Servers = new Dictionary<int, ServerForJoin>();

        //if (servers.Any() is false) return;

        //foreach (Server server in servers.Where(entry => entry.ServerStatus.Equals(ChatServerProtocol.ServerStatus.Active)))
        //    Servers.Add(server.ServerId, new ServerForJoin(server.ServerId.ToString(), server.Address, server.Port.ToString(), server.Location));

        // TODO: Filter Server List By Game Phase

        /*

            return GamePhase switch
            {
                0   => "Ready to host",
                1   => "Lobby",
                2   => "Banning",
                3   => "Hero Select",
                4   => GetMatchDuration(MatchDuration),
                5   => GetMatchDuration(MatchDuration),
                6   => GetMatchDuration(MatchDuration),
                7   => "Finished",
                8   => "Blind Banning",
                9   => "Locking",
                10  => "Lock Picking",
                11  => "Shuffle Picking",
                _   => "0:00:00"
            };

         */
    }

    [PhpProperty("server_list")]
    public Dictionary<int, ServerForJoin> Servers { get; set; } = [];
}

public abstract class ServerListResponse
{
    // TODO: Handle Account Key And Account Key Hash

    [PhpProperty("acc_key")]
    public required string AccountKey { get; set; }

    [PhpProperty("acc_key_hash")]
    public required string AccountKeyHash { get; set; }

    [PhpProperty("vested_threshold")]
    public int VestedThreshold { get; set; } = 5;

    [PhpProperty(0)]
    public bool Zero { get; set; } = true;
}

public class ServerForCreate(string id, string ip, string port, string location) : ServerForResponse(id, ip, port, location)
{
    [PhpProperty("c_state")]
    public string Category { get; set; } = "1";
}

public class ServerForJoin(string id, string ip, string port, string location) : ServerForResponse(id, ip, port, location)
{
    [PhpProperty("class")]
    public string Category { get; set; } = "1";
}

public abstract class ServerForResponse(string id, string ip, string port, string location)
{
    [PhpProperty("server_id")]
    public string ID { get; set; } = id;

    [PhpProperty("ip")]
    public string IPAddress { get; set; } = ip;

    [PhpProperty("port")]
    public string Port { get; set; } = port;

    [PhpProperty("location")]
    public string Location { get; set; } = location;
}
