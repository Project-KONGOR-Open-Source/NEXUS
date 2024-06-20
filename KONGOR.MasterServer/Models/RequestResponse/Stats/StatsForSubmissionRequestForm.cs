namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

// TODO: Inspect Stats Data For Matchmaking

/*
    AmmSoloRating($player_match_stats['amm_solo_rating'])
    AmmSoloCount($player_match_stats['amm_solo_count'])
    AmmTeamRating($player_match_stats['amm_team_rating'])
    AmmTeamCount($player_match_stats['amm_team_count'])
 */

// Properties Common To Both "submit_stats" And "resubmit_stats" Requests
public partial class StatsForSubmissionRequestForm
{
    [FromForm(Name = "f")]
    public required string Function { get; set; }

    [FromForm(Name = "match_stats")]
    public required MatchStats MatchStats { get; set; }

    [FromForm(Name = "team_stats")]
    public required Dictionary<int, Dictionary<string, int>> TeamStats { get; set; }

    [FromForm(Name = "player_stats")]
    public required Dictionary<int, Dictionary<string, IndividualPlayerStats>> PlayerStats { get; set; }

    [FromForm(Name = "inventory")]
    public required Dictionary<int, Dictionary<int, string>> PlayerInventory { get; set; }
}

// Properties Specific To "submit_stats" Requests
public partial class StatsForSubmissionRequestForm
{
    [FromForm(Name = "session")]
    public string? Session { get; set; }
}

// Properties Specific To "resubmit_stats" Requests
public partial class StatsForSubmissionRequestForm
{
    [FromForm(Name = "login")]
    public string? HostAccountName { get; set; }

    [FromForm(Name = "pass")]
    public string? HostAccountPasswordHash { get; set; }

    [FromForm(Name = "resubmission_key")]
    public string? StatsResubmissionKey { get; set; }

    [FromForm(Name = "server_id")]
    public long? ServerID { get; set; }
}

public class MatchStats
{
    [FromForm(Name = "server_id")]
    public long ServerID { get; set; }

    [FromForm(Name = "match_id")]
    public int MatchID { get; set; }

    [FromForm(Name = "map")]
    public string? Map { get; set; }

    [FromForm(Name = "map_version")]
    public string? MapVersion { get; set; }

    [FromForm(Name = "time_played")]
    public int TimePlayed { get; set; }

    [FromForm(Name = "file_size")]
    public int FileSize { get; set; }

    [FromForm(Name = "file_name")]
    public string? FileName { get; set; }

    [FromForm(Name = "c_state")]
    public int ConnectionState { get; set; }

    [FromForm(Name = "version")]
    public string? Version { get; set; }

    [FromForm(Name = "avgpsr")]
    public int AveragePSR { get; set; }

    [FromForm(Name = "avgpsr_team1")]
    public int AveragePSRTeamOne { get; set; }

    [FromForm(Name = "avgpsr_team2")]
    public int AveragePSRTeamTwo { get; set; }

    [FromForm(Name = "gamemode")]
    public string? GameMode { get; set; }

    [FromForm(Name = "teamscoregoal")]
    public int TeamScoreGoal { get; set; }

    [FromForm(Name = "playerscoregoal")]
    public int PlayerScoreGoal { get; set; }

    [FromForm(Name = "numrounds")]
    public int NumberOfRounds { get; set; }

    [FromForm(Name = "release_stage")]
    public string? ReleaseStage { get; set; }

    [FromForm(Name = "banned_heroes")]
    public string? BannedHeroes { get; set; }

    [FromForm(Name = "awd_mann")]
    public int AwardMostAnnihilations { get; set; }

    [FromForm(Name = "awd_mqk")]
    public int AwardMostQuadKills { get; set; }

    [FromForm(Name = "awd_lgks")]
    public int AwardLargestKillStreak { get; set; }

    [FromForm(Name = "awd_msd")]
    public int AwardMostSmackdowns { get; set; }

