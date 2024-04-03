namespace MERRICK.Database.Entities.Core;

[Index(nameof(EmailAddress), IsUnique = true)]
public class User
{
    [Key]
    public int ID { get; set; }

    [MaxLength(30)]
    public required string EmailAddress { get; set; }

    public required Role Role { get; set; }

    [StringLength(64)]
    public required string SRPPasswordSalt { get; set; }

    [StringLength(64)]
    public required string SRPPasswordHash { get; set; }

    [StringLength(84)]
    public string PBKDF2PasswordHash { get; set; } = null!;

    public List<Account> Accounts { get; set; } = [];

    public DateTime TimestampCreated { get; set; } = DateTime.UtcNow;

    public DateTime TimestampLastActive { get; set; } = DateTime.UtcNow;

    public int GoldCoins { get; set; } = 0;

    public int SilverCoins { get; set; } = 0;

    public int PlinkoTickets { get; set; } = 0;

    public int TotalLevel { get; set; } = 0;

    public int TotalExperience { get; set; } = 0;

    public List<string> OwnedStoreItems { get; set; } = ["ai.Default Icon", "cc.white", "t.Standard"];

    [NotMapped]
    public bool IsAdministrator => Role.Name.Equals(UserRoles.Administrator);
}
