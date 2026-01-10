namespace MERRICK.DatabaseContext.Entities.Utility;

[Index(nameof(Name), IsUnique = true)]
public class Role
{
    [Key] public int ID { get; set; }

    [StringLength(20)] public required string Name { get; set; }
}