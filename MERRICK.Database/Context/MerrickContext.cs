namespace MERRICK.Database.Context;

public class MerrickContext : IdentityDbContext<User>
{
    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Clan> Clans => Set<Clan>();

    public new DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityUser>()
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

        builder.Entity<IdentityUser>().ToTable("Users");
    }
}
