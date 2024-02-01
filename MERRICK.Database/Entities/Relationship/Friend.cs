namespace MERRICK.Database.Entities.Relationship;

public class Friend
{
    [Key]
    public Guid ID { get; set; }

    public required Account Account { get; set; }

    public required string Group { get; set; }
}
