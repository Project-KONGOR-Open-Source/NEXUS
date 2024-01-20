namespace MERRICK.Database.Entities;

[Index(nameof(Name), IsUnique = true)]
public class Account
{
    [Key]
    public Guid ID { get; set; }

    [StringLength(15)]
    public required string Name { get; set; }

    public required User User { get; set; }

    public AccountType Type { get; set; } = AccountType.Legacy;

    public required bool IsMain { get; set; }

    public Clan? Clan { get; set; } = default;

    public ClanTier ClanTier { get; set; } = ClanTier.None;

    public int AscensionLevel { get; set; } = 0;

    public DateTime TimestampCreated { get; set; } = DateTime.UtcNow;

    public DateTime TimestampLastActive { get; set; } = DateTime.UtcNow;

    public List<string> SelectedStoreItems { get; set; } = ["ai.Default Icon", "cc.white", "t.Standard"];

    public List<string> IPAddressCollection { get; set; } = [];

    public List<string> HardwareIDCollection { get; set; } = [];

    public List<string> MACAddressCollection { get; set; } = [];

    public List<string> SystemInformationCollection { get; set; } = [];

    [NotMapped]
    public string NameWithClanTag => Equals(Clan, null) ? Name : $"[{Clan.Tag}]{Name}";
}
