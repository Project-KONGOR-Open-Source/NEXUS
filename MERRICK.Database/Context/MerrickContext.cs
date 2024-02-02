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
        builder.HasMany(account => account.FriendAccounts).WithMany().UsingEntity <Dictionary<string, object>>
        (
            joinEntityName: "AccountFriendAccounts",
            left  => left.HasOne<Account>().WithMany().HasForeignKey("FriendAccountID"),
            right => right.HasOne<Account>().WithMany().HasForeignKey("AccountID"),
            join  => { join.HasKey("AccountID"); join.Property<Guid>("AccountID"); join.Property<Guid>("FriendAccountID"); join.HasIndex("AccountID", "FriendAccountID").IsUnique(); }
        );

        builder.HasMany(account => account.IgnoredAccounts).WithMany().UsingEntity <Dictionary<string, object>>
        (
            joinEntityName: "AccountIgnoredAccounts",
            left  => left.HasOne<Account>().WithMany().HasForeignKey("IgnoredAccountID"),
            right => right.HasOne<Account>().WithMany().HasForeignKey("AccountID"),
            join  => { join.HasKey("AccountID"); join.Property<Guid>("AccountID"); join.Property<Guid>("IgnoredAccountID"); join.HasIndex("AccountID", "IgnoredAccountID").IsUnique(); }
        );

        builder.HasMany(account => account.BannedAccounts).WithMany().UsingEntity<Dictionary<string, object>>
        (
            joinEntityName: "AccountBannedAccounts",
            left  => left.HasOne<Account>().WithMany().HasForeignKey("BannedAccountID"),
            right => right.HasOne<Account>().WithMany().HasForeignKey("AccountID"),
            join  => { join.HasKey("AccountID"); join.Property<Guid>("AccountID"); join.Property<Guid>("BannedAccountID"); join.HasIndex("AccountID", "BannedAccountID").IsUnique(); }
        );
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
