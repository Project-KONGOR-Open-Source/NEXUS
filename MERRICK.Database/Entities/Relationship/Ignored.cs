namespace MERRICK.Database.Entities.Relationship;

public class Ignored
{
    [Key]
    public Guid ID { get; set; }

    public required Account Account { get; set; }
}
