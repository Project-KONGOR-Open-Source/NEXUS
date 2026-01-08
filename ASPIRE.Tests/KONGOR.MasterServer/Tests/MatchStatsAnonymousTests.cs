using System.Net;
using ASPIRE.Common.Enumerations;
using ASPIRE.Tests;
using KONGOR.MasterServer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MERRICK.DatabaseContext;
using MERRICK.DatabaseContext.Entities.Statistics;
using StackExchange.Redis;
using KONGOR.MasterServer.Extensions.Cache;

using ASPIRE.Tests.KONGOR.MasterServer; // For KONGORServiceProvider
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

public sealed partial class MatchStatsSubmissionTests
{
    [Test]
    public async Task GetMatchStats_WithEmptyCookie_ReturnsSuccess()
    {
        // Arrange
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory =
            KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = webApplicationFactory.CreateClient();

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();
        MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        // Seed MatchStatistics
        int matchID = 963564305;
        MatchStatistics matchStats = new()
        {
            MatchID = matchID,
            ServerID = 1,
            HostAccountName = "TestHost",
            Map = "caldavar",
            MapVersion = "1.0",
            Version = "4.10.1",
            GameMode = "normal",
            TimePlayed = 1200, // Seconds
            FileSize = 1024,
            FileName = "M963564305.honreplay",
            ConnectionState = 0,
            AveragePSR = 1500,
            AveragePSRTeamOne = 1500,
            AveragePSRTeamTwo = 1500,
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
        await dbContext.SaveChangesAsync();

        // payload with empty cookie
        Dictionary<string, string> payload = new()
        {
            { "f", "get_match_stats" }, { "match_id", matchID.ToString() }, { "cookie", "" }
        };

        // Act
        // Use /client_requester.php as per controller route
        HttpResponseMessage response =
            await client.PostAsync("/client_requester.php", new FormUrlEncodedContent(payload));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        // Manual verification of PHP serialized content check or basic containment
        // Since PhpSerialization might not be easily accessible or requires internal namespaces,
        // We can just verify the content contains expected keys in their serialized form.
        // Array/Dict keys: "match_summ", "match_player_stats", "selected_upgrades"

        // Serialized strings look like: s:10:"match_summ"; or s:17:"selected_upgrades";
        await Assert.That(content).Contains("match_summ");
        await Assert.That(content).Contains("match_player_stats");
        await Assert.That(content).Contains("selected_upgrades");
    }

    [Test]
    public async Task GetMatchStats_WithValidCookie_ReturnsAuthenticatedSuccess()
    {
        // Arrange
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory =
            KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = webApplicationFactory.CreateClient();

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();
        MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
        IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();

        // Seed MatchStatistics
        int matchID = 963564306;
        MatchStatistics matchStats = new()
        {
            MatchID = matchID,
            ServerID = 1,
            HostAccountName = "TestHost",
            Map = "caldavar",
            MapVersion = "1.0",
            Version = "4.10.1",
            GameMode = "normal",
            TimePlayed = 1200, // Seconds
            FileSize = 1024,
            FileName = "M963564306.honreplay",
            ConnectionState = 0,
            AveragePSR = 1500,
            AveragePSRTeamOne = 1500,
            AveragePSRTeamTwo = 1500,
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

        // Seed Account and Session
        // Use Global Constants for seeding
        // Assuming global::MERRICK.DatabaseContext.Entities.Utility.Role exists
        global::MERRICK.DatabaseContext.Entities.Utility.Role userRole = new() { Name = "User" };

        global::MERRICK.DatabaseContext.Entities.Core.User user = new()
        {
            EmailAddress = "auth_user@kongor.net",
            SRPPasswordHash = "srphash",
            SRPPasswordSalt = "srpsalt",
            PBKDF2PasswordHash = "hash",
            Role = userRole
        };
        await dbContext.Users.AddAsync(user);

        global::MERRICK.DatabaseContext.Entities.Core.Account account = new()
        {
            User = user, Name = "AuthUser", Cookie = "valid_cookie_123", IsMain = true
        };
        // Add some selected upgrades to verify personalization
        account.SelectedStoreItems.Add("aa.test_avatar");

        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();

        // Seed Redis
        await distributedCache.SetAccountNameForSessionCookie("valid_cookie_123", "AuthUser");

        // payload with valid cookie
        Dictionary<string, string> payload = new()
        {
            { "f", "get_match_stats" }, { "match_id", matchID.ToString() }, { "cookie", "valid_cookie_123" }
        };

        // Act
        HttpResponseMessage response =
            await client.PostAsync("/client_requester.php", new FormUrlEncodedContent(payload));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        // Verify response contains personalized data
        await Assert.That(content).Contains("match_summ");
        // Should contain result of SelectedStoreItems
        // Expected serialized: s:14:"aa.test_avatar"; or similar
        await Assert.That(content).Contains("aa.test_avatar");
    }

    [Test]
    public async Task GetMatchStats_WithInvalidMatchID_ReturnsSoftFailure()
    {
        // Arrange
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory =
            KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = webApplicationFactory.CreateClient();

        // payload with mismatched/missing Match ID
        Dictionary<string, string> payload = new()
        {
            { "f", "get_match_stats" },
            { "match_id", "123456789" }, // ID does not exist in seeded DB
            { "cookie", "any_cookie" }
        };

        // Act
        HttpResponseMessage response =
            await client.PostAsync("/client_requester.php", new FormUrlEncodedContent(payload));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // Should return 200 OK now, not 404
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        // Should return serialized "Soft Failure" (Empty Stats) object
        // b:0; is no longer returned to prevent client crashes.
        // We verify that it returns a serialized array containing basic keys like "match_summ"
        await Assert.That(content).Contains("match_summ");
    }

    [Test]
    public async Task GetMatchStats_ViaQueryString_WithEmptyCookie_ReturnsSuccess()
    {
        // Arrange
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory =
            KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = webApplicationFactory.CreateClient();

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();
        MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        // Seed MatchStatistics
        int matchID = 963564307;
        MatchStatistics matchStats = new()
        {
            MatchID = matchID,
            ServerID = 1,
            HostAccountName = "TestHost",
            Map = "caldavar",
            MapVersion = "1.0",
            Version = "4.10.1",
            GameMode = "normal",
            TimePlayed = 1200,
            FileSize = 1024,
            FileName = "M963564307.honreplay",
            ConnectionState = 0,
            AveragePSR = 1500,
            AveragePSRTeamOne = 1500,
            AveragePSRTeamTwo = 1500,
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
        await dbContext.SaveChangesAsync();

        // Payload WITHOUT "f" in body, but with invalid/empty cookie
        Dictionary<string, string> payload = new()
        {
            { "match_id", matchID.ToString() }, 
            { "cookie", "" }
        };

        // Act
        // Pass "f" in the Query String
        HttpResponseMessage response =
            await client.PostAsync("/client_requester.php?f=get_match_stats", new FormUrlEncodedContent(payload));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        
        // If this fails with 401, it means the controller didn't pick up "f" from the QueryString,
        // and thus treated it as a generic request requiring validation (which fails due to empty cookie).
        
        await Assert.That(content).Contains("match_summ");
    }

    [Test]
    public async Task GetMatchStats_WithMixedCase_ReturnsSuccess()
    {
        // Arrange
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory =
            KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = webApplicationFactory.CreateClient();

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();
        MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        // Seed MatchStatistics
        int matchID = 963564308;
        MatchStatistics matchStats = new()
        {
            MatchID = matchID,
            ServerID = 1,
            HostAccountName = "TestHost",
            Map = "caldavar",
            MapVersion = "1.0",
            Version = "4.10.1",
            GameMode = "normal",
            TimePlayed = 1200,
            FileSize = 1024,
            FileName = "M963564308.honreplay",
            ConnectionState = 0,
            AveragePSR = 1500,
            AveragePSRTeamOne = 1500,
            AveragePSRTeamTwo = 1500,
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
        await dbContext.SaveChangesAsync();

        Dictionary<string, string> payload = new()
        {
            { "match_id", matchID.ToString() }, 
            { "cookie", "" }
        };

        // Act
        // Pass "f" as "Get_Match_Stats" (mixed case)
        HttpResponseMessage response =
            await client.PostAsync("/client_requester.php?f=Get_Match_Stats", new FormUrlEncodedContent(payload));
        
        // Assert
        // If the controller is case-sensitive, this will be 401 Unauthorized.
        // We WANT it to be OK.
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }
}
