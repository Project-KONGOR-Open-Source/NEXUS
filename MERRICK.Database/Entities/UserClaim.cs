namespace MERRICK.Database.Entities;

public class UserClaim : IdentityUserClaim<Guid>
{
    [Key]
    public Guid Id { get; set; }
}
