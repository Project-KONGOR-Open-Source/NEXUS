namespace MERRICK.DatabaseContext.Entities.Core;

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

    public ConfigurationBackup? ConfigurationBackup { get; set; } = null;

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

    /// <summary>
    ///     Gets the chat channels which the account joins on every login, regardless of its auto-connect list.
    ///     These are the general channel, the account's clan channel, and the channels of the account's role, where the hosts channel also extends to every account of a user who owns a host account.
    /// </summary>
    public List<string> GetDefaultChatChannels()
    {
        List<string> channels = [];

        if (Type is not AccountType.ServerHost)
            channels.Add(ChatChannels.GeneralChannel);

        if (Clan is not null)
            channels.Add(Clan.GetChatChannelName());

        if (Type is AccountType.GameMaster or AccountType.Staff)
            channels.Add(ChatChannels.GameMastersChannel);

        if (Type is AccountType.Guest or AccountType.Staff)
            channels.Add(ChatChannels.GuestsChannel);

        // Host Accounts Are Registered As Sub-Accounts Which Cannot Log Into The Game Client, So The Hosts Channel Also Belongs To Every Other Account Of A User Who Owns A Host Account
        if (Type is AccountType.ServerHost or AccountType.Staff || User.Accounts.Any(subAccount => subAccount.Type is AccountType.ServerHost))
            channels.Add(ChatChannels.ServerHostsChannel);

        if (Type is AccountType.Streamer or AccountType.Staff)
            channels.Add(ChatChannels.StreamersChannel);

        if (Type is AccountType.VIP or AccountType.Staff)
            channels.Add(ChatChannels.VIPChannel);

        if (Type is AccountType.Staff)
            channels.Add(ChatChannels.StaffChannel);

        return channels;
    }

    /// <summary>
    ///     Determines whether the channel is one of the account's default chat channels, matching the channel name case-insensitively.
    ///     Every general channel name counts as the general channel, since the general channel is load-balanced across its numbered channels.
    /// </summary>
    public bool IsDefaultChatChannel(string channelName)
    {
        string defaultChannelName = ChatChannels.IsGeneralChannel(channelName) ? ChatChannels.GeneralChannel : channelName;

        return GetDefaultChatChannels().Contains(defaultChannelName, StringComparer.OrdinalIgnoreCase);
    }

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
