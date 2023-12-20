namespace MERRICK.Database.Entities;

public class UserRole : IdentityUserRole<Guid>
{
    [Key]
    public Guid Id { get; set; }
}
