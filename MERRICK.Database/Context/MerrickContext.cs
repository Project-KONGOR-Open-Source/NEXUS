namespace MERRICK.Database.Context;

public sealed class MerrickContext : DbContext
{
    public MerrickContext(DbContextOptions options) : base(options)
    {
        if (Database.IsInMemory().Equals(false))
            Database.SetCommandTimeout(60); // 1 Minute - Helps Prevent Migrations From Timing Out When Many Records Need To Update
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Banned> BannedAccounts => Set<Banned>();
    public DbSet<Clan> Clans => Set<Clan>();
    public DbSet<Friend> FriendAccounts => Set<Friend>();
    public DbSet<Ignored> IgnoredAccounts => Set<Ignored>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Token> Tokens => Set<Token>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureRoles(builder.Entity<Role>());
    }

    private static void ConfigureRoles(EntityTypeBuilder<Role> builder)
    {
        builder.HasData
        (
            new Role
            {
                ID = Guid.Parse($"00000000-0000-0000-0000-{Constants.UserRoles.Administrator.GetDeterministicHashCode():X12}"),
                Name = Constants.UserRoles.Administrator
            },

            new Role
            {
                ID = Guid.Parse($"00000000-0000-0000-0000-{Constants.UserRoles.User.GetDeterministicHashCode():X12}"),
                Name = Constants.UserRoles.User
            }
        );
    }
}
