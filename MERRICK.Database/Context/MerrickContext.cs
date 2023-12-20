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

        builder.Entity<Account>().ToTable("Accounts");
        builder.Entity<Role>().ToTable("Roles");
        builder.Entity<RoleClaim>().ToTable("RoleClaims");
        builder.Entity<User>().ToTable("Users");
        builder.Entity<UserClaim>().ToTable("UserClaims");
        builder.Entity<UserLogin>().ToTable("UserLogins");
        builder.Entity<UserRole>().ToTable("UserRoles");
        builder.Entity<UserToken>().ToTable("UserTokens");

        builder.Entity<User>()
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
            .Ignore(user => user.AccessFailedCount);

        builder.Entity<Role>().Property(role => role.Id).HasColumnName("ID");
        builder.Entity<RoleClaim>().Property(claim => claim.Id).HasColumnName("ID");
        builder.Entity<User>().Property(user => user.Id).HasColumnName("ID");
        builder.Entity<UserClaim>().Property(claim => claim.Id).HasColumnName("ID");
    }
}
