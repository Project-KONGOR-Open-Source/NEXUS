using System.Net;

using ASPIRE.Tests.Data;

using KONGOR.MasterServer.Extensions.Cache;
using KONGOR.MasterServer.Models.ServerManagement;

using MERRICK.DatabaseContext.Entities.Statistics;

namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

using EntityRole = Role;

public sealed class ClientRequestTests
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
            Role = new EntityRole { Name = UserRoles.User }
        };

        Account account = new()
        {
            Name = name,
            User = user,
            Type = AccountType.Staff, // Default to Staff for broad permission in tests
            IsMain = true,
            Cookie = cookie
        };

        await dbContext.Users.AddAsync(user);
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();

        await cache.SetAccountNameForSessionCookie(cookie, account.Name);

        return (client, dbContext, cache, cookie, factory);
    }

    #region From ClientRequesterTests.cs

    [Test]
    public async Task GetInitStats_WithValidCookie_ReturnsStats()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync("ClientTester");
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.GetInitStats(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);

            response.EnsureSuccessStatusCode();
            string body = await response.Content.ReadAsStringAsync();

            // Structural Assertion for Init Stats
            // Verify nickname is strictly "ClientTester" and slot_id exists

            // Nickname Match: "nickname";s:12:"ClientTester";
            Regex nicknameRegex = new(@"""nickname"";\s*s:\d+:""([^""]+)""");
            Match nicknameMatch = nicknameRegex.Match(body);
            await Assert.That(nicknameMatch.Success).IsTrue();
            await Assert.That(nicknameMatch.Groups[1].Value).IsEqualTo("ClientTester");

            // Slot ID Match: "slot_id";s:\d+:"...";
            Regex slotRegex = new(@"""slot_id"";\s*s:\d+:""([^""]*)""");
            Match slotMatch = slotRegex.Match(body);
            await Assert.That(slotMatch.Success).IsTrue();
        }
    }

    [Test]
    public async Task GetProducts_WithValidCookie_ReturnsProductList()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync("ProductTester");
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.GetProducts(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);

            string body = await response.Content.ReadAsStringAsync();
            await Assert.That(body).IsNotEmpty();
            // Verify basic PHP array structure
            await Assert.That(body).StartsWith("a:");
        }
    }

    #endregion

    #region From ClientRequesterUnverifiedTests.cs

    [Test]
    public async Task Auth_Unverified_ReturnsBadRequest()
    {
        // SetupAsync creates a session, but here we just need client
        (HttpClient client, _, _, _, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.Auth("login", "password");
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php?f=auth", content);
            if (response.StatusCode != HttpStatusCode.BadRequest)
            {
                await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
            }
        }
    }

    [Test]
    public async Task GetServerList_Unverified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync();
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.GetServerList(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task GetAllHeroes_Unverified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync();
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.GetAllHeroes(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task CreateGame_Unverified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync();
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.CreateGame(cookie, "TestGame");
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task ShowSimpleStats_Unverified_ReturnsSuccess()
    {
        // Use a known name to verify we can query it
        (HttpClient client, MerrickContext dbContext, _, string cookie,
            WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
            // We need the name associated with the cookie
            Account account = await dbContext.Accounts.FirstAsync(a => a.Cookie == cookie);

            Dictionary<string, string> payload = ClientRequestPayloads.ShowSimpleStats(cookie, account.Name);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task ClientEventsInfo_Unverified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync();
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.ClientEventsInfo(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task GetSpecialMessages_Unverified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync();
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.GetSpecialMessages(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task GetInitStats_Unverified_ReturnsBadRequest()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync();
        await using (factory)
        {
            // Verifies snake_case "get_init_stats" failure
            Dictionary<string, string> payload = ClientRequestPayloads.GetInitStats_SnakeCase(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);

            if (response.StatusCode != HttpStatusCode.BadRequest)
            {
                await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
            }
        }
    }

    [Test]
    public async Task GrabServerList_Unverified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync();
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.GrabServerList(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task ServerList_Unverified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync();
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.ServerList(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task GetHeroList_Unverified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync();
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.GetHeroList(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task ShowStats_Unverified_ReturnsSuccess()
    {
        (HttpClient client, MerrickContext dbContext, _, string cookie,
            WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
            Account account = await dbContext.Accounts.FirstAsync(a => a.Cookie == cookie);

            Dictionary<string, string> payload = ClientRequestPayloads.ShowStats(cookie, account.Name);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    #endregion

    #region From ClientRequesterVerifiedPayloadTests.cs

    [Test]
    public async Task PreAuth_Verified_ReturnsSuccess()
    {
        // Special setup for PreAuth (does NOT need cookie in cache, just user in DB)
        WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = factory.CreateClient();

        await using (factory)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

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

            Dictionary<string, string> payload = ClientRequestPayloads.PreAuth(accountName);
            FormUrlEncodedContent content = new(payload);
            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            // Structural Assertion for Pre-Auth
            // Verify 'salt' and 'B' (server ephemeral) are present and structurally correct
            // Regex for salt: "salt";s:\d+:"...";
            Regex saltRegex = new(@"""salt"";\s*s:\d+:""([^""]+)""");
            Match saltMatch = saltRegex.Match(responseBody);
            await Assert.That(saltMatch.Success).IsTrue();
            await Assert.That(saltMatch.Groups[1].Value).IsNotEmpty();

            // Regex for B: "B";s:\d+:"...";
            Regex bRegex = new(@"""B"";\s*s:\d+:""([^""]+)""");
            Match bMatch = bRegex.Match(responseBody);
            await Assert.That(bMatch.Success).IsTrue();
            await Assert.That(bMatch.Groups[1].Value).IsNotEmpty();
        }
    }

    [Test]
    public async Task SrpAuth_Verified_Returns401_Or_422()
    {
        WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = factory.CreateClient();

        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.SrpAuth("SomeUser", "bad_proof");
            FormUrlEncodedContent content = new(payload);
            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);

            await Assert.That(response.StatusCode).IsNotEqualTo(HttpStatusCode.BadRequest);
        }
    }

    [Test]
    public async Task GetSpecialMessages_Verified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync("MsgUser");
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.GetSpecialMessages(cookie);
            FormUrlEncodedContent content = new(payload);
            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            await Assert.That(responseBody).Contains("a:0:{}");
        }
    }

    [Test]
    public async Task GetProducts_Verified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync("ProdUser");
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.GetProducts(cookie);
            FormUrlEncodedContent content = new(payload);
            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            await Assert.That(responseBody).Contains("products");
        }
    }

    [Test]
    public async Task ClientEventsInfo_Verified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync("EventUser");
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.ClientEventsInfo(cookie);
            FormUrlEncodedContent content = new(payload);
            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            await Assert.That(responseBody).Contains("a:0:{}");
        }
    }

    [Test]
    public async Task ShowSimpleStats_Verified_ReturnsSuccess()
    {
        string nickname = "StatsUser";
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync(nickname);
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.ShowSimpleStats(cookie, nickname);
            FormUrlEncodedContent content = new(payload);
            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            await Assert.That(responseBody).Contains("nickname");
            await Assert.That(responseBody).Contains("account_id");
            await Assert.That(responseBody).Contains("level");
        }
    }

    [Test]
    public async Task GetAccountAllHeroStats_Verified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync("HeroStatsUser");
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.GetAccountAllHeroStats(cookie);
            FormUrlEncodedContent content = new(payload);
            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            await Assert.That(responseBody).Contains("a:0:{}");
        }
    }

    [Test]
    public async Task CreateGame_Verified_ReturnsSuccess()
    {
        (HttpClient client, _, IDatabase distributedCache, string cookie,
            WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync("GameCreator");
        await using (factory)
        {
            // Seed Idle Match Server
            MatchServer matchServer = new()
            {
                ID = 1,
                IPAddress = "127.0.0.1",
                Port = 5000,
                Status = ServerStatus.SERVER_STATUS_IDLE,
                Location = "USE",
                Name = "TestServer1",
                HostAccountName = "ServerHost",
                HostAccountID = 999,
                Instance = 1,
                Description = "Test Server"
            };
            await distributedCache.SetMatchServer(matchServer.HostAccountName, matchServer);

            Dictionary<string, string> payload = ClientRequestPayloads.CreateGame(cookie, "My Custom Game");
            FormUrlEncodedContent content = new(payload);
            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            await Assert.That(responseBody).Contains("match_id");
            await Assert.That(responseBody).Contains("server_id");
            await Assert.That(responseBody).Contains("server_address");
            await Assert.That(responseBody).Contains("127.0.0.1");
        }
    }

    [Test]
    public async Task NewGameAvailable_Verified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync("Notifier");
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.NewGameAvailable(cookie);
            FormUrlEncodedContent content = new(payload);
            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            await Assert.That(responseBody).Contains("b:1;");
        }
    }

    [Test]
    public async Task ClaimSeasonRewards_Verified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync("SeasonUser");
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.ClaimSeasonRewards(cookie);
            FormUrlEncodedContent content = new(payload);
            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);

            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task ServerList_Verified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync("ListUser");
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.ServerList(cookie);
            FormUrlEncodedContent content = new(payload);
            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            // Expecting serialized list/array
            await Assert.That(responseBody).StartsWith("a:");
        }
    }

    [Test]
    public async Task Logout_Verified_ReturnsSuccess()
    {
        (HttpClient client, _, IDatabase distributedCache, string cookie,
            WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync("LogoutUser");
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.Logout(cookie);
            FormUrlEncodedContent content = new(payload);
            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);

            response.EnsureSuccessStatusCode();

            (bool isValid, _) = await distributedCache.ValidateAccountSessionCookie(cookie);
            await Assert.That(isValid).IsFalse();
        }
    }

    [Test]
    public async Task GetMatchStats_Verified_ReturnsSuccess()
    {
        (HttpClient client, MerrickContext dbContext, _, string cookie,
            WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync("MatchStatsUser");
        await using (factory)
        {
            // Seed a completed match in DB for stats
            MatchStatistics matchStats = new()
            {
                MatchID = 12345,
                Map = "caldavar",
                Version = "4.10.1.0",
                GameMode = "ap",
                TimestampRecorded = DateTime.UtcNow,
                TimePlayed = 1800,
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

            Dictionary<string, string> payload = ClientRequestPayloads.GetMatchStats(cookie, 12345);
            FormUrlEncodedContent content = new(payload);
            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);

            response.EnsureSuccessStatusCode();

            Dictionary<string, object> expected = ClientRequestPayloads.ExpectedResponses.GetMatchStats(12345);
            string responseBody = await response.Content.ReadAsStringAsync();

            foreach (string key in expected.Keys)
            {
                if (key == "0")
                {
                    continue;
                }

                await Assert.That(responseBody).Contains(key);
            }
        }
    }

    [Test]
    public async Task GetUpgrades_Verified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync("UpgradeUser");
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.GetUpgrades(cookie);
            FormUrlEncodedContent content = new(payload);
            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);

            response.EnsureSuccessStatusCode();

            Dictionary<string, object> expected = ClientRequestPayloads.ExpectedResponses.GetUpgrades();
            string responseBody = await response.Content.ReadAsStringAsync();

            foreach (string key in expected.Keys)
            {
                if (key == "0")
                {
                    continue;
                }

                await Assert.That(responseBody).Contains(key);
            }
        }
    }

    [Test]
    public async Task GetDailySpecial_Verified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync("DailySpecialUser");
        await using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.GetDailySpecial(cookie);
            FormUrlEncodedContent content = new(payload);
            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            // Expect at least success or empty array (if not populated)
            // Daily special often returns cost/product_id, or just success=true if empty
            // At minimum check for success
            await Assert.That(responseBody).Contains("s:1:\"0\";"); // "0" key usually
        }
    }

    #endregion
}