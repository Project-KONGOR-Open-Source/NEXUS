namespace MERRICK.DatabaseContext.Entities.Statistics;

[Index(nameof(MatchID), IsUnique = true)]
public class MatchStatistics
{
    [Key]
    public int ID { get; set; }

    public DateTimeOffset TimestampRecorded { get; set; } = DateTimeOffset.UtcNow;

    public required int ServerID { get; set; }

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

    public required string GameMode { get; set; }

    public required int ScoreTeam1 { get; set; }

    public required int ScoreTeam2 { get; set; }

    public required int TeamScoreGoal { get; set; }

    public required int PlayerScoreGoal { get; set; }

    public required int NumberOfRounds { get; set; }

    public required string ReleaseStage { get; set; }

    public required string? BannedHeroes { get; set; }

    public int? ScheduledEventID { get; set; }

    public int? ScheduledMatchID { get; set; }

    public int? MVPAccountID { get; set; }

    public required int? AwardMostAnnihilations { get => field is -1 ? null : field; set => field = value is -1 ? null : value; }

    public required int? AwardMostQuadKills { get => field is -1 ? null : field; set => field = value is -1 ? null : value; }

    public required int? AwardLargestKillStreak { get => field is -1 ? null : field; set => field = value is -1 ? null : value; }

    public required int? AwardMostSmackdowns { get => field is -1 ? null : field; set => field = value is -1 ? null : value; }

    public required int? AwardMostKills { get => field is -1 ? null : field; set => field = value is -1 ? null : value; }

    public required int? AwardMostAssists { get => field is -1 ? null : field; set => field = value is -1 ? null : value; }

    public required int? AwardLeastDeaths { get => field is -1 ? null : field; set => field = value is -1 ? null : value; }

    public required int? AwardMostBuildingDamage { get => field is -1 ? null : field; set => field = value is -1 ? null : value; }

    public required int? AwardMostWardsKilled { get => field is -1 ? null : field; set => field = value is -1 ? null : value; }

    public required int? AwardMostHeroDamageDealt { get => field is -1 ? null : field; set => field = value is -1 ? null : value; }

    public required int? AwardHighestCreepScore { get => field is -1 ? null : field; set => field = value is -1 ? null : value; }

    public List<FragEvent>? FragHistory { get; set; }
}

public class FragEvent
{
    public required int SourceID { get; set; }

    public required int TargetID { get; set; }

    public required int GameTimeSeconds { get; set; }

    public required List<int>? SupporterIDs { get; set; }
}
