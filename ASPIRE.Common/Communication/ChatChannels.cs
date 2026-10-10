namespace ASPIRE.Common.Communication;

public static class ChatChannels
{
    // "KONGOR" is the general channel while it is the only one, and is renamed "KONGOR 1" once it overflows into "KONGOR 2", "KONGOR 3", and so on; joining by any of these names joins whichever general channel is not full.
    public const string GeneralChannel = "KONGOR";

    public const string GameMastersChannel = "GAME MASTERS";
    public const string GuestsChannel = "GUESTS";
    public const string ServerHostsChannel = "HOSTS"; // TODO: Implement Commands To Query Hosts From This Channel As Administrator
    public const string StreamersChannel = "STREAMERS";
    public const string VIPChannel = "VIP";

    // "TERMINAL" is a special channel from which chat server commands can be executed.
    public const string StaffChannel = "TERMINAL"; // TODO: Implement TERMINAL Command Palette

    public static readonly string[] AllDefaultChannels = [ GeneralChannel, GameMastersChannel, GuestsChannel, ServerHostsChannel, StreamersChannel, VIPChannel, StaffChannel ];

    // Clan channels are named after their clan, following this prefix and a space.
    public const string ClanChannelPrefix = "Clan";

    // Match channels are named after their match ID, following this prefix and a space.
    public const string MatchChannelPrefix = "Match";

    // Matchmaking group channels are named after their group ID, following this prefix and a space.
    public const string GroupChannelPrefix = "TMM Group";

    /// <summary>
    ///     Determines whether the channel is one which accounts are placed in based on their state, rather than one which players choose freely.
    ///     These are the general channel along with its numbered overflow channels, the clan channels, the role channels, and the match and matchmaking group channels.
    /// </summary>
    public static bool IsReservedChannel(string channelName)
    {
        if (AllDefaultChannels.Contains(channelName, StringComparer.OrdinalIgnoreCase))
            return true;

        if (IsGeneralChannel(channelName) || IsClanChannel(channelName))
            return true;

        return HasPrefix(channelName, MatchChannelPrefix) || HasPrefix(channelName, GroupChannelPrefix);
    }

    /// <summary>
    ///     Determines whether the channel is the general channel or one of the numbered overflow channels which the general channel is load-balanced across.
    /// </summary>
    public static bool IsGeneralChannel(string channelName)
    {
        if (channelName.Equals(GeneralChannel, StringComparison.OrdinalIgnoreCase))
            return true;

        // Overflow General Channels Are Named After The General Channel, Followed By A Number
        return HasPrefix(channelName, GeneralChannel) && int.TryParse(channelName.AsSpan(GeneralChannel.Length + 1), out _);
    }

    /// <summary>
    ///     Determines whether the channel is the channel of a clan, whether or not that clan exists.
    /// </summary>
    public static bool IsClanChannel(string channelName)
        => HasPrefix(channelName, ClanChannelPrefix);

    private static bool HasPrefix(string channelName, string prefix)
        => channelName.StartsWith(prefix + TextConstant.Space, StringComparison.OrdinalIgnoreCase);
}
