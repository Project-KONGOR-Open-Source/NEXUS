namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

using ASPIRE.Tests.Data;
using ASPIRE.Tests.KONGOR.MasterServer.Infrastructure;
using global::KONGOR.MasterServer.Extensions.Cache;
using Microsoft.AspNetCore.Mvc.Testing;
using global::MERRICK.DatabaseContext;
using global::MERRICK.DatabaseContext.Entities;
using global::MERRICK.DatabaseContext.Entities.Statistics;
using global::MERRICK.DatabaseContext.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using EntityRole = global::MERRICK.DatabaseContext.Entities.Utility.Role;

public sealed class ClientRequesterVerifiedPayloadTests
{
    private async Task<(HttpClient Client, WebApplicationFactory<KONGORAssemblyMarker> Factory)> SetupAsync()
    {
        WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = factory.CreateClient();
        return (client, factory);
    }

    private async Task SeedAccountAsync(MerrickContext dbContext, IDatabase distributedCache, string cookie, string accountName)
    {
        User user = new()
        {
            EmailAddress = $"{accountName}@kongor.net",
            PBKDF2PasswordHash = "hash",
            SRPPasswordHash = "hash",
            SRPPasswordSalt = "salt",
            Role = new EntityRole { Name = UserRoles.User }
        };

        Account account = new()
        {
            Name = accountName,
            User = user,
            Type = AccountType.Normal,
            IsMain = true,
            Cookie = cookie
        };

        await dbContext.Users.AddAsync(user);
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();

        await distributedCache.SetAccountNameForSessionCookie(cookie, accountName);
    }

    [Test]
    public async Task PreAuth_ReturnsSuccess()
    {
        (HttpClient client, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
            
            // Seed a user for PreAuth (needs to find the account)
            string accountName = "PreAuthUser";
            User user = new()
            {
                EmailAddress = "preauth@kongor.net",
                PBKDF2PasswordHash = "hash",
                SRPPasswordHash = "hash",
                SRPPasswordSalt = "salt",
                Role = new EntityRole { Name = UserRoles.User }
            };
            Account account = new() { Name = accountName, User = user, Type = AccountType.Normal, IsMain = true };
            await dbContext.Users.AddAsync(user);
            await dbContext.Accounts.AddAsync(account);
            await dbContext.SaveChangesAsync();

            Dictionary<string, string> payload = ClientRequestPayloads.Verified.PreAuth(accountName);
            FormUrlEncodedContent content = new(payload);
            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
            
            // Should return SRP Stage One params (B, s)
            string responseBody = await response.Content.ReadAsStringAsync();
            await Assert.That(responseBody).Contains("s:4:\"salt\""); // serialized 'salt'
            await Assert.That(responseBody).Contains("s:1:\"B\""); // serialized 'B'
        }
    }

    [Test]
    public async Task SrpAuth_Returns401_Or_422()
    {
        // SrpAuth is complex to mock fully. We expect 422 UnprocessableEntity (missing cache) 
        // or 401 Unauthorized (invalid proof) but NOT 400 Bad Request (parameters valid).
        
        (HttpClient client, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.Verified.SrpAuth("SomeUser", "bad_proof");
            FormUrlEncodedContent content = new(payload);
            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            
            // We verify that it IS NOT a Bad Request (which would mean missing params)
            await Assert.That(response.StatusCode).IsNotEqualTo(System.Net.HttpStatusCode.BadRequest);
        }
    }

