namespace TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

/// <summary>
///     Represents a match between two teams.
/// </summary>
public class MatchmakingMatch
{
    /// <summary>
    ///     The unique identifier for this match.
    /// </summary>
    public Guid GUID { get; } = Guid.CreateVersion7();

    /// <summary>
    ///     A deterministic integer derived from the GUID, used for correlating CreateMatch and AnnounceMatch messages.
    ///     This is sent to the game server and returned in the response for validation.
    /// </summary>
    public int CorrelationID => GUID.GetDeterministicInt32Hash();

    /// <summary>
    ///     The Legion team (team 1).
    /// </summary>
    public required MatchmakingTeam LegionTeam { get; set; }

    /// <summary>
    ///     The Hellbourne team (team 2).
    ///     <see langword="null"/> for bot matches where the game server fills the opposing team with bots.
    /// </summary>
    public MatchmakingTeam? HellbourneTeam { get; set; }

    /// <summary>
    ///     The predicted win probability for the Legion team (0.0 to 1.0).
    /// </summary>
    public double MatchupPrediction { get; set; }

    /// <summary>
    ///     Whether the teams have mismatched group compositions.
    /// </summary>
    public bool MismatchedGroupMakeup { get; set; }

    /// <summary>
    ///     The method used to combine groups into this match.
    /// </summary>
    public MatchmakingCombineMethod CombineMethod { get; set; }

    /// <summary>
    ///     The selected map for this match.
    /// </summary>
    public string SelectedMap { get; set; } = string.Empty;

    /// <summary>
    ///     The selected game mode for this match.
    /// </summary>
    public string SelectedMode { get; set; } = string.Empty;

    /// <summary>
    ///     The selected region for this match.
    /// </summary>
    public string SelectedRegion { get; set; } = string.Empty;

    /// <summary>
    ///     The regions acceptable to every player in this match, where "NEWERTH" is a wildcard that matches all regions.
    ///     Used to allocate a match server in (or as close as possible to) a requested region.
    /// </summary>
    public string[] CommonGameRegions { get; set; } = [];

    /// <summary>
    ///     The game type for this match.
    /// </summary>
    public ChatProtocol.TMMGameType GameType { get; set; }

    /// <summary>
    ///     Whether this is a ranked match.
    /// </summary>
    public bool IsRanked { get; set; }

    /// <summary>
    ///     Whether this is a bot (co-op) match.
    ///     Bot matches have only one human team; the game server fills the remaining slots with bots.
    /// </summary>
    public bool IsBotMatch { get; set; }

    /// <summary>
    ///     The bot difficulty level (1 = Easy, 2 = Medium, 3 = Hard).
    ///     Only used when <see cref="IsBotMatch"/> is <see langword="true"/>.
    /// </summary>
    public byte BotDifficulty { get; set; }

    /// <summary>
    ///     The arranged match type derived from the bot-match status, the game type, and the ranked status.
    /// </summary>
    public MatchType ArrangedMatchType => IsBotMatch ? MatchType.AM_MATCHMAKING_BOTMATCH : GameType switch
    {
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL          => IsRanked ? MatchType.AM_MATCHMAKING : MatchType.AM_UNRANKED_MATCHMAKING,
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_CASUAL          => IsRanked ? MatchType.AM_MATCHMAKING : MatchType.AM_UNRANKED_MATCHMAKING,

        // Reborn variants (including Caldavar Reborn) are intentionally grouped under MIDWARS.
        // The original match server uses this match type to route all Reborn and MidWars matches through a shared "alternative queue" code path for stat submission and leaver handling.
        // Changing these mappings would cause mismatched behaviour of the original match server binary.

        ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS         => MatchType.AM_MATCHMAKING_MIDWARS,
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_REBORN_NORMAL   => MatchType.AM_MATCHMAKING_MIDWARS,
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_REBORN_CASUAL   => MatchType.AM_MATCHMAKING_MIDWARS,
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS_REBORN  => MatchType.AM_MATCHMAKING_MIDWARS,

        ChatProtocol.TMMGameType.TMM_GAME_TYPE_RIFTWARS        => MatchType.AM_MATCHMAKING_RIFTWARS,
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL => MatchType.AM_MATCHMAKING_CAMPAIGN,
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_CASUAL => MatchType.AM_MATCHMAKING_CAMPAIGN,
        ChatProtocol.TMMGameType.TMM_GAME_TYPE_CUSTOM          => MatchType.AM_MATCHMAKING_CUSTOM,

        _                                                      => throw new ArgumentOutOfRangeException(nameof(GameType), $@"Unsupported Game Type ""{GameType}""")
    };

