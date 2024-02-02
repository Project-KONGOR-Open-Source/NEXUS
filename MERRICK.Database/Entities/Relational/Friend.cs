namespace MERRICK.Database.Entities.Relational;

[Index(nameof(AccountName), IsUnique = true)]
public class Friend
{
    public required Guid AccountID { get; set; }

    [MaxLength(15)]
    public required string AccountName { get; set; }

    [StringLength(4)]
    public required string? ClanTag { get; set; }

    [MaxLength(15)]
    public required string Group { get; set; }
}
