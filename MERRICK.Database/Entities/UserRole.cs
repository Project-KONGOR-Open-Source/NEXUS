namespace MERRICK.Database.Entities;

public class UserRole : IdentityUserRole<Guid>
{
    [Key]
    public Guid ID { get; set; }
}
