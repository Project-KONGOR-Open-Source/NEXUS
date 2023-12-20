namespace MERRICK.Database.Entities;

[Index(nameof(Name), nameof(EmailAddress), IsUnique = true)]
public class User() : IdentityUser
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(20)]
    public required string Name { get; set; } = null!;

    public required string EmailAddress { get; set; } = null!;

    public required string Salt { get; set; } = null!;

    public required string PasswordSalt { get; set; } = null!;

    public required string HashedPassword { get; set; } = null!;

    public List<Account> Accounts { get; set; } = [];

    public int GoldCoins { get; set; } = 0;

    public int SilverCoins { get; set; } = 0;

    public int PlinkoTickets { get; set; } = 0;

    public int TotalLevel { get; set; } = 0;

    public int TotalExperience { get; set; } = 0;

    public List<string> OwnedStoreItems { get; set; } = [];
}
