namespace MERRICK.Database.Entities.Relational;

[Index(nameof(AccountName), IsUnique = true)]
public class Friend
{
    // This Property Cannot Be Named "AccountID" Because Entity Framework Will Try To Create A Foreign Key To The Account Table
    // And Because This Entity Is Stored In The Database As JSON, Overriding The Name Of The Column For This Property Is Not Possible
    public required Guid AccountIdentifier { get; set; }

    [MaxLength(15)]
    public required string AccountName { get; set; }

    [StringLength(4)]
    public required string? ClanTag { get; set; }

    [MaxLength(15)]
    public required string Group { get; set; }
}
