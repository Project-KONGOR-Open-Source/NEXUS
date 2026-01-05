namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

using global::KONGOR.MasterServer.Extensions.Cache;
using global::KONGOR.MasterServer.Models.RequestResponse.Stats;
using global::KONGOR.MasterServer.Models.ServerManagement;
using global::MERRICK.DatabaseContext;
using global::MERRICK.DatabaseContext.Constants;
using global::MERRICK.DatabaseContext.Entities;
using global::MERRICK.DatabaseContext.Entities.Statistics;
using global::MERRICK.DatabaseContext.Entities.Utility;
using global::MERRICK.DatabaseContext.Enumerations;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

/// <summary>
///     Tests For Match Statistics Submission Functionality In KONGOR Master Server
/// </summary>
public sealed partial class MatchStatsSubmissionTests
{
    [Test]
    public async Task SubmitStats_WithValidData_ReturnsSuccess()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient httpClient = webApplicationFactory.CreateClient();

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();
        MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
        IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();

        // 1. Seed Database with Host and Player Accounts
        global::MERRICK.DatabaseContext.Entities.Utility.Role userRole = new() { Name = UserRoles.User };

        User hostUser = new()
        {
            EmailAddress = "host@kongor.net",
            PBKDF2PasswordHash = "hash",
            SRPPasswordHash = "hash",
            SRPPasswordSalt = "salt",
            GoldCoins = 0,
            PlinkoTickets = 0,
            Role = userRole
        };

        Account hostAccount = new()
        {
            Name = "MatchHost", // MatchServer uses this name
            User = hostUser,
            Type = AccountType.Staff,
            IsMain = true
        };

        User playerUser = new()
        {
            EmailAddress = "player@kongor.net",
            PBKDF2PasswordHash = "hash",
            SRPPasswordHash = "hash",
            SRPPasswordSalt = "salt",
            GoldCoins = 0,
            PlinkoTickets = 0,
            Role = userRole
        };

        Account playerAccount = new()
        {
            Name = "TestPlayer1", // Matches payload
            User = playerUser,
            Type = AccountType.Staff,
            IsMain = true
        };

        await dbContext.Users.AddRangeAsync(hostUser, playerUser);
        await dbContext.Accounts.AddRangeAsync(hostAccount, playerAccount);
        await dbContext.SaveChangesAsync();

        // 2. Seed Cache with Match Server Session
        string sessionCookie = Guid.NewGuid().ToString();
        int serverId = 1;

        MatchServer matchServer = new()
        {
            ID = serverId,
            Instance = 1,
            HostAccountID = hostAccount.ID,
            HostAccountName = hostAccount.Name,
            Name = "Test Server",
            IPAddress = "127.0.0.1",
            Port = 11235,
            Location = "US",
            Description = "Test Server",
            Cookie = sessionCookie,
            Status = ServerStatus.SERVER_STATUS_ACTIVE
        };

        await distributedCache.SetMatchServer(hostAccount.Name, matchServer);

        // 3. Construct the payload with the valid session
        Dictionary<string, string> formData = GetValidStatsPayload(sessionCookie, serverId);
        int matchId = int.Parse(formData["match_stats[match_id]"]);
        FormUrlEncodedContent content = new(formData);

        // 4. Submit the request
        Console.WriteLine($"[TEST] Submitting stats for Match ID: {matchId}...");
        HttpResponseMessage response = await httpClient.PostAsync("stats_requester.php", content);

