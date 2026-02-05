using System.Net;
using System.Text.RegularExpressions;

using ASPIRE.Tests.Data;

using KONGOR.MasterServer.Extensions.Cache;
using KONGOR.MasterServer.Models.RequestResponse.Stats;
using KONGOR.MasterServer.Models.ServerManagement;
using KONGOR.MasterServer.Services;
using KONGOR.MasterServer.Services.Requester;

using MERRICK.DatabaseContext.Entities.Statistics;

using Microsoft.EntityFrameworkCore;

namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

using EntityRole = Role;

public sealed class UpgradesRequestTests
{
    private async
        Task<(HttpClient Client, MerrickContext DbContext, IDatabase Cache, string Cookie,
            WebApplicationFactory<KONGORAssemblyMarker> Factory)> SetupAsync(string? accountName = null)
    {
        WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = factory.CreateClient();
        IServiceScope scope = factory.Services.CreateScope();
        MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
        IDatabase cache = scope.ServiceProvider.GetRequiredService<IDatabase>();

        string cookie = Guid.NewGuid().ToString("N");
        string name = accountName ?? $"TestUser_{Guid.NewGuid().ToString("N")[..8]}";

        User user = new()
        {
            EmailAddress = $"{name}@kongor.net",
            PBKDF2PasswordHash = "hash",
            SRPPasswordHash = "hash",
            SRPPasswordSalt = "salt",
            Role = new EntityRole { Name = UserRoles.User },
            OwnedStoreItems = new List<string> { "item1", "item2" }
        };

        Account account = new()
        {
            Name = name,
            User = user,
            Type = AccountType.Staff,
            IsMain = true,
            Cookie = cookie
        };

        await dbContext.Users.AddAsync(user);
        await dbContext.Accounts.AddAsync(account);

        // Seed some stats to verify aggregation logic
        PlayerStatistics stats1 = CreateDefaultPlayerStatistics(account.ID, 101);
        stats1.RankedMatch = 1;
        stats1.RankedSkillRatingChange = 10;
        stats1.HeroKills = 5;
        stats1.HeroDeaths = 2;
        stats1.SecondsPlayed = 1800;
        stats1.Experience = 1000;
        stats1.Gold = 500;

        PlayerStatistics stats2 = CreateDefaultPlayerStatistics(account.ID, 102);
        stats2.PublicMatch = 1;
        stats2.PublicSkillRatingChange = 5;
        stats2.HeroKills = 10;
        stats2.HeroDeaths = 0;
        stats2.SecondsPlayed = 1200;
        stats2.Experience = 800;
        stats2.Gold = 400;

        await dbContext.PlayerStatistics.AddRangeAsync(stats1, stats2);
        await dbContext.SaveChangesAsync();

        await cache.SetAccountNameForSessionCookie(cookie, account.Name);

        return (client, dbContext, cache, cookie, factory);
    }

