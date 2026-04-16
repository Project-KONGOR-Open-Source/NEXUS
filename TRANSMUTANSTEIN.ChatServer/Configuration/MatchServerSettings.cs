namespace TRANSMUTANSTEIN.ChatServer.Configuration;

/// <summary>
///     Configuration settings for match server remote commands.
///     These values are sent to the HON match server via the chat server's remote command channel during the server handshake.
///     Each property maps directly to a match server console variable (CVAR).
/// </summary>
public class MatchServerSettings
{
    /// <summary>
    ///     The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "MatchServer";

    /// <summary>
    ///     The rate at which the match server processes game state updates (player inputs, entity positions, combat, physics, etc.).
    ///     Maps to the "svr_gameFPS" CVAR. Has a range of 1Hz to 60Hz, with a default of 20Hz that translates to 50ms per tick.
    ///     Higher values increase game world simulation fidelity at the cost of higher CPU and bandwidth usage.
    /// </summary>
    public int TickRate { get; set; } = 20;

    /// <summary>
    ///     Whether the match server should submit detailed item purchase statistics at the end of each match.
    ///     Maps to the "svr_submitMatchStatItems" CVAR.
    /// </summary>
    public bool SubmitMatchStatisticsItems { get; set; } = true;

    /// <summary>
    ///     Whether the match server should submit detailed ability usage statistics at the end of each match.
    ///     Maps to the "svr_submitMatchStatAbilities" CVAR.
    /// </summary>
    public bool SubmitMatchStatisticsAbilities { get; set; } = true;

    /// <summary>
    ///     Whether the match server should submit detailed kill/death statistics at the end of each match.
    ///     Maps to the "svr_submitMatchStatFrags" CVAR.
    /// </summary>
    public bool SubmitMatchStatisticsFrags { get; set; } = true;

    /// <summary>
    ///     Whether the match server should broadcast its presence on the local network.
    ///     Maps to the "svr_broadcast" CVAR.
    ///     Should be disabled in production to prevent the server from appearing in local server browsers.
    /// </summary>
    public bool Broadcast { get; set; } = true;

    /// <summary>
    ///     The maximum outgoing bandwidth per client in bytes per second.
    ///     Maps to the "svr_maxbps" CVAR.
    ///     A value of 0 uses the match server's built-in default.
    /// </summary>
    public int MaxBytesPerSecond { get; set; } = 0;

    /// <summary>
    ///     The interval in milliseconds at which the match server logs long frame warnings.
    ///     Maps to the "svr_longFrameWarnTime" CVAR (default 125).
    ///     Frames exceeding this duration are logged as performance warnings.
    ///     A value of 0 uses the match server's built-in default.
    /// </summary>
    public int LongFrameWarnTime { get; set; } = 0;
}