    [FromForm(Name = "awd_mkill")]
    public int AwardMostKills { get; set; }

    [FromForm(Name = "awd_masst")]
    public int AwardMostAssists { get; set; }

    [FromForm(Name = "awd_ledth")]
    public int AwardLeastDeaths { get; set; }

    [FromForm(Name = "awd_mbdmg")]
    public int AwardMostBuildingDamage { get; set; }

    [FromForm(Name = "awd_mwk")]
    public int AwardMostWardsKilled { get; set; }

    [FromForm(Name = "awd_mhdd")]
    public int AwardMostHeroDamageDealt { get; set; }

    [FromForm(Name = "awd_hcs")]
    public int AwardHighestCreepScore { get; set; }

    [FromForm(Name = "submission_debug")]
    public string? SubmissionDebug { get; set; }
}

public class IndividualPlayerStats
{
    [FromForm(Name = "nickname")]
    public string? AccountName { get; set; }

    [FromForm(Name = "clan_tag")]
    public string? ClanTag { get; set; }

    [FromForm(Name = "clan_id")]
    public int ClanID { get; set; }

    [FromForm(Name = "team")]
    public int Team { get; set; }

    [FromForm(Name = "position")]
    public int LobbyPosition { get; set; }

    [FromForm(Name = "group_num")]
    public int GroupNumber { get; set; }

    [FromForm(Name = "benefit")]
    public int Benefit { get; set; }

    [FromForm(Name = "hero_id")]
    public long HeroID { get; set; }

    [FromForm(Name = "wins")]
    public int Win { get; set; }

    [FromForm(Name = "losses")]
    public int Loss { get; set; }

    [FromForm(Name = "discos")]
    public int Disconnected { get; set; }

    [FromForm(Name = "concedes")]
    public int Conceded { get; set; }

    [FromForm(Name = "kicked")]
    public int Kicked { get; set; }

    [FromForm(Name = "social_bonus")]
    public int SocialBonus { get; set; }

    [FromForm(Name = "used_token")]
    public int UsedToken { get; set; }

    [FromForm(Name = "pub_skill")]
    public double PublicSkillRatingChange { get; set; }

    [FromForm(Name = "pub_count")]
    public int PublicMatch { get; set; }

    [FromForm(Name = "concedevotes")]
    public int ConcedeVotes { get; set; }

    [FromForm(Name = "herokills")]
    public int HeroKills { get; set; }

    [FromForm(Name = "herodmg")]
    public int HeroDamage { get; set; }

    [FromForm(Name = "herokillsgold")]
    public int GoldFromHeroKills { get; set; }

    [FromForm(Name = "heroassists")]
    public int HeroAssists { get; set; }

    [FromForm(Name = "heroexp")]
    public int HeroExperience { get; set; }

    [FromForm(Name = "deaths")]
    public int HeroDeaths { get; set; }

    [FromForm(Name = "buybacks")]
    public int Buybacks { get; set; }

    [FromForm(Name = "goldlost2death")]
    public int GoldLostToDeath { get; set; }

    [FromForm(Name = "secs_dead")]
    public int SecondsDead { get; set; }

    [FromForm(Name = "teamcreepkills")]
    public int TeamCreepKills { get; set; }

    [FromForm(Name = "teamcreepdmg")]
    public int TeamCreepDamage { get; set; }

    [FromForm(Name = "teamcreepgold")]
    public int TeamCreepGold { get; set; }

    [FromForm(Name = "teamcreepexp")]
    public int TeamCreepExperience { get; set; }

    [FromForm(Name = "neutralcreepkills")]
    public int NeutralCreepKills { get; set; }

    [FromForm(Name = "neutralcreepdmg")]
    public int NeutralCreepDamage { get; set; }

    [FromForm(Name = "neutralcreepgold")]
    public int NeutralCreepGold { get; set; }

    [FromForm(Name = "neutralcreepexp")]
    public int NeutralCreepExperience { get; set; }

