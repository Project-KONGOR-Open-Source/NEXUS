namespace TRANSMUTANSTEIN.ChatServer.Domain.Social;

/// <summary>
///     Tracks pending clan invitations in memory.
///     Mirrors the C++ <c>CClientManager::m_pClanInvites</c> map.
/// </summary>
public static class PendingClanInvites
{
    /// <summary>
    ///     Maps target account ID to pending invite details.
    ///     A player can only have one pending clan invite at a time.
    /// </summary>
    public static ConcurrentDictionary<int, PendingClanInvite> Invites { get; set; } = [];
}

public class PendingClanInvite
{
    /// <summary>
    ///     Account ID of the player being invited.
    /// </summary>
    public required int TargetAccountID { get; init; }

    /// <summary>
    ///     Account ID of the player who initiated the invite.
    /// </summary>
    public required int OriginAccountID { get; init; }

    /// <summary>
    ///     The clan ID the target is being invited to.
    /// </summary>
    public required int ClanID { get; init; }

    /// <summary>
    ///     The clan name.
    /// </summary>
    public required string ClanName { get; init; }

    /// <summary>
    ///     The clan tag.
    /// </summary>
    public required string ClanTag { get; init; }

    /// <summary>
    ///     Timestamp of when the invite was created, for expiry checking.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
