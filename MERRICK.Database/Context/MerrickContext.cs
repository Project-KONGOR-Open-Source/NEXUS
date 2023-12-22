namespace MERRICK.Database.Context;

public sealed class MerrickContext : IdentityDbContext<User, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
{
    public MerrickContext(DbContextOptions options) : base(options)
    {
        Database.SetCommandTimeout(300); // 5 Minutes - Helps Prevent Migrations From Timing Out When Many Records Need To Update
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Clan> Clans => Set<Clan>();

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

        builder.Ignore(role => role.ConcurrencyStamp);

        builder.Ignore(role => role.Id);
    }

    private static void DefineRoleClaim(EntityTypeBuilder<RoleClaim> builder)
    {
        builder.Ignore(roleClaim => roleClaim.Id);

        builder.Property(roleClaim => roleClaim.RoleId).HasColumnName("RoleID");

        builder.ToTable(nameof(RoleClaims), table => table.ExcludeFromMigrations());
    }

    private static void DefineUser(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(nameof(Users));

        builder
            .Ignore(user => user.UserName)
            .Ignore(user => user.NormalizedUserName)
            .Ignore(user => user.Email)
            .Ignore(user => user.NormalizedEmail)
            .Ignore(user => user.EmailConfirmed)
            .Ignore(user => user.SecurityStamp)
            .Ignore(user => user.ConcurrencyStamp)
            .Ignore(user => user.PhoneNumber)
            .Ignore(user => user.PhoneNumberConfirmed)
            .Ignore(user => user.TwoFactorEnabled)
            .Ignore(user => user.LockoutEnd)
            .Ignore(user => user.LockoutEnabled)
            .Ignore(user => user.AccessFailedCount)
            .Ignore(user => user.PasswordHash);

        builder.Ignore(user => user.Id);
    }

    private static void DefineUserClaim(EntityTypeBuilder<UserClaim> builder)
    {
        builder.Ignore(userClaim => userClaim.Id);

        builder.Property(userClaim => userClaim.UserId).HasColumnName("UserID");

        builder.ToTable(nameof(UserClaims), table => table.ExcludeFromMigrations());
    }

    private static void DefineUserLogin(EntityTypeBuilder<UserLogin> builder)
    {
        builder.Property(userLogin => userLogin.UserId).HasColumnName("UserID");

        builder.ToTable(nameof(UserLogins), table => table.ExcludeFromMigrations());
    }

    private static void DefineUserRole(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable(nameof(UserRoles));

        builder.Property(userRole => userRole.RoleId).HasColumnName("RoleID");

        builder.Property(userRole => userRole.UserId).HasColumnName("UserID");
    }

    private static void DefineUserToken(EntityTypeBuilder<UserToken> builder)
    {
        builder.Property(userToken => userToken.UserId).HasColumnName("UserID");

        builder.ToTable(nameof(UserTokens), table => table.ExcludeFromMigrations());
    }
}
