﻿namespace MERRICK.DatabaseContext.Entities.Statistics;

[Index(nameof(MatchID), IsUnique = true)]
public class MatchStatistics
{
    [Key]
    public int ID { get; set; }

    public required long ServerID { get; set; }

    [MaxLength(15)]
    public required string HostAccountName { get; set; }

    public required int MatchID { get; set; }

    public required string Map { get; set; }

    [MaxLength(15)]
    public required string MapVersion { get; set; }

    public required int TimePlayed { get; set; }

    public required int FileSize { get; set; }

    public required string FileName { get; set; }

    public required int ConnectionState { get; set; }

    public required string Version { get; set; }

    public required int AveragePSR { get; set; }

    public required int AveragePSRTeamOne { get; set; }

    public required int AveragePSRTeamTwo { get; set; }

    // TODO: MMR And Casual MMR May Need To Also Be Added Here
    // TODO: PSR And (Casual) MMR Should Default To The Data Type Default Value If Not Provided

    public required string GameMode { get; set; }

    public required int ScoreTeam1 { get; set; }

    public required int ScoreTeam2 { get; set; }

    public required int TeamScoreGoal { get; set; }

    public required int PlayerScoreGoal { get; set; }

    public required int NumberOfRounds { get; set; }

    public required string ReleaseStage { get; set; }

    public required string BannedHeroes { get; set; }

    public required int AwardMostAnnihilations { get; set; }

    public required int AwardMostQuadKills { get; set; }

    public required int AwardLargestKillStreak { get; set; }

    public required int AwardMostSmackdowns { get; set; }

    public required int AwardMostKills { get; set; }

    public required int AwardMostAssists { get; set; }

    public required int AwardLeastDeaths { get; set; }

    public required int AwardMostBuildingDamage { get; set; }

    public required int AwardMostWardsKilled { get; set; }

    public required int AwardMostHeroDamageDealt { get; set; }

    public required int AwardHighestCreepScore { get; set; }

    public required string SubmissionDebug { get; set; }
}
