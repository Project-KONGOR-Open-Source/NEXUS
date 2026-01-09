namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

using Models.RequestResponse.SRP;

public partial class ClientRequesterController
{
    private async Task<IActionResult> HandleGetUpgrades()
    {
        try
        {
            string? cookie = Request.Form["cookie"];
            if (string.IsNullOrEmpty(cookie)) 
            {
                Logger.LogWarning("[Upgrades] Missing cookie.");
                return new UnauthorizedResult();
            }
            
            Logger.LogInformation($"[Upgrades] HandleGetUpgrades Called. Cookie: '{cookie}'");
    
            string? sessionAccountName = HttpContext.Items["SessionAccountName"] as string;
    
            if (sessionAccountName is null)
            {
                 Logger.LogInformation($"[Upgrades] Validating cookie via DistributedCache...");
                 (bool accountSessionCookieIsValid, string? cacheAccountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);
                 Logger.LogInformation($"[Upgrades] Validation Result: IsValid={accountSessionCookieIsValid}, Account={cacheAccountName}");
                 
                 if (accountSessionCookieIsValid) sessionAccountName = cacheAccountName;
            }
    
            if (sessionAccountName is null)
            {
                 Logger.LogWarning($"[Upgrades] Validation Failed for cookie '{cookie}'");
                 return Unauthorized($@"No Session Found For Cookie ""{cookie}""");
            }
    
            Account? account = await MerrickContext.Accounts
                .Include(a => a.User)
                .Include(a => a.Clan)
                .FirstOrDefaultAsync(a => a.Name == sessionAccountName);
    
            if (account is null) 
            {
                Logger.LogError($"[Upgrades] Account '{sessionAccountName}' not found in DB.");
                return Unauthorized($@"Account Not Found");
            }
    
            // We use CreateShowSimpleStatsResponse to fetch calculated values like DiceTokens, Levels, etc.
            // But we will NOT include the full stats payloads (season_normal, etc) in the response.
            ShowSimpleStatsResponse fullStats = await CreateShowSimpleStatsResponse(account);

            // Populate my_upgrades_info with default data for each owned item
            Dictionary<string, object> myUpgradesInfo = new();
            foreach (string item in account.User.OwnedStoreItems)
            {
                // We use default StoreItemData which implies permanent ownership (AvailableUntil = 1000 years from now)
                myUpgradesInfo[item] = new StoreItemData();
            }

            // Construct Manual Response conforming to Legacy UpgradesLookupResponse + Trap Fixes.
            Dictionary<string, object> response = new()
            {
                // Core Identity
                ["nickname"] = fullStats.NameWithClanTag,
                ["account_id"] = fullStats.ID,

                // Legacy FieldStats (Unwrapped to Root per Phase 2)
                ["level"] = fullStats.Level,
                ["level_exp"] = fullStats.LevelExperience,

                // Currency & Tokens
                ["points"] = account.User.GoldCoins.ToString(), // String
                ["mmpoints"] = account.User.SilverCoins, // Int
                ["dice_tokens"] = fullStats.DiceTokens,
                ["game_tokens"] = fullStats.GameTokens,
                
                // Inventory & Upgrades
                ["my_upgrades_info"] = myUpgradesInfo,
                ["my_upgrades"] = account.User.OwnedStoreItems.Distinct().ToList(),
                ["selected_upgrades"] = account.SelectedStoreItems,
                ["slot_id"] = fullStats.CustomIconSlotID,
                
                // Metadata
                ["timestamp"] = fullStats.ServerTimestamp,
                ["vested_threshold"] = fullStats.VestedThreshold,

                // TRAP FIXES: Required by modern client to prevent logout/crash
                ["quest_system"] = new Dictionary<string, object>
                {
                    { "error", new Dictionary<string, int>
                        {
                            { "quest_status", 0 },
                            { "leaderboard_status", 0 }
                        }
                    }
                },
                ["season_system"] = new Dictionary<string, object>
                {
                    { "drop_diamonds", 0 },
                    { "cur_diamonds", 0 },
                    { "box_price", new Dictionary<int, int>() }
                },
                ["con_reward"] = new Dictionary<string, object>
                {
                    { "old_lvl", 5 },
                    { "curr_lvl", 6 },
                    { "next_lvl", 0 },
                    { "require_rank", 0 },
                    { "need_more_play", 0 },
                    { "percentage_before", "0.92" },
                    { "percentage", "1.00" }
                },

                // Success Flag
                ["0"] = true
            };
            
            string serializedResponse = PhpSerialization.Serialize(response);
            Logger.LogInformation($"[Upgrades] Response: {serializedResponse}");
    
            return Ok(serializedResponse);
        }
        catch (Exception ex)
        {
             Logger.LogError(ex, "[Upgrades] CRITICAL FAILURE in HandleGetUpgrades");
             return StatusCode(500, "Internal Server Error");
        }
    }
}
