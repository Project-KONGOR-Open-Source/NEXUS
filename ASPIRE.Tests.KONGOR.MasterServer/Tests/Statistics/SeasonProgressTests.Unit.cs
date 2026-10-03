namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Statistics;

/// <summary>
///     Pure-logic tests for the seasonal campaign progression block ("campaign_info") attached to each match stats player row.
///     Only ranked matchmaking matches participate in the seasonal campaign, so the block must be empty for every other match type, and the medal and placement values must come from the statistics row matching the match type that was actually played.
/// </summary>
public sealed class SeasonProgressTests_Unit
{
    private static User BuildUser() => new ()
    {
        EmailAddress = "season.tests@kongor.com",
        Role = new Role { Name = UserRoles.User },
        SRPPasswordSalt = "salt",
        SRPPasswordHash = "hash",
        OwnedStoreItems = []
    };

    private static Account BuildAccount() => new () { Name = "SeasonTester", User = BuildUser(), IsMain = true };

    private static AccountStatistics BuildStatistics(AccountStatisticsType type, double skillRating, string? placementMatchesData) => new ()
    {
        Account = BuildAccount(),
        Type = type,
        SkillRating = skillRating,
        PlacementMatchesData = placementMatchesData
    };

    private static MatchPlayerStatistics BuildPlayerRow(MatchType matchType, string map, AccountStatistics currentMatchTypeStatistics, AccountStatistics matchmakingStatistics, bool isCasual = false)
    {
        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(matchType, isCasual, map);

        MatchParticipantStatistics participant = MatchDataHelper.BuildParticipant(accountID: 1, accountName: "SeasonTester", groupNumber: 1);

        AccountStatistics publicStatistics = BuildStatistics(AccountStatisticsType.Public, 1500, null);

        return new MatchPlayerStatistics(matchInformation, BuildAccount(), participant, currentMatchTypeStatistics, publicStatistics, matchmakingStatistics) { HeroIdentifier = participant.HeroIdentifier };
    }

    [Test]
    public async Task Campaign_Info_Is_Populated_For_A_Ranked_Matchmaking_Match()
    {
        AccountStatistics matchmakingStatistics = BuildStatistics(AccountStatisticsType.Matchmaking, 1600, "110101");

        MatchPlayerStatistics playerRow = BuildPlayerRow(MatchType.AM_MATCHMAKING, "caldavar", matchmakingStatistics, matchmakingStatistics);

        using (Assert.Multiple())
        {
            await Assert.That(playerRow.SeasonProgress.PlacementMatches).IsEqualTo(AccountStatistics.ExpectedPlacementMatchCount);
            await Assert.That(playerRow.SeasonProgress.PlacementWins).IsEqualTo("110101");
            await Assert.That(playerRow.SeasonProgress.MMRAfter).IsEqualTo("1600");
            await Assert.That(playerRow.SeasonProgress.MedalAfter).IsNotEqualTo("0");
        }
    }

    [Test]
    public async Task Campaign_Info_Is_Empty_For_A_MidWars_Match()
    {
        AccountStatistics midWarsStatistics = BuildStatistics(AccountStatisticsType.MidWars, 1669, null);

        // The Ranked Row Is Mid-Placement, Which Previously Leaked A Placement Progress Block Into MidWars Match Stats
        AccountStatistics matchmakingStatistics = BuildStatistics(AccountStatisticsType.Matchmaking, 1600, "110");

        MatchPlayerStatistics playerRow = BuildPlayerRow(MatchType.AM_MATCHMAKING_MIDWARS, "midwars", midWarsStatistics, matchmakingStatistics);

        using (Assert.Multiple())
        {
            await Assert.That(playerRow.SeasonProgress.PlacementMatches).IsEqualTo(0);
            await Assert.That(playerRow.SeasonProgress.PlacementWins).IsEqualTo(string.Empty);
            await Assert.That(playerRow.SeasonProgress.MMRAfter).IsEqualTo("0");
            await Assert.That(playerRow.SeasonProgress.MedalAfter).IsEqualTo("0");
        }
    }

    [Test]
    public async Task Campaign_Info_For_A_Casual_Match_Uses_The_Casual_Statistics_Row()
    {
        AccountStatistics casualStatistics = BuildStatistics(AccountStatisticsType.MatchmakingCasual, 1700, "111111");

        AccountStatistics matchmakingStatistics = BuildStatistics(AccountStatisticsType.Matchmaking, 1500, "111111");

        MatchPlayerStatistics playerRow = BuildPlayerRow(MatchType.AM_MATCHMAKING, "caldavar_old", casualStatistics, matchmakingStatistics, isCasual: true);

        using (Assert.Multiple())
        {
            await Assert.That(playerRow.SeasonProgress.IsCasual).IsEqualTo("1");
            await Assert.That(playerRow.SeasonProgress.MMRAfter).IsEqualTo("1700");
        }
    }
}
