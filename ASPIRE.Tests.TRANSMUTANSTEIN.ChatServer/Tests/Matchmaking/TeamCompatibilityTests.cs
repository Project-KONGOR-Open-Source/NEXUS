namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Locks down the team-level compatibility enforced when pairing two formed teams (shared game modes, shared regions with the "NEWERTH" wildcard, and ranked status), the narrowing of shared preferences during team formation, and the selection of the match's mode and region from the preferences shared by both teams.
/// </summary>
public sealed class TeamCompatibilityTests
{
    [Test]
    public async Task Teams_With_Disjoint_Game_Modes_Do_Not_Match()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        MatchmakingGroup apStack = BuildFullStack(MatchmakingTestBuilder.Information(gameModes: ["ap"]));
        MatchmakingGroup sdStack = BuildFullStack(MatchmakingTestBuilder.Information(gameModes: ["sd"]));

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle([apStack, sdStack], settings);

        await Assert.That(matches.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Teams_With_Disjoint_Regions_Do_Not_Match()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        MatchmakingGroup useStack = BuildFullStack(MatchmakingTestBuilder.Information(gameRegions: ["USE"]));
        MatchmakingGroup euStack  = BuildFullStack(MatchmakingTestBuilder.Information(gameRegions: ["EU"]));

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle([useStack, euStack], settings);

        await Assert.That(matches.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Teams_In_The_Same_Region_Aggregate_Match()
    {
        // "USE" And "USW" Both Belong To The "US" Aggregate, So The Teams Are Compatible And The Match's Region Is The Aggregate

        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        MatchmakingGroup useStack = BuildFullStack(MatchmakingTestBuilder.Information(gameRegions: ["USE"]));
        MatchmakingGroup uswStack = BuildFullStack(MatchmakingTestBuilder.Information(gameRegions: ["USW"]));

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle([useStack, uswStack], settings);

        using (Assert.Multiple())
        {
            await Assert.That(matches.Count).IsEqualTo(1);
            await Assert.That(matches[0].SelectedRegion).IsEqualTo("US");
        }
    }

    [Test]
    public async Task Ranked_And_Unranked_Full_Stacks_Do_Not_Match()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        MatchmakingGroup rankedStack   = BuildFullStack(MatchmakingTestBuilder.Information(ranked: true));
        MatchmakingGroup unrankedStack = BuildFullStack(MatchmakingTestBuilder.Information(ranked: false));

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle([rankedStack, unrankedStack], settings);

        await Assert.That(matches.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Selected_Mode_Comes_From_The_Modes_Shared_By_Both_Teams()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        MatchmakingGroup flexibleStack = BuildFullStack(MatchmakingTestBuilder.Information(gameModes: ["ap", "sd"]));
        MatchmakingGroup sdOnlyStack   = BuildFullStack(MatchmakingTestBuilder.Information(gameModes: ["sd"]));

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle([flexibleStack, sdOnlyStack], settings);

        using (Assert.Multiple())
        {
            await Assert.That(matches.Count).IsEqualTo(1);
            await Assert.That(matches[0].SelectedMode).IsEqualTo("sd");
        }
    }

    [Test]
    public async Task Selected_Region_Comes_From_The_Regions_Shared_By_Both_Teams()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        MatchmakingGroup wildcardStack = BuildFullStack(MatchmakingTestBuilder.Information(gameRegions: ["NEWERTH"]));
        MatchmakingGroup euOnlyStack   = BuildFullStack(MatchmakingTestBuilder.Information(gameRegions: ["EU"]));

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle([wildcardStack, euOnlyStack], settings);

        using (Assert.Multiple())
        {
            await Assert.That(matches.Count).IsEqualTo(1);
            await Assert.That(matches[0].SelectedRegion).IsEqualTo("EU");
        }
    }

    [Test]
    public async Task Team_Formation_Narrows_Shared_Game_Modes_Across_All_Groups()
    {
        // The Anchor Solo Accepts Both Modes; Once An "ap"-Only Solo Joins, The Team's Shared Modes Narrow To "ap" And The "sd"-Only Solo Can No Longer Join

        MatchmakingGroupInformation apOrSd = MatchmakingTestBuilder.Information(gameModes: ["ap", "sd"]);
        MatchmakingGroupInformation apOnly = MatchmakingTestBuilder.Information(gameModes: ["ap"]);
        MatchmakingGroupInformation sdOnly = MatchmakingTestBuilder.Information(gameModes: ["sd"]);

        MatchmakingGroup anchorSolo = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], queuedMinutesAgo: 6, information: apOrSd);
        MatchmakingGroup secondSolo = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], queuedMinutesAgo: 5, information: apOnly);
        MatchmakingGroup thirdSolo  = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], queuedMinutesAgo: 4, information: sdOnly);
        MatchmakingGroup fourthSolo = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], queuedMinutesAgo: 3, information: apOnly);
        MatchmakingGroup fifthSolo  = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], queuedMinutesAgo: 2, information: apOnly);
        MatchmakingGroup sixthSolo  = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], queuedMinutesAgo: 1, information: apOnly);

        List<MatchmakingGroup> queue = [anchorSolo, secondSolo, thirdSolo, fourthSolo, fifthSolo, sixthSolo];

        IReadOnlyList<MatchmakingTeam> teams = MatchmakingAlgorithm.FormTeams(queue, playersPerTeam: 5);

        using (Assert.Multiple())
        {
            await Assert.That(teams.Count).IsEqualTo(1);
            await Assert.That(teams[0].Groups.Any(group => group.GUID == thirdSolo.GUID)).IsFalse();
            await Assert.That(teams[0].CommonGameModes.Single()).IsEqualTo("ap");
        }
    }

    [Test]
    public async Task Intersect_Game_Regions_Wildcard_Defers_To_The_Other_Side()
    {
        string[] commonRegions = MatchmakingTeam.IntersectGameRegions(["NEWERTH"], ["EU"]);

        await Assert.That(commonRegions.Single()).IsEqualTo("EU");
    }

    [Test]
    public async Task Intersect_Game_Regions_Wildcard_On_Both_Sides_Survives()
    {
        string[] commonRegions = MatchmakingTeam.IntersectGameRegions(["NEWERTH"], ["NEWERTH"]);

        await Assert.That(commonRegions.Single()).IsEqualTo("NEWERTH");
    }

    [Test]
    public async Task Intersect_Game_Regions_Without_Wildcard_Intersects()
    {
        string[] commonRegions = MatchmakingTeam.IntersectGameRegions(["USE", "EU"], ["EU", "SEA"]);

        await Assert.That(commonRegions.Single()).IsEqualTo("EU");
    }

    [Test]
    public async Task Intersect_Game_Regions_Disjoint_Without_Wildcard_Is_Empty()
    {
        string[] commonRegions = MatchmakingTeam.IntersectGameRegions(["USE"], ["EU"]);

        await Assert.That(commonRegions.Length).IsEqualTo(0);
    }

    private static MatchmakingGroup BuildFullStack(MatchmakingGroupInformation information)
        => MatchmakingTestBuilder.BuildGroup([.. Enumerable.Repeat(MatchmakingTestBuilder.BaselineTMR, 5)], information: information);
}
