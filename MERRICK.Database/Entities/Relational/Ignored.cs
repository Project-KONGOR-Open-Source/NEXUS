namespace MERRICK.Database.Entities.Relational;

[Index(nameof(AccountName), IsUnique = true)]
public class Ignored
{
    public required Guid AccountID { get; set; }

    [MaxLength(15)]
    public required string AccountName { get; set; }
}
