namespace TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

public class MatchmakingGroupMember(ClientChatSession session)
{
    public Account Account = session.Account;

    public ClientChatSession Session = session;

    public required byte Slot { get; set; }

    public required bool IsLeader { get; set; }

    public required bool IsReady { get; set; }

    public required bool IsInGame { get; set; }

    public required bool IsEligibleForMatchmaking { get; set; }

    public required byte LoadingPercent { get; set; }

    public string Country { get; set; } = "NEWERTH";

    /// <summary>
    ///     Whether or not the group member has access to all of the group's game modes.
    /// </summary>
    public bool HasGameModeAccess { get; set; } = true;

    /// <summary>
    ///     The group member's game mode access, delimited by "|" (e.g. "true|true|false").
    /// </summary>
    public required string GameModeAccess { get; set; }

    /// <summary>
    ///     The player's Team Match Rating (TMR) for matchmaking.
    /// </summary>
    public double TMR { get; set; } = 1500.0;

    /// <summary>
    ///     The total number of matches played by this player.
    /// </summary>
    public int TotalMatchCount { get; set; } = 0;

    /// <summary>
    ///     The number of matches played in the current game type.
    /// </summary>
    public int GameTypeMatchCount { get; set; } = 0;

    /// <summary>
    ///     The number of recent wins for streak tracking.
    /// </summary>
    public int RecentWins { get; set; } = 0;

    /// <summary>
    ///     The number of recent losses for streak tracking.
    /// </summary>
    public int RecentLosses { get; set; } = 0;

    /// <summary>
    ///     The TMR adjustment based on match history.
    /// </summary>
    public double MatchHistoryAdjustment { get; set; } = 0.0;

    /// <summary>
    ///     The pre-calculated TMR gain value for winning the current match.
    /// </summary>
    public double MatchWinValue { get; set; }

    /// <summary>
    ///     The pre-calculated TMR loss value for losing the current match.
    /// </summary>
    public double MatchLossValue { get; set; }

    /// <summary>
    ///     The player's IP address for conflict detection.
    /// </summary>
    public string IPAddress { get; set; } = string.Empty;
}
