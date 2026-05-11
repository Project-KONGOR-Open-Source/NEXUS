namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Locks down both compatibility predicates used during team formation:
///     <list type="bullet">
///         <item><see cref="MatchmakingGroup.IsCompatibleWith"/> — a strict full check used elsewhere in the chat server.</item>
///         <item><see cref="MatchmakingAlgorithm.HasCompatibleQueuePreferences"/> — a relaxed check (no size/TMR filter) used inside <see cref="MatchmakingAlgorithm.FormTeams"/> when stitching multiple groups onto one team.</item>
///     </list>
/// </summary>
public sealed class GroupCompatibilityTests
{
    [Test]
    public async Task Queue_Preferences_Same_Mode_Same_Region_Same_Ranked_Are_Compatible()
    {
        MatchmakingGroup left  = MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR);
        MatchmakingGroup right = MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR);

        await Assert.That(MatchmakingAlgorithm.HasCompatibleQueuePreferences(left, right)).IsTrue();
    }

    [Test]
    public async Task Queue_Preferences_Disjoint_Game_Modes_Are_Not_Compatible()
    {
        MatchmakingGroupInformation apOnly = MatchmakingTestBuilder.Information(gameModes: ["ap"]);
        MatchmakingGroupInformation sdOnly = MatchmakingTestBuilder.Information(gameModes: ["sd"]);

        MatchmakingGroup left  = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: apOnly);
        MatchmakingGroup right = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: sdOnly);

        await Assert.That(MatchmakingAlgorithm.HasCompatibleQueuePreferences(left, right)).IsFalse();
    }

    [Test]
    public async Task Queue_Preferences_Overlapping_Game_Modes_Are_Compatible()
    {
        MatchmakingGroupInformation apOrSd = MatchmakingTestBuilder.Information(gameModes: ["ap", "sd"]);
        MatchmakingGroupInformation sdOrBd = MatchmakingTestBuilder.Information(gameModes: ["sd", "bd"]);

        MatchmakingGroup left  = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: apOrSd);
        MatchmakingGroup right = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: sdOrBd);

        await Assert.That(MatchmakingAlgorithm.HasCompatibleQueuePreferences(left, right)).IsTrue();
    }

    [Test]
    public async Task Queue_Preferences_Disjoint_Regions_Without_Newerth_Wildcard_Are_Not_Compatible()
    {
        MatchmakingGroupInformation useOnly = MatchmakingTestBuilder.Information(gameRegions: ["USE"]);
        MatchmakingGroupInformation euOnly  = MatchmakingTestBuilder.Information(gameRegions: ["EU"]);

        MatchmakingGroup left  = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: useOnly);
        MatchmakingGroup right = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: euOnly);

        await Assert.That(MatchmakingAlgorithm.HasCompatibleQueuePreferences(left, right)).IsFalse();
    }

    [Test]
    public async Task Queue_Preferences_Newerth_Wildcard_Overlaps_Any_Region()
    {
        MatchmakingGroupInformation newerth = MatchmakingTestBuilder.Information(gameRegions: ["NEWERTH"]);
        MatchmakingGroupInformation euOnly  = MatchmakingTestBuilder.Information(gameRegions: ["EU"]);

        MatchmakingGroup left  = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: newerth);
        MatchmakingGroup right = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: euOnly);

        await Assert.That(MatchmakingAlgorithm.HasCompatibleQueuePreferences(left, right)).IsTrue();
    }

    [Test]
    public async Task Queue_Preferences_Differing_Ranked_Status_Are_Not_Compatible()
    {
        MatchmakingGroupInformation ranked   = MatchmakingTestBuilder.Information(ranked: true);
        MatchmakingGroupInformation unranked = MatchmakingTestBuilder.Information(ranked: false);

        MatchmakingGroup left  = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: ranked);
        MatchmakingGroup right = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: unranked);

        await Assert.That(MatchmakingAlgorithm.HasCompatibleQueuePreferences(left, right)).IsFalse();
    }

    [Test]
    public async Task Group_Is_Compatible_With_Differing_Game_Type_Is_Not_Compatible()
    {
        MatchmakingGroupInformation normal  = MatchmakingTestBuilder.Information(gameType: ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL);
        MatchmakingGroupInformation midwars = MatchmakingTestBuilder.Information(gameType: ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS);

        MatchmakingGroup left  = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: normal);
        MatchmakingGroup right = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: midwars);

        await Assert.That(left.IsCompatibleWith(right)).IsFalse();
    }

    [Test]
    public async Task Group_Is_Compatible_With_Combined_Size_Exceeds_Team_Size_Is_Not_Compatible()
    {
        // Two Trios Cannot Combine For A 5-Player Team (3 + 3 = 6 > 5)

        MatchmakingGroup left  = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]);
        MatchmakingGroup right = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]);

        await Assert.That(left.IsCompatibleWith(right)).IsFalse();
    }
}
