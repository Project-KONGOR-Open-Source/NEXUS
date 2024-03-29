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

        ConfigureRoles(builder.Entity<Role>());
        ConfigureUsers(builder.Entity<User>());
        ConfigureClans(builder.Entity<Clan>());
        ConfigureAccounts(builder.Entity<Account>());
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

    private static void ConfigureUsers(EntityTypeBuilder<User> builder)
    {
        builder.HasData(new
        {
            ID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            EmailAddress = "project.kongor@proton.me",
            RoleID = Guid.Parse($"00000000-0000-0000-0000-{UserRoles.Administrator.GetDeterministicHashCode():X12}"),
            SRPPasswordSalt = "861c37ec6d049d92cc1c67d195b414f26b572a56358272af3e9c06fcd9bfa053",
            SRPPasswordHash = "fe6f16b0ecb80f6b2bc95d68420fd13afef0c895172a81819870660208ac221a",
            PBKDF2PasswordHash = "AQAAAAIAAYagAAAAEMUkpLAr01NjkKRPaXCyTa17nlOdPKJucn5QYur+wQBTDKCpgsAcREenK+pGJPBCRw==",
            GoldCoins = 5_555_555,
            SilverCoins = 555_555_555,
            PlinkoTickets = 5_555_555,
            TotalLevel = 666,
            TotalExperience = 222_11_666,
            OwnedStoreItems = new List<string> { "ai.custom_icon:1", "av.Flamboyant", "c.cat_courier", "cc.frostburnlogo", "cr.Punk Creep", "cs.frostburnlogo", "m.Super-Taunt", "sc.paragon_circle_upgrade", "t.Dumpster_Taunt", "te.Punk TP", "w.8bit_ward" },

            // TODO: Add All Upgrades (Including Missing Ones From PK Version Control); Maybe Scrape The Resources Again

            // The Following Properties Are Not Required, But Entity Framework Thinks They Are

            TimestampCreated = DateTime.UtcNow, TimestampLastActive = DateTime.UtcNow
        });
    }

    private static void ConfigureClans(EntityTypeBuilder<Clan> builder)
    {
        builder.HasData(new Clan()
        {
            ID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "KONGOR",
            Tag = "K"
        });

        builder.HasData(new Clan()
        {
            ID = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Name = "Project KONGOR",
            Tag = "PK"
        });

        builder.HasData(new Clan()
        {
            ID = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Name = "Project KONGOR Open-Source",
            Tag = "PKOS"
        });

        builder.HasData(new Clan()
        {
            ID = Guid.Parse("00000000-0000-0000-0000-000000000004"),
            Name = "Project KONGOR Developers",
            Tag = "DEV"
        });
    }

    private static void ConfigureAccounts(EntityTypeBuilder<Account> builder)
    {
        builder.OwnsMany(account => account.BannedPeers, ownedNavigationBuilder => { ownedNavigationBuilder.ToJson(); });
        builder.OwnsMany(account => account.FriendedPeers, ownedNavigationBuilder => { ownedNavigationBuilder.ToJson(); });
        builder.OwnsMany(account => account.IgnoredPeers, ownedNavigationBuilder => { ownedNavigationBuilder.ToJson(); });

        const string mainAccount = "KONGOR";

        builder.HasData(new
        {
            ID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = mainAccount,
            UserID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Type = AccountType.Staff,
            IsMain = true,
            ClanID = Guid.Parse("00000000-0000-0000-0000-000000000004"), // Project KONGOR Developers
            ClanTier = ClanTier.Leader,
            TimestampJoinedClan = DateTime.UtcNow,
            AscensionLevel = 666,
            AutoConnectChatChannels = new List<string> { "KONGOR", "TERMINAL" },
            SelectedStoreItems = new List<string> { "ai.custom_icon:1", "av.Flamboyant", "c.cat_courier", "cc.frostburnlogo", "cr.Punk Creep", "cs.frostburnlogo", "m.Super-Taunt", "sc.paragon_circle_upgrade", "t.Dumpster_Taunt", "te.Punk TP", "w.8bit_ward" },

            // The Following Properties Are Not Required, But Entity Framework Thinks They Are

            IPAddressCollection = new List<string>(), MACAddressCollection = new List<string>(), SystemInformationCollection = new List<string>(), SystemInformationHashCollection = new List<string>(),
            TimestampCreated = DateTime.UtcNow, TimestampLastActive = DateTime.UtcNow
        });

        string[] subAccounts = [ "GOPO", "Xen0byte", /* [K] */ "ONGOR" ];

        builder.HasData(subAccounts.Select(subAccount => new
        {
            ID = Guid.Parse($"00000000-0000-0000-0000-{(Array.IndexOf(subAccounts, subAccount) + 1 /* Main Account */ + 1 /* 1-Based Indexing */ ):D12}"),
            Name = subAccount,
            UserID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Type = AccountType.Staff,
            IsMain = false,
            ClanID = Guid.Parse("00000000-0000-0000-0000-000000000001"), // KONGOR
            ClanTier = subAccount is "ONGOR" ? ClanTier.Leader : ClanTier.Officer,
            TimestampJoinedClan = DateTime.UtcNow,
            AscensionLevel = 666,
            AutoConnectChatChannels = new List<string> { "KONGOR", "TERMINAL" },
            SelectedStoreItems = new List<string> { "ai.custom_icon:1", "av.Flamboyant", "c.cat_courier", "cc.frostburnlogo", "cr.Punk Creep", "cs.frostburnlogo", "m.Super-Taunt", "sc.paragon_circle_upgrade", "t.Dumpster_Taunt", "te.Punk TP", "w.8bit_ward" },

            // The Following Properties Are Not Required, But Entity Framework Thinks They Are

            IPAddressCollection = new List<string>(), MACAddressCollection = new List<string>(), SystemInformationCollection = new List<string>(), SystemInformationHashCollection = new List<string>(),
            TimestampCreated = DateTime.UtcNow, TimestampLastActive = DateTime.UtcNow
        }));
    }
}
