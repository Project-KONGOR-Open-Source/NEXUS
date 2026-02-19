namespace TRANSMUTANSTEIN.ChatServer.Domain.Social;

/// <summary>
///     Tracks pending clan operations (invitations and creations) in memory.
/// </summary>
public static class PendingClan
{
    /// <summary>
    ///     Maps target account ID to pending invite details.
    ///     A player can only have one pending clan invite at a time.
    /// </summary>
    public static ConcurrentDictionary<int, PendingClanInvite> Invites { get; set; } = [];

    /// <summary>
    ///     Maps founder account ID to the pending clan creation details.
    /// </summary>
    public static ConcurrentDictionary<int, PendingClanCreation> Creations { get; set; } = [];
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

public class PendingClanCreation
{
    /// <summary>
    ///     Account ID of the clan founder (leader).
    /// </summary>
    public required int FounderAccountID { get; init; }

    /// <summary>
    ///     The clan name.
    /// </summary>
    public required string ClanName { get; init; }

    /// <summary>
    ///     The clan tag (max 4 characters).
    /// </summary>
    public required string ClanTag { get; init; }

    /// <summary>
    ///     Account IDs of the 4 required founding members (excluding the founder).
    /// </summary>
    public required int[] TargetAccountIDs { get; init; }

    /// <summary>
    ///     Acceptance status for each of the 4 founding members, indexed to match <see cref="TargetAccountIDs"/>.
    /// </summary>
    public bool[] Accepted { get; init; } = [false, false, false, false];

    /// <summary>
    ///     Timestamp of when the creation request was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Checks whether all 4 additional founding members have accepted.
    /// </summary>
    public bool AllAccepted => Accepted.All(accepted => accepted);

    /// <summary>
    ///     Checks whether the specified account ID is one of the founding members.
    /// </summary>
    public bool IsFoundingMember(int accountID)
        => TargetAccountIDs.Contains(accountID);

    /// <summary>
    ///     Marks a founding member as having accepted, and returns whether all have now accepted.
    /// </summary>
    public bool AcceptMember(int accountID)
    {
        for (int index = 0; index < TargetAccountIDs.Length; index++)
        {
            if (TargetAccountIDs[index] == accountID)
            {
                Accepted[index] = true;

                break;
            }
        }

        return AllAccepted;
    }
}
