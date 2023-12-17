namespace MERRICK.Database.Context;

public sealed class MerrickContext : IdentityDbContext<User>
{
    public MerrickContext(DbContextOptions options) : base(options)
    {
        Database.SetCommandTimeout(300); // 5 Minutes - Helps Prevent Migrations From Timing Out When Many Records Need To Update
    }

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Clan> Clans => Set<Clan>();

    public new DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        //builder.Entity<IdentityUser>().ToTable("Users");

        //builder.Entity<IdentityUser>()
        //    .Ignore(user => user.UserName)
        //    .Ignore(user => user.NormalizedUserName)
        //    .Ignore(user => user.Email)
        //    .Ignore(user => user.NormalizedEmail)
        //    .Ignore(user => user.EmailConfirmed)
        //    .Ignore(user => user.SecurityStamp)
        //    .Ignore(user => user.ConcurrencyStamp)
        //    .Ignore(user => user.PhoneNumber)
        //    .Ignore(user => user.PhoneNumberConfirmed)
        //    .Ignore(user => user.TwoFactorEnabled)
        //    .Ignore(user => user.LockoutEnd)
        //    .Ignore(user => user.LockoutEnabled)
        //    .Ignore(user => user.AccessFailedCount);

        // TODO: Create Custom Identity Tables
    }
}
