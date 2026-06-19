namespace TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

/// <summary>
///     Represents a team formed from one or more matchmaking groups.
///     A team participates in a match against another team.
/// </summary>
public class MatchmakingTeam
{
    /// <summary>
    ///     Team rank weighting exponent for power mean calculation.
    ///     Higher values weight the highest-rated players more heavily.
    /// </summary>
    private const double TeamRankWeighting = 6.5;

    /// <summary>
    ///     The unique identifier for this team.
    /// </summary>
    public Guid GUID { get; } = Guid.CreateVersion7();

    /// <summary>
    ///     The groups that make up this team.
    /// </summary>
    public List<MatchmakingGroup> Groups { get; set; } = [];

    /// <summary>
    ///     The total number of players in this team.
    /// </summary>
    public int PlayerCount => Groups.Sum(group => group.Members.Count);

    /// <summary>
    ///     The target team size (number of players per team).
    /// </summary>
    public int TeamSize { get; set; } = 5;

    /// <summary>
    ///     The total TMR (Team Match Rating) of all players in this team.
    /// </summary>
    public double TotalTMR => Groups.Sum(group => group.TotalTMR);

    /// <summary>
    ///     The average TMR of all players in this team.
    /// </summary>
    public double AverageTMR => PlayerCount > 0 ? TotalTMR / PlayerCount : 0;

    /// <summary>
    ///     The power mean team rating using the original generalized mean formula.
    ///     This weights higher-rated players more heavily (exponent 6.5).
    /// </summary>
    /// <remarks>
    ///     Formula: (sum of rating^6.5) ^ (1/6.5)
    /// </remarks>
    public double PowerMeanTMR => CalculatePowerMeanTMR();

    /// <summary>
    ///     The effective team rating for matchmaking, combining:
    ///     <list type="number">
    ///         <item>Power Mean (weights highest-rated players heavily)</item>
    ///         <item>Pre-Made Bonus (adds flat bonus for group size)</item>
    ///     </list>
    /// </summary>
    public double EffectiveTeamRating
    {
        get
        {
            // Start With Power Mean Rating
            double baseRating = PowerMeanTMR;

            // Add Premade Bonus (Distributed Across Team)
            double totalPremadeBonus = Groups.Sum(group => group.GetPremadeBonus());
            double perPlayerBonus = PlayerCount > 0 ? totalPremadeBonus / PlayerCount : 0;

            return baseRating + perPlayerBonus;
        }
    }

    /// <summary>
    ///     Calculates the power mean (generalized mean) of team member ratings.
    /// </summary>
    /// <remarks>
    ///     Original Formula: <c>INT_ROUND(pow(fRating, 1.0f / matchmaker_teamRankWeighting))</c>
    /// </remarks>
    private double CalculatePowerMeanTMR()
    {
        List<MatchmakingGroupMember> members = [.. GetAllMembers()];

        if (members.Count == 0)
            return 0;

        // Sum Of (Rating ^ Exponent)
        double sum = members.Sum(member => Math.Pow(member.TMR, TeamRankWeighting));

        // Take The N-th Root And Round To Nearest Integer (Matches Original INT_ROUND Behaviour)
        return Math.Round(Math.Pow(sum, 1.0 / TeamRankWeighting));
    }

    /// <summary>
    ///     The highest TMR among all players in this team.
    /// </summary>
    public double HighestTMR => GetAllMembers().Any() ? GetAllMembers().Max(member => member.TMR) : 0;

    /// <summary>
    ///     The lowest TMR among all players in this team.
    /// </summary>
    public double LowestTMR => GetAllMembers().Any() ? GetAllMembers().Min(member => member.TMR) : 0;

    /// <summary>
    ///     The group composition score for matchmaking fairness.
    ///     Higher scores indicate more premade groups.
    /// </summary>
    public int GroupMakeup { get; set; }

    /// <summary>
    ///     A string representation of the group composition (e.g. "5", "4+1", "3+2", "3+1+1").
    /// </summary>
    public string GroupMakeupString { get; set; } = string.Empty;

    /// <summary>
    ///     The game modes acceptable to every group in this team.
    /// </summary>
    public string[] CommonGameModes { get; set; } = [];

    /// <summary>
    ///     The regions acceptable to every group in this team, where "NEWERTH" is a wildcard that matches all regions.
    /// </summary>
    public string[] CommonGameRegions { get; set; } = [];

    /// <summary>
    ///     Whether this team queues for ranked matches.
    /// </summary>
    public bool IsRanked { get; set; }

    /// <summary>
    ///     Whether any group in this team prefers match fidelity (fairer matches over shorter queues).
    ///     Only honoured for ranked teams, mirroring the client's host-only, ranked-only fidelity slider.
    /// </summary>
    public bool PrefersMatchFidelity { get; set; }

    /// <summary>
    ///     Whether this team has been matched with another team.
    /// </summary>
    public bool MatchedUp { get; set; }

    /// <summary>
    ///     Gets all members from all groups in this team.
    /// </summary>
    public IEnumerable<MatchmakingGroupMember> GetAllMembers()
        => Groups.SelectMany(group => group.Members);