    [FromForm(Name = "bdmg")]
    public int BuildingDamage { get; set; }

    [FromForm(Name = "razed")]
    public int BuildingsRazed { get; set; }

    [FromForm(Name = "bdmgexp")]
    public int ExperienceFromBuildings { get; set; }

    [FromForm(Name = "bgold")]
    public int GoldFromBuildings { get; set; }

    [FromForm(Name = "denies")]
    public int Denies { get; set; }

    [FromForm(Name = "exp_denied")]
    public int ExperienceDenied { get; set; }

    [FromForm(Name = "gold")]
    public int Gold { get; set; }

    [FromForm(Name = "gold_spent")]
    public int GoldSpent { get; set; }

    [FromForm(Name = "exp")]
    public int Experience { get; set; }

    [FromForm(Name = "actions")]
    public int Actions { get; set; }

    [FromForm(Name = "secs")]
    public int SecondsPlayed { get; set; }

    [FromForm(Name = "level")]
    public int HeroLevel { get; set; }

    [FromForm(Name = "consumables")]
    public int ConsumablesUsed { get; set; }

    [FromForm(Name = "wards")]
    public int WardsPlaced { get; set; }

    [FromForm(Name = "bloodlust")]
    public int Bloodlust { get; set; }

    [FromForm(Name = "doublekill")]
    public int DoubleKill { get; set; }

    [FromForm(Name = "triplekill")]
    public int TripleKill { get; set; }

    [FromForm(Name = "quadkill")]
    public int QuadKill { get; set; }

    [FromForm(Name = "annihilation")]
    public int Annihilation { get; set; }

    [FromForm(Name = "ks3")]
    public int KillStreak3 { get; set; }

    [FromForm(Name = "ks4")]
    public int KillStreak4 { get; set; }

    [FromForm(Name = "ks5")]
    public int KillStreak5 { get; set; }

    [FromForm(Name = "ks6")]
    public int KillStreak6 { get; set; }

    [FromForm(Name = "ks7")]
    public int KillStreak7 { get; set; }

    [FromForm(Name = "ks8")]
    public int KillStreak8 { get; set; }

    [FromForm(Name = "ks9")]
    public int KillStreak9 { get; set; }

    [FromForm(Name = "ks10")]
    public int KillStreak10 { get; set; }

    [FromForm(Name = "ks15")]
    public int KillStreak15 { get; set; }

    [FromForm(Name = "smackdown")]
    public int Smackdown { get; set; }

    [FromForm(Name = "humiliation")]
    public int Humiliation { get; set; }

    [FromForm(Name = "nemesis")]
    public int Nemesis { get; set; }

    [FromForm(Name = "retribution")]
    public int Retribution { get; set; }

    [FromForm(Name = "score")]
    public int Score { get; set; }

    [FromForm(Name = "gameplaystat0")]
    public double GameplayStat0 { get; set; }

    [FromForm(Name = "gameplaystat1")]
    public double GameplayStat1 { get; set; }

    [FromForm(Name = "gameplaystat2")]
    public double GameplayStat2 { get; set; }

    [FromForm(Name = "gameplaystat3")]
    public double GameplayStat3 { get; set; }

    [FromForm(Name = "gameplaystat4")]
    public double GameplayStat4 { get; set; }

    [FromForm(Name = "gameplaystat5")]
    public double GameplayStat5 { get; set; }

    [FromForm(Name = "gameplaystat6")]
    public double GameplayStat6 { get; set; }

    [FromForm(Name = "gameplaystat7")]
    public double GameplayStat7 { get; set; }

    [FromForm(Name = "gameplaystat8")]
    public double GameplayStat8 { get; set; }

    [FromForm(Name = "gameplaystat9")]
    public double GameplayStat9 { get; set; }

    [FromForm(Name = "time_earning_exp")]
    public int TimeEarningExperience { get; set; }
}
