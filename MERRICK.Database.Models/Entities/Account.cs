namespace MERRICK.Database.Models.Entities;

[Index(nameof(Name), IsUnique = true)]
public class Account
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(15)]
    public required string Name { get; set; }

    public required User User { get; set; }

    public AccountType AccountType { get; set; } = AccountType.Legacy;

    public Clan? Clan { get; set; }

    public ClanTier ClanTier { get; set; } = ClanTier.None;

    public int AscensionLevel { get; set; } = 0;

    public DateTime TimestampCreated { get; set; } = DateTime.UtcNow;

    public DateTime TimestampLastActive { get; set; } = DateTime.UtcNow;

    public List<string> SelectedStoreItems { get; set; } = ["ai.Default Icon", "cc.white", "t.Standard"];

    public List<string> IPAddressCollection { get; set; } = [];

    public List<string> HardwareIdCollection { get; set; } = [];

    public List<string> MACAddressCollection { get; set; } = [];

    public List<string> SystemInformationCollection { get; set; } = [];

    [NotMapped]
    public bool IsMain => Name == User.Name;

    [NotMapped]
    public string NameWithClanTag => Clan == null ? Name : $"[{Clan.Tag}]{Name}";
}
