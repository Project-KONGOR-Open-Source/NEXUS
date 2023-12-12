namespace MERRICK.Database.Entities;

[Index(nameof(Name), IsUnique = true)]
public class Account(string name, User user)
{
    [Key]
    public int ID { get; set; }

    [StringLength(20)]
    public required string Name { get; set; } = name;

    public required User User { get; set; } = user;

    public AccountType AccountType { get; set; } = AccountType.Legacy;

    public Clan? Clan { get; set; } = null;

    public ClanTier ClanTier { get; set; } = ClanTier.None;

    public int AscensionLevel { get; set; } = 0;

    public DateTime TimestampCreated { get; set; } = DateTime.UtcNow;

    public DateTime TimestampLastActive { get; set; } = DateTime.UtcNow;

    public List<string> SelectedStoreItems { get; set; } = [];

    public List<string> IPAddressCollection { get; set; } = [];

    public List<string> HardwareIDCollection { get; set; } = [];

    public List<string> MACAddressCollection { get; set; } = [];

    public List<string> SystemInformationCollection { get; set; } = [];

    [NotMapped]
    public bool IsMain => Name == User.Name;

    [NotMapped]
    public string NameWithClanTag => Clan == null ? Name : $"[{Clan.Tag}]{Name}";
}
