using KONGOR.MasterServer.Models.RequestResponse.Stats;
using KONGOR.MasterServer.Services;

using MERRICK.DatabaseContext.Entities.Statistics;

using Moq;

namespace ASPIRE.Tests.KONGOR.MasterServer.Tests
{
    public class StatsExtensionTests
    {
        [Test]
        public async Task ToPlayerStatistics_ResolvesHeroId_WhenPayloadIdIsZero()
        {
            // Arrange
            Mock<IHeroDefinitionService> mockHeroService = new Mock<IHeroDefinitionService>();
            mockHeroService.Setup(s => s.GetBaseHeroId("Hero_Valkyrie")).Returns(34);

            StatsForSubmissionRequestForm form = new StatsForSubmissionRequestForm
            {
                Function = "submit_stats",
                MatchStats = new Dictionary<string, string> { { "match_id", "12345" } },
                TeamStats = [],
                PlayerStats = new Dictionary<int, Dictionary<string, Dictionary<string, string>>>
                {
                    {
                        0, new Dictionary<string, Dictionary<string, string>>
                        {
                            {
                                "Hero_Valkyrie", new Dictionary<string, string>
                                {
                                    { "hero_id", "0" }, // The bug: Public games send 0
                                    { "nickname", "TestUser" },
                                    { "team", "1" },
                                    { "position", "0" },
                                    { "group_num", "1" },
                                    { "benefit", "0" },
                                    { "wins", "1" },
                                    { "losses", "0" },
                                    { "discos", "0" },
                                    { "concedes", "0" },
                                    { "kicked", "0" },
                                    { "social_bonus", "0" },
                                    { "used_token", "0" },
                                    { "concedevotes", "0" },
                                    { "herokills", "10" },
                                    { "herodmg", "5000" },
                                    { "herokillsgold", "2000" },
                                    { "heroassists", "5" },
                                    { "heroexp", "8000" },
                                    { "deaths", "2" },
                                    { "buybacks", "0" },
                                    { "goldlost2death", "100" },
                                    { "secs_dead", "60" },
                                    { "teamcreepkills", "0" },
                                    { "teamcreepdmg", "0" },
                                    { "teamcreepgold", "0" },
                                    { "teamcreepexp", "0" },
                                    { "neutralcreepkills", "20" },
                                    { "neutralcreepdmg", "2000" },
                                    { "neutralcreepgold", "500" },
                                    { "neutralcreepexp", "600" },
                                    { "bdmg", "1000" },
                                    { "razed", "1" },
                                    { "bdmgexp", "200" },
                                    { "bgold", "300" },
                                    { "denies", "5" },
                                    { "exp_denied", "100" },
                                    { "gold", "12000" },
                                    { "gold_spent", "11000" },
                                    { "exp", "9000" },
                                    { "actions", "1500" },
                                    { "secs", "1800" },
                                    { "level", "15" },
                                    { "consumables", "3" },
                                    { "wards", "2" },
                                    { "bloodlust", "0" },
                                    { "doublekill", "1" },
                                    { "triplekill", "0" },
                                    { "quadkill", "0" },
                                    { "annihilation", "0" },
                                    { "ks3", "1" },
                                    { "ks4", "0" },
                                    { "ks5", "0" },
                                    { "ks6", "0" },
                                    { "ks7", "0" },
                                    { "ks8", "0" },
                                    { "ks9", "0" },
                                    { "ks10", "0" },
                                    { "ks15", "0" },
                                    { "smackdown", "0" },
                                    { "humiliation", "0" },
                                    { "nemesis", "0" },
                                    { "retribution", "0" },
                                    { "score", "500" },
                                    { "gameplaystat0", "0" },
                                    { "gameplaystat1", "0" },
                                    { "gameplaystat2", "0" },
                                    { "gameplaystat3", "0" },
                                    { "gameplaystat4", "0" },
                                    { "gameplaystat5", "0" },
                                    { "gameplaystat6", "0" },
                                    { "gameplaystat7", "0" },
                                    { "gameplaystat8", "0" },
                                    { "gameplaystat9", "0" },
                                    { "time_earning_exp", "1200" }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            PlayerStatistics result = form.ToPlayerStatistics(0, 100, "TestUser", null, null, mockHeroService.Object);

            // Assert
            await Assert.That(result.HeroProductID).IsEqualTo(34u); // Should be resolved to Valkyrie
            mockHeroService.Verify(s => s.GetBaseHeroId("Hero_Valkyrie"), Times.Once);
        }

        [Test]
        public async Task ToPlayerStatistics_UsesPayloadId_WhenItIsValid()
        {
             // Arrange
            Mock<IHeroDefinitionService> mockHeroService = new Mock<IHeroDefinitionService>();
            
            StatsForSubmissionRequestForm form = new StatsForSubmissionRequestForm
            {
                Function = "submit_stats",
                MatchStats = new Dictionary<string, string> { { "match_id", "12345" } },
                TeamStats = [],
                PlayerStats = new Dictionary<int, Dictionary<string, Dictionary<string, string>>>
                {
                    {
                        0, new Dictionary<string, Dictionary<string, string>>
                        {
                            {
                                "Hero_Legionnaire", new Dictionary<string, string> // Identifier doesn't matter here if ID is valid
                                {
                                    { "hero_id", "123" }, // Valid ID
                                    { "nickname", "TestUser" },
                                    { "team", "1" },
                                    { "position", "0" },
                                    { "group_num", "1" },
                                    { "benefit", "0" },
                                    { "wins", "1" },
                                    { "losses", "0" },
                                    { "discos", "0" },
                                    { "concedes", "0" },
                                    { "kicked", "0" },
                                    { "social_bonus", "0" },
                                    { "used_token", "0" },
                                    { "concedevotes", "0" },
                                    { "herokills", "0" },
                                    { "herodmg", "0" },
                                    { "herokillsgold", "0" },
                                    { "heroassists", "0" },
                                    { "heroexp", "0" },
                                    { "deaths", "0" },
                                    { "buybacks", "0" },
                                    { "goldlost2death", "0" },
                                    { "secs_dead", "0" },
                                    { "teamcreepkills", "0" },
                                    { "teamcreepdmg", "0" },
                                    { "teamcreepgold", "0" },
                                    { "teamcreepexp", "0" },
                                    { "neutralcreepkills", "0" },
                                    { "neutralcreepdmg", "0" },
                                    { "neutralcreepgold", "0" },
                                    { "neutralcreepexp", "0" },
                                    { "bdmg", "0" },
                                    { "razed", "0" },
                                    { "bdmgexp", "0" },
                                    { "bgold", "0" },
                                    { "denies", "0" },
                                    { "exp_denied", "0" },
                                    { "gold", "0" },
                                    { "gold_spent", "0" },
                                    { "exp", "0" },
                                    { "actions", "0" },
                                    { "secs", "0" },
                                    { "level", "1" },
                                    { "consumables", "0" },
                                    { "wards", "0" },
                                    { "bloodlust", "0" },
                                    { "doublekill", "0" },
                                    { "triplekill", "0" },
                                    { "quadkill", "0" },
                                    { "annihilation", "0" },
                                    { "ks3", "0" },
                                    { "ks4", "0" },
                                    { "ks5", "0" },
                                    { "ks6", "0" },
                                    { "ks7", "0" },
                                    { "ks8", "0" },
                                    { "ks9", "0" },
                                    { "ks10", "0" },
                                    { "ks15", "0" },
                                    { "smackdown", "0" },
                                    { "humiliation", "0" },
                                    { "nemesis", "0" },
                                    { "retribution", "0" },
                                    { "score", "0" },
                                    { "gameplaystat0", "0" },
                                    { "gameplaystat1", "0" },
                                    { "gameplaystat2", "0" },
                                    { "gameplaystat3", "0" },
                                    { "gameplaystat4", "0" },
                                    { "gameplaystat5", "0" },
                                    { "gameplaystat6", "0" },
                                    { "gameplaystat7", "0" },
                                    { "gameplaystat8", "0" },
                                    { "gameplaystat9", "0" },
                                    { "time_earning_exp", "0" }
                                }
                            }
                        }
                    }
                }
            };
            
            // Act
            PlayerStatistics result = form.ToPlayerStatistics(0, 100, "TestUser", null, null, mockHeroService.Object);
            
            // Assert
            await Assert.That(result.HeroProductID).IsEqualTo(123u);
            mockHeroService.Verify(s => s.GetBaseHeroId(It.IsAny<string>()), Times.Never);
        }
    }
}
