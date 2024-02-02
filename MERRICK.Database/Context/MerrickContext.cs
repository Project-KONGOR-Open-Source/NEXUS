namespace MERRICK.Database.Context;

public sealed class MerrickContext : DbContext
{
    public MerrickContext(DbContextOptions options) : base(options)
    {
        if (Database.IsInMemory().Equals(false))
            Database.SetCommandTimeout(60); // 1 Minute - Helps Prevent Migrations From Timing Out When Many Records Need To Update
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Clan> Clans => Set<Clan>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Token> Tokens => Set<Token>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureAccounts(builder.Entity<Account>());
        ConfigureRoles(builder.Entity<Role>());
    }

    private static void ConfigureAccounts(EntityTypeBuilder<Account> builder)
    {
        builder.OwnsMany(account => account.BannedAccounts,  ownedNavigationBuilder => { ownedNavigationBuilder.ToJson(); });
        builder.OwnsMany(account => account.FriendAccounts,  ownedNavigationBuilder => { ownedNavigationBuilder.ToJson(); });
        builder.OwnsMany(account => account.IgnoredAccounts, ownedNavigationBuilder => { ownedNavigationBuilder.ToJson(); });
    }

    private static void ConfigureRoles(EntityTypeBuilder<Role> builder)
    {
        builder.HasData
        (
            new Role
            {
                ID = Guid.Parse($"00000000-0000-0000-0000-{UserRoles.Administrator.GetDeterministicHashCode():X12}"),
                Name = UserRoles.Administrator
            },

            new Role
            {
                ID = Guid.Parse($"00000000-0000-0000-0000-{UserRoles.User.GetDeterministicHashCode():X12}"),
                Name = UserRoles.User
            }
        );
    }
}
