namespace KONGOR.MasterServer.Models.RequestResponse.ServerManagement;

public class ServerForJoinListResponse(List<MatchServer> servers, string cookie, string? requestorIP) : ServerListResponse(cookie)
{
    [PHPProperty("server_list")]
    public Dictionary<int, ServerForJoin> Servers { get; set; } = servers.Any() is false
        ? []
        : servers.Where(server =>
                server.Status is ServerStatus.SERVER_STATUS_LOADING or ServerStatus.SERVER_STATUS_ACTIVE)
            .ToDictionary(server => server.ID,
                server => new ServerForJoin(
                    server.ID.ToString(), 
                    ResolveIP(server, requestorIP) ?? server.IPAddress, 
                    server.Port.ToString(),
                    server.Location));
}
