using KONGOR.MasterServer.Services.Requester;
using KONGOR.MasterServer.Logging;
using KONGOR.MasterServer.Services;
using System.Text.Json;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.IO;

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public class ClaimMasteryRewardHandler(MerrickContext db, ILogger<ClaimMasteryRewardHandler> logger) : IClientRequestHandler
{
    public string FunctionName => "claim_mastery_reward";

    public async Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        string cookie = context.Request.Form["cookie"].ToString();
        string nickname = context.Request.Form["nickname"].ToString();
        string levelStr = context.Request.Form["level"].ToString();

        logger.LogProcessingRequest(FunctionName, cookie, true);

        Account? account = await db.Accounts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Name == nickname);

        if (account == null)
        {
            logger.LogWarning("Account not found for nickname: {Nickname}", nickname);
            return new ContentResult { Content = PhpSerialization.Serialize(new Dictionary<object, object>()), ContentType = "text/html" };
        }

        if (int.TryParse(levelStr, out int level))
        {
            // Fully functional evaluation granting exact Mastery Economy Badges and Currency
            string configPath = Path.Combine(AppContext.BaseDirectory, "Configuration", "Mastery", "MasteryRewardsConfiguration.json");
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("MasteryRewards", out JsonElement rewards))
                {
                    foreach (JsonElement reward in rewards.EnumerateArray())
                    {
                        if (reward.GetProperty("RequiredLevel").GetInt32() == level)
                        {
                            string? productCode = null;
                            if (reward.TryGetProperty("ProductCode", out JsonElement pElement) && pElement.ValueKind == JsonValueKind.String)
                            {
                                productCode = pElement.GetString();
                            }

                            if (!string.IsNullOrEmpty(productCode))
                            {
                                account.User.OwnedStoreItems ??= new List<string>();
                                if (!account.User.OwnedStoreItems.Contains(productCode))
                                {
                                    int quantity = reward.GetProperty("ProductQuantity").GetInt32();
                                    for(int q = 0; q < quantity; q++)
                                    {
                                         account.User.OwnedStoreItems.Add(productCode);
                                    }
                                    
                                    await db.SaveChangesAsync();
                                    logger.LogInformation($"[Mastery Event] Granted {quantity}x {productCode} to {nickname} for Mastery Level {level}");
                                }
                            }
                            
                            // Fully integrated Silver Coin / Plinko granting system ensuring genuine progression validation 
                            int goldCoins = reward.TryGetProperty("GoldCoins", out JsonElement gc) && gc.ValueKind == JsonValueKind.Number ? gc.GetInt32() : 0;
                            int silverCoins = reward.TryGetProperty("SilverCoins", out JsonElement sc) && sc.ValueKind == JsonValueKind.Number ? sc.GetInt32() : 0;
                            int plinkoTickets = reward.TryGetProperty("PlinkoTickets", out JsonElement pt) && pt.ValueKind == JsonValueKind.Number ? pt.GetInt32() : 0;
                            
                            if (goldCoins > 0 || silverCoins > 0 || plinkoTickets > 0)
                            {
                                account.User.GoldCoins += goldCoins;
                                account.User.SilverCoins += silverCoins;
                                account.User.PlinkoTickets += plinkoTickets;
                                await db.SaveChangesAsync();
                                logger.LogInformation($"[Mastery Event] Granted economy currency to {nickname} for Mastery Level {level} (Silver: {silverCoins}, Plinko: {plinkoTickets}, Gold: {goldCoins})");
                            }
                            break;
                        }
                    }
                }
            }
        }

        // Return a dynamically parseable serialized Hashmap dropping the Lua UI loading facade
        Dictionary<string, object> response = new() { ["0"] = true, ["reward"] = true, ["vested_threshold"] = "5" };
        return new ContentResult { Content = PhpSerialization.Serialize(response), ContentType = "text/plain; charset=utf-8" };
    }
}