    /// <summary>
    ///     Recalculates the team statistics, including the group makeup and the shared queue preferences.
    /// </summary>
    public void RecalculateStatistics()
    {
        CalculateGroupMakeup();
        CalculateCommonQueuePreferences();
    }

    /// <summary>
    ///     Calculates the queue preferences shared by every group in this team: the common game modes, the common regions, and the ranked status.
    /// </summary>
    public void CalculateCommonQueuePreferences()
    {
        if (Groups.Count == 0)
        {
            CommonGameModes = [];
            CommonGameRegions = [];
            IsRanked = false;

            return;
        }

        string[] commonGameModes = Groups[0].Information.GameModes;
        string[] commonGameRegions = Groups[0].Information.GameRegions;

        foreach (MatchmakingGroup group in Groups.Skip(1))
        {
            commonGameModes = IntersectGameModes(commonGameModes, group.Information.GameModes);
            commonGameRegions = IntersectGameRegions(commonGameRegions, group.Information.GameRegions);
        }

        CommonGameModes = commonGameModes;
        CommonGameRegions = commonGameRegions;
        IsRanked = Groups[0].Information.Ranked;
        PrefersMatchFidelity = IsRanked && Groups.Any(group => group.Information.MatchFidelity > 0);
    }

    /// <summary>
    ///     Intersects two sets of game modes.
    /// </summary>
    public static string[] IntersectGameModes(string[] left, string[] right)
        => [.. left.Intersect(right, StringComparer.OrdinalIgnoreCase)];

    /// <summary>
    ///     Intersects two sets of regions at the aggregate level (see <see cref="GameRegions"/>), so that for example "USE" and "USW" intersect through their shared "US" aggregate.
    ///     The <see cref="GameRegions.Wildcard"/> region matches all regions: a side containing it defers to the other side's regions, so the wildcard only survives the intersection when both sides carry it.
    /// </summary>
    public static string[] IntersectGameRegions(string[] left, string[] right)
    {
        bool leftHasWildcard = left.Contains(GameRegions.Wildcard, StringComparer.OrdinalIgnoreCase);
        bool rightHasWildcard = right.Contains(GameRegions.Wildcard, StringComparer.OrdinalIgnoreCase);

        string[] leftAggregates = [.. left.Select(GameRegions.GetAggregate).Distinct(StringComparer.OrdinalIgnoreCase)];
        string[] rightAggregates = [.. right.Select(GameRegions.GetAggregate).Distinct(StringComparer.OrdinalIgnoreCase)];

        if (leftHasWildcard)
            return rightAggregates.Length > 0 ? rightAggregates : leftAggregates;

        if (rightHasWildcard)
            return leftAggregates.Length > 0 ? leftAggregates : rightAggregates;

        return [.. leftAggregates.Intersect(rightAggregates, StringComparer.OrdinalIgnoreCase)];
    }

    /// <summary>
    ///     Calculates the group makeup score and string.
    /// </summary>
    public void CalculateGroupMakeup()
    {
        if (Groups.Count == 0)
        {
            GroupMakeup = 0;
            GroupMakeupString = string.Empty;

            return;
        }

        List<int> groupSizes = [.. Groups.Select(group => group.Members.Count).OrderByDescending(size => size)];

        GroupMakeupString = string.Join("+", groupSizes);

        // Calculate Composition Score Based On Team Size
        GroupMakeup = TeamSize switch
        {
            5 => GroupMakeupString switch
            {
                "5"         => 25,
                "4+1"       => 17,
                "3+2"       => 13,
                "3+1+1"     => 11,
                "2+2+1"     => 9,
                "2+1+1+1"   => 7,
                "1+1+1+1+1" => 5,
                _           => groupSizes.Sum() // Fall Back To Sum Of Group Sizes
            },

            3 => GroupMakeupString switch
            {
                "3"     => 9,
                "2+1"   => 5,
                "1+1+1" => 3,
                _       => groupSizes.Sum()
            },

            1 => 1, // 1v1 Always Has Score Of 1

            _ => groupSizes.Sum()
        };
    }

    /// <summary>
    ///     Checks if this team is compatible with another team for matching.
    /// </summary>
    public bool IsCompatibleWith(MatchmakingTeam other)
    {
        // Check That Both Teams Are Not Already Matched
        if (MatchedUp || other.MatchedUp)
            return false;

        // Check That Both Teams Have The Same Size
        if (PlayerCount != other.PlayerCount)
            return false;

        // Check That Both Teams Have The Same Ranked Status
        if (IsRanked != other.IsRanked)
            return false;

        // Check That Both Teams Have At Least One Game Mode In Common
        if (IntersectGameModes(CommonGameModes, other.CommonGameModes).Length == 0)
            return false;

        // Check That Both Teams Have At Least One Region In Common ("NEWERTH" Is A Wildcard That Matches All Regions)
        if (IntersectGameRegions(CommonGameRegions, other.CommonGameRegions).Length == 0)
            return false;

        return true;
    }

    /// <summary>
    ///     Creates a team from a list of groups.
    /// </summary>
    public static MatchmakingTeam FromGroups(IEnumerable<MatchmakingGroup> groups, int teamSize)
    {
        MatchmakingTeam team = new ()
        {
            TeamSize = teamSize,
            Groups = [.. groups]
        };

        team.RecalculateStatistics();

        return team;
    }
}
