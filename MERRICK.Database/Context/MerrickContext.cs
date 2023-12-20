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

        builder.Entity<Role>().ToTable(nameof(Roles));
        builder.Entity<User>().ToTable(nameof(Users));
        builder.Entity<UserRole>().ToTable(nameof(UserRoles));

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
            .Ignore(user => user.AccessFailedCount)
            .Ignore(user => user.PasswordHash);

        builder.Entity<Role>().Ignore(role => role.ConcurrencyStamp);

        builder.Entity<Role>().Ignore(role => role.Id);
        builder.Entity<RoleClaim>().Ignore(claim => claim.Id);
        builder.Entity<User>().Ignore(user => user.Id);
        builder.Entity<UserClaim>().Ignore(claim => claim.Id);

        builder.Entity<RoleClaim>().Property(claim => claim.RoleId).HasColumnName("RoleID");
        builder.Entity<UserClaim>().Property(claim => claim.UserId).HasColumnName("UserID");
        builder.Entity<UserLogin>().Property(login => login.UserId).HasColumnName("UserID");
        builder.Entity<UserRole>().Property(role => role.UserId).HasColumnName("UserID");
        builder.Entity<UserRole>().Property(role => role.RoleId).HasColumnName("RoleID");
        builder.Entity<UserToken>().Property(token => token.UserId).HasColumnName("UserID");

        builder.Entity<RoleClaim>().ToTable(nameof(RoleClaims), table => table.ExcludeFromMigrations());
        builder.Entity<UserClaim>().ToTable(nameof(UserClaims), table => table.ExcludeFromMigrations());
        builder.Entity<UserLogin>().ToTable(nameof(UserLogins), table => table.ExcludeFromMigrations());
        builder.Entity<UserToken>().ToTable(nameof(UserTokens), table => table.ExcludeFromMigrations());
    }
}
