namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> HandleDebugUpgradesDiff()
    {
        try
        {
            string? cookie = Request.Form["cookie"];
            if (string.IsNullOrEmpty(cookie))
            {
                return BadRequest("Missing cookie");
            }

            string? sessionAccountName = HttpContext.Items["SessionAccountName"] as string
                                         ?? await DistributedCache.GetAccountNameForSessionCookie(cookie);

            if (sessionAccountName is null)
            {
                return Unauthorized("Session Not Found");
            }

            Account? account = await MerrickContext.Accounts
                .Include(a => a.User)
                .Include(a => a.Clan)
                .FirstOrDefaultAsync(a => a.Name == sessionAccountName);

            if (account is null)
            {
                return NotFound("Account Not Found");
            }

            StringBuilder report = new();
            report.AppendLine($"--- Payload Comparison Report for {sessionAccountName} ---");

            // 1. Generate Reference Payload (InitStats) logic locally to inspect types
            ShowSimpleStatsResponse fullStats = await CreateShowSimpleStatsResponse(account);

            // Replicating HandleInitStats Logic
            Dictionary<string, object> initStatsReference = new()
            {
                { "points", account.User.SilverCoins }, // int (Fixed: Access from account.User)
                { "mmpoints", account.User.GoldCoins }, // int (Fixed: Access from account.User)
                { "standing", "3" },
                { "0", fullStats.Zero } // bool, Key "0" is STRING
            };

            // 2. Generate Suspect Payload (GetUpgrades) logic locally
            Dictionary<string, object> fieldStats = new()
            {
                ["nickname"] = fullStats.NameWithClanTag, ["account_id"] = fullStats.ID
            };
            Dictionary<string, object> myUpgradesInfo = new();
            foreach (string item in account.User.OwnedStoreItems)
            {
                myUpgradesInfo[item] = new StoreItemData();
            }

            // Replicating HandleGetUpgrades Logic (The FIX version)
            Dictionary<string, object> upgradesFix = new()
            {
                ["field_stats"] = fieldStats,
                ["my_upgrades_info"] = myUpgradesInfo,
                ["points"] = account.User.SilverCoins, // int
                ["mmpoints"] = account.User.GoldCoins, // int
                ["game_tokens"] = fullStats.GameTokens, // int
                ["standing"] = "3",
                ["0"] = true // STRING Key "0"
            };

            report.AppendLine("\n[Critical Key Analysis]");

            // Compare "0" vs 0
            report.AppendLine($"InitStats (Working): Key '0' is STRING. Value: {initStatsReference["0"]}");
            report.AppendLine($"Upgrades (Proposed): Key '0' is STRING. Value: {upgradesFix["0"]}");

            report.AppendLine("\n[Serialization Preview]");
            report.AppendLine(
                $"InitStats '0' -> {PhpSerialization.Serialize(initStatsReference["0"])} (inside dict with string keys)");

            // Force a mini-serialization of the keys
            Dictionary<string, bool> d1 = new() { ["0"] = true };
            Dictionary<int, bool> d2 = new() { [0] = true };

            report.AppendLine($"Dict<string,bool> {{[\"0\"]=true}} -> {PhpSerialization.Serialize(d1)}");
            report.AppendLine($"Dict<int,bool> {{[0]=true}}       -> {PhpSerialization.Serialize(d2)}");

            report.AppendLine("\n[Conclusion]");
            report.AppendLine(
                "If InitStats works with s:1:\"0\";b:1;, then Upgrades MUST match it unless the client parser expects different types for different packets.");
            report.AppendLine("Current Fix sets it to i:0;b:1; (Int Key). Verify if this is desired.");

            return Ok(report.ToString());
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}