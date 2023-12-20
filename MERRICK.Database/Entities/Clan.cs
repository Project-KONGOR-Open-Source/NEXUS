namespace MERRICK.Database.Entities;

[Index(nameof(Name), nameof(Tag), IsUnique = true)]
public class Clan
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(20)]
    public required string Name { get; set; } = null!;

    [StringLength(5)]
    public required string Tag { get; set; } = null!;

    public List<Account> Members { get; set; } = [];

    [Required]
    public DateTime TimestampCreated { get; set; } = DateTime.UtcNow;
}
