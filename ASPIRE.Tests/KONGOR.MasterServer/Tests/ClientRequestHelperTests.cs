using KONGOR.MasterServer.Models.RequestResponse.Stats;
using KONGOR.MasterServer.Services.Requester;

using MERRICK.DatabaseContext.Entities;
using MERRICK.DatabaseContext.Entities.Statistics;

namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

public class ClientRequestHelperTests
{
    [Test]
    public async Task CreateShowSimpleStatsResponse_ReturnsAwardsInFixedOrder()
    {
        // Arrange
        // Create an account and some stats
        Account account = new()
        {
            ID = 123,
            Name = "TestPlayer",
            Type = AccountType.Normal,
            IsMain = true,
            User = new User
            {
                EmailAddress = "test@example.com",
                Role = new Role { ID = 1, Name = "User" },
                PBKDF2PasswordHash = "hash",
                SRPPasswordHash = "hash",
                SRPPasswordSalt = "salt",
                TotalLevel = 10,
                TotalExperience = 1000,
                OwnedStoreItems = new List<string>()
            }
        };

        // Create player stats aggregated DTO
        // Fixed Order: Smackdown, Annihilation, Assists, Kills
        PlayerStatisticsAggregatedDTO stats = new()
        {
            Smackdowns = 1,
            Annihilations = 5,
            Assists = 100,
            Kills = 50,
            RankedWins = 1,
            RankedMatches = 1
        };

        // Act
        ShowSimpleStatsResponse response = ClientRequestHelper.CreateShowSimpleStatsResponse(account, stats, 12);

        // Assert
        // 1. Verify Names are in the exact fixed order
        await Assert.That(response.Top4AwardNames).IsNotNull();
        await Assert.That(response.Top4AwardNames.Count).IsEqualTo(4);

        await Assert.That(response.Top4AwardNames[0]).IsEqualTo("awd_msd");
        await Assert.That(response.Top4AwardNames[1]).IsEqualTo("awd_mann");
        await Assert.That(response.Top4AwardNames[2]).IsEqualTo("awd_masst");
        await Assert.That(response.Top4AwardNames[3]).IsEqualTo("awd_mkill");

        // 2. Verify Counts match the keys
        await Assert.That(response.Top4AwardCounts[0]).IsEqualTo(1);   // Smackdown
        await Assert.That(response.Top4AwardCounts[1]).IsEqualTo(5);   // Annihilation
        await Assert.That(response.Top4AwardCounts[2]).IsEqualTo(100); // Assists
        await Assert.That(response.Top4AwardCounts[3]).IsEqualTo(50);  // Kills
    }
}
