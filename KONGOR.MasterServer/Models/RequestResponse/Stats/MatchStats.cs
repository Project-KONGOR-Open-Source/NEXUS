namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class MatchStats
{
    [FromForm(Name = "server_id")] public required int ServerID { get; set; }

    [FromForm(Name = "match_id")] public required int MatchID { get; set; }

    [FromForm(Name = "map")] public required string Map { get; set; }

    [FromForm(Name = "map_version")] public required string MapVersion { get; set; }

    [FromForm(Name = "time_played")] public required int TimePlayed { get; set; }

    [FromForm(Name = "file_size")] public required int FileSize { get; set; }

    [FromForm(Name = "file_name")] public required string FileName { get; set; }

    [FromForm(Name = "c_state")] public required int ConnectionState { get; set; }

    [FromForm(Name = "version")] public required string Version { get; set; }

    [FromForm(Name = "avgpsr")] public required int AveragePSR { get; set; }

    [FromForm(Name = "avgpsr_team1")] public required int AveragePSRTeamOne { get; set; }

    [FromForm(Name = "avgpsr_team2")] public required int AveragePSRTeamTwo { get; set; }

    [FromForm(Name = "gamemode")] public required string GameMode { get; set; }

    [FromForm(Name = "teamscoregoal")] public required int TeamScoreGoal { get; set; }

    [FromForm(Name = "playerscoregoal")] public required int PlayerScoreGoal { get; set; }

    [FromForm(Name = "numrounds")] public required int NumberOfRounds { get; set; }

    [FromForm(Name = "release_stage")] public required string ReleaseStage { get; set; }

    [FromForm(Name = "banned_heroes")] public string? BannedHeroes { get; set; }

    [FromForm(Name = "event_id")] public int? ScheduledEventID { get; set; }

    [FromForm(Name = "matchup_id")] public int? ScheduledMatchID { get; set; }

    [FromForm(Name = "mvp")] public int? MVPAccountID { get; set; }

    [FromForm(Name = "awd_mann")] public required int AwardMostAnnihilations { get; set; }

    [FromForm(Name = "awd_mqk")] public required int AwardMostQuadKills { get; set; }

    [FromForm(Name = "awd_lgks")] public required int AwardLargestKillStreak { get; set; }

    [FromForm(Name = "awd_msd")] public required int AwardMostSmackdowns { get; set; }

    [FromForm(Name = "awd_mkill")] public required int AwardMostKills { get; set; }

    [FromForm(Name = "awd_masst")] public required int AwardMostAssists { get; set; }

    [FromForm(Name = "awd_ledth")] public required int AwardLeastDeaths { get; set; }

    [FromForm(Name = "awd_mbdmg")] public required int AwardMostBuildingDamage { get; set; }

    [FromForm(Name = "awd_mwk")] public required int AwardMostWardsKilled { get; set; }

    [FromForm(Name = "awd_mhdd")] public required int AwardMostHeroDamageDealt { get; set; }

    [FromForm(Name = "awd_hcs")] public required int AwardHighestCreepScore { get; set; }

    [FromForm(Name = "submission_debug")] public required string SubmissionDebug { get; set; }
}