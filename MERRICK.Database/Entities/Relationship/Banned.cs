namespace MERRICK.Database.Entities.Relationship;

public class Banned
{
    [Key]
    public Guid ID { get; set; }

    public required Account Account { get; set; }

    public required string Reason { get; set; }
}
