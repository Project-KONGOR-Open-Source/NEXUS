using System.Diagnostics;
using System.Globalization;
using System.Net;

using MERRICK.DatabaseContext.Entities.Statistics;

// ReSharper disable once CheckNamespace
namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

public sealed class MatchStatsSubmissionPerformanceTests
{
    [Test]
    public async Task SubmitStats_PerformanceBenchmark_10Players()
    {
        // Arrange
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory =
            KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = webApplicationFactory.CreateClient();

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();
        MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
        IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();

        const int matchId = 999999999;

        // Seed MatchStatistics
        MatchStatistics matchStats = new()
        {
            MatchID = matchId,
            ServerID = 1,
            HostAccountName = "PerfHost",
            Map = "caldavar",
            MapVersion = "1.0",
            Version = "4.10.1",
            GameMode = "normal",
            TimePlayed = 1800,
            FileSize = 2048,
            FileName = $"M{matchId}.honreplay",
            ConnectionState = 0,
            AveragePSR = 1600,
            AveragePSRTeamOne = 1600,
            AveragePSRTeamTwo = 1600,
            ScoreTeam1 = 0,
            ScoreTeam2 = 0,
            TeamScoreGoal = 0,
            PlayerScoreGoal = 0,
            NumberOfRounds = 1,
            ReleaseStage = "Live",
            BannedHeroes = null,
            AwardMostAnnihilations = null,
            AwardMostQuadKills = null,
            AwardLargestKillStreak = null,
            AwardMostSmackdowns = null,
            AwardMostKills = null,
            AwardMostAssists = null,
            AwardLeastDeaths = null,
            AwardMostBuildingDamage = null,
            AwardMostWardsKilled = null,
            AwardMostHeroDamageDealt = null,
            AwardHighestCreepScore = null
        };
        await dbContext.MatchStatistics.AddAsync(matchStats);

        // Seed 10 Accounts
        Role userRole = new() { Name = "User" };
        // Note: In a real scenario, we might want to attach the same Role instance to all users to avoid tracking issues,
        // but for this test we'll just let EF handle it or create distinct ones if needed.
        // Better to attach existing if possible, but here we just create a new one for simplicity or assume it works.
        // Actually, to be safe, let's just create one Role and reuse it.

        // Wait, if "User" role already exists in the orchestrated DB, we might have a conflict if we try to add it again.
        // Usually KONGORServiceProvider seeds some data. Let's check if we can just fetch it or create if missing.
        // For simplicity in this test, we'll assume fresh DB per test (standard for WebApplicationFactory with InMemory/Respawner usually,
        // but here it seems KONGORServiceProvider handles it).
        // I will create a new unique role name to avoid conflicts just in case.
        Role perfRole = new() { Name = "PerfUserRole" };

        List<Account> accounts = new List<Account>();
        for (int i = 0; i < 10; i++)
        {
            User user = new()
            {
                EmailAddress = $"perf_user_{i}@kongor.net",
                SRPPasswordHash = "hash",
                SRPPasswordSalt = "salt",
                PBKDF2PasswordHash = "hash",
                Role = perfRole
            };

            Account account = new()
            {
                User = user,
                Name = $"PerfPlayer_{i}",
                Cookie = $"cookie_{i}",
                IsMain = true
            };
            accounts.Add(account);
        }

        await dbContext.Users.AddRangeAsync(accounts.Select(a => a.User));
        await dbContext.Accounts.AddRangeAsync(accounts);
        await dbContext.SaveChangesAsync();

        // Prepare Payload
        Dictionary<string, string> payload = new()
        {
            { "f", "submit_stats" },
            { "session", "cookie_0" }, // Use first player as session/host for simplicity, though logic might verify session matches something.
            // Actually, submit_stats usually comes from the server or a client.
            // The controller checks `form.Session`. If it finds a MatchServer in Redis, good.
            // If not, it falls back to DB check for `a.Cookie == form.Session`.
            // So we need to make sure `cookie_0` is valid.
        };

        // Add Match Stats
        payload.Add("match_stats[match_id]", matchId.ToString(CultureInfo.InvariantCulture));
        payload.Add("match_stats[server_id]", "1");

        // Add Team Stats (Required)
        payload.Add("team_stats[1][score]", "0");
        payload.Add("team_stats[2][score]", "0");

        // Add Player Stats
        for (int i = 0; i < 10; i++)
        {
            string prefix = $"player_stats[{i}]";
            string heroName = "Hero_Legionnaire";
            string innerPrefix = $"{prefix}[{heroName}]";

            payload.Add($"{innerPrefix}[hero_id]", "1");
            payload.Add($"{innerPrefix}[nickname]", $"PerfPlayer_{i}"); // Plain name, no clan tag for simplicity
            payload.Add($"{innerPrefix}[team]", i < 5 ? "1" : "2");
            // Add other mandatory fields to avoid validation errors if any (simplified)
            payload.Add($"{innerPrefix}[position]", "0");
            payload.Add($"{innerPrefix}[level]", "10");
            payload.Add($"{innerPrefix}[group_num]", "1");
            payload.Add($"{innerPrefix}[benefit]", "0");
            payload.Add($"{innerPrefix}[wins]", "1");
            payload.Add($"{innerPrefix}[losses]", "0");
            payload.Add($"{innerPrefix}[discos]", "0");
            payload.Add($"{innerPrefix}[concedes]", "0");
            payload.Add($"{innerPrefix}[kicked]", "0");
            payload.Add($"{innerPrefix}[social_bonus]", "0");
            payload.Add($"{innerPrefix}[used_token]", "0");
            payload.Add($"{innerPrefix}[concedevotes]", "0");
            payload.Add($"{innerPrefix}[herokills]", "0");
            payload.Add($"{innerPrefix}[herodmg]", "0");
            payload.Add($"{innerPrefix}[herokillsgold]", "0");
            payload.Add($"{innerPrefix}[heroassists]", "0");
            payload.Add($"{innerPrefix}[heroexp]", "0");
            payload.Add($"{innerPrefix}[deaths]", "0");
            payload.Add($"{innerPrefix}[buybacks]", "0");
            payload.Add($"{innerPrefix}[goldlost2death]", "0");
            payload.Add($"{innerPrefix}[secs_dead]", "0");
            payload.Add($"{innerPrefix}[teamcreepkills]", "0");
            payload.Add($"{innerPrefix}[teamcreepdmg]", "0");
            payload.Add($"{innerPrefix}[teamcreepgold]", "0");
            payload.Add($"{innerPrefix}[teamcreepexp]", "0");
            payload.Add($"{innerPrefix}[neutralcreepkills]", "0");
            payload.Add($"{innerPrefix}[neutralcreepdmg]", "0");
            payload.Add($"{innerPrefix}[neutralcreepgold]", "0");
            payload.Add($"{innerPrefix}[neutralcreepexp]", "0");
            payload.Add($"{innerPrefix}[bdmg]", "0");
            payload.Add($"{innerPrefix}[razed]", "0");
            payload.Add($"{innerPrefix}[bdmgexp]", "0");
            payload.Add($"{innerPrefix}[bgold]", "0");
            payload.Add($"{innerPrefix}[denies]", "0");
            payload.Add($"{innerPrefix}[exp_denied]", "0");
            payload.Add($"{innerPrefix}[gold]", "0");
            payload.Add($"{innerPrefix}[gold_spent]", "0");
            payload.Add($"{innerPrefix}[exp]", "0");
            payload.Add($"{innerPrefix}[actions]", "0");
            payload.Add($"{innerPrefix}[secs]", "0");
            payload.Add($"{innerPrefix}[consumables]", "0");
            payload.Add($"{innerPrefix}[wards]", "0");
            payload.Add($"{innerPrefix}[bloodlust]", "0");
            payload.Add($"{innerPrefix}[doublekill]", "0");
            payload.Add($"{innerPrefix}[triplekill]", "0");
            payload.Add($"{innerPrefix}[quadkill]", "0");
            payload.Add($"{innerPrefix}[annihilation]", "0");
            payload.Add($"{innerPrefix}[ks3]", "0");
            payload.Add($"{innerPrefix}[ks4]", "0");
            payload.Add($"{innerPrefix}[ks5]", "0");
            payload.Add($"{innerPrefix}[ks6]", "0");
            payload.Add($"{innerPrefix}[ks7]", "0");
            payload.Add($"{innerPrefix}[ks8]", "0");
            payload.Add($"{innerPrefix}[ks9]", "0");
            payload.Add($"{innerPrefix}[ks10]", "0");
            payload.Add($"{innerPrefix}[ks15]", "0");
            payload.Add($"{innerPrefix}[smackdown]", "0");
            payload.Add($"{innerPrefix}[humiliation]", "0");
            payload.Add($"{innerPrefix}[nemesis]", "0");
            payload.Add($"{innerPrefix}[retribution]", "0");
            payload.Add($"{innerPrefix}[score]", "0");
            payload.Add($"{innerPrefix}[gameplaystat0]", "0");
            payload.Add($"{innerPrefix}[gameplaystat1]", "0");
            payload.Add($"{innerPrefix}[gameplaystat2]", "0");
            payload.Add($"{innerPrefix}[gameplaystat3]", "0");
            payload.Add($"{innerPrefix}[gameplaystat4]", "0");
            payload.Add($"{innerPrefix}[gameplaystat5]", "0");
            payload.Add($"{innerPrefix}[gameplaystat6]", "0");
            payload.Add($"{innerPrefix}[gameplaystat7]", "0");
            payload.Add($"{innerPrefix}[gameplaystat8]", "0");
            payload.Add($"{innerPrefix}[gameplaystat9]", "0");
            payload.Add($"{innerPrefix}[time_earning_exp]", "0");
        }

        // Run Benchmark
        Stopwatch sw = Stopwatch.StartNew();

        HttpResponseMessage response =
            await client.PostAsync("/stats_requester.php", new FormUrlEncodedContent(payload));

        sw.Stop();

        string content = await response.Content.ReadAsStringAsync();

        // Log the time - this will show up in test output
        Console.WriteLine($"[Benchmark] SubmitStats (10 Players) took: {sw.ElapsedMilliseconds} ms");

        // Assert Success
        if (response.StatusCode != HttpStatusCode.OK)
        {
            Console.WriteLine($"Failed with {response.StatusCode}: {content}");
        }
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }
}
