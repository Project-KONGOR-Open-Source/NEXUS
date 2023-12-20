namespace MERRICK.Database.Entities;

[Index(nameof(Name), IsUnique = true)]
public class Role : IdentityRole<Guid>
{
    [Key]
    public Guid ID { get; set; }
}
