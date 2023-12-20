namespace MERRICK.Database.Entities;

public class RoleClaim : IdentityRoleClaim<Guid>
{
    [Key]
    public Guid Id { get; set; }
}
