namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> HandleGetUpgrades()
    {
        string? cookie = Request.Form["cookie"];
        if (string.IsNullOrEmpty(cookie)) return new UnauthorizedResult();
        
        if (string.IsNullOrEmpty(cookie)) return new UnauthorizedResult();
        
        Logger.LogInformation($"[Upgrades] HandleGetUpgrades Called. Cookie: '{cookie}'");

        string? sessionAccountName = HttpContext.Items["SessionAccountName"] as string;

        if (sessionAccountName is null)
        {
             (bool accountSessionCookieIsValid, string? cacheAccountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);
             if (accountSessionCookieIsValid) sessionAccountName = cacheAccountName;
        }

        if (sessionAccountName is null)
             return Unauthorized($@"No Session Found For Cookie ""{cookie}""");

        Account? account = await MerrickContext.Accounts
            .Include(a => a.User)
            .Include(a => a.Clan)
            .FirstOrDefaultAsync(a => a.Name == sessionAccountName);

        if (account is null) return Unauthorized($@"Account Not Found");

        Dictionary<string, object> fieldStats = new()
        {
            ["account_id"] = account.ID.ToString(),
            ["level"] = account.User.TotalLevel,
            ["level_exp"] = account.User.TotalExperience
        };

        // Construct UpgradesLookupResponse manual dictionary
        Dictionary<string, object> response = new()
        {
            ["field_stats"] = fieldStats,
            ["my_upgrades_info"] = new Dictionary<string, object>(),
            ["points"] = account.User.GoldCoins.ToString(),
            ["mmpoints"] = account.User.SilverCoins.ToString(),
            ["game_tokens"] = account.User.PlinkoTickets.ToString(),
            ["standing"] = 3,
            ["my_upgrades"] = account.User.OwnedStoreItems.Distinct().ToDictionary(item => item, _ => true),
            ["selected_upgrades"] = account.SelectedStoreItems.Distinct().ToDictionary(item => item, _ => true),
            ["0"] = false
        };

        // Note: READ ONLY uses a class that handles serialization attributes. 
        // We are using dictionary, keys must match [PhpProperty] values.
        // field_stats, my_upgrades_info, points, mmpoints, game_tokens, standing, my_upgrades, selected_upgrades. Checks out.
        
        string serializedResponse = PhpSerialization.Serialize(response);
        Logger.LogInformation($"[Upgrades] Response: {serializedResponse}");

        return Ok(serializedResponse);
    }
}
