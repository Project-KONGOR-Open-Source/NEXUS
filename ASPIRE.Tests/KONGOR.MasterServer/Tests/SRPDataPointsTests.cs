using System.Security.Cryptography;
using ASPIRE.Tests.KONGOR.MasterServer.Services;
using KONGOR.MasterServer.Handlers.SRP;
using MERRICK.DatabaseContext.Entities.Core;
using MERRICK.DatabaseContext.Entities.Statistics;
using MERRICK.DatabaseContext.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

public sealed class SRPDataPointsTests
{
    [Test]
    public async Task AuthenticateWithSRP_ReturnsCorrectDataPoints_WhenStatisticsExist()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory =
            KONGORServiceProvider.CreateOrchestratedInstance();

        SRPAuthenticationService srpAuthenticationService = new(webApplicationFactory);
        string email = "datapoints@kongor.com";
        string accountName = "DataPointsPlayer";
        string password = "Password123!";

        // 1. Create Account
        (Account account, _) = await srpAuthenticationService.CreateAccountWithSRPCredentials(email, accountName, password);

        // 2. Seed AccountStatistics
        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        List<AccountStatistics> stats =
        [
            new AccountStatistics
            {
                AccountID = account.ID,
                Account = account,
                Type = AccountStatisticsType.Public,
                MatchesPlayed = 10,
                MatchesWon = 5,
                MatchesLost = 5,
                MatchesDisconnected = 1,
                SkillRating = 1600.5,
                PerformanceScore = 0.0,
                PlacementMatchesData = ""
            },
            new AccountStatistics
            {
                AccountID = account.ID,
                Account = account,
                Type = AccountStatisticsType.Matchmaking,
                MatchesPlayed = 20,
                MatchesWon = 12,
                MatchesLost = 8,
                MatchesDisconnected = 0,
                SkillRating = 1750.0,
                PerformanceScore = 0.0,
                PlacementMatchesData = ""
            },
             new AccountStatistics
            {
                AccountID = account.ID,
                Account = account,
                Type = AccountStatisticsType.MidWars,
                MatchesPlayed = 5,
                MatchesWon = 3,
                MatchesLost = 2,
                MatchesDisconnected = 2,
                SkillRating = 1400.0,
                PerformanceScore = 0.0,
                PlacementMatchesData = ""
            }
        ];

        await databaseContext.AccountStatistics.AddRangeAsync(stats);

        // Update Account AscensionLevel
        account.AscensionLevel = 5;
        // Update User Experience
        account.User.TotalExperience = 1000;

        await databaseContext.SaveChangesAsync();

