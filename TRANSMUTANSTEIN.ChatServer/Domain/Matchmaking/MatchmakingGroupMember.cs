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
    ///     The player's Casual TMR for casual game modes.
    /// </summary>
    public double CasualTMR { get; set; } = 1500.0;

    /// <summary>
    ///     Whether the player's rating for the queued game type is still in its placement phase, in which case no medal is shown.
    /// </summary>
    public bool IsInPlacementPhase { get; set; }

    /// <summary>
    ///     Whether the player's casual rating is still in its placement phase, in which case no casual medal is shown.
    /// </summary>
    public bool IsInCasualPlacementPhase { get; set; }

    /// <summary>
    ///     The pre-calculated TMR gain value for winning the current match.
    /// </summary>
    public double MatchWinValue { get; set; }

    /// <summary>
    ///     The pre-calculated TMR loss value for losing the current match.
    /// </summary>
    public double MatchLossValue { get; set; }

    /// <summary>
    ///     Whether the player is in the provisional phase of their rating for the queued game type.
    ///     Assigned together with the match point values and reported to the match server, which awards provisional players bonus rating for kill streaks as a form of smurf protection.
    /// </summary>
    public bool IsProvisional { get; set; }
}
