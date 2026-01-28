namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

// Properties Common To Both "submit_stats" And "resubmit_stats" Requests
public partial class StatsForSubmissionRequestForm
{
    [FromForm(Name = "f")] public required string Function { get; set; }

    [FromForm(Name = "match_stats")] public required Dictionary<string, string> MatchStats { get; set; }

    [FromForm(Name = "team_stats")] public required Dictionary<int, Dictionary<string, int>> TeamStats { get; set; }

    [FromForm(Name = "player_stats")]
    public required Dictionary<int, Dictionary<string, Dictionary<string, string>>> PlayerStats { get; set; }

    [FromForm(Name = "inventory")] public Dictionary<int, Dictionary<int, string>>? PlayerInventory { get; set; }
}

// Properties Specific To "submit_stats" Requests
public partial class StatsForSubmissionRequestForm
{
    [FromForm(Name = "session")] public string? Session { get; set; }
}

// Properties Specific To "resubmit_stats" Requests
public partial class StatsForSubmissionRequestForm
{
    [FromForm(Name = "login")] public string? HostAccountName { get; set; }

    [FromForm(Name = "pass")] public string? HostAccountPasswordHash { get; set; }

    [FromForm(Name = "resubmission_key")] public string? StatsResubmissionKey { get; set; }

    [FromForm(Name = "server_id")] public int? ServerID { get; set; }
}

// Conditional Properties
public partial class StatsForSubmissionRequestForm
{
    /// <summary>
    ///     Item purchase, sell, and drop history.
    ///     Only submitted if server CVAR "svr_submitMatchStatItems" is enabled.
    /// </summary>
    [FromForm(Name = "items")]
    public List<ItemEvent>? ItemHistory { get; set; }

    /// <summary>
    ///     Ability upgrade timeline for each player.
    ///     Only submitted if server CVAR "svr_submitMatchStatAbilities" is enabled.
    /// </summary>
    [FromForm(Name = "abilities")]
    public Dictionary<int, List<AbilityEvent>>? AbilityHistory { get; set; }

    /// <summary>
    ///     Kill/Death event details with assists.
    ///     Only submitted if server CVAR "svr_submitMatchStatFrags" is enabled.
    /// </summary>
    [FromForm(Name = "frags")]
    public List<FragEvent>? FragHistory { get; set; }
}

// Properties Common To All Game Modes

// Properties Specific To Public Matches
public partial class IndividualPlayerStats
{
    [FromForm(Name = "pub_skill")] public double PublicSkillRatingChange { get; set; }

    [FromForm(Name = "pub_count")] public int PublicMatch { get; set; }
}

// Properties Specific To Ranked (Arranged Matchmaking) Matches
public partial class IndividualPlayerStats
{
    [FromForm(Name = "amm_team_rating")] public double RankedSkillRatingChange { get; set; }

    [FromForm(Name = "amm_team_count")] public int RankedMatch { get; set; }

    [FromForm(Name = "achievement_data")] public string? AchievementData { get; set; }
}
