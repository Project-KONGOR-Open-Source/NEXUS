namespace MERRICK.Database.Entities;

[Index(nameof(Name), IsUnique = true)]
public class UserToken : IdentityUserToken<Guid>
{
    [Key]
    public Guid ID { get; set; }
}
