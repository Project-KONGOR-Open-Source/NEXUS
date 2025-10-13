﻿namespace TRANSMUTANSTEIN.ChatServer.Matchmaking;

public class MatchmakingGroupMember(ChatSession session)
{
    public Account Account = session.Account;

    public ChatSession Session = session;

    public required byte Slot { get; set; }

    public required bool IsLeader { get; set; }

    public required bool IsReady { get; set; }

    public required bool IsInGame { get; set; }

    public required bool IsEligibleForMatchmaking { get; set; }

    public required byte LoadingPercent { get; set; }

    public string Country { get; set; } = "NEWERTH";

    /// <summary>
    ///     Whether Or Not The Group Member Has Access To All Of The Group's Game Modes
    /// </summary>
    public bool HasGameModeAccess { get; set; } = true;

    /// <summary>
    ///     The Group Member's Game Mode Access, Delimited By "|" (e.g. "true|true|false")
    /// </summary>
    public required string GameModeAccess { get; set; }
}
