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
        // The Players Are Solos So The High-Rated Player's Distance From The Team Average Does Not Trip The Coordination Penalty, Which Only Applies Within Multi-Player Groups
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        MatchmakingMatch match = MatchmakingMatch.FromTeams(BuildSoloTeam([1900.0, 1400.0, 1400.0, 1400.0, 1400.0]), BuildSoloTeam([1900.0, 1400.0, 1400.0, 1400.0, 1400.0]), settings.LogisticPredictionScale);

        match.GameType = ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL;
        match.AssignMatchPointValues(settings);

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
    public async Task A_Provisional_Loss_Near_The_Minimum_TMR_Does_Not_Drop_Below_It()
    {
        // A Provisional Solo At 1015 TMR Carries The Doubled K-Factor (20) And, On A Heavily-Favoured Team, Would Lose Close To The Full 20, Taking Them To 995
        // The Below-Minimum Guard Must Reduce The Loss To 15, Leaving Them Exactly At The 1000 Minimum

        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        MatchmakingGroup fourStack = MatchmakingTestBuilder.BuildGroup([1900.0, 1900.0, 1900.0, 1900.0]);
        MatchmakingGroup provisionalSolo = MatchmakingTestBuilder.BuildSoloGroup(1015.0, totalMatchCount: 0);

        MatchmakingTeam favouredTeam = MatchmakingTeam.FromGroups([fourStack, provisionalSolo], teamSize: 5);
        MatchmakingTeam underdogTeam = BuildFullStackTeam([1500.0, 1500.0, 1500.0, 1500.0, 1500.0]);

        MatchmakingMatch match = MatchmakingMatch.FromTeams(favouredTeam, underdogTeam, settings.LogisticPredictionScale);

        match.GameType = ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL;
        match.AssignMatchPointValues(settings);

        MatchmakingGroupMember provisionalMember = match.LegionTeam.GetAllMembers().Single(member => member.TMR == 1015.0);

        using (Assert.Multiple())
        {
            await Assert.That(provisionalMember.IsProvisional).IsTrue();
            await Assert.That(Math.Abs(provisionalMember.MatchLossValue - -15.0) < PointValueTolerance).IsTrue();
            await Assert.That(provisionalMember.TMR + provisionalMember.MatchLossValue >= settings.MinimumTMR).IsTrue();
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
    public async Task A_Small_Group_Team_Facing_A_Pre_Made_Stack_Loses_Half_The_Rating()
    {
        // A 2+2+1 Team (Makeup 9) Faces A Full Five-Stack (Makeup 25); With The Feature Enabled The Small-Group Team's Loss Is Halved While The Stack's Loss Is Unchanged

        MatchmakingTeam smallGroupTeam = MatchmakingTeam.FromGroups(
        [
            MatchmakingTestBuilder.BuildGroup([1500.0, 1500.0]),
            MatchmakingTestBuilder.BuildGroup([1500.0, 1500.0]),
            MatchmakingTestBuilder.BuildSoloGroup(1500.0)
        ], teamSize: 5);

        MatchmakingTeam stackTeam = BuildFullStackTeam([1500.0, 1500.0, 1500.0, 1500.0, 1500.0]);

        MatchmakingMatch match = MatchmakingMatch.FromTeams(smallGroupTeam, stackTeam, MatchmakingTestBuilder.DefaultSettings().LogisticPredictionScale);

        match.GameType = ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL;

        MatchmakingSettings reductionEnabled = MatchmakingTestBuilder.DefaultSettings();
        MatchmakingSettings reductionDisabled = MatchmakingTestBuilder.DefaultSettings();

        reductionDisabled.ReducedLossForSmallGroupsEnabled = false;

        match.AssignMatchPointValues(reductionEnabled);

        double[] smallGroupLossEnabled = [.. match.LegionTeam.GetAllMembers().Select(member => member.MatchLossValue)];
        double[] smallGroupWinEnabled = [.. match.LegionTeam.GetAllMembers().Select(member => member.MatchWinValue)];
        double[] stackLossEnabled = [.. match.HellbourneTeam!.GetAllMembers().Select(member => member.MatchLossValue)];

        match.AssignMatchPointValues(reductionDisabled);

        double[] smallGroupLossDisabled = [.. match.LegionTeam.GetAllMembers().Select(member => member.MatchLossValue)];
        double[] smallGroupWinDisabled = [.. match.LegionTeam.GetAllMembers().Select(member => member.MatchWinValue)];
        double[] stackLossDisabled = [.. match.HellbourneTeam!.GetAllMembers().Select(member => member.MatchLossValue)];

        using (Assert.Multiple())
        {
            for (int index = 0; index < smallGroupLossEnabled.Length; index++)
            {
                // The Small-Group Team's Loss Is Meaningful And Exactly Halved
                await Assert.That(smallGroupLossDisabled[index]).IsLessThan(0.0);
                await Assert.That(Math.Abs(smallGroupLossEnabled[index] - smallGroupLossDisabled[index] * 0.5) < PointValueTolerance).IsTrue();

                // The Win Value Is Untouched By The Loss Reduction
                await Assert.That(Math.Abs(smallGroupWinEnabled[index] - smallGroupWinDisabled[index]) < PointValueTolerance).IsTrue();
            }

            // The Pre-Made Stack Receives No Loss Reduction
            for (int index = 0; index < stackLossEnabled.Length; index++)
                await Assert.That(Math.Abs(stackLossEnabled[index] - stackLossDisabled[index]) < PointValueTolerance).IsTrue();
        }
    }

    [Test]
    public async Task A_Boosting_Group_Has_Its_Rating_Gains_And_Losses_Reduced()
    {
        // A 2000 TMR Player Grouped With A 1200 TMR Friend Forms A Wide-Spread Pre-Made; Both Sit Far From The Team Average And Have Their Gains And Losses Cut To The Curve's Floor Of A Tenth
        // The Solo Players On The Same Team, At The Team Average, Are In Single-Player Groups And So Are Untouched; The Opponent Is Five Solos So The Small-Group Loss Reduction Does Not Also Apply

        MatchmakingGroup boostingDuo = MatchmakingTestBuilder.BuildGroup([2000.0, 1200.0]);

        MatchmakingTeam boostingTeam = MatchmakingTeam.FromGroups(
        [
            boostingDuo,
            MatchmakingTestBuilder.BuildSoloGroup(1500.0),
            MatchmakingTestBuilder.BuildSoloGroup(1500.0),
            MatchmakingTestBuilder.BuildSoloGroup(1500.0)
        ], teamSize: 5);

        MatchmakingTeam opponentTeam = BuildSoloTeam([1500.0, 1500.0, 1500.0, 1500.0, 1500.0]);

        MatchmakingMatch match = MatchmakingMatch.FromTeams(boostingTeam, opponentTeam, MatchmakingTestBuilder.DefaultSettings().LogisticPredictionScale);

        match.GameType = ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL;

        MatchmakingGroupMember booster = boostingDuo.Members.Single(member => member.TMR == 2000.0);
        MatchmakingGroupMember boostee = boostingDuo.Members.Single(member => member.TMR == 1200.0);
        MatchmakingGroupMember solo = boostingTeam.Groups.First(group => group.Members.Count == 1).Members.Single();

        MatchmakingSettings penaltyEnabled = MatchmakingTestBuilder.DefaultSettings();
        MatchmakingSettings penaltyDisabled = MatchmakingTestBuilder.DefaultSettings();

        penaltyDisabled.CoordinationPenaltyEnabled = false;

        match.AssignMatchPointValues(penaltyEnabled);

        (double Win, double Loss) boosterEnabled = (booster.MatchWinValue, booster.MatchLossValue);
        (double Win, double Loss) boosteeEnabled = (boostee.MatchWinValue, boostee.MatchLossValue);
        (double Win, double Loss) soloEnabled = (solo.MatchWinValue, solo.MatchLossValue);

        match.AssignMatchPointValues(penaltyDisabled);

        (double Win, double Loss) boosterDisabled = (booster.MatchWinValue, booster.MatchLossValue);
        (double Win, double Loss) boosteeDisabled = (boostee.MatchWinValue, boostee.MatchLossValue);
        (double Win, double Loss) soloDisabled = (solo.MatchWinValue, solo.MatchLossValue);

        using (Assert.Multiple())
        {
            // The Base Values Are Meaningful, So The Reduction Is Observable
            await Assert.That(boosterDisabled.Win).IsGreaterThan(0.0);
            await Assert.That(boosteeDisabled.Loss).IsLessThan(0.0);

            // Both Members Of The Wide-Spread Group Are Reduced To A Tenth (The Curve's Clamp Floor)
            await Assert.That(Math.Abs(boosterEnabled.Win - boosterDisabled.Win * 0.1) < PointValueTolerance).IsTrue();
            await Assert.That(Math.Abs(boosterEnabled.Loss - boosterDisabled.Loss * 0.1) < PointValueTolerance).IsTrue();
            await Assert.That(Math.Abs(boosteeEnabled.Win - boosteeDisabled.Win * 0.1) < PointValueTolerance).IsTrue();
            await Assert.That(Math.Abs(boosteeEnabled.Loss - boosteeDisabled.Loss * 0.1) < PointValueTolerance).IsTrue();

            // The Solo Players At The Team Average Are Unaffected
            await Assert.That(Math.Abs(soloEnabled.Win - soloDisabled.Win) < PointValueTolerance).IsTrue();
            await Assert.That(Math.Abs(soloEnabled.Loss - soloDisabled.Loss) < PointValueTolerance).IsTrue();
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

    private static MatchmakingTeam BuildSoloTeam(IReadOnlyList<double> memberTMRs)
        => MatchmakingTeam.FromGroups([.. memberTMRs.Select(tmr => MatchmakingTestBuilder.BuildSoloGroup(tmr))], teamSize: 5);

    private static MatchmakingTeam BuildFourPlusOneTeam(int soloMatchCount)
    {
        MatchmakingGroup fourStack = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]);
        MatchmakingGroup solo = MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR, totalMatchCount: soloMatchCount);

        return MatchmakingTeam.FromGroups([fourStack, solo], teamSize: 5);
    }
}
