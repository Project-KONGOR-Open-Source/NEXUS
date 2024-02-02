namespace MERRICK.Database.Entities.Core;

[Index(nameof(Name), nameof(Tag), IsUnique = true)]
public class Clan
{
    [Key]
    public Guid ID { get; set; }

    [StringLength(20)]
    public required string Name { get; set; }

    [StringLength(3)]
    public required string Tag { get; set; }

    public List<Account> Members { get; set; } = [];

    public DateTime TimestampCreated { get; set; } = DateTime.UtcNow;
}

// TODO: Get Clan Tier Parser