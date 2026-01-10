namespace ASPIRE.Common.Communication;

public static class ChatChannels
{
    // "KONGOR" is a special channel name that maps to "KONGOR 1", then to "KONGOR 2" if "KONGOR 1" is full, then to "KONGOR 3" if "KONGOR 2" is full, and so on.
    public const string GeneralChannel = "KONGOR"; // TODO: Implement Channel Load Balancing

    public const string GameMastersChannel = "GAME MASTERS";
    public const string GuestsChannel = "GUESTS";

    public const string
        ServerHostsChannel = "HOSTS"; // TODO: Implement Commands To Query Hosts From This Channel As Administrator

    public const string StreamersChannel = "STREAMERS";
    public const string VIPChannel = "VIP";

    // "TERMINAL" is a special channel from which chat server commands can be executed.
    public const string StaffChannel = "TERMINAL"; // TODO: Implement TERMINAL Command Palette

    public static readonly string[] AllDefaultChannels =
    [
        GeneralChannel, GameMastersChannel, GuestsChannel, ServerHostsChannel, StreamersChannel, VIPChannel,
        StaffChannel
    ];
}