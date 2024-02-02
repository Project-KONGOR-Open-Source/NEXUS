namespace MERRICK.Database.Entities.Relational;

[Index(nameof(SelfAccount) + nameof(SelfAccount.ID), nameof(BelongsToAccount) + nameof(BelongsToAccount.ID), IsUnique = true)]
public class FriendAccount
{
    [Key]
    public Guid ID { get; set; }

    public required Account SelfAccount { get; set; }

    public required Account BelongsToAccount { get; set; }

    public required string Group { get; set; }
}
