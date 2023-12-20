namespace MERRICK.Database.Entities;

public class Role : IdentityRole<Guid>
{
    [Key]
    public Guid Id { get; set; }
}
