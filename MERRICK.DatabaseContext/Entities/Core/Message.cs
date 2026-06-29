namespace MERRICK.DatabaseContext.Entities.Core;

/// <summary>
///     A single message in an account's in-game message inbox.
///     Messages are either individual (sent to one account, such as a clan leadership notice) or copies of a system message (delivered to every account, such as the welcome message).
/// </summary>
public class Message
{
    /// <summary>
    ///     The default icon shown for a message in the in-game inbox.
    /// </summary>
    public const string DefaultImage = "/ui/fe2/NewUI/Res/system_message/msg_type1.png";

    [Key]
    public int ID { get; set; }

    public int AccountID { get; set; }

    /// <summary>
    ///     The owning account. Optional as a navigation so that messages can be created by <see cref="AccountID"/> alone (for example when broadcasting to every account). The underlying foreign key is always required.
    /// </summary>
    [ForeignKey(nameof(AccountID))]
    public Account? Account { get; set; }

    [MaxLength(100)]
    public required string Subject { get; set; }

    [MaxLength(80)]
    public string Subtitle { get; set; } = string.Empty;

    [MaxLength(100)]
    public string BodyTitle { get; set; } = string.Empty;

    [MaxLength(2000)]
    public required string Body { get; set; }

    [MaxLength(80)]
    public string Footer { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Image { get; set; } = DefaultImage;

    public bool Read { get; set; } = false;

    public bool Deletable { get; set; } = true;

    public DateTimeOffset TimestampSent { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     The time at which the message expires and stops being delivered to the client, or <see langword="null"/> when it never expires.
    /// </summary>
    public DateTimeOffset? TimestampExpires { get; set; } = null;
}
