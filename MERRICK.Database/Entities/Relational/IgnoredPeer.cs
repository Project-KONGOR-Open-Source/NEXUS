namespace MERRICK.Database.Entities.Relational;

[Index(nameof(Name), IsUnique = true)]
public class IgnoredPeer
{
    public required int ID { get; set; }

    [MaxLength(15)]
    public required string Name { get; set; }
}
