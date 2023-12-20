namespace MERRICK.Database.Entities;

public class UserToken : IdentityUserToken<Guid>
{
    [Key]
    public Guid Id { get; set; }
}
