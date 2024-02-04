namespace MERRICK.Database.Entities.Core;

[Index(nameof(Name), IsUnique = true)]
public class Account
{
    [Key]
    public Guid ID { get; set; }

    [MaxLength(15)]
    public required string Name { get; set; }

    public required User User { get; set; }

    public AccountType Type { get; set; } = AccountType.Legacy;

    public required bool IsMain { get; set; }

    public Clan? Clan { get; set; } = null;

    public ClanTier ClanTier { get; set; } = ClanTier.None;

    public DateTime? TimestampJoinedClan { get; set; } = null;

    public int AscensionLevel { get; set; } = 0;

    public DateTime TimestampCreated { get; set; } = DateTime.UtcNow;

    public DateTime TimestampLastActive { get; set; } = DateTime.UtcNow;

    public List<string> AutoConnectChatChannels { get; set; } = [];

    public List<BannedPeer> BannedPeers { get; set; } = [];

    public List<FriendedPeer> FriendedPeers { get; set; } = [];

    public List<IgnoredPeer> IgnoredPeers { get; set; } = [];

    public List<string> SelectedStoreItems { get; set; } = ["ai.Default Icon", "cc.white", "t.Standard"];

    public List<string> IPAddressCollection { get; set; } = [];

    public List<string> MACAddressCollection { get; set; } = [];

    public List<string> SystemInformationCollection { get; set; } = [];

    public List<string> SystemInformationHashCollection { get; set; } = [];

    [NotMapped]
    public string NameWithClanTag => Equals(Clan, null) ? Name : $"[{Clan.Tag}]{Name}";

    [NotMapped]
    public string ClanTierName => ClanTier switch
    {
        ClanTier.None       => "None",
        ClanTier.Member     => "Member",
        ClanTier.Officer    => "Officer",
        ClanTier.Leader     => "Leader",
        _                   => throw new ArgumentOutOfRangeException(@$"Unsupported Clan Tier ""{ClanTier}""")
    };
}
