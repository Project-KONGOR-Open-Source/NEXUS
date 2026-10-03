namespace MERRICK.DatabaseContext.Entities.Core;

[Index(nameof(Name), nameof(Tag), IsUnique = true)]
public class Clan
{
    [Key]
    public int ID { get; set; }

    [MaxLength(30)]
    public required string Name { get; set; }

    [MaxLength(4)]
    public required string Tag { get; set; }

    /// <summary>
    ///     The clan's title (motto), surfaced as the "title" field of the clan member data point in the authentication response.
    /// </summary>
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     The identifier of the clan's logo, surfaced as the "logo" field of the clan member data point in the authentication response.
    /// </summary>
    [MaxLength(50)]
    public string Logo { get; set; } = string.Empty;

    public List<Account> Members { get; set; } = [];

    public DateTimeOffset TimestampCreated { get; set; } = DateTimeOffset.UtcNow;

    public string GetChatChannelName() => $"Clan {Name}";

    /// <summary>
    ///     The clan's default welcome message, used both as the clan chat channel's topic and as the "message" field of the clan member data point in the authentication response.
    /// </summary>
    public string GetDefaultWelcomeMessage() => $"Welcome To The {GetChatChannelName()} Channel !";
}