        // 5. Verify response
        string responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"[TEST] Response Status: {response.StatusCode}");
        Console.WriteLine($"[TEST] Response Body: {responseBody}");

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        MatchStatistics? savedStats = await dbContext.MatchStatistics
            .FirstOrDefaultAsync(m => m.MatchID == matchId);

        if (savedStats != null)
        {
            Console.WriteLine($"[TEST] ✅ Database Verification: Match {savedStats.MatchID} saved successfully.");
            System.Text.Json.JsonSerializerOptions options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true, ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };
            string json = System.Text.Json.JsonSerializer.Serialize(savedStats, options);
            foreach (string line in json.Split(Environment.NewLine))
            {
                Console.WriteLine($"[TEST] {line}");
            }
        }
        else
        {
            Console.WriteLine($"[TEST] ❌ Database Verification: Match {matchId} NOT found in database.");
        }

        await Assert.That(savedStats).IsNotNull();

        List<PlayerStatistics> playerStats = await dbContext.PlayerStatistics
            .Where(p => p.MatchID == matchId)
            .ToListAsync();

        Console.WriteLine($"[TEST] ✅ Database Verification: Found {playerStats.Count} player statistic records.");
        if (playerStats.Count > 0)
        {
            System.Text.Json.JsonSerializerOptions options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true, ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };
            string json = System.Text.Json.JsonSerializer.Serialize(playerStats, options);
            foreach (string line in json.Split(Environment.NewLine))
            {
                Console.WriteLine($"[TEST] {line}");
            }
        }

        await Assert.That(playerStats).IsNotEmpty();
    }

    private static Dictionary<string, string> GetValidStatsPayload(string session, int serverId)
    {
        return new Dictionary<string, string>
        {
            ["f"] = "submit_stats",
            ["session"] = session,
            ["match_stats[server_id]"] = serverId.ToString(),
            ["match_stats[match_id]"] = "705750",
            ["match_stats[map]"] = "caldavar",
            ["match_stats[map_version]"] = "4.10.1",
            ["match_stats[time_played]"] = "1800",
            ["match_stats[file_size]"] = "102400",
            ["match_stats[file_name]"] = "M705750.honreplay",
            ["match_stats[c_state]"] = "0",
            ["match_stats[version]"] = "4.10.1.0",
            ["match_stats[avgpsr]"] = "1500",
            ["match_stats[avgpsr_team1]"] = "1500",
            ["match_stats[avgpsr_team2]"] = "1500",
            ["match_stats[gamemode]"] = "ap",
            ["match_stats[teamscoregoal]"] = "0",
            ["match_stats[playerscoregoal]"] = "0",
            ["match_stats[numrounds]"] = "1",
            ["match_stats[release_stage]"] = "stable",
            ["match_stats[awd_mann]"] = "0",
            ["match_stats[awd_mqk]"] = "0",
            ["match_stats[awd_lgks]"] = "5",
            ["match_stats[awd_msd]"] = "0",
            ["match_stats[awd_mkill]"] = "10",
            ["match_stats[awd_masst]"] = "5",
            ["match_stats[awd_ledth]"] = "2",
            ["match_stats[awd_mbdmg]"] = "5000",
            ["match_stats[awd_mwk]"] = "2",
            ["match_stats[awd_mhdd]"] = "15000",
            ["match_stats[awd_hcs]"] = "150",
            ["match_stats[submission_debug]"] = "debug_info",

            // Team Stats
            ["team_stats[1][score]"] = "0",
            ["team_stats[2][score]"] = "0",

            // Player Stats
            ["player_stats[0][Hero_Legionnaire][nickname]"] = "TestPlayer1",
            ["player_stats[0][Hero_Legionnaire][clan_id]"] = "0",
            ["player_stats[0][Hero_Legionnaire][team]"] = "1",
            ["player_stats[0][Hero_Legionnaire][position]"] = "0",
            ["player_stats[0][Hero_Legionnaire][group_num]"] = "0",
            ["player_stats[0][Hero_Legionnaire][benefit]"] = "0",
            ["player_stats[0][Hero_Legionnaire][hero_id]"] = "12",
            ["player_stats[0][Hero_Legionnaire][wins]"] = "1",
            ["player_stats[0][Hero_Legionnaire][losses]"] = "0",
            ["player_stats[0][Hero_Legionnaire][discos]"] = "0",
            ["player_stats[0][Hero_Legionnaire][concedes]"] = "0",
            ["player_stats[0][Hero_Legionnaire][kicked]"] = "0",
            ["player_stats[0][Hero_Legionnaire][social_bonus]"] = "0",
            ["player_stats[0][Hero_Legionnaire][used_token]"] = "0",
            ["player_stats[0][Hero_Legionnaire][concedevotes]"] = "0",
            ["player_stats[0][Hero_Legionnaire][herokills]"] = "5",
            ["player_stats[0][Hero_Legionnaire][herodmg]"] = "5000",
            ["player_stats[0][Hero_Legionnaire][herokillsgold]"] = "1500",
            ["player_stats[0][Hero_Legionnaire][heroassists]"] = "2",
            ["player_stats[0][Hero_Legionnaire][heroexp]"] = "2000",
            ["player_stats[0][Hero_Legionnaire][deaths]"] = "2",
            ["player_stats[0][Hero_Legionnaire][buybacks]"] = "0",
            ["player_stats[0][Hero_Legionnaire][goldlost2death]"] = "500",
            ["player_stats[0][Hero_Legionnaire][secs_dead]"] = "60",
            ["player_stats[0][Hero_Legionnaire][teamcreepkills]"] = "0",
            ["player_stats[0][Hero_Legionnaire][teamcreepdmg]"] = "0",
            ["player_stats[0][Hero_Legionnaire][teamcreepgold]"] = "0",
            ["player_stats[0][Hero_Legionnaire][teamcreepexp]"] = "0",
            ["player_stats[0][Hero_Legionnaire][neutralcreepkills]"] = "10",
            ["player_stats[0][Hero_Legionnaire][neutralcreepdmg]"] = "500",
            ["player_stats[0][Hero_Legionnaire][neutralcreepgold]"] = "300",
            ["player_stats[0][Hero_Legionnaire][neutralcreepexp]"] = "400",
            ["player_stats[0][Hero_Legionnaire][bdmg]"] = "1000",
            ["player_stats[0][Hero_Legionnaire][razed]"] = "1",
            ["player_stats[0][Hero_Legionnaire][bdmgexp]"] = "200",
            ["player_stats[0][Hero_Legionnaire][bgold]"] = "500",
            ["player_stats[0][Hero_Legionnaire][denies]"] = "5",
            ["player_stats[0][Hero_Legionnaire][exp_denied]"] = "100",
            ["player_stats[0][Hero_Legionnaire][gold]"] = "10000",
            ["player_stats[0][Hero_Legionnaire][gold_spent]"] = "9000",
            ["player_stats[0][Hero_Legionnaire][exp]"] = "5000",
            ["player_stats[0][Hero_Legionnaire][actions]"] = "1000",
            ["player_stats[0][Hero_Legionnaire][secs]"] = "1800",
            ["player_stats[0][Hero_Legionnaire][level]"] = "15",
            ["player_stats[0][Hero_Legionnaire][consumables]"] = "5",
            ["player_stats[0][Hero_Legionnaire][wards]"] = "2",
            ["player_stats[0][Hero_Legionnaire][bloodlust]"] = "1",
            ["player_stats[0][Hero_Legionnaire][doublekill]"] = "0",
            ["player_stats[0][Hero_Legionnaire][triplekill]"] = "0",
            ["player_stats[0][Hero_Legionnaire][quadkill]"] = "0",
            ["player_stats[0][Hero_Legionnaire][annihilation]"] = "0",
            ["player_stats[0][Hero_Legionnaire][ks3]"] = "1",
            ["player_stats[0][Hero_Legionnaire][ks4]"] = "0",
            ["player_stats[0][Hero_Legionnaire][ks5]"] = "0",
            ["player_stats[0][Hero_Legionnaire][ks6]"] = "0",
            ["player_stats[0][Hero_Legionnaire][ks7]"] = "0",
            ["player_stats[0][Hero_Legionnaire][ks8]"] = "0",
            ["player_stats[0][Hero_Legionnaire][ks9]"] = "0",
            ["player_stats[0][Hero_Legionnaire][ks10]"] = "0",
            ["player_stats[0][Hero_Legionnaire][ks15]"] = "0",
            ["player_stats[0][Hero_Legionnaire][smackdown]"] = "0",
            ["player_stats[0][Hero_Legionnaire][humiliation]"] = "0",
            ["player_stats[0][Hero_Legionnaire][nemesis]"] = "0",
            ["player_stats[0][Hero_Legionnaire][retribution]"] = "0",
            ["player_stats[0][Hero_Legionnaire][score]"] = "0",
            ["player_stats[0][Hero_Legionnaire][gameplaystat0]"] = "0",
            ["player_stats[0][Hero_Legionnaire][gameplaystat1]"] = "0",
            ["player_stats[0][Hero_Legionnaire][gameplaystat2]"] = "0",
            ["player_stats[0][Hero_Legionnaire][gameplaystat3]"] = "0",
            ["player_stats[0][Hero_Legionnaire][gameplaystat4]"] = "0",
            ["player_stats[0][Hero_Legionnaire][gameplaystat5]"] = "0",
            ["player_stats[0][Hero_Legionnaire][gameplaystat6]"] = "0",
            ["player_stats[0][Hero_Legionnaire][gameplaystat7]"] = "0",
            ["player_stats[0][Hero_Legionnaire][gameplaystat8]"] = "0",
            ["player_stats[0][Hero_Legionnaire][gameplaystat9]"] = "0",
            ["player_stats[0][Hero_Legionnaire][time_earning_exp]"] = "1700",

            // Inventory
            ["inventory[0][0]"] = "Item_LoggersHatchet",
            ["inventory[0][1]"] = "Item_Marchers",
            ["inventory[0][2]"] = "Item_HealthPotion",
        };
    }
}
