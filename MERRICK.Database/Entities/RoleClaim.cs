namespace MERRICK.Database.Entities;

public class RoleClaim : IdentityRoleClaim<Guid>
{
    [Key]
    public Guid ID { get; set; }
}
