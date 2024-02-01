namespace MERRICK.Database.Entities.Utility;

[Index(nameof(Name), IsUnique = true)]
public class Role
{
    [Key]
    public Guid ID { get; set; }

    [StringLength(20)]
    public required string Name { get; set; }
}
