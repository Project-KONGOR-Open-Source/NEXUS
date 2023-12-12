namespace MERRICK.Database.Entities;

[Index(nameof(Name), nameof(EmailAddress), IsUnique = true)]
public class User(string name, string emailAddress, string salt, string passwordSalt, string hashedPassword)
{
    [Key]
    public int ID { get; set; }

    [StringLength(20)]
    public required string Name { get; set; } = name;

    public required string EmailAddress { get; set; } = emailAddress;

    public required string Salt { get; set; } = salt;

    public required string PasswordSalt { get; set; } = passwordSalt;

    public required string HashedPassword { get; set; } = hashedPassword;

    public List<Account> Accounts { get; set; } = [];

    public int GoldCoins { get; set; } = 0;

    public int SilverCoins { get; set; } = 0;

    public int PlinkoTickets { get; set; } = 0;

    public int TotalLevel { get; set; } = 0;

    public int TotalExperience { get; set; } = 0;

    public List<string> OwnedStoreItems { get; set; } = [];
}
