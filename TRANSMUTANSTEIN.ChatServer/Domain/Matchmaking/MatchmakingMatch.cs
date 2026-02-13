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
    ///     A sequential match index assigned by the broker for this cycle.
    /// </summary>
    public int MatchIndex { get; set; }

    /// <summary>
    ///     The Legion team (team 1).
    /// </summary>
    public required MatchmakingTeam LegionTeam { get; set; }

    /// <summary>
    ///     The Hellbourne team (team 2).
    /// </summary>
    public required MatchmakingTeam HellbourneTeam { get; set; }

    /// <summary>
    ///     The predicted win probability for the Legion team (0.0 to 1.0).
    /// </summary>
    public double MatchupPrediction { get; set; }

    /// <summary>
    ///     The win percentage threshold that was acceptable for this match.
    /// </summary>
    public double WinPercentThreshold { get; set; }

    /// <summary>
    ///     The loss percentage threshold that was acceptable for this match.
    /// </summary>
    public double LossPercentThreshold { get; set; }

    /// <summary>
    ///     Whether the teams have mismatched group compositions.
    /// </summary>
    public bool MismatchedGroupMakeup { get; set; }

    /// <summary>
    ///     Whether the match was balanced in the first pass.
    /// </summary>
    public bool FirstPassBalanced { get; set; }

    /// <summary>
    ///     Whether the match was balanced in the second pass.
    /// </summary>
    public bool SecondPassBalanced { get; set; }

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
    ///     The game type for this match.
    /// </summary>
    public ChatProtocol.TMMGameType GameType { get; set; }

    /// <summary>
    ///     Whether this is a ranked match.
    /// </summary>
    public bool IsRanked { get; set; }

    /// <summary>
    ///     Pre-calculated rating changes per player (AccountID -> (WinValue, LossValue)).
    /// </summary>
    public Dictionary<int, (double WinValue, double LossValue)> MatchPointValues { get; set; } = [];

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
    ///     Gets all players in this match from both teams.
    /// </summary>
    public IEnumerable<MatchmakingGroupMember> GetAllPlayers()
        => LegionTeam.GetAllMembers().Concat(HellbourneTeam.GetAllMembers());

    /// <summary>
    ///     Gets all groups in this match from both teams.
    /// </summary>
    public IEnumerable<MatchmakingGroup> GetAllGroups()
        => LegionTeam.Groups.Concat(HellbourneTeam.Groups);

    /// <summary>
    ///     Calculates the matchup prediction using ELO-based formula.
    ///     Returns the probability that Legion (team 1) wins.
    /// </summary>
    public static double CalculateMatchupPrediction(double legionTMR, double hellbourneTMR, double scale = 225.0)
        => 1.0 / (1.0 + Math.Pow(10.0, -(legionTMR - hellbourneTMR) / scale));

    /// <summary>
    ///     Creates a match from two teams and calculates the matchup prediction.
    /// </summary>
    public static MatchmakingMatch FromTeams(MatchmakingTeam legionTeam, MatchmakingTeam hellbourneTeam, int matchIndex = 0)
    {
        MatchmakingMatch match = new ()
        {
            LegionTeam = legionTeam,
            HellbourneTeam = hellbourneTeam,
            MatchIndex = matchIndex
        };

        // Calculate Matchup Prediction
        match.MatchupPrediction = CalculateMatchupPrediction(legionTeam.AverageTMR, hellbourneTeam.AverageTMR);

        // Check For Mismatched Group Makeup
        match.MismatchedGroupMakeup = Math.Abs(legionTeam.GroupMakeup - hellbourneTeam.GroupMakeup) > 2;

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
///     Values match ETMMCombineMethod from legacy HON Chat Server.
/// </summary>
public enum MatchmakingCombineMethod
{
    /// <summary>
    ///     Simple first-in-first-out matching (NEXUS extension, not in legacy).
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

    // Team Size 3 Methods (Grimm's Crossing)

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

    // Team Size 1 Methods (Solo Map)

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
