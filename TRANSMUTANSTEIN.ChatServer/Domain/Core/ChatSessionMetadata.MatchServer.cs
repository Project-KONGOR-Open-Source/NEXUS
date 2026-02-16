namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

public class MatchServerChatSessionMetadata
{
    public required int ServerID { get; set; }

    public required string SessionCookie { get; set; }

    public required int ChatProtocolVersion { get; set; }

    // Match Server Location And Identity
    public string? Location { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public int Port { get; set; }

    public int? SlaveID { get; set; }

    // Current Match State

    /// <summary>
    ///     The match ID of the currently active match on this game server.
    ///     Populated by NET_CHAT_GS_STATUS updates from the game server.
    ///     A value of -1 (the default) means no match has been announced yet.
    /// </summary>
    public int MatchID { get; set; } = -1;

    public string? MapName { get; set; }

    public string? GameName { get; set; }

    public string? GameModeName { get; set; }

    public byte TeamSize { get; set; }

    // Match Flags
    public byte Tier { get; set; }

    public bool IsOfficial { get; set; }

    public bool OfficialWithStats { get; set; }

    public bool NoLeaver { get; set; }

    public bool IsPrivate { get; set; }

    public bool AllHeroes { get; set; }

    public bool CasualMode { get; set; }

    public bool ForceRandom { get; set; }

    public bool AutoBalanced { get; set; }

    public bool AdvancedOptions { get; set; }

    public ushort MinPSR { get; set; }

    public ushort MaxPSR { get; set; }

    public bool DevHeroes { get; set; }

    public bool Hardcore { get; set; }

    public bool VerifiedOnly { get; set; }

    public bool Gated { get; set; }

    // System Metrics
    public uint ServerLoad { get; set; }

    public uint LongServerFrames { get; set; }

    public ulong FreePhysicalMemory { get; set; }

    public ulong TotalPhysicalMemory { get; set; }

    public ulong SpaceFree { get; set; }

    public ulong TotalSpace { get; set; }

    public string? Version { get; set; }

    // Server Status
    public ChatProtocol.ServerStatus Status { get; set; } = ChatProtocol.ServerStatus.SERVER_STATUS_UNKNOWN;

    // Timestamp Tracking
    public DateTimeOffset LastStatusUpdate { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? RecentlyUsedExpiration { get; set; }
}
