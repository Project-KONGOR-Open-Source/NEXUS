namespace ASPIRE.Common.Communication;

public static class ChatChannels
{
    // "KONGOR" is a special channel name that maps to "KONGOR 1", then to "KONGOR 2" if "KONGOR 1" is full, then to "KONGOR 3" if "KONGOR 2" is full, and so on.
    public const string GeneralChannel = "KONGOR"; // TODO: Implement Channel Load Balancing

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

        if (HasPrefix(channelName, ClanChannelPrefix) || HasPrefix(channelName, MatchChannelPrefix) || HasPrefix(channelName, GroupChannelPrefix))
            return true;

        // Overflow General Channels Are Named After The General Channel, Followed By A Number
        return HasPrefix(channelName, GeneralChannel) && int.TryParse(channelName.AsSpan(GeneralChannel.Length + 1), out _);
    }

    private static bool HasPrefix(string channelName, string prefix)
        => channelName.StartsWith($"{prefix} ", StringComparison.OrdinalIgnoreCase);
}
