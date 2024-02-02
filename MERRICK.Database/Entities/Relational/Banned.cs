namespace MERRICK.Database.Entities.Relational;

[Index(nameof(AccountName), IsUnique = true)]
public class Banned
{
    public Guid AccountID { get; set; }

    [MaxLength(15)]
    public required string AccountName { get; set; }

    [MaxLength(30)]
    public required string Reason { get; set; }
}
