namespace MERRICK.Database.Models.Context;

public sealed class MerrickContext : IdentityDbContext<User, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
{
    public MerrickContext(DbContextOptions options) : base(options)
    {
        Database.SetCommandTimeout(300); // 5 Minutes - Helps Prevent Migrations From Timing Out When Many Records Need To Update
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Clan> Clans => Set<Clan>();
    public DbSet<Token> Tokens => Set<Token>();

    public override DbSet<Role> Roles => Set<Role>();
    public override DbSet<RoleClaim> RoleClaims => Set<RoleClaim>();
    public override DbSet<User> Users => Set<User>();
    public override DbSet<UserClaim> UserClaims => Set<UserClaim>();
    public override DbSet<UserLogin> UserLogins => Set<UserLogin>();
    public override DbSet<UserRole> UserRoles => Set<UserRole>();
    public override DbSet<UserToken> UserTokens => Set<UserToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        DefineRole(builder.Entity<Role>());
        DefineRoleClaim(builder.Entity<RoleClaim>());
        DefineUser(builder.Entity<User>());
        DefineUserClaim(builder.Entity<UserClaim>());
        DefineUserLogin(builder.Entity<UserLogin>());
        DefineUserRole(builder.Entity<UserRole>());
        DefineUserToken(builder.Entity<UserToken>());
    }

    private static void DefineRole(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable(nameof(Roles));

        builder.HasData
        (
            new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                Name = Constants.UserRoles.Administrator,
                NormalizedName = Constants.UserRoles.Administrator.ToUpper()
            },

            new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                Name = Constants.UserRoles.User,
                NormalizedName = Constants.UserRoles.User.ToUpper()
            }
        );
    }

    private static void DefineRoleClaim(EntityTypeBuilder<RoleClaim> builder)
    {
        builder.ToTable(nameof(RoleClaims));
    }

    private static void DefineUser(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(nameof(Users));
    }

    private static void DefineUserClaim(EntityTypeBuilder<UserClaim> builder)
    {
        builder.ToTable(nameof(UserClaims));
    }

    private static void DefineUserLogin(EntityTypeBuilder<UserLogin> builder)
    {
        builder.ToTable(nameof(UserLogins));
    }

    private static void DefineUserRole(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable(nameof(UserRoles));
    }

    private static void DefineUserToken(EntityTypeBuilder<UserToken> builder)
    {
        builder.ToTable(nameof(UserTokens));
    }
}
