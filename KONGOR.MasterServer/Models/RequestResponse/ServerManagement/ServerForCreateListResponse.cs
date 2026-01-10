namespace KONGOR.MasterServer.Models.RequestResponse.ServerManagement;

public class ServerForCreateListResponse(List<MatchServer> servers, string cookie)
    : ServerListResponse(cookie)

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

    [PhpProperty("server_list")]
    public Dictionary<int, ServerForCreate> Servers { get; set; } = servers.Any() is false
        ? []
        : servers.Where(server =>
                server.Status is ServerStatus.SERVER_STATUS_SLEEPING or ServerStatus.SERVER_STATUS_IDLE)
            .ToDictionary(server => server.ID,
                server => new ServerForCreate(server.ID.ToString(), server.IPAddress, server.Port.ToString(),
                    server.Location));
}