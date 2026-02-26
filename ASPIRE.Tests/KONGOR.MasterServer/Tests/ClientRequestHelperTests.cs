using KONGOR.MasterServer.Models.RequestResponse.Stats;
using KONGOR.MasterServer.Services;
using KONGOR.MasterServer.Services.Requester;

using MERRICK.DatabaseContext.Entities;
using MERRICK.DatabaseContext.Entities.Statistics;

namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

public class ClientRequestHelperTests
{
    [Test]
    public async Task CreateShowSimpleStatsResponse_ReturnsSortedStats()
    {
        // Arrange
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
        PlayerStatisticsAggregatedDTO stats = new()
        {
            Smackdowns = 1,
            Annihilations = 5,
            Assists = 100,
            Kills = 50,
            RankedWins = 1,
            RankedMatches = 1,
            RankedSeconds = 1000, 
            CasualSeconds = 1000,
            
            // Input unordered to verify sorting
            TopHeroes = new List<FavHeroDTO>
            {
                new() { HeroId = 3, SecondsPlayed = 200 },  // Least (3rd)
                new() { HeroId = 1, SecondsPlayed = 1000 }, // Most (1st)
                new() { HeroId = 2, SecondsPlayed = 500 }   // Middle (2nd)
            }
        };

        IHeroDefinitionService heroDefinitions = new TestHeroDefinitionService();

        // Act
        ShowSimpleStatsResponse response = ClientRequestHelper.CreateShowSimpleStatsResponse(account, stats, 12, heroDefinitions);

        // Assert
        // 1. Verify Awards are sorted by Count Descending
        // Expected: Assists (100) > Kills (50) > Annihilations (5) > Smackdowns (1)
        await Assert.That(response.Top4AwardNames.Count).IsEqualTo(4);

        await Assert.That(response.Top4AwardNames[0]).IsEqualTo("awd_masst"); // 100
        await Assert.That(response.Top4AwardNames[1]).IsEqualTo("awd_mkill"); // 50
        await Assert.That(response.Top4AwardNames[2]).IsEqualTo("awd_mann");  // 5
        await Assert.That(response.Top4AwardNames[3]).IsEqualTo("awd_msd");   // 1

        await Assert.That(response.Top4AwardCounts[0]).IsEqualTo("100");
        await Assert.That(response.Top4AwardCounts[1]).IsEqualTo("50");
        await Assert.That(response.Top4AwardCounts[2]).IsEqualTo("5");
        await Assert.That(response.Top4AwardCounts[3]).IsEqualTo("1");
        
        // 2. Verify Heroes are sorted by Time Descending
        // Expected: Hero 1 (1000s) > Hero 2 (500s) > Hero 3 (200s)
        await Assert.That(response.FavHero1).IsEqualTo("jereziah");      // Hero 1
        await Assert.That(response.FavHero1Time).IsEqualTo(50);          // 1000/2000 * 100
        
        await Assert.That(response.FavHero2).IsEqualTo("legionnaire");   // Hero 2
        await Assert.That(response.FavHero2Time).IsEqualTo(25);          // 500/2000 * 100
        
        await Assert.That(response.FavHero3).IsEqualTo("magmar");        // Hero 3
        await Assert.That(response.FavHero3Time).IsEqualTo(10);          // 200/2000 * 100
    }

    private class TestHeroDefinitionService : IHeroDefinitionService
    {
        public string GetHeroIdentifier(uint heroId)
        {
            return heroId switch
            {
                1 => "Hero_Jereziah",
                2 => "Hero_Legionnaire",
                3 => "Hero_Magmar",
                4 => "Hero_PuppetMaster",
                5 => "Hero_Devourer",
                _ => $"Hero_{heroId}"
            };
        }
        public uint GetBaseHeroId(uint productId) => productId;
        public uint GetBaseHeroId(string identifier) => 0;
        public bool IsHero(uint heroId) => true;
        public IEnumerable<uint> GetAllHeroIds() => Enumerable.Empty<uint>();
    }
}
