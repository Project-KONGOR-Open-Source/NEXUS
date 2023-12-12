namespace MERRICK.Database.Context;

public class MerrickContext : IdentityDbContext<User>
{
    public DbSet<Account> Accounts { get; set; }

    public DbSet<Clan> Clans { get; set; }

    public DbSet<User> User { get; set; }
}
