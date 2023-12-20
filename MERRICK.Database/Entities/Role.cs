namespace MERRICK.Database.Entities;

public class Role : IdentityRole<Guid>
{
    [Key]
    public Guid ID { get; set; }
}
