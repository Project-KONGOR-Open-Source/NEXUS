namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class StatsForSubmissionRequestForm
{
    [FromForm(Name = "match_stats")]
    public required MatchStats MatchStats { get; set; }

    [FromForm(Name = "team_stats")]
    public required Dictionary<int, Dictionary<string, int>> TeamStats { get; set; }

    [FromForm(Name = "player_stats")]
    public required Dictionary<int, Dictionary<string, IndividualPlayerStats>> PlayerStats { get; set; }

    [FromForm(Name = "inventory")]
    public required Dictionary<int, Dictionary<int, string>> PlayerInventory { get; set; }
}

public class MatchStats
{
    [FromForm(Name = "server_id")]
    public required long ServerID { get; set; }

    [FromForm(Name = "match_id")]
    public required int MatchID { get; set; }

    [FromForm(Name = "map")]
    public required string Map { get; set; }

    [FromForm(Name = "map_version")]
    public required string MapVersion { get; set; }

    [FromForm(Name = "time_played")]
    public required int TimePlayed { get; set; }

    [FromForm(Name = "file_size")]
    public required int FileSize { get; set; }

    [FromForm(Name = "file_name")]
    public required string FileName { get; set; }

    [FromForm(Name = "c_state")]
    public required int ConnectionState { get; set; }

    [FromForm(Name = "version")]
    public required string Version { get; set; }

    [FromForm(Name = "avgpsr")]
    public required int AveragePSR { get; set; }

    [FromForm(Name = "avgpsr_team1")]
    public required int AveragePSRTeamOne { get; set; }

    [FromForm(Name = "avgpsr_team2")]
    public required int AveragePSRTeamTwo { get; set; }

    [FromForm(Name = "gamemode")]
    public required string GameMode { get; set; }

    [FromForm(Name = "teamscoregoal")]
    public required int TeamScoreGoal { get; set; }

    [FromForm(Name = "playerscoregoal")]
    public required int PlayerScoreGoal { get; set; }

    [FromForm(Name = "numrounds")]
    public required int NumberOfRounds { get; set; }

    [FromForm(Name = "release_stage")]
    public required string ReleaseStage { get; set; }

    [FromForm(Name = "banned_heroes")]
    public required string BannedHeroes { get; set; }

    [FromForm(Name = "awd_mann")]
    public required int AwardMostAnnihilations { get; set; }

    [FromForm(Name = "awd_mqk")]
    public required int AwardMostQuadKills { get; set; }

    [FromForm(Name = "awd_lgks")]
    public required int AwardLargestKillStreak { get; set; }

    [FromForm(Name = "awd_msd")]
    public required int AwardMostSmackdowns { get; set; }

    [FromForm(Name = "awd_mkill")]
    public required int AwardMostKills { get; set; }

    [FromForm(Name = "awd_masst")]
    public required int AwardMostAssists { get; set; }

    [FromForm(Name = "awd_ledth")]
    public required int AwardLeastDeaths { get; set; }

    [FromForm(Name = "awd_mbdmg")]
    public required int AwardMostBuildingDamage { get; set; }

    [FromForm(Name = "awd_mwk")]
    public required int AwardMostWardsKilled { get; set; }

    [FromForm(Name = "awd_mhdd")]
    public required int AwardMostHeroDamageDealt { get; set; }

    [FromForm(Name = "awd_hcs")]
    public required int AwardHighestCreepScore { get; set; }

    [FromForm(Name = "submission_debug")]
    public required string SubmissionDebug { get; set; }
}

public class IndividualPlayerStats
{
    public string? nickname { get; set; }
    public string? clan_tag { get; set; }
    public int clan_id { get; set; }
    public int team { get; set; }
    public int position { get; set; }
    public int group_num { get; set; }
    public int benefit { get; set; }
    public long hero_id { get; set; }
    public int wins { get; set; }
    public int losses { get; set; }
    public int discos { get; set; }
    public int concedes { get; set; }
    public int kicked { get; set; }
    public int social_bonus { get; set; }
    public int used_token { get; set; }
    public double pub_skill { get; set; }
    public int pub_count { get; set; }
    public double amm_team_rating { get; set; }
    public int amm_team_count { get; set; }
    public int concedevotes { get; set; }
    public int herokills { get; set; }
    public int herodmg { get; set; }
    public int herokillsgold { get; set; }
    public int heroassists { get; set; }
    public int heroexp { get; set; }
    public int deaths { get; set; }
    public int buybacks { get; set; }
    public int goldlost2death { get; set; }
    public int secs_dead { get; set; }
    public int teamcreepkills { get; set; }
    public int teamcreepdmg { get; set; }
    public int teamcreepgold { get; set; }
    public int teamcreepexp { get; set; }
    public int neutralcreepkills { get; set; }
    public int neutralcreepdmg { get; set; }
    public int neutralcreepgold { get; set; }
    public int neutralcreepexp { get; set; }
    public int bdmg { get; set; }
    public int razed { get; set; }
    public int bdmgexp { get; set; }
    public int bgold { get; set; }
    public int denies { get; set; }
    public int exp_denied { get; set; }
    public int gold { get; set; }
    public int gold_spent { get; set; }
    public int exp { get; set; }
    public int actions { get; set; }
    public int secs { get; set; }
    public int level { get; set; }
    public int consumables { get; set; }
    public int wards { get; set; }
    public int bloodlust { get; set; }
    public int doublekill { get; set; }
    public int triplekill { get; set; }
    public int quadkill { get; set; }
    public int annihilation { get; set; }
    public int ks3 { get; set; }
    public int ks4 { get; set; }
    public int ks5 { get; set; }
    public int ks6 { get; set; }
    public int ks7 { get; set; }
    public int ks8 { get; set; }
    public int ks9 { get; set; }
    public int ks10 { get; set; }
    public int ks15 { get; set; }
    public int smackdown { get; set; }
    public int humiliation { get; set; }
    public int nemesis { get; set; }
    public int retribution { get; set; }
    public int score { get; set; }
    public double gameplaystat0 { get; set; }
    public double gameplaystat1 { get; set; }
    public double gameplaystat2 { get; set; }
    public double gameplaystat3 { get; set; }
    public double gameplaystat4 { get; set; }
    public double gameplaystat5 { get; set; }
    public double gameplaystat6 { get; set; }
    public double gameplaystat7 { get; set; }
    public double gameplaystat8 { get; set; }
    public double gameplaystat9 { get; set; }
    public int time_earning_exp { get; set; }

    // Extra fields that we populate manually.
    public int match_id { get; set; }
    public int? account_id { get; set; }
    public string? map { get; set; }
    public string? cli_name { get; set; }
    public string? mdt { get; set; } // DEPRECATED -> TODO: convert this to a [NotMapped] string generated from "datetime"
    public DateTime datetime { get; set; }
}
