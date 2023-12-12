namespace MERRICK.Database.Entities;

[Index(nameof(Name), nameof(Tag), IsUnique = true)]
public class Clan(string name, string tag)
{
    [Key]
    public int ID { get; set; }

    [StringLength(20)]
    public required string Name { get; set; } = name;

    [StringLength(5)]
    public required string Tag { get; set; } = tag;

    public List<Account> Members { get; set; } = [];

    [Required]
    public DateTime TimestampCreated { get; set; } = DateTime.UtcNow;
}
