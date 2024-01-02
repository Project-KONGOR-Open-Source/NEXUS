namespace MERRICK.Database.Models.Entities;

[Index(nameof(Name), nameof(SanitizedEmailAddress), IsUnique = true)]
public class User : IdentityUser<Guid>
{
    [MaxLength(15)]
    public required string Name { get; set; }

    [MaxLength(30)]
    public required string EmailAddress { get; set; }

    [MaxLength(30)]
    public required string SanitizedEmailAddress { get; set; }

    [StringLength(512)]
    public required string SRPSalt { get; set; } // TODO: Maybe Just Rename This To PasswordSalt ?

    [StringLength(22)]
    public required string SRPPasswordSalt { get; set; } // TODO: Maybe Just Rename This To PasswordSRPSalt ? Is This Even Needed ?

    [StringLength(64)]
    public required string SRPPasswordHash { get; set; } // TODO: Maybe Just Rename This To PasswordSRPHash ?

    public HashSet<Account> Accounts { get; set; } = [];

    public int GoldCoins { get; set; } = 0;

    public int SilverCoins { get; set; } = 0;

    public int PlinkoTickets { get; set; } = 0;

    public int TotalLevel { get; set; } = 0;

    public int TotalExperience { get; set; } = 0;

    public HashSet<string> OwnedStoreItems { get; set; } = ["ai.Default Icon", "cc.white", "t.Standard"];
}
