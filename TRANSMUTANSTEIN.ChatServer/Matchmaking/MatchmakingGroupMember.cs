namespace TRANSMUTANSTEIN.ChatServer.Matchmaking;

public class MatchmakingGroupMember
{
    public required int ID { get; set; }

    public required string Name { get; set; }

    public required byte Slot { get; set; }

    public required bool IsLeader { get; set; }

    public required bool IsReady { get; set; }

    public required bool IsInGame { get; set; }

    public required bool IsEligibleForMatchmaking { get; set; }

    public required byte LoadingPercent { get; set; }

    public required string ChatNameColor { get; set; }

    public required string AccountIcon { get; set; }

    public string Country { get; set; } = "NEWERTH";

    /// <summary>
    ///     Whether Or Not The Group Member Has Access To All Of The Group's Game Modes
    /// </summary>
    public bool HasGameModeAccess { get; set; } = true;

    /// <summary>
    ///     The Group Member's Game Mode Access, Delimited By "|" (e.g. "true|true|false")
    /// </summary>
    public required string GameModeAccess { get; set; }

    public required bool IsFriend { get; set; }
}
