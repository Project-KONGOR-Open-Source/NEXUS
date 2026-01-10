namespace MERRICK.DatabaseContext.Entities.Core;

[Index(nameof(Name), nameof(Tag), IsUnique = true)]
public class Clan
{
    [Key] public int ID { get; set; }

    [MaxLength(30)] public required string Name { get; set; }

    [MaxLength(4)] public required string Tag { get; set; }

    public List<Account> Members { get; set; } = [];

    public DateTimeOffset TimestampCreated { get; set; } = DateTimeOffset.UtcNow;

    public string GetChatChannelName()
    {
        return $"Clan {Name}";
    }
}