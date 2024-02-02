namespace MERRICK.Database.Context;

public sealed class MerrickContext : DbContext
{
    public MerrickContext(DbContextOptions options) : base(options)
    {
        if (Database.IsInMemory().Equals(false))
            Database.SetCommandTimeout(60); // 1 Minute - Helps Prevent Migrations From Timing Out When Many Records Need To Update
    }

    # region Core
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Clan> Clans => Set<Clan>();
    public DbSet<User> Users => Set<User>();
    # endregion

    # region Relational
    public DbSet<BannedAccount> BannedAccounts => Set<BannedAccount>();
    public DbSet<FriendAccount> FriendAccounts => Set<FriendAccount>();
    public DbSet<IgnoredAccount> IgnoredAccounts => Set<IgnoredAccount>();
    # endregion

    # region Utility
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Token> Tokens => Set<Token>();
    # endregion

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureAccounts(builder.Entity<Account>());
        ConfigureRoles(builder.Entity<Role>());
    }

    private static void ConfigureAccounts(EntityTypeBuilder<Account> builder)
    {
        builder.HasMany(account => account.FriendAccounts).WithOne(friend => friend.BelongsToAccount).OnDelete(DeleteBehavior.NoAction);
        builder.HasMany(account => account.IgnoredAccounts).WithOne(ignored => ignored.BelongsToAccount).OnDelete(DeleteBehavior.NoAction);
        builder.HasMany(account => account.BannedAccounts).WithOne(banned => banned.BelongsToAccount).OnDelete(DeleteBehavior.NoAction);
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
