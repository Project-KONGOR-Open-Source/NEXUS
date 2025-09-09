namespace MERRICK.Database.Entities.Game;

public class HeroGuide
{
    [Key]
    public int ID { get; set; }

    [MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(20)]
    public required string HeroName { get; set; }

    [MaxLength(25)]
    public required string HeroIdentifier { get; set; }

    [MaxLength(500)]
    public required string Intro { get; set; }

    [MaxLength(1500)]
    public required string Content { get; set; }

    [MaxLength(150)]
    public required string StartingItems { get; set; }

    [MaxLength(150)]
    public required string EarlyGameItems { get; set; }

    [MaxLength(150)]
    public required string CoreItems { get; set; }

    [MaxLength(150)]
    public required string LuxuryItems { get; set; }

    [MaxLength(750)]
    public required string AbilityQueue { get; set; }

    public required Account Author { get; set; }

    public required float Rating { get; set; }

    public required int UpVotes { get; set; }

    public required int DownVotes { get; set; }

    public required bool Public { get; set; }

    public required bool Featured { get; set; }

    public DateTimeOffset TimestampCreated { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? TimestampLastUpdated { get; set; }
}
