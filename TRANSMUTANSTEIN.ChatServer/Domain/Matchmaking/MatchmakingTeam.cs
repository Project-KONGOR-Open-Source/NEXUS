namespace TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

/// <summary>
///     Represents a team formed from one or more matchmaking groups.
///     A team participates in a match against another team.
/// </summary>
public class MatchmakingTeam
{
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
    ///     The game mode flags that are compatible with all groups in this team.
    /// </summary>
    public uint GameModeFlags { get; set; }

    /// <summary>
    ///     The region flags that are compatible with all groups in this team.
    /// </summary>
    public uint RegionFlags { get; set; }

    /// <summary>
    ///     Whether this team has been matched with another team.
    /// </summary>
    public bool MatchedUp { get; set; }

    /// <summary>
    ///     Whether this team is virtual (for simulation or testing purposes).
    /// </summary>
    public bool Virtual { get; set; }

    /// <summary>
    ///     Gets all members from all groups in this team.
    /// </summary>
    public IEnumerable<MatchmakingGroupMember> GetAllMembers()
        => Groups.SelectMany(group => group.Members);

    /// <summary>
    ///     Recalculates the team statistics including group makeup.
    /// </summary>
    public void RecalculateStatistics()
    {
        CalculateGroupMakeup();
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
                _           => groupSizes.Sum() // Fallback To Sum Of Group Sizes
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

        // Check That Both Teams Have Overlapping Game Modes (If Flags Are Set)
        if (GameModeFlags != 0 && other.GameModeFlags != 0 && (GameModeFlags & other.GameModeFlags) == 0)
            return false;

        // Check That Both Teams Have Overlapping Regions (If Flags Are Set)
        if (RegionFlags != 0 && other.RegionFlags != 0 && (RegionFlags & other.RegionFlags) == 0)
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