    [Test]
    public async Task GetUpgrades_SchemaParity_Returns46Fields()
    {
        (HttpClient client, MerrickContext dbContext, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync("SchemaUser");
        await using (factory)
        {
            Dictionary<string, string> payload = new()
            {
                { "f", "get_upgrades" },
                { "cookie", cookie }
            };
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);

            response.EnsureSuccessStatusCode();
            string body = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Response Body: {body}");

            // Verify Root Keys
            string[] requiredRootKeys =
            {
                "field_stats", "standing", "points", "mmpoints", "0"
            };
            foreach (string key in requiredRootKeys)
            {
                await Assert.That(body).Contains(key);
            }

            // Verify String Typing for specific fields (Regression Fix)
            // Working capture shows dice_tokens as String (s:1:"1")
            await Assert.That(body).Contains("\"dice_tokens\";s:1:\"1\";");
            await Assert.That(body).Contains("\"slot_id\";s:1:\"1\";");
            await Assert.That(body).Contains("\"season_level\";i:0;");

            // Verify Field Stats Schema (The 46 Fields)

            string[] requiredStatKeys =
            {
                "account_id", "super_id", "beta", "lan", "account_type", "standing",
                "rnk_amm_team_rating", "cs_amm_team_rating", "mid_amm_team_rating", "rift_amm_team_rating", "rb_amm_team_rating",
                "acc_games_played", "rnk_games_played", "cs_games_played", "mid_games_played", "rift_games_played", "bot_games_played",
                "acc_discos", "rnk_discos", "cs_discos", "mid_discos", "rift_discos",
                "rnk_herokills", "cs_herokills",
                "rnk_deaths", "cs_deaths",
                "rnk_heroassists", "cs_heroassists",
                "rnk_exp", "cs_exp", "rb_exp",
                "rnk_gold", "cs_gold", "rb_gold",
                "rnk_secs", "cs_secs",
                "level", "level_exp",
                "campaign_casual_mmr", "campaign_casual_medal", "campaign_casual_discos", "campaign_casual_match_played",
                "campaign_normal_mmr", "campaign_normal_medal", "campaign_normal_discos", "campaign_normal_match_played"
            };

            foreach (string statKey in requiredStatKeys)
            {
                // We look for strict serialization pattern: "key";
                await Assert.That(body).Contains($"\"{statKey}\";");
            }

            // Verify Types for Campaign Fields (should be integers i:0;)
            await Assert.That(body).Contains("\"campaign_casual_discos\";i:0;");
            await Assert.That(body).Contains("\"campaign_normal_discos\";i:0;");

            // Verify Missing Fields are present
            await Assert.That(body).Contains("\"mid_discos\";s:1:\"0\";");
            await Assert.That(body).Contains("\"rift_discos\";s:1:\"0\";");
            await Assert.That(body).Contains("\"rb_exp\";s:1:\"0\";");
            await Assert.That(body).Contains("\"rb_gold\";s:1:\"0\";");

            // Verify Values (Aggregation Check)
            // rnk_herokills = 5 (string)
            await Assert.That(body).Contains("\"rnk_herokills\";s:1:\"5\";");
            // cs_herokills = 10 (string)
            await Assert.That(body).Contains("\"cs_herokills\";s:2:\"10\";");

            // --- Dynamic Verification via ClientRequestHelper ---
            using IServiceScope testScope = factory.Services.CreateScope();
            MerrickContext dbContextScope = testScope.ServiceProvider.GetRequiredService<MerrickContext>();
            IPlayerStatisticsService statsService = testScope.ServiceProvider.GetRequiredService<IPlayerStatisticsService>();

            // Fetch account and stats from DB
            Account account = await dbContextScope.Accounts
                .Include(a => a.User)
                .Include(a => a.Clan)
                .FirstAsync(a => a.Cookie == cookie);

            PlayerStatisticsAggregatedDTO stats = await statsService.GetAggregatedStatisticsAsync(account.ID);

            ShowSimpleStatsResponse fullStats = ClientRequestHelper.CreateShowSimpleStatsResponse(account, stats, 12);

            // Assert Response matches Helper Logic for Level
            await Assert.That(body).Contains($"\"level\";s:{fullStats.Level.ToString().Length}:\"{fullStats.Level}\";");
            await Assert.That(body).Contains($"\"level_exp\";s:{fullStats.LevelExperience.ToString().Length}:\"{fullStats.LevelExperience}\";");
        }
    }

    private static PlayerStatistics CreateDefaultPlayerStatistics(int accountId, int matchId)
    {
        return new PlayerStatistics
        {
            MatchID = matchId,
            AccountID = accountId,
            AccountName = "Test",
            ClanID = null,
            ClanTag = null,
            Team = 1,
            LobbyPosition = 1,
            GroupNumber = 0,
            Benefit = 0,
            HeroProductID = 0,
            MVP = 0,
            Inventory = new List<string>(),
            Win = 0,
            Loss = 0,
            Disconnected = 0,
            Conceded = 0,
            Kicked = 0,
            PublicMatch = 0,
            PublicSkillRatingChange = 0,
            RankedMatch = 0,
            RankedSkillRatingChange = 0,
            SocialBonus = 0,
            UsedToken = 0,
            ConcedeVotes = 0,
            HeroKills = 0,
            HeroDamage = 0,
            GoldFromHeroKills = 0,
            HeroAssists = 0,
            HeroExperience = 0,
            HeroDeaths = 0,
            Buybacks = 0,
            GoldLostToDeath = 0,
            SecondsDead = 0,
            TeamCreepKills = 0,
            TeamCreepDamage = 0,
            TeamCreepGold = 0,
            TeamCreepExperience = 0,
            NeutralCreepKills = 0,
            NeutralCreepDamage = 0,
            NeutralCreepGold = 0,
            NeutralCreepExperience = 0,
            BuildingDamage = 0,
            BuildingsRazed = 0,
            ExperienceFromBuildings = 0,
            GoldFromBuildings = 0,
            Denies = 0,
            ExperienceDenied = 0,
            Gold = 0,
            GoldSpent = 0,
            Experience = 0,
            Actions = 0,
            SecondsPlayed = 0,
            HeroLevel = 1,
            ConsumablesPurchased = 0,
            WardsPlaced = 0,
            FirstBlood = 0,
            DoubleKill = 0,
            TripleKill = 0,
            QuadKill = 0,
            Annihilation = 0,
            KillStreak03 = 0,
            KillStreak04 = 0,
            KillStreak05 = 0,
            KillStreak06 = 0,
            KillStreak07 = 0,
            KillStreak08 = 0,
            KillStreak09 = 0,
            KillStreak10 = 0,
            KillStreak15 = 0,
            Smackdown = 0,
            Humiliation = 0,
            Nemesis = 0,
            Retribution = 0,
            Score = 0,
            GameplayStat0 = 0,
            GameplayStat1 = 0,
            GameplayStat2 = 0,
            GameplayStat3 = 0,
            GameplayStat4 = 0,
            GameplayStat5 = 0,
            GameplayStat6 = 0,
            GameplayStat7 = 0,
            GameplayStat8 = 0,
            GameplayStat9 = 0,
            TimeEarningExperience = 0
        };
    }
}