        // 3. Perform Authentication
        HttpClient httpClient = webApplicationFactory.CreateClient();
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "S2 Games/Heroes Of Newerth/4.10.1.0/wac/x86_64");

        // Pre-Auth
        SrpParameters parameters = SrpParameters.Create<SHA256>(SRPAuthenticationSessionDataStageOne.SafePrimeNumber, SRPAuthenticationSessionDataStageOne.MultiplicativeGroupGenerator);
        parameters.Generator = parameters.Pad(parameters.Generator);
        SrpClient client = new(parameters);
        SrpEphemeral clientEphemeral = client.GenerateEphemeral();

        Dictionary<string, string> preAuthFormData = new()
        {
            { "login", accountName },
            { "A", clientEphemeral.Public },
            { "SysInfo", "test|sys|info|hash|mac" }
        };
        HttpResponseMessage preAuthResponse = await httpClient.PostAsync("client_requester.php?f=pre_auth", new FormUrlEncodedContent(preAuthFormData));
        string preAuthBody = await preAuthResponse.Content.ReadAsStringAsync();
        IDictionary<object, object>? preAuthData = PhpSerialization.Deserialize(preAuthBody) as IDictionary<object, object>;

        await Assert.That(preAuthData).IsNotNull();

        string sessionSalt = preAuthData!["salt"] as string ?? throw new InvalidOperationException("Salt is null");
        string passwordSalt = preAuthData["salt2"] as string ?? throw new InvalidOperationException("Salt2 is null");
        string serverPublicEphemeral = preAuthData["B"] as string ?? throw new InvalidOperationException("B is null");

        string passwordHash = SRPRegistrationHandlers.ComputeSRPPasswordHash(password, passwordSalt);
        string privateClientKey = client.DerivePrivateKey(sessionSalt, accountName, passwordHash);
        SrpSession clientSession = client.DeriveSession(clientEphemeral.Secret, serverPublicEphemeral, sessionSalt, accountName, privateClientKey);

        // Auth
        Dictionary<string, string> authFormData = new()
        {
            { "login", accountName },
            { "proof", clientSession.Proof },
            { "OSType", "windows" },
            { "MajorVersion", "4" },
            { "MinorVersion", "10" },
            { "MicroVersion", "1" },
            { "SysInfo", "testhash|testhash|testhash|testhash|testhash" }
        };

        HttpResponseMessage authResponse = await httpClient.PostAsync("client_requester.php?f=srpAuth", new FormUrlEncodedContent(authFormData));
        string authResponseBody = await authResponse.Content.ReadAsStringAsync();

        await Assert.That(authResponse.IsSuccessStatusCode).IsTrue();

        IDictionary<object, object>? stageTwoData = PhpSerialization.Deserialize(authResponseBody) as IDictionary<object, object>;

        // 4. Verify DataPoints
        await Assert.That(stageTwoData).IsNotNull();
        await Assert.That(stageTwoData!.ContainsKey("infos")).IsTrue();

        object infos = stageTwoData["infos"];
        IDictionary<object, object>? dataPoint = null;

        if (infos is IList<object> list)
        {
            dataPoint = list[0] as IDictionary<object, object>;
        }
        else if (infos is IDictionary<object, object> dict)
        {
             // PHP arrays can be deserialized as Dict<int, object> or Dict<string, object>
             dataPoint = dict.Values.Cast<IDictionary<object, object>>().First();
        }

        await Assert.That(dataPoint).IsNotNull();

        // Verify Mapped Fields
        using (Assert.Multiple())
        {
            await Assert.That(dataPoint!["level"]).IsEqualTo("5"); // AscensionLevel
            await Assert.That(dataPoint["level_exp"]).IsEqualTo("1000"); // User.TotalExperience

            // Disconnects (Total = 1 + 0 + 2 = 3)
            await Assert.That(dataPoint["discos"]).IsEqualTo("3");

            // MatchesPlayed (Total = 10 + 20 + 5 = 35)
            await Assert.That(dataPoint["games_played"]).IsEqualTo("35");

            // Public Stats
            await Assert.That(dataPoint["acc_pub_skill"]).IsEqualTo("1600.500");
            await Assert.That(dataPoint["acc_games_played"]).IsEqualTo("10");
            await Assert.That(dataPoint["acc_wins"]).IsEqualTo("5");
            await Assert.That(dataPoint["acc_losses"]).IsEqualTo("5");
            await Assert.That(dataPoint["acc_discos"]).IsEqualTo("1");

            // Matchmaking Stats
            await Assert.That(dataPoint["rnk_amm_team_rating"]).IsEqualTo("1750.000");
            await Assert.That(dataPoint["rnk_games_played"]).IsEqualTo("20");
            await Assert.That(dataPoint["rnk_wins"]).IsEqualTo("12");
            await Assert.That(dataPoint["rnk_losses"]).IsEqualTo("8");
            await Assert.That(dataPoint["rnk_discos"]).IsEqualTo("0");

            // MidWars Stats
            await Assert.That(dataPoint["mid_amm_team_rating"]).IsEqualTo("1400.000");
            await Assert.That(dataPoint["mid_games_played"]).IsEqualTo("5");
            await Assert.That(dataPoint["mid_discos"]).IsEqualTo("2");

            // Unset stats should be defaults
            await Assert.That(dataPoint["rift_amm_team_rating"]).IsEqualTo("1500.000");
            await Assert.That(dataPoint["rift_games_played"]).IsEqualTo("0");
        }
    }
}
