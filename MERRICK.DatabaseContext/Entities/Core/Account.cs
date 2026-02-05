namespace MERRICK.DatabaseContext.Entities.Core;
using global::MERRICK.DatabaseContext.Entities.Statistics;

[Index(nameof(Name), IsUnique = true)]
public class Account
{
    [Key] public int ID { get; set; }

    [MaxLength(15)] public required string Name { get; set; }

    public required User User { get; set; }

    public string? Cookie { get; set; }

    public AccountType Type { get; set; } = AccountType.Legacy;

    public required bool IsMain { get; set; }

    public Clan? Clan { get; set; } = null;

    public ClanTier ClanTier { get; set; } = ClanTier.None;

    public DateTimeOffset? TimestampJoinedClan { get; set; } = null;

    public int AscensionLevel { get; set; } = 0;

    public DateTimeOffset TimestampCreated { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset TimestampLastActive { get; set; } = DateTimeOffset.UtcNow;

    public List<string> AutoConnectChatChannels { get; set; } = [];

    public List<BannedPeer> BannedPeers { get; set; } = [];

    public List<FriendedPeer> FriendedPeers { get; set; } = [];

    public List<IgnoredPeer> IgnoredPeers { get; set; } = [];

    public List<string> SelectedStoreItems { get; set; } = ["ai.Default Icon", "cc.white", "t.Standard"];

    public List<string> IPAddressCollection { get; set; } = [];

    public List<string> MACAddressCollection { get; set; } = [];

    public List<string> SystemInformationCollection { get; set; } = [];

    public List<string> SystemInformationHashCollection { get; set; } = [];

    public List<string> BlockedPhrases { get; set; } = [];

    public bool UseCloud { get; set; } = false;

    public bool AutomaticCloudUpload { get; set; } = false;

    public DateTimeOffset? BackupLastUpdatedTime { get; set; } = null;

    public List<AccountStatistics> Statistics { get; set; } = [];
}