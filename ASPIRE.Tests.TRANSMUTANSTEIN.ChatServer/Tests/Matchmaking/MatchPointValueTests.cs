namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Locks down <see cref="MatchmakingMatch.AssignMatchPointValues"/>: the Elo-style win/loss point values derived from the matchup prediction, the provisional and high-rating K-factor adjustments, and the minimum-TMR loss protection.
/// </summary>
public sealed class MatchPointValueTests
{
    /// <summary>
    ///     Tolerance for point-value comparisons since the values are computed from <see cref="Math.Exp"/> via the matchup prediction.
    /// </summary>
    private const double PointValueTolerance = 0.0001;

    [Test]
    public async Task Evenly_Matched_Established_Players_Gain_And_Lose_Half_The_Base_K_Factor()
    {
        MatchmakingMatch match = BuildMirroredMatch([1500.0, 1500.0, 1500.0, 1500.0, 1500.0]);

        using (Assert.Multiple())
        {
            foreach (MatchmakingGroupMember member in match.GetAllPlayers())
            {
                await Assert.That(Math.Abs(member.MatchWinValue - 5.0) < PointValueTolerance).IsTrue();
                await Assert.That(Math.Abs(member.MatchLossValue - -5.0) < PointValueTolerance).IsTrue();
                await Assert.That(member.IsProvisional).IsFalse();
            }
        }
    }

    [Test]
    public async Task The_Favoured_Team_Gains_Less_For_A_Win_And_Loses_More_For_A_Loss()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        MatchmakingTeam favouredTeam = BuildFullStackTeam([1600.0, 1600.0, 1600.0, 1600.0, 1600.0]);
        MatchmakingTeam underdogTeam = BuildFullStackTeam([1500.0, 1500.0, 1500.0, 1500.0, 1500.0]);

        MatchmakingMatch match = MatchmakingMatch.FromTeams(favouredTeam, underdogTeam, settings.LogisticPredictionScale);

        match.GameType = ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL;
        match.AssignMatchPointValues(settings);

        MatchmakingGroupMember favouredMember = favouredTeam.GetAllMembers().First();
        MatchmakingGroupMember underdogMember = underdogTeam.GetAllMembers().First();

