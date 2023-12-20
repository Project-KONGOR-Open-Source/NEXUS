namespace MERRICK.Database.Entities;

public class UserClaim : IdentityUserClaim<Guid>
{
    [Key]
    public Guid ID { get; set; }
}
