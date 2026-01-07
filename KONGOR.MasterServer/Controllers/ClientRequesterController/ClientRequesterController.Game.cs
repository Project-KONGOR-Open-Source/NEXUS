namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> HandleCreateGame()
    {
        string? name = Request.Form["name"];
        string? map = Request.Form["map"];
        string? mode = Request.Form["mode"];
        string? teamSize = Request.Form["team_size"];
        // string? spectators = Request.Form["spectators"];
        // string? referees = Request.Form["referees"];
        string? privateGame = Request.Form["private"];

        if (name is null || map is null || mode is null)
            return BadRequest(PhpSerialization.Serialize(new { error = "Missing Required Parameters" }));

        // 1. Get Available Servers
        List<MatchServer> allServers = await DistributedCache.GetMatchServers();
        
        // Filter for servers that are IDLE (ready for a game)
        // We prioritize servers that are IDLE. SLEEPING servers might need waking up (logic for another time).
        List<MatchServer> availableServers = allServers
            .Where(s => s.Status == ServerStatus.SERVER_STATUS_IDLE)
            .ToList();

        if (availableServers.Count == 0)
        {
            Logger.LogWarning("No Idle Match Servers Available. Total Servers: {Count}", allServers.Count);
            return Ok(PhpSerialization.Serialize(new { match_id = 0, error = "No Servers Available" }));
        }

        // 2. Select a Server (Random for now to distribute load)
        MatchServer selectedServer = availableServers[Random.Shared.Next(availableServers.Count)];

        // 3. Get Host Account Info
        string? accountName = HttpContext.Items["SessionAccountName"] as string;
        if (accountName is null)
             return Unauthorized(PhpSerialization.Serialize(new { error = "Unauthorized" }));

        // 4. Create Match Start Data
        MatchStartData matchStartData = new()
        {
            MatchName = name,
            ServerID = selectedServer.ID,
            HostAccountName = accountName,
            Map = map,
            MatchMode = mode,
            Version = "4.10.1", // TODO: Get from Client Headers or Config
            IsCasual = mode.Contains("casual", StringComparison.OrdinalIgnoreCase), // Heuristic
            MatchType = privateGame == "1" ? 3 : 0, // 3 = Public/Private? Need to verify MatchType enum. 0 is standard.
            Options = MatchOptions.None // TODO: Parse detailed options
        };

        // 5. Save to Redis
        // This is the CRITICAL step that was missing. 
        // ServerRequester looks for this when the server reports "Match Initialized".
        await DistributedCache.SetMatchStartData(matchStartData);

        Logger.LogInformation("Match Created: ID: {MatchID}, Server: {ServerID} ({IP}:{Port}), Host: {Account}", 
            matchStartData.MatchID, selectedServer.ID, selectedServer.IPAddress, selectedServer.Port, accountName);

        // 6. Return Response to Client
        // The client needs the match ID and the server IP/Port to connect.
        // It also likely needs a 'server_auth_hash' or similar for the handshake.
        return Ok(PhpSerialization.Serialize(new Dictionary<string, object>
        {
            { "match_id", matchStartData.MatchID },
            { "server_id", selectedServer.ID },
            { "server_address", selectedServer.IPAddress },
            { "server_port", selectedServer.Port },
            // "server_auth_hash" might be needed if the client uses it for validation.
            // For now, we return basic connection info.
            { "error", false } 
        }));
    }

    private async Task<IActionResult> HandleNewGameAvailable()
    {
        // TODO: Implement logic to notify clients of new games? 
        // Or check if a specific game is ready?
        Logger.LogWarning("[STUB] HandleNewGameAvailable called but not implemented.");
        return Ok(PhpSerialization.Serialize(true));
    }

    private async Task<IActionResult> HandleFinalMatchStats()
    {
        // TODO: This likely handles the final stats submission from the client's perspective
        // or a confirmation that the client has received them.
        Logger.LogWarning("[STUB] HandleFinalMatchStats called but not implemented.");
        return Ok(PhpSerialization.Serialize(true));
    }
}
