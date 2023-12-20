namespace MERRICK.Database.Entities;

public class UserLogin : IdentityUserLogin<Guid>
{
    [Key]
    public Guid ID { get; set; }
}