        using (Assert.Multiple())
        {
            await Assert.That(favouredMember.MatchWinValue).IsLessThan(underdogMember.MatchWinValue);
            await Assert.That(favouredMember.MatchLossValue).IsLessThan(underdogMember.MatchLossValue);
            await Assert.That(Math.Abs(favouredMember.MatchWinValue + underdogMember.MatchWinValue - 10.0) < PointValueTolerance).IsTrue();
        }
    }

    [Test]
    public async Task A_Provisional_Player_Receives_The_Provisional_K_Factor_Multiplier()
    {
        MatchmakingMatch match = BuildMirroredFourPlusOneMatch(soloMatchCount: 0);

        MatchmakingGroupMember provisionalMember = match.LegionTeam.GetAllMembers().Single(member => member.GameTypeMatchCount == 0);
        MatchmakingGroupMember establishedMember = match.LegionTeam.GetAllMembers().First(member => member.GameTypeMatchCount > 0);

        await Assert.That(provisionalMember.IsProvisional).IsTrue();

        using (Assert.Multiple())
        {
            await Assert.That(Math.Abs(provisionalMember.MatchWinValue - 10.0) < PointValueTolerance).IsTrue();
            await Assert.That(Math.Abs(provisionalMember.MatchLossValue - -10.0) < PointValueTolerance).IsTrue();
            await Assert.That(Math.Abs(establishedMember.MatchWinValue - 5.0) < PointValueTolerance).IsTrue();
            await Assert.That(establishedMember.IsProvisional).IsFalse();
        }
    }

    [Test]
    public async Task The_Provisional_Phase_Does_Not_Apply_To_MidWars()
    {
        MatchmakingMatch match = BuildMirroredFourPlusOneMatch(soloMatchCount: 0, gameType: ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS);

        MatchmakingGroupMember newPlayer = match.LegionTeam.GetAllMembers().Single(member => member.GameTypeMatchCount == 0);

        using (Assert.Multiple())
        {
            await Assert.That(newPlayer.IsProvisional).IsFalse();
            await Assert.That(Math.Abs(newPlayer.MatchWinValue - 5.0) < PointValueTolerance).IsTrue();
            await Assert.That(Math.Abs(newPlayer.MatchLossValue - -5.0) < PointValueTolerance).IsTrue();
        }
    }

    [Test]
    public async Task A_Highly_Rated_Player_Receives_A_Reduced_K_Factor()
    {
        // The Reduction Ramps Linearly Over 300 TMR Above The Cutoff (1600), So A 1900 TMR Player Gets The Full Reduction: K = 10 - 10 * 0.20 = 8
        MatchmakingMatch match = BuildMirroredMatch([1900.0, 1400.0, 1400.0, 1400.0, 1400.0]);

        MatchmakingGroupMember highlyRatedMember = match.LegionTeam.GetAllMembers().Single(member => member.TMR == 1900.0);
        MatchmakingGroupMember regularMember = match.LegionTeam.GetAllMembers().First(member => member.TMR == 1400.0);

        using (Assert.Multiple())
        {
            await Assert.That(Math.Abs(highlyRatedMember.MatchWinValue - 4.0) < PointValueTolerance).IsTrue();
            await Assert.That(Math.Abs(highlyRatedMember.MatchLossValue - -4.0) < PointValueTolerance).IsTrue();
            await Assert.That(Math.Abs(regularMember.MatchWinValue - 5.0) < PointValueTolerance).IsTrue();
        }
    }

    [Test]
    public async Task A_Loss_Never_Takes_A_Player_Below_The_Minimum_TMR()
    {
        // A 1003 TMR Player In An Even Match Would Lose 5, Which Would Take Them Below The Minimum Of 1000, So The Loss Is Reduced To 3
        MatchmakingMatch match = BuildMirroredMatch([1003.0, 1003.0, 1003.0, 1003.0, 1003.0]);

        using (Assert.Multiple())
        {
            foreach (MatchmakingGroupMember member in match.GetAllPlayers())
            {
                await Assert.That(Math.Abs(member.MatchWinValue - 5.0) < PointValueTolerance).IsTrue();
                await Assert.That(Math.Abs(member.MatchLossValue - -3.0) < PointValueTolerance).IsTrue();
            }
        }
    }

    [Test]
    public async Task A_Player_At_The_Minimum_TMR_Loses_Nothing()
    {
        MatchmakingMatch match = BuildMirroredMatch([1000.0, 1000.0, 1000.0, 1000.0, 1000.0]);

        using (Assert.Multiple())
        {
            foreach (MatchmakingGroupMember member in match.GetAllPlayers())
            {
                await Assert.That(Math.Abs(member.MatchWinValue - 5.0) < PointValueTolerance).IsTrue();
                await Assert.That(member.MatchLossValue).IsEqualTo(0.0);
            }
        }
    }

    [Test]
    public async Task A_Bot_Match_Does_Not_Assign_Match_Point_Values()
    {
        MatchmakingGroup coopGroup = MatchmakingTestBuilder.BuildGroup([1500.0], information: MatchmakingTestBuilder.CoopInformation());

        MatchmakingMatch match = MatchmakingMatch.FromBotGroup(coopGroup);

        match.AssignMatchPointValues(MatchmakingTestBuilder.DefaultSettings());

        MatchmakingGroupMember member = match.GetAllPlayers().Single();

        using (Assert.Multiple())
        {
            await Assert.That(member.MatchWinValue).IsEqualTo(0.0);
            await Assert.That(member.MatchLossValue).IsEqualTo(0.0);
            await Assert.That(member.IsProvisional).IsFalse();
        }
    }

    /// <summary>
    ///     Builds a match between two identically-composed 5-stacks with the supplied member TMRs, so the matchup prediction is exactly 50%.
    /// </summary>
    private static MatchmakingMatch BuildMirroredMatch(IReadOnlyList<double> memberTMRs)
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        MatchmakingMatch match = MatchmakingMatch.FromTeams(BuildFullStackTeam(memberTMRs), BuildFullStackTeam(memberTMRs), settings.LogisticPredictionScale);

        match.GameType = ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL;
        match.AssignMatchPointValues(settings);

        return match;
    }

    /// <summary>
    ///     Builds a match between two identically-composed 4+1 teams at baseline TMR, where each team's solo has the supplied match count, so the matchup prediction is exactly 50%.
    /// </summary>
    private static MatchmakingMatch BuildMirroredFourPlusOneMatch(int soloMatchCount, ChatProtocol.TMMGameType gameType = ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL)
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        MatchmakingMatch match = MatchmakingMatch.FromTeams(BuildFourPlusOneTeam(soloMatchCount), BuildFourPlusOneTeam(soloMatchCount), settings.LogisticPredictionScale);

        match.GameType = gameType;
        match.AssignMatchPointValues(settings);

        return match;
    }

    private static MatchmakingTeam BuildFullStackTeam(IReadOnlyList<double> memberTMRs)
        => MatchmakingTeam.FromGroups([MatchmakingTestBuilder.BuildGroup(memberTMRs)], teamSize: 5);

    private static MatchmakingTeam BuildFourPlusOneTeam(int soloMatchCount)
    {
        MatchmakingGroup fourStack = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]);
        MatchmakingGroup solo = MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR, totalMatchCount: soloMatchCount);

        return MatchmakingTeam.FromGroups([fourStack, solo], teamSize: 5);
    }
}
