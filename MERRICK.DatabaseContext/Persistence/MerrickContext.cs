namespace MERRICK.DatabaseContext.Persistence;

public sealed class MerrickContext : DbContext
{
    public MerrickContext(DbContextOptions options) : base(options)
    {
        if (Database.IsInMemory().Equals(false))
            Database.SetCommandTimeout(60); // 1 Minute - Helps Prevent Migrations From Timing Out When Many Records Need To Update
    }

    public const string MetadataSchema = "meta";
    public const string DefaultSchema = "data";
    public const string CoreSchema = "core";
    public const string AuthenticationSchema = "auth";
    public const string StatisticsSchema = "stat";
    public const string MiscellaneousSchema = "misc";

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Clan> Clans => Set<Clan>();
    public DbSet<HeroGuide> HeroGuides => Set<HeroGuide>();
    public DbSet<MatchStatistics> MatchStatistics => Set<MatchStatistics>();
    public DbSet<PlayerStatistics> PlayerStatistics => Set<PlayerStatistics>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Token> Tokens => Set<Token>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureSchemas(builder);

        ConfigureRoles(builder.Entity<Role>());
        ConfigureAccounts(builder.Entity<Account>());
        ConfigurePlayerStatistics(builder.Entity<PlayerStatistics>());
    }

    private static void ConfigureSchemas(ModelBuilder builder)
    {
        builder.HasDefaultSchema(DefaultSchema);

        builder.Entity<Account>().ToTable("Accounts", CoreSchema);
        builder.Entity<Clan>().ToTable("Clans", CoreSchema);
        builder.Entity<HeroGuide>().ToTable("HeroGuides", MiscellaneousSchema);
        builder.Entity<MatchStatistics>().ToTable("MatchStatistics", StatisticsSchema);
        builder.Entity<PlayerStatistics>().ToTable("PlayerStatistics", StatisticsSchema);
        builder.Entity<Role>().ToTable("Roles", AuthenticationSchema);
        builder.Entity<Token>().ToTable("Tokens", AuthenticationSchema);
        builder.Entity<User>().ToTable("Users", CoreSchema);
    }

    private static void ConfigureRoles(EntityTypeBuilder<Role> builder)
    {
        builder.HasData
        (
            new Role
            {
                ID = 1,
                Name = UserRoles.Administrator
            },

            new Role
            {
                ID = 2,
                Name = UserRoles.User
            }
        );
    }

    private static void ConfigureAccounts(EntityTypeBuilder<Account> builder)
    {
        builder.OwnsMany(account => account.BannedPeers, ownedNavigationBuilder => { ownedNavigationBuilder.ToJson(); });
        builder.OwnsMany(account => account.FriendedPeers, ownedNavigationBuilder => { ownedNavigationBuilder.ToJson(); });
        builder.OwnsMany(account => account.IgnoredPeers, ownedNavigationBuilder => { ownedNavigationBuilder.ToJson(); });
    }

    private static void ConfigurePlayerStatistics(EntityTypeBuilder<PlayerStatistics> builder)
    {
        builder.Property(statistics => statistics.Inventory).HasConversion
        (
            value => JsonSerializer.Serialize(value, new JsonSerializerOptions()),
            value => JsonSerializer.Deserialize<List<string>>(value, new JsonSerializerOptions()) ?? new List<string>(),
            new ValueComparer<List<string>>((first, second) => (first ?? new List<string>()).SequenceEqual(second ?? new List<string>()),
                collection => collection.Aggregate(0, (accumulatedHashCode, value) => HashCode.Combine(accumulatedHashCode, value.GetHashCode())), collection => collection.ToList())
        );
    }
}
