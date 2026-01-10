namespace MERRICK.DatabaseContext.Entities.Relational;

[Index(nameof(Name), IsUnique = true)]
public class BannedPeer
{
    public required int ID { get; set; }

    [MaxLength(15)] public required string Name { get; set; }

    [MaxLength(30)] public required string BanReason { get; set; }
}