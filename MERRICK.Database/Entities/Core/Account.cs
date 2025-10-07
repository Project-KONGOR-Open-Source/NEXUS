namespace MERRICK.Database.Entities.Core;

[Index(nameof(Name), IsUnique = true)]
public class Account
{
    [Key]
    public int ID { get; set; }

    [MaxLength(15)]
    public required string Name { get; set; }

    public required User User { get; set; }

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

    public static (string ClanTag, string AccountName) SeparateClanTagFromAccountName(string accountNameWithClanTag)
    {
        // If no '[' and ']' characters are found, then the account is not part of a clan and has no clan tag.
        if (accountNameWithClanTag.Contains('[').Equals(false) && accountNameWithClanTag.Contains(']').Equals(false))
            return (string.Empty, accountNameWithClanTag);

        // If '[' is not the first character, then the account name contains the '[' and ']' characters, but the account is not part of a clan and has no clan tag.
        if (accountNameWithClanTag.StartsWith('[').Equals(false))
            return (string.Empty, accountNameWithClanTag);

        // Remove the leading '[' character and split at the first occurrence of the ']' character. The resulting account name may contain the '[' and ']' characters.
        string[] segments = accountNameWithClanTag.TrimStart('[').Split(']', count: 2);

        return (segments.First(), segments.Last());
    }

    public string Icon => SelectedStoreItems.SingleOrDefault(item => item.StartsWith("ai.")) ?? "ai.Default Icon";
    public string IconNoPrefixCode => Icon.Replace("ai.", string.Empty);

    public string ChatSymbol => SelectedStoreItems.SingleOrDefault(item => item.StartsWith("cs.")) ?? string.Empty;
    public string ChatSymbolNoPrefixCode => ChatSymbol.Replace("cs.", string.Empty);

    public string NameColour => SelectedStoreItems.SingleOrDefault(item => item.StartsWith("cc.")) ?? "cc.white";
    public string NameColourNoPrefixCode => NameColour.Replace("cc.", string.Empty);
}