    /// <summary>
    ///     The ID of the assigned game server, if any.
    /// </summary>
    public int? AssignedServerID { get; set; }

    /// <summary>
    ///     The address of the assigned game server, if any.
    /// </summary>
    public string? ServerAddress { get; set; }

    /// <summary>
    ///     The port of the assigned game server, if any.
    /// </summary>
    public ushort? ServerPort { get; set; }

    /// <summary>
    ///     When this match was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     The current state of this match.
    /// </summary>
    public MatchmakingMatchState State { get; set; } = MatchmakingMatchState.Created;

    /// <summary>
    ///     The team size for this match, validated to be consistent across all teams.
    /// </summary>
    public int TeamSize => GetAllTeams().Select(team => team.TeamSize).Distinct().Single();

    /// <summary>
    ///     Gets all teams in this match.
    /// </summary>
    public IEnumerable<MatchmakingTeam> GetAllTeams()
        => HellbourneTeam is not null ? [LegionTeam, HellbourneTeam] : [LegionTeam];

    /// <summary>
    ///     Gets all human players in this match from both teams.
    /// </summary>
    public IEnumerable<MatchmakingGroupMember> GetAllPlayers()
        => GetAllTeams().SelectMany(team => team.GetAllMembers());

    /// <summary>
    ///     Gets all groups in this match from both teams.
    /// </summary>
    public IEnumerable<MatchmakingGroup> GetAllGroups()
        => GetAllTeams().SelectMany(team => team.Groups);

    /// <summary>
    ///     Calculates the matchup prediction using a logistic function with the natural exponential base.
    ///     Returns the probability that Legion (team 1) wins.
    /// </summary>
    /// <remarks>
    ///     Original Formula: <c>1.0f / (1.0f + pow(M_E, -(fTeamRating - fOtherTeamRating) / matchmaker_logisticPredictionScale))</c>
    /// </remarks>
    public static double CalculateMatchupPrediction(double legionTMR, double hellbourneTMR, double scale = 80.0)
        => 1.0 / (1.0 + Math.Exp(-(legionTMR - hellbourneTMR) / scale));

    /// <summary>
    ///     The width of the TMR band over which the high-rating K-factor reduction ramps from zero to the full <see cref="MatchmakingSettings.ReducedKFactorMultiplier"/>.
    /// </summary>
    private const double ReducedKFactorRampRange = 300.0;

    /// <summary>
    ///     Pre-calculates each player's rating point values for winning and for losing this match, and flags provisional players.
    ///     The match server adjusts these values for in-game events (for example leavers, terminations, and rating-exempt modes) and submits the final value with the match statistics.
    /// </summary>
    public void AssignMatchPointValues(MatchmakingSettings settings)
    {
        // Bot Matches Have No Opposing Team And Do Not Affect Ratings
        if (HellbourneTeam is null)
            return;

        AssignTeamMatchPointValues(LegionTeam, HellbourneTeam, MatchupPrediction, settings);
        AssignTeamMatchPointValues(HellbourneTeam, LegionTeam, 1.0 - MatchupPrediction, settings);
    }

    private void AssignTeamMatchPointValues(MatchmakingTeam team, MatchmakingTeam opposingTeam, double winPrediction, MatchmakingSettings settings)
    {
        double lossMultiplier = GetSmallGroupLossMultiplier(team, opposingTeam, settings);
        double teamAverageTMR = team.AverageTMR;

        foreach (MatchmakingGroup group in team.Groups)
        {
            // The Coordination Penalty Only Engages For A Pre-Made Group With A Wide Internal Rating Spread (The Boosting Signature)
            bool groupIncursCoordinationPenalty = settings.CoordinationPenaltyEnabled && group.Members.Count > 1 && group.TMRRange > GammaCurveRange;

            foreach (MatchmakingGroupMember member in group.Members)
            {
                member.IsProvisional = IsProvisionalPlayer(member, settings);

                double kFactor = CalculateKFactor(member, settings);

                // The Penalty Scales With How Far The Member's Rating Sits From Their Team's Average, Damping Both Gains And Losses Down To A Tenth
                double coordinationMultiplier = groupIncursCoordinationPenalty ? CalculateSkillDifferenceAdjustment(member.TMR, teamAverageTMR) : 1.0;

                member.MatchWinValue = Math.Clamp((1.0 - winPrediction) * kFactor * coordinationMultiplier, 0.0, settings.MaximumKFactor);
                member.MatchLossValue = CalculateMatchLossValue(member, winPrediction, kFactor, coordinationMultiplier, lossMultiplier, settings);
            }
        }
    }

