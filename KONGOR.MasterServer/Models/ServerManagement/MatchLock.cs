namespace KONGOR.MasterServer.Models.ServerManagement;

/// <summary>
///     Binds a user to the competitive matchmade match which they are playing, so that they cannot play another game until it ends.
/// </summary>
public class MatchLock
{
    /// <summary>
    ///     The safety limit after which a match lock lifts by itself, whether or not the match is known to have ended.
    /// </summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromHours(1);

    /// <summary>
    ///     Decides whether accounts of a type are exempt from the match lock, so that they are never locked, never refused, and never sent back.
    ///     Every guest account shares one user, as does every staff account, so locking one of them would lock all of them at once.
    /// </summary>
    public static bool IsExempt(AccountType accountType) => accountType is AccountType.Guest or AccountType.Staff;

    public required int UserID { get; init; }

    public required int AccountID { get; init; }

    public required string AccountName { get; init; }

    /// <summary>
    ///     The name of the account with its clan tag, as it was when the match was announced, which is what notices and logs name the account by.
    ///     It is optional, so that a lock stored without it still reads.
    /// </summary>
    public string? AccountNameWithClanTag { get; init; }

    public required int MatchID { get; init; }

    public required int CorrelationID { get; init; }

    public required MatchType ArrangedMatchType { get; init; }

    public required int ServerID { get; init; }

    public required string ServerAddress { get; init; }

    public required ushort ServerPort { get; init; }

    public required DateTimeOffset TimestampStarted { get; init; }

    /// <summary>
    ///     The name which notices and logs call the account by, which is its bare name for a lock stored without a clan-tagged name.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string TaggedAccountName => AccountNameWithClanTag ?? AccountName;

    /// <summary>
    ///     Computed from the start of the match, so that it is never stored.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public DateTimeOffset TimestampExpires => TimestampStarted + Lifetime;
}
