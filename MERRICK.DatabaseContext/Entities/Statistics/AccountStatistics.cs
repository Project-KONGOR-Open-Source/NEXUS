using ASPIRE.Common.Enumerations.Statistics;

namespace MERRICK.DatabaseContext.Entities.Statistics;

[Table("account_statistics")]
[Index(nameof(AccountID), nameof(Type), IsUnique = true)]
public class AccountStatistics
{
    [Key]
    [Column("id")]
    public int ID { get; set; }

    [Column("account_id")]
    public int AccountID { get; set; }

    [ForeignKey(nameof(AccountID))]
    public required Account Account { get; set; }

    [Column("type")]
    public required AccountStatisticsType Type { get; set; }

    [Column("matches_played")] public int MatchesPlayed { get; set; }

    [Column("matches_won")] public int MatchesWon { get; set; }

    [Column("matches_lost")] public int MatchesLost { get; set; }

    [Column("matches_conceded")] public int MatchesConceded { get; set; }

    [Column("matches_disconnected")] public int MatchesDisconnected { get; set; }

    [Column("matches_kicked")] public int MatchesKicked { get; set; }

    [Column("skill_rating")] public double SkillRating { get; set; }

    [Column("performance_score")] public double PerformanceScore { get; set; }

    [Column("placement_matches_data")] public string? PlacementMatchesData { get; set; }

    [NotMapped] public Rank Rank => RankExtensions.GetRank(SkillRating);
}

public enum AccountStatisticsType
{
    Cooperative       = 0,
    Public            = 1,
    Matchmaking       = 2,
    MatchmakingCasual = 3,
    MidWars           = 4,
    RiftWars          = 5,
    Player            = 6
}