    /// <summary>
    ///     The highest group makeup score (see <see cref="MatchmakingTeam.GroupMakeup"/>) still considered a team of small groups, equal to the "2+2+1" composition.
    /// </summary>
    private const int SmallGroupMakeupCeiling = 9;

    /// <summary>
    ///     The lowest group makeup score (see <see cref="MatchmakingTeam.GroupMakeup"/>) considered a large pre-made stack, equal to the "4+1" composition.
    /// </summary>
    private const int LargeStackMakeupFloor = 17;

    /// <summary>
    ///     Gets the rating-loss multiplier for a team, halving the loss when a team of only small groups (the "2+2+1" composition or smaller) faces a large pre-made stack (the "4+1" composition or a full team), to compensate for the opponent's coordination advantage.
    ///     Only applies to full five-player teams, matching the original composition constants, and only when <see cref="MatchmakingSettings.ReducedLossForSmallGroupsEnabled"/> is set.
    /// </summary>
    private static double GetSmallGroupLossMultiplier(MatchmakingTeam team, MatchmakingTeam opposingTeam, MatchmakingSettings settings)
    {
        if (settings.ReducedLossForSmallGroupsEnabled is false || team.TeamSize is not 5)
            return 1.0;

        bool teamIsSmallGroups = team.GroupMakeup <= SmallGroupMakeupCeiling;
        bool opponentIsLargeStack = opposingTeam.GroupMakeup >= LargeStackMakeupFloor;

        return teamIsSmallGroups && opponentIsLargeStack ? 0.5 : 1.0;
    }

    /// <summary>
    ///     A player is provisional while their rating for the queued game type is still converging: fewer than <see cref="MatchmakingSettings.ProvisionalMatchCount"/> matches played and a rating below <see cref="MatchmakingSettings.ProvisionalTMRCutoff"/>.
    ///     MidWars and RiftWars ratings have no provisional phase, matching the original implementation.
    ///     The provisional phase is distinct from placement matches (<see cref="AccountStatistics.IsInPlacementPhase"/>), which are counted separately by the master server and only gate the visible medal.
    /// </summary>
    private bool IsProvisionalPlayer(MatchmakingGroupMember member, MatchmakingSettings settings)
    {
        bool gameTypeHasNoProvisionalPhase = GameType
            is ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS
            or ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS_REBORN
            or ChatProtocol.TMMGameType.TMM_GAME_TYPE_RIFTWARS;

        if (gameTypeHasNoProvisionalPhase)
            return false;

        return member.TMR < settings.ProvisionalTMRCutoff && member.GameTypeMatchCount < settings.ProvisionalMatchCount;
    }

    /// <summary>
    ///     Calculates the player's K-factor: the base value, multiplied by <see cref="MatchmakingSettings.ProvisionalKFactorMultiplier"/> for provisional players, and reduced for highly-rated players to counter rating inflation.
    ///     The high-rating reduction ramps linearly from zero at <see cref="MatchmakingSettings.ReducedKFactorTMRCutoff"/> to the full <see cref="MatchmakingSettings.ReducedKFactorMultiplier"/> over <see cref="ReducedKFactorRampRange"/> TMR.
    /// </summary>
    private static double CalculateKFactor(MatchmakingGroupMember member, MatchmakingSettings settings)
    {
        if (member.IsProvisional)
            return settings.BaseKFactor * settings.ProvisionalKFactorMultiplier;

        if (member.TMR > settings.ReducedKFactorTMRCutoff)
        {
            double reduction = Math.Clamp((member.TMR - settings.ReducedKFactorTMRCutoff) / ReducedKFactorRampRange, 0.0, 1.0);

            return settings.BaseKFactor - settings.BaseKFactor * reduction * settings.ReducedKFactorMultiplier;
        }

        return settings.BaseKFactor;
    }

