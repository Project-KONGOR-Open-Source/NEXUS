// For ClanTier

namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> HandleSetRank()
    {
        // DEBUG: Log all form keys
        string debugForm = string.Join(", ", Request.Form.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        Logger.LogInformation($"[Clans] HandleSetRank RAW Form: {debugForm}");

        string? cookie = Request.Form["cookie"];
        string? targetIdStr = Request.Form["target_id"];
        string? clanIdStr = Request.Form["clan_id"];
        string? rankStr = Request.Form["rank"];

        if (cookie == null || targetIdStr == null || clanIdStr == null || rankStr == null)
        {
             Logger.LogError($"[Clans] HandleSetRank Missing parameters: C:{cookie!=null} T:{targetIdStr} Cl:{clanIdStr} R:{rankStr}");
             return BadRequest("Missing parameters");
        }

        if (!int.TryParse(targetIdStr, out int targetId) || !int.TryParse(clanIdStr, out int clanId))
        {
             Logger.LogError($"[Clans] HandleSetRank Invalid ID: T:{targetIdStr} Cl:{clanIdStr}");
             return BadRequest("Invalid ID parameters");
        }

        // Validate Session
        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);
        if (!isValid || accountName == null)
        {
            Logger.LogError($"[Clans] HandleSetRank Invalid Session: {cookie}");
            return Unauthorized("Invalid Session");
        }

        // Fetch Requester
        Account? requester = await MerrickContext.Accounts
            .Include(a => a.Clan)
            .FirstOrDefaultAsync(a => a.Name == accountName);

        if (requester == null || requester.Clan?.ID != clanId)
        {
            Logger.LogError($"[Clans] HandleSetRank Requester (ID:{requester?.ID}) not in specified clan ({clanId}) or null.");
            return BadRequest("Requester not in specified clan");
        }

        if (requester.ClanTier < ClanTier.Officer)
        {
            Logger.LogError($"[Clans] HandleSetRank Requester (Tier:{requester.ClanTier}) has insufficient permissions.");
            return BadRequest("Insufficient Permissions");
        }

        // Fetch Target
        Account? target = await MerrickContext.Accounts
            .Include(a => a.Clan)
            .FirstOrDefaultAsync(a => a.ID == targetId);

        if (target == null || target.Clan?.ID != clanId)
        {
            Logger.LogError($"[Clans] HandleSetRank Target (ID:{targetId}) not in clan ({clanId}) or null.");
            return BadRequest("Target not in clan");
        }

        // Logic
        bool success = false;
        
        // Normalize rank string
        rankStr = rankStr.Trim();

        if (rankStr.Equals("Remove", StringComparison.OrdinalIgnoreCase))
        {
            // Kick
            // Standard check: Requester must be higher rank or Leader
            if (requester.ClanTier <= target.ClanTier && requester.ID != target.ID) // Allow self-leave? Usually client uses LeaveClan for self.
            {
                // Verify specific rules. Usually Leader can kick Officer. Officer can kick Member.
                // Officer cannot kick Officer.
                if (requester.ClanTier == ClanTier.Officer && target.ClanTier == ClanTier.Officer)
                {
                     Logger.LogError($"[Clans] HandleSetRank Officer cannot kick Officer.");
                     return BadRequest("Officer cannot kick Officer");
                }
            }

            target.Clan = null;
            target.ClanTier = ClanTier.None;
            success = true;
            Logger.LogInformation($"[Clans] Account {requester.Name} kicked {target.Name} from Clan {clanId}.");
        }
        else
        {
            // Promote/Demote
            ClanTier newTier;
            if (rankStr.Equals("Officer", StringComparison.OrdinalIgnoreCase)) newTier = ClanTier.Officer;
            else if (rankStr.Equals("Member", StringComparison.OrdinalIgnoreCase)) newTier = ClanTier.Member;
            else if (rankStr.Equals("Leader", StringComparison.OrdinalIgnoreCase)) newTier = ClanTier.Leader;
            else 
            {
                Logger.LogError($"[Clans] HandleSetRank Unknown Rank: {rankStr}");
                return BadRequest($"Unknown Rank: {rankStr}");
            }

            // Permission Checks
            if (newTier == ClanTier.Leader)
            {
                if (requester.ClanTier != ClanTier.Leader) 
                {
                    Logger.LogError($"[Clans] HandleSetRank Only Leader can promote to Leader.");
                    return BadRequest("Only Leader can promote to Leader");
                }
                
                // Swap
                requester.ClanTier = ClanTier.Officer; // Demote self
                target.ClanTier = ClanTier.Leader;
                success = true;
                Logger.LogInformation($"[Clans] {requester.Name} transferred Leadership to {target.Name}.");
            }
            else if (newTier == ClanTier.Officer)
            {
                if (requester.ClanTier < ClanTier.Leader && target.ClanTier == ClanTier.Officer) 
                {
                    Logger.LogError($"[Clans] HandleSetRank Officer cannot promote Officer.");
                    return BadRequest("Officer cannot promote Officer"); // No-op
                }
                
                target.ClanTier = ClanTier.Officer;
                success = true;
                Logger.LogInformation($"[Clans] {requester.Name} promoted {target.Name} to Officer.");
            }
            else if (newTier == ClanTier.Member)
            {
                 // Demote
                 if (requester.ClanTier <= target.ClanTier && requester.ID != target.ID) 
                 {
                     Logger.LogError($"[Clans] HandleSetRank Cannot demote higher/equal rank. Req:{requester.ClanTier} Tgt:{target.ClanTier}");
                     return BadRequest("Cannot demote higher/equal rank");
                 }
                 
                 target.ClanTier = ClanTier.Member;
                 success = true;
                 Logger.LogInformation($"[Clans] {requester.Name} demoted {target.Name} to Member.");
            }
        }

        if (success)
        {
            await MerrickContext.SaveChangesAsync();
            
            // Return Notification Keys to trigger Chat Server packet
            // Use positive random keys to ensure compatibility
            string key1 = Random.Shared.Next(1000000, 9999999).ToString();
            
            // 2026-01-10: Publish Update to Redis for Chat Server (Bypass Client Notification if Offline)
            // Format: "TargetID:Rank:RequesterID:ClanID:ClanName"
            string redisPayload = $"{targetId}:{target.ClanTier}:{requester.ID}:{clanId}:{requester.Clan.Name}";
            await DistributedCache.PublishAsync(RedisChannel.Literal("nexus.clan.updates"), redisPayload);
            Logger.LogInformation($"[Clans] Published Redis Update: {redisPayload}");

            Dictionary<string, object> response = new()
            {
                { "set_rank", "OK" },
                {
                    "notification", new Dictionary<string, int>
                    {
                        { key1, requester.ID }
                    }
                }
            };
            
            if (rankStr.Equals("Remove", StringComparison.OrdinalIgnoreCase))
            {
                response.Add("kick", "OK");
                response.Add("remove_member", "OK");
            }
            
            string serialized = PhpSerialization.Serialize(response);
            Logger.LogInformation($"[Clans] set_rank Response: {serialized}");
            
            return Ok(serialized);
        }

        return BadRequest("Action failed");
    }
}
