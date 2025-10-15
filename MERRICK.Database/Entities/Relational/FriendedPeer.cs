namespace MERRICK.Database.Entities.Relational;

[Index(nameof(Name), IsUnique = true)]
public class FriendedPeer
{
    public required int ID { get; set; }

    [MaxLength(15)]
    public required string Name { get; set; }

    [StringLength(4)]
    public required string? ClanTag { get; set; }

    [MaxLength(15)]
    public required string FriendGroup { get; set; }
}