    /// <summary>
    ///     Calculates the rating point value for losing this match, which is always zero or negative.
    ///     A player already at the minimum TMR loses nothing, and a loss never takes a player below the minimum TMR.
    /// </summary>
    private static double CalculateMatchLossValue(MatchmakingGroupMember member, double winPrediction, double kFactor, double coordinationMultiplier, double lossMultiplier, MatchmakingSettings settings)
    {
        if (member.TMR < settings.MinimumTMR + 0.01)
            return 0.0;

        // Clamp To The Same Bound As The Returned Value So The Below-Minimum Check Tests The Loss That Is Actually Applied
        // A Static -BaseKFactor Bound Here Would Underestimate A Provisional Player's Loss (Whose kFactor Can Reach MaximumKFactor) And Let It Drop Below MinimumTMR
        double lossValue = Math.Clamp(-winPrediction * kFactor * coordinationMultiplier, -settings.MaximumKFactor, 0.0);

        // The Below-Minimum Floor Is Tested Against The Coordination-Adjusted Loss, Then The Small-Group Reduction Is Applied To The Final Value (Matching The Original Order)
        if (member.TMR + lossValue < settings.MinimumTMR)
            return Math.Clamp(settings.MinimumTMR - member.TMR, -settings.MaximumKFactor, 0.0);

        return lossValue * lossMultiplier;
    }

    /// <summary>
    ///     The rating spread (around a team's average) over which the coordination penalty ramps in, and the shape and scale of the gamma distribution that defines the ramp.
    ///     A member exactly at their team's average is unpenalised; one a full <see cref="GammaCurveRange"/> above or below has their gains and losses reduced to a tenth.
    /// </summary>
    private const double GammaCurveRange = 175.0;
    private const int GammaCurveShape = 18;
    private const double GammaCurveScale = 5.0;

    /// <summary>
    ///     Calculates the coordination-penalty multiplier (between 0.1 and 1.0) for a member, based on how far their rating sits from their team's average.
    ///     The further the member is from the average in either direction, the smaller the multiplier, so that a high-rated player boosting far-lower-rated friends (and the boosted friends themselves) gain and lose very little rating.
    /// </summary>
    private static double CalculateSkillDifferenceAdjustment(double playerTMR, double teamAverageTMR)
    {
        double skillDifference = Math.Max(GammaCurveRange - Math.Abs(playerTMR - teamAverageTMR), 0.1);

        return Math.Clamp(GammaDistribution(skillDifference, GammaCurveShape, GammaCurveScale), 0.1, 1.0);
    }

    /// <summary>
    ///     Evaluates the cumulative distribution function of a gamma (Erlang) distribution with the given integer shape and scale at the supplied value.
    /// </summary>
    private static double GammaDistribution(double value, int shape, double scale)
    {
        double scaledValue = value / scale;

        double cumulative = 0.0;
        long factorial = 1;

        for (int term = 0; term < shape; term++)
        {
            cumulative += Math.Exp(-scaledValue) * (Math.Pow(scaledValue, term) / factorial);
            factorial *= term + 1;
        }

        return 1.0 - cumulative;
    }

    /// <summary>
    ///     Creates a bot (co-op) match from a single group.
    ///     The group is placed on the Legion team; the game server fills remaining slots with bots.
    ///     In the original implementation, bot matches force the game type to <see cref="ChatProtocol.TMMGameType.TMM_GAME_TYPE_CASUAL"/> and the map to "caldavar".
    /// </summary>
    public static MatchmakingMatch FromBotGroup(MatchmakingGroup group)
    {
        MatchmakingTeam legionTeam = new () { Groups = [group], TeamSize = group.Information.TeamSize };

        legionTeam.RecalculateStatistics();

        MatchmakingMatch match = new ()
        {
            LegionTeam = legionTeam,
            HellbourneTeam = null,
            IsBotMatch = true,
            BotDifficulty = group.Information.BotDifficulty,
            SelectedMap = "caldavar",
            SelectedMode = "botmatch",
            SelectedRegion = group.Information.GameRegions.Length > 0 ? group.Information.GameRegions.RandomElement() : "NEWERTH",
            CommonGameRegions = group.Information.GameRegions,
            GameType = ChatProtocol.TMMGameType.TMM_GAME_TYPE_CASUAL,
            IsRanked = false
        };

        group.MatchedUp = true;
        group.AssignedMatchGUID = match.GUID;
        group.AssignedTeamGUID = legionTeam.GUID;

        return match;
    }