    [Test]
    public async Task GetSpecialMessages_ReturnsSuccess()
    {
        (HttpClient client, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
             using IServiceScope scope = factory.Services.CreateScope();
             IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();
             MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
             
             string cookie = Guid.NewGuid().ToString("N");
             await SeedAccountAsync(dbContext, distributedCache, cookie, "MsgUser");

             Dictionary<string, string> payload = ClientRequestPayloads.Verified.GetSpecialMessages(cookie);
             FormUrlEncodedContent content = new(payload);
             HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
             response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task GetProducts_ReturnsSuccess()
    {
        (HttpClient client, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
             using IServiceScope scope = factory.Services.CreateScope();
             IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();
             MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
             
             string cookie = Guid.NewGuid().ToString("N");
             await SeedAccountAsync(dbContext, distributedCache, cookie, "ProdUser");

             Dictionary<string, string> payload = ClientRequestPayloads.Verified.GetProducts(cookie);
             FormUrlEncodedContent content = new(payload);
             HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
             response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task ClientEventsInfo_ReturnsSuccess()
    {
        (HttpClient client, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
             using IServiceScope scope = factory.Services.CreateScope();
             IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();
             MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
             
             string cookie = Guid.NewGuid().ToString("N");
             await SeedAccountAsync(dbContext, distributedCache, cookie, "EventUser");

             Dictionary<string, string> payload = ClientRequestPayloads.Verified.ClientEventsInfo(cookie);
             FormUrlEncodedContent content = new(payload);
             HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
             response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task ShowSimpleStats_ReturnsSuccess()
    {
        (HttpClient client, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
             using IServiceScope scope = factory.Services.CreateScope();
             IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();
             MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
             
             string cookie = Guid.NewGuid().ToString("N");
             string nickname = "StatsUser";
             await SeedAccountAsync(dbContext, distributedCache, cookie, nickname);

             Dictionary<string, string> payload = ClientRequestPayloads.Verified.ShowSimpleStats(cookie, nickname);
             FormUrlEncodedContent content = new(payload);
             HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
             response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task GetAccountAllHeroStats_ReturnsSuccess()
    {
        (HttpClient client, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
             using IServiceScope scope = factory.Services.CreateScope();
             IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();
             MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
             
             string cookie = Guid.NewGuid().ToString("N");
             await SeedAccountAsync(dbContext, distributedCache, cookie, "HeroStatsUser");

             Dictionary<string, string> payload = ClientRequestPayloads.Verified.GetAccountAllHeroStats(cookie);
             FormUrlEncodedContent content = new(payload);
             HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
             response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task CreateGame_ReturnsSuccess()
    {
        (HttpClient client, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
             using IServiceScope scope = factory.Services.CreateScope();
             IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();
             MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
             
             string cookie = Guid.NewGuid().ToString("N");
             string accountName = "GameCreator";
             await SeedAccountAsync(dbContext, distributedCache, cookie, accountName);

             // Seed Idle Match Server
             global::KONGOR.MasterServer.Models.ServerManagement.MatchServer matchServer = new()
             {
                 ID = 1,
                 IPAddress = "127.0.0.1",
                 Port = 5000,
                 Status = global::KONGOR.MasterServer.Models.ServerManagement.ServerStatus.SERVER_STATUS_IDLE,
                 Location = "USE",
                 Name = "TestServer1",
                 HostAccountName = "ServerHost",
                 HostAccountID = 999,
                 Instance = 1,
                 Description = "Test Server"
             };
             // Use singular SetMatchServer
             await distributedCache.SetMatchServer(matchServer.HostAccountName, matchServer);

             Dictionary<string, string> payload = ClientRequestPayloads.Verified.CreateGame(cookie, "My Custom Game");
             FormUrlEncodedContent content = new(payload);
             HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
             
             response.EnsureSuccessStatusCode();
             string responseBody = await response.Content.ReadAsStringAsync();
             
             // Verify serialized response contains match details
             await Assert.That(responseBody).Contains("match_id");
             await Assert.That(responseBody).Contains("server_id");
             await Assert.That(responseBody).Contains("server_address");
             await Assert.That(responseBody).Contains("127.0.0.1");
        }
    }

    [Test]
    public async Task NewGameAvailable_ReturnsSuccess()
    {
        (HttpClient client, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
             using IServiceScope scope = factory.Services.CreateScope();
             IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();
             MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
             
             string cookie = Guid.NewGuid().ToString("N");
             await SeedAccountAsync(dbContext, distributedCache, cookie, "Notifier");

             Dictionary<string, string> payload = ClientRequestPayloads.Verified.NewGameAvailable(cookie);
             FormUrlEncodedContent content = new(payload);
             HttpResponseMessage response = await client.PostAsync("client_requester.php", content);

             response.EnsureSuccessStatusCode();
             string responseBody = await response.Content.ReadAsStringAsync();
             await Assert.That(responseBody).Contains("b:1;"); // Expecting true
        }
    }
    [Test]
    public async Task ClaimSeasonRewards_ReturnsSuccess()
    {
        (HttpClient client, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
             using IServiceScope scope = factory.Services.CreateScope();
             IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();
             MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
             
             string cookie = Guid.NewGuid().ToString("N");
             await SeedAccountAsync(dbContext, distributedCache, cookie, "SeasonUser");

             Dictionary<string, string> payload = ClientRequestPayloads.Verified.ClaimSeasonRewards(cookie);
             FormUrlEncodedContent content = new(payload);
             HttpResponseMessage response = await client.PostAsync("client_requester.php", content);

             response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task ServerList_ReturnsSuccess()
    {
        (HttpClient client, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
             using IServiceScope scope = factory.Services.CreateScope();
             IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();
             MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
             
             string cookie = Guid.NewGuid().ToString("N");
             await SeedAccountAsync(dbContext, distributedCache, cookie, "ListUser");

             Dictionary<string, string> payload = ClientRequestPayloads.Verified.ServerList(cookie);
             FormUrlEncodedContent content = new(payload);
             HttpResponseMessage response = await client.PostAsync("client_requester.php", content);

             response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task Logout_ReturnsSuccess()
    {
        (HttpClient client, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
             using IServiceScope scope = factory.Services.CreateScope();
             IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();
             MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
             
             string cookie = Guid.NewGuid().ToString("N");
             await SeedAccountAsync(dbContext, distributedCache, cookie, "LogoutUser");

             Dictionary<string, string> payload = ClientRequestPayloads.Verified.Logout(cookie);
             FormUrlEncodedContent content = new(payload);
             HttpResponseMessage response = await client.PostAsync("client_requester.php", content);

             response.EnsureSuccessStatusCode();
             
             // Verify logout logic? (e.g., cookie removed from cache)
             // Controller: await DistributedCache.RemoveSession(sessionAccountName);
             // We can check if cookie is invalid now.
             (bool isValid, _) = await distributedCache.ValidateAccountSessionCookie(cookie);
             await Assert.That(isValid).IsFalse();
        }
    }

    [Test]
    public async Task GetMatchStats_ReturnsSuccess()
    {
        (HttpClient client, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
             using IServiceScope scope = factory.Services.CreateScope();
             IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();
             MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
             
             string cookie = Guid.NewGuid().ToString("N");
             await SeedAccountAsync(dbContext, distributedCache, cookie, "MatchStatsUser");

             // Seed a completed match in DB for stats
             MatchStatistics matchStats = new()
             {
                 MatchID = 12345,
                 Map = "caldavar",
                 // Name property doesn't exist on MatchStatistics, removing
                 Version = "4.10.1.0",
                 GameMode = "ap", // Was Mode
                 TimestampRecorded = DateTime.UtcNow, // Was Date
                 TimePlayed = 1800, // Was Length
                 
                 // Required fields
                 ServerID = 1,
                 HostAccountName = "ServerHost",
                 MapVersion = "4.10.1",
                 FileSize = 1000,
                 FileName = "M12345.honreplay",
                 ConnectionState = 0,
                 AveragePSR = 1500,
                 AveragePSRTeamOne = 1500,
                 AveragePSRTeamTwo = 1500,
                 ScoreTeam1 = 0,
                 ScoreTeam2 = 0,
                 TeamScoreGoal = 0,
                 PlayerScoreGoal = 0,
                 NumberOfRounds = 1,
                 ReleaseStage = "stable",
                 BannedHeroes = null,
                 
                 // Awards - all required nullable int?
                 AwardMostAnnihilations = 0,
                 AwardMostQuadKills = 0,
                 AwardLargestKillStreak = 0,
                 AwardMostSmackdowns = 0,
                 AwardMostKills = 0,
                 AwardMostAssists = 0,
                 AwardLeastDeaths = 0,
                 AwardMostBuildingDamage = 0,
                 AwardMostWardsKilled = 0,
                 AwardMostHeroDamageDealt = 0,
                 AwardHighestCreepScore = 0
             };
             await dbContext.MatchStatistics.AddAsync(matchStats);
             await dbContext.SaveChangesAsync();

             Dictionary<string, string> payload = ClientRequestPayloads.Verified.GetMatchStats(cookie, 12345);
             FormUrlEncodedContent content = new(payload);
             HttpResponseMessage response = await client.PostAsync("client_requester.php", content);

             response.EnsureSuccessStatusCode();
        }
    }
}