    /// <summary>
    ///     Creates a match from two teams and calculates the matchup prediction.
    /// </summary>
    /// <remarks>
    ///     The <paramref name="groupMakeupTolerance"/> argument should match the pool-size-aware tolerance the broker used to permit this pairing, so that <see cref="MismatchedGroupMakeup"/> only fires when the makeup difference actually exceeded the broker's allowance for that pool tier.
    ///     The default of 2, which is the strictest value, matches the Macro/Large tolerance and is preserved for direct callers without a pool context.
    /// </remarks>
    public static MatchmakingMatch FromTeams(MatchmakingTeam legionTeam, MatchmakingTeam hellbourneTeam, double logisticPredictionScale = 80.0, int groupMakeupTolerance = 2)
    {
        MatchmakingMatch match = new ()
        {
            LegionTeam = legionTeam,
            HellbourneTeam = hellbourneTeam
        };

        // Calculate Matchup Prediction Using Effective Team Rating (Power Mean + Premade Bonus)
        match.MatchupPrediction = CalculateMatchupPrediction(legionTeam.EffectiveTeamRating, hellbourneTeam.EffectiveTeamRating, logisticPredictionScale);

        // Check For Mismatched Group Makeup Using The Pool-Size-Aware Tolerance That Permitted This Pairing
        match.MismatchedGroupMakeup = Math.Abs(legionTeam.GroupMakeup - hellbourneTeam.GroupMakeup) > groupMakeupTolerance;

        // Mark Teams As Matched
        legionTeam.MatchedUp = true;
        hellbourneTeam.MatchedUp = true;

        // Mark All Groups As Matched
        foreach (MatchmakingGroup group in match.GetAllGroups())
        {
            group.MatchedUp = true;
            group.AssignedMatchGUID = match.GUID;
        }

        // Assign Team GUIDs To Groups
        foreach (MatchmakingGroup group in legionTeam.Groups)
            group.AssignedTeamGUID = legionTeam.GUID;

        foreach (MatchmakingGroup group in hellbourneTeam.Groups)
            group.AssignedTeamGUID = hellbourneTeam.GUID;

        return match;
    }
}

/// <summary>
///     The state of a matchmaking match.
/// </summary>
public enum MatchmakingMatchState
{
    /// <summary>
    ///     The match has been created but not yet allocated to a server.
    /// </summary>
    Created,

    /// <summary>
    ///     The match is being allocated to a server.
    /// </summary>
    ServerAllocating,

    /// <summary>
    ///     The match has been allocated to a server.
    /// </summary>
    ServerAllocated,

    /// <summary>
    ///     The match is waiting for players to connect.
    /// </summary>
    WaitingForPlayers,

    /// <summary>
    ///     The match has started.
    /// </summary>
    Started,

    /// <summary>
    ///     The match was abandoned (not enough players connected).
    /// </summary>
    Abandoned,

    /// <summary>
    ///     The match has completed.
    /// </summary>
    Completed
}

/// <summary>
///     The method used to combine groups into teams.
/// </summary>
public enum MatchmakingCombineMethod
{
    /// <summary>
    ///     Simple first-in-first-out matching.
    /// </summary>
    FirstInFirstOut = 0,

    // Team Size 5 Methods

    /// <summary>
    ///     Full teams or two-group combinations, sorted by queue time.
    /// </summary>
    FullOrTwoGroupsTimeQueued = 1,

    /// <summary>
    ///     Full teams or two-group combinations, randomised.
    /// </summary>
    FullOrTwoGroupsRandom = 2,

    /// <summary>
    ///     Only match full 5-stacks.
    /// </summary>
    FiveRandom = 3,

    /// <summary>
    ///     Only 4+1 combinations.
    /// </summary>
    FourPlusOneRandom = 4,

    /// <summary>
    ///     Only 3+2 combinations.
    /// </summary>
    ThreePlusTwoRandom = 5,

    /// <summary>
    ///     Only solo queue players.
    /// </summary>
    AllOnesRandom = 6,

    /// <summary>
    ///     Any combination of group sizes.
    /// </summary>
    AllGroupSizesRandom = 7,

    /// <summary>
    ///     Experimental matching (currently disabled in legacy).
    /// </summary>
    AllExperimental = 8,

    // Team Size 3 Methods

    /// <summary>
    ///     3v3 full or two-group combinations, sorted by queue time.
    /// </summary>
    ThreeFullOrTwoGroupsTimeQueued = 9,

    /// <summary>
    ///     3v3 full or two-group combinations, randomised.
    /// </summary>
    ThreeFullOrTwoGroupsRandom = 10,

    /// <summary>
    ///     3v3 solo queue only.
    /// </summary>
    ThreeAllOnesRandom = 11,

    /// <summary>
    ///     3v3 any combination.
    /// </summary>
    ThreeAllGroupSizesRandom = 12,

    // Team Size 1 Methods

    /// <summary>
    ///     1v1 matching.
    /// </summary>
    OneVsOneRandom = 13,

    // Special Methods

    /// <summary>
    ///     Brute force matching for long-waiting groups.
    /// </summary>
    BruteForce = 14
}